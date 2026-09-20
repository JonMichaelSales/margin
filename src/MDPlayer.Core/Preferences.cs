namespace MDPlayer.Core;

public enum ReadingWidthMode { Custom, Comfortable, Wide, Full }

public sealed record ReadingPreferences
{
    public string FontFamily { get; init; } = "Georgia";
    public double FontSize { get; init; } = 18;
    public double LineHeight { get; init; } = 1.65;
    public double ParagraphGap { get; init; } = .8;
    public int WidthCharacters { get; init; } = 68;
    public ReadingWidthMode WidthMode { get; init; } = ReadingWidthMode.Custom;
    public double MeasureWidth(double availableWidth)
    {
        var available = double.IsFinite(availableWidth) ? Math.Max(0, availableWidth) : 960;
        var characters = WidthMode switch { ReadingWidthMode.Comfortable => 68, ReadingWidthMode.Wide => 110, _ => WidthCharacters };
        return WidthMode == ReadingWidthMode.Full ? available : Math.Min(available, characters * FontSize * .54 + 64);
    }
    public bool Justified { get; init; }
    public double FirstLineIndent { get; init; }
    public ReadingPreferences Sanitize() => this with
    {
        FontFamily = string.IsNullOrWhiteSpace(FontFamily) ? "Georgia" : FontFamily,
        FontSize = Clamp(FontSize, 12, 32, 18), LineHeight = Clamp(LineHeight, 1.2, 2.2, 1.65),
        ParagraphGap = Clamp(ParagraphGap, 0, 1.8, .8), WidthCharacters = Math.Clamp(WidthCharacters, 45, 160),
        WidthMode = Enum.IsDefined(WidthMode) ? WidthMode : ReadingWidthMode.Custom,
        FirstLineIndent = Clamp(FirstLineIndent, 0, 2, 0)
    };
    private static double Clamp(double value, double min, double max, double fallback) => double.IsFinite(value) ? Math.Clamp(value, min, max) : fallback;
}
public sealed record ThemePreference(bool FollowSystem = true, string SkinName = "Margin Paper Light")
{
    // Read old selections without rewriting preferences just because the app was renamed.
    public ThemePreference Normalize() => SkinName is "MDPlayer Paper Light" or "MDPlayer Paper Dark"
        or "MDPlayer Slate Light" or "MDPlayer Slate Dark" or "MDPlayer Studio Light" or "MDPlayer Studio Dark"
        ? this with { SkinName = "Margin " + SkinName[9..] } : this;
}
public sealed record WindowGeometry(double Width = 1280, double Height = 860, double? X = null, double? Y = null, bool Maximized = false);
public sealed record UserPreferences
{
    public const int CurrentSchemaVersion = 2;
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public ThemePreference Theme { get; init; } = new();
    public ReadingPreferences Reading { get; init; } = new();
    public WindowGeometry Window { get; init; } = new();
    public string EditorFontFamily { get; init; } = "Cascadia Mono, Menlo, monospace";
    public double EditorFontSize { get; init; } = 14;
}
public interface IUserPreferencesStore
{
    UserPreferences Current { get; }
    void Save(UserPreferences preferences);
    string? TakeLoadNotice();
}
