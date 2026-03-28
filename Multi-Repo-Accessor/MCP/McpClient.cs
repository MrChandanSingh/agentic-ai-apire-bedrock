using Microsoft.AspNetCore.SignalR.Client;
using Multi_Repo_Accessor.Models;

namespace Multi_Repo_Accessor.MCP
{
    public interface IMcpClient
    {
        Task<RepositoryInfo> GetRepositoryInfo(string repoId);
        Task NotifyChanges(ScanResult result);
    }

    public class McpClient : IMcpClient
    {
        private readonly HubConnection _connection;

        public McpClient(IConfiguration configuration)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(configuration["McpHub"])
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task<RepositoryInfo> GetRepositoryInfo(string repoId)
        {
            return await _connection.InvokeAsync<RepositoryInfo>("GetRepositoryInfo", repoId);
        }

        public async Task NotifyChanges(ScanResult result)
        {
            await _connection.InvokeAsync("NotifyChanges", result);
        }
    }
}