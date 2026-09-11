using System.Text;
using Avalonia.Media;
using Avalonia.Media.TextFormatting.Unicode;

namespace MDPlayer.Rendering;

public static class ParagraphDirection
{
    public static FlowDirection Detect(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            var bidi = new Codepoint((uint)rune.Value).BiDiClass;
            if (bidi is BidiClass.RightToLeft or BidiClass.ArabicLetter) return FlowDirection.RightToLeft;
            if (bidi == BidiClass.LeftToRight) return FlowDirection.LeftToRight;
        }
        return FlowDirection.LeftToRight;
    }
}
