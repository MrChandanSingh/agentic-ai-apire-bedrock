using System.Threading.Tasks;
using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.PaymentGateway.Interfaces;
using AspireApp.BedRock.SonetOps.Web.Models;
using System.Linq;

namespace AspireApp.BedRock.SonetOps.Web.Services
{
    public class CheckoutService
    {
        private readonly IPaymentGatewayService _paymentGateway;

        public CheckoutService(IPaymentGatewayService paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        public async Task<string> InitiateCheckout(CartState cart)
        {
            if (!cart.Items.Any())
            {
                throw new InvalidOperationException("Cannot checkout with empty cart");
            }

            var orderRequest = new PaymentOrderRequest
            {
                Amount = cart.Total,
                Currency = "USD",
                Notes = new Dictionary<string, string>
                {
                    { "items_count", cart.Items.Count.ToString() },
                    { "items", string.Join(", ", cart.Items.Select(i => i.Name)) }
                }
            };

            var order = await _paymentGateway.CreateOrderAsync(orderRequest);
            return order.OrderId;
        }
    }
}