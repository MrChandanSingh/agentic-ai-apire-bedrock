using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.Registry.Models;

public class MCPRegistration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Dictionary<string, string> Capabilities { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public MCPStatus Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public List<string> SupportedAgentTypes { get; set; } = new();
    public int CurrentLoad { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public class AgentRegistration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string PreferredMCPId { get; set; } = null!;
    public Dictionary<string, string> Capabilities { get; set; } = new();
    public AgentStatus Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MCPStatus
{
    Online,
    Offline,
    Busy,
    Maintenance
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentStatus
{
    Available,
    Busy,
    Offline
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public class RoutingDecision
{
    public string AgentId { get; set; } = null!;
    public string MCPId { get; set; } = null!;
    public Dictionary<string, string> RoutingMetadata { get; set; } = new();
    public DateTime DecisionTime { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}