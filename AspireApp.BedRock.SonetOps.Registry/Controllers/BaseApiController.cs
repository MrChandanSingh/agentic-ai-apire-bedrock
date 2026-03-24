using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.Registry.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger<BaseApiController> _logger;

    protected BaseApiController(ILogger<BaseApiController> logger)
    {
        _logger = logger;
    }

    protected IActionResult HandleException(Exception ex, string operation)
    {
        _logger.LogError(ex, "Error during {Operation}: {Message}", operation, ex.Message);
        
        return ex switch
        {
            ArgumentException => BadRequest(ex.Message),
            UnauthorizedAccessException => Unauthorized(ex.Message),
            KeyNotFoundException => NotFound(ex.Message),
            InvalidOperationException => StatusCode(409, ex.Message),
            _ => StatusCode(500, "An unexpected error occurred")
        };
    }
}