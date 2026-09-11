using System.Globalization;
using System.Text;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MDPlayer.Rendering;

public enum DocumentBlockKind { Paragraph, Heading, Quote, Code, Rule, Table, Image }
[Flags] public enum InlineStyle { Normal = 0, Bold = 1, Italic = 2, Code = 4, Strike = 8 }
public sealed record InlineRun(int Start, int Length, InlineStyle Style, string? Link = null, int SourceStart = -1, int SourceEnd = -1);
public sealed record SourceMapSpan(int TextStart, int TextLength, int SourceStart, int SourceLength)
{
    public int TextEnd => TextStart + TextLength;
    public int SourceEnd => SourceStart + Math.Max(0, SourceLength - 1);
}
public sealed record DocumentImageRef(int TextStart, int TextLength, string Alt, string Target, int SourceStart, int SourceEnd);
public sealed record TableCellData(int Row, int Column, bool IsHeader, string Text, int TextStart, int SourceStart, int SourceEnd,
    IReadOnlyList<InlineRun> Runs, IReadOnlyList<SourceMapSpan> SourceMap, IReadOnlyList<DocumentImageRef> Images);
public sealed record TableData(int Columns, IReadOnlyList<TableCellData> Cells);
public sealed record DocumentBlock(DocumentBlockKind Kind, string Text, int SourceStart, int SourceEnd,
    int Level = 0, int Indent = 0, string? Target = null, IReadOnlyList<InlineRun>? Runs = null, int TextStart = 0,
    bool Continuation = false, bool Continues = false, IReadOnlyList<SourceMapSpan>? SourceMap = null,
    IReadOnlyList<DocumentImageRef>? Images = null, TableData? Table = null);
public sealed record OutlineEntry(string Title, int Level, int BlockIndex, int SourceStart);
public sealed record ParsedDocument(long Revision, IReadOnlyList<DocumentBlock> Blocks, IReadOnlyList<OutlineEntry> Outline, string PlainText, bool IsComplete = true)
{
    public int BlockIndexAtText(int textPosition)
    {
        if (Blocks.Count == 0) return 0;
        textPosition = Math.Clamp(textPosition, 0, PlainText.Length);
        var low = 0; var high = Blocks.Count - 1;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            if (Blocks[middle].TextStart <= textPosition) low = middle + 1; else high = middle - 1;
        }
        return Math.Clamp(high, 0, Blocks.Count - 1);
    }

    public int TextToSource(int textPosition)
    {
        if (Blocks.Count == 0) return 0;
        var block = Blocks[BlockIndexAtText(textPosition)];
        var local = Math.Clamp(textPosition - block.TextStart, 0, block.Text.Length);
        var maps = block.SourceMap ?? [];
        var map = maps.FirstOrDefault(x => local >= x.TextStart && local < x.TextEnd);
        if (map is null && maps.Count > 0) map = maps.MinBy(x => Math.Min(Math.Abs(local - x.TextStart), Math.Abs(local - x.TextEnd)));
        if (map is null || map.SourceLength <= 0 || map.TextLength <= 0) return block.SourceStart;
        var offset = Math.Clamp(local - map.TextStart, 0, map.TextLength);
        return map.SourceStart + Math.Min(map.SourceLength - 1, (int)Math.Round(offset * (map.SourceLength - 1d) / Math.Max(1, map.TextLength - 1)));
    }

    public int SourceToText(int sourcePosition)
    {
        if (Blocks.Count == 0) return 0;
        SourceMapSpan? best = null; DocumentBlock? bestBlock = null; var bestDistance = int.MaxValue;
        foreach (var block in Blocks)
        {
            foreach (var map in block.SourceMap ?? [])
            {
                var distance = sourcePosition < map.SourceStart ? map.SourceStart - sourcePosition : sourcePosition > map.SourceEnd ? sourcePosition - map.SourceEnd : 0;
                if (distance >= bestDistance) continue;
                best = map; bestBlock = block; bestDistance = distance;
                if (distance == 0) break;
            }
            if (bestDistance == 0) break;
        }
        if (best is null || bestBlock is null)
        {
            var block = Blocks.MinBy(x => sourcePosition < x.SourceStart ? x.SourceStart - sourcePosition : sourcePosition > x.SourceEnd ? sourcePosition - x.SourceEnd : 0)!;
            return block.TextStart;
        }
        var sourceOffset = Math.Clamp(sourcePosition - best.SourceStart, 0, Math.Max(0, best.SourceLength - 1));
        var textOffset = best.SourceLength <= 1 ? 0 : (int)Math.Round(sourceOffset * Math.Max(0, best.TextLength - 1d) / (best.SourceLength - 1));
        return Math.Clamp(bestBlock.TextStart + best.TextStart + textOffset, 0, PlainText.Length);
    }
}

public interface IMarkdownParser
{
    ParsedDocument Parse(string source, long revision, CancellationToken cancellationToken = default);
}

public sealed class MarkdownParser : IMarkdownParser
{
    private const int MaximumBlockCharacters = 8192;
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
            if (block.Kind == DocumentBlockKind.Table || block.Text.Length <= MaximumBlockCharacters)
            {
                blocks.Add(block with { TextStart = plain.Length }); plain.Append(block.Text); return;
            }
            for (var offset = 0; offset < block.Text.Length;)
            {
                var limit = Math.Min(offset + MaximumBlockCharacters, block.Text.Length);
                var end = offset;
                while (end < limit)
                {
                    var step = StringInfo.GetNextTextElementLength(block.Text, end);
                    if (end + step > limit && end > offset) break;
                    end += step;
                }
                var runs = SliceRuns(block.Runs, offset, end);
                var maps = SliceMaps(block.SourceMap, offset, end);
                var images = SliceImages(block.Images, offset, end);
                var sourceStart = maps.Count > 0 ? maps.Min(x => x.SourceStart) : block.SourceStart;
                var sourceEnd = maps.Count > 0 ? maps.Max(x => x.SourceEnd) : block.SourceEnd;
                var chunk = block with
                {
                    Text = block.Text[offset..end], TextStart = plain.Length, Runs = runs, SourceMap = maps, Images = images,
                    SourceStart = sourceStart, SourceEnd = sourceEnd, Continuation = offset > 0, Continues = end < block.Text.Length
                };
                blocks.Add(chunk); plain.Append(chunk.Text); offset = end;
            }
        }

        void Visit(ContainerBlock container, int indent = 0, bool quote = false, string? prefix = null, int prefixSource = -1)
        {
            foreach (var block in container)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (block is Table table) Add(CreateTable(table, source));
                else if (block is ListBlock list)
                {
                    var number = int.TryParse(list.OrderedStart, out var start) ? start : 1;
                    foreach (ListItemBlock item in list)
                    {
                        var marker = list.IsOrdered ? $"{number++}. " : "• ";
                        Visit(item, indent + 1, quote, marker, Math.Max(0, item.Span.Start));
                    }
                }
                else if (block is QuoteBlock quoteBlock) Visit(quoteBlock, indent, true);
                else if (block is CodeBlock code)
                {
                    var text = code.Lines.ToString();
                    Add(new(DocumentBlockKind.Code, text, block.Span.Start, block.Span.End, Indent: indent,
                        SourceMap: MapWholeText(source, text, block.Span.Start, block.Span.End)));
                }
                else if (block is ThematicBreakBlock)
                    Add(new(DocumentBlockKind.Rule, "—", block.Span.Start, block.Span.End,
                        SourceMap: [new(0, 1, block.Span.Start, SpanLength(block.Span.Start, block.Span.End))]));
                else if (block is LeafBlock leaf)
                {
                    var content = InlineText(leaf.Inline, source);
                    if (leaf is HtmlBlock)
                    {
                        var html = leaf.Lines.ToString();
                        content = new(html, [], [.. MapWholeText(source, html, block.Span.Start, block.Span.End)], []);
                    }
                    if (prefix is not null)
                    {
                        content.Offset(prefix.Length);
                        content.Text = prefix + content.Text;
                        content.Maps.Insert(0, new(0, prefix.Length, prefixSource < 0 ? block.Span.Start : prefixSource, Math.Max(1, prefix.Length)));
                        prefix = null;
                    }
                    var kind = leaf is HeadingBlock ? DocumentBlockKind.Heading : quote ? DocumentBlockKind.Quote : DocumentBlockKind.Paragraph;
                    if (content.Images.Count > 0 && content.Text.Trim().Length == content.Images.Sum(x => x.Alt.Length)) kind = DocumentBlockKind.Image;
                    if (content.Text.Length > 0 || content.Images.Count > 0)
                        Add(new(kind, content.Text, block.Span.Start, block.Span.End, (leaf as HeadingBlock)?.Level ?? 0, indent,
                            Target: content.Images.FirstOrDefault()?.Target, Runs: content.Runs, SourceMap: content.Maps, Images: content.Images));
                }
                else if (block is ContainerBlock child) Visit(child, indent, quote);
            }
        }

        Visit(ast);
        return new(revision, blocks, outline, plain.ToString());
    }

    private static DocumentBlock CreateTable(Table table, string source)
    {
        var cells = new List<TableCellData>();
        var tableText = new StringBuilder();
        var tableMaps = new List<SourceMapSpan>();
        var rowIndex = 0; var columns = 0;
        var rows = table.OfType<TableRow>().ToArray();
        foreach (var row in rows)
        {
            var columnIndex = 0;
            foreach (var cell in row.Cast<TableCell>())
            {
                if (columnIndex > 0) tableText.Append('\t');
                var cellTextStart = tableText.Length;
                var cellContent = new InlineContent("", [], [], []);
                foreach (var leaf in cell.OfType<LeafBlock>())
                {
                    var part = InlineText(leaf.Inline, source);
                    if (cellContent.Text.Length > 0) cellContent.Append("\n", leaf.Span.Start, 1);
                    cellContent.Append(part);
                }
                tableText.Append(cellContent.Text);
                tableMaps.AddRange(cellContent.Maps.Select(x => x with { TextStart = x.TextStart + cellTextStart }));
                cells.Add(new(rowIndex, columnIndex, row.IsHeader, cellContent.Text, cellTextStart, cell.Span.Start, cell.Span.End,
                    cellContent.Runs, cellContent.Maps, cellContent.Images));
                columnIndex++;
            }
            columns = Math.Max(columns, columnIndex);
            rowIndex++;
            if (rowIndex < rows.Length) tableText.Append('\n');
        }
        return new(DocumentBlockKind.Table, tableText.ToString(), table.Span.Start, table.Span.End,
            SourceMap: tableMaps, Images: cells.SelectMany(x => x.Images.Select(image => image with { TextStart = image.TextStart + x.TextStart })).ToArray(),
            Table: new(columns, cells));
    }

    private static InlineContent InlineText(ContainerInline? root, string source)
    {
        var result = new InlineContent("", [], [], []);
        void Visit(Inline node, InlineStyle style = InlineStyle.Normal, string? link = null)
        {
            switch (node)
            {
                case LiteralInline literal: result.Append(literal.Content.ToString(), node, source, style, link); break;
                case CodeInline code: result.Append(code.Content, node, source, style | InlineStyle.Code, link); break;
                case LineBreakInline line: result.Append(line.IsHard ? "\n" : " ", node, source, style, link); break;
                case AutolinkInline auto: result.Append(auto.Url, node, source, style, auto.IsEmail ? "mailto:" + auto.Url : auto.Url); break;
                case HtmlEntityInline entity: result.Append(entity.Transcoded.ToString(), node, source, style, link); break;
                case HtmlInline html: result.Append(html.Tag, node, source, style, link); break;
                case TaskList task: result.Append(task.Checked ? "☑ " : "☐ ", node, source, style, link); break;
                case EmphasisInline emphasis:
                    var flag = emphasis.DelimiterChar == '~' ? InlineStyle.Strike : emphasis.DelimiterCount >= 2 ? InlineStyle.Bold : InlineStyle.Italic;
                    foreach (var child in emphasis) Visit(child, style | flag, link);
                    break;
                case LinkInline target when target.IsImage:
                    var alt = new StringBuilder();
                    foreach (var child in target.Descendants<LiteralInline>()) alt.Append(child.Content.ToString());
                    var renderedAlt = alt.Length == 0 ? "Image" : alt.ToString();
                    var textStart = result.Text.Length;
                    result.Append(renderedAlt, node, source, style, null);
                    if (!string.IsNullOrWhiteSpace(target.Url)) result.Images.Add(new(textStart, renderedAlt.Length, renderedAlt, target.Url!, node.Span.Start, node.Span.End));
                    break;
                case LinkInline target:
                    foreach (var child in target) Visit(child, style, target.Url);
                    break;
                case ContainerInline container:
                    foreach (var child in container) Visit(child, style, link);
                    break;
            }
        }
        if (root is not null) Visit(root);
        return result;
    }

    private static IReadOnlyList<InlineRun> SliceRuns(IReadOnlyList<InlineRun>? runs, int start, int end) => (runs ?? [])
        .Where(x => x.Start < end && x.Start + x.Length > start)
        .Select(x => x with { Start = Math.Max(x.Start, start) - start, Length = Math.Min(x.Start + x.Length, end) - Math.Max(x.Start, start) }).ToArray();

    private static IReadOnlyList<SourceMapSpan> SliceMaps(IReadOnlyList<SourceMapSpan>? maps, int start, int end) => (maps ?? [])
        .Where(x => x.TextStart < end && x.TextEnd > start)
        .Select(x =>
        {
            var overlapStart = Math.Max(x.TextStart, start); var overlapEnd = Math.Min(x.TextEnd, end);
            var sourceOffset = x.TextLength <= 1 ? 0 : (int)Math.Round((overlapStart - x.TextStart) * (x.SourceLength - 1d) / Math.Max(1, x.TextLength - 1));
            var sourceEndOffset = x.TextLength <= 1 ? sourceOffset : (int)Math.Round((Math.Max(overlapStart, overlapEnd - 1) - x.TextStart) * (x.SourceLength - 1d) / Math.Max(1, x.TextLength - 1));
            return new SourceMapSpan(overlapStart - start, overlapEnd - overlapStart, x.SourceStart + sourceOffset, Math.Max(1, sourceEndOffset - sourceOffset + 1));
        }).ToArray();

    private static IReadOnlyList<DocumentImageRef> SliceImages(IReadOnlyList<DocumentImageRef>? images, int start, int end) => (images ?? [])
        .Where(x => x.TextStart < end && x.TextStart + x.TextLength > start)
        .Select(x => x with { TextStart = Math.Max(x.TextStart, start) - start }).ToArray();

    private static IReadOnlyList<SourceMapSpan> MapWholeText(string source, string text, int sourceStart, int sourceEnd)
    {
        if (text.Length == 0) return [];
        sourceStart = Math.Clamp(sourceStart, 0, Math.Max(0, source.Length - 1));
        sourceEnd = Math.Clamp(sourceEnd, sourceStart, Math.Max(sourceStart, source.Length - 1));
        var slice = sourceStart < source.Length ? source.Substring(sourceStart, Math.Min(source.Length - sourceStart, sourceEnd - sourceStart + 1)) : "";
        var offset = slice.IndexOf(text, StringComparison.Ordinal);
        return [new(0, text.Length, offset >= 0 ? sourceStart + offset : sourceStart, offset >= 0 ? text.Length : Math.Max(1, sourceEnd - sourceStart + 1))];
    }

    private static int SpanLength(int start, int end) => Math.Max(1, end - start + 1);

    private sealed class InlineContent(string text, List<InlineRun> runs, List<SourceMapSpan> maps, List<DocumentImageRef> images)
    {
        public string Text { get; set; } = text;
        public List<InlineRun> Runs { get; } = runs;
        public List<SourceMapSpan> Maps { get; } = maps;
        public List<DocumentImageRef> Images { get; } = images;

        public void Offset(int amount)
        {
            for (var i = 0; i < Runs.Count; i++) Runs[i] = Runs[i] with { Start = Runs[i].Start + amount };
            for (var i = 0; i < Maps.Count; i++) Maps[i] = Maps[i] with { TextStart = Maps[i].TextStart + amount };
            for (var i = 0; i < Images.Count; i++) Images[i] = Images[i] with { TextStart = Images[i].TextStart + amount };
        }

        public void Append(InlineContent other)
        {
            var offset = Text.Length; Text += other.Text;
            Runs.AddRange(other.Runs.Select(x => x with { Start = x.Start + offset }));
            Maps.AddRange(other.Maps.Select(x => x with { TextStart = x.TextStart + offset }));
            Images.AddRange(other.Images.Select(x => x with { TextStart = x.TextStart + offset }));
        }

        public void Append(string value, int sourceStart, int sourceLength)
        {
            if (value.Length == 0) return;
            var start = Text.Length; Text += value; Maps.Add(new(start, value.Length, sourceStart, Math.Max(1, sourceLength)));
        }

        public void Append(string value, Inline node, string source, InlineStyle style, string? link)
        {
            if (value.Length == 0) return;
            var textStart = Text.Length; Text += value;
            var sourceStart = Math.Clamp(node.Span.Start, 0, Math.Max(0, source.Length - 1));
            var sourceEnd = Math.Clamp(node.Span.End, sourceStart, Math.Max(sourceStart, source.Length - 1));
            var slice = sourceStart < source.Length ? source.Substring(sourceStart, Math.Min(source.Length - sourceStart, sourceEnd - sourceStart + 1)) : "";
            var exact = slice.IndexOf(value, StringComparison.Ordinal);
            if (exact >= 0) { sourceStart += exact; sourceEnd = sourceStart + value.Length - 1; }
            Runs.Add(new(textStart, value.Length, style, link, sourceStart, sourceEnd));
            Maps.Add(new(textStart, value.Length, sourceStart, SpanLength(sourceStart, sourceEnd)));
        }
    }
}
