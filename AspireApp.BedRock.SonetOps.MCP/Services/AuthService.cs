using AspireApp.BedRock.SonetOps.MCP.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IAuthService
{
    Task<bool> ValidateApiKey(string apiKey);
    Task<ApiKey> GetApiKeyDetails(string apiKey);
    Task<bool> CheckQuota(string apiKey);
    Task UpdateUsage(string apiKey);
    Task<ApiKeyResponse> GetApiKeyStatus(string apiKey);
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
        try
        {
            var keyDetails = await GetApiKeyDetails(apiKey);
            return keyDetails != null && keyDetails.IsActive && 
                   (keyDetails.ExpiresAt == null || keyDetails.ExpiresAt > DateTime.UtcNow);
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiKey> GetApiKeyDetails(string apiKey)
    {
        if (_cache.TryGetValue($"apikey_{apiKey}", out ApiKey? details))
        {
            return details!;
        }

        // In a real implementation, this would check against a database
        if (apiKey.StartsWith("test_"))
        {
            var key = new ApiKey
            {
                Key = apiKey,
                OwnerId = "test_user",
                Tier = DetermineKeyTier(apiKey),
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

    public async Task<ApiKeyResponse> GetApiKeyStatus(string apiKey)
    {
        var keyDetails = await GetApiKeyDetails(apiKey);
        var plan = _plans[keyDetails.Tier];

        return new ApiKeyResponse
        {
            Key = apiKey,
            Tier = keyDetails.Tier,
            Status = keyDetails.IsActive ? "active" : "inactive",
            RemainingQuota = new Dictionary<string, int>
            {
                { "daily_requests", plan.RequestsPerDay - keyDetails.UsageQuotas["daily_requests"] }
            }
        };
    }

    private SubscriptionTier DetermineKeyTier(string apiKey)
    {
        return apiKey switch
        {
            var k when k.StartsWith("test_free_") => SubscriptionTier.Free,
            var k when k.StartsWith("test_basic_") => SubscriptionTier.Basic,
            var k when k.StartsWith("test_premium_") => SubscriptionTier.Premium,
            var k when k.StartsWith("test_enterprise_") => SubscriptionTier.Enterprise,
            _ => SubscriptionTier.Free
        };
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
                    MaxResponseTokens = 1000,
                    MaxContextTokens = 2000,
                    AllowedModels = new List<string> { "claude-instant-1" },
                    AllowsCustomInstructions = false,
                    AllowsWebAccess = false,
                    IncludesAuditLog = false,
                    PricePerMonth = 0
                }
            },
            {
                SubscriptionTier.Basic, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Basic,
                    RequestsPerDay = 1000,
                    MaxResponseTokens = 4000,
                    MaxContextTokens = 8000,
                    AllowedModels = new List<string> { "claude-instant-1", "claude-2" },
                    AllowsCustomInstructions = true,
                    AllowsWebAccess = false,
                    IncludesAuditLog = true,
                    PricePerMonth = 29.99m
                }
            },
            {
                SubscriptionTier.Premium, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Premium,
                    RequestsPerDay = 10000,
                    MaxResponseTokens = 8000,
                    MaxContextTokens = 16000,
                    AllowedModels = new List<string> { "claude-instant-1", "claude-2", "claude-2.1" },
                    AllowsCustomInstructions = true,
                    AllowsWebAccess = true,
                    IncludesAuditLog = true,
                    PricePerMonth = 99.99m
                }
            },
            {
                SubscriptionTier.Enterprise, new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Enterprise,
                    RequestsPerDay = 100000,
                    MaxResponseTokens = 16000,
                    MaxContextTokens = 32000,
                    AllowedModels = new List<string> { "claude-instant-1", "claude-2", "claude-2.1", "claude-3" },
                    AllowsCustomInstructions = true,
                    AllowsWebAccess = true,
                    IncludesAuditLog = true,
                    PricePerMonth = 499.99m
                }
            }
        };
    }
}