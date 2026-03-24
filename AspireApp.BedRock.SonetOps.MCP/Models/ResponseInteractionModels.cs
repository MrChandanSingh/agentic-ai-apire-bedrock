using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class InteractionOptions
{
    public bool EnableCopy { get; set; } = true;
    public bool EnableDownload { get; set; } = false;
    public bool EnableExpand { get; set; } = false;
    public bool EnableShare { get; set; } = false;
    public bool EnableEdit { get; set; } = false;
    public List<CustomAction> CustomActions { get; set; } = new();
    public InteractionBehavior Behavior { get; set; } = new();
    public InteractionAnimation Animation { get; set; } = new();
}

public class CustomAction
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Shortcut { get; set; }
    public Dictionary<string, object> Payload { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
}

public class InteractionBehavior
{
    public bool AutoFocus { get; set; } = false;
    public bool AutoScroll { get; set; } = true;
    public int AutoScrollDelay { get; set; } = 0;
    public bool DragEnabled { get; set; } = false;
    public bool ResizeEnabled { get; set; } = false;
    public bool MinimizeEnabled { get; set; } = false;
    public bool FullscreenEnabled { get; set; } = false;
    public InteractionMode Mode { get; set; } = InteractionMode.Default;
    public Dictionary<string, object> CustomBehaviors { get; set; } = new();
}

public class InteractionAnimation
{
    public bool Enabled { get; set; } = true;
    public string EntryAnimation { get; set; } = "fade";
    public string ExitAnimation { get; set; } = "fade";
    public int Duration { get; set; } = 300;
    public string TimingFunction { get; set; } = "ease";
    public bool EnableTransitions { get; set; } = true;
    public Dictionary<string, AnimationPreset> CustomAnimations { get; set; } = new();
}

public class AnimationPreset
{
    public string Name { get; set; } = string.Empty;
    public string KeyframesIn { get; set; } = string.Empty;
    public string KeyframesOut { get; set; } = string.Empty;
    public int Duration { get; set; } = 300;
    public string TimingFunction { get; set; } = "ease";
}

public enum InteractionMode
{
    Default,
    ReadOnly,
    Interactive,
    Editable,
    Streaming
}

public class ResponsiveLayout
{
    public string Layout { get; set; } = "auto";
    public Dictionary<string, BreakpointOptions> Breakpoints { get; set; } = new();
}

public class BreakpointOptions
{
    public int MaxWidth { get; set; }
    public string Layout { get; set; } = string.Empty;
    public Dictionary<string, string> Styles { get; set; } = new();
}

public class ThemeOptions
{
    public string Name { get; set; } = "default";
    public ColorScheme Colors { get; set; } = new();
    public Typography Typography { get; set; } = new();
    public Spacing Spacing { get; set; } = new();
    public Dictionary<string, string> CustomStyles { get; set; } = new();
}

public class ColorScheme
{
    public string Primary { get; set; } = "#007bff";
    public string Secondary { get; set; } = "#6c757d";
    public string Success { get; set; } = "#28a745";
    public string Danger { get; set; } = "#dc3545";
    public string Warning { get; set; } = "#ffc107";
    public string Info { get; set; } = "#17a2b8";
    public string Light { get; set; } = "#f8f9fa";
    public string Dark { get; set; } = "#343a40";
    public string Background { get; set; } = "#ffffff";
    public string Text { get; set; } = "#212529";
}

public class Typography
{
    public string FontFamily { get; set; } = "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial";
    public string CodeFontFamily { get; set; } = "SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono'";
    public Dictionary<string, FontSettings> Variants { get; set; } = new();
}

public class FontSettings
{
    public string Weight { get; set; } = "normal";
    public string Size { get; set; } = "1rem";
    public string LineHeight { get; set; } = "1.5";
    public string LetterSpacing { get; set; } = "normal";
}

public class Spacing
{
    public string Base { get; set; } = "1rem";
    public Dictionary<string, string> Scale { get; set; } = new()
    {
        { "xs", "0.25rem" },
        { "sm", "0.5rem" },
        { "md", "1rem" },
        { "lg", "1.5rem" },
        { "xl", "2rem" }
    };
}