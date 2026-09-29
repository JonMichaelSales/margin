using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using MDPlayer.Core;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace MDPlayer.Tests;
public sealed class EditorInteractionTests
{
    [AvaloniaFact]
    public async Task VisibleEditorAcceptsKeyboardInputAndUndoInEditAndSplitAcrossSkins()
    {
        var path = Path.Combine(Path.GetTempPath(), "margin-editor-" + Guid.NewGuid() + ".md");
        const string source = "# Editor verification\n\nVisible editable text.";
        await File.WriteAllTextAsync(path, source);
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var window = app.CreateWindow();
        appearance.BeginPreview();
        try
        {
            window.Show(); window.Width = 1440; window.Height = 900;
            await window.OpenDocumentAsync(path);
            foreach (var skin in appearance.Catalog)
            {
                appearance.Preview(new(false, skin.Name));
                foreach (var mode in new[] { "EditButton", "SplitButton" })
                {
                    window.FindControl<Button>(mode)!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Render();
                    var editor = window.FindControl<DockPanel>("EditorHost")!.Children.OfType<TextEditor>().Single();
                    Assert.NotNull(editor.Template);
                    Assert.True(editor.TextArea.Bounds.Width > 100);
                    Assert.True(editor.TextArea.TextView.Bounds.Height > 100);
                    Assert.True(editor.TextArea.TextView.VisualLinesValid);
                    Assert.NotEmpty(editor.TextArea.TextView.VisualLines);
                    Assert.Equal(((ISolidColorBrush)app.Resources["TextPrimaryBrush"]!).Color, ((ISolidColorBrush)editor.Foreground!).Color);
                    editor.TextArea.Focus();
                    window.KeyPressQwerty(PhysicalKey.End, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
                    window.KeyTextInput(" Keyboard input 日本語.");
                    Assert.EndsWith(" Keyboard input 日本語.", window.Session.Text);
                    Assert.True(window.Session.IsDirty);
                    window.KeyPressQwerty(PhysicalKey.Z, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
                    Assert.Equal(source, window.Session.Text);
                    Assert.False(window.Session.IsDirty);
                    Assert.Equal(source, await File.ReadAllTextAsync(path));
                }
            }
        }
        finally { appearance.Cancel(); window.Close(); File.Delete(path); }
    }
    private static void Render()
    {
        Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
    }
}
