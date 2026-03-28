using System.Collections.Generic;
using System.Threading.Tasks;
using MCP.Models;

namespace MCP.Services
{
    public class RepositoryService
    {
        private readonly Dictionary<string, RepositoryInfo> _repositories = new();

        public async Task RegisterRepositoryAsync(string repoId, RepositoryInfo info)
        {
            _repositories[repoId] = info;
        }

        public async Task<RepositoryInfo> GetRepositoryAsync(string repoId)
        {
            return _repositories.TryGetValue(repoId, out var repo) ? repo : null;
        }

        public async Task<IEnumerable<string>> AnalyzeChangeImpactAsync(string sourceRepoId, string changedFile)
        {
            var impactedRepos = new List<string>();
            foreach (var repo in _repositories)
            {
                if (await HasDependencyOn(repo.Value, sourceRepoId, changedFile))
                {
                    impactedRepos.Add(repo.Key);
                }
            }
            return impactedRepos;
        }

        private async Task<bool> HasDependencyOn(RepositoryInfo repo, string sourceRepoId, string changedFile)
        {
            // Implement dependency analysis logic
            return false;
        }
    }
}