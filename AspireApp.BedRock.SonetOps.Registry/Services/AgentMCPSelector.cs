namespace AspireApp.BedRock.SonetOps.Registry.Services;

public class AgentMCPSelector
{
    private readonly IAgentDocumentationReader _docReader;
    private readonly IContextManager _contextManager;
    private readonly ILogger<AgentMCPSelector> _logger;

    public AgentMCPSelector(
        IAgentDocumentationReader docReader,
        IContextManager contextManager,
        ILogger<AgentMCPSelector> logger)
    {
        _docReader = docReader;
        _contextManager = contextManager;
        _logger = logger;
    }

    public async Task<MCPRegistration> SelectBestMCPAsync(
        AgentRegistration agent,
        string requestType,
        Dictionary<string, string> requirements,
        IEnumerable<MCPRegistration> availableMCPs)
    {
        var mcpScores = new List<(MCPRegistration MCP, double Score)>();

        foreach (var mcp in availableMCPs)
        {
            try
            {
                // Read and analyze MCP documentation
                var mcpDoc = await _docReader.ReadMCPDocumentationAsync(mcp.Id);
                
                // Get MCP context
                var context = await _contextManager.GetContextAsync(mcp.Id);

                // Calculate documentation-based score
                var docScore = await CalculateDocumentationScoreAsync(mcpDoc, requestType, requirements);

                // Calculate context-based score
                var contextScore = await _contextManager.CalculateContextMatchScoreAsync(context, requirements);

                // Calculate final score considering both documentation and context
                var finalScore = (docScore * 0.4) + (contextScore * 0.6);

                // Adjust score based on current load and health
                finalScore *= GetHealthAdjustment(mcp);
                finalScore *= GetLoadAdjustment(mcp);

                mcpScores.Add((mcp, finalScore));

                _logger.LogDebug(
                    "MCP {MCPId} scores: Doc={DocScore}, Context={ContextScore}, Final={FinalScore}",
                    mcp.Id,
                    docScore,
                    contextScore,
                    finalScore);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to evaluate MCP {MCPId}", mcp.Id);
            }
        }

        // Select the MCP with the highest score
        var bestMatch = mcpScores
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestMatch.MCP == null)
        {
            throw new InvalidOperationException("No suitable MCP found for the request");
        }

        return bestMatch.MCP;
    }

    private async Task<double> CalculateDocumentationScoreAsync(
        MCPCapabilityDocument doc,
        string requestType,
        Dictionary<string, string> requirements)
    {
        var score = 0.0;
        var maxScore = 0.0;

        // Check if request type is supported
        if (doc.SupportedRequestTypes.Contains(requestType))
        {
            score += 10;
        }
        maxScore += 10;

        // Check subscription tier capabilities
        var tierScore = CalculateTierScore(doc.SubscriptionTiers, requirements);
        score += tierScore;
        maxScore += 10;

        // Check endpoint support
        if (doc.Endpoints.ContainsKey(requestType))
        {
            score += 5;
            
            // Check if endpoint supports all required parameters
            var endpoint = doc.Endpoints[requestType];
            foreach (var req in requirements)
            {
                if (endpoint.RequestSchema.ContainsKey(req.Key))
                {
                    score += 1;
                }
                maxScore += 1;
            }
        }
        maxScore += 5;

        // Check limits
        foreach (var req in requirements)
        {
            if (doc.Limits.TryGetValue(req.Key, out var limit))
            {
                if (IsWithinLimit(req.Value, limit))
                {
                    score += 2;
                }
                maxScore += 2;
            }
        }

        return maxScore > 0 ? score / maxScore : 0;
    }

    private double CalculateTierScore(Dictionary<string, SubscriptionTier> tiers, Dictionary<string, string> requirements)
    {
        var score = 0.0;

        // Find the minimum tier that supports all requirements
        foreach (var tier in tiers.OrderBy(t => t.Value.RequestsPerDay))
        {
            var meetsRequirements = requirements.All(req =>
                tier.Value.Features.Any(f => f.Contains(req.Key, StringComparison.OrdinalIgnoreCase)));

            if (meetsRequirements)
            {
                score = 10.0 * (tier.Value.RequestsPerDay / 100000.0); // Normalize by max requests
                break;
            }
        }

        return score;
    }

    private double GetHealthAdjustment(MCPRegistration mcp)
    {
        return mcp.HealthStatus switch
        {
            HealthStatus.Healthy => 1.0,
            HealthStatus.Degraded => 0.5,
            _ => 0.1
        };
    }

    private double GetLoadAdjustment(MCPRegistration mcp)
    {
        return Math.Max(0.1, 1.0 - (mcp.CurrentLoad / 100.0));
    }

    private bool IsWithinLimit(string value, object limit)
    {
        if (int.TryParse(value, out var intValue) && limit is int intLimit)
            return intValue <= intLimit;

        if (decimal.TryParse(value, out var decValue) && limit is decimal decLimit)
            return decValue <= decLimit;

        return true;
    }
}