namespace MCP.Models
{
    public class RepositoryInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Platform { get; set; }
        public string AccessToken { get; set; }
        public Dictionary<string, string> Dependencies { get; set; } = new();
    }
}