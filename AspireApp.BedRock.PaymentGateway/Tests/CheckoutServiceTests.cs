using System.Threading.Tasks;
using AspireApp.BedRock.PaymentGateway.Components;
using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.SonetOps.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AspireApp.BedRock.PaymentGateway.Tests
{
    public class CheckoutServiceTests
    {
        private readonly Mock<PaymentProcessor> _mockPaymentProcessor;
        private readonly Mock<ILogger<CheckoutService>> _mockLogger;
        private readonly CheckoutService _checkoutService;

        public CheckoutServiceTests()
        {
            _mockPaymentProcessor = new Mock<PaymentProcessor>(
                Mock.Of<IPaymentGatewayService>(),
                Mock.Of<ILogger<PaymentProcessor>>());
            _mockLogger = new Mock<ILogger<CheckoutService>>();
            _checkoutService = new CheckoutService(_mockPaymentProcessor.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InitializeCheckout_ValidRequest_ReturnsSessionId()
        {
            // Arrange
            var request = new CheckoutRequest
            {
                SubTotal = 100.00m,
                Tax = 10.00m,
                Shipping = 5.00m,
                OrderId = "test-order-123",
                Currency = "INR",
                Customer = new CustomerInfo
                {
                    Id = "cust-123",
                    Name = "Test Customer",
                    Email = "test@example.com"
                }
            };

            _mockPaymentProcessor
                .Setup(x => x.ProcessPaymentAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<string>()))
                .ReturnsAsync("test-payment-order-123");

            // Act
            var sessionId = await _checkoutService.InitializeCheckoutAsync(request);

            // Assert
            Assert.NotNull(sessionId);
            Assert.NotEmpty(sessionId);
        }

        [Fact]
        public async Task ProcessPayment_ValidPayment_ReturnsTrue()
        {
            // Arrange
            var sessionId = "test-session-123";
            var paymentData = new PaymentData
            {
                PaymentMethod = "card",
                Amount = 115.00m,
                Currency = "INR",
                PaymentMethodData = new
                {
                    CardNumber = "4111111111111111",
                    ExpiryDate = "12/25",
                    Cvv = "123"
                }
            };

            _mockPaymentProcessor
                .Setup(x => x.ProcessPaymentAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<string>()))
                .ReturnsAsync("test-payment-123");

            // Act
            var result = await _checkoutService.ProcessPaymentAsync(sessionId, paymentData);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ProcessPayment_InvalidPayment_ReturnsFalse()
        {
            // Arrange
            var sessionId = "test-session-123";
            var paymentData = new PaymentData
            {
                PaymentMethod = "card",
                Amount = 115.00m,
                Currency = "INR",
                PaymentMethodData = new
                {
                    CardNumber = "4111111111111111",
                    ExpiryDate = "12/25",
                    Cvv = "123"
                }
            };

            _mockPaymentProcessor
                .Setup(x => x.ProcessPaymentAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Act
            var result = await _checkoutService.ProcessPaymentAsync(sessionId, paymentData);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetSessionState_UnknownSession_ThrowsNotImplementedException()
        {
            // Arrange
            var sessionId = "unknown-session";

            // Act & Assert
            await Assert.ThrowsAsync<System.NotImplementedException>(
                () => _checkoutService.GetSessionStateAsync(sessionId));
        }
    }
}