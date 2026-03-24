using AspireApp.BedRock.SonetOps.MCP.Services;
using AspireApp.BedRock.SonetOps.MCP.Services.Processors;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components
builder.AddServiceDefaults();

// Add services to container
builder.Services.AddMemoryCache();

// Add error monitoring
builder.Services.AddSingleton<IErrorMonitor, ErrorMonitor>();
builder.Services.AddHttpClient();

// Add AWS services
builder.Services.AddSingleton<IAWSService, AWSService>();
builder.Services.AddScoped<AwsSsoLoginCommand>();

// Add processors
builder.Services.AddScoped<CodeGenerationProcessor>();
builder.Services.AddScoped<CodeReviewProcessor>();
builder.Services.AddScoped<SecurityAnalysisProcessor>();
builder.Services.AddScoped<DependencyAnalysisProcessor>();

// Add MCP service
builder.Services.AddSingleton<IMCPService, MCPService>();

var app = builder.Build();

// Configure HTTP request pipeline
// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapDefaultEndpoints();

// Map AWS SSO login endpoint
app.MapPost("/api/aws/sso/login", async (AwsSsoLoginCommand command) =>
{
    var response = await command.ProcessAsync(new Dictionary<string, string>());
    return Results.Ok(response);
});

// Map MCP endpoints
app.MapPost("/api/process/{requestType}", async (string requestType, Dictionary<string, string> parameters, IMCPService mcpService) =>
{
    var response = await mcpService.ProcessRequestAsync(requestType, parameters);
    return Results.Ok(response);
});

app.MapGet("/api/capabilities", async (IMCPService mcpService) =>
{
    var capabilities = await mcpService.GetCapabilitiesAsync();
    return Results.Ok(capabilities);
});

app.MapGet("/health", async (IMCPService mcpService) =>
{
    var health = await mcpService.GetHealthStatusAsync();
    return Results.Ok(new { status = health.ToString() });
});

// Register with Registry service on startup
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var mcpService = scope.ServiceProvider.GetRequiredService<IMCPService>();
    await mcpService.RegisterWithRegistryAsync();
});

app.Run();