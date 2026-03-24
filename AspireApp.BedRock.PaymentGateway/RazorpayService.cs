using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.PaymentGateway.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using AspireApp.BedRock.PaymentGateway.Utilities;

namespace AspireApp.BedRock.PaymentGateway
{
    public class RazorpayService : IPaymentGatewayService
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly PaymentSettings _settings;
        private readonly ILogger<RazorpayService> _logger;
        private readonly RazorpayClient _razorpayClient;

        public RazorpayService(IOptions<PaymentSettings> settings, ILogger<RazorpayService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (string.IsNullOrEmpty(_settings.RazorpayKey))
                throw new ArgumentException("Razorpay key is required", nameof(settings));
            if (string.IsNullOrEmpty(_settings.RazorpaySecret))
                throw new ArgumentException("Razorpay secret is required", nameof(settings));
                
            _razorpayClient = new RazorpayClient(_settings.RazorpayKey, _settings.RazorpaySecret);
        }

        public async Task<string> CreateOrderAsync(PaymentOrderRequest request, CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(30), linkedCts.Token))
                {
                    throw new PaymentGatewayException("Payment service is busy. Please try again.");
                }

                try
                {
                    await Task.Run(() =>
                    {
                        var validationResults = new List<ValidationResult>();
                        if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
                        {
                            throw new ValidationException(string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
                        }
                    }, linkedCts.Token);

                    Dictionary<string, object> orderOptions = new()
                    {
                        { "amount", request.Amount * 100 },
                        { "currency", request.Currency ?? "INR" },
                        { "receipt", request.ReceiptId },
                        { "payment_capture", 1 }
                    };

                    var order = await Task.Run(() => _razorpayClient.Order.Create(orderOptions), linkedCts.Token);
                    return order.Id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Razorpay order. Amount: {Amount}, Currency: {Currency}", 
                        request.Amount, request.Currency);
                    _logger.LogDebug("Order creation failed. Details: {Details}", 
                        LogMasker.MaskSensitiveData(ex.ToString()));
                    throw new PaymentGatewayException("Failed to create Razorpay order", ex);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                throw new PaymentGatewayException("Payment request timed out");
            }
        }

        public async Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature, CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                await Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(orderId))
                        throw new ArgumentException("Order ID is required", nameof(orderId));
                    if (string.IsNullOrEmpty(paymentId))
                        throw new ArgumentException("Payment ID is required", nameof(paymentId));
                    if (string.IsNullOrEmpty(signature))
                        throw new ArgumentException("Signature is required", nameof(signature));
                }, linkedCts.Token);

                string payload = $"{orderId}|{paymentId}";
                string generatedSignature = await Task.Run(() => 
                    Utils.ComputeSignature(payload, _settings.RazorpaySecret), 
                    linkedCts.Token
                );
                
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(generatedSignature),
                    Convert.FromBase64String(signature)
                );
            }
            catch (Exception ex)
            {
                var maskedOrderId = LogMasker.MaskSensitiveData(orderId);
                var maskedPaymentId = LogMasker.MaskSensitiveData(paymentId);
                _logger.LogError(ex, "Payment verification failed. OrderId: {OrderId}, PaymentId: {PaymentId}", 
                    maskedOrderId, maskedPaymentId);
                _logger.LogDebug("Verification failed. Details: {Details}", 
                    LogMasker.MaskSensitiveData(ex.ToString()));
                throw new PaymentGatewayException("Payment verification failed", ex);
            }
            catch (OperationCanceledException)
            {
                throw new PaymentGatewayException("Payment verification timed out");
            }
        }
    }
}