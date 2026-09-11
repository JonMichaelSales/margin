using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaThemeManager.Theme;
using MDPlayer.Core;

namespace MDPlayer.Desktop.Services;

public sealed record SkinChoice(string Name, bool IsApplicationSkin, Skin Skin)
{
    public IBrush Background => new SolidColorBrush(Skin.PrimaryBackground);
    public IBrush Foreground => new SolidColorBrush(Skin.PrimaryTextColor);
    public IBrush Accent => new SolidColorBrush(Skin.AccentColor);
    public IBrush Secondary => new SolidColorBrush(Skin.SecondaryColor);
}
public interface IAppearanceService
{
    IReadOnlyList<SkinChoice> Catalog { get; }
    ThemePreference Selection { get; }
    event EventHandler? Changed;
    void Initialize();
    void BeginPreview();
    void Preview(ThemePreference selection);
    void Commit();
    void Cancel();
}
public sealed class ThemeSelectionStore(IUserPreferencesStore preferences) : IThemeSelectionStore
{
    public string? GetSavedThemeName() => preferences.Current.Theme.Normalize().SkinName;
    public void SaveSelectedTheme(string? themeName)
    {
        if (themeName is not null) preferences.Save(preferences.Current with { Theme = new(false, themeName) });
    }
}
public sealed class AppearanceService(ISkinManager manager, IUserPreferencesStore preferences) : IAppearanceService, IDisposable
{
    private readonly List<SkinChoice> _catalog = [];
    private ThemePreference? _beforePreview;
    private bool _previewing;
    public IReadOnlyList<SkinChoice> Catalog => _catalog;
    public ThemePreference Selection { get; private set; } = new();
    public event EventHandler? Changed;
    public void Initialize()
    {
        foreach (var name in manager.GetAvailableSkinNames())
        {
            var original = manager.GetSkin(name) ?? throw new InvalidDataException("Missing included skin " + name);
            _catalog.Add(new(name, false, PaletteCopy(original)));
        }
        if (_catalog.Count != 12) throw new InvalidDataException($"Expected 12 included skins, found {_catalog.Count}.");
        foreach (var uri in AssetLoader.GetAssets(new Uri("avares://Margin/Skins"), null).Where(uri => uri.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            using var stream = AssetLoader.Open(uri);
            var serializable = JsonSerializer.Deserialize<SerializableTheme>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("Invalid skin " + uri);
            var skin = serializable.ToSkin();
            manager.RegisterSkin(skin.Name, skin); _catalog.Add(new(skin.Name!, true, PaletteCopy(skin)));
        }
        if (_catalog.Count != 18) throw new InvalidDataException($"Expected 18 skins, found {_catalog.Count}.");
        Selection = preferences.Current.Theme.Normalize();
        if (!Selection.FollowSystem && !_catalog.Any(x => x.Name == Selection.SkinName)) Selection = new();
        Apply(Selection);
        if (Application.Current?.PlatformSettings is { } settings) settings.ColorValuesChanged += SystemColorsChanged;
    }
    public void BeginPreview()
    {
        if (_previewing) throw new InvalidOperationException("An appearance preview is already open.");
        _beforePreview = Selection; _previewing = true;
    }
    public void Preview(ThemePreference selection) { Apply(selection); Selection = selection; }
    public void Commit()
    {
        preferences.Save(preferences.Current with { Theme = Selection });
        _previewing = false; _beforePreview = null;
    }
    public void Cancel()
    {
        if (_beforePreview is { } original) { Apply(original); Selection = original; }
        _previewing = false; _beforePreview = null;
    }
    private void Apply(ThemePreference selection)
    {
        Dispatcher.UIThread.VerifyAccess();
        var dark = Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark;
        var name = selection.FollowSystem ? "Margin Paper " + (dark ? "Dark" : "Light") : selection.SkinName;
        var choice = _catalog.FirstOrDefault(x => x.Name == name) ?? throw new InvalidDataException("Unknown skin: " + name);
        var previous = manager.CurrentSkin is { } old ? PaletteCopy(old) : null;
        var appBefore = Application.Current!;
        var resourcesBefore = appBefore.Resources.ToDictionary(pair => pair.Key, pair => pair.Value is SolidColorBrush brush ? (object)new SolidColorBrush(brush.Color, brush.Opacity) : pair.Value);
        var variantBefore = appBefore.RequestedThemeVariant;
        var selectionBefore = Selection;
        var notified = false;
        void OnChanged(object? _, EventArgs __) => notified = true;
        manager.SkinChanged += OnChanged;
        try
        {
            manager.ApplySkin(choice.Skin);
            var expected = new Dictionary<string, Color>
            {
                ["BackgroundBrush"] = choice.Skin.PrimaryBackground, ["BackgroundLightBrush"] = choice.Skin.SecondaryBackground,
                ["PrimaryColorBrush"] = choice.Skin.PrimaryColor, ["SecondaryColorBrush"] = choice.Skin.SecondaryColor,
                ["AccentBlueBrush"] = choice.Skin.AccentColor, ["TextPrimaryBrush"] = choice.Skin.PrimaryTextColor,
                ["TextSecondaryBrush"] = choice.Skin.SecondaryTextColor, ["BorderBrush"] = choice.Skin.BorderColor,
                ["ErrorBrush"] = choice.Skin.ErrorColor, ["WarningBrush"] = choice.Skin.WarningColor, ["SuccessBrush"] = choice.Skin.SuccessColor
            };
            if (!notified || expected.Any(pair => Application.Current?.Resources[pair.Key] is not SolidColorBrush brush || brush.Color != pair.Value))
                throw new InvalidOperationException("The selected skin could not be applied completely.");
            var app = Application.Current!;
            app.RequestedThemeVariant = Luminance(choice.Skin.PrimaryBackground) < Luminance(choice.Skin.PrimaryTextColor) ? ThemeVariant.Dark : ThemeVariant.Light;
            var onAccent = Contrast(choice.Skin.PrimaryTextColor, choice.Skin.AccentColor) >= Contrast(choice.Skin.PrimaryBackground, choice.Skin.AccentColor)
                ? choice.Skin.PrimaryTextColor : choice.Skin.PrimaryBackground;
            app.Resources["OnAccentBrush"] = new SolidColorBrush(onAccent);
            // Fluent template aliases use the same SkinManager values, never an independent palette.
            foreach (var key in new[] { "SystemControlForegroundBaseHighBrush", "SystemControlForegroundBaseMediumBrush", "TextControlForeground", "TextControlForegroundFocused", "TextControlForegroundPointerOver", "ComboBoxForeground", "MenuFlyoutItemForeground" }) app.Resources[key] = app.Resources["TextPrimaryBrush"];
            foreach (var key in new[] { "TextControlBackground", "TextControlBackgroundFocused", "ComboBoxBackground", "MenuFlyoutPresenterBackground", "ToolTipBackground" }) app.Resources[key] = app.Resources["BackgroundLightBrush"];
            foreach (var key in new[] { "TextControlBorderBrush", "ComboBoxBorderBrush", "ToolTipBorderBrush" }) app.Resources[key] = app.Resources["BorderBrush"];
            app.Resources["TextControlSelectionHighlightColor"] = choice.Skin.SecondaryColor;
            FluentBrushAliases.Apply(app);
            Selection = selection; Changed?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            if (previous is not null) manager.ApplySkin(previous);
            foreach (var key in appBefore.Resources.Keys.Where(key => !resourcesBefore.ContainsKey(key)).ToArray()) appBefore.Resources.Remove(key);
            foreach (var pair in resourcesBefore) appBefore.Resources[pair.Key] = pair.Value;
            appBefore.RequestedThemeVariant = variantBefore; Selection = selectionBefore;
            // Restore derived drawing caches as well as dynamic resource bindings.
            foreach (EventHandler handler in Changed?.GetInvocationList() ?? [])
                try { handler(this, EventArgs.Empty); } catch { /* Preserve the original application failure. */ }
            throw;
        }
        finally { manager.SkinChanged -= OnChanged; }
    }
    private void SystemColorsChanged(object? sender, PlatformColorValues values)
    {
        if (Selection.FollowSystem) Dispatcher.UIThread.Post(() => Apply(Selection));
    }
    public void Dispose() { if (Application.Current?.PlatformSettings is { } settings) settings.ColorValuesChanged -= SystemColorsChanged; }
    public static double Contrast(Color a, Color b) => (Math.Max(Luminance(a), Luminance(b)) + .05) / (Math.Min(Luminance(a), Luminance(b)) + .05);
    private static double Luminance(Color color)
    {
        static double Channel(byte v) { var n = v / 255d; return n <= .04045 ? n / 12.92 : Math.Pow((n + .055) / 1.055, 2.4); }
        return .2126 * Channel(color.R) + .7152 * Channel(color.G) + .0722 * Channel(color.B);
    }
    private static Skin PaletteCopy(Skin skin) => new()
    {
        Name = skin.Name, PrimaryColor = skin.PrimaryColor, SecondaryColor = skin.SecondaryColor, AccentColor = skin.AccentColor,
        PrimaryBackground = skin.PrimaryBackground, SecondaryBackground = skin.SecondaryBackground,
        PrimaryTextColor = skin.PrimaryTextColor, SecondaryTextColor = skin.SecondaryTextColor, BorderColor = skin.BorderColor,
        ErrorColor = skin.ErrorColor, WarningColor = skin.WarningColor, SuccessColor = skin.SuccessColor,
        FontFamily = skin.FontFamily, FontSizeSmall = skin.FontSizeSmall, FontSizeMedium = skin.FontSizeMedium, FontSizeLarge = skin.FontSizeLarge,
        FontWeight = skin.FontWeight, BorderRadius = skin.BorderRadius, BorderThickness = skin.BorderThickness,
        HeaderFontFamily = skin.HeaderFontFamily, BodyFontFamily = skin.BodyFontFamily, MonospaceFontFamily = skin.MonospaceFontFamily,
        Typography = skin.Typography, LineHeight = skin.LineHeight, LetterSpacing = skin.LetterSpacing, EnableLigatures = skin.EnableLigatures,
        ControlThemeUris = new(), StyleUris = new()
    };
}
