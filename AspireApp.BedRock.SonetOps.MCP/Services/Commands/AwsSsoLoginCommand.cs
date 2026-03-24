using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Services.Commands;

public class AwsSsoLoginCommand : IRequestProcessor
{
    private readonly ILogger<AwsSsoLoginCommand> _logger;
    private readonly IAWSService _awsService;

    public AwsSsoLoginCommand(ILogger<AwsSsoLoginCommand> logger, IAWSService awsService)
    {
        _logger = logger;
        _awsService = awsService;
    }

    public string CommandName => "aws_sso_login";
    public string Description => "Execute AWS SSO login and handle the authentication flow";

    public async Task<ProcessingResponse> ProcessAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting AWS SSO login process");

        try
        {
            // Step 1: Check existing SSO session
            var hasValidSession = await _awsService.CheckSsoSessionAsync();
            if (hasValidSession)
            {
                return new ProcessingResponse
                {
                    Success = true,
                    Result = new Dictionary<string, object>
                    {
                        ["message"] = "Already logged in to AWS SSO",
                        ["status"] = "active_session"
                    }
                };
            }

            // Step 2: Start SSO login process
            var loginResult = await _awsService.StartSsoLoginAsync();
            if (!loginResult.Success)
            {
                throw new ProcessingException(
                    "Failed to start SSO login",
                    CommandName,
                    "aws_sso",
                    new Dictionary<string, string>
                    {
                        ["error"] = loginResult.Error ?? "Unknown error"
                    });
            }

            // Step 3: Guide user through browser authentication if needed
            if (loginResult.RequiresBrowserAuth)
            {
                return new ProcessingResponse
                {
                    Success = true,
                    Result = new Dictionary<string, object>
                    {
                        ["message"] = "Please complete authentication in your browser",
                        ["verification_url"] = loginResult.VerificationUrl,
                        ["user_code"] = loginResult.UserCode,
                        ["expires_in"] = loginResult.ExpiresIn
                    }
                };
            }

            // Step 4: Get credentials
            var credentials = await _awsService.GetSsoCredentialsAsync();

            return new ProcessingResponse
            {
                Success = true,
                Result = new Dictionary<string, object>
                {
                    ["message"] = "Successfully logged in to AWS SSO",
                    ["status"] = "logged_in",
                    ["account_id"] = credentials.AccountId,
                    ["role_name"] = credentials.RoleName,
                    ["expires_at"] = credentials.ExpiresAt
                }
            };
        }
        catch (Exception ex) when (ex is not ProcessingException)
        {
            _logger.LogError(ex, "Error during AWS SSO login");
            throw new ProcessingException(
                "AWS SSO login failed",
                CommandName,
                "aws_sso",
                new Dictionary<string, string>
                {
                    ["error_type"] = ex.GetType().Name,
                    ["error_message"] = ex.Message
                });
        }
    }
}