using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using MDPlayer.Core;

namespace MDPlayer.Rendering;

/// <summary>Native document surface. Selection indexes belong to the entire document, not recycled visuals.</summary>
public readonly record struct ReadingAnchor(int TextPosition, double ViewportOffset);
public sealed class MarkdownDocumentView : Control
{
    private DocumentImageCache? _images;
    private string? _imageDocumentPath;
    public string? DocumentPath { get; set; }
    public event Action<string>? RemoteImageRequested;
    public void AllowRemoteImage(string target) => _images?.AllowRemote(target);
    public int CachedLayoutCount => _layouts.Count;
    private int PrefixLength(DocumentBlock block) => block.Kind == DocumentBlockKind.Paragraph && !block.Continuation && block.Indent == 0 && _preferences.FirstLineIndent > 0 ? 1 : 0;
    private ParsedDocument _document = new(0, [], [], "");
    private ReadingPreferences _preferences = new();
    private ReadingAnchor? _pendingAnchor;
    private readonly Dictionary<int, TextLayout> _layouts = new();
    private readonly Dictionary<int, TableVisualLayout> _tableLayouts = new();
    private double[] _tops = [0], _heights = [];
    private double _layoutWidth, _viewportTop, _viewportHeight = 900;
    private int _selectionAnchor, _selectionEnd, _activeLinkIndex = -1;
    private bool _dragging, _measurePending, _selectAllWhenComplete;
    public event Action? IndexingPending;
    public event Action<string>? LinkInvoked;
    public event Action<int>? SourcePositionChanged;
    public event Action<double>? ScrollRequested;
    public ParsedDocument Document => _document;
    public ReadingPreferences Preferences { get => _preferences; set { _pendingAnchor = CaptureReadingAnchor(); _preferences = value.Sanitize(); ResetLayout(); } }
    public string SelectedText
    {
        get { var start = Math.Min(_selectionAnchor, _selectionEnd); var length = Math.Abs(_selectionEnd - _selectionAnchor); return _document.PlainText.Substring(start, length); }
    }
    public MarkdownDocumentView()
    {
        Focusable = true; Cursor = new Cursor(StandardCursorType.Ibeam);
        AutomationProperties.SetName(this, "Rendered Markdown document");
        AutomationProperties.SetHelpText(this, "Read-only document. Use arrow keys to move, Shift to select, and Control or Command C to copy.");
        ClipToBounds = true;
    }
    public void SetDocument(ParsedDocument document)
    {
        if (_images is null || !string.Equals(_imageDocumentPath, DocumentPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            _images?.Dispose(); _imageDocumentPath = DocumentPath; _images = new() { DocumentPath = DocumentPath };
            _images.Changed += () => { _pendingAnchor ??= CaptureReadingAnchor(); ClearLayouts(); InvalidateMeasure(); InvalidateVisual(); };
        }
        var sameRevision = _document.Revision == document.Revision; _document = document; _selectionAnchor = sameRevision ? Math.Min(_selectionAnchor, document.PlainText.Length) : 0; _selectionEnd = sameRevision ? Math.Min(_selectionEnd, document.PlainText.Length) : 0; if (!sameRevision) _activeLinkIndex = -1; if (_selectAllWhenComplete && sameRevision && document.IsComplete) { _selectionAnchor = 0; _selectionEnd = document.PlainText.Length; }
        if (!sameRevision || document.IsComplete) _selectAllWhenComplete = false;
        ResetLayout(); (ControlAutomationPeer.FromElement(this) as DocumentAutomationPeer)?.Refresh();
    }
    public void SetViewport(double top, double height)
    {
        _viewportTop = Math.Max(0, top); _viewportHeight = Math.Max(200, height); InvalidateVisual();
    }
    public ReadingAnchor CaptureReadingAnchor()
    {
        if (_document.Blocks.Count == 0 || _heights.Length != _document.Blocks.Count || _layoutWidth <= 0) return default;
        var hit = Hit(new Point(32, Math.Max(24, _viewportTop)));
        var block = _document.Blocks[hit.Index];
        var rect = PositionRect(hit.Index, hit.Position);
        return new(block.TextStart + hit.Position, _tops[hit.Index] + 24 + rect.Y - _viewportTop);
    }
    public void RestoreReadingAnchor(ReadingAnchor anchor)
    {
        if (_document.Blocks.Count == 0 || _heights.Length != _document.Blocks.Count || _layoutWidth <= 0) { _pendingAnchor = anchor; return; }
        var index = _document.BlockIndexAtText(anchor.TextPosition);
        var block = _document.Blocks[index];
        var rect = PositionRect(index, Math.Clamp(anchor.TextPosition - block.TextStart, 0, block.Text.Length));
        ScrollRequested?.Invoke(Math.Max(0, _tops[index] + 24 + rect.Y - anchor.ViewportOffset));
    }
    public int VisibleSourceStart
    {
        get
        {
            if (_document.Blocks.Count == 0) return 0;
            var hit = Hit(new Point(32, Math.Max(24, _viewportTop + 24)));
            return _document.TextToSource(_document.Blocks[hit.Index].TextStart + hit.Position);
        }
    }
    public void GoToSource(int source)
    {
        if (_document.Blocks.Count == 0) return;
        var textPosition = _document.SourceToText(source);
        var index = _document.BlockIndexAtText(textPosition);
        var block = _document.Blocks[index];
        var rect = PositionRect(index, Math.Clamp(textPosition - block.TextStart, 0, block.Text.Length));
        ScrollRequested?.Invoke(Math.Max(0, _tops[Math.Min(index, _tops.Length - 1)] + 24 + rect.Y));
        InvalidateVisual();
    }
    public bool Find(string query, bool backwards = false)
    {
        if (string.IsNullOrWhiteSpace(query) || _document.PlainText.Length == 0) return false;
        var text = _document.PlainText;
        var position = backwards ? text.LastIndexOf(query, Math.Max(0, Math.Min(text.Length - 1, Math.Min(_selectionAnchor, _selectionEnd) - 1)), StringComparison.OrdinalIgnoreCase)
            : text.IndexOf(query, Math.Min(text.Length, Math.Max(_selectionAnchor, _selectionEnd)), StringComparison.OrdinalIgnoreCase);
        if (position < 0) position = backwards ? text.LastIndexOf(query, StringComparison.OrdinalIgnoreCase) : text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (position < 0) return false;
        _selectionAnchor = position; _selectionEnd = position + query.Length; RevealSelection(); return true;
    }
    private void ClearLayouts()
    {
        foreach (var layout in _layouts.Values) layout.Dispose();
        foreach (var layout in _tableLayouts.Values) layout.Dispose();
        _layouts.Clear(); _tableLayouts.Clear();
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) { _images?.Dispose(); ClearLayouts(); base.OnDetachedFromVisualTree(e); }
    public void RefreshColors() { ClearLayouts(); InvalidateVisual(); }
    private IBrush Brush(string name) => this.TryFindResource(name, out var resource) && resource is IBrush brush ? brush : throw new InvalidOperationException("Missing skin resource: " + name);
    private void ResetLayout()
    {
        ClearLayouts(); _layoutWidth = 0; InvalidateMeasure(); InvalidateVisual();
    }
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = Math.Max(160, Math.Min(double.IsFinite(availableSize.Width) ? availableSize.Width : 800, _preferences.WidthCharacters * _preferences.FontSize * .54 + 64));
        if (Math.Abs(width - _layoutWidth) > .5)
        {
            if (_layoutWidth > 0) _pendingAnchor ??= CaptureReadingAnchor();
            _layoutWidth = width; ClearLayouts(); _heights = new double[_document.Blocks.Count]; _tops = new double[_heights.Length + 1];
            for (int i = 0; i < _heights.Length; i++)
            {
                var block = _document.Blocks[i]; var font = FontSizeFor(block);
                var chars = Math.Max(10, (width - 64 - block.Indent * 20) / (font * .53));
                _heights[i] = block.Kind switch
                {
                    DocumentBlockKind.Rule => 30,
                    DocumentBlockKind.Table => Math.Max(1, block.Table?.Cells.Select(x => x.Row).Distinct().Count() ?? 1) * (font * _preferences.LineHeight + 18) + Gap(block),
                    _ => Math.Max(1, block.Text.Split('\n').Sum(line => Math.Max(1, Math.Ceiling(line.Length / chars)))) * font * _preferences.LineHeight + Gap(block) + ImageAreaHeight(block.Images)
                };
                _tops[i + 1] = _tops[i] + _heights[i];
            }
        }
        if (_pendingAnchor is { } anchor)
        {
            _pendingAnchor = null;
            Avalonia.Threading.Dispatcher.UIThread.Post(() => RestoreReadingAnchor(anchor), Avalonia.Threading.DispatcherPriority.Loaded);
        }
        return new Size(width, _tops[^1] + 64);
    }
    private double FontSizeFor(DocumentBlock block) => block.Kind switch
    {
        DocumentBlockKind.Heading => _preferences.FontSize * (block.Level == 1 ? 1.9 : block.Level == 2 ? 1.45 : 1.18),
        DocumentBlockKind.Code or DocumentBlockKind.Table => _preferences.FontSize * .83, _ => _preferences.FontSize
    };
    private double Gap(DocumentBlock block) => block.Continues ? 0 : _preferences.FontSize * _preferences.ParagraphGap + (block.Kind == DocumentBlockKind.Heading ? 18 : 8);
    private TextLayout Layout(int i)
    {
        if (_layouts.TryGetValue(i, out var existing)) return existing;
        var block = _document.Blocks[i]; var fontSize = FontSizeFor(block);
        var mono = block.Kind == DocumentBlockKind.Code;
        var family = new FontFamily(mono ? "Cascadia Mono, Menlo, monospace" : _preferences.FontFamily);
        var typeface = new Typeface(family, block.Kind == DocumentBlockKind.Quote ? FontStyle.Italic : FontStyle.Normal,
            block.Kind == DocumentBlockKind.Heading ? FontWeight.SemiBold : FontWeight.Normal);
        var color = Brush(block.Kind == DocumentBlockKind.Quote ? "AccentBlueBrush" : "TextPrimaryBrush");
        var spans = new List<ValueSpan<TextRunProperties>>();
        foreach (var run in block.Runs ?? [])
        {
            var face = new Typeface(run.Style.HasFlag(InlineStyle.Code) ? new FontFamily("Cascadia Mono, Menlo, monospace") : family,
                run.Style.HasFlag(InlineStyle.Italic) ? FontStyle.Italic : typeface.Style,
                run.Style.HasFlag(InlineStyle.Bold) ? FontWeight.Bold : typeface.Weight);
            spans.Add(new(run.Start + PrefixLength(block), run.Length, new GenericTextRunProperties(face, fontSize,
                textDecorations: run.Style.HasFlag(InlineStyle.Strike) ? TextDecorations.Strikethrough : run.Link is not null ? TextDecorations.Underline : null,
                foregroundBrush: run.Link is not null ? Brush("AccentBlueBrush") : color)));
        }
        var prefix = PrefixLength(block);
        if (prefix > 0) spans.Insert(0, new(0, 1, new GenericTextRunProperties(typeface, fontSize * _preferences.FirstLineIndent, foregroundBrush: color)));
        var text = (prefix > 0 ? "\u2003" : "") + block.Text;
        if (block.Kind == DocumentBlockKind.Image && block.Target is { } target)
            text += _images?.Get(target) is not null ? "" : "\n" + (_images?.Error(target) ?? (DocumentImageCache.IsRemote(target) ? "Remote image · click to load" : "Loading local image…"));
        var layout = new TextLayout(text, typeface, fontSize, color,
            textAlignment: _preferences.Justified && block.Kind == DocumentBlockKind.Paragraph ? TextAlignment.Justify : TextAlignment.Start,
            flowDirection: mono ? FlowDirection.LeftToRight : ParagraphDirection.Detect(block.Text), textWrapping: TextWrapping.Wrap, maxWidth: Math.Max(100, _layoutWidth - 64 - block.Indent * 20),
            lineHeight: fontSize * _preferences.LineHeight, textStyleOverrides: spans);
        _layouts[i] = layout;
        UpdateMeasuredHeight(i, layout.Height + Gap(block) + ImageAreaHeight(block.Images));
        return layout;
    }
    private static double ImageAreaHeight(IReadOnlyList<DocumentImageRef>? images) => (images?.Count ?? 0) * 280;
    private void UpdateMeasuredHeight(int index, double height)
    {
        if (index >= _heights.Length || Math.Abs(height - _heights[index]) <= .5) return;
        var delta = height - _heights[index]; _heights[index] = height;
        for (var j = index + 1; j < _tops.Length; j++) _tops[j] += delta;
        if (_measurePending) return;
        _measurePending = true;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => { _measurePending = false; InvalidateMeasure(); InvalidateVisual(); });
    }
    private TableVisualLayout TableLayout(int index)
    {
        if (_tableLayouts.TryGetValue(index, out var existing)) return existing;
        var block = _document.Blocks[index]; var table = block.Table ?? throw new InvalidOperationException("Table block is missing table data.");
        var availableWidth = Math.Max(160, _layoutWidth - 64 - block.Indent * 20);
        var columns = Math.Max(1, table.Columns);
        var weights = Enumerable.Range(0, columns).Select(column =>
            Math.Clamp(table.Cells.Where(x => x.Column == column).Select(x => x.Text.Length).DefaultIfEmpty(8).Max(), 8, 48)).ToArray();
        var weightTotal = weights.Sum();
        var widths = weights.Select(weight => availableWidth * weight / weightTotal).ToArray();
        var visuals = new List<TableCellVisual>(); var top = 0d;
        foreach (var row in table.Cells.GroupBy(x => x.Row).OrderBy(x => x.Key))
        {
            var rowCells = new List<(TableCellData Cell, TextLayout Layout, double Left)>();
            foreach (var cell in row.OrderBy(x => x.Column))
            {
                var width = widths[Math.Min(cell.Column, widths.Length - 1)];
                var left = widths.Take(Math.Min(cell.Column, widths.Length)).Sum();
                rowCells.Add((cell, CreateCellTextLayout(cell.Text, cell.Runs, cell.IsHeader, width - 16), left));
            }
            if (rowCells.Count == 0) continue;
            var rowHeight = Math.Max(FontSizeFor(block) * _preferences.LineHeight + 16,
                rowCells.Max(x => x.Layout.Height + 16 + x.Cell.Images.Count * 112));
            foreach (var item in rowCells)
            {
                var width = widths[Math.Min(item.Cell.Column, widths.Length - 1)];
                visuals.Add(new(item.Cell, item.Layout, new Rect(item.Left, top, width, rowHeight)));
            }
            top += rowHeight;
        }
        var created = new TableVisualLayout(visuals, top);
        _tableLayouts[index] = created; UpdateMeasuredHeight(index, top + Gap(block));
        return created;
    }
    private TextLayout CreateCellTextLayout(string text, IReadOnlyList<InlineRun> runs, bool header, double width)
    {
        var fontSize = _preferences.FontSize * .83; var family = new FontFamily(_preferences.FontFamily);
        var typeface = new Typeface(family, FontStyle.Normal, header ? FontWeight.SemiBold : FontWeight.Normal);
        var color = Brush("TextPrimaryBrush"); var spans = new List<ValueSpan<TextRunProperties>>();
        foreach (var run in runs)
        {
            var face = new Typeface(run.Style.HasFlag(InlineStyle.Code) ? new FontFamily("Cascadia Mono, Menlo, monospace") : family,
                run.Style.HasFlag(InlineStyle.Italic) ? FontStyle.Italic : FontStyle.Normal,
                run.Style.HasFlag(InlineStyle.Bold) || header ? FontWeight.Bold : FontWeight.Normal);
            spans.Add(new(run.Start, run.Length, new GenericTextRunProperties(face, fontSize,
                textDecorations: run.Style.HasFlag(InlineStyle.Strike) ? TextDecorations.Strikethrough : run.Link is not null ? TextDecorations.Underline : null,
                foregroundBrush: run.Link is not null ? Brush("AccentBlueBrush") : color)));
        }
        return new TextLayout(text, typeface, fontSize, color, textWrapping: TextWrapping.Wrap,
            maxWidth: Math.Max(40, width), lineHeight: fontSize * _preferences.LineHeight, textStyleOverrides: spans,
            flowDirection: ParagraphDirection.Detect(text));
    }
    protected override void OnSizeChanged(SizeChangedEventArgs e) { base.OnSizeChanged(e); InvalidateVisual(); }
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_document.Blocks.Count == 0 || _heights.Length != _document.Blocks.Count) return;
        var start = BlockAt(Math.Max(0, _viewportTop - 200));
        var last = start;
        _images?.Retain(_document.Blocks.Skip(start).TakeWhile((_, offset) => _tops[start + offset] < _viewportTop + _viewportHeight + 400)
            .SelectMany(b => b.Images ?? []).Select(x => x.Target).ToHashSet());
        for (int i = start; i < _document.Blocks.Count && _tops[i] < _viewportTop + _viewportHeight + 400; i++)
        {
            last = i; var block = _document.Blocks[i]; var y = _tops[i] + 24; var x = 32 + block.Indent * 20;
            if (block.Kind == DocumentBlockKind.Table)
            {
                var table = TableLayout(i);
                foreach (var cell in table.Cells)
                {
                    var bounds = cell.Bounds.Translate(new Vector(x, y));
                    context.DrawRectangle(cell.Cell.IsHeader ? Brush("SecondaryColorBrush") : Brush("BackgroundLightBrush"), new Pen(Brush("BorderBrush"), 1), bounds);
                    DrawSelection(context, cell.Layout, block, cell.Cell.TextStart, cell.Cell.Text.Length, new Point(bounds.X + 8, bounds.Y + 8));
                    cell.Layout.Draw(context, new Point(bounds.X + 8, bounds.Y + 8));
                    var imageTop = bounds.Y + 8 + cell.Layout.Height;
                    foreach (var image in cell.Cell.Images)
                    {
                        DrawImagePanel(context, image.Target, new Rect(bounds.X + 8, imageTop + 8, Math.Max(40, bounds.Width - 16), 96));
                        imageTop += 112;
                    }
                }
                continue;
            }
            var layout = Layout(i);
            if (block.Kind == DocumentBlockKind.Code)
                context.DrawRectangle(Brush("BackgroundLightBrush"), null, new Rect(x - 12, y - 8, Math.Max(100, _layoutWidth - x - 20), layout.Height + 16), 6, 6);
            if (block.Kind == DocumentBlockKind.Quote) context.DrawRectangle(Brush("AccentBlueBrush"), null, new Rect(x - 16, y, 3, layout.Height));
            if (block.Kind == DocumentBlockKind.Rule) context.DrawLine(new Pen(Brush("BorderBrush"), 1), new Point(x, y + 8), new Point(_layoutWidth - 32, y + 8));
            else
            {
                DrawSelection(context, layout, block, 0, block.Text.Length, new Point(x, y), PrefixLength(block));
                layout.Draw(context, new Point(x, y));
                var imageTop = y + layout.Height;
                foreach (var image in block.Images ?? [])
                {
                    DrawImagePanel(context, image.Target, new Rect(x, imageTop + 8, Math.Max(100, _layoutWidth - x - 32), 264));
                    imageTop += 280;
                }
            }
        }
        foreach (var key in _layouts.Keys.Where(key => key < start - 20 || key > last + 20).ToArray()) { _layouts[key].Dispose(); _layouts.Remove(key); }
        foreach (var key in _tableLayouts.Keys.Where(key => key < start - 20 || key > last + 20).ToArray()) { _tableLayouts[key].Dispose(); _tableLayouts.Remove(key); }
    }
    private void DrawSelection(DrawingContext context, TextLayout layout, DocumentBlock block, int localStart, int localLength, Point origin, int prefix = 0)
    {
        var selectionStart = Math.Max(localStart, Math.Min(_selectionAnchor, _selectionEnd) - block.TextStart);
        var selectionEnd = Math.Min(localStart + localLength, Math.Max(_selectionAnchor, _selectionEnd) - block.TextStart);
        if (selectionEnd <= selectionStart) return;
        foreach (var rect in layout.HitTestTextRange(selectionStart - localStart + prefix, selectionEnd - selectionStart))
            context.DrawRectangle(Brush("SecondaryColorBrush"), null, rect.Translate(new Vector(origin.X, origin.Y)));
    }
    private void DrawImagePanel(DrawingContext context, string target, Rect bounds)
    {
        _images?.Request(target);
        context.DrawRectangle(Brush("BackgroundLightBrush"), new Pen(Brush("BorderBrush"), 1), bounds, 6, 6);
        if (_images?.Get(target) is not { } bitmap) return;
        var scale = Math.Min(bounds.Width / bitmap.Size.Width, bounds.Height / bitmap.Size.Height);
        var size = new Size(bitmap.Size.Width * scale, bitmap.Size.Height * scale);
        context.DrawImage(bitmap, new Rect(bitmap.Size), new Rect(bounds.Center.X - size.Width / 2, bounds.Center.Y - size.Height / 2, size.Width, size.Height));
    }
    private int BlockAt(double y)
    {
        if (_document.Blocks.Count == 0) return 0;
        var i = Array.BinarySearch(_tops, y); if (i < 0) i = ~i - 1;
        return Math.Clamp(i, 0, _document.Blocks.Count - 1);
    }
    private (int Index, int Position) Hit(Point point)
    {
        var i = BlockAt(point.Y - 24); var block = _document.Blocks[i];
        if (block.Kind == DocumentBlockKind.Table)
        {
            var local = new Point(point.X - 32 - block.Indent * 20, point.Y - 24 - _tops[i]);
            var table = TableLayout(i);
            var cell = table.Cells.FirstOrDefault(x => x.Bounds.Contains(local)) ?? table.Cells.OrderBy(x => Math.Abs(x.Bounds.Center.Y - local.Y) + Math.Abs(x.Bounds.Center.X - local.X)).First();
            var cellHit = cell.Layout.HitTestPoint(new Point(local.X - cell.Bounds.X - 8, local.Y - cell.Bounds.Y - 8));
            return (i, Math.Clamp(cell.Cell.TextStart + cellHit.TextPosition, cell.Cell.TextStart, cell.Cell.TextStart + cell.Cell.Text.Length));
        }
        var hit = Layout(i).HitTestPoint(new Point(point.X - 32 - block.Indent * 20, point.Y - 24 - _tops[i]));
        return (i, Math.Clamp(hit.TextPosition - PrefixLength(block), 0, block.Text.Length));
    }
    private Rect PositionRect(int index, int localPosition)
    {
        var block = _document.Blocks[index];
        if (block.Kind != DocumentBlockKind.Table)
            return Layout(index).HitTestTextPosition(Math.Clamp(localPosition + PrefixLength(block), 0, block.Text.Length + PrefixLength(block)));
        var table = TableLayout(index);
        var cell = table.Cells.LastOrDefault(x => x.Cell.TextStart <= localPosition) ?? table.Cells[0];
        var position = Math.Clamp(localPosition - cell.Cell.TextStart, 0, cell.Cell.Text.Length);
        return cell.Layout.HitTestTextPosition(position).Translate(new Vector(cell.Bounds.X + 8, cell.Bounds.Y + 8));
    }
    private string? ImageTargetAt(int index, Point point)
    {
        var block = _document.Blocks[index]; var localY = point.Y - 24 - _tops[index]; var localX = point.X - 32 - block.Indent * 20;
        if (block.Kind == DocumentBlockKind.Table)
        {
            var cell = TableLayout(index).Cells.FirstOrDefault(x => x.Bounds.Contains(new Point(localX, localY)));
            if (cell is null) return null;
            var imageIndex = (int)Math.Floor((localY - cell.Bounds.Y - 8 - cell.Layout.Height - 8) / 112);
            return imageIndex >= 0 && imageIndex < cell.Cell.Images.Count ? cell.Cell.Images[imageIndex].Target : null;
        }
        var layout = Layout(index); var normalImageIndex = (int)Math.Floor((localY - layout.Height - 8) / 280);
        return normalImageIndex >= 0 && normalImageIndex < (block.Images?.Count ?? 0) ? block.Images![normalImageIndex].Target : null;
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e); if (_document.Blocks.Count == 0) return;
        _selectAllWhenComplete = false; Focus(); var hit = Hit(e.GetPosition(this)); var block = _document.Blocks[hit.Index];
        var position = block.TextStart + hit.Position;
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)) _selectionAnchor = position;
        _selectionEnd = position; _dragging = true; e.Pointer.Capture(this); InvalidateVisual(); e.Handled = true;
        SourcePositionChanged?.Invoke(_document.TextToSource(position));
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e); if (!_dragging || _document.Blocks.Count == 0) return;
        var point = e.GetPosition(this); var hit = Hit(point); _selectionEnd = _document.Blocks[hit.Index].TextStart + hit.Position;
        if (point.Y < _viewportTop + 20) ScrollRequested?.Invoke(Math.Max(0, _viewportTop - 30));
        if (point.Y > _viewportTop + _viewportHeight - 20) ScrollRequested?.Invoke(_viewportTop + 30);
        InvalidateVisual();
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e); if (!_dragging) return; _dragging = false; e.Pointer.Capture(null);
        if (_selectionAnchor != _selectionEnd || _document.Blocks.Count == 0) return;
        var hit = Hit(e.GetPosition(this)); var block = _document.Blocks[hit.Index];
        var imageTarget = ImageTargetAt(hit.Index, e.GetPosition(this));
        if (imageTarget is not null && DocumentImageCache.IsRemote(imageTarget)) RemoteImageRequested?.Invoke(imageTarget);
        var link = LinkAt(block, hit.Position);
        if (link is not null) LinkInvoked?.Invoke(link);
    }
    private static string? LinkAt(DocumentBlock block, int localPosition)
    {
        if (block.Kind == DocumentBlockKind.Table && block.Table is { } table)
        {
            var cell = table.Cells.LastOrDefault(x => x.TextStart <= localPosition);
            if (cell is not null)
            {
                var cellPosition = localPosition - cell.TextStart;
                return cell.Runs.FirstOrDefault(x => x.Link is not null && cellPosition >= x.Start && cellPosition < x.Start + x.Length)?.Link;
            }
        }
        return block.Runs?.FirstOrDefault(x => x.Link is not null && localPosition >= x.Start && localPosition < x.Start + x.Length)?.Link;
    }
    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e); var command = e.KeyModifiers.HasFlag(OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control);
        var links = DocumentLinks();
        if (e.Key == Key.Tab && links.Count > 0)
        {
            _activeLinkIndex = e.KeyModifiers.HasFlag(KeyModifiers.Shift)
                ? (_activeLinkIndex <= 0 ? links.Count - 1 : _activeLinkIndex - 1)
                : (_activeLinkIndex + 1) % links.Count;
            var active = links[_activeLinkIndex]; _selectionAnchor = active.TextStart; _selectionEnd = active.TextStart + active.Length;
            RevealSelection(); SourcePositionChanged?.Invoke(_document.TextToSource(active.TextStart)); e.Handled = true; return;
        }
        if (e.Key == Key.Enter && _activeLinkIndex >= 0 && _activeLinkIndex < links.Count)
        {
            LinkInvoked?.Invoke(links[_activeLinkIndex].Target); e.Handled = true; return;
        }
        if (command && e.Key == Key.C && _selectAllWhenComplete) { IndexingPending?.Invoke(); e.Handled = true; return; }
        if (command && e.Key == Key.A && !_document.IsComplete) { _selectAllWhenComplete = true; _selectionAnchor = _selectionEnd = 0; IndexingPending?.Invoke(); InvalidateVisual(); e.Handled = true; return; }
        if (command && e.Key == Key.C) { if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard) await clipboard.SetTextAsync(SelectedText); e.Handled = true; return; }
        if (command && e.Key == Key.A) { _selectionAnchor = 0; _selectionEnd = _document.PlainText.Length; InvalidateVisual(); e.Handled = true; return; }
        var next = e.Key switch { Key.Left => TextNavigation.Move(_document.PlainText, _selectionEnd, -1), Key.Right => TextNavigation.Move(_document.PlainText, _selectionEnd, 1), Key.Home => 0, Key.End => _document.PlainText.Length, Key.Up => VerticalMove(-1), Key.Down => VerticalMove(1), Key.PageUp => VerticalMove(-1, true), Key.PageDown => VerticalMove(1, true), _ => -1 };
        if (next < 0 && e.Key != Key.Left) return;
        _selectionEnd = Math.Clamp(next, 0, _document.PlainText.Length);
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)) _selectionAnchor = _selectionEnd;
        _activeLinkIndex = -1; RevealSelection(); SourcePositionChanged?.Invoke(_document.TextToSource(_selectionEnd)); e.Handled = true;
    }
    private IReadOnlyList<DocumentLink> DocumentLinks()
    {
        var links = new List<DocumentLink>();
        foreach (var block in _document.Blocks)
        {
            if (block.Kind == DocumentBlockKind.Table && block.Table is { } table)
                links.AddRange(table.Cells.SelectMany(cell => cell.Runs.Where(x => x.Link is not null)
                    .Select(x => new DocumentLink(block.TextStart + cell.TextStart + x.Start, x.Length, x.Link!))));
            else
                links.AddRange((block.Runs ?? []).Where(x => x.Link is not null)
                    .Select(x => new DocumentLink(block.TextStart + x.Start, x.Length, x.Link!)));
        }
        return links;
    }
    private int VerticalMove(int direction, bool page = false)
    {
        if (_document.Blocks.Count == 0) return 0;
        var index = 0;
        for (var n = 0; n < _document.Blocks.Count; n++) { if (_document.Blocks[n].TextStart > _selectionEnd) break; index = n; }
        var block = _document.Blocks[index];
        var rect = PositionRect(index, Math.Clamp(_selectionEnd - block.TextStart, 0, block.Text.Length));
        var point = new Point(rect.X + 32 + block.Indent * 20, _tops[index] + 24 + rect.Y + rect.Height / 2 + direction * (page ? _viewportHeight : rect.Height));
        var hit = Hit(point);
        return _document.Blocks[hit.Index].TextStart + hit.Position;
    }
    private void RevealSelection()
    {
        var i = 0; for (var n = 0; n < _document.Blocks.Count; n++) { if (_document.Blocks[n].TextStart > _selectionEnd) break; i = n; }
        if (_document.Blocks.Count == 0) return;
        var block = _document.Blocks[i];
        var rect = PositionRect(i, Math.Clamp(_selectionEnd - block.TextStart, 0, block.Text.Length));
        var top = _tops[Math.Min(i, _tops.Length - 1)] + 24 + rect.Y;
        if (top < _viewportTop) ScrollRequested?.Invoke(top);
        else if (top + rect.Height > _viewportTop + _viewportHeight) ScrollRequested?.Invoke(Math.Max(0, top + rect.Height - _viewportHeight));
        InvalidateVisual();
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new DocumentAutomationPeer(this);
    private sealed record DocumentLink(int TextStart, int Length, string Target);
    private sealed record TableCellVisual(TableCellData Cell, TextLayout Layout, Rect Bounds);
    private sealed class TableVisualLayout(IReadOnlyList<TableCellVisual> cells, double height) : IDisposable
    {
        public IReadOnlyList<TableCellVisual> Cells { get; } = cells;
        public double Height { get; } = height;
        public void Dispose() { foreach (var cell in Cells) cell.Layout.Dispose(); }
    }
    private sealed class DocumentAutomationPeer(MarkdownDocumentView owner) : ControlAutomationPeer(owner), Avalonia.Automation.Provider.IValueProvider
    {
        private ParsedDocument? _snapshot;
        private List<AutomationPeer>? _children;
        public void Refresh() => InvalidateChildren();
        public bool IsReadOnly => true;
        public string Value => owner.Document.PlainText;
        public void SetValue(string? value) => throw new InvalidOperationException("The rendered document is read-only. Choose Edit to change the source.");
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Document;
        protected override string GetNameCore() => "Markdown document";
        protected override List<AutomationPeer>? GetChildrenCore()
        {
            if (!ReferenceEquals(_snapshot, owner.Document))
            {
                _snapshot = owner.Document;
                _children = owner.Document.Blocks.Select((block, index) => (AutomationPeer)new BlockAutomationPeer(owner, this, block, index)).ToList();
            }
            return _children;
        }
    }
    private sealed class BlockAutomationPeer(MarkdownDocumentView owner, AutomationPeer parent, DocumentBlock block, int index) : AutomationPeer
    {
        private AutomationPeer? _parent = parent;
        private bool Current => index < owner.Document.Blocks.Count && ReferenceEquals(owner.Document.Blocks[index], block);
        protected override bool TrySetParent(AutomationPeer? value) { _parent = value; return true; }
        protected override AutomationPeer? GetParentCore() => _parent;
        protected override void BringIntoViewCore() { if (Current) owner.GoToSource(block.SourceStart); }
        protected override string? GetAcceleratorKeyCore() => null;
        protected override string? GetAccessKeyCore() => null;
        protected override string GetClassNameCore() => "MarkdownBlock";
        protected override AutomationPeer? GetLabeledByCore() => null;
        protected override bool HasKeyboardFocusCore() => false;
        protected override bool IsContentElementCore() => true;
        protected override bool IsControlElementCore() => true;
        protected override bool IsEnabledCore() => Current && owner.IsEffectivelyEnabled;
        protected override void SetFocusCore() { BringIntoViewCore(); owner.Focus(); }
        protected override bool ShowContextMenuCore() => false;
        protected override int GetHeadingLevelCore() => block.Kind == DocumentBlockKind.Heading ? block.Level : 0;
        protected override AutomationControlType GetAutomationControlTypeCore() => block.Kind switch
        {
            DocumentBlockKind.Image => AutomationControlType.Image,
            DocumentBlockKind.Table => AutomationControlType.Table,
            _ => AutomationControlType.Text
        };
        protected override string GetNameCore() => block.Kind switch
        {
            DocumentBlockKind.Heading => $"Heading {block.Level}: {block.Text}",
            DocumentBlockKind.Table => $"Table, {block.Table?.Cells.Select(x => x.Row).Distinct().Count() ?? 0} rows and {block.Table?.Columns ?? 0} columns",
            _ => block.Text
        };
        protected override string GetAutomationIdCore() => "markdown-block-" + index;
        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore() => block.Table?.Cells
            .Select(cell => (AutomationPeer)new TableCellAutomationPeer(owner, this, block, cell, index)).ToArray() ?? Array.Empty<AutomationPeer>();
        protected override Rect GetBoundingRectangleCore()
        {
            if (!Current || index >= owner._heights.Length || TopLevel.GetTopLevel(owner) is not { } top || owner.TransformToVisual(top) is not { } transform) return default;
            return new Rect(0, owner._tops[index] + 24, owner.Bounds.Width, owner._heights[index]).TransformToAABB(transform);
        }
        protected override bool IsOffscreenCore() => !Current || index >= owner._heights.Length || owner._tops[index] + owner._heights[index] < owner._viewportTop || owner._tops[index] > owner._viewportTop + owner._viewportHeight;
        protected override bool IsKeyboardFocusableCore() => false;
    }
    private sealed class TableCellAutomationPeer(MarkdownDocumentView owner, AutomationPeer parent, DocumentBlock block, TableCellData cell, int blockIndex) : AutomationPeer
    {
        private AutomationPeer? _parent = parent;
        private bool Current => blockIndex < owner.Document.Blocks.Count && ReferenceEquals(owner.Document.Blocks[blockIndex], block);
        protected override bool TrySetParent(AutomationPeer? value) { _parent = value; return true; }
        protected override AutomationPeer? GetParentCore() => _parent;
        protected override void BringIntoViewCore() { if (Current) owner.GoToSource(cell.SourceStart); }
        protected override string? GetAcceleratorKeyCore() => null;
        protected override string? GetAccessKeyCore() => null;
        protected override string GetClassNameCore() => cell.IsHeader ? "MarkdownTableHeader" : "MarkdownTableCell";
        protected override AutomationPeer? GetLabeledByCore() => null;
        protected override bool HasKeyboardFocusCore() => false;
        protected override bool IsContentElementCore() => true;
        protected override bool IsControlElementCore() => true;
        protected override bool IsEnabledCore() => Current && owner.IsEffectivelyEnabled;
        protected override void SetFocusCore() { BringIntoViewCore(); owner.Focus(); }
        protected override bool ShowContextMenuCore() => false;
        protected override int GetHeadingLevelCore() => 0;
        protected override AutomationControlType GetAutomationControlTypeCore() => cell.IsHeader ? AutomationControlType.Header : AutomationControlType.DataItem;
        protected override string GetNameCore() => $"Row {cell.Row + 1}, column {cell.Column + 1}: {cell.Text}";
        protected override string GetAutomationIdCore() => $"markdown-table-{blockIndex}-cell-{cell.Row}-{cell.Column}";
        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore() => Array.Empty<AutomationPeer>();
        protected override Rect GetBoundingRectangleCore()
        {
            if (!Current || !owner._tableLayouts.TryGetValue(blockIndex, out var table) || TopLevel.GetTopLevel(owner) is not { } top || owner.TransformToVisual(top) is not { } transform) return default;
            var visual = table.Cells.FirstOrDefault(x => ReferenceEquals(x.Cell, cell));
            return visual is null ? default : visual.Bounds.Translate(new Vector(32 + block.Indent * 20, owner._tops[blockIndex] + 24)).TransformToAABB(transform);
        }
        protected override bool IsOffscreenCore()
        {
            if (!Current || !owner._tableLayouts.TryGetValue(blockIndex, out var table)) return true;
            var visual = table.Cells.FirstOrDefault(x => ReferenceEquals(x.Cell, cell));
            if (visual is null) return true;
            var top = owner._tops[blockIndex] + visual.Bounds.Y;
            return top + visual.Bounds.Height < owner._viewportTop || top > owner._viewportTop + owner._viewportHeight;
        }
        protected override bool IsKeyboardFocusableCore() => false;
    }
}
