using System.Text.Json;
using AspireApp.BedRock.SonetOps.MCP.Models;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IThemeService
{
    ThemePreset GetTheme(string name);
    ThemePreset GetThemeForFormat(ResponseFormat format);
    ThemePreset CustomizeTheme(string baseName, Action<ThemePreset> customization);
}

public class ThemeService : IThemeService
{
    private readonly Dictionary<string, ThemePreset> _themePresets;
    private readonly Dictionary<ResponseFormat, string> _formatThemes;

    public ThemeService()
    {
        _themePresets = new Dictionary<string, ThemePreset>
        {
            { "default", CreateDefaultTheme() },
            { "dark", CreateDarkTheme() },
            { "high-contrast", CreateHighContrastTheme() },
            { "terminal", CreateTerminalTheme() },
            { "modern", CreateModernTheme() },
            { "classic", CreateClassicTheme() }
        };

        _formatThemes = new Dictionary<ResponseFormat, string>
        {
            { ResponseFormat.Code, "terminal" },
            { ResponseFormat.Terminal, "terminal" },
            { ResponseFormat.DataGrid, "modern" },
            { ResponseFormat.Chart, "modern" },
            { ResponseFormat.Markdown, "classic" },
            { ResponseFormat.Text, "default" }
        };
    }

    public ThemePreset GetTheme(string name)
    {
        return _themePresets.GetValueOrDefault(name) ?? _themePresets["default"];
    }

    public ThemePreset GetThemeForFormat(ResponseFormat format)
    {
        var themeName = _formatThemes.GetValueOrDefault(format) ?? "default";
        return GetTheme(themeName);
    }

    public ThemePreset CustomizeTheme(string baseName, Action<ThemePreset> customization)
    {
        var baseTheme = GetTheme(baseName);
        var customTheme = JsonSerializer.Deserialize<ThemePreset>(
            JsonSerializer.Serialize(baseTheme)) ?? new ThemePreset();
        
        customization(customTheme);
        return customTheme;
    }

    private static ThemePreset CreateDefaultTheme() => new()
    {
        Name = "default",
        Description = "Clean, modern default theme",
        Colors = new ColorPalette
        {
            Primary = new ColorSet
            {
                Main = "#1976d2",
                Light = "#42a5f5",
                Dark = "#1565c0",
                Contrast = "#ffffff"
            },
            Secondary = new ColorSet
            {
                Main = "#9c27b0",
                Light = "#ba68c8",
                Dark = "#7b1fa2",
                Contrast = "#ffffff"
            },
            Background = new BackgroundColors
            {
                Default = "#ffffff",
                Paper = "#f5f5f5",
                Elevated = "#ffffff",
                Overlay = "rgba(0, 0, 0, 0.5)"
            },
            Text = new TextColors
            {
                Primary = "rgba(0, 0, 0, 0.87)",
                Secondary = "rgba(0, 0, 0, 0.6)",
                Disabled = "rgba(0, 0, 0, 0.38)",
                Hint = "rgba(0, 0, 0, 0.38)"
            }
        },
        Typography = new TypographySystem
        {
            FontFamilies = new()
            {
                {
                    "primary", new FontFamily
                    {
                        Primary = "Inter",
                        Fallback = new[] { "system-ui", "sans-serif" },
                        IsVariable = true,
                        VariableAxes = new()
                        {
                            { "wght", "100 900" },
                            { "slnt", "-10 0" }
                        }
                    }
                },
                {
                    "code", new FontFamily
                    {
                        Primary = "JetBrains Mono",
                        Fallback = new[] { "Consolas", "monospace" }
                    }
                }
            },
            Styles = new()
            {
                {
                    "h1", new FontStyle
                    {
                        Family = "primary",
                        Size = "2.5rem",
                        Weight = "700",
                        LineHeight = "1.2",
                        LetterSpacing = "-0.02em"
                    }
                },
                {
                    "body", new FontStyle
                    {
                        Family = "primary",
                        Size = "1rem",
                        Weight = "400",
                        LineHeight = "1.5",
                        LetterSpacing = "0"
                    }
                },
                {
                    "code", new FontStyle
                    {
                        Family = "code",
                        Size = "0.9em",
                        Weight = "400",
                        LineHeight = "1.4",
                        LetterSpacing = "0"
                    }
                }
            }
        },
        Spacing = new SpacingSystem
        {
            Unit = "rem",
            Scale = new()
            {
                { "xs", 0.25 },
                { "sm", 0.5 },
                { "md", 1 },
                { "lg", 1.5 },
                { "xl", 2 }
            }
        },
        Borders = new BorderSystem
        {
            Styles = new()
            {
                {
                    "default", new Border
                    {
                        Width = "1px",
                        Style = "solid",
                        Color = "rgba(0, 0, 0, 0.12)",
                        Radius = "4px"
                    }
                }
            }
        },
        Shadows = new ShadowSystem
        {
            Elevations = new()
            {
                { "1", "0 2px 4px rgba(0,0,0,0.1)" },
                { "2", "0 4px 8px rgba(0,0,0,0.1)" },
                { "3", "0 8px 16px rgba(0,0,0,0.1)" }
            }
        }
    };

    private static ThemePreset CreateDarkTheme()
    {
        var theme = CreateDefaultTheme();
        theme.Name = "dark";
        theme.Description = "Dark mode theme";
        
        theme.Colors.Background.Default = "#121212";
        theme.Colors.Background.Paper = "#1e1e1e";
        theme.Colors.Text.Primary = "rgba(255, 255, 255, 0.87)";
        theme.Colors.Text.Secondary = "rgba(255, 255, 255, 0.6)";
        
        return theme;
    }

    private static ThemePreset CreateHighContrastTheme()
    {
        var theme = CreateDefaultTheme();
        theme.Name = "high-contrast";
        theme.Description = "High contrast accessibility theme";
        
        theme.Colors.Background.Default = "#000000";
        theme.Colors.Background.Paper = "#000000";
        theme.Colors.Text.Primary = "#ffffff";
        theme.Colors.Text.Secondary = "#ffffff";
        
        theme.Typography.Styles["body"].Size = "1.2rem";
        theme.Typography.Styles["body"].LineHeight = "1.8";
        
        return theme;
    }

    private static ThemePreset CreateTerminalTheme()
    {
        var theme = CreateDarkTheme();
        theme.Name = "terminal";
        theme.Description = "Classic terminal theme";
        
        theme.Colors.Background.Default = "#000000";
        theme.Colors.Background.Paper = "#000000";
        theme.Colors.Primary.Main = "#00ff00";
        theme.Colors.Text.Primary = "#00ff00";
        
        theme.Typography.FontFamilies["primary"] = new FontFamily
        {
            Primary = "JetBrains Mono",
            Fallback = new[] { "Consolas", "monospace" }
        };
        
        return theme;
    }

    private static ThemePreset CreateModernTheme()
    {
        var theme = CreateDefaultTheme();
        theme.Name = "modern";
        theme.Description = "Modern, clean design theme";
        
        theme.Colors.Primary.Main = "#0070f3";
        theme.Colors.Background.Default = "#fafafa";
        
        theme.Typography.FontFamilies["primary"] = new FontFamily
        {
            Primary = "Plus Jakarta Sans",
            Fallback = new[] { "system-ui", "sans-serif" },
            IsVariable = true
        };
        
        theme.Borders.Styles["default"].Radius = "8px";
        theme.Shadows.Elevations["1"] = "0 2px 8px rgba(0,0,0,0.05)";
        
        return theme;
    }

    private static ThemePreset CreateClassicTheme()
    {
        var theme = CreateDefaultTheme();
        theme.Name = "classic";
        theme.Description = "Traditional, serif-based theme";
        
        theme.Typography.FontFamilies["primary"] = new FontFamily
        {
            Primary = "Merriweather",
            Fallback = new[] { "Georgia", "serif" }
        };
        
        theme.Typography.Styles["body"].Size = "1.125rem";
        theme.Typography.Styles["body"].LineHeight = "1.8";
        
        return theme;
    }
}