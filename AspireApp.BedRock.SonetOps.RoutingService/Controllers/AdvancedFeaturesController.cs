using AspireApp.BedRock.SonetOps.RoutingService.Models;
using AspireApp.BedRock.SonetOps.RoutingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.RoutingService.Controllers;

[ApiController]
[Route("api/routing")]
public class AdvancedFeaturesController : ControllerBase
{
    private readonly IAdvancedFeatureService _advancedFeatureService;
    private readonly IAuthService _authService;
    private readonly ILogger<AdvancedFeaturesController> _logger;

    public AdvancedFeaturesController(
        IAdvancedFeatureService advancedFeatureService,
        IAuthService authService,
        ILogger<AdvancedFeaturesController> logger)
    {
        _advancedFeatureService = advancedFeatureService;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("routes/{routeId}/updates")]
    public async Task<IActionResult> GetRealTimeUpdates(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId)
    {
        try
        {
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            if (keyDetails.Tier < SubscriptionTier.Premium)
            {
                return StatusCode(403, "Real-time updates are only available in Premium and Enterprise tiers");
            }

            var updates = await _advancedFeatureService.GetRealTimeUpdate(routeId);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time updates");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpGet("routes/{routeId}/fare")]
    public async Task<IActionResult> GetFareEstimate(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId)
    {
        try
        {
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            var estimate = await _advancedFeatureService.GetFareEstimate(routeId);
            return Ok(estimate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fare estimate");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpGet("routes/{routeId}/weather")]
    public async Task<IActionResult> GetWeatherInfo(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId)
    {
        try
        {
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            if (keyDetails.Tier < SubscriptionTier.Premium)
            {
                return StatusCode(403, "Weather information is only available in Premium and Enterprise tiers");
            }

            // For demo purposes, using Seattle coordinates
            var location = new Location { Latitude = 47.6062, Longitude = -122.3321 };
            var weather = await _advancedFeatureService.GetWeatherInfo(location);
            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather information");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpGet("routes/{routeId}/analytics")]
    public async Task<IActionResult> GetRouteAnalytics(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId)
    {
        try
        {
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            if (keyDetails.Tier < SubscriptionTier.Basic)
            {
                return StatusCode(403, "Analytics are only available in Basic tier and above");
            }

            var analytics = await _advancedFeatureService.GetRouteAnalytics(routeId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route analytics");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpPost("routes/{routeId}/optimize")]
    public async Task<IActionResult> OptimizeRoute(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId,
        [FromBody] OptimizationPreferences preferences)
    {
        try
        {
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            if (keyDetails.Tier < SubscriptionTier.Premium)
            {
                return StatusCode(403, "Route optimization is only available in Premium and Enterprise tiers");
            }

            // Get the original route (in a real implementation, this would come from a database)
            var route = new Route { RouteId = routeId };
            var optimizedRoute = await _advancedFeatureService.OptimizeRoute(route, preferences);
            return Ok(optimizedRoute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing route");
            return StatusCode(500, "An error occurred processing your request");
        }
    }
}