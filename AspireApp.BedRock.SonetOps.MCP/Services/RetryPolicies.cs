using AspireApp.BedRock.SonetOps.MCP.Exceptions;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public static class RetryPolicies
{
    public static AsyncRetryPolicy<T> CreateProcessingPolicy<T>(ILogger logger)
    {
        return Policy<T>
            .Handle<ProcessingException>()
            .Or<ResourceExhaustedException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 8)), // Max 8 seconds
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Retry {RetryCount} after {Delay}ms. Error: {Error}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                }
            );
    }

    public static AsyncCircuitBreakerPolicy<T> CreateCircuitBreakerPolicy<T>(ILogger logger)
    {
        return Policy<T>
            .Handle<MCPException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Circuit breaker opened for {Duration}ms. Error: {Error}",
                        duration.TotalMilliseconds,
                        exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open");
                }
            );
    }

    public static AsyncRetryPolicy<T> CreateRegistryPolicy<T>(ILogger logger)
    {
        return Policy<T>
            .Handle<RegistryException>()
            .WaitAndRetryAsync(
                5, // More retries for registry operations
                retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(1.5, retryAttempt), 60)), // Max 60 seconds
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Registry operation retry {RetryCount} after {Delay}ms. Error: {Error}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                }
            );
    }

    public static AsyncRetryPolicy CreateHealthCheckPolicy(ILogger logger)
    {
        return Policy
            .Handle<HealthCheckException>()
            .WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 300)), // Max 5 minutes
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Health check retry {RetryCount} after {Delay}ms. Component: {Component}, Status: {Status}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        ((HealthCheckException)exception).Component,
                        ((HealthCheckException)exception).CurrentStatus);
                }
            );
    }

    public static AsyncRetryPolicy<T> CreateResourcePolicy<T>(ILogger logger)
    {
        return Policy<T>
            .Handle<ResourceExhaustedException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(retryAttempt * 2), // Linear backoff
                (exception, timeSpan, retryCount, context) =>
                {
                    var resourceEx = (ResourceExhaustedException)exception;
                    logger.LogWarning(
                        "Resource {Resource} exceeded limit of {Limit}. Retry {RetryCount} after {Delay}ms",
                        resourceEx.ResourceType,
                        resourceEx.Limit,
                        retryCount,
                        timeSpan.TotalMilliseconds);
                }
            );
    }

    public static IAsyncPolicy<T> CreateCombinedPolicy<T>(ILogger logger)
    {
        return Policy<T>
            .WrapAsync(
                CreateCircuitBreakerPolicy<T>(logger),
                CreateProcessingPolicy<T>(logger),
                CreateRegistryPolicy<T>(logger),
                CreateResourcePolicy<T>(logger)
            );
    }
}