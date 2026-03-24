using System.Data;
using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using Dapper;
using Microsoft.Extensions.Logging;

namespace AspireApp.BedRock.SonetOps.DapperORM.Repository;

public interface IDapperRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    Task<IDbTransaction> BeginTransactionAsync();
}

public class DapperRepository : IDapperRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DapperRepository> _logger;
    private readonly string _connectionName;
    private IDbConnection? _currentConnection;

    public DapperRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<DapperRepository> logger,
        string connectionName)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _connectionName = connectionName;
    }

    public async Task<MultiResultSet> QueryMultipleAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);
            var result = new MultiResultSet();

            using var multi = await connection.QueryMultipleAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            while (!multi.IsConsumed)
            {
                try
                {
                    var dataset = await multi.ReadAsync();
                    if (dataset != null)
                    {
                        result.AddResult(dataset);
                    }
                }
                catch (Exception)
                {
                    break; // No more result sets
                }
            }

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multi-query: {Sql}", sql);
            throw;
        }
    }

    public async Task<(T1, T2)> QueryTwoAsync<T1, T2>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);

            using var multi = await connection.QueryMultipleAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            var result1 = (await multi.ReadAsync<T1>()).ToList();
            var result2 = (await multi.ReadAsync<T2>()).ToList();

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return (result1.AsEnumerable(), result2.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing two-query: {Sql}", sql);
            throw;
        }
    }

    public async Task<(T1, T2, T3)> QueryThreeAsync<T1, T2, T3>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);

            using var multi = await connection.QueryMultipleAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            var result1 = (await multi.ReadAsync<T1>()).ToList();
            var result2 = (await multi.ReadAsync<T2>()).ToList();
            var result3 = (await multi.ReadAsync<T3>()).ToList();

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return (result1.AsEnumerable(), result2.AsEnumerable(), result3.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing three-query: {Sql}", sql);
            throw;
        }
    }
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DapperRepository> _logger;
    private readonly string _connectionName;
    private IDbConnection? _currentConnection;

    public DapperRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<DapperRepository> logger,
        string connectionName)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _connectionName = connectionName;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);
            var result = await connection.QueryAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Sql}", sql);
            throw;
        }
    }

    public async Task<T> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);
            var result = await connection.QueryFirstOrDefaultAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Sql}", sql);
            throw;
        }
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);
            var result = await connection.ExecuteAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Sql}", sql);
            throw;
        }
    }

    public async Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
    {
        try
        {
            var connection = await GetConnectionAsync(transaction);
            var result = await connection.ExecuteScalarAsync<T>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType);

            if (transaction == null)
            {
                await ReturnConnectionAsync(connection);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scalar: {Sql}", sql);
            throw;
        }
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        _currentConnection = await _connectionFactory.CreateConnectionAsync(_connectionName);
        return _currentConnection.BeginTransaction();
    }

    private async Task<IDbConnection> GetConnectionAsync(IDbTransaction? transaction)
    {
        if (transaction != null)
        {
            return transaction.Connection!;
        }

        return await _connectionFactory.CreateConnectionAsync(_connectionName);
    }

    private async Task ReturnConnectionAsync(IDbConnection connection)
    {
        if (connection != _currentConnection)
        {
            await DbConnectionFactory.ReturnConnectionToPoolAsync(_connectionName, connection);
        }
    }
}