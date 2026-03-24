using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class ApiKey
{
    public string Key { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
    public SubscriptionTier Tier { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, int> UsageQuotas { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionTier
{
    Free,
    Basic,
    Premium,
    Enterprise
}

public class SubscriptionPlan
{
    public SubscriptionTier Tier { get; set; }
    public int RequestsPerDay { get; set; }
    public int MaxResponseTokens { get; set; }
    public int MaxContextTokens { get; set; }
    public List<string> AllowedModels { get; set; } = new();
    public bool AllowsCustomInstructions { get; set; }
    public bool AllowsWebAccess { get; set; }
    public bool IncludesAuditLog { get; set; }
    public decimal PricePerMonth { get; set; }
}

public class ApiKeyResponse
{
    public string Key { get; set; } = null!;
    public SubscriptionTier Tier { get; set; }
    public string Status { get; set; } = null!;
    public Dictionary<string, int> RemainingQuota { get; set; } = new();
}