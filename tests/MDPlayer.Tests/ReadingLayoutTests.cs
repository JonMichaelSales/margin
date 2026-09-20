using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using MDPlayer.Core;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Services;
using MDPlayer.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MDPlayer.Tests;
public sealed class ReadingLayoutTests
{
    [AvaloniaFact]
    public async Task FullWidthUsesWorkspaceAndPreservesDocumentAcrossSizesAndSkins()
    {
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var preferences = app.Services.GetRequiredService<IUserPreferencesStore>();
        var original = preferences.Current;
        var fixture = Path.Combine(Path.GetTempPath(), "margin-layout-" + Guid.NewGuid() + ".md");
        var output = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../artifacts/qualification/reading-layout"));
        Directory.CreateDirectory(output);
        const string source = "# A little room to read.\n\nA thoughtful workspace for **your words**, with [useful links](https://example.com) and `inline code`. Change your reading width to suit the document and the moment.\n\n## Everything in its place\n\n- A quieter toolbar\n- Reading preferences that stay with you\n- A full workspace when you need it\n\n> Good design makes room for the things that matter.\n\n```csharp\nvar document = await OpenAsync(path);\nreader.Show(document);\n```\n\n| Feature | Status |\n| --- | --- |\n| Native rendering | Ready |\n| Personal typography | Saved |\n| Full workspace | Available |\n\n---\n\nYour document stays yours.";
        await File.WriteAllTextAsync(fixture, source);
        var window = app.CreateWindow();
        appearance.BeginPreview();
        try
        {
            window.Show(); await window.OpenDocumentAsync(fixture);
            window.FindControl<Button>("TypeButton")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var reader = window.FindControl<MarkdownDocumentView>("Reader")!;
            Render(); reader.Focus();
            window.KeyPressQwerty(PhysicalKey.A, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
            var selected = reader.SelectedText;
            Assert.Equal(reader.Document.PlainText, selected);
            foreach (var skin in new[] { "Margin Paper Light", "Margin Paper Dark", "Margin Slate Dark" })
            {
                appearance.Preview(new(false, skin));
                foreach (var width in new[] { 720, 1280, 1920, 3440 })
                {
                    window.Width = width; window.Height = 1000;
                    window.FindControl<ComboBox>("WidthModeBox")!.SelectedIndex = (int)ReadingWidthMode.Full;
                    Render();
                    var available = window.FindControl<ScrollViewer>("DocumentScroll")!.Viewport.Width;
                    Assert.InRange(reader.Bounds.Width, available - 4, available + 1);
                    Assert.Equal(selected, reader.SelectedText);
                    Assert.False(window.Session.IsDirty);
                    Assert.Equal(source, window.Session.Text);
                    using var frame = window.CaptureRenderedFrame();
                    Assert.NotNull(frame);
                    frame.Save(Path.Combine(output, skin.Replace(' ', '-') + "-" + width + ".png"));
                }
            }
            reader.SetDocument(new MarkdownParser().Parse("A plain paragraph for measuring line length.", 2));
            window.Width = 1920;
            window.FindControl<ComboBox>("WidthModeBox")!.SelectedIndex = (int)ReadingWidthMode.Comfortable;
            Render(); var comfortable = reader.Bounds.Width;
            window.FindControl<ComboBox>("WidthModeBox")!.SelectedIndex = (int)ReadingWidthMode.Wide;
            Render(); Assert.True(reader.Bounds.Width > comfortable);
            window.FindControl<ComboBox>("WidthModeBox")!.SelectedIndex = (int)ReadingWidthMode.Custom;
            window.FindControl<Slider>("ReadingWidthSlider")!.Value = 160;
            Render(); Assert.Equal(160, reader.Preferences.WidthCharacters);
            Assert.Equal(source, await File.ReadAllTextAsync(fixture));
        }
        finally { appearance.Cancel(); window.Close(); preferences.Save(original); File.Delete(fixture); }
    }
    private static void Render()
    {
        Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
    }
}
