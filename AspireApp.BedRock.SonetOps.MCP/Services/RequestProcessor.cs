using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public abstract class RequestProcessor
{
    protected readonly ILogger _logger;
    protected readonly System.Diagnostics.Stopwatch _stopwatch;

    protected RequestProcessor(ILogger logger)
    {
        _logger = logger;
        _stopwatch = new System.Diagnostics.Stopwatch();
    }

    public async Task<ProcessingResponse> ProcessAsync(Dictionary<string, string> parameters)
    {
        _stopwatch.Restart();
        try
        {
            ValidateParameters(parameters);
            var result = await ProcessInternalAsync(parameters);
            return CreateSuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed");
            return CreateErrorResponse(ex.Message);
        }
        finally
        {
            _stopwatch.Stop();
        }
    }

    protected abstract Task<Dictionary<string, object>> ProcessInternalAsync(Dictionary<string, string> parameters);
    protected abstract void ValidateParameters(Dictionary<string, string> parameters);

    protected virtual ProcessingResponse CreateSuccessResponse(Dictionary<string, object> result)
    {
        return new ProcessingResponse
        {
            Success = true,
            Result = result,
            Metrics = new ProcessingMetrics
            {
                ProcessingTimeMs = _stopwatch.ElapsedMilliseconds,
                TokensUsed = CalculateTokensUsed(result),
                ModelUsed = GetModelUsed()
            }
        };
    }

    protected virtual ProcessingResponse CreateErrorResponse(string error)
    {
        return new ProcessingResponse
        {
            Success = false,
            Error = error,
            Metrics = new ProcessingMetrics
            {
                ProcessingTimeMs = _stopwatch.ElapsedMilliseconds
            }
        };
    }

    protected virtual int CalculateTokensUsed(Dictionary<string, object> result)
    {
        // Simple estimation - in a real implementation use proper token counting
        return string.Join("", result.Values).Length / 4;
    }

    protected virtual string GetModelUsed()
    {
        return "claude-3";
    }
}