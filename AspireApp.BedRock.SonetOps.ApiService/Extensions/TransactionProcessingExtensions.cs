using AspireApp.BedRock.SonetOps.ApiService.Services;
using StackExchange.Redis;

namespace AspireApp.BedRock.SonetOps.ApiService.Extensions
{
    public static class TransactionProcessingExtensions
    {
        public static IServiceCollection AddTransactionProcessing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            // Add services
            services.AddSingleton<IDistributedLock, RedisDistributedLock>();
            services.AddScoped<ITransactionProcessingService, TransactionProcessingService>();
            
            // Add background service
            services.AddHostedService<TransactionProcessingBackgroundService>();

            return services;
        }
    }
}