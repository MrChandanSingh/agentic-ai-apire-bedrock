using AspireApp.BedRock.SonetOps.Registry.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.Registry.Services;

public interface IRegistryService
{
    Task<MCPRegistration> RegisterMCPAsync(MCPRegistration registration);
    Task<AgentRegistration> RegisterAgentAsync(AgentRegistration registration);
    Task<MCPRegistration> GetMCPAsync(string mcpId);
    Task<IEnumerable<MCPRegistration>> GetAllMCPsAsync();
    Task<RoutingDecision> GetRoutingDecisionAsync(string agentId, string requestType);
    Task UpdateMCPHealthAsync(string mcpId, HealthStatus status);
    Task UpdateMCPLoadAsync(string mcpId, int currentLoad);
    Task<bool> HeartbeatAsync(string id, bool isMCP);
}

public class RegistryService : IRegistryService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RegistryService> _logger;
    private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(30);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RegistryService(IMemoryCache cache, ILogger<RegistryService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPRegistration> RegisterMCPAsync(MCPRegistration registration)
    {
        await _lock.WaitAsync();
        try
        {
            var mcps = GetMCPs();
            
            // Update or add registration
            registration.LastHeartbeat = DateTime.UtcNow;
            mcps[registration.Id] = registration;
            
            _cache.Set("mcps", mcps);
            _logger.LogInformation("MCP {Name} ({Id}) registered successfully", registration.Name, registration.Id);
            
            return registration;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AgentRegistration> RegisterAgentAsync(AgentRegistration registration)
    {
        await _lock.WaitAsync();
        try
        {
            var agents = GetAgents();
            
            // Update or add registration
            registration.LastHeartbeat = DateTime.UtcNow;
            agents[registration.Id] = registration;
            
            _cache.Set("agents", agents);
            _logger.LogInformation("Agent {Name} ({Id}) registered successfully", registration.Name, registration.Id);
            
            return registration;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<MCPRegistration> GetMCPAsync(string mcpId)
    {
        var mcps = GetMCPs();
        if (!mcps.TryGetValue(mcpId, out var registration))
        {
            throw new KeyNotFoundException($"MCP with ID {mcpId} not found");
        }

        // Check heartbeat
        if (DateTime.UtcNow - registration.LastHeartbeat > _heartbeatTimeout)
        {
            registration.Status = MCPStatus.Offline;
            _logger.LogWarning("MCP {Id} appears to be offline", mcpId);
        }

        return registration;
    }

    public async Task<IEnumerable<MCPRegistration>> GetAllMCPsAsync()
    {
        return GetMCPs().Values;
    }

    public async Task<RoutingDecision> GetRoutingDecisionAsync(string agentId, string requestType)
    {
        var agents = GetAgents();
        var mcps = GetMCPs();

        if (!agents.TryGetValue(agentId, out var agent))
        {
            throw new KeyNotFoundException($"Agent with ID {agentId} not found");
        }

        // Get available MCPs
        var availableMCPs = mcps.Values
            .Where(m => m.Status == MCPStatus.Online &&
                       m.HealthStatus == HealthStatus.Healthy &&
                       m.SupportedAgentTypes.Contains(agent.Type))
            .ToList();

        if (!availableMCPs.Any())
        {
            throw new InvalidOperationException("No suitable MCP available");
        }

        // Check preferred MCP first
        if (!string.IsNullOrEmpty(agent.PreferredMCPId))
        {
            var preferredMCP = availableMCPs.FirstOrDefault(m => m.Id == agent.PreferredMCPId);
            if (preferredMCP != null && IsMCPSuitable(preferredMCP, agent, requestType))
            {
                return CreateRoutingDecision(agent, preferredMCP, "Preferred MCP available");
            }
        }

        // Find best MCP based on load and capabilities
        var selectedMCP = availableMCPs
            .OrderBy(m => m.CurrentLoad)
            .FirstOrDefault(m => IsMetadataCompatible(m.Metadata, agent.Capabilities));

        if (selectedMCP == null)
        {
            throw new InvalidOperationException("No compatible MCP found");
        }

        return CreateRoutingDecision(agent, selectedMCP, "Selected based on load balancing");
    }

    public async Task UpdateMCPHealthAsync(string mcpId, HealthStatus status)
    {
        await _lock.WaitAsync();
        try
        {
            var mcps = GetMCPs();
            if (mcps.TryGetValue(mcpId, out var registration))
            {
                registration.HealthStatus = status;
                _cache.Set("mcps", mcps);
                _logger.LogInformation("MCP {Id} health status updated to {Status}", mcpId, status);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateMCPLoadAsync(string mcpId, int currentLoad)
    {
        await _lock.WaitAsync();
        try
        {
            var mcps = GetMCPs();
            if (mcps.TryGetValue(mcpId, out var registration))
            {
                registration.CurrentLoad = currentLoad;
                _cache.Set("mcps", mcps);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> HeartbeatAsync(string id, bool isMCP)
    {
        await _lock.WaitAsync();
        try
        {
            if (isMCP)
            {
                var mcps = GetMCPs();
                if (mcps.TryGetValue(id, out var mcp))
                {
                    mcp.LastHeartbeat = DateTime.UtcNow;
                    mcp.Status = MCPStatus.Online;
                    _cache.Set("mcps", mcps);
                    return true;
                }
            }
            else
            {
                var agents = GetAgents();
                if (agents.TryGetValue(id, out var agent))
                {
                    agent.LastHeartbeat = DateTime.UtcNow;
                    agent.Status = AgentStatus.Available;
                    _cache.Set("agents", agents);
                    return true;
                }
            }

            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private Dictionary<string, MCPRegistration> GetMCPs()
    {
        return _cache.GetOrCreate("mcps", _ => new Dictionary<string, MCPRegistration>());
    }

    private Dictionary<string, AgentRegistration> GetAgents()
    {
        return _cache.GetOrCreate("agents", _ => new Dictionary<string, AgentRegistration>());
    }

    private bool IsMetadataCompatible(Dictionary<string, string> mcpMetadata, Dictionary<string, string> agentCapabilities)
    {
        return agentCapabilities.All(ac => 
            mcpMetadata.TryGetValue(ac.Key, out var value) && value == ac.Value);
    }

    private bool IsTimeoutExceeded(DateTime lastHeartbeat)
    {
        return DateTime.UtcNow - lastHeartbeat > _heartbeatTimeout;
    }

    private bool IsMaxLoadExceeded(MCPRegistration mcp, int maxLoad = 100)
    {
        return mcp.CurrentLoad >= maxLoad;
    }

    private bool IsHealthy(MCPRegistration mcp)
    {
        return mcp.HealthStatus == HealthStatus.Healthy;
    }

    private bool IsAvailable(MCPRegistration mcp)
    {
        return mcp.Status == MCPStatus.Online && !IsTimeoutExceeded(mcp.LastHeartbeat);
    }

    private bool IsMetadataMatch(MCPRegistration mcp, string requestType)
    {
        return mcp.Metadata.ContainsKey("supportedRequestTypes") &&
               mcp.Metadata["supportedRequestTypes"].Contains(requestType);
    }

    private bool IsCapabilityMatch(MCPRegistration mcp, AgentRegistration agent)
    {
        return agent.Capabilities.All(c => 
            mcp.Capabilities.ContainsKey(c.Key) && mcp.Capabilities[c.Key] == c.Value);
    }

    private bool IsAgentTypeSupported(MCPRegistration mcp, AgentRegistration agent)
    {
        return mcp.SupportedAgentTypes.Contains(agent.Type);
    }

    private bool IsValidForRequest(MCPRegistration mcp, AgentRegistration agent, string requestType)
    {
        return IsAvailable(mcp) &&
               IsHealthy(mcp) &&
               !IsMaxLoadExceeded(mcp) &&
               IsMetadataMatch(mcp, requestType) &&
               IsCapabilityMatch(mcp, agent) &&
               IsAgentTypeSupported(mcp, agent);
    }

    private bool IsNewMCPBetter(MCPRegistration currentMCP, MCPRegistration newMCP)
    {
        // Consider load balancing
        if (newMCP.CurrentLoad < currentMCP.CurrentLoad * 0.8) // 20% less load
            return true;

        // Consider health status
        if (newMCP.HealthStatus == HealthStatus.Healthy && currentMCP.HealthStatus != HealthStatus.Healthy)
            return true;

        // Consider capabilities (more capabilities is better)
        if (newMCP.Capabilities.Count > currentMCP.Capabilities.Count)
            return true;

        return false;
    }

    private bool IsRoutingStable(string agentId, string mcpId, TimeSpan minStabilityPeriod)
    {
        var routingHistory = _cache.GetOrCreate($"routing_{agentId}", _ => new List<RoutingDecision>());
        var lastRouting = routingHistory.LastOrDefault();

        if (lastRouting == null)
            return true;

        // Check if enough time has passed since last routing change
        if (DateTime.UtcNow - lastRouting.DecisionTime < minStabilityPeriod)
            return false;

        return true;
    }

    private bool IsLoadBalanced(MCPRegistration mcp, IEnumerable<MCPRegistration> allMCPs)
    {
        var avgLoad = allMCPs.Average(m => m.CurrentLoad);
        return mcp.CurrentLoad <= avgLoad * 1.2; // Within 20% of average load
    }

    private bool IsRoutingOptimal(RoutingDecision decision, AgentRegistration agent, MCPRegistration mcp)
    {
        // Check if this is the best possible routing based on multiple factors
        if (!IsLoadBalanced(mcp, GetMCPs().Values))
            return false;

        if (!IsRoutingStable(agent.Id, mcp.Id, TimeSpan.FromMinutes(5)))
            return false;

        return true;
    }

    private bool IsAffinityPreferred(AgentRegistration agent, MCPRegistration mcp)
    {
        // Check if there's a preferred affinity between agent and MCP
        return agent.PreferredMCPId == mcp.Id;
    }

    private bool IsLocationOptimal(AgentRegistration agent, MCPRegistration mcp)
    {
        // In a real implementation, this would check geographic proximity
        // For now, just return true
        return true;
    }

    private bool IsSecurityCompliant(AgentRegistration agent, MCPRegistration mcp)
    {
        // Check security requirements
        if (!mcp.Metadata.TryGetValue("securityLevel", out var mcpSecLevel))
            return false;

        if (!agent.Capabilities.TryGetValue("requiredSecurityLevel", out var agentSecLevel))
            return false;

        return string.Compare(mcpSecLevel, agentSecLevel, true) >= 0;
    }

    private bool IsPerformanceAcceptable(MCPRegistration mcp)
    {
        return mcp.CurrentLoad < 80; // Less than 80% load
    }

    private bool IsLatencyAcceptable(MCPRegistration mcp)
    {
        // In a real implementation, this would check actual latency metrics
        // For now, just return true
        return true;
    }

    private bool IsVersionCompatible(AgentRegistration agent, MCPRegistration mcp)
    {
        if (!agent.Capabilities.TryGetValue("version", out var agentVersion))
            return true; // No version requirement

        if (!mcp.Metadata.TryGetValue("version", out var mcpVersion))
            return false;

        return Version.Parse(mcpVersion) >= Version.Parse(agentVersion);
    }

    private bool IsFeatureCompatible(AgentRegistration agent, MCPRegistration mcp)
    {
        if (!agent.Capabilities.TryGetValue("requiredFeatures", out var requiredFeatures))
            return true; // No feature requirements

        if (!mcp.Metadata.TryGetValue("supportedFeatures", out var supportedFeatures))
            return false;

        var required = requiredFeatures.Split(',');
        var supported = supportedFeatures.Split(',');

        return required.All(r => supported.Contains(r.Trim()));
    }

    private bool IsResourceAvailable(MCPRegistration mcp)
    {
        // Check if MCP has enough resources for new connections
        return mcp.CurrentLoad < 90; // Less than 90% load
    }

    private bool IsBackupNeeded(MCPRegistration primaryMCP)
    {
        // Check if we need a backup MCP
        return primaryMCP.HealthStatus != HealthStatus.Healthy ||
               primaryMCP.CurrentLoad > 70;
    }

    private bool IsMCPSuitable(MCPRegistration mcp, AgentRegistration agent, string requestType)
    {
        return IsValidForRequest(mcp, agent, requestType) &&
               IsSecurityCompliant(agent, mcp) &&
               IsPerformanceAcceptable(mcp) &&
               IsLatencyAcceptable(mcp) &&
               IsVersionCompatible(agent, mcp) &&
               IsFeatureCompatible(agent, mcp) &&
               IsResourceAvailable(mcp);
    }

    private RoutingDecision CreateRoutingDecision(AgentRegistration agent, MCPRegistration mcp, string reason)
    {
        var decision = new RoutingDecision
        {
            AgentId = agent.Id,
            MCPId = mcp.Id,
            Reason = reason,
            RoutingMetadata = new Dictionary<string, string>
            {
                { "mcpLoad", mcp.CurrentLoad.ToString() },
                { "mcpHealth", mcp.HealthStatus.ToString() },
                { "isPreferred", (agent.PreferredMCPId == mcp.Id).ToString() }
            }
        };

        // Store routing history
        var routingHistory = _cache.GetOrCreate($"routing_{agent.Id}", _ => new List<RoutingDecision>());
        routingHistory.Add(decision);
        if (routingHistory.Count > 10) // Keep last 10 decisions
            routingHistory.RemoveAt(0);
        _cache.Set($"routing_{agent.Id}", routingHistory);

        return decision;
    }
}