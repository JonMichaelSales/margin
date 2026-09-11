using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using MDPlayer.Core;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Services;
using MDPlayer.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MDPlayer.Tests;
public sealed class FailureAndIndexingTests
{
    [AvaloniaFact]
    public void FailedPreviewRestoresPaletteAliasesVariantAndSelection()
    {
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var original = appearance.Selection;
        var variant = app.RequestedThemeVariant;
        var colors = app.Resources.Where(pair => pair.Value is SolidColorBrush).ToDictionary(pair => pair.Key, pair => ((SolidColorBrush)pair.Value!).Color);
        var preferences = app.Services.GetRequiredService<IUserPreferencesStore>().Current;
        var failOnce = true;
        void Fail(object? _, EventArgs __) { if (failOnce) { failOnce = false; throw new InvalidOperationException("Injected listener failure"); } }
        appearance.BeginPreview(); appearance.Changed += Fail;
        try
        {
            Assert.Throws<InvalidOperationException>(() => appearance.Preview(new(false, "Margin Slate Dark")));
            Assert.Equal(original, appearance.Selection);
            Assert.Equal(variant, app.RequestedThemeVariant);
            Assert.Equal(preferences, app.Services.GetRequiredService<IUserPreferencesStore>().Current);
            foreach (var pair in colors) Assert.Equal(pair.Value, Assert.IsType<SolidColorBrush>(app.Resources[pair.Key]).Color);
        }
        finally { appearance.Changed -= Fail; appearance.Cancel(); }
    }

    [AvaloniaFact]
    public void SelectAllWaitsForCompleteIndexAndDoesNotCopyAPrefix()
    {
        var parser = new MarkdownParser();
        var reader = new MarkdownDocumentView();
        var window = new Window { Content = new ScrollViewer { Content = reader }, Width = 800, Height = 600 };
        reader.SetDocument(parser.Parse("First paragraph.", 4) with { IsComplete = false });
        var pending = 0; reader.IndexingPending += () => pending++;
        window.Show(); reader.Focus();
        window.KeyPressQwerty(PhysicalKey.A, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
        Assert.Empty(reader.SelectedText); Assert.Equal(1, pending);
        var full = parser.Parse("First paragraph.\n\nLast paragraph.", 4);
        reader.SetDocument(full);
        Assert.Equal(full.PlainText, reader.SelectedText);
        window.Close();
    }
}
