using System.Threading;
using System.Threading.Tasks;

namespace AspireApp.BedRock.SonetOps.Services
{
    public interface ICheckoutService
    {
        /// <summary>
        /// Initializes a checkout session with the given details.
        /// </summary>
        /// <param name="request">The checkout request containing order details</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The checkout session ID</returns>
        Task<string> InitializeCheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the payment for an existing checkout session.
        /// </summary>
        /// <param name="sessionId">The checkout session ID</param>
        /// <param name="paymentData">The payment details</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if payment was successful, false otherwise</returns>
        Task<bool> ProcessPaymentAsync(string sessionId, PaymentData paymentData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current state of a checkout session.
        /// </summary>
        /// <param name="sessionId">The checkout session ID</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The current checkout session state</returns>
        Task<CheckoutSessionState> GetSessionStateAsync(string sessionId, CancellationToken cancellationToken = default);
    }

    public class CheckoutRequest
    {
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }
        public decimal Total => SubTotal + Tax + Shipping - Discount;
        public string Currency { get; set; } = "INR";
        public string OrderId { get; set; }
        public CustomerInfo Customer { get; set; }
    }

    public class CustomerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class PaymentData
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public object PaymentMethodData { get; set; }
        public bool SavePaymentMethod { get; set; }
    }

    public class CheckoutSessionState
    {
        public string SessionId { get; set; }
        public string OrderId { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentId { get; set; }
        public string Error { get; set; }
    }
}