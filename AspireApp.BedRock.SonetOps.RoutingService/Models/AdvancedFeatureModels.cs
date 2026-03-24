using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.RoutingService.Models;

public class RealTimeUpdate
{
    public string RouteId { get; set; } = null!;
    public string SegmentId { get; set; } = null!;
    public UpdateType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UpdateType
{
    Delay,
    Incident,
    WeatherAlert,
    TrafficUpdate,
    ServiceChange
}

public class FareEstimate
{
    public string RouteId { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public Dictionary<string, decimal> Adjustments { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public List<FareComponent> Components { get; set; } = new();
}

public class FareComponent
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public bool IsOptional { get; set; }
}

public class WeatherInfo
{
    public double Temperature { get; set; }
    public int Humidity { get; set; }
    public string Condition { get; set; } = null!;
    public double WindSpeed { get; set; }
    public double PrecipitationChance { get; set; }
    public List<WeatherAlert> Alerts { get; set; } = new();
}

public class WeatherAlert
{
    public string Type { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class RouteAnalytics
{
    public string RouteId { get; set; } = null!;
    public int PopularityScore { get; set; }
    public Dictionary<string, double> Statistics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> CustomData { get; set; } = new();
}

public class OptimizationPreferences
{
    public bool MinimizeTime { get; set; }
    public bool MinimizeCost { get; set; }
    public bool AvoidHighTraffic { get; set; }
    public bool PreferScenicRoute { get; set; }
    public bool ConsiderWeather { get; set; }
    public Dictionary<string, int> CustomWeights { get; set; } = new();
}