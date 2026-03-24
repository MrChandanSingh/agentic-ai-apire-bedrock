var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server for MCP
var sql = builder.AddSqlServer("sql", "Password123!")
    .AddDatabase("SonetMCP");

// Add Registry service
var registryService = builder.AddProject<Projects.AspireApp_BedRock_SonetOps_Registry>("registry")
    .WithReference(sql)
    .WithHttpHealthCheck("/health");

// Add MCP service
var mcpService = builder.AddProject<Projects.AspireApp_BedRock_SonetOps_MCP>("mcpservice")
    .WithReference(sql)
    .WithReference(registryService)
    .WithHttpHealthCheck("/health");

// Add Routing service
var routingService = builder.AddProject<Projects.AspireApp_BedRock_SonetOps_RoutingService>("routing")
    .WithReference(registryService)
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.AspireApp_BedRock_SonetOps_ApiService>("apiservice")
    .WithReference(mcpService)
    .WithReference(routingService)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireApp_BedRock_SonetOps_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();