namespace AspireApp.BedRock.SonetOps.MCP.Services.AWS;

public interface IAWSService
{
    Task<bool> CheckSsoSessionAsync();
    Task<SsoLoginResult> StartSsoLoginAsync();
    Task<SsoCredentials> GetSsoCredentialsAsync();
}

public class SsoLoginResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool RequiresBrowserAuth { get; set; }
    public string? VerificationUrl { get; set; }
    public string? UserCode { get; set; }
    public int ExpiresIn { get; set; }
}

public class SsoCredentials
{
    public string AccountId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
}