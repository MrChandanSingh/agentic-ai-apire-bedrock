using System.Data;
using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace AspireApp.BedRock.SonetOps.DapperORM.Tests.Repository;

[TestClass]
public class DapperRepositoryTests
{
    private Mock<IDbConnectionFactory> _connectionFactoryMock = null!;
    private Mock<ILogger<DapperRepository>> _loggerMock = null!;
    private Mock<IDbConnection> _connectionMock = null!;
    private Mock<IDbTransaction> _transactionMock = null!;
    private DapperRepository _repository = null!;
    private const string TestConnectionName = "TestConnection";

    [TestInitialize]
    public void Setup()
    {
        _connectionMock = new Mock<IDbConnection>();
        _transactionMock = new Mock<IDbTransaction>();
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _loggerMock = new Mock<ILogger<DapperRepository>>();

        _connectionFactoryMock.Setup(f => f.CreateConnectionAsync(TestConnectionName))
            .ReturnsAsync(_connectionMock.Object);

        _connectionMock.Setup(c => c.BeginTransaction())
            .Returns(_transactionMock.Object);

        _repository = new DapperRepository(
            _connectionFactoryMock.Object,
            _loggerMock.Object,
            TestConnectionName);
    }

    [TestMethod]
    public async Task QueryAsync_ValidSql_ExecutesQuery()
    {
        // Arrange
        var expectedResult = new List<TestEntity> { new TestEntity { Id = 1, Name = "Test" } };
        _connectionMock.Setup(c => c.Query<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
            .Returns(expectedResult);

        // Act
        var result = await _repository.QueryAsync<TestEntity>("SELECT * FROM TestTable");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("Test", result.First().Name);
    }

    [TestMethod]
    public async Task QueryFirstOrDefaultAsync_ValidSql_ReturnsResult()
    {
        // Arrange
        var expectedResult = new TestEntity { Id = 1, Name = "Test" };
        _connectionMock.Setup(c => c.QueryFirstOrDefault<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
            .Returns(expectedResult);

        // Act
        var result = await _repository.QueryFirstOrDefaultAsync<TestEntity>("SELECT * FROM TestTable WHERE Id = @Id", new { Id = 1 });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Test", result.Name);
    }

    [TestMethod]
    public async Task ExecuteAsync_ValidSql_ReturnsAffectedRows()
    {
        // Arrange
        const int expectedAffectedRows = 1;
        _connectionMock.Setup(c => c.Execute(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
            .Returns(expectedAffectedRows);

        // Act
        var result = await _repository.ExecuteAsync("INSERT INTO TestTable (Name) VALUES (@Name)", new { Name = "Test" });

        // Assert
        Assert.AreEqual(expectedAffectedRows, result);
    }

    [TestMethod]
    public async Task ExecuteScalarAsync_ValidSql_ReturnsScalarValue()
    {
        // Arrange
        const int expectedValue = 1;
        _connectionMock.Setup(c => c.ExecuteScalar<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
            .Returns(expectedValue);

        // Act
        var result = await _repository.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TestTable");

        // Assert
        Assert.AreEqual(expectedValue, result);
    }

    [TestMethod]
    public async Task BeginTransactionAsync_CreatesNewConnection()
    {
        // Act
        var transaction = await _repository.BeginTransactionAsync();

        // Assert
        Assert.IsNotNull(transaction);
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(TestConnectionName), Times.Once);
    }

    [TestMethod]
    public async Task Query_WithTransaction_ReusesConnection()
    {
        // Arrange
        var transaction = await _repository.BeginTransactionAsync();

        // Act
        await _repository.QueryAsync<TestEntity>("SELECT * FROM TestTable", transaction: transaction);

        // Assert
        _connectionFactoryMock.Verify(f => f.CreateConnectionAsync(TestConnectionName), Times.Once);
    }

    [TestMethod]
    public async Task Query_WithoutTransaction_ReturnsConnectionToPool()
    {
        // Act
        await _repository.QueryAsync<TestEntity>("SELECT * FROM TestTable");

        // Assert
        _connectionMock.Verify(c => c.Dispose(), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task Query_ErrorOccurs_LogsError()
    {
        // Arrange
        _connectionMock.Setup(c => c.Query<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType?>()))
            .Throws(new Exception("Test error"));

        // Act
        await _repository.QueryAsync<TestEntity>("SELECT * FROM TestTable");
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}