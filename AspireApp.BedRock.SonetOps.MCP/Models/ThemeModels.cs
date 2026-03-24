using System.Text.Json.Serialization;

namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class ThemeSystem
{
    public string CurrentTheme { get; set; } = "default";
    public bool EnableDarkMode { get; set; } = false;
    public ThemePreferences Preferences { get; set; } = new();
    public Dictionary<string, ThemePreset> Presets { get; set; } = new();
}

public class ThemePreferences
{
    public bool UseSystemTheme { get; set; } = true;
    public bool AutoDarkMode { get; set; } = true;
    public string? DarkModeStart { get; set; }
    public string? DarkModeEnd { get; set; }
    public double ContrastLevel { get; set; } = 1.0;
    public string FontScale { get; set; } = "1";
    public bool ReduceAnimations { get; set; } = false;
    public bool HighContrastMode { get; set; } = false;
}

public class ThemePreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ColorPalette Colors { get; set; } = new();
    public TypographySystem Typography { get; set; } = new();
    public SpacingSystem Spacing { get; set; } = new();
    public BorderSystem Borders { get; set; } = new();
    public ShadowSystem Shadows { get; set; } = new();
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class ColorPalette
{
    public ColorSet Primary { get; set; } = new();
    public ColorSet Secondary { get; set; } = new();
    public ColorSet Success { get; set; } = new();
    public ColorSet Warning { get; set; } = new();
    public ColorSet Error { get; set; } = new();
    public ColorSet Info { get; set; } = new();
    public BackgroundColors Background { get; set; } = new();
    public TextColors Text { get; set; } = new();
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class ColorSet
{
    public string Main { get; set; } = string.Empty;
    public string Light { get; set; } = string.Empty;
    public string Dark { get; set; } = string.Empty;
    public string Contrast { get; set; } = string.Empty;
    public double Alpha { get; set; } = 1.0;
}

public class BackgroundColors
{
    public string Default { get; set; } = string.Empty;
    public string Paper { get; set; } = string.Empty;
    public string Elevated { get; set; } = string.Empty;
    public string Overlay { get; set; } = string.Empty;
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class TextColors
{
    public string Primary { get; set; } = string.Empty;
    public string Secondary { get; set; } = string.Empty;
    public string Disabled { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class TypographySystem
{
    public Dictionary<string, FontFamily> FontFamilies { get; set; } = new();
    public Dictionary<string, FontStyle> Styles { get; set; } = new();
    public Dictionary<string, double> Scale { get; set; } = new();
    public LineHeights LineHeights { get; set; } = new();
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class FontFamily
{
    public string Primary { get; set; } = string.Empty;
    public string[] Fallback { get; set; } = Array.Empty<string>();
    public bool IsVariable { get; set; } = false;
    public Dictionary<string, string>? VariableAxes { get; set; }
}

public class FontStyle
{
    public string Family { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string LineHeight { get; set; } = string.Empty;
    public string LetterSpacing { get; set; } = string.Empty;
    public string TextTransform { get; set; } = "none";
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class LineHeights
{
    public string Tight { get; set; } = "1.2";
    public string Normal { get; set; } = "1.5";
    public string Relaxed { get; set; } = "1.75";
    public Dictionary<string, string> Custom { get; set; } = new();
}

public class SpacingSystem
{
    public string Unit { get; set; } = "rem";
    public Dictionary<string, double> Scale { get; set; } = new();
    public Dictionary<string, Space> Presets { get; set; } = new();
}

public class Space
{
    public string Top { get; set; } = "0";
    public string Right { get; set; } = "0";
    public string Bottom { get; set; } = "0";
    public string Left { get; set; } = "0";
}

public class BorderSystem
{
    public Dictionary<string, Border> Styles { get; set; } = new();
    public Dictionary<string, double> Widths { get; set; } = new();
    public Dictionary<string, string> Colors { get; set; } = new();
    public Dictionary<string, string> Radii { get; set; } = new();
}

public class Border
{
    public string Width { get; set; } = "1px";
    public string Style { get; set; } = "solid";
    public string Color { get; set; } = "currentColor";
    public string Radius { get; set; } = "0";
}

public class ShadowSystem
{
    public Dictionary<string, string> Elevations { get; set; } = new();
    public Dictionary<string, string> Custom { get; set; } = new();
}