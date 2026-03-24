using AspireApp.BedRock.SonetOps.ApiService.Services;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;

namespace AspireApp.BedRock.SonetOps.Tests;

[TestClass]
public class BedrockServiceTests
{
    private BedrockService _bedrockService;
    private IConfiguration _configuration;
    private Mock<IAmazonBedrockRuntime> _bedrockClientMock;

    [TestInitialize]
    public void Setup()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"AWS:Region", "us-west-2"},
            {"AWS:AccessKey", "test-key"},
            {"AWS:SecretKey", "test-secret"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
            
        _bedrockClientMock = new Mock<IAmazonBedrockRuntime>();
        _bedrockService = new BedrockService(_configuration);
    }

    [TestMethod]
    public async Task InvokeModel_WithValidInput_ReturnsResponse()
    {
        // Arrange
        var modelId = "anthropic.claude-v2";
        var prompt = "Hello, how are you?";
        
        var mockResponse = new InvokeModelResponse
        {
            Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"completion\":\"I'm doing well, thank you!\"}")),
            ContentType = "application/json",
            ResponseMetadata = new Amazon.Runtime.ResponseMetadata { RequestId = "test-request" }
        };

        _bedrockClientMock.Setup(x => x.InvokeModelAsync(It.IsAny<InvokeModelRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse);

        // Act
        var response = await _bedrockService.InvokeModel(modelId, prompt);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Contains("I'm doing well"));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task InvokeModel_WithNullModelId_ThrowsException()
    {
        // Arrange
        string modelId = null;
        var prompt = "Hello";

        // Act
        await _bedrockService.InvokeModel(modelId, prompt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task InvokeModel_WithNullPrompt_ThrowsException()
    {
        // Arrange
        var modelId = "anthropic.claude-v2";
        string prompt = null;

        // Act
        await _bedrockService.InvokeModel(modelId, prompt);
    }
}