using AspireApp.BedRock.SonetOps.Registry.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

// Configure Swagger with authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Registry Service API", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API key needed to access the endpoints"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

// Add Registry Services
builder.Services.AddSingleton<IContextManager, ContextManager>();
builder.Services.AddSingleton<IAgentDocumentationReader, AgentDocumentationReader>();
builder.Services.AddScoped<IAgentDecisionService, AgentDecisionService>();

// Add routing strategies
builder.Services.AddSingleton<IRoutingStrategy, RoundRobinStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, LeastLoadStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, GeographicProximityStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, WeightedLoadBalancingStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, CapabilityMatchingStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, FailoverRoutingStrategy>();
builder.Services.AddSingleton<IRoutingStrategy, HybridRoutingStrategy>();

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = Microsoft.AspNetCore.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(httpContext =>
        Microsoft.AspNetCore.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-API-Key"].ToString(),
            factory: _ => new Microsoft.AspNetCore.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();