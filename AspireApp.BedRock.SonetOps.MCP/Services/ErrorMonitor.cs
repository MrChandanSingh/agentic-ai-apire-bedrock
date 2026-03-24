namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IErrorMonitor
{
    void TrackError(Exception exception, Dictionary<string, string> context);
    void TrackRetry(string operation, int attemptNumber, TimeSpan delay);
    void TrackCircuitBreakerStateChange(string operation, string newState);
}

public class ErrorMonitor : IErrorMonitor
{
    private readonly ILogger<ErrorMonitor> _logger;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, List<ErrorRecord>> _errorHistory = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ErrorMonitor(ILogger<ErrorMonitor> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public void TrackError(Exception exception, Dictionary<string, string> context)
    {
        var record = new ErrorRecord
        {
            Timestamp = DateTime.UtcNow,
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            Context = context
        };

        Task.Run(async () => await StoreErrorRecordAsync(record));

        // Check for error patterns
        Task.Run(async () => await AnalyzeErrorPatternsAsync(record));
    }

    public void TrackRetry(string operation, int attemptNumber, TimeSpan delay)
    {
        var key = $"retry_{operation}";
        var retries = _cache.GetOrCreate(key, _ => new List<RetryRecord>());

        retries.Add(new RetryRecord
        {
            Timestamp = DateTime.UtcNow,
            AttemptNumber = attemptNumber,
            Delay = delay
        });

        // Keep only last hour of retry data
        retries = retries
            .Where(r => r.Timestamp > DateTime.UtcNow.AddHours(-1))
            .ToList();

        _cache.Set(key, retries);

        // Alert if too many retries
        if (retries.Count > 10)
        {
            _logger.LogWarning(
                "High number of retries for operation {Operation}: {Count} in the last hour",
                operation,
                retries.Count);
        }
    }

    public void TrackCircuitBreakerStateChange(string operation, string newState)
    {
        var key = $"circuit_{operation}";
        var states = _cache.GetOrCreate(key, _ => new List<CircuitBreakerStateChange>());

        states.Add(new CircuitBreakerStateChange
        {
            Timestamp = DateTime.UtcNow,
            NewState = newState
        });

        // Keep only last day of state changes
        states = states
            .Where(s => s.Timestamp > DateTime.UtcNow.AddDays(-1))
            .ToList();

        _cache.Set(key, states);

        // Log state change
        _logger.LogInformation(
            "Circuit breaker for {Operation} changed to {State}",
            operation,
            newState);
    }

    private async Task StoreErrorRecordAsync(ErrorRecord record)
    {
        await _lock.WaitAsync();
        try
        {
            var errorType = record.ExceptionType;
            if (!_errorHistory.ContainsKey(errorType))
            {
                _errorHistory[errorType] = new List<ErrorRecord>();
            }

            _errorHistory[errorType].Add(record);

            // Keep only last 24 hours of errors
            _errorHistory[errorType] = _errorHistory[errorType]
                .Where(e => e.Timestamp > DateTime.UtcNow.AddDays(-1))
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task AnalyzeErrorPatternsAsync(ErrorRecord record)
    {
        await _lock.WaitAsync();
        try
        {
            var errorType = record.ExceptionType;
            if (_errorHistory.TryGetValue(errorType, out var errors))
            {
                // Check for error spikes
                var recentErrors = errors.Count(e => e.Timestamp > DateTime.UtcNow.AddMinutes(-5));
                if (recentErrors > 10)
                {
                    _logger.LogError(
                        "Error spike detected: {Count} {ErrorType} errors in last 5 minutes",
                        recentErrors,
                        errorType);
                }

                // Check for patterns in context
                var commonContextKeys = errors
                    .SelectMany(e => e.Context.Keys)
                    .GroupBy(k => k)
                    .Where(g => g.Count() > errors.Count / 2)
                    .Select(g => g.Key)
                    .ToList();

                if (commonContextKeys.Any())
                {
                    _logger.LogWarning(
                        "Common context detected in {ErrorType} errors: {Keys}",
                        errorType,
                        string.Join(", ", commonContextKeys));
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private class ErrorRecord
    {
        public DateTime Timestamp { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> Context { get; set; } = new();
    }

    private class RetryRecord
    {
        public DateTime Timestamp { get; set; }
        public int AttemptNumber { get; set; }
        public TimeSpan Delay { get; set; }
    }

    private class CircuitBreakerStateChange
    {
        public DateTime Timestamp { get; set; }
        public string NewState { get; set; } = string.Empty;
    }
}