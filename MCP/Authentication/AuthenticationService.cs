using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Octokit;
using Microsoft.TeamFoundation.Core.WebApi;

namespace MCP.Authentication
{
    public class AuthenticationService
    {
        private readonly IConfiguration _configuration;

        public AuthenticationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GitHubClient> AuthenticateGitHubAsync(string token)
        {
            var github = new GitHubClient(new ProductHeaderValue("MCP-Agent"))
            {
                Credentials = new Credentials(token)
            };
            return github;
        }

        public async Task<VssConnection> AuthenticateAzureDevOpsAsync(string pat, string organization)
        {
            var credentials = new VssBasicCredential(string.Empty, pat);
            var connection = new VssConnection(new Uri($"https://dev.azure.com/{organization}"), credentials);
            await connection.ConnectAsync();
            return connection;
        }
    }
}