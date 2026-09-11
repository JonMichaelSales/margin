using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using MDPlayer.Desktop;
using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;
public sealed class NativeRenderTests
{
    [AvaloniaFact]
    public void CompositorCanRenderAndScrollARealDocumentWithoutInvalidatingDuringRender()
    {
        var reader = new MarkdownDocumentView();
        reader.SetDocument(new MarkdownParser().Parse(string.Concat(Enumerable.Repeat("# Heading\n\nA longer paragraph with **bold**, العربية, 日本語, and 👩🏽‍💻 text that wraps across several native text lines.\n\n", 150)), 1));
        var scroll = new ScrollViewer { Content = reader };
        var window = new Window { Content = scroll, Width = 900, Height = 600 };
        scroll.ScrollChanged += (_, _) => reader.SetViewport(scroll.Offset.Y, scroll.Viewport.Height);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        scroll.Offset = new Vector(0, 8000);
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        Assert.True(reader.VisibleSourceStart > 0);
        window.Close();
    }
}
