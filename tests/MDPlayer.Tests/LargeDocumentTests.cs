using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;
public sealed class LargeDocumentTests
{
    [Fact]
    public void LongParagraphsAreBoundedWithoutChangingCopyTextOrSplittingEmoji()
    {
        var source = string.Concat(Enumerable.Repeat("Long line 👩🏽‍💻 é 日本語 ", 5000));
        var parsed = new MarkdownParser().Parse(source, 7);
        Assert.True(parsed.Blocks.Count > 10);
        Assert.All(parsed.Blocks, block => Assert.InRange(block.Text.Length, 1, 8192));
        Assert.Equal(source.TrimEnd(), parsed.PlainText);
        Assert.All(parsed.Blocks, block => Assert.Equal(block.Text, parsed.PlainText.Substring(block.TextStart, block.Text.Length)));
        Assert.All(parsed.Blocks.Skip(1), block => Assert.True(block.Continuation));
        Assert.All(parsed.Blocks.SkipLast(1), block => Assert.True(block.Continues));
        Assert.All(parsed.Blocks, block => Assert.False(char.IsLowSurrogate(block.Text[0])));
    }
    [Fact]
    public void LargeFilesUseBoundedFirstViewportAndSmallFilesKeepTheirFullSource()
    {
        var small = "# A heading\n\nA paragraph.";
        Assert.Same(small, MarkdownParser.FirstViewportSource(small));
        var large = string.Concat(Enumerable.Repeat(small + "\n\n", 30_000));
        var initial = MarkdownParser.FirstViewportSource(large);
        Assert.InRange(initial.Length, 8000, 32000);
        Assert.StartsWith(initial, large);
    }
}
