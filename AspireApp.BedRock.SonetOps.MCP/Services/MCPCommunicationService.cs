using AspireApp.BedRock.SonetOps.Registry.Models;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IMCPCommunicationService
{
    Task<bool> RegisterWithRegistryAsync();
    Task<bool> SendHeartbeatAsync();
    Task<MCPRegistration> GetMCPDetailsAsync(string mcpId);
    Task<bool> ForwardRequestToMCPAsync(string mcpId, string requestType, object request);
    Task<T> SendRequestToMCPAsync<T>(string mcpId, string endpoint, HttpMethod method, object? data = null);
    Task NotifyLoadChangeAsync(int currentLoad);
    Task UpdateHealthStatusAsync(HealthStatus status);
    Task<RoutingDecision> GetRoutingDecisionAsync(string agentId, string requestType);
}

public class MCPCommunicationService : IMCPCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MCPCommunicationService> _logger;
    private readonly string _registryUrl;
    private readonly MCPRegistration _localMCP;
    private Timer? _heartbeatTimer;
    private Timer? _healthCheckTimer;

    public MCPCommunicationService(
        HttpClient httpClient,
        ILogger<MCPCommunicationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _registryUrl = configuration["Registry:Url"] ?? throw new ArgumentNullException("Registry:Url");
        
        _localMCP = new MCPRegistration
        {
            Name = configuration["MCP:Name"] ?? "Unknown",
            Url = configuration["MCP:Url"] ?? throw new ArgumentNullException("MCP:Url"),
            Capabilities = ParseCapabilities(configuration["MCP:Capabilities"]),
            SupportedAgentTypes = ParseAgentTypes(configuration["MCP:SupportedAgentTypes"]),
            Status = MCPStatus.Online,
            HealthStatus = HealthStatus.Healthy
        };

        InitializeTimers();
    }

    private void InitializeTimers()
    {
        _heartbeatTimer = new Timer(async _ => await SendHeartbeatAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        _healthCheckTimer = new Timer(async _ => await CheckHealthAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public async Task<bool> RegisterWithRegistryAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_registryUrl}/api/registry/mcp", _localMCP);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("MCP registered successfully with Registry");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register MCP with Registry");
            return false;
        }
    }

    public async Task<bool> SendHeartbeatAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"{_registryUrl}/api/registry/heartbeat?id={_localMCP.Id}&isMCP=true",
                null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send heartbeat");
            return false;
        }
    }

    public async Task<MCPRegistration> GetMCPDetailsAsync(string mcpId)
    {
        var response = await _httpClient.GetAsync($"{_registryUrl}/api/registry/mcp/{mcpId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MCPRegistration>()
            ?? throw new InvalidOperationException("Failed to get MCP details");
    }

    public async Task<bool> ForwardRequestToMCPAsync(string mcpId, string requestType, object request)
    {
        var targetMCP = await GetMCPDetailsAsync(mcpId);
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{targetMCP.Url}/api/mcp/process/{requestType}",
                request);
            
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forward request to MCP {MCPId}", mcpId);
            return false;
        }
    }

    public async Task<T> SendRequestToMCPAsync<T>(string mcpId, string endpoint, HttpMethod method, object? data = null)
    {
        var targetMCP = await GetMCPDetailsAsync(mcpId);
        var request = new HttpRequestMessage(method, $"{targetMCP.Url}{endpoint}");

        if (data != null)
        {
            request.Content = JsonContent.Create(data);
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task NotifyLoadChangeAsync(int currentLoad)
    {
        try
        {
            await _httpClient.PutAsync(
                $"{_registryUrl}/api/registry/mcp/{_localMCP.Id}/load?value={currentLoad}",
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify load change");
        }
    }

    public async Task UpdateHealthStatusAsync(HealthStatus status)
    {
        try
        {
            await _httpClient.PutAsync(
                $"{_registryUrl}/api/registry/mcp/{_localMCP.Id}/health?status={status}",
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update health status");
        }
    }

    public async Task<RoutingDecision> GetRoutingDecisionAsync(string agentId, string requestType)
    {
        var response = await _httpClient.GetAsync(
            $"{_registryUrl}/api/registry/routing?agentId={agentId}&requestType={requestType}");
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoutingDecision>()
            ?? throw new InvalidOperationException("Failed to get routing decision");
    }

    private async Task CheckHealthAsync()
    {
        try
        {
            // Perform health checks
            var health = await PerformHealthChecks();
            await UpdateHealthStatusAsync(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            await UpdateHealthStatusAsync(HealthStatus.Degraded);
        }
    }

    private async Task<HealthStatus> PerformHealthChecks()
    {
        try
        {
            // Check CPU usage
            var cpuUsage = await GetCpuUsage();
            if (cpuUsage > 90) return HealthStatus.Unhealthy;
            if (cpuUsage > 70) return HealthStatus.Degraded;

            // Check memory usage
            var memoryUsage = await GetMemoryUsage();
            if (memoryUsage > 90) return HealthStatus.Unhealthy;
            if (memoryUsage > 70) return HealthStatus.Degraded;

            // Check response time
            var responseTime = await CheckResponseTime();
            if (responseTime > 5000) return HealthStatus.Unhealthy;
            if (responseTime > 2000) return HealthStatus.Degraded;

            return HealthStatus.Healthy;
        }
        catch
        {
            return HealthStatus.Unhealthy;
        }
    }

    private async Task<double> GetCpuUsage()
    {
        // In a real implementation, this would check actual CPU usage
        return Random.Shared.NextDouble() * 100;
    }

    private async Task<double> GetMemoryUsage()
    {
        // In a real implementation, this would check actual memory usage
        return Random.Shared.NextDouble() * 100;
    }

    private async Task<double> CheckResponseTime()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _httpClient.GetAsync(_localMCP.Url + "/health");
            return sw.ElapsedMilliseconds;
        }
        catch
        {
            return double.MaxValue;
        }
    }

    private Dictionary<string, string> ParseCapabilities(string? capabilities)
    {
        if (string.IsNullOrEmpty(capabilities))
            return new Dictionary<string, string>();

        return capabilities.Split(',')
            .Select(c => c.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    private List<string> ParseAgentTypes(string? agentTypes)
    {
        if (string.IsNullOrEmpty(agentTypes))
            return new List<string>();

        return agentTypes.Split(',')
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
    }
}