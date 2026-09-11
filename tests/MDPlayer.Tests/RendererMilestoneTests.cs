using System.Buffers.Binary;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;

public sealed class RendererMilestoneTests
{
    [Fact]
    public void InlineAndChunkedTextMapBidirectionallyToExactSource()
    {
        const string markdown = "Before **bold** and [linked words](https://example.com) after.";
        var parsed = new MarkdownParser().Parse(markdown, 1);
        foreach (var value in new[] { "Before", "bold", "linked words", "after" })
        {
            var source = markdown.IndexOf(value, StringComparison.Ordinal);
            var rendered = parsed.PlainText.IndexOf(value, StringComparison.Ordinal);
            Assert.Equal(source, parsed.TextToSource(rendered));
            Assert.Equal(rendered, parsed.SourceToText(source));
        }

        var longMarkdown = string.Join(' ', Enumerable.Range(0, 1400).Select(index => $"**token{index:D4}**"));
        var longParsed = new MarkdownParser().Parse(longMarkdown, 2);
        Assert.True(longParsed.Blocks.Count > 1);
        const string lateToken = "token1200";
        var lateSource = longMarkdown.IndexOf(lateToken, StringComparison.Ordinal);
        var lateRendered = longParsed.PlainText.IndexOf(lateToken, StringComparison.Ordinal);
        Assert.Equal(lateSource, longParsed.TextToSource(lateRendered));
        Assert.Equal(lateRendered, longParsed.SourceToText(lateSource));
    }

    [Fact]
    public void TablesKeepSemanticCellsRichRunsImagesAndSourceMaps()
    {
        const string markdown = "| Name | Detail |\n|---|---|\n| **Alpha** | [site](https://example.com) ![logo](logo.png) |";
        var parsed = new MarkdownParser().Parse(markdown, 3);
        var block = Assert.Single(parsed.Blocks);
        Assert.Equal(DocumentBlockKind.Table, block.Kind);
        var table = Assert.IsType<TableData>(block.Table);
        Assert.Equal(2, table.Columns);
        Assert.Equal(4, table.Cells.Count);
        Assert.Equal(2, table.Cells.Count(x => x.IsHeader));
        Assert.Contains(table.Cells, cell => cell.Runs.Any(run => run.Style.HasFlag(InlineStyle.Bold)));
        Assert.Contains(table.Cells, cell => cell.Runs.Any(run => run.Link == "https://example.com"));
        Assert.Contains(table.Cells, cell => cell.Images.Any(image => image.Target == "logo.png"));
        Assert.Contains('\t', block.Text);
        Assert.Contains('\n', block.Text);
        var source = markdown.IndexOf("Alpha", StringComparison.Ordinal);
        var rendered = parsed.PlainText.IndexOf("Alpha", StringComparison.Ordinal);
        Assert.Equal(source, parsed.TextToSource(rendered));
        Assert.Equal(rendered, parsed.SourceToText(source));
    }

    [AvaloniaFact]
    public void NativeTableRendersSelectsNavigatesLinksAndExposesCells()
    {
        var prefix = string.Concat(Enumerable.Repeat("A paragraph before the table so source navigation has a measurable destination.\n\n", 30));
        var markdown = prefix + "| Name | Detail |\n|---|---|\n| Alpha | [site](https://example.com) |\n| Beta | العربية |";
        var reader = new MarkdownDocumentView();
        var parsed = new MarkdownParser().Parse(markdown, 4);
        reader.SetDocument(parsed);
        var scroll = new ScrollViewer { Content = reader };
        reader.ScrollRequested += offset => scroll.Offset = new(0, offset);
        var window = new Window { Content = scroll, Width = 720, Height = 480 };
        string? invoked = null; reader.LinkInvoked += target => invoked = target;
        window.Show(); reader.Focus();
        Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        window.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, "\t");
        window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, "\r");
        Assert.Equal("https://example.com", invoked);
        window.KeyPressQwerty(PhysicalKey.A, OperatingSystem.IsMacOS() ? RawInputModifiers.Meta : RawInputModifiers.Control);
        Assert.Equal(parsed.PlainText, reader.SelectedText);

        var documentPeer = ControlAutomationPeer.CreatePeerForElement(reader)!;
        var tablePeer = Assert.Single(documentPeer.GetChildren()!, peer => peer.GetName().StartsWith("Table,", StringComparison.Ordinal));
        Assert.Equal(6, tablePeer.GetChildren()!.Count);
        Assert.Contains("Row 1, column 1", tablePeer.GetChildren()![0].GetName());
        reader.GoToSource(markdown.IndexOf("العربية", StringComparison.Ordinal));
        Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs();
        Assert.True(scroll.Offset.Y > 0);
        using var tableFrame = window.CaptureRenderedFrame();
        Assert.NotNull(tableFrame);
        window.Close();
    }

    [Fact]
    public void ImageHeadersAreValidatedBeforeDecode()
    {
        var png = new byte[24];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(png, 0);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(16, 4), 800);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(20, 4), 600);
        Assert.Equal(new ImageDimensions(800, 600), ImageSafety.Validate(png));
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(16, 4), 20_000);
        Assert.Throws<IOException>(() => ImageSafety.Validate(png));
        Assert.Throws<IOException>(() => ImageSafety.Validate("not an image"u8));
    }

    [Fact]
    public void SupportedImageFormatsExposeDimensionsWithoutDecodingPixels()
    {
        var jpeg = new byte[] { 0xff, 0xd8, 0xff, 0xc0, 0, 7, 8, 1, 224, 2, 128, 0 };
        Assert.Equal(new ImageDimensions(640, 480), ImageSafety.Validate(jpeg));

        var gif = new byte[10];
        "GIF89a"u8.CopyTo(gif);
        BinaryPrimitives.WriteUInt16LittleEndian(gif.AsSpan(6, 2), 320);
        BinaryPrimitives.WriteUInt16LittleEndian(gif.AsSpan(8, 2), 240);
        Assert.Equal(new ImageDimensions(320, 240), ImageSafety.Validate(gif));

        var bmp = new byte[26];
        bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
        BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18, 4), 1024);
        BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22, 4), -768);
        Assert.Equal(new ImageDimensions(1024, 768), ImageSafety.Validate(bmp));

        var webp = new byte[30];
        "RIFF"u8.CopyTo(webp);
        "WEBP"u8.CopyTo(webp.AsSpan(8));
        "VP8X"u8.CopyTo(webp.AsSpan(12));
        WriteUInt24LittleEndian(webp.AsSpan(24, 3), 799);
        WriteUInt24LittleEndian(webp.AsSpan(27, 3), 599);
        Assert.Equal(new ImageDimensions(800, 600), ImageSafety.Validate(webp));
    }

    private static void WriteUInt24LittleEndian(Span<byte> target, int value)
    {
        target[0] = (byte)value;
        target[1] = (byte)(value >> 8);
        target[2] = (byte)(value >> 16);
    }
}
