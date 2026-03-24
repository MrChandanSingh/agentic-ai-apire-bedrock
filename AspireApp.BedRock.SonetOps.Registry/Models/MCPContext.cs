using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.Registry.Models;

public class MCPContext
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MCPId { get; set; } = null!;
    public Dictionary<string, ContextValue> Variables { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Specializations { get; set; } = new();
    public Dictionary<string, double> Weights { get; set; } = new();
    public List<ProcessingRule> ProcessingRules { get; set; } = new();
    public ModelCapabilities ModelCapabilities { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ContextValue
{
    public string Value { get; set; } = null!;
    public ValueType Type { get; set; }
    public double Weight { get; set; } = 1.0;
    public bool IsCritical { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType
{
    String,
    Number,
    Boolean,
    Array,
    Object
}

public class ProcessingRule
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Pattern { get; set; } = null!;
    public double Priority { get; set; }
    public Dictionary<string, string> Requirements { get; set; } = new();
    public List<string> ExcludedTopics { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class ModelCapabilities
{
    public int MaxTokens { get; set; }
    public List<string> SupportedModels { get; set; } = new();
    public Dictionary<string, ModelFeature> Features { get; set; } = new();
    public List<string> SupportedLanguages { get; set; } = new();
    public Dictionary<string, double> ModelWeights { get; set; } = new();
    public bool SupportsStreaming { get; set; }
    public bool SupportsFineTuning { get; set; }
}

public class ModelFeature
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double EfficiencyScore { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
}

public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public double ThroughputPerSecond { get; set; }
    public int ConcurrentRequests { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<string, double> ModelLatencies { get; set; } = new();
    public Dictionary<string, int> RequestCounts { get; set; } = new();
}