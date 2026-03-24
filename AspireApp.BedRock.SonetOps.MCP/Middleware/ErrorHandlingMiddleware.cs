using System.Net;
using System.Text.Json;
using AspireApp.BedRock.SonetOps.MCP.Exceptions;

namespace AspireApp.BedRock.SonetOps.MCP.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var error = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ProcessingException processingEx:
                error.StatusCode = (int)HttpStatusCode.BadRequest;
                error.ErrorCode = processingEx.ErrorCode;
                error.Message = processingEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["requestType"] = processingEx.RequestType,
                    ["processorType"] = processingEx.ProcessorType,
                    ["context"] = processingEx.Context
                };
                break;

            case ResourceExhaustedException resourceEx:
                error.StatusCode = (int)HttpStatusCode.TooManyRequests;
                error.ErrorCode = resourceEx.ErrorCode;
                error.Message = resourceEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["resourceType"] = resourceEx.ResourceType,
                    ["limit"] = resourceEx.Limit
                };
                break;

            case InvalidRequestException validationEx:
                error.StatusCode = (int)HttpStatusCode.BadRequest;
                error.ErrorCode = validationEx.ErrorCode;
                error.Message = validationEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["validationErrors"] = validationEx.ValidationErrors
                };
                break;

            case ProcessorNotFoundException notFoundEx:
                error.StatusCode = (int)HttpStatusCode.NotFound;
                error.ErrorCode = notFoundEx.ErrorCode;
                error.Message = notFoundEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["requestType"] = notFoundEx.RequestType
                };
                break;

            case RegistryException registryEx:
                error.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                error.ErrorCode = registryEx.ErrorCode;
                error.Message = registryEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["registryOperation"] = registryEx.RegistryOperation
                };
                break;

            case HealthCheckException healthEx:
                error.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                error.ErrorCode = healthEx.ErrorCode;
                error.Message = healthEx.Message;
                error.Details = new Dictionary<string, object>
                {
                    ["component"] = healthEx.Component,
                    ["status"] = healthEx.CurrentStatus.ToString()
                };
                break;

            default:
                error.StatusCode = (int)HttpStatusCode.InternalServerError;
                error.ErrorCode = "INTERNAL_ERROR";
                error.Message = "An unexpected error occurred";
                break;
        }

        // Add stack trace in development
        if (_env.IsDevelopment())
        {
            error.Details["stackTrace"] = exception.StackTrace;
        }

        // Log the error
        var logLevel = error.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        _logger.Log(logLevel, exception, 
            "Error {ErrorCode} ({StatusCode}): {Message}. TraceId: {TraceId}",
            error.ErrorCode,
            error.StatusCode,
            error.Message,
            error.TraceId);

        // Set response
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}

public class ErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}