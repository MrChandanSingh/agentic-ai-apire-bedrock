using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AspireApp.BedRock.PaymentGateway.Models;
using AspireApp.BedRock.PaymentGateway.Security;
using AspireApp.BedRock.PaymentGateway.Logging;

namespace AspireApp.BedRock.PaymentGateway
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRazorpayServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add security services
            services.AddPaymentSecurity();
            
            // Add logging services
            services.AddSingleton<PaymentAuditLogger>();
            services.AddOptions<PaymentSettings>()
                .Bind(configuration.GetSection("Payment"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            services.AddScoped<IPaymentGatewayService, RazorpayService>();
            
            return services;
        }
    }
}