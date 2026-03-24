using AspireApp.BedRock.SonetOps.Registry.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.Registry.Services;

public interface IRoutingStrategy
{
    string Name { get; }
    Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs);
}

public class RoundRobinStrategy : IRoutingStrategy
{
    private readonly IMemoryCache _cache;
    private const string LastIndexKey = "RoundRobin_LastIndex";

    public string Name => "RoundRobin";

    public RoundRobinStrategy(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var mcps = availableMCPs.ToList();
        if (!mcps.Any()) throw new InvalidOperationException("No MCPs available");

        var lastIndex = _cache.GetOrCreate(LastIndexKey, _ => -1);
        var nextIndex = (lastIndex + 1) % mcps.Count;
        _cache.Set(LastIndexKey, nextIndex);

        return mcps[nextIndex];
    }
}

public class LeastLoadStrategy : IRoutingStrategy
{
    public string Name => "LeastLoad";

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        return availableMCPs
            .OrderBy(m => m.CurrentLoad)
            .First();
    }
}

public class GeographicProximityStrategy : IRoutingStrategy
{
    public string Name => "GeographicProximity";

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var agentLocation = GetLocation(agent.Metadata);
        
        return availableMCPs
            .OrderBy(m => CalculateDistance(agentLocation, GetLocation(m.Metadata)))
            .First();
    }

    private (double Lat, double Lon) GetLocation(Dictionary<string, string> metadata)
    {
        var hasLat = metadata.TryGetValue("latitude", out var latStr);
        var hasLon = metadata.TryGetValue("longitude", out var lonStr);

        if (!hasLat || !hasLon)
            return (0, 0);

        return (double.Parse(latStr!), double.Parse(lonStr!));
    }

    private double CalculateDistance((double Lat, double Lon) point1, (double Lat, double Lon) point2)
    {
        var d1 = point1.Lat * (Math.PI / 180.0);
        var d2 = point2.Lat * (Math.PI / 180.0);
        var d3 = d1 - d2;
        var d4 = (point1.Lon - point2.Lon) * (Math.PI / 180.0);

        var d = Math.Sin(d3 / 2) * Math.Sin(d3 / 2) +
                Math.Cos(d1) * Math.Cos(d2) *
                Math.Sin(d4 / 2) * Math.Sin(d4 / 2);
        return 2 * Math.Asin(Math.Sqrt(d)) * 6371; // Earth radius in kilometers
    }
}

public class WeightedLoadBalancingStrategy : IRoutingStrategy
{
    public string Name => "WeightedLoadBalancing";

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var weightedMCPs = availableMCPs.Select(m => new
        {
            MCP = m,
            Weight = CalculateWeight(m)
        });

        return weightedMCPs
            .OrderByDescending(w => w.Weight)
            .First()
            .MCP;
    }

    private double CalculateWeight(MCPRegistration mcp)
    {
        var loadWeight = 1.0 - (mcp.CurrentLoad / 100.0); // 0-1, higher is better
        var healthWeight = mcp.HealthStatus switch
        {
            HealthStatus.Healthy => 1.0,
            HealthStatus.Degraded => 0.5,
            HealthStatus.Unhealthy => 0.0,
            _ => 0.0
        };

        // Calculate capacity weight based on metadata
        var capacity = double.Parse(mcp.Metadata.GetValueOrDefault("capacity", "1.0"));
        var capacityWeight = Math.Min(capacity, 2.0) / 2.0; // Normalize to 0-1

        return (loadWeight * 0.4) + (healthWeight * 0.4) + (capacityWeight * 0.2);
    }
}

public class CapabilityMatchingStrategy : IRoutingStrategy
{
    public string Name => "CapabilityMatching";

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var scoredMCPs = availableMCPs.Select(m => new
        {
            MCP = m,
            Score = CalculateCapabilityScore(agent, m, requestType)
        });

        return scoredMCPs
            .OrderByDescending(s => s.Score)
            .First()
            .MCP;
    }

    private double CalculateCapabilityScore(AgentRegistration agent, MCPRegistration mcp, string requestType)
    {
        var score = 0.0;

        // Match capabilities
        foreach (var capability in agent.Capabilities)
        {
            if (mcp.Capabilities.TryGetValue(capability.Key, out var value) &&
                value == capability.Value)
            {
                score += 1.0;
            }
        }

        // Check request type support
        if (mcp.Metadata.TryGetValue("supportedRequestTypes", out var types) &&
            types.Contains(requestType))
        {
            score += 2.0;
        }

        // Check version compatibility
        if (IsVersionCompatible(agent, mcp))
        {
            score += 1.0;
        }

        return score;
    }

    private bool IsVersionCompatible(AgentRegistration agent, MCPRegistration mcp)
    {
        if (!agent.Capabilities.TryGetValue("version", out var agentVersion) ||
            !mcp.Metadata.TryGetValue("version", out var mcpVersion))
            return true;

        return Version.Parse(mcpVersion) >= Version.Parse(agentVersion);
    }
}

public class HybridRoutingStrategy : IRoutingStrategy
{
    private readonly IEnumerable<IRoutingStrategy> _strategies;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HybridRoutingStrategy> _logger;

    public string Name => "Hybrid";

    public HybridRoutingStrategy(
        IEnumerable<IRoutingStrategy> strategies,
        IMemoryCache cache,
        ILogger<HybridRoutingStrategy> logger)
    {
        _strategies = strategies;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var mcps = availableMCPs.ToList();
        
        // Get historical performance data
        var history = _cache.GetOrCreate($"routing_history_{agent.Id}", _ => new Dictionary<string, RoutingPerformance>());

        // Try each strategy and combine results
        var results = new List<(MCPRegistration MCP, double Score)>();

        foreach (var strategy in _strategies.Where(s => s.Name != Name))
        {
            try
            {
                var selectedMCP = await strategy.SelectMCPAsync(agent, requestType, mcps);
                var score = CalculateStrategyScore(strategy.Name, selectedMCP, history);
                results.Add((selectedMCP, score));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Strategy {Strategy} failed", strategy.Name);
            }
        }

        // Select the MCP with the highest score
        var bestResult = results
            .OrderByDescending(r => r.Score)
            .First();

        // Update history
        UpdateHistory(agent.Id, bestResult.MCP, strategy: Name);

        return bestResult.MCP;
    }

    private double CalculateStrategyScore(string strategy, MCPRegistration mcp, Dictionary<string, RoutingPerformance> history)
    {
        var baseScore = 0.0;

        // Consider historical performance
        if (history.TryGetValue($"{strategy}_{mcp.Id}", out var performance))
        {
            baseScore += performance.SuccessRate * 0.3;
            baseScore += (1.0 - performance.AverageLatency / 1000.0) * 0.2;
        }

        // Consider current state
        baseScore += (1.0 - mcp.CurrentLoad / 100.0) * 0.3;
        baseScore += (mcp.HealthStatus == HealthStatus.Healthy ? 0.2 : 0.0);

        return baseScore;
    }

    private void UpdateHistory(string agentId, MCPRegistration mcp, string strategy)
    {
        var key = $"routing_history_{agentId}_{strategy}_{mcp.Id}";
        var history = _cache.GetOrCreate(key, _ => new RoutingPerformance());

        // Update performance metrics
        history.RequestCount++;
        history.LastUsed = DateTime.UtcNow;

        _cache.Set(key, history);
    }

    private class RoutingPerformance
    {
        public int RequestCount { get; set; }
        public int SuccessCount { get; set; }
        public double AverageLatency { get; set; }
        public DateTime LastUsed { get; set; }
        public double SuccessRate => RequestCount == 0 ? 0 : (double)SuccessCount / RequestCount;
    }
}

public class FailoverRoutingStrategy : IRoutingStrategy
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<FailoverRoutingStrategy> _logger;

    public string Name => "Failover";

    public FailoverRoutingStrategy(IMemoryCache cache, ILogger<FailoverRoutingStrategy> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs)
    {
        var mcps = availableMCPs.ToList();
        var primaryMCP = GetPrimaryMCP(agent, mcps);
        
        if (IsHealthy(primaryMCP))
            return primaryMCP;

        // Primary is unhealthy, select backup
        var backupMCP = SelectBackupMCP(agent, mcps, primaryMCP);
        
        _logger.LogWarning(
            "Failing over from {PrimaryMCP} to {BackupMCP} for agent {AgentId}",
            primaryMCP.Id,
            backupMCP.Id,
            agent.Id);

        return backupMCP;
    }

    private MCPRegistration GetPrimaryMCP(AgentRegistration agent, List<MCPRegistration> mcps)
    {
        // Check if agent has preferred MCP
        if (!string.IsNullOrEmpty(agent.PreferredMCPId))
        {
            var preferred = mcps.FirstOrDefault(m => m.Id == agent.PreferredMCPId);
            if (preferred != null)
                return preferred;
        }

        // Use previously assigned primary if exists
        var primaryId = _cache.GetOrCreate($"primary_mcp_{agent.Id}", _ => "");
        if (!string.IsNullOrEmpty(primaryId))
        {
            var primary = mcps.FirstOrDefault(m => m.Id == primaryId);
            if (primary != null)
                return primary;
        }

        // Select new primary based on load and health
        var newPrimary = mcps
            .Where(m => m.HealthStatus == HealthStatus.Healthy)
            .OrderBy(m => m.CurrentLoad)
            .First();

        _cache.Set($"primary_mcp_{agent.Id}", newPrimary.Id);
        return newPrimary;
    }

    private MCPRegistration SelectBackupMCP(
        AgentRegistration agent,
        List<MCPRegistration> mcps,
        MCPRegistration primaryMCP)
    {
        // Check previously assigned backup
        var backupId = _cache.GetOrCreate($"backup_mcp_{agent.Id}", _ => "");
        if (!string.IsNullOrEmpty(backupId))
        {
            var backup = mcps.FirstOrDefault(m => m.Id == backupId && IsHealthy(m));
            if (backup != null)
                return backup;
        }

        // Select new backup based on:
        // 1. Different region than primary (if possible)
        // 2. Good health status
        // 3. Low load
        var backup = mcps
            .Where(m => m.Id != primaryMCP.Id && IsHealthy(m))
            .Where(m => !IsSameRegion(m, primaryMCP))
            .OrderBy(m => m.CurrentLoad)
            .FirstOrDefault()
            ?? mcps.Where(m => m.Id != primaryMCP.Id && IsHealthy(m))
                   .OrderBy(m => m.CurrentLoad)
                   .First();

        _cache.Set($"backup_mcp_{agent.Id}", backup.Id);
        return backup;
    }

    private bool IsHealthy(MCPRegistration mcp)
    {
        return mcp.HealthStatus == HealthStatus.Healthy &&
               mcp.Status == MCPStatus.Online &&
               mcp.CurrentLoad < 90;
    }

    private bool IsSameRegion(MCPRegistration mcp1, MCPRegistration mcp2)
    {
        return mcp1.Metadata.TryGetValue("region", out var region1) &&
               mcp2.Metadata.TryGetValue("region", out var region2) &&
               region1 == region2;
    }
}