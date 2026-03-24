using AspireApp.BedRock.SonetOps.MCP.Hubs;
using AspireApp.BedRock.SonetOps.MCP.Models;
using Microsoft.AspNetCore.SignalR;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IUINotificationService
{
    Task NotifyProcessingStartedAsync(int instructionId);
    Task NotifyProcessingCompletedAsync(int instructionId, string result);
    Task NotifyProcessingFailedAsync(int instructionId, string error);
    Task NotifySonetResponseAsync(SonetResponse response);
}

public class UINotificationService : IUINotificationService
{
    private readonly IHubContext<ResponseHub> _hubContext;
    private readonly ILogger<UINotificationService> _logger;

    public UINotificationService(
        IHubContext<ResponseHub> hubContext,
        ILogger<UINotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyProcessingStartedAsync(int instructionId)
    {
        var response = UIResponse.CreateInteractive(
            $"Started processing instruction {instructionId}",
            ResponseFormat.Text,
            interactions => {
                interactions.EnableCopy = false;
                interactions.Behavior.AutoScroll = true;
                interactions.Animation = new InteractionAnimation
                {
                    Enabled = true,
                    EntryAnimation = "slideIn",
                    Duration = 500,
                    CustomAnimations = new()
                    {
                        { "processing", new AnimationPreset 
                            {
                                Name = "processing",
                                KeyframesIn = "pulse",
                                Duration = 1000,
                                TimingFunction = "ease-in-out"
                            }
                        }
                    }
                };
                interactions.CustomActions.Add(new CustomAction
                {
                    Id = "cancel_processing",
                    Label = "Cancel Processing",
                    Icon = "stop-circle",
                    Shortcut = "Esc",
                    Payload = new() { { "instructionId", instructionId } }
                });
            },
            theme => {
                theme.Colors.Primary = "#1976d2";
                theme.Colors.Background = "#f5f5f5";
                theme.Typography.Variants["status"] = new FontSettings
                {
                    Weight = "600",
                    Size = "1.1rem",
                    LineHeight = "1.4"
                };
            },
            layout => {
                layout.Layout = "flex";
                layout.Breakpoints = new()
                {
                    { "sm", new BreakpointOptions 
                        { 
                            MaxWidth = 640,
                            Layout = "compact",
                            Styles = new() 
                            { 
                                { "padding", "0.5rem" },
                                { "margin", "0.25rem" }
                            }
                        }
                    }
                };
            })
        {
            Type = "processing_started",
            Status = ResponseStatus.Processing,
            Metadata = new Dictionary<string, object>
            {
                { "instruction_id", instructionId }
            }
        };

        await SendNotificationAsync(response);
    }

    public async Task NotifyProcessingCompletedAsync(int instructionId, string result)
    {
        var response = new UIResponse
        {
            Type = "processing_completed",
            Content = result,
            Timestamp = DateTime.UtcNow,
            Status = ResponseStatus.Success,
            Format = DetectResponseFormat(result),
            Metadata = new Dictionary<string, object>
            {
                { "instruction_id", instructionId }
            },
            Actions = new List<ResponseAction>
            {
                new()
                {
                    Type = "copy",
                    Label = "Copy Response",
                    Value = result
                },
                new()
                {
                    Type = "retry",
                    Label = "Run Again",
                    Value = instructionId.ToString()
                }
            },
            Styling = new ResponseStyling
            {
                Theme = "light",
                Accent = "success",
                HighlightCode = true,
                Language = DetectCodeLanguage(result)
            }
        };

        await SendNotificationAsync(response);
    }

    public async Task NotifyProcessingFailedAsync(int instructionId, string error)
    {
        var response = new UIResponse
        {
            Type = "processing_failed",
            Content = "Processing failed",
            Error = error,
            Status = ResponseStatus.Error,
            Format = ResponseFormat.Text,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "instruction_id", instructionId }
            },
            Actions = new List<ResponseAction>
            {
                new()
                {
                    Type = "retry",
                    Label = "Retry",
                    Value = instructionId.ToString()
                },
                new()
                {
                    Type = "report",
                    Label = "Report Issue",
                    Value = $"error_{instructionId}"
                }
            },
            Styling = new ResponseStyling
            {
                Theme = "light",
                Accent = "error",
                CustomStyles = new Dictionary<string, string>
                {
                    { "icon", "error" },
                    { "border", "red" }
                }
            }
        };

        await SendNotificationAsync(response);
    }

    public async Task NotifySonetResponseAsync(SonetResponse sonetResponse)
    {
        var response = new UIResponse
        {
            Type = "sonet_response",
            Content = sonetResponse.Content,
            Timestamp = DateTime.UtcNow,
            Status = ResponseStatus.Success,
            Format = DetectResponseFormat(sonetResponse.Content),
            Metadata = new Dictionary<string, object>
            {
                { "response_id", sonetResponse.ResponseId },
                { "model_metadata", sonetResponse.Metadata }
            },
            Actions = new List<ResponseAction>
            {
                new()
                {
                    Type = "copy",
                    Label = "Copy Response",
                    Value = sonetResponse.Content
                },
                new()
                {
                    Type = "feedback",
                    Label = "Provide Feedback",
                    Value = sonetResponse.ResponseId
                }
            },
            Styling = new ResponseStyling
            {
                Theme = "light",
                Accent = "primary",
                HighlightCode = true,
                Language = DetectCodeLanguage(sonetResponse.Content)
            }
        };

        await SendNotificationAsync(response);
    }

    private async Task SendNotificationAsync(UIResponse response)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveResponse", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending UI notification");
        }
    }

    private static (ResponseFormat Format, ResponseFormatMetadata? Metadata) DetectResponseFormat(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return (ResponseFormat.Text, null);

        // Check for code blocks
        if (content.Contains("```") || content.Contains("    "))
        {
            var language = DetectCodeLanguage(content);
            return (ResponseFormat.Code, new ResponseFormatMetadata
            {
                Format = "code",
                Options = new Dictionary<string, object>
                {
                    { "language", language ?? "plaintext" },
                    { "showLineNumbers", true }
                }
            });
        }

        // Check for JSON
        if ((content.StartsWith("{") && content.EndsWith("}")) ||
            (content.StartsWith("[") && content.EndsWith("]")))
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // Check if it looks like data that should be in a grid
                    if (doc.RootElement.GetArrayLength() > 0 &&
                        doc.RootElement[0].ValueKind == JsonValueKind.Object)
                    {
                        var columns = doc.RootElement[0].EnumerateObject()
                            .Select(p => p.Name)
                            .ToArray();

                        return (ResponseFormat.DataGrid, new ResponseFormatMetadata
                        {
                            Format = "grid",
                            Options = new Dictionary<string, object>
                            {
                                { "columns", columns },
                                { "enableSorting", true },
                                { "enableFiltering", true }
                            }
                        });
                    }
                }
                return (ResponseFormat.Json, new ResponseFormatMetadata
                {
                    Format = "json",
                    Options = new Dictionary<string, object>
                    {
                        { "expandLevel", 2 },
                        { "enableClipboard", true }
                    }
                });
            }
            catch
            {
                return (ResponseFormat.Text, null);
            }
        }

        // Check for table-like structure
        if (content.Contains("|") && content.Contains("-+-"))
        {
            return (ResponseFormat.Table, new ResponseFormatMetadata
            {
                Format = "table",
                Options = new Dictionary<string, object>
                {
                    { "enableSorting", true },
                    { "style", "bordered" }
                }
            });
        }

        // Check for HTML
        if (content.Contains("<") && content.Contains(">"))
        {
            return (ResponseFormat.Html, new ResponseFormatMetadata
            {
                Format = "html",
                Options = new Dictionary<string, object>
                {
                    { "sanitize", true }
                }
            });
        }

        // Check for markdown
        if (content.Contains("#") || content.Contains("**") || content.Contains("__"))
        {
            return (ResponseFormat.Markdown, new ResponseFormatMetadata
            {
                Format = "markdown",
                Options = new Dictionary<string, object>
                {
                    { "breaks", true },
                    { "linkify", true },
                    { "typographer", true }
                }
            });
        }

        return (ResponseFormat.Text, null);
    }

    private static string? DetectCodeLanguage(string content)
    {
        if (!content.Contains("```"))
            return null;

        var codeBlockStart = content.IndexOf("```");
        var newlineIndex = content.IndexOf('\n', codeBlockStart);
        if (newlineIndex <= codeBlockStart + 3)
            return null;

        var language = content.Substring(codeBlockStart + 3, newlineIndex - (codeBlockStart + 3)).Trim();
        return string.IsNullOrWhiteSpace(language) ? null : language;
    }
}