using AspireApp.BedRock.SonetOps.RoutingService.Models;
using AspireApp.BedRock.SonetOps.RoutingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.RoutingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutingController : ControllerBase
{
    private readonly IRoutingService _routingService;
    private readonly IAuthService _authService;
    private readonly ILogger<RoutingController> _logger;

    public RoutingController(
        IRoutingService routingService,
        IAuthService authService,
        ILogger<RoutingController> logger)
    {
        _routingService = routingService;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("routes")]
    public async Task<IActionResult> GetRoutes(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        [FromBody] RouteRequest request)
    {
        try
        {
            // Validate API key
            if (!await _authService.ValidateApiKey(apiKey))
            {
                return Unauthorized("Invalid API key");
            }

            // Check quota
            if (!await _authService.CheckQuota(apiKey))
            {
                return StatusCode(429, "Daily request quota exceeded");
            }

            // Get API key details to check subscription features
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);
            var routes = await _routingService.GetRoutes(request);

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return Ok(routes);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing route request");
            return StatusCode(500, "An error occurred processing your request");
        }
    }

    [HttpGet("routes/{routeId}")]
    public async Task<IActionResult> GetRouteDetails(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        string routeId)
    {
        try
        {
            // Validate API key
            if (!await _authService.ValidateApiKey(apiKey))
            {
                return Unauthorized("Invalid API key");
            }

            // Check quota
            if (!await _authService.CheckQuota(apiKey))
            {
                return StatusCode(429, "Daily request quota exceeded");
            }

            var route = await _routingService.GetRouteDetails(routeId);

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return Ok(route);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route details");
            return StatusCode(500, "An error occurred processing your request");
        }
    }
}