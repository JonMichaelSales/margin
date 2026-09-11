using Avalonia.Media;
using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;
public sealed class ParagraphDirectionTests
{
    [Theory]
    [InlineData("123 · العربية and English", FlowDirection.RightToLeft)]
    [InlineData("• שלום world", FlowDirection.RightToLeft)]
    [InlineData("English العربية", FlowDirection.LeftToRight)]
    [InlineData("日本語", FlowDirection.LeftToRight)]
    [InlineData("123 👩🏽‍💻", FlowDirection.LeftToRight)]
    public void FirstStrongUnicodeCharacterDeterminesParagraphDirection(string text, FlowDirection expected)
        => Assert.Equal(expected, ParagraphDirection.Detect(text));
}
