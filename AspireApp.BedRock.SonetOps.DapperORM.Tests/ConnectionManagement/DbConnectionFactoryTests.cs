using System.Data;
using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.MsSql;

namespace AspireApp.BedRock.SonetOps.DapperORM.Tests.ConnectionManagement;

[TestClass]
public class DbConnectionFactoryTests
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<ILogger<DbConnectionFactory>> _loggerMock = null!;
    private DbConnectionFactory _factory = null!;
    private MsSqlContainer _sqlContainer = null!;
    private const string TestConnectionName = "TestConnection";

    [TestInitialize]
    public async Task Setup()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DbConnectionFactory>>();
        
        // Start SQL Server container
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();
        await _sqlContainer.StartAsync();

        // Configure mock connection string
        _configurationMock.Setup(c => c.GetConnectionString(TestConnectionName))
            .Returns(_sqlContainer.GetConnectionString());

        _factory = new DbConnectionFactory(_configurationMock.Object, _loggerMock.Object);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _sqlContainer.DisposeAsync();
    }

    [TestMethod]
    public async Task CreateConnection_ValidConnectionString_ReturnsOpenConnection()
    {
        // Act
        var connection = await _factory.CreateConnectionAsync(TestConnectionName);

        // Assert
        Assert.IsNotNull(connection);
        Assert.AreEqual(ConnectionState.Open, connection.State);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task CreateConnection_InvalidConnectionName_ThrowsException()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetConnectionString("InvalidConnection"))
            .Returns((string?)null);

        // Act
        await _factory.CreateConnectionAsync("InvalidConnection");
    }

    [TestMethod]
    public async Task TestConnection_ValidConnection_ReturnsTrue()
    {
        // Act
        var result = await _factory.TestConnectionAsync(TestConnectionName);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task TestConnection_InvalidConnection_ReturnsFalse()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetConnectionString("InvalidConnection"))
            .Returns("Server=invalidserver;Database=invaliddb;");

        // Act
        var result = await _factory.TestConnectionAsync("InvalidConnection");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CreateConnection_RetryOnFailure_EventuallySucceeds()
    {
        // Arrange
        var attemptCount = 0;
        _configurationMock.Setup(c => c.GetConnectionString(TestConnectionName))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    throw new SqlException();
                }
                return _sqlContainer.GetConnectionString();
            });

        // Act
        var connection = await _factory.CreateConnectionAsync(TestConnectionName);

        // Assert
        Assert.IsNotNull(connection);
        Assert.AreEqual(ConnectionState.Open, connection.State);
        Assert.AreEqual(2, attemptCount);
    }

    [TestMethod]
    public async Task ConnectionPooling_ReuseConnection_ReturnsSameConnection()
    {
        // Arrange
        var firstConnection = await _factory.CreateConnectionAsync(TestConnectionName);
        await DbConnectionFactory.ReturnConnectionToPoolAsync(TestConnectionName, firstConnection);

        // Act
        var secondConnection = await _factory.CreateConnectionAsync(TestConnectionName);

        // Assert
        Assert.AreEqual(firstConnection, secondConnection);
    }

    [TestMethod]
    public async Task ConnectionPool_LimitReached_DisposesExcessConnections()
    {
        // Arrange
        var connections = new List<IDbConnection>();
        for (int i = 0; i < 15; i++)
        {
            connections.Add(await _factory.CreateConnectionAsync(TestConnectionName));
        }

        // Act - Return all connections to pool
        foreach (var conn in connections)
        {
            await DbConnectionFactory.ReturnConnectionToPoolAsync(TestConnectionName, conn);
        }

        // Get all connections from pool
        var pooledConnections = new List<IDbConnection>();
        for (int i = 0; i < 15; i++)
        {
            try
            {
                pooledConnections.Add(await _factory.CreateConnectionAsync(TestConnectionName));
            }
            catch
            {
                // Ignore failures
            }
        }

        // Assert - Pool should be limited to 10 connections
        Assert.IsTrue(pooledConnections.Count <= 10);
    }

    [TestMethod]
    public async Task CircuitBreaker_TooManyFailures_BreaksCircuit()
    {
        // Arrange
        _configurationMock.Setup(c => c.GetConnectionString(TestConnectionName))
            .Returns("Server=invalidserver;Database=invaliddb;");

        // Act - Attempt multiple connections to trigger circuit breaker
        var exceptions = new List<Exception>();
        for (int i = 0; i < 15; i++)
        {
            try
            {
                await _factory.CreateConnectionAsync(TestConnectionName);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            await Task.Delay(100); // Small delay between attempts
        }

        // Assert
        Assert.IsTrue(exceptions.Any(e => e is SqlException));
        Assert.IsTrue(exceptions.Any(e => e.Message.Contains("Circuit"))); // Circuit breaker exception
    }

    [TestMethod]
    public async Task RetryMechanism_ProgressiveBackoff_DoesNotExceedMaxDelay()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        _configurationMock.Setup(c => c.GetConnectionString(TestConnectionName))
            .Returns("Server=invalidserver;Database=invaliddb;");

        // Act
        try
        {
            await _factory.CreateConnectionAsync(TestConnectionName);
        }
        catch
        {
            // Expected to fail
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        // With 3 retries and 1.5 power progression, total delay should be less than 10 seconds
        Assert.IsTrue(duration.TotalSeconds < 10);
    }
}