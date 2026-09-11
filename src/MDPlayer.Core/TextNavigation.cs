using System.Globalization;

namespace MDPlayer.Core;

/// <summary>Selection moves by Unicode text elements, preserving combining marks and emoji sequences.</summary>
public static class TextNavigation
{
    public static int Move(string text, int offset, int direction)
    {
        offset = Math.Clamp(offset, 0, text.Length);
        if (direction > 0)
            return offset == text.Length ? offset : Math.Min(text.Length, offset + StringInfo.GetNextTextElementLength(text, offset));
        if (offset == 0) return 0;
        // Enumerate only the current paragraph, keeping navigation bounded for normal documents.
        var start = text.LastIndexOf('\n', offset - 1);
        if (start == offset - 1) return start;
        start++;
        var previous = start;
        for (var index = start; index < offset; index += StringInfo.GetNextTextElementLength(text, index)) previous = index;
        return previous;
    }
}
