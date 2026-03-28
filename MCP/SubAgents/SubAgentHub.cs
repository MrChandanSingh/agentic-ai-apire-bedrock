using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using MCP.Services;

namespace MCP.SubAgents
{
    public class SubAgentHub : Hub
    {
        private readonly RepositoryService _repoService;

        public SubAgentHub(RepositoryService repoService)
        {
            _repoService = repoService;
        }

        public async Task RequestChangeAnalysis(string repoId, string filePath)
        {
            var impactedRepos = await _repoService.AnalyzeChangeImpactAsync(repoId, filePath);
            await Clients.Caller.SendAsync("AnalysisResult", impactedRepos);
        }
    }
}