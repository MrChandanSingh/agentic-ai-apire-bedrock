using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(string connectionName);
    Task<bool> TestConnectionAsync(string connectionName);
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbConnectionFactory> _logger;
    private readonly AsyncRetryPolicy<IDbConnection> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<IDbConnection> _circuitBreaker;
    
    // Connection pool
    private static readonly Dictionary<string, Stack<IDbConnection>> _connectionPool = new();
    private static readonly SemaphoreSlim _poolLock = new(1, 1);
    
    // Circuit breaker state tracking
    private static readonly Dictionary<string, int> _failureCount = new();
    private static readonly Dictionary<string, DateTime> _lastFailureTime = new();
    private const int MaxFailuresBeforeBreak = 3;
    private static readonly TimeSpan BreakDuration = TimeSpan.FromMinutes(1);

    public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Configure retry policy
        _retryPolicy = Policy<IDbConnection>
            .Handle<SqlException>(ex => 
                ex.Number != 1205 && // Deadlock
                ex.Number != 1222)   // Lock request timeout
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)), // Progressive delay but not exponential
                OnRetry
            );

        // Configure circuit breaker
        _circuitBreaker = Policy<IDbConnection>
            .Handle<SqlException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5, // 50% failure threshold
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: 10,
                durationOfBreak: TimeSpan.FromSeconds(60),
                OnCircuitBreak,
                OnCircuitReset,
                OnCircuitHalfOpen
            );
    }

    public async Task<IDbConnection> CreateConnectionAsync(string connectionName)
    {
        // Try to get connection from pool
        var connection = await GetConnectionFromPoolAsync(connectionName);
        if (connection != null)
        {
            return connection;
        }

        // Create new connection with retry and circuit breaker
        return await _circuitBreaker.WrapAsync(_retryPolicy)
            .ExecuteAsync(async () =>
            {
                var connectionString = _configuration.GetConnectionString(connectionName) 
                    ?? throw new ArgumentException($"Connection string '{connectionName}' not found.");

                var conn = new SqlConnection(connectionString);
                
                try
                {
                    await conn.OpenAsync();
                    ResetFailureCount(connectionName);
                    return conn;
                }
                catch (SqlException ex)
                {
                    IncrementFailureCount(connectionName);
                    _logger.LogError(ex, "Error opening connection to {ConnectionName}", connectionName);
                    throw;
                }
            });
    }

    public async Task<bool> TestConnectionAsync(string connectionName)
    {
        try
        {
            using var connection = await CreateConnectionAsync(connectionName);
            return connection.State == ConnectionState.Open;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed for {ConnectionName}", connectionName);
            return false;
        }
    }

    private async Task<IDbConnection?> GetConnectionFromPoolAsync(string connectionName)
    {
        await _poolLock.WaitAsync();
        try
        {
            if (_connectionPool.TryGetValue(connectionName, out var pool) && pool.Count > 0)
            {
                var connection = pool.Pop();
                if (connection.State != ConnectionState.Open)
                {
                    try
                    {
                        await ((SqlConnection)connection).OpenAsync();
                    }
                    catch
                    {
                        connection.Dispose();
                        return null;
                    }
                }
                return connection;
            }
        }
        finally
        {
            _poolLock.Release();
        }
        return null;
    }

    public static async Task ReturnConnectionToPoolAsync(string connectionName, IDbConnection connection)
    {
        await _poolLock.WaitAsync();
        try
        {
            if (!_connectionPool.ContainsKey(connectionName))
            {
                _connectionPool[connectionName] = new Stack<IDbConnection>();
            }
            
            // Limit pool size
            if (_connectionPool[connectionName].Count < 10)
            {
                _connectionPool[connectionName].Push(connection);
            }
            else
            {
                connection.Dispose();
            }
        }
        finally
        {
            _poolLock.Release();
        }
    }

    private void IncrementFailureCount(string connectionName)
    {
        if (!_failureCount.ContainsKey(connectionName))
        {
            _failureCount[connectionName] = 0;
        }
        _failureCount[connectionName]++;
        _lastFailureTime[connectionName] = DateTime.UtcNow;
    }

    private void ResetFailureCount(string connectionName)
    {
        if (_failureCount.ContainsKey(connectionName))
        {
            _failureCount[connectionName] = 0;
        }
    }

    private void OnRetry(Exception exception, TimeSpan duration, int retryCount, Context context)
    {
        _logger.LogWarning(
            exception,
            "Retry {RetryCount} after {DurationMs}ms due to {ExceptionType}: {ExceptionMessage}",
            retryCount,
            duration.TotalMilliseconds,
            exception.GetType().Name,
            exception.Message);
    }

    private void OnCircuitBreak(Exception exception, TimeSpan duration, Context context)
    {
        _logger.LogError(
            exception,
            "Circuit breaker opened for {DurationMs}ms due to {ExceptionType}: {ExceptionMessage}",
            duration.TotalMilliseconds,
            exception.GetType().Name,
            exception.Message);
    }

    private void OnCircuitReset(Context context)
    {
        _logger.LogInformation("Circuit breaker reset");
    }

    private void OnCircuitHalfOpen(Context context)
    {
        _logger.LogInformation("Circuit breaker half-open");
    }
}