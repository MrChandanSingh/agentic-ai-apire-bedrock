using AspireApp.BedRock.SonetOps.ApiService.Services;
using AspireApp.BedRock.SonetOps.ApiService.Data;
using AspireApp.BedRock.SonetOps.ApiService.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add security services
builder.Services.AddScoped<ISecureConfigurationService, SecureConfigurationService>();

// Add transaction processing services
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionProcessingService, TransactionProcessingService>();
builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();
builder.Services.AddHostedService<TransactionProcessingBackgroundService>();

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Add Azure Key Vault configuration if URL is provided
if (!string.IsNullOrEmpty(builder.Configuration["KeyVault:Url"]))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration["KeyVault:Url"]),
        new DefaultAzureCredential());
}
builder.Services.AddAWSService<Amazon.BedrockRuntime.IAmazonBedrockRuntime>();
builder.Services.AddScoped<IBedrockService, BedrockService>();

// ⚠️ CRITICAL: AWS Authentication Configuration
// Ensure these credentials are stored securely in configuration and never in source code
var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(
    builder.Configuration["AWS:AccessKey"],
    builder.Configuration["AWS:SecretKey"]
);

// Register the credentials as a singleton in the DI container
builder.Services.AddSingleton<Amazon.Runtime.AWSCredentials>(awsCredentials);

// ⚠️ CRITICAL: Bedrock Runtime Configuration
// Configure regional endpoint for optimal latency and compliance
builder.Services.AddAWSService<Amazon.BedrockRuntime.IAmazonBedrockRuntime>(new Amazon.BedrockRuntime.AmazonBedrockRuntimeConfig
{
    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Add security headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Add API key authentication
app.UseMiddleware<ApiKeyMiddleware>();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
