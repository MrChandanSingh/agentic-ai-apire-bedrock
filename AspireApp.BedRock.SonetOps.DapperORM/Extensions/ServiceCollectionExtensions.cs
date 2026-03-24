using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.BedRock.SonetOps.DapperORM.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperORM(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        return services;
    }

    public static IServiceCollection AddDapperRepository(
        this IServiceCollection services,
        string connectionName)
    {
        services.AddScoped<IDapperRepository>(sp =>
        {
            var factory = sp.GetRequiredService<IDbConnectionFactory>();
            var logger = sp.GetRequiredService<ILogger<DapperRepository>>();
            return new DapperRepository(factory, logger, connectionName);
        });

        return services;
    }
}