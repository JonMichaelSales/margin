using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using MDPlayer.Core;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Services;
using MDPlayer.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(MDPlayer.Tests.TestApplication))]
namespace MDPlayer.Tests;

public sealed class MemoryPreferences : IUserPreferencesStore
{
    public UserPreferences Current { get; private set; } = new();
    public void Save(UserPreferences preferences) => Current = preferences;
    public string? TakeLoadNotice() => null;
}
public sealed class TestApplication : App
{
    protected override IUserPreferencesStore CreatePreferencesStore() => new MemoryPreferences();
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>().UseSkia().WithInterFont().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class AppearanceTests
{
    [AvaloniaFact]
    public void AllEighteenSkinsApplyAndCancelWithoutPersistingPreviews()
    {
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var preferences = app.Services.GetRequiredService<IUserPreferencesStore>();
        Assert.Equal(18, appearance.Catalog.Count);
        Assert.Equal(6, appearance.Catalog.Count(x => x.IsApplicationSkin));
        var original = appearance.Selection;
        var saved = preferences.Current.Theme;
        appearance.BeginPreview();
        foreach (var skin in appearance.Catalog)
        {
            appearance.Preview(new(false, skin.Name));
            Assert.Equal(saved, preferences.Current.Theme);
            var background = Assert.IsType<Avalonia.Media.SolidColorBrush>(app.Resources["BackgroundBrush"]);
            Assert.Equal(skin.Skin.PrimaryBackground, background.Color);
            Assert.Empty(skin.Skin.ControlThemeUris);
            Assert.Empty(skin.Skin.StyleUris);
        }
        appearance.Cancel();
        Assert.Equal(original, appearance.Selection);
        Assert.Equal(saved, preferences.Current.Theme);
        appearance.BeginPreview();
        appearance.Preview(new(false, "Margin Studio Dark"));
        appearance.Commit();
        Assert.Equal("Margin Studio Dark", preferences.Current.Theme.SkinName);
        appearance.BeginPreview(); appearance.Preview(original); appearance.Commit();
    }

    [AvaloniaFact]
    public void ApplicationSkinTextAndAccentContrastMeetReadingThreshold()
    {
        var appearance = ((App)Application.Current!).Services.GetRequiredService<IAppearanceService>();
        foreach (var choice in appearance.Catalog.Where(x => x.IsApplicationSkin))
        {
            var skin = choice.Skin;
            Assert.True(AppearanceService.Contrast(skin.PrimaryTextColor, skin.PrimaryBackground) >= 7, choice.Name + " primary text");
            Assert.True(AppearanceService.Contrast(skin.SecondaryTextColor, skin.PrimaryBackground) >= 4.5, choice.Name + " secondary text");
            Assert.True(AppearanceService.Contrast(skin.AccentColor, skin.PrimaryBackground) >= 4.5, choice.Name + " link");
        }
    }

    [AvaloniaFact]
    public void ReaderSelectionSpansParagraphsAndSkinChangesPreserveTypography()
    {
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var reader = new MarkdownDocumentView { Preferences = new() { FontSize = 23, LineHeight = 1.8 } };
        var document = new MarkdownParser().Parse("First 👩🏽‍💻 paragraph.\n\nSecond العربية paragraph.", 1);
        reader.SetDocument(document);
        var window = new Window { Content = new ScrollViewer { Content = reader }, Width = 900, Height = 650 };
        window.Show(); reader.Focus();
        window.KeyPressQwerty(PhysicalKey.A, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
        Assert.Equal(document.PlainText, reader.SelectedText);
        var reading = reader.Preferences;
        appearance.BeginPreview();
        foreach (var skin in appearance.Catalog)
        {
            appearance.Preview(new(false, skin.Name)); reader.RefreshColors();
            Assert.Equal(reading, reader.Preferences);
            Assert.Same(document, reader.Document);
            Assert.Equal(document.PlainText, reader.SelectedText);
        }
        appearance.Cancel(); window.Close();
    }

    [AvaloniaFact]
    public void MainWindowCanCreateNativeReaderAndStartsEmpty()
    {
        var window = ((App)Application.Current!).CreateWindow();
        window.Show();
        Assert.Equal(DocumentMode.Read, window.Session.Mode);
        Assert.False(window.Session.IsDirty);
        Assert.Null(window.Session.FilePath);
        Assert.True(window.FindControl<ScrollViewer>("EmptyPanel")!.IsVisible);
        window.Close();
    }

    [AvaloniaFact]
    public async Task ReadingAppearanceAutoSavesAndLoadsInTheNextWindow()
    {
        var app = (App)Application.Current!;
        var preferences = app.Services.GetRequiredService<IUserPreferencesStore>();
        var original = preferences.Current;
        try
        {
            var first = app.CreateWindow();
            first.Show();
            first.FindControl<Slider>("FontSizeSlider")!.Value = 23;
            first.FindControl<Slider>("ParagraphGapSlider")!.Value = 1.4;
            await Task.Delay(650);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(23, preferences.Current.Reading.FontSize);
            Assert.Equal(1.4, preferences.Current.Reading.ParagraphGap, 5);
            Assert.Equal("Saved automatically", first.FindControl<TextBlock>("TypographySaveStatus")!.Text);
            first.Close();

            var second = app.CreateWindow();
            second.Show();
            Assert.Equal(23, second.FindControl<Slider>("FontSizeSlider")!.Value);
            Assert.Equal(1.4, second.FindControl<Slider>("ParagraphGapSlider")!.Value, 5);
            second.Close();
        }
        finally { preferences.Save(original); }
    }
}
