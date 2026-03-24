using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.PaymentGateway.Interfaces;

namespace AspireApp.BedRock.PaymentGateway.Components
{
    public class PaymentProcessor
    {
        private readonly IPaymentGatewayService _paymentService;
        private readonly ILogger<PaymentProcessor> _logger;

        public PaymentProcessor(IPaymentGatewayService paymentService, ILogger<PaymentProcessor> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<string> ProcessPaymentAsync(decimal amount, string currency = "INR")
        {
            try
            {
                var request = new PaymentOrderRequest
                {
                    Amount = amount,
                    Currency = currency,
                    ReceiptId = Guid.NewGuid().ToString("N")
                };

                var orderId = await _paymentService.CreateOrderAsync(request);
                _logger.LogInformation("Payment order created successfully. Amount: {Amount} {Currency}", 
                    amount, currency);

                return orderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing failed. Amount: {Amount} {Currency}", 
                    amount, currency);
                throw;
            }
        }

        public async Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature)
        {
            try
            {
                var isValid = await _paymentService.VerifyPaymentAsync(orderId, paymentId, signature);
                _logger.LogInformation("Payment verification {Result}. OrderId: {OrderId}", 
                    isValid ? "succeeded" : "failed",
                    orderId);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment verification failed");
                throw;
            }
        }
    }
}