using AspireApp.BedRock.SonetOps.MCP.Services;
using AspireApp.BedRock.SonetOps.MCP.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.BedRock.SonetOps.Tests;

[TestClass]
public class MCPServiceTests
{
    private MCPService _mcpService;
    private Mock<ILogger<MCPService>> _loggerMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<MCPService>>();
        _mcpService = new MCPService(_loggerMock.Object);
    }

    [TestMethod]
    public async Task ProcessInstruction_WithValidInput_ReturnsResult()
    {
        // Arrange
        var instruction = new Instruction
        {
            Id = Guid.NewGuid(),
            Content = "Test instruction",
            Type = "test"
        };

        // Act
        var result = await _mcpService.ProcessInstruction(instruction);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Processing instruction")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessInstruction_WithNullInstruction_ThrowsException()
    {
        // Act
        await _mcpService.ProcessInstruction(null);
    }

    [TestMethod]
    public async Task ProcessInstruction_WithEmptyContent_ReturnsError()
    {
        // Arrange
        var instruction = new Instruction
        {
            Id = Guid.NewGuid(),
            Content = "",
            Type = "test"
        };

        // Act
        var result = await _mcpService.ProcessInstruction(instruction);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Empty content")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}