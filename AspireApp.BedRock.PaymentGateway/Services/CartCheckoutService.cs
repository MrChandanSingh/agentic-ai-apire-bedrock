using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.PaymentGateway.Interfaces;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.PaymentGateway.Services;

public interface ICartCheckoutService
{
    Task<string> InitiateCheckoutAsync(CancellationToken cancellationToken = default);
    Task<bool> VerifyPaymentStatusAsync(string orderId, string paymentId, string signature, CancellationToken cancellationToken = default);
    Task<(decimal subtotal, decimal tax, decimal total)> CalculateOrderTotalAsync();
    Task<bool> ValidateCartForCheckoutAsync();
}

public class CartCheckoutService : ICartCheckoutService
{
    private readonly ICartService _cartService;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly CartState _cartState;
    private readonly ILogger<CartCheckoutService> _logger;

    public CartCheckoutService(
        ICartService cartService,
        IPaymentGatewayService paymentGateway,
        CartState cartState,
        ILogger<CartCheckoutService> logger)
    {
        _cartService = cartService;
        _paymentGateway = paymentGateway;
        _cartState = cartState;
        _logger = logger;
    }

    public async Task<string> InitiateCheckoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ValidateCartForCheckoutAsync())
            {
                throw new InvalidOperationException("Cart validation failed");
            }

            var (subtotal, tax, total) = await CalculateOrderTotalAsync();
            
            var receiptId = GenerateReceiptId();

            var orderRequest = new PaymentOrderRequest
            {
                Amount = total,
                Currency = "USD", // TODO: Make configurable
                ReceiptId = receiptId
            };

            var orderId = await _paymentGateway.CreateOrderAsync(orderRequest, cancellationToken);
            
            _logger.LogInformation("Initiated checkout with order ID: {OrderId}", orderId);
            
            return orderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate checkout");
            throw;
        }
    }

    public async Task<bool> VerifyPaymentStatusAsync(string orderId, string paymentId, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var isValid = await _paymentGateway.VerifyPaymentAsync(orderId, paymentId, signature, cancellationToken);
            
            if (isValid)
            {
                _logger.LogInformation("Payment verified successfully for order ID: {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("Payment verification failed for order ID: {OrderId}", orderId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment status for order ID: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<(decimal subtotal, decimal tax, decimal total)> CalculateOrderTotalAsync()
    {
        var items = await _cartService.GetCartItemsAsync();
        
        var subtotal = items.Sum(item => item.Product.Price * item.Quantity);
        var tax = subtotal * 0.1m; // TODO: Make tax rate configurable
        var total = subtotal + tax;

        return (subtotal, tax, total);
    }

    public async Task<bool> ValidateCartForCheckoutAsync()
    {
        // Validate cart is not empty
        var items = await _cartService.GetCartItemsAsync();
        if (!items.Any())
        {
            _logger.LogWarning("Cannot checkout with empty cart");
            return false;
        }

        // Validate delivery address exists
        if (!_cartState.HasDeliveryAddress)
        {
            _logger.LogWarning("Cannot checkout without delivery address");
            return false;
        }

        // Validate all products are still available and quantities are valid
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity for product ID: {ProductId}", item.Product.Id);
                return false;
            }

            // TODO: Add stock availability check
        }

        return true;
    }

    private string GenerateReceiptId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        return $"RCPT-{timestamp}-{random}";
    }
}