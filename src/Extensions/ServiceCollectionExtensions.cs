using Microsoft.Extensions.DependencyInjection;
using AspireApp.BedRock.SonetOps.Services;
using AspireApp.BedRock.PaymentGateway.Components;

namespace AspireApp.BedRock.SonetOps.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCheckoutServices(this IServiceCollection services)
        {
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<PaymentProcessor>();

            return services;
        }
    }
}