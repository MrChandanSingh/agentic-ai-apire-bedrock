using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.RoutingService.Models;

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
    public bool SupportsRealTimeUpdates { get; set; }
    public bool SupportsAlternativeRoutes { get; set; }
    public List<TransportMode> SupportedModes { get; set; } = new();
    public decimal PricePerMonth { get; set; }
}