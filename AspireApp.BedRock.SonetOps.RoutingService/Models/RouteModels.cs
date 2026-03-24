using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.RoutingService.Models;

public class RouteRequest
{
    public Location Source { get; set; } = null!;
    public Location Destination { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransportMode Mode { get; set; }
    public List<string>? Preferences { get; set; }
    public DateTime? DepartureTime { get; set; }
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
}

public enum TransportMode
{
    Cycle,
    Walk,
    Car,
    Train,
    Bus
}

public class RouteResponse
{
    public List<Route> Routes { get; set; } = new();
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class Route
{
    public string RouteId { get; set; } = Guid.NewGuid().ToString();
    public TransportMode Mode { get; set; }
    public double Distance { get; set; }
    public TimeSpan Duration { get; set; }
    public List<RouteSegment> Segments { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class RouteSegment
{
    public string SegmentId { get; set; } = Guid.NewGuid().ToString();
    public TransportMode Mode { get; set; }
    public Location StartLocation { get; set; } = null!;
    public Location EndLocation { get; set; } = null!;
    public double Distance { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Instructions { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}