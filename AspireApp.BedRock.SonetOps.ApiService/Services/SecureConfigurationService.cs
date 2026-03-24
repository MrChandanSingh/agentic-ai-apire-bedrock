using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

public interface ISecureConfigurationService
{
    Task<string> GetSecretAsync(string secretName);
}

public class SecureConfigurationService : ISecureConfigurationService
{
    private readonly SecretClient _secretClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecureConfigurationService> _logger;

    public SecureConfigurationService(IConfiguration configuration, ILogger<SecureConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var keyVaultUri = new Uri(_configuration["KeyVault:Url"] ?? throw new ArgumentNullException("KeyVault:Url"));
        _secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
            throw new SecureConfigurationException($"Failed to retrieve secret: {secretName}", ex);
        }
    }
}