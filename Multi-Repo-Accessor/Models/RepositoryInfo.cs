namespace Multi_Repo_Accessor.Models
{
    public class RepositoryInfo
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Branch { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
    }

    public class ScanResult
    {
        public string RepoId { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
    }
}