using System.Security.Cryptography;
using System.Text;
using AspireApp.BedRock.SonetOps.ApiService.Services;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.ApiService.Middleware;

public class MobileAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecureConfigurationService _secureConfig;
    private readonly ILogger<MobileAuthMiddleware> _logger;
    private string? _cachedJwtSecret;
    
    private const string AUTH_HEADER = "Authorization";
    private const string JWT_SECRET_NAME = "JwtSecret";
    private const string BEARER_PREFIX = "Bearer ";

    public MobileAuthMiddleware(
        RequestDelegate next,
        ISecureConfigurationService secureConfig,
        ILogger<MobileAuthMiddleware> logger)
    {
        _next = next;
        _secureConfig = secureConfig;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var authHeader = context.Request.Headers[AUTH_HEADER].ToString();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith(BEARER_PREFIX))
            {
                _logger.LogWarning("Request missing or invalid Authorization header");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Bearer token is missing or invalid");
                return;
            }

            var token = authHeader.Substring(BEARER_PREFIX.Length);

            // Cache JWT secret to minimize Key Vault calls
            _cachedJwtSecret ??= await _secureConfig.GetSecretAsync(JWT_SECRET_NAME);

            if (!ValidateToken(token, _cachedJwtSecret))
            {
                _logger.LogWarning("Invalid JWT token attempt");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Bearer token");
                return;
            }

            await _next(context);
        }
        catch (SecureConfigurationException ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    private static bool ValidateToken(string token, string secret)
    {
        try
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            
            tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}