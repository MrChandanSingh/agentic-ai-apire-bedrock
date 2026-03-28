using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Multi_Repo_Accessor.Services;
using Multi_Repo_Accessor.MCP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRepositoryScanner, RepositoryScanner>();
builder.Services.AddSingleton<IMcpClient, McpClient>();
builder.Services.AddHostedService<McpConnectionService>();

var app = builder.Build();

app.MapGet("/scan/{repoId}", async (string repoId, IRepositoryScanner scanner) => 
    await scanner.ScanRepository(repoId));

app.Run();