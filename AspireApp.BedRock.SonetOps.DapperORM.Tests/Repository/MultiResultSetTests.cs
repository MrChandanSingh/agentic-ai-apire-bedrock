using AspireApp.BedRock.SonetOps.DapperORM.ConnectionManagement;
using AspireApp.BedRock.SonetOps.DapperORM.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace AspireApp.BedRock.SonetOps.DapperORM.Tests.Repository;

[TestClass]
public class MultiResultSetTests
{
    private IDapperRepository _repository = null!;
    private MsSqlContainer _sqlContainer = null!;

    [TestInitialize]
    public async Task Setup()
    {
        // Start SQL Server container
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("Your_password123")
            .Build();
        
        await _sqlContainer.StartAsync();

        // Configure services
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TestConnection"] = _sqlContainer.GetConnectionString()
            })
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var connectionFactory = new DbConnectionFactory(
            configuration,
            loggerFactory.CreateLogger<DbConnectionFactory>());

        _repository = new DapperRepository(
            connectionFactory,
            loggerFactory.CreateLogger<DapperRepository>(),
            "TestConnection");

        // Create test tables
        await _repository.ExecuteAsync(@"
            CREATE TABLE Users (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100)
            );

            CREATE TABLE Orders (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT,
                Amount DECIMAL(18,2)
            );

            CREATE TABLE OrderItems (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                OrderId INT,
                ProductName NVARCHAR(100),
                Quantity INT
            );

            -- Insert test data
            INSERT INTO Users (Name) VALUES ('John'), ('Jane');
            
            INSERT INTO Orders (UserId, Amount) 
            VALUES (1, 100.00), (1, 200.00), (2, 150.00);
            
            INSERT INTO OrderItems (OrderId, ProductName, Quantity)
            VALUES (1, 'Item 1', 2), (1, 'Item 2', 1),
                   (2, 'Item 3', 3), (3, 'Item 1', 1);");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _sqlContainer.DisposeAsync();
    }

    [TestMethod]
    public async Task QueryMultipleAsync_ReturnsAllResultSets()
    {
        // Arrange
        var sql = @"
            SELECT * FROM Users WHERE Id = 1;
            SELECT * FROM Orders WHERE UserId = 1;
            SELECT i.* 
            FROM OrderItems i
            JOIN Orders o ON o.Id = i.OrderId
            WHERE o.UserId = 1;";

        // Act
        var results = await _repository.QueryMultipleAsync(sql);

        // Assert
        Assert.AreEqual(3, results.ResultCount);
        
        var user = results.GetResult<User>(0).Single();
        var orders = results.GetResult<Order>(1).ToList();
        var items = results.GetResult<OrderItem>(2).ToList();

        Assert.AreEqual("John", user.Name);
        Assert.AreEqual(2, orders.Count);
        Assert.AreEqual(3, items.Count);
    }

    [TestMethod]
    public async Task QueryTwoAsync_ReturnsTwoResultSets()
    {
        // Arrange
        var sql = @"
            SELECT * FROM Users WHERE Id = 1;
            SELECT * FROM Orders WHERE UserId = 1;";

        // Act
        var (user, orders) = await _repository.QueryTwoAsync<User, Order>(sql);

        // Assert
        Assert.AreEqual(1, user.Count());
        Assert.AreEqual(2, orders.Count());
        Assert.AreEqual("John", user.First().Name);
    }

    [TestMethod]
    public async Task QueryThreeAsync_ReturnsThreeResultSets()
    {
        // Arrange
        var sql = @"
            SELECT * FROM Users WHERE Id = 1;
            SELECT * FROM Orders WHERE UserId = 1;
            SELECT i.* 
            FROM OrderItems i
            JOIN Orders o ON o.Id = i.OrderId
            WHERE o.UserId = 1;";

        // Act
        var (user, orders, items) = await _repository.QueryThreeAsync<User, Order, OrderItem>(sql);

        // Assert
        Assert.AreEqual(1, user.Count());
        Assert.AreEqual(2, orders.Count());
        Assert.AreEqual(3, items.Count());
    }

    [TestMethod]
    public async Task QueryMultipleAsync_WithTransaction_CommitsAllChanges()
    {
        // Arrange
        var transaction = await _repository.BeginTransactionAsync();
        try
        {
            var sql = @"
                INSERT INTO Users (Name) VALUES ('Alice');
                SELECT SCOPE_IDENTITY() as Id;
                
                INSERT INTO Orders (UserId, Amount) 
                VALUES (SCOPE_IDENTITY(), 300.00);
                SELECT SCOPE_IDENTITY() as Id;
                
                INSERT INTO OrderItems (OrderId, ProductName, Quantity)
                VALUES (SCOPE_IDENTITY(), 'New Item', 5);
                SELECT * FROM OrderItems WHERE OrderId = SCOPE_IDENTITY();";

            // Act
            var results = await _repository.QueryMultipleAsync(sql, transaction: transaction);
            transaction.Commit();

            // Assert
            var userId = results.GetResult<IdResult>(0).First().Id;
            var orderId = results.GetResult<IdResult>(1).First().Id;
            var orderItem = results.GetResult<OrderItem>(2).First();

            Assert.AreEqual("New Item", orderItem.ProductName);
            Assert.AreEqual(5, orderItem.Quantity);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    [TestMethod]
    public async Task QueryMultipleAsync_WithLargeDataSets_HandlesEfficiently()
    {
        // Arrange
        await _repository.ExecuteAsync(@"
            DECLARE @i INT = 0;
            WHILE @i < 1000
            BEGIN
                INSERT INTO Users (Name) VALUES (CONCAT('User', @i));
                INSERT INTO Orders (UserId, Amount) VALUES (SCOPE_IDENTITY(), @i * 10);
                INSERT INTO OrderItems (OrderId, ProductName, Quantity)
                VALUES (SCOPE_IDENTITY(), CONCAT('Product', @i), @i % 10 + 1);
                SET @i = @i + 1;
            END");

        var sql = @"
            SELECT * FROM Users;
            SELECT * FROM Orders;
            SELECT * FROM OrderItems;";

        // Act
        var results = await _repository.QueryMultipleAsync(sql);

        // Assert
        var users = results.GetResult<User>(0).ToList();
        var orders = results.GetResult<Order>(1).ToList();
        var items = results.GetResult<OrderItem>(2).ToList();

        Assert.AreEqual(1002, users.Count); // 1000 + 2 from setup
        Assert.AreEqual(1003, orders.Count); // 1000 + 3 from setup
        Assert.AreEqual(1004, items.Count);  // 1000 + 4 from setup
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }

    private class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    private class IdResult
    {
        public int Id { get; set; }
    }
}