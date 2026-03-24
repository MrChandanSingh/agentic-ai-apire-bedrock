using AspireApp.BedRock.SonetOps.Registry.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.Registry.Services;

public interface IContextManager
{
    Task<MCPContext> GetContextAsync(string mcpId);
    Task UpdateContextAsync(string mcpId, MCPContext context);
    Task<double> CalculateContextMatchScoreAsync(MCPContext context, Dictionary<string, string> requirements);
    Task<List<MCPContext>> FindMatchingContextsAsync(Dictionary<string, string> requirements);
    Task UpdateMetricsAsync(string mcpId, PerformanceMetrics metrics);
}

public class ContextManager : IContextManager
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ContextManager> _logger;

    public ContextManager(IMemoryCache cache, ILogger<ContextManager> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPContext> GetContextAsync(string mcpId)
    {
        var context = _cache.Get<MCPContext>($"context_{mcpId}");
        if (context == null)
        {
            throw new KeyNotFoundException($"Context not found for MCP {mcpId}");
        }
        return context;
    }

    public async Task UpdateContextAsync(string mcpId, MCPContext context)
    {
        context.LastUpdated = DateTime.UtcNow;
        _cache.Set($"context_{mcpId}", context);
        
        // Update search index
        UpdateSearchIndex(mcpId, context);
        
        _logger.LogInformation("Updated context for MCP {MCPId}", mcpId);
    }

    public async Task<double> CalculateContextMatchScoreAsync(
        MCPContext context,
        Dictionary<string, string> requirements)
    {
        var score = 0.0;
        var maxScore = 0.0;

        foreach (var requirement in requirements)
        {
            var weight = GetRequirementWeight(requirement.Key);
            maxScore += weight;

            // Check variables
            if (context.Variables.TryGetValue(requirement.Key, out var contextValue))
            {
                if (IsMatch(contextValue.Value, requirement.Value))
                {
                    score += weight * contextValue.Weight;
                }
            }

            // Check specializations
            if (requirement.Key == "specialization" &&
                context.Specializations.Contains(requirement.Value))
            {
                score += weight;
            }

            // Check keywords
            if (requirement.Key == "keyword" &&
                context.Keywords.Contains(requirement.Value))
            {
                score += weight;
            }

            // Check model capabilities
            if (requirement.Key.StartsWith("model.") &&
                CheckModelCapability(context.ModelCapabilities, requirement))
            {
                score += weight;
            }

            // Check performance requirements
            if (requirement.Key.StartsWith("performance.") &&
                CheckPerformanceRequirement(context.PerformanceMetrics, requirement))
            {
                score += weight;
            }
        }

        return maxScore > 0 ? score / maxScore : 0;
    }

    public async Task<List<MCPContext>> FindMatchingContextsAsync(
        Dictionary<string, string> requirements)
    {
        var allContexts = GetAllContexts();
        var matches = new List<(MCPContext Context, double Score)>();

        foreach (var context in allContexts)
        {
            var score = await CalculateContextMatchScoreAsync(context, requirements);
            if (score > 0.5) // Minimum match threshold
            {
                matches.Add((context, score));
            }
        }

        return matches
            .OrderByDescending(m => m.Score)
            .Select(m => m.Context)
            .ToList();
    }

    public async Task UpdateMetricsAsync(string mcpId, PerformanceMetrics metrics)
    {
        var context = await GetContextAsync(mcpId);
        context.PerformanceMetrics = metrics;
        context.LastUpdated = DateTime.UtcNow;
        
        await UpdateContextAsync(mcpId, context);
    }

    private double GetRequirementWeight(string requirement)
    {
        return requirement switch
        {
            var r when r.StartsWith("model.") => 2.0,
            var r when r.StartsWith("performance.") => 1.5,
            "specialization" => 2.0,
            "keyword" => 1.0,
            _ => 1.0
        };
    }

    private bool IsMatch(string contextValue, string requirement)
    {
        // Handle numeric comparisons
        if (double.TryParse(contextValue, out var contextNum) &&
            requirement.Contains(':'))
        {
            var parts = requirement.Split(':');
            if (parts.Length != 2) return false;

            var op = parts[0];
            if (!double.TryParse(parts[1], out var reqNum))
                return false;

            return op switch
            {
                "gt" => contextNum > reqNum,
                "lt" => contextNum < reqNum,
                "gte" => contextNum >= reqNum,
                "lte" => contextNum <= reqNum,
                "eq" => Math.Abs(contextNum - reqNum) < 0.001,
                _ => false
            };
        }

        // Handle array/list values
        if (contextValue.StartsWith("[") && contextValue.EndsWith("]"))
        {
            var values = contextValue.Trim('[', ']').Split(',');
            return values.Contains(requirement);
        }

        // Simple string match
        return contextValue.Equals(requirement, StringComparison.OrdinalIgnoreCase);
    }

    private bool CheckModelCapability(ModelCapabilities capabilities, KeyValuePair<string, string> requirement)
    {
        return requirement.Key switch
        {
            "model.maxTokens" => capabilities.MaxTokens >= int.Parse(requirement.Value),
            "model.supportedModel" => capabilities.SupportedModels.Contains(requirement.Value),
            "model.feature" => capabilities.Features.ContainsKey(requirement.Value),
            "model.language" => capabilities.SupportedLanguages.Contains(requirement.Value),
            _ => false
        };
    }

    private bool CheckPerformanceRequirement(PerformanceMetrics metrics, KeyValuePair<string, string> requirement)
    {
        return requirement.Key switch
        {
            "performance.responseTime" => metrics.AverageResponseTime <= double.Parse(requirement.Value),
            "performance.throughput" => metrics.ThroughputPerSecond >= double.Parse(requirement.Value),
            "performance.errorRate" => metrics.ErrorRate <= double.Parse(requirement.Value),
            _ => false
        };
    }

    private List<MCPContext> GetAllContexts()
    {
        return _cache.GetKeys<MCPContext>()
            .Where(k => k.StartsWith("context_"))
            .Select(k => _cache.Get<MCPContext>(k))
            .Where(c => c != null)
            .ToList()!;
    }

    private void UpdateSearchIndex(string mcpId, MCPContext context)
    {
        var index = _cache.GetOrCreate("context_search_index", _ => new Dictionary<string, HashSet<string>>());

        // Index keywords
        foreach (var keyword in context.Keywords)
        {
            if (!index.ContainsKey(keyword))
                index[keyword] = new HashSet<string>();
            index[keyword].Add(mcpId);
        }

        // Index specializations
        foreach (var spec in context.Specializations)
        {
            if (!index.ContainsKey(spec))
                index[spec] = new HashSet<string>();
            index[spec].Add(mcpId);
        }

        // Index model capabilities
        foreach (var model in context.ModelCapabilities.SupportedModels)
        {
            var key = $"model:{model}";
            if (!index.ContainsKey(key))
                index[key] = new HashSet<string>();
            index[key].Add(mcpId);
        }

        _cache.Set("context_search_index", index);
    }
}