using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using MDPlayer.Core;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Services;
using MDPlayer.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MDPlayer.Tests;
public sealed class WorkspaceTests
{
    [AvaloniaFact]
    public async Task EighteenSkinsAcrossReadEditSplitPreserveEditorHistoryAndDocumentBytes()
    {
        var path = Path.Combine(Path.GetTempPath(), "MDPlayer-workspace-" + Guid.NewGuid().ToString("N") + ".md");
        const string source = "# A quiet moment\n\nFirst **bold** paragraph with 日本語.\n\nSecond العربية paragraph.\n\n- [x] Task\n\n```csharp\nvar greeting = \"Hello\";\n```";
        await File.WriteAllTextAsync(path, source);
        var app = (App)Application.Current!;
        var appearance = app.Services.GetRequiredService<IAppearanceService>();
        var window = app.CreateWindow();
        try
        {
            window.Show(); await window.OpenDocumentAsync(path);
            Assert.Equal(DocumentMode.Read, window.Session.Mode);
            Click(window, "EditButton");
            var editor = window.FindControl<DockPanel>("EditorHost")!.Children.OfType<TextEditor>().Single();
            editor.Document.Insert(editor.Document.TextLength, "\nAn explicit edit.");
            editor.Select(4, 5);
            var undoStack = editor.Document.UndoStack;
            var buffer = window.Session.Text;
            var reader = window.FindControl<MarkdownDocumentView>("Reader")!;
            var typography = reader.Preferences;
            appearance.BeginPreview();
            try
            {
                foreach (var skin in appearance.Catalog)
                {
                    appearance.Preview(new(false, skin.Name));
                    foreach (var mode in new[] { "ReadButton", "EditButton", "SplitButton" })
                    {
                        Click(window, mode);
                        Dispatcher.UIThread.RunJobs();
                        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                        Dispatcher.UIThread.RunJobs();
                        Assert.Equal(buffer, window.Session.Text);
                        Assert.True(window.Session.IsDirty);
                        Assert.Equal(typography, reader.Preferences);
                        Assert.Same(undoStack, editor.Document.UndoStack); Assert.True(editor.CanUndo);
                        Assert.Equal(4, editor.SelectionStart); Assert.Equal(5, editor.SelectionLength);
                        Assert.Same(editor, window.FindControl<DockPanel>("EditorHost")!.Children.OfType<TextEditor>().Single());
                    }
                }
            }
            finally { appearance.Cancel(); }
            Assert.Equal(source, await File.ReadAllTextAsync(path));
            editor.Undo(); Assert.Equal(source, editor.Text); Assert.False(window.Session.IsDirty);
        }
        finally { window.Close(); File.Delete(path); }
    }

    [Theory]
    [InlineData("e\u0301", 2)]
    [InlineData("👩🏽‍💻", 7)]
    [InlineData("🇺🇸", 4)]
    public void NavigationDoesNotSplitUnicodeTextElements(string cluster, int length)
    {
        var text = "A" + cluster + "B";
        Assert.Equal(length + 1, TextNavigation.Move(text, 1, 1));
        Assert.Equal(1, TextNavigation.Move(text, length + 1, -1));
    }

    [AvaloniaFact]
    public void FirstLineIndentPreservesDocumentSelectionAndChangesLayout()
    {
        var reader = new MarkdownDocumentView();
        var document = new MarkdownParser().Parse("A paragraph with enough words to occupy more than one line when using a narrow reading measure.", 1);
        reader.SetDocument(document);
        var window = new Window { Content = reader, Width = 600, Height = 400 };
        window.Show(); reader.Focus();
        window.KeyPressQwerty(Avalonia.Input.PhysicalKey.A, Avalonia.Input.RawInputModifiers.Control);
        reader.Preferences = reader.Preferences with { FirstLineIndent = 1.3 };
        Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
        Assert.Equal(document.PlainText, reader.SelectedText);
        Assert.True(reader.CachedLayoutCount > 0);
        window.Close();
    }
    private static void Click(MainWindow window, string name) => window.FindControl<Button>(name)!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
}
