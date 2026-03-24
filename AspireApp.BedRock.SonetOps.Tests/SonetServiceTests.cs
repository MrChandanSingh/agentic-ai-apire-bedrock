using AspireApp.BedRock.SonetOps.MCP.Services;
using AspireApp.BedRock.SonetOps.MCP.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspireApp.BedRock.SonetOps.Tests;

[TestClass]
public class SonetServiceTests
{
    private SonetService _sonetService;
    private Mock<ILogger<SonetService>> _loggerMock;
    private Mock<IBedrockService> _bedrockServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SonetService>>();
        _bedrockServiceMock = new Mock<IBedrockService>();
        _sonetService = new SonetService(_loggerMock.Object, _bedrockServiceMock.Object);
    }

    [TestMethod]
    public async Task ProcessSonetRequest_WithValidInput_ReturnsResult()
    {
        // Arrange
        var request = new SonetRequest
        {
            Id = Guid.NewGuid(),
            Content = "Test request",
            ModelId = "anthropic.claude-v2"
        };

        _bedrockServiceMock.Setup(x => x.InvokeModel(
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync("Test response");

        // Act
        var result = await _sonetService.ProcessSonetRequest(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Test response", result.Response);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessSonetRequest_WithNullRequest_ThrowsException()
    {
        // Act
        await _sonetService.ProcessSonetRequest(null);
    }

    [TestMethod]
    public async Task ProcessSonetRequest_WithInvalidModel_ReturnsError()
    {
        // Arrange
        var request = new SonetRequest
        {
            Id = Guid.NewGuid(),
            Content = "Test content",
            ModelId = "invalid-model"
        };

        _bedrockServiceMock.Setup(x => x.InvokeModel(
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ThrowsAsync(new InvalidOperationException("Invalid model"));

        // Act
        var result = await _sonetService.ProcessSonetRequest(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsTrue(result.Error.Contains("Invalid model"));
    }

    [TestMethod]
    public async Task ProcessSonetRequest_WithEmptyContent_ReturnsError()
    {
        // Arrange
        var request = new SonetRequest
        {
            Id = Guid.NewGuid(),
            Content = "",
            ModelId = "anthropic.claude-v2"
        };

        // Act
        var result = await _sonetService.ProcessSonetRequest(request);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsTrue(result.Error.Contains("empty"));
    }
}