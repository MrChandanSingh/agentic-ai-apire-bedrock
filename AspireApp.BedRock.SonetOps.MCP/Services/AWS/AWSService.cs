using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SSO;
using Amazon.SSO.Model;

namespace AspireApp.BedRock.SonetOps.MCP.Services.AWS;

public class AWSService : IAWSService
{
    private readonly ILogger<AWSService> _logger;
    private readonly IAmazonSSO _ssoClient;
    private readonly IAmazonSecurityTokenService _stsClient;
    private readonly string _ssoStartUrl;
    private readonly string _ssoRegion;

    public AWSService(
        ILogger<AWSService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _ssoStartUrl = configuration["AWS:SSO:StartUrl"] ?? throw new ArgumentNullException("AWS:SSO:StartUrl");
        _ssoRegion = configuration["AWS:SSO:Region"] ?? "us-east-1";

        var ssoConfig = new AmazonSSOConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_ssoRegion) };
        _ssoClient = new AmazonSSOClient(ssoConfig);

        var stsConfig = new AmazonSecurityTokenServiceConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_ssoRegion) };
        _stsClient = new AmazonSecurityTokenServiceClient(stsConfig);
    }

    public async Task<bool> CheckSsoSessionAsync()
    {
        try
        {
            // Check if AWS SSO token exists and is valid
            var tokenFile = GetSsoTokenFile();
            if (!File.Exists(tokenFile))
                return false;

            var tokenContent = await File.ReadAllTextAsync(tokenFile);
            var token = System.Text.Json.JsonSerializer.Deserialize<SsoToken>(tokenContent);
            
            if (token == null || token.ExpiresAt < DateTime.UtcNow)
                return false;

            // Validate token with AWS
            var request = new GetRoleCredentialsRequest
            {
                AccessToken = token.AccessToken,
                AccountId = token.AccountId,
                RoleName = token.RoleName
            };

            var response = await _ssoClient.GetRoleCredentialsAsync(request);
            return response.RoleCredentials != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking SSO session");
            return false;
        }
    }

    public async Task<SsoLoginResult> StartSsoLoginAsync()
    {
        try
        {
            // Start device authorization
            var startAuthRequest = new StartDeviceAuthorizationRequest
            {
                StartUrl = _ssoStartUrl,
                ClientId = "aws-cli",
                ClientSecret = "aws-cli-client-secret"
            };

            var startAuthResponse = await _ssoClient.StartDeviceAuthorizationAsync(startAuthRequest);

            // Return information for browser authentication
            return new SsoLoginResult
            {
                Success = true,
                RequiresBrowserAuth = true,
                VerificationUrl = startAuthResponse.VerificationUriComplete,
                UserCode = startAuthResponse.UserCode,
                ExpiresIn = startAuthResponse.ExpiresIn
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SSO login");
            return new SsoLoginResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<SsoCredentials> GetSsoCredentialsAsync()
    {
        try
        {
            // Get stored token
            var tokenFile = GetSsoTokenFile();
            if (!File.Exists(tokenFile))
                throw new InvalidOperationException("SSO token not found");

            var tokenContent = await File.ReadAllTextAsync(tokenFile);
            var token = System.Text.Json.JsonSerializer.Deserialize<SsoToken>(tokenContent)
                ?? throw new InvalidOperationException("Invalid SSO token");

            // Get role credentials
            var request = new GetRoleCredentialsRequest
            {
                AccessToken = token.AccessToken,
                AccountId = token.AccountId,
                RoleName = token.RoleName
            };

            var response = await _ssoClient.GetRoleCredentialsAsync(request);
            var credentials = response.RoleCredentials;

            return new SsoCredentials
            {
                AccountId = token.AccountId,
                RoleName = token.RoleName,
                ExpiresAt = DateTime.FromFileTimeUtc(credentials.Expiration),
                AccessKeyId = credentials.AccessKeyId,
                SecretAccessKey = credentials.SecretAccessKey,
                SessionToken = credentials.SessionToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SSO credentials");
            throw;
        }
    }

    private string GetSsoTokenFile()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".aws", "sso", "cache", "token.json");
    }

    private class SsoToken
    {
        public string AccessToken { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}