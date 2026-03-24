using System.Security.Cryptography;
using System.Text;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.ApiService.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecureConfigurationService _secureConfig;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private string? _cachedApiKey;
    
    private const string API_KEY_HEADER = "X-API-Key";
    private const string API_KEY_SECRET_NAME = "ApiKey";

    public ApiKeyMiddleware(
        RequestDelegate next,
        ISecureConfigurationService secureConfig,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _secureConfig = secureConfig;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                _logger.LogWarning("Request missing API key header");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            // Cache API key to minimize Key Vault calls
            _cachedApiKey ??= await _secureConfig.GetSecretAsync(API_KEY_SECRET_NAME);

            if (!ValidateApiKey(extractedApiKey!, _cachedApiKey))
            {
                _logger.LogWarning("Invalid API key attempt");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _next(context);
        }
        catch (SecureConfigurationException ex)
        {
            _logger.LogError(ex, "Error validating API key");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    private static bool ValidateApiKey(string providedKey, string actualKey)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey),
            Encoding.UTF8.GetBytes(actualKey));
    }
}