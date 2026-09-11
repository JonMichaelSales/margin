using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;

namespace MDPlayer.Rendering;

public enum DocumentBlockKind { Paragraph, Heading, Quote, Code, Rule, TableRow, Image }
[Flags] public enum InlineStyle { Normal = 0, Bold = 1, Italic = 2, Code = 4, Strike = 8 }
public sealed record InlineRun(int Start, int Length, InlineStyle Style, string? Link = null);
public sealed record DocumentBlock(DocumentBlockKind Kind, string Text, int SourceStart, int SourceEnd,
    int Level = 0, int Indent = 0, string? Target = null, IReadOnlyList<InlineRun>? Runs = null, int TextStart = 0, bool Continuation = false, bool Continues = false);
public sealed record OutlineEntry(string Title, int Level, int BlockIndex, int SourceStart);
public sealed record ParsedDocument(long Revision, IReadOnlyList<DocumentBlock> Blocks, IReadOnlyList<OutlineEntry> Outline, string PlainText, bool IsComplete = true);
public interface IMarkdownParser
{
    ParsedDocument Parse(string source, long revision, CancellationToken cancellationToken = default);
}

public sealed class MarkdownParser : IMarkdownParser
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UsePipeTables().UseTaskLists().UseEmphasisExtras().UseAutoLinks().Build();
    public static string FirstViewportSource(string source)
    {
        if (source.Length <= 250_000) return source;
        var end = source.LastIndexOf("\n\n", 32_000, StringComparison.Ordinal);
        if (end < 8_000) end = 32_000;
        if (char.IsHighSurrogate(source[end - 1])) end--;
        return source[..end];
    }
    public ParsedDocument Parse(string source, long revision, CancellationToken cancellationToken = default)
    {
        var ast = Markdown.Parse(source, _pipeline);
        var blocks = new List<DocumentBlock>();
        var outline = new List<OutlineEntry>();
        var plain = new StringBuilder();
        void Add(DocumentBlock block)
        {
            if (blocks.Count > 0) plain.Append('\n');
            if (block.Kind == DocumentBlockKind.Heading) outline.Add(new(block.Text, block.Level, blocks.Count, block.SourceStart));
            if (block.Text.Length <= 8192) { blocks.Add(block with { TextStart = plain.Length }); plain.Append(block.Text); return; }
            for (int offset = 0; offset < block.Text.Length;)
            {
                var limit = Math.Min(offset + 8192, block.Text.Length);
                var end = offset;
                while (end < limit)
                {
                    var step = System.Globalization.StringInfo.GetNextTextElementLength(block.Text, end);
                    if (end + step > limit && end > offset) break;
                    end += step;
                }
                var length = end - offset;
                var runs = block.Runs?.Where(r => r.Start < end && r.Start + r.Length > offset)
                    .Select(r => r with { Start = Math.Max(r.Start, offset) - offset, Length = Math.Min(r.Start + r.Length, end) - Math.Max(r.Start, offset) }).ToArray();
                var chunk = block with { Text = block.Text.Substring(offset, length), TextStart = plain.Length, Runs = runs, SourceStart = Math.Min(block.SourceEnd, block.SourceStart + offset), SourceEnd = Math.Min(block.SourceEnd, block.SourceStart + end - 1), Continuation = offset > 0, Continues = end < block.Text.Length };
                blocks.Add(chunk); plain.Append(chunk.Text); offset = end;
            }
        }
        void Visit(ContainerBlock container, int indent = 0, bool quote = false, string? prefix = null)
        {
            foreach (var block in container)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (block is Table table)
                {
                    var rows = new List<(TableRow Row, string[] Cells)>();
                    foreach (TableRow row in table)
                        rows.Add((row, row.Cast<TableCell>().Select(cell => string.Join(" ", cell.OfType<LeafBlock>().Select(x => InlineText(x.Inline).Text))).ToArray()));
                    int columns = rows.Count == 0 ? 0 : rows.Max(r => r.Cells.Length);
                    var widths = Enumerable.Range(0, columns).Select(i => Math.Min(60, rows.Max(r => i < r.Cells.Length ? r.Cells[i].Length : 0))).ToArray();
                    foreach (var row in rows)
                        Add(new(DocumentBlockKind.TableRow, string.Join("  │  ", row.Cells.Select((t, i) => t.PadRight(widths[i]))), row.Row.Span.Start, row.Row.Span.End, row.Row.IsHeader ? 1 : 0));
                }
                else if (block is ListBlock list)
                {
                    int number = int.TryParse(list.OrderedStart, out var start) ? start : 1;
                    foreach (ListItemBlock item in list)
                        Visit(item, indent + 1, quote, list.IsOrdered ? $"{number++}. " : "• ");
                }
                else if (block is QuoteBlock qb) Visit(qb, indent, true);
                else if (block is CodeBlock code)
                    Add(new(DocumentBlockKind.Code, code.Lines.ToString(), block.Span.Start, block.Span.End, Indent: indent));
                else if (block is ThematicBreakBlock)
                    Add(new(DocumentBlockKind.Rule, "—", block.Span.Start, block.Span.End));
                else if (block is LeafBlock leaf)
                {
                    var (text, runs) = InlineText(leaf.Inline);
                    if (leaf is HtmlBlock) text = leaf.Lines.ToString();
                    var images = leaf.Inline?.Descendants<LinkInline>().Where(x => x.IsImage).ToArray() ?? [];
                    if (prefix is not null)
                    {
                        runs = runs.Select(r => r with { Start = r.Start + prefix.Length }).ToList();
                        text = prefix + text; prefix = null;
                    }
                    var kind = leaf is HeadingBlock ? DocumentBlockKind.Heading : quote ? DocumentBlockKind.Quote : DocumentBlockKind.Paragraph;
                    if (text.Length > 0) Add(new(kind, text, block.Span.Start, block.Span.End, (leaf as HeadingBlock)?.Level ?? 0, indent, Runs: runs));
                    foreach (var img in images) Add(new(DocumentBlockKind.Image, InlineText(img).Text, img.Span.Start, img.Span.End, Target: img.Url));
                }
                else if (block is ContainerBlock child) Visit(child, indent, quote);
            }
        }
        Visit(ast);
        return new(revision, blocks, outline, plain.ToString());
    }
    private static (string Text, List<InlineRun> Runs) InlineText(ContainerInline? root)
    {
        var text = new StringBuilder(); var runs = new List<InlineRun>();
        void Visit(Inline node, InlineStyle style = InlineStyle.Normal, string? link = null)
        {
            var start = text.Length;
            switch (node)
            {
                case LiteralInline literal: text.Append(literal.Content.ToString()); break;
                case CodeInline code: text.Append(code.Content); style |= InlineStyle.Code; break;
                case LineBreakInline line: text.Append(line.IsHard ? '\n' : ' '); break;
                case AutolinkInline auto: text.Append(auto.Url); link = auto.IsEmail ? "mailto:" + auto.Url : auto.Url; break;
                case HtmlInline html: text.Append(html.Tag); break;
                case TaskList task: text.Append(task.Checked ? "☑ " : "☐ "); break;
                case EmphasisInline emphasis:
                    var flag = emphasis.DelimiterChar == '~' ? InlineStyle.Strike : emphasis.DelimiterCount >= 2 ? InlineStyle.Bold : InlineStyle.Italic;
                    foreach (var child in emphasis) Visit(child, style | flag, link);
                    return;
                case LinkInline target:
                    foreach (var child in target) Visit(child, style, target.IsImage ? null : target.Url);
                    return;
                case ContainerInline container:
                    foreach (var child in container) Visit(child, style, link);
                    return;
            }
            if (text.Length > start) runs.Add(new(start, text.Length - start, style, link));
        }
        if (root is not null) Visit(root);
        return (text.ToString(), runs);
    }
}
