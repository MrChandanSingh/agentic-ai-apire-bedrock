using AspireApp.BedRock.SonetOps.Registry.Models;
using AspireApp.BedRock.SonetOps.Registry.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.BedRock.SonetOps.Registry.Controllers;

public class RegistryController : BaseApiController
{
    private readonly IContextManager _contextManager;
    private readonly IAgentDocumentationReader _docReader;
    private readonly IAgentDecisionService _decisionService;

    public RegistryController(
        IContextManager contextManager,
        IAgentDocumentationReader docReader,
        IAgentDecisionService decisionService,
        ILogger<RegistryController> logger)
        : base(logger)
    {
        _contextManager = contextManager;
        _docReader = docReader;
        _decisionService = decisionService;
    }

    [HttpPost("mcp")]
    public async Task<IActionResult> RegisterMCP([FromBody] MCPRegistration registration)
    {
        try
        {
            // Add initial context
            var context = new MCPContext
            {
                MCPId = registration.Id,
                Variables = registration.Metadata.ToDictionary(
                    x => x.Key,
                    x => new ContextValue { Value = x.Value }
                ),
                Keywords = registration.Capabilities.Keys.ToList(),
                Specializations = registration.SupportedAgentTypes.ToList(),
                ModelCapabilities = new ModelCapabilities
                {
                    MaxTokens = int.Parse(registration.Capabilities.GetValueOrDefault("maxTokens", "1000"))
                }
            };

            await _contextManager.UpdateContextAsync(registration.Id, context);
            
            _logger.LogInformation(
                "Registered MCP {Name} ({Id}) with {Count} capabilities",
                registration.Name,
                registration.Id,
                registration.Capabilities.Count);

            return Ok(registration);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "registering MCP");
        }
    }

    [HttpGet("mcp/{mcpId}/context")]
    public async Task<IActionResult> GetMCPContext(string mcpId)
    {
        try
        {
            var context = await _contextManager.GetContextAsync(mcpId);
            return Ok(context);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting MCP context");
        }
    }

    [HttpPut("mcp/{mcpId}/context")]
    public async Task<IActionResult> UpdateMCPContext(string mcpId, [FromBody] MCPContext context)
    {
        try
        {
            await _contextManager.UpdateContextAsync(mcpId, context);
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "updating MCP context");
        }
    }

    [HttpGet("routing")]
    public async Task<IActionResult> GetRoutingDecision(
        [FromQuery] string agentId,
        [FromQuery] string requestType,
        [FromBody] Dictionary<string, string> requirements)
    {
        try
        {
            var mcpDoc = await _docReader.ReadMCPDocumentationAsync(agentId);
            var isSupported = await _docReader.IsRequestSupportedAsync(agentId, requestType, requirements);

            if (!isSupported)
            {
                return BadRequest("Request type or requirements not supported by any MCP");
            }

            // In a real implementation, you would get this from your MCP registry
            var availableMCPs = new List<MCPRegistration>();

            var selectedMCP = await _decisionService.SelectBestMCPAsync(
                new AgentRegistration { Id = agentId },
                requestType,
                requirements,
                availableMCPs);

            return Ok(new RoutingDecision
            {
                AgentId = agentId,
                MCPId = selectedMCP.Id,
                RoutingMetadata = new Dictionary<string, string>
                {
                    ["mcpLoad"] = selectedMCP.CurrentLoad.ToString(),
                    ["mcpHealth"] = selectedMCP.HealthStatus.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting routing decision");
        }
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromQuery] string id, [FromQuery] bool isMCP)
    {
        try
        {
            // In a real implementation, update last seen timestamp and status
            _logger.LogInformation("Received heartbeat from {Type} {Id}", isMCP ? "MCP" : "Agent", id);
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "processing heartbeat");
        }
    }

    [HttpPut("mcp/{mcpId}/health")]
    public async Task<IActionResult> UpdateHealth(string mcpId, [FromQuery] HealthStatus status)
    {
        try
        {
            var context = await _contextManager.GetContextAsync(mcpId);
            context.LastUpdated = DateTime.UtcNow;
            await _contextManager.UpdateContextAsync(mcpId, context);

            _logger.LogInformation("Updated health status for MCP {Id} to {Status}", mcpId, status);
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex, "updating health status");
        }
    }

    [HttpGet("mcps")]
    public async Task<IActionResult> GetAvailableMCPs(
        [FromQuery] string? capability = null,
        [FromQuery] int? maxLoad = null,
        [FromQuery] HealthStatus? healthStatus = null)
    {
        try
        {
            // In a real implementation, query your MCP registry
            var mcps = new List<MCPRegistration>();

            if (!string.IsNullOrEmpty(capability))
            {
                mcps = mcps.Where(m => m.Capabilities.ContainsKey(capability)).ToList();
            }

            if (maxLoad.HasValue)
            {
                mcps = mcps.Where(m => m.CurrentLoad <= maxLoad.Value).ToList();
            }

            if (healthStatus.HasValue)
            {
                mcps = mcps.Where(m => m.HealthStatus == healthStatus.Value).ToList();
            }

            return Ok(mcps);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting available MCPs");
        }
    }
}