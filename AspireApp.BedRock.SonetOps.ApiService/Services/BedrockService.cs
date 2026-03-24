using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AspireApp.BedRock.SonetOps.ApiService.Services;

/// <summary>
/// Interface for interacting with Amazon Bedrock's Claude AI model.
/// Provides methods for sending prompts and receiving AI-generated responses.
/// </summary>
public interface IBedrockService
{
    /// <summary>
    /// Asynchronously sends a prompt to Claude and receives the generated response.
    /// </summary>
    /// <param name="prompt">The user's input prompt to send to Claude</param>
    /// <returns>The AI-generated response as a string</returns>
    Task<string> InvokeClaudeAsync(string prompt);
}

public class BedrockService : IBedrockService
{
    // ⚠️ CRITICAL: Core service dependencies
    private readonly IAmazonBedrockRuntime _bedrockClient;
    private readonly string _modelId;
    
    // ⚠️ CRITICAL: Default model configuration
    // Change this if you need to use a different Claude model version
    private const string DefaultModelId = "apac.anthropic.claude-3-5-sonnet-20241022-v2:0";

    public BedrockService(IAmazonBedrockRuntime bedrockClient, IConfiguration configuration)
    {
        _bedrockClient = bedrockClient;
        _modelId = configuration.GetValue<string>("Bedrock:ModelId") ?? DefaultModelId;
    }

    /// <summary>
    /// Sends a prompt to Amazon Bedrock's Claude model and retrieves the generated response.
    /// Uses configured model settings and handles JSON serialization/deserialization.
    /// </summary>
    /// <param name="prompt">The user's input prompt to send to Claude</param>
    /// <returns>The AI-generated text completion</returns>
    /// <exception cref="JsonException">Thrown when response parsing fails</exception>
    /// <exception cref="AmazonBedrockRuntimeException">Thrown when the API call fails</exception>
    /// <remarks>
    /// ⚠️ CRITICAL SECTION - Model Invocation
    /// - Handles core interaction with Amazon Bedrock API
    /// - Uses default model settings: max_tokens=2048, temperature=0.7
    /// - Response parsing assumes Claude JSON response format
    /// </remarks>
    public async Task<string> InvokeClaudeAsync(string prompt)
    {
        var request = new InvokeModelRequest
        {
            ModelId = _modelId,
            Body = JsonSerializer.SerializeToUtf8Bytes(new
            {
                prompt = $"\\n\\nHuman: {prompt}\\n\\nAssistant:",
                max_tokens = 2048,
                temperature = 0.7
            }),
            ContentType = "application/json"
        };

        var response = await _bedrockClient.InvokeModelAsync(request);
        using var reader = new StreamReader(response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var responseJson = JsonSerializer.Deserialize<JsonDocument>(responseBody);
        return responseJson.RootElement.GetProperty("completion").GetString();
    }
}