using System.Text.Json;
using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface ISonetService
{
    Task<SonetResponse> ProcessRequestAsync(SonetRequest request);
    Task<SonetResponse> GetModelResponseAsync(string modelInput);
}

public class SonetService : ISonetService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SonetService> _logger;
    private readonly IConfiguration _configuration;

    public SonetService(
        IHttpClientFactory httpClientFactory,
        ILogger<SonetService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SonetResponse> ProcessRequestAsync(SonetRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SonetAPI");
            
            var response = await client.PostAsJsonAsync("process", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<SonetResponse>();
            return result ?? throw new InvalidOperationException("Null response from Sonet API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Sonet request");
            throw new SonetServiceException("Failed to process Sonet request", ex);
        }
    }

    public async Task<SonetResponse> GetModelResponseAsync(string modelInput)
    {
        try
        {
            var request = new SonetRequest
            {
                ModelId = _configuration["Sonet:ModelId"] ?? throw new InvalidOperationException("Sonet ModelId not configured"),
                Input = modelInput,
                Parameters = new Dictionary<string, object>
                {
                    { "temperature", 0.7 },
                    { "max_tokens", 1000 }
                }
            };

            return await ProcessRequestAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model response for input: {Input}", modelInput);
            throw new SonetServiceException("Failed to get model response", ex);
        }
    }
}

public class SonetServiceException : Exception
{
    public SonetServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}