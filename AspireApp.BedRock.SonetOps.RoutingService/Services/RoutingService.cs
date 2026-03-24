using AspireApp.BedRock.SonetOps.RoutingService.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.RoutingService.Services;

public interface IRoutingService
{
    Task<RouteResponse> GetRoutes(RouteRequest request);
    Task<Route> GetRouteDetails(string routeId);
}

public class RoutingService : IRoutingService
{
    private readonly ILogger<RoutingService> _logger;
    private readonly IMemoryCache _cache;

    public RoutingService(ILogger<RoutingService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<RouteResponse> GetRoutes(RouteRequest request)
    {
        _logger.LogInformation("Calculating routes for {Mode} from {Source} to {Destination}", 
            request.Mode, request.Source.Address, request.Destination.Address);

        var response = new RouteResponse();

        // In a real implementation, this would call external routing services
        // For now, generating sample data
        var route = new Route
        {
            Mode = request.Mode,
            Distance = CalculateDistance(request.Source, request.Destination),
            Duration = CalculateDuration(request.Mode, request.Source, request.Destination)
        };

        // Add segments based on transport mode
        route.Segments = GenerateSegments(request);

        // Add mode-specific metadata
        route.Metadata = GetModeMetadata(request.Mode);

        response.Routes.Add(route);

        // Cache the route
        _cache.Set(route.RouteId, route, TimeSpan.FromMinutes(30));

        return response;
    }

    public async Task<Route> GetRouteDetails(string routeId)
    {
        if (_cache.TryGetValue(routeId, out Route? route) && route != null)
        {
            return route;
        }

        throw new KeyNotFoundException($"Route {routeId} not found");
    }

    private double CalculateDistance(Location source, Location destination)
    {
        // Simple distance calculation - in real implementation use proper geo calculations
        var latDiff = source.Latitude - destination.Latitude;
        var lonDiff = source.Longitude - destination.Longitude;
        return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff) * 111.32; // Rough conversion to kilometers
    }

    private TimeSpan CalculateDuration(TransportMode mode, Location source, Location destination)
    {
        var distance = CalculateDistance(source, destination);
        var speedKmH = mode switch
        {
            TransportMode.Walk => 5,
            TransportMode.Cycle => 15,
            TransportMode.Car => 50,
            TransportMode.Bus => 30,
            TransportMode.Train => 80,
            _ => 30
        };

        return TimeSpan.FromHours(distance / speedKmH);
    }

    private List<RouteSegment> GenerateSegments(RouteRequest request)
    {
        var segments = new List<RouteSegment>();
        var distance = CalculateDistance(request.Source, request.Destination);
        
        // Create intermediate points
        var numSegments = request.Mode switch
        {
            TransportMode.Walk => 1,
            TransportMode.Cycle => 2,
            TransportMode.Car => 3,
            TransportMode.Bus => 4,
            TransportMode.Train => 3,
            _ => 2
        };

        for (int i = 0; i < numSegments; i++)
        {
            var progress = (i + 1.0) / numSegments;
            var endLat = request.Source.Latitude + (request.Destination.Latitude - request.Source.Latitude) * progress;
            var endLon = request.Source.Longitude + (request.Destination.Longitude - request.Source.Longitude) * progress;

            var segment = new RouteSegment
            {
                Mode = request.Mode,
                StartLocation = i == 0 ? request.Source : new Location 
                { 
                    Latitude = request.Source.Latitude + (request.Destination.Latitude - request.Source.Latitude) * (i * 1.0 / numSegments),
                    Longitude = request.Source.Longitude + (request.Destination.Longitude - request.Source.Longitude) * (i * 1.0 / numSegments)
                },
                EndLocation = i == numSegments - 1 ? request.Destination : new Location
                {
                    Latitude = endLat,
                    Longitude = endLon
                },
                Distance = distance / numSegments,
                Duration = TimeSpan.FromMinutes(30),
                Instructions = GenerateInstructions(request.Mode, i)
            };

            segments.Add(segment);
        }

        return segments;
    }

    private List<string> GenerateInstructions(TransportMode mode, int segmentIndex)
    {
        return mode switch
        {
            TransportMode.Walk => new List<string> { "Continue straight", "Follow the pedestrian path" },
            TransportMode.Cycle => new List<string> { "Use the bike lane", "Watch for traffic signals" },
            TransportMode.Car => new List<string> { "Drive straight ahead", "Follow traffic rules" },
            TransportMode.Bus => new List<string> { "Board the bus", "Get off at next stop" },
            TransportMode.Train => new List<string> { "Board the train", "Exit at the station" },
            _ => new List<string> { "Continue on your route" }
        };
    }

    private Dictionary<string, string> GetModeMetadata(TransportMode mode)
    {
        return mode switch
        {
            TransportMode.Walk => new Dictionary<string, string>
            {
                { "terrain", "flat" },
                { "difficulty", "easy" }
            },
            TransportMode.Cycle => new Dictionary<string, string>
            {
                { "bikeType", "any" },
                { "bikeLanes", "available" }
            },
            TransportMode.Car => new Dictionary<string, string>
            {
                { "trafficLevel", "moderate" },
                { "roadType", "highway" }
            },
            TransportMode.Bus => new Dictionary<string, string>
            {
                { "operator", "local transit" },
                { "fare", "2.50" }
            },
            TransportMode.Train => new Dictionary<string, string>
            {
                { "operator", "national rail" },
                { "class", "standard" }
            },
            _ => new Dictionary<string, string>()
        };
    }
}