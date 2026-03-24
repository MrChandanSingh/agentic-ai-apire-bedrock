using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class ProcessingResponse
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Result { get; set; } = new();
    public ProcessingMetrics Metrics { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ProcessingMetrics
{
    public double ProcessingTimeMs { get; set; }
    public int TokensUsed { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MCPStatus
{
    Online,
    Offline,
    Busy,
    Maintenance
}

public class MCPRegistration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Dictionary<string, string> Capabilities { get; set; } = new();
    public MCPStatus Status { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public int CurrentLoad { get; set; }
}