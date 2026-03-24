using AspireApp.BedRock.SonetOps.RoutingService.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.RoutingService.Services;

public interface IAuthService
{
    Task<bool> ValidateApiKey(string apiKey);
    Task<ApiKey> GetApiKeyDetails(string apiKey);
    Task<bool> CheckQuota(string apiKey);
    Task UpdateUsage(string apiKey);
}

public class AuthService : IAuthService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<SubscriptionTier, SubscriptionPlan> _plans;

    public AuthService(IMemoryCache cache, ILogger<AuthService> logger)
    {
        _cache = cache;
        _logger = logger;
        _plans = InitializePlans();
    }

    public async Task<bool> ValidateApiKey(string apiKey)
    {
        var keyDetails = await GetApiKeyDetails(apiKey);
        return keyDetails != null && keyDetails.IsActive && 
               (keyDetails.ExpiresAt == null || keyDetails.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<ApiKey> GetApiKeyDetails(string apiKey)
    {
        if (_cache.TryGetValue($"apikey_{apiKey}", out ApiKey? details))
        {
            return details!;
        }

        // In a real implementation, this would check against a database
        // For now, creating a sample API key if it matches a pattern
        if (apiKey.StartsWith("test_"))
        {
            var key = new ApiKey
            {
                Key = apiKey,
                OwnerId = "test_user",
                Tier = SubscriptionTier.Basic,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true,
                UsageQuotas = new Dictionary<string, int>
                {
                    { "daily_requests", 0 },
                    { "monthly_requests", 0 }
                }
            };

            _cache.Set($"apikey_{apiKey}", key, TimeSpan.FromHours(1));
            return key;
        }

        throw new UnauthorizedAccessException("Invalid API key");
    }

    public async Task<bool> CheckQuota(string apiKey)
    {
        var keyDetails = await GetApiKeyDetails(apiKey);
        var plan = _plans[keyDetails.Tier];

        return keyDetails.UsageQuotas["daily_requests"] < plan.RequestsPerDay;
    }

    public async Task UpdateUsage(string apiKey)
    {
        var keyDetails = await GetApiKeyDetails(apiKey);
        keyDetails.UsageQuotas["daily_requests"]++;
        keyDetails.UsageQuotas["monthly_requests"]++;

        _cache.Set($"apikey_{apiKey}", keyDetails, TimeSpan.FromHours(1));
    }

    private Dictionary<SubscriptionTier, SubscriptionPlan> InitializePlans()
    {
        return new Dictionary<SubscriptionTier, SubscriptionPlan>
        {
            {
                SubscriptionTier.Free, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Free,
                    RequestsPerDay = 100,
                    SupportsRealTimeUpdates = false,
                    SupportsAlternativeRoutes = false,
                    SupportedModes = new List<TransportMode> { TransportMode.Walk, TransportMode.Car },
                    PricePerMonth = 0
                }
            },
            {
                SubscriptionTier.Basic, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Basic,
                    RequestsPerDay = 1000,
                    SupportsRealTimeUpdates = false,
                    SupportsAlternativeRoutes = true,
                    SupportedModes = new List<TransportMode> { TransportMode.Walk, TransportMode.Car, TransportMode.Bus },
                    PricePerMonth = 29.99m
                }
            },
            {
                SubscriptionTier.Premium, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Premium,
                    RequestsPerDay = 10000,
                    SupportsRealTimeUpdates = true,
                    SupportsAlternativeRoutes = true,
                    SupportedModes = new List<TransportMode> { TransportMode.Walk, TransportMode.Car, TransportMode.Bus, TransportMode.Train, TransportMode.Cycle },
                    PricePerMonth = 99.99m
                }
            },
            {
                SubscriptionTier.Enterprise, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Enterprise,
                    RequestsPerDay = 100000,
                    SupportsRealTimeUpdates = true,
                    SupportsAlternativeRoutes = true,
                    SupportedModes = new List<TransportMode> { TransportMode.Walk, TransportMode.Car, TransportMode.Bus, TransportMode.Train, TransportMode.Cycle },
                    PricePerMonth = 499.99m
                }
            }
        };
    }
}