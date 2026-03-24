using AspireApp.BedRock.SonetOps.Registry.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.Registry.Services;

public interface IAgentDecisionService
{
    Task<MCPRegistration> SelectMCPAsync(AgentRegistration agent, string requestType, IEnumerable<MCPRegistration> availableMCPs);
    Task<Dictionary<string, string>> AnalyzeRequestAsync(string requestType, string requestContent);
    Task<bool> ValidateContextMatchAsync(MCPContext context, Dictionary<string, string> requirements);
}

public class AgentDecisionService : IAgentDecisionService
{
    private readonly IContextManager _contextManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AgentDecisionService> _logger;

    public AgentDecisionService(
        IContextManager contextManager,
        IMemoryCache cache,
        ILogger<AgentDecisionService> logger)
    {
        _contextManager = contextManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPRegistration> SelectMCPAsync(
        AgentRegistration agent,
        string requestType,
        IEnumerable<MCPRegistration> availableMCPs)
    {
        // Analyze the request to determine requirements
        var requirements = await AnalyzeRequestAsync(requestType, agent.Capabilities["requestContent"]);

        // Add agent-specific requirements
        requirements["agentType"] = agent.Type;
        foreach (var capability in agent.Capabilities)
        {
            requirements[$"agent.{capability.Key}"] = capability.Value;
        }

        var mcps = availableMCPs.ToList();
        var bestMatch = await FindBestMatchAsync(mcps, requirements);

        if (bestMatch == null)
        {
            throw new InvalidOperationException("No suitable MCP found for the request");
        }

        return bestMatch;
    }

    public async Task<Dictionary<string, string>> AnalyzeRequestAsync(
        string requestType,
        string requestContent)
    {
        var requirements = new Dictionary<string, string>();

        // Basic request type analysis
        requirements["requestType"] = requestType;

        // Analyze content length and complexity
        var contentLength = requestContent.Length;
        requirements["content.length"] = contentLength.ToString();

        if (contentLength > 1000)
        {
            requirements["performance.throughput"] = "50";
        }

        // Analyze for specific keywords
        foreach (var keyword in ExtractKeywords(requestContent))
        {
            requirements[$"keyword.{keyword}"] = "true";
        }

        // Check for language-specific requirements
        var language = DetectLanguage(requestContent);
        if (language != null)
        {
            requirements["model.language"] = language;
        }

        // Check for model-specific requirements
        if (requestContent.Contains("gpt-4") || requestContent.Contains("claude-3"))
        {
            requirements["model.type"] = "advanced";
            requirements["model.maxTokens"] = "8000";
        }

        // Performance requirements based on content analysis
        if (IsComplexRequest(requestContent))
        {
            requirements["performance.responseTime"] = "2000";
            requirements["performance.errorRate"] = "0.001";
        }

        return requirements;
    }

    public async Task<bool> ValidateContextMatchAsync(
        MCPContext context,
        Dictionary<string, string> requirements)
    {
        var score = await _contextManager.CalculateContextMatchScoreAsync(context, requirements);
        return score >= 0.8; // 80% match required
    }

    private async Task<MCPRegistration> FindBestMatchAsync(
        List<MCPRegistration> mcps,
        Dictionary<string, string> requirements)
    {
        var matches = new List<(MCPRegistration MCP, double Score, MCPContext Context)>();

        foreach (var mcp in mcps)
        {
            try
            {
                var context = await _contextManager.GetContextAsync(mcp.Id);
                var score = await _contextManager.CalculateContextMatchScoreAsync(context, requirements);
                
                if (score > 0.5) // Minimum threshold
                {
                    matches.Add((mcp, score, context));
                }
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("No context found for MCP {MCPId}", mcp.Id);
                continue;
            }
        }

        if (!matches.Any())
            return null!;

        // Apply additional factors
        var finalScores = matches.Select(m =>
        {
            var finalScore = m.Score;

            // Consider current load
            finalScore *= (1 - (m.MCP.CurrentLoad / 100.0));

            // Consider health status
            finalScore *= m.MCP.HealthStatus switch
            {
                HealthStatus.Healthy => 1.0,
                HealthStatus.Degraded => 0.5,
                _ => 0.1
            };

            // Consider performance metrics
            finalScore *= CalculatePerformanceScore(m.Context.PerformanceMetrics);

            return (m.MCP, FinalScore: finalScore);
        });

        return finalScores
            .OrderByDescending(m => m.FinalScore)
            .First()
            .MCP;
    }

    private double CalculatePerformanceScore(PerformanceMetrics metrics)
    {
        var score = 1.0;

        // Response time (lower is better)
        score *= Math.Min(1.0, 2000 / Math.Max(metrics.AverageResponseTime, 1));

        // Throughput (higher is better)
        score *= Math.Min(metrics.ThroughputPerSecond / 100.0, 1.0);

        // Error rate (lower is better)
        score *= (1 - Math.Min(metrics.ErrorRate, 0.5));

        return score;
    }

    private List<string> ExtractKeywords(string content)
    {
        var keywords = new List<string>();
        var words = content.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Check for code-related keywords
        if (words.Any(w => w.Contains("code") || w.Contains("program") || w.Contains("function")))
        {
            keywords.Add("code");
        }

        // Check for data-related keywords
        if (words.Any(w => w.Contains("data") || w.Contains("database") || w.Contains("sql")))
        {
            keywords.Add("data");
        }

        // Check for AI-related keywords
        if (words.Any(w => w.Contains("ai") || w.Contains("model") || w.Contains("train")))
        {
            keywords.Add("ai");
        }

        return keywords;
    }

    private string? DetectLanguage(string content)
    {
        // Simple language detection based on keywords
        if (content.Contains("python") || content.Contains(".py"))
            return "python";
        if (content.Contains("javascript") || content.Contains(".js"))
            return "javascript";
        if (content.Contains("java") || content.Contains(".java"))
            return "java";

        return null;
    }

    private bool IsComplexRequest(string content)
    {
        // Check for indicators of complexity
        if (content.Length > 1000)
            return true;

        if (content.Count(c => c == '{') > 5) // Complex JSON/code
            return true;

        if (ExtractKeywords(content).Count > 3)
            return true;

        return false;
    }
}