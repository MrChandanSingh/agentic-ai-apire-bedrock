using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class SonetRequest
{
    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class SonetResponse
{
    [JsonPropertyName("response_id")]
    public string ResponseId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class UIResponse
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public ResponseStatus Status { get; set; } = ResponseStatus.Success;
    public string? Error { get; set; }
    public ResponseFormat Format { get; set; } = ResponseFormat.Text;
    public ResponseFormatMetadata? FormatMetadata { get; set; }
    public List<ResponseAction>? Actions { get; set; }
    public ResponseStyling? Styling { get; set; }
    public InteractionOptions? Interactions { get; set; }
    public ResponsiveLayout? Layout { get; set; }
    public ThemeOptions? Theme { get; set; }
    
    // Helper method to create interactive response
    public static UIResponse CreateInteractive(
        string content,
        ResponseFormat format,
        Action<InteractionOptions>? configureInteractions = null,
        Action<ThemeOptions>? configureTheme = null,
        Action<ResponsiveLayout>? configureLayout = null)
    {
        var response = new UIResponse
        {
            Content = content,
            Format = format,
            Timestamp = DateTime.UtcNow,
            Interactions = new InteractionOptions(),
            Theme = new ThemeOptions(),
            Layout = new ResponsiveLayout()
        };

        configureInteractions?.Invoke(response.Interactions);
        configureTheme?.Invoke(response.Theme);
        configureLayout?.Invoke(response.Layout);

        return response;
    }
    
    // Helper methods for specific formats
    public static UIResponse CreateChart(string title, ChartOptions options)
    {
        return new UIResponse
        {
            Type = "data_visualization",
            Format = ResponseFormat.Chart,
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "chart",
                Options = new Dictionary<string, object>
                {
                    { "chartOptions", options }
                }
            },
            Styling = new ResponseStyling
            {
                Theme = "light",
                Accent = "primary"
            }
        };
    }

    public static UIResponse CreateDataGrid(string[] data, DataGridOptions options)
    {
        return new UIResponse
        {
            Type = "data_display",
            Format = ResponseFormat.DataGrid,
            Content = System.Text.Json.JsonSerializer.Serialize(data),
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "grid",
                Options = new Dictionary<string, object>
                {
                    { "gridOptions", options }
                }
            }
        };
    }

    public static UIResponse CreateTimeline(object[] events, TimelineOptions options)
    {
        return new UIResponse
        {
            Type = "timeline",
            Format = ResponseFormat.Timeline,
            Content = System.Text.Json.JsonSerializer.Serialize(events),
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "timeline",
                Options = new Dictionary<string, object>
                {
                    { "timelineOptions", options }
                }
            }
        };
    }

    public static UIResponse CreateDiff(string oldContent, string newContent, DiffOptions options)
    {
        return new UIResponse
        {
            Type = "diff",
            Format = ResponseFormat.Diff,
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "diff",
                Options = new Dictionary<string, object>
                {
                    { "diffOptions", options },
                    { "oldContent", oldContent },
                    { "newContent", newContent }
                }
            }
        };
    }

    public static UIResponse CreateTerminal(string content, TerminalOptions options)
    {
        return new UIResponse
        {
            Type = "terminal",
            Format = ResponseFormat.Terminal,
            Content = content,
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "terminal",
                Options = new Dictionary<string, object>
                {
                    { "terminalOptions", options }
                }
            },
            Styling = new ResponseStyling
            {
                Theme = options.Theme ?? "dark",
                CustomStyles = new Dictionary<string, string>
                {
                    { "fontFamily", "monospace" }
                }
            }
        };
    }

    public static UIResponse CreateCard(string content, CardOptions options)
    {
        return new UIResponse
        {
            Type = "card",
            Format = ResponseFormat.Card,
            Content = content,
            FormatMetadata = new ResponseFormatMetadata
            {
                Format = "card",
                Options = new Dictionary<string, object>
                {
                    { "cardOptions", options }
                }
            }
        };
    }
}

public enum ResponseStatus
{
    Success,
    Error,
    Warning,
    Info,
    Processing
}

public enum ResponseFormat
{
    Text,
    Markdown,
    Html,
    Json,
    Table,
    Code,
    Chart,
    Image,
    DataGrid,
    Timeline,
    List,
    Alert,
    Card,
    Terminal,
    Diff,
    Tree
}

public class ResponseFormatMetadata
{
    public string Format { get; set; } = string.Empty;
    public Dictionary<string, object> Options { get; set; } = new();
}

public class ChartOptions
{
    public string Type { get; set; } = "line"; // line, bar, pie, etc.
    public string[] Labels { get; set; } = Array.Empty<string>();
    public Dictionary<string, double[]> Datasets { get; set; } = new();
    public ChartStyling? Styling { get; set; }
}

public class ChartStyling
{
    public string[] Colors { get; set; } = Array.Empty<string>();
    public bool ShowLegend { get; set; } = true;
    public bool ShowGrid { get; set; } = true;
    public string? Title { get; set; }
}

public class DataGridOptions
{
    public string[] Columns { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> ColumnTypes { get; set; } = new();
    public bool EnableSorting { get; set; } = true;
    public bool EnableFiltering { get; set; } = true;
    public bool EnablePagination { get; set; } = true;
    public int PageSize { get; set; } = 10;
}

public class TimelineOptions
{
    public bool ShowTime { get; set; } = true;
    public bool Grouped { get; set; } = false;
    public string? GroupBy { get; set; }
    public TimelineStyle Style { get; set; } = TimelineStyle.Vertical;
}

public enum TimelineStyle
{
    Vertical,
    Horizontal,
    Branching
}

public class TreeOptions
{
    public bool Expandable { get; set; } = true;
    public int InitialExpandLevel { get; set; } = 1;
    public bool ShowIcons { get; set; } = true;
    public bool EnableSearch { get; set; } = false;
}

public class DiffOptions
{
    public bool ShowLineNumbers { get; set; } = true;
    public bool SplitView { get; set; } = true;
    public bool HighlightIntraline { get; set; } = true;
    public string? Language { get; set; }
}

public class CardOptions
{
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = false;
    public string? HeaderIcon { get; set; }
    public string? FooterText { get; set; }
    public CardStyle Style { get; set; } = CardStyle.Default;
}

public enum CardStyle
{
    Default,
    Elevated,
    Outlined,
    Compact
}

public class TerminalOptions
{
    public bool ShowPrompt { get; set; } = true;
    public string PromptSymbol { get; set; } = "$";
    public bool ShowTimestamp { get; set; } = false;
    public string? Theme { get; set; }
    public bool EnableCopy { get; set; } = true;
}

public class ResponseAction
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class ResponseStyling
{
    public string? Theme { get; set; }
    public string? Accent { get; set; }
    public Dictionary<string, string>? CustomStyles { get; set; }
    public bool HighlightCode { get; set; }
    public string? Language { get; set; }
}

public class SonetModelParameters
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 2048;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.95;

    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0.0;

    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; } = 0.0;

    [JsonPropertyName("stop_sequences")]
    public List<string>? StopSequences { get; set; }
}