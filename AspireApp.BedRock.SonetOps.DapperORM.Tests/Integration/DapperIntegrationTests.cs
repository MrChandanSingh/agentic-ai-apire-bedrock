using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace AspireApp.BedRock.SonetOps.DapperORM.Tests.Integration;

[TestClass]
public class DapperIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private MsSqlContainer _sqlContainer = null!;
    private const string TestConnectionName = "IntegrationTest";

    [TestInitialize]
    public async Task Setup()
    {
        // Start SQL Server container
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();
        
        await _sqlContainer.StartAsync();

        // Setup DI
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{TestConnectionName}"] = _sqlContainer.GetConnectionString()
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddDapperORM();
        services.AddDapperRepository(TestConnectionName);

        _serviceProvider = services.BuildServiceProvider();

        // Create test table
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        await repository.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TestTable')
            CREATE TABLE TestTable (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            )");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _sqlContainer.DisposeAsync();
    }

    [TestMethod]
    public async Task FullCrud_Operations_WorkCorrectly()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        var testName = $"Test_{Guid.NewGuid()}";

        // Act - Create
        var id = await repository.ExecuteScalarAsync<int>(
            "INSERT INTO TestTable (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()",
            new { Name = testName });

        // Act - Read
        var result = await repository.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Id = @Id",
            new { Id = id });

        // Assert - Read
        Assert.IsNotNull(result);
        Assert.AreEqual(testName, result.Name);

        // Act - Update
        var newName = $"Updated_{testName}";
        await repository.ExecuteAsync(
            "UPDATE TestTable SET Name = @Name WHERE Id = @Id",
            new { Id = id, Name = newName });

        var updatedResult = await repository.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Id = @Id",
            new { Id = id });

        // Assert - Update
        Assert.IsNotNull(updatedResult);
        Assert.AreEqual(newName, updatedResult.Name);

        // Act - Delete
        await repository.ExecuteAsync(
            "DELETE FROM TestTable WHERE Id = @Id",
            new { Id = id });

        var deletedResult = await repository.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Id = @Id",
            new { Id = id });

        // Assert - Delete
        Assert.IsNull(deletedResult);
    }

    [TestMethod]
    public async Task Transaction_Commit_PersistsChanges()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        var testName = $"Test_{Guid.NewGuid()}";

        // Act
        var transaction = await repository.BeginTransactionAsync();
        try
        {
            var id = await repository.ExecuteScalarAsync<int>(
                "INSERT INTO TestTable (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()",
                new { Name = testName },
                transaction);

            await repository.ExecuteAsync(
                "UPDATE TestTable SET Name = @Name WHERE Id = @Id",
                new { Id = id, Name = $"Updated_{testName}" },
                transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        // Assert
        var result = await repository.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Name LIKE @Name",
            new { Name = $"Updated_{testName}" });

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Transaction_Rollback_DiscardsChanges()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        var testName = $"Test_{Guid.NewGuid()}";

        // Act
        var transaction = await repository.BeginTransactionAsync();
        var id = await repository.ExecuteScalarAsync<int>(
            "INSERT INTO TestTable (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()",
            new { Name = testName },
            transaction);
        
        transaction.Rollback();

        // Assert
        var result = await repository.QueryFirstOrDefaultAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Name = @Name",
            new { Name = testName });

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ConcurrentQueries_WorkCorrectly()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        var tasks = new List<Task>();
        var results = new List<int>();
        var lock_object = new object();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var testName = $"Test_{i}_{Guid.NewGuid()}";
            tasks.Add(Task.Run(async () =>
            {
                var id = await repository.ExecuteScalarAsync<int>(
                    "INSERT INTO TestTable (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()",
                    new { Name = testName });
                lock (lock_object)
                {
                    results.Add(id);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var count = await repository.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TestTable");
        Assert.AreEqual(10, count);
        Assert.AreEqual(10, results.Distinct().Count());
    }

    [TestMethod]
    public async Task LargeDataSet_HandledEfficiently()
    {
        // Arrange
        var repository = _serviceProvider.GetRequiredService<IDapperRepository>();
        var testData = Enumerable.Range(1, 1000)
            .Select(i => $"Test_{i}_{Guid.NewGuid()}")
            .ToList();

        // Act - Insert
        foreach (var batch in testData.Chunk(100))
        {
            await repository.ExecuteAsync(
                "INSERT INTO TestTable (Name) VALUES " +
                string.Join(",", batch.Select((_, i) => $"(@Name{i})")),
                batch.Select((name, i) => new { Key = $"Name{i}", Value = name })
                    .ToDictionary(x => x.Key, x => x.Value as object));
        }

        // Act - Read
        var results = await repository.QueryAsync<TestEntity>(
            "SELECT * FROM TestTable WHERE Name LIKE 'Test_%'");

        // Assert
        Assert.AreEqual(testData.Count, results.Count());
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}