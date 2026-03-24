using System.Threading;
using System.Threading.Tasks;
using AspireApp.BedRock.PaymentGateway.Models;

namespace AspireApp.BedRock.PaymentGateway.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<string> CreateOrderAsync(PaymentOrderRequest request, CancellationToken cancellationToken = default);
        Task<bool> VerifyPaymentAsync(string orderId, string paymentId, string signature, CancellationToken cancellationToken = default);
    }
}