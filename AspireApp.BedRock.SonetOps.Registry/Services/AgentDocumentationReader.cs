using System.Text.RegularExpressions;
using Markdig;
using Microsoft.Extensions.Caching.Memory;

namespace AspireApp.BedRock.SonetOps.Registry.Services;

public interface IAgentDocumentationReader
{
    Task<MCPCapabilityDocument> ReadMCPDocumentationAsync(string mcpId);
    Task<Dictionary<string, object>> ExtractEndpointDetailsAsync(string markdown);
    Task<Dictionary<string, object>> AnalyzeCapabilitiesAsync(string markdown);
    Task<bool> IsRequestSupportedAsync(string mcpId, string requestType, Dictionary<string, string> requirements);
}

public class MCPCapabilityDocument
{
    public List<string> SupportedRequestTypes { get; set; } = new();
    public Dictionary<string, EndpointInfo> Endpoints { get; set; } = new();
    public Dictionary<string, SubscriptionTier> SubscriptionTiers { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, object> Limits { get; set; } = new();
}

public class EndpointInfo
{
    public string Path { get; set; } = null!;
    public string Method { get; set; } = null!;
    public Dictionary<string, object> RequestSchema { get; set; } = new();
    public Dictionary<string, object> ResponseSchema { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
}

public class SubscriptionTier
{
    public string Name { get; set; } = null!;
    public int RequestsPerDay { get; set; }
    public List<string> Features { get; set; } = new();
    public Dictionary<string, object> Limits { get; set; } = new();
}

public class AgentDocumentationReader : IAgentDocumentationReader
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AgentDocumentationReader> _logger;

    public AgentDocumentationReader(IMemoryCache cache, ILogger<AgentDocumentationReader> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<MCPCapabilityDocument> ReadMCPDocumentationAsync(string mcpId)
    {
        // Try to get from cache first
        var cacheKey = $"mcp_docs_{mcpId}";
        if (_cache.TryGetValue<MCPCapabilityDocument>(cacheKey, out var cachedDoc))
        {
            return cachedDoc;
        }

        // Read and parse the API documentation
        var markdownContent = await ReadApiDocumentationAsync(mcpId);
        var document = new MCPCapabilityDocument
        {
            Endpoints = await ExtractEndpointDetailsAsync(markdownContent),
            Capabilities = await ExtractCapabilitiesAsync(markdownContent),
            SubscriptionTiers = await ExtractSubscriptionTiersAsync(markdownContent),
            Limits = await ExtractLimitsAsync(markdownContent)
        };

        // Cache the parsed document
        _cache.Set(cacheKey, document, TimeSpan.FromHours(1));

        return document;
    }

    public async Task<Dictionary<string, object>> ExtractEndpointDetailsAsync(string markdown)
    {
        var endpoints = new Dictionary<string, object>();
        
        // Use regex to find endpoint sections
        var endpointPattern = @"###\s+([^\n]+)\n```http\n(POST|GET|PUT|DELETE)\s+([^\n]+)";
        var matches = Regex.Matches(markdown, endpointPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            var method = match.Groups[2].Value.Trim();
            var path = match.Groups[3].Value.Trim();

            // Extract request/response schema if available
            var schemaPattern = @"```json\n(.*?)\n```";
            var schemaMatches = Regex.Matches(match.Groups[0].Value, schemaPattern, RegexOptions.Singleline);

            var endpoint = new EndpointInfo
            {
                Path = path,
                Method = method,
                RequestSchema = schemaMatches.Count > 0 ? ParseJsonSchema(schemaMatches[0].Groups[1].Value) : new(),
                ResponseSchema = schemaMatches.Count > 1 ? ParseJsonSchema(schemaMatches[1].Groups[1].Value) : new()
            };

            endpoints[name] = endpoint;
        }

        return endpoints;
    }

    public async Task<Dictionary<string, object>> AnalyzeCapabilitiesAsync(string markdown)
    {
        var capabilities = new Dictionary<string, object>();

        // Extract capabilities from Overview section
        var overviewPattern = @"##\s*Overview\n(.*?)(?=##|\z)";
        var overviewMatch = Regex.Match(markdown, overviewPattern, RegexOptions.Singleline);
        if (overviewMatch.Success)
        {
            capabilities["overview"] = ExtractCapabilitiesFromText(overviewMatch.Groups[1].Value);
        }

        // Extract subscription tier capabilities
        var tierPattern = @"###\s+(\w+)\s+Tier\n(.*?)(?=###|\z)";
        var tierMatches = Regex.Matches(markdown, tierPattern, RegexOptions.Singleline);
        foreach (Match match in tierMatches)
        {
            var tierName = match.Groups[1].Value.ToLower();
            capabilities[$"tier_{tierName}"] = ExtractCapabilitiesFromText(match.Groups[2].Value);
        }

        return capabilities;
    }

    public async Task<bool> IsRequestSupportedAsync(string mcpId, string requestType, Dictionary<string, string> requirements)
    {
        var doc = await ReadMCPDocumentationAsync(mcpId);
        
        // Check if request type is supported
        if (!doc.SupportedRequestTypes.Contains(requestType))
            return false;

        // Check if required capabilities are available
        foreach (var req in requirements)
        {
            // Check against capabilities
            if (!doc.Capabilities.Contains(req.Key))
                return false;

            // Check against limits
            if (doc.Limits.TryGetValue(req.Key, out var limit))
            {
                if (!IsWithinLimit(req.Value, limit))
                    return false;
            }
        }

        return true;
    }

    private async Task<string> ReadApiDocumentationAsync(string mcpId)
    {
        // Read the api.md file for the specified MCP
        var filePath = $"docs/api.md"; // Adjust path based on MCP
        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read API documentation for MCP {MCPId}", mcpId);
            throw;
        }
    }

    private async Task<List<string>> ExtractCapabilitiesAsync(string markdown)
    {
        var capabilities = new List<string>();

        // Extract capabilities from markdown
        var capabilityPattern = @"[\-\*]\s*`([^`]+)`\s*:\s*([^\n]+)";
        var matches = Regex.Matches(markdown, capabilityPattern);

        foreach (Match match in matches)
        {
            capabilities.Add(match.Groups[1].Value.Trim());
        }

        return capabilities;
    }

    private async Task<Dictionary<string, SubscriptionTier>> ExtractSubscriptionTiersAsync(string markdown)
    {
        var tiers = new Dictionary<string, SubscriptionTier>();

        var tierPattern = @"###\s+(\w+)\s+Tier\s*\(\$?([\d.]+)/month\)\n(.*?)(?=###|\z)";
        var matches = Regex.Matches(markdown, tierPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var name = match.Groups[1].Value;
            var price = decimal.Parse(match.Groups[2].Value);
            var description = match.Groups[3].Value;

            var features = ExtractFeatures(description);
            var limits = ExtractLimits(description);

            tiers[name] = new SubscriptionTier
            {
                Name = name,
                Features = features,
                Limits = limits,
                RequestsPerDay = ExtractRequestLimit(description)
            };
        }

        return tiers;
    }

    private async Task<Dictionary<string, object>> ExtractLimitsAsync(string markdown)
    {
        var limits = new Dictionary<string, object>();

        // Extract limits from Request Limits section
        var limitsPattern = @"##\s*Request Limits\n(.*?)(?=##|\z)";
        var limitsMatch = Regex.Match(markdown, limitsPattern, RegexOptions.Singleline);

        if (limitsMatch.Success)
        {
            var limitLines = limitsMatch.Groups[1].Value.Split('\n');
            foreach (var line in limitLines)
            {
                if (line.Contains(":"))
                {
                    var parts = line.Split(':');
                    var key = parts[0].Trim('- ', ' ');
                    var value = parts[1].Trim();
                    limits[key] = ParseLimit(value);
                }
            }
        }

        return limits;
    }

    private List<string> ExtractFeatures(string text)
    {
        var features = new List<string>();
        var featurePattern = @"[\-\*]\s*([^\n]+)";
        var matches = Regex.Matches(text, featurePattern);

        foreach (Match match in matches)
        {
            features.Add(match.Groups[1].Value.Trim());
        }

        return features;
    }

    private Dictionary<string, object> ExtractLimits(string text)
    {
        var limits = new Dictionary<string, object>();
        var limitPattern = @"(\w+)\s*:\s*(\d+)";
        var matches = Regex.Matches(text, limitPattern);

        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            limits[key] = ParseLimit(value);
        }

        return limits;
    }

    private int ExtractRequestLimit(string text)
    {
        var requestPattern = @"(\d+)\s+requests?\s+per\s+day";
        var match = Regex.Match(text, requestPattern);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private List<string> ExtractCapabilitiesFromText(string text)
    {
        var capabilities = new List<string>();
        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            if (line.StartsWith("-") || line.StartsWith("*"))
            {
                capabilities.Add(line.Trim('- ', '*', ' '));
            }
        }

        return capabilities;
    }

    private Dictionary<string, object> ParseJsonSchema(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private object ParseLimit(string value)
    {
        if (int.TryParse(value, out var intValue))
            return intValue;

        if (decimal.TryParse(value, out var decValue))
            return decValue;

        return value;
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