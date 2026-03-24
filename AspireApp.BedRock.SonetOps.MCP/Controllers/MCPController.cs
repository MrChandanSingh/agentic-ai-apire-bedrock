using AspireApp.BedRock.SonetOps.MCP.Models;
using AspireApp.BedRock.SonetOps.MCP.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.MCP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MCPController : ControllerBase
{
    private readonly IMCPService _mcpService;
    private readonly IAuthService _authService;
    private readonly ILogger<MCPController> _logger;

    public MCPController(
        IMCPService mcpService,
        IAuthService authService,
        ILogger<MCPController> logger)
    {
        _mcpService = mcpService;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("instructions")]
    public async Task<ActionResult<Instruction>> CreateInstruction(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        [FromBody] CreateInstructionRequest request)
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

            // Get subscription details
            var keyDetails = await _authService.GetApiKeyDetails(apiKey);

            // Check if the instruction type is allowed for this subscription
            if (!IsInstructionTypeAllowed(keyDetails.Tier, request.Type))
            {
                return StatusCode(403, $"Instruction type '{request.Type}' is not available in your subscription tier");
            }

            var instruction = await _mcpService.CreateInstructionAsync(
                request.Type,
                request.Content,
                request.Parameters);

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return CreatedAtAction(
                nameof(GetInstruction),
                new { id = instruction.Id },
                instruction);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating instruction");
            return StatusCode(500, "Error creating instruction");
        }
    }

    [HttpGet("instructions/{id}")]
    public async Task<ActionResult<Instruction>> GetInstruction(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        int id)
    {
        try
        {
            // Validate API key
            if (!await _authService.ValidateApiKey(apiKey))
            {
                return Unauthorized("Invalid API key");
            }

            var instruction = await _mcpService.GetInstructionByIdAsync(id);
            if (instruction == null)
            {
                return NotFound();
            }

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return instruction;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving instruction");
            return StatusCode(500, "Error retrieving instruction");
        }
    }

    [HttpPost("instructions/{id}/process")]
    public async Task<ActionResult<Instruction>> ProcessInstruction(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        int id)
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

            var instruction = await _mcpService.ProcessInstructionAsync(id);

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return Ok(instruction);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing instruction {Id}", id);
            return StatusCode(500, "Error processing instruction");
        }
    }

    [HttpGet("instructions/pending")]
    public async Task<ActionResult<IEnumerable<Instruction>>> GetPendingInstructions(
        [FromHeader(Name = "X-API-Key")] string apiKey)
    {
        try
        {
            // Validate API key
            if (!await _authService.ValidateApiKey(apiKey))
            {
                return Unauthorized("Invalid API key");
            }

            var instructions = await _mcpService.GetPendingInstructionsAsync();

            // Update usage
            await _authService.UpdateUsage(apiKey);

            return Ok(instructions);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending instructions");
            return StatusCode(500, "Error retrieving pending instructions");
        }
    }

    [HttpGet("auth/status")]
    public async Task<ActionResult<ApiKeyResponse>> GetApiKeyStatus(
        [FromHeader(Name = "X-API-Key")] string apiKey)
    {
        try
        {
            var status = await _authService.GetApiKeyStatus(apiKey);
            return Ok(status);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authentication failed");
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API key status");
            return StatusCode(500, "Error retrieving API key status");
        }
    }

    private bool IsInstructionTypeAllowed(SubscriptionTier tier, string instructionType)
    {
        // Define which instruction types are allowed for each tier
        return tier switch
        {
            SubscriptionTier.Free => new[] { "basic", "simple" }.Contains(instructionType.ToLower()),
            SubscriptionTier.Basic => new[] { "basic", "simple", "standard" }.Contains(instructionType.ToLower()),
            SubscriptionTier.Premium => new[] { "basic", "simple", "standard", "advanced" }.Contains(instructionType.ToLower()),
            SubscriptionTier.Enterprise => true, // All instruction types allowed
            _ => false
        };
    }
}

public record CreateInstructionRequest(string Type, string Content, string? Parameters = null);