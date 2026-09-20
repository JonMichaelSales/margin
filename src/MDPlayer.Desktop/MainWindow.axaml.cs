using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using MDPlayer.Core;
using MDPlayer.Desktop.Services;
using MDPlayer.Desktop.ViewModels;
using MDPlayer.Desktop.Views;
using MDPlayer.Desktop.Controls;
using MDPlayer.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace MDPlayer.Desktop;

public partial class MainWindow : Window
{
    private readonly DocumentViewModel _vm;
    private readonly IDocumentFileService _files;
    private readonly IMarkdownParser _parser;
    private readonly IUserPreferencesStore _preferences;
    private readonly IAppearanceService _appearance;
    private readonly IPlatformIntegration _platform;
    private readonly MarkdownDocumentView _reader;
    private TextEditor? _editor;
    private CancellationTokenSource? _parseCancellation, _loadCancellation;
    private FileSystemWatcher? _watcher;
    private readonly DispatcherTimer _watchDebounce = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private readonly DispatcherTimer _typographySaveDebounce = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private bool _hasDocument, _changingEditor, _closingApproved, _closePending, _isSaving, _settingTypography, _syncing;
    private bool _readingNeedsSave;
    private string? _layoutSignature; private bool _outlineVisible = true, _typeVisible, _focusMode;
    private string? _initialPath;
    private long _openGeneration;
    public DocumentSession Session => _vm.Session;
    private T C<T>(string name) where T : Control => this.FindControl<T>(name) ?? throw new InvalidOperationException("Missing control " + name);
    public MainWindow() : this(((App)Application.Current!).Services) { }
    public MainWindow(IServiceProvider services, string? initialPath = null)
    {
        AvaloniaXamlLoader.Load(this);
        using (var icon = Avalonia.Platform.AssetLoader.Open(new Uri("avares://Margin/Assets/Margin.ico"))) Icon = new WindowIcon(icon);
        _files = services.GetRequiredService<IDocumentFileService>(); _parser = services.GetRequiredService<IMarkdownParser>();
        _preferences = services.GetRequiredService<IUserPreferencesStore>(); _appearance = services.GetRequiredService<IAppearanceService>(); _platform = services.GetRequiredService<IPlatformIntegration>();
        _vm = new(_preferences.Current.Reading); DataContext = _vm; _reader = C<MarkdownDocumentView>("Reader"); _initialPath = initialPath;
        var geometry = _preferences.Current.Window;
        Width = Math.Clamp(geometry.Width, 720, 2560); Height = Math.Clamp(geometry.Height, 480, 1600);
        if (geometry.Maximized) WindowState = WindowState.Maximized;
        ConfigureIcons(); WireCommands(); WireTypography(); BuildNativeMenu();
        C<Button>("DefaultAppButton").IsVisible = _platform.CanChooseDefaultMarkdownApp;
        _reader.Preferences = _vm.Reading;
        C<ListBox>("OutlineList").ItemTemplate = new FuncDataTemplate<OutlineEntry>((item, _) => new TextBlock { Text = item?.Title, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(Math.Max(0, (item?.Level ?? 1) - 1) * 8, 5, 0, 5), FontSize = 12 });
        C<ListBox>("OutlineList").SelectionChanged += (_, _) => { if (C<ListBox>("OutlineList").SelectedItem is OutlineEntry entry) _reader.GoToSource(entry.SourceStart); };
        _reader.ScrollRequested += offset => { var scroll = C<ScrollViewer>("DocumentScroll"); scroll.Offset = new Vector(0, Math.Max(0, offset)); };
        _reader.SourcePositionChanged += position => SyncEditor(position, moveCaret: true);
        _reader.IndexingPending += () => SetStatus("Indexing the full document · Select all will be ready shortly");
                _reader.LinkInvoked += OpenLink;
        _reader.RemoteImageRequested += async target =>
        {
            var answer = await PromptWindow.Ask(this, "Load remote image?", "This contacts the image host for this document only.\n" + target, "Cancel", "Load image");
            if (answer == "Load image") _reader.AllowRemoteImage(target);
        };
        C<ScrollViewer>("DocumentScroll").ScrollChanged += (_, _) =>
        {
            var scroll = C<ScrollViewer>("DocumentScroll"); _reader.SetViewport(scroll.Offset.Y, scroll.Viewport.Height);
            if (!_syncing && Session.Mode == DocumentMode.Split && scroll.IsPointerOver) SyncEditor(_reader.VisibleSourceStart);
        };
        Session.PropertyChanged += SessionChanged;
        _appearance.Changed += AppearanceChanged;
        _watchDebounce.Tick += async (_, _) => { _watchDebounce.Stop(); await CheckDiskAsync(); };
        _typographySaveDebounce.Tick += (_, _) => { _typographySaveDebounce.Stop(); SaveReadingPreferences(); };
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, (_, e) => { e.DragEffects = e.DataTransfer.Formats.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; });
        AddHandler(DragDrop.DropEvent, async (_, e) =>
        {
            var paths = e.DataTransfer.TryGetFiles()?.Select(file => file.TryGetLocalPath()).Where(path => path is not null).Cast<string>().ToArray() ?? [];
            await OpenPathsAsync(paths); e.Handled = true;
        });
                Opened += async (_, _) => {
            var working = Screens.ScreenFromWindow(this)?.WorkingArea;
            if (working is { } area && WindowState != WindowState.Maximized)
            {
                Width = Math.Min(Width, area.Width / RenderScaling); Height = Math.Min(Height, area.Height / RenderScaling);
                Position = new PixelPoint(Math.Max(area.X, Math.Min(Position.X, area.Right - (int)(Width * RenderScaling))), Math.Max(area.Y, Math.Min(Position.Y, area.Bottom - (int)(Height * RenderScaling))));
            }
            if (_initialPath is { } path) { _initialPath = null; await OpenDocumentAsync(path); }
            if (_preferences.TakeLoadNotice() is { } notice) ShowMessage(notice);
        };
        Closing += OnClosing;
        Closed += (_, _) =>
        {
            _parseCancellation?.Cancel(); _loadCancellation?.Cancel(); _watcher?.Dispose(); _watchDebounce.Stop(); _typographySaveDebounce.Stop();
            _appearance.Changed -= AppearanceChanged; Session.PropertyChanged -= SessionChanged;
            try
            {
                var current = _readingNeedsSave ? _preferences.Current with { Reading = _vm.Reading } : _preferences.Current;
                _preferences.Save(current with { Window = new(Width, Height, Position.X, Position.Y, WindowState == WindowState.Maximized) });
                _readingNeedsSave = false;
            }
            catch { /* Closing must not discard an already completed file save because preference storage is unavailable. */ }
        };
        SizeChanged += (_, _) => UpdateLayoutMode();
        KeyDown += OnWindowKeyDown; RefreshChrome();
    }
    private void WireCommands()
    {
        void Bind(string name, Action action) => C<Button>(name).Click += (_, _) => action();
        void BindAsync(string name, Func<Task> action) => C<Button>(name).Click += async (_, _) => await action();
        BindAsync("OpenButton", OpenPickerAsync); BindAsync("EmptyOpenButton", OpenPickerAsync);
        Bind("ReadButton", () => SetMode(DocumentMode.Read)); Bind("EditButton", () => SetMode(DocumentMode.Edit)); Bind("SplitButton", () => SetMode(DocumentMode.Split));
        Bind("OutlineButton", () => { _outlineVisible = !_outlineVisible; UpdateLayoutMode(); });
        Bind("TypeButton", () => { _typeVisible = !_typeVisible; _focusMode = false; UpdateLayoutMode(); if (_typeVisible) C<TextBox>("FontFamilyBox").Focus(); });
        Bind("CloseTypeButton", () => { _typeVisible = false; UpdateLayoutMode(); C<Button>("TypeButton").Focus(); });
        Bind("FocusButton", () => { _focusMode = !_focusMode; UpdateLayoutMode(); });
        Bind("FindButton", ShowFind); Bind("CloseFindButton", () => { C<Border>("FindBar").IsVisible = false; _reader.Focus(); });
        Bind("NextFindButton", () => Find(false)); Bind("PreviousFindButton", () => Find(true));
        C<TextBox>("FindText").KeyDown += (_, e) => { if (e.Key == Key.Enter) { Find(e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; } };
        BindAsync("AppearanceButton", async () => { try { await new AppearanceWindow(_appearance).ShowDialog(this); } catch (Exception ex) { ShowError(ex); } });
        Bind("DefaultAppButton", () =>
        {
            try { _platform.OpenDefaultMarkdownAppSettings(); ShowMessage("Windows Default Apps opened. Choose Margin for .md and .markdown files."); }
            catch (Exception ex) { ShowError(ex); }
        });
        BindAsync("SaveButton", async () => { await SaveAsync(); }); BindAsync("SaveAsButton", async () => { await SaveAsync(true); });
        BindAsync("ReloadButton", async () => { if (Session.FilePath is { } path && await GuardChangesAsync()) await OpenDocumentAsync(path); });
        Bind("DismissBanner", () => C<Border>("Banner").IsVisible = false);
        Bind("CancelLoadButton", () => _loadCancellation?.Cancel());
        Bind("UndoButton", () => _editor?.Undo()); Bind("RedoButton", () => _editor?.Redo());
        foreach (var pair in new[] { ("HeadingButton", "heading"), ("BoldButton", "bold"), ("ItalicButton", "italic"), ("QuoteButton", "quote"), ("ListButton", "list"), ("CodeButton", "code"), ("LinkButton", "link") }) Bind(pair.Item1, () => Format(pair.Item2));
    }
    private void WireTypography()
    {
        _settingTypography = true;
        C<TextBox>("FontFamilyBox").Text = _vm.Reading.FontFamily;
        foreach (var pair in new[] { ("FontSizeSlider", _vm.Reading.FontSize), ("LineHeightSlider", _vm.Reading.LineHeight), ("ParagraphGapSlider", _vm.Reading.ParagraphGap), ("ReadingWidthSlider", (double)_vm.Reading.WidthCharacters), ("IndentSlider", _vm.Reading.FirstLineIndent) })
        {
            var slider = C<Slider>(pair.Item1); slider.Value = pair.Item2;
            slider.PropertyChanged += (_, e) => { if (e.Property == RangeBase.ValueProperty) UpdateTypography(); };
        }
        C<ComboBox>("AlignmentBox").SelectedIndex = _vm.Reading.Justified ? 1 : 0;
        C<ComboBox>("AlignmentBox").SelectionChanged += (_, _) => UpdateTypography();
        C<TextBox>("FontFamilyBox").LostFocus += (_, _) => UpdateTypography();
        C<TextBox>("FontFamilyBox").KeyDown += (_, e) => { if (e.Key == Key.Enter) UpdateTypography(); };
        C<Button>("ResetReadingButton").Click += (_, _) => ResetReadingPreferences();
        _settingTypography = false; RefreshTypographyLabels();
    }
    private void UpdateTypography()
    {
        if (_settingTypography) return;

        _vm.Reading = new ReadingPreferences
        {
            FontFamily = C<TextBox>("FontFamilyBox").Text ?? "Georgia", FontSize = C<Slider>("FontSizeSlider").Value,
            LineHeight = C<Slider>("LineHeightSlider").Value, ParagraphGap = C<Slider>("ParagraphGapSlider").Value,
            WidthCharacters = (int)C<Slider>("ReadingWidthSlider").Value, Justified = C<ComboBox>("AlignmentBox").SelectedIndex == 1,
            FirstLineIndent = C<Slider>("IndentSlider").Value
        };
        _reader.Preferences = _vm.Reading;
        _readingNeedsSave = true;
        _typographySaveDebounce.Stop();
        _typographySaveDebounce.Start();
        C<TextBlock>("TypographySaveStatus").Text = "Saving…";
        C<TextBlock>("TypographySaveStatus")[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("TextSecondaryBrush");
        RefreshTypographyLabels();
    }
    private void RefreshTypographyLabels()
    {
        C<TextBlock>("FontSizeLabel").Text = $"Font size · {_vm.Reading.FontSize:0} DIP";
        C<TextBlock>("LineHeightLabel").Text = $"Line height · {_vm.Reading.LineHeight:0.00}";
        C<TextBlock>("ParagraphGapLabel").Text = $"Paragraph gap · {_vm.Reading.ParagraphGap:0.0} em";
        C<TextBlock>("ReadingWidthLabel").Text = $"Reading width · {_vm.Reading.WidthCharacters} characters";
        C<TextBlock>("IndentLabel").Text = $"First-line indent · {_vm.Reading.FirstLineIndent:0.0} em";
    }
    private void SaveReadingPreferences()
    {
        if (!_readingNeedsSave) return;
        try
        {
            _preferences.Save(_preferences.Current with { Reading = _vm.Reading });
            _readingNeedsSave = false;
            C<TextBlock>("TypographySaveStatus").Text = "Saved automatically";
            C<TextBlock>("TypographySaveStatus")[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("SuccessBrush");
        }
        catch (Exception ex)
        {
            C<TextBlock>("TypographySaveStatus").Text = "Could not save";
            C<TextBlock>("TypographySaveStatus")[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("ErrorBrush");
            ShowError(ex);
        }
    }
    private void ResetReadingPreferences()
    {
        _settingTypography = true;
        _vm.Reading = new ReadingPreferences();
        C<TextBox>("FontFamilyBox").Text = _vm.Reading.FontFamily;
        C<Slider>("FontSizeSlider").Value = _vm.Reading.FontSize;
        C<Slider>("LineHeightSlider").Value = _vm.Reading.LineHeight;
        C<Slider>("ParagraphGapSlider").Value = _vm.Reading.ParagraphGap;
        C<Slider>("ReadingWidthSlider").Value = _vm.Reading.WidthCharacters;
        C<ComboBox>("AlignmentBox").SelectedIndex = _vm.Reading.Justified ? 1 : 0;
        C<Slider>("IndentSlider").Value = _vm.Reading.FirstLineIndent;
        _settingTypography = false;
        _reader.Preferences = _vm.Reading;
        _readingNeedsSave = true;
        RefreshTypographyLabels();
        SaveReadingPreferences();
    }
    public async Task OpenDocumentAsync(string path)
    {
        var generation = ++_openGeneration;
        _loadCancellation?.Cancel(); _loadCancellation = new(); var token = _loadCancellation.Token;
        _vm.IsBusy = true; SetStatus("Opening document…"); C<Button>("CancelLoadButton").IsVisible = true;
        try
        {
            var document = await _files.OpenAsync(path, token);
            var initialSource = MarkdownParser.FirstViewportSource(document.Text); var staged = initialSource.Length != document.Text.Length;
            var parsed = await Task.Run(() => _parser.Parse(initialSource, Session.Version + 1, token), token);
            if (generation != _openGeneration) return;
            _parseCancellation?.Cancel(); _hasDocument = true;
            _changingEditor = true;
            try { Session.Open(document); if (_editor is not null) { _editor.Text = document.Text; _editor.Document.UndoStack.ClearAll(); } }
            finally { _changingEditor = false; }
            _reader.DocumentPath = Session.FilePath; _reader.SetDocument(parsed with { IsComplete = !staged }); C<ListBox>("OutlineList").ItemsSource = parsed.Outline;
            C<ScrollViewer>("DocumentScroll").Offset = default;
            Watch(document.Revision.Path); RefreshChrome(); SetStatus(staged ? "Reading · indexing the full document…" : "Reading");
            if (staged) _ = CompleteIndexAsync(document.Text, Session.Version, generation, token);
            C<TextBlock>("DetailsLabel").Text = $"{WordCount(parsed.PlainText):N0} words · {EncodingName(document.Revision.CodePage)} · {LineEndingName(document.Text)}";
            C<Border>("Banner").IsVisible = false;
        }
        catch (OperationCanceledException) { if (generation == _openGeneration) SetStatus("Opening cancelled"); }
        catch (Exception ex) { ShowError(ex); }
        finally { if (generation == _openGeneration) { _vm.IsBusy = false; C<Button>("CancelLoadButton").IsVisible = false; RefreshChrome(); } }
    }
    private async Task CompleteIndexAsync(string text, long revision, long generation, CancellationToken token)
    {
        try
        {
            var parsed = await Task.Run(() => _parser.Parse(text, revision, token), token);
            if (token.IsCancellationRequested || generation != _openGeneration || revision != Session.Version) return;
            var anchor = _reader.CaptureReadingAnchor();
            _reader.SetDocument(parsed); C<ListBox>("OutlineList").ItemsSource = parsed.Outline;
            Dispatcher.UIThread.Post(() => _reader.RestoreReadingAnchor(anchor), DispatcherPriority.Loaded);
            C<TextBlock>("DetailsLabel").Text = $"{WordCount(parsed.PlainText):N0} words · {EncodingName(Session.DiskRevision?.CodePage ?? 65001)} · {LineEndingName(text)}";
            SetStatus("Reading");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex); }
    }
    private async Task OpenPickerAsync()
    {
        try { await OpenPathsAsync(await _platform.PickOpenFilesAsync(this)); } catch (Exception ex) { ShowError(ex); }
    }
    private async Task OpenPathsAsync(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0) return;
        if (!_hasDocument) await OpenDocumentAsync(paths[0]);
        else ((App)Application.Current!).CreateWindow(paths[0]).Show();
        foreach (var path in paths.Skip(1)) ((App)Application.Current!).CreateWindow(path).Show();
    }
    private void SetMode(DocumentMode mode)
    {
        if (!_hasDocument) return;
        var source = _reader.VisibleSourceStart;
        if (mode != DocumentMode.Read) EnsureEditor();
        Session.Mode = mode; RefreshChrome();
        if (mode != DocumentMode.Read) { SyncEditor(source); _editor?.Focus(); }
    }
    private void EnsureEditor()
    {
        if (_editor is not null) return;
        _editor = new TextEditor { ShowLineNumbers = true, WordWrap = true, FontFamily = new FontFamily(_preferences.Current.EditorFontFamily), FontSize = _preferences.Current.EditorFontSize, Text = Session.Text };
        _editor.Options.ConvertTabsToSpaces = true; _editor.Options.IndentationSize = 4;
        _editor.TextChanged += (_, _) => { if (!_changingEditor) Session.Text = _editor.Text; };
        _editor.TextArea.Caret.PositionChanged += (_, _) =>
        {
            if (!_syncing && Session.Mode == DocumentMode.Split && _editor.IsKeyboardFocusWithin)
            {
                _syncing = true; try { _reader.GoToSource(_editor.CaretOffset); } finally { _syncing = false; }
            }
        };
        C<DockPanel>("EditorHost").Children.Add(_editor); UpdateEditorColors();
    }
    private void SessionChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshChrome();
        if (e.PropertyName == nameof(DocumentSession.Text) && !_changingEditor) _ = RefreshPreviewAsync();
    }
    private async Task RefreshPreviewAsync()
    {
        _parseCancellation?.Cancel(); _parseCancellation = new(); var token = _parseCancellation.Token; var snapshot = Session.Capture(); var generation = _openGeneration;
        try
        {
            await Task.Delay(120, token);
            var parsed = await Task.Run(() => _parser.Parse(snapshot.Text, snapshot.Version, token), token);
            if (token.IsCancellationRequested || Session.Version != snapshot.Version || generation != _openGeneration) return;
            var source = _reader.VisibleSourceStart; _reader.DocumentPath = Session.FilePath; _reader.SetDocument(parsed); C<ListBox>("OutlineList").ItemsSource = parsed.Outline;
            Dispatcher.UIThread.Post(() => _reader.GoToSource(source), DispatcherPriority.Loaded);
            C<TextBlock>("DetailsLabel").Text = $"{WordCount(parsed.PlainText):N0} words · unsaved buffer";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { ShowError(ex); }
    }
    private async Task<bool> SaveAsync(bool saveAs = false)
    {
        if (!_hasDocument || _isSaving) return false;
        if (!saveAs && !Session.IsDirty) return true;
        _isSaving = true; RefreshChrome();
        try
        {
            var path = Session.FilePath; FileRevision? expected = Session.DiskRevision;
            if (saveAs || path is null)
            {
                path = await _platform.PickSaveFileAsync(this, Session.FileName); if (path is null) return false;
                expected = File.Exists(path) ? (await _files.OpenAsync(path)).Revision : null;
            }
            var snapshot = Session.Capture();
            var saved = await _files.SaveAsync(path, snapshot, expected, sourceFormat: Session.DiskRevision);
            Session.MarkSaved(snapshot, saved); Watch(saved.Path); SetStatus(Session.IsDirty ? "Saved · newer edits remain unsaved" : "Saved"); return true;
        }
        catch (Exception ex) { ShowError(ex); return false; }
        finally { _isSaving = false; RefreshChrome(); }
    }
    private async Task<bool> GuardChangesAsync()
    {
        if (!Session.IsDirty) return true;
        var result = await PromptWindow.Ask(this, "Save your changes?", $"Your edits to {Session.FileName} have not been saved.", "Discard changes", "Cancel", "Save");
        if (result == "Discard changes") return true;
        if (result == "Save") return await SaveAsync() && !Session.IsDirty;
        return false;
    }
    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closingApproved) return;
        if (_isSaving || _closePending) { e.Cancel = true; return; }
        if (!Session.IsDirty) return;
        e.Cancel = true; _closePending = true;
        try { if (await GuardChangesAsync()) { _closingApproved = true; Close(); } }
        finally { _closePending = false; }
    }
    private void Watch(string path)
    {
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path)) { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName };
        void Changed(object? _, FileSystemEventArgs __) => Dispatcher.UIThread.Post(() => { _watchDebounce.Stop(); _watchDebounce.Start(); });
        _watcher.Changed += Changed; _watcher.Deleted += Changed; _watcher.Renamed += (_, _) => Dispatcher.UIThread.Post(() => { _watchDebounce.Stop(); _watchDebounce.Start(); });
        _watcher.EnableRaisingEvents = true;
    }
    private async Task CheckDiskAsync()
    {
        if (_isSaving || Session.DiskRevision is not { } disk) return;
        try
        {
            if (await _files.HasChangedAsync(disk)) { ShowMessage(Session.IsDirty ? "This file changed outside Margin. Your edits are preserved. Save as a copy or reload after resolving your edits." : "This file changed outside Margin. Reload to see the new version, or keep reading this copy."); C<Button>("ReloadButton").IsVisible = true; C<AppIcon>("BannerIcon").Kind = "external-change"; }
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private void Format(string kind)
    {
        if (_editor is null) return;
        var start = _editor.SelectionStart; var length = _editor.SelectionLength; var selected = _editor.SelectedText;
        using (_editor.Document.RunUpdate())
        {
            if (kind is "heading" or "quote" or "list")
            {
                var first = _editor.Document.GetLineByOffset(start).LineNumber; var last = _editor.Document.GetLineByOffset(start + length).LineNumber;
                var prefix = kind == "heading" ? "## " : kind == "quote" ? "> " : "- ";
                for (var n = last; n >= first; n--) _editor.Document.Insert(_editor.Document.GetLineByNumber(n).Offset, prefix);
            }
            else
            {
                if (string.IsNullOrEmpty(selected)) selected = "text";
                var replacement = kind switch { "bold" => "**" + selected + "**", "italic" => "*" + selected + "*", "code" => "```\n" + selected + "\n```", "link" => "[" + selected + "](https://example.com)", _ => selected };
                _editor.Document.Replace(start, length, replacement); _editor.Select(start, replacement.Length);
            }
        }
        _editor.Focus();
    }
    private void SyncEditor(int source, bool moveCaret = false)
    {
        if (_editor is null || _syncing) return;
        _syncing = true;
        try
        {
            var offset = Math.Clamp(source, 0, _editor.Document.TextLength);
            var line = _editor.Document.GetLineByOffset(offset);
            if (moveCaret) _editor.CaretOffset = offset;
            _editor.ScrollToLine(line.LineNumber);
        }
        finally { _syncing = false; }
    }
    private void AppearanceChanged(object? sender, EventArgs e) { _reader.RefreshColors(); UpdateEditorColors(); }
    private void UpdateEditorColors()
    {
        if (_editor is null) return;
        IBrush Brush(string name) => (IBrush)Application.Current!.Resources[name]!;
        _editor.Foreground = Brush("TextPrimaryBrush"); _editor.Background = Brush("BackgroundBrush");
        _editor.LineNumbersForeground = Brush("TextSecondaryBrush");
        _editor.TextArea.SelectionBrush = Brush("SecondaryColorBrush"); _editor.TextArea.SelectionForeground = Brush("TextPrimaryBrush");
        string Color(string name) => ((SolidColorBrush)Brush(name)).Color.ToString();
        var xshd = $"""
            <SyntaxDefinition name="Markdown" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
              <Color name="Heading" foreground="{Color("AccentBlueBrush")}" fontWeight="bold" />
              <Color name="Code" foreground="{Color("SuccessBrush")}" />
              <Color name="Link" foreground="{Color("AccentBlueBrush")}" />
              <RuleSet><Rule color="Heading">^\#+\s.*$</Rule><Rule color="Code">`[^`]+`</Rule><Rule color="Link">\[[^\]]+\]\([^\)]+\)</Rule></RuleSet>
            </SyntaxDefinition>
            """;
        using var reader = XmlReader.Create(new StringReader(xshd)); _editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
    private void RefreshChrome()
    {
        Title = _hasDocument ? (Session.IsDirty ? "● " : "") + Session.FileName + " · Margin" : "Margin";
        C<TextBlock>("FileNameLabel").Text = _hasDocument ? Session.FileName : "Margin";
        C<TextBlock>("FileStateLabel").Text = !_hasDocument ? "A little room to read." : Session.IsDirty ? "Unsaved changes" : "Original unchanged";
        C<AppIcon>("DirtyIcon").IsVisible = _hasDocument && Session.IsDirty;
        foreach (var pair in new[] { ("ReadButton", DocumentMode.Read), ("EditButton", DocumentMode.Edit), ("SplitButton", DocumentMode.Split) })
        {
            var button = C<Button>(pair.Item1); button.IsEnabled = _hasDocument; button.Classes.Set("active", Session.Mode == pair.Item2);
        }
        C<Button>("SaveButton").IsVisible = _hasDocument && Session.IsDirty; C<Button>("SaveButton").IsEnabled = !_isSaving;
        C<Button>("SaveAsButton").IsVisible = _hasDocument; C<Button>("SaveAsButton").IsEnabled = !_isSaving;
        C<Button>("UndoButton").IsEnabled = _editor?.CanUndo ?? false; C<Button>("RedoButton").IsEnabled = _editor?.CanRedo ?? false;
        UpdateLayoutMode();
    }
    private void UpdateLayoutMode()
    {
        C<Grid>("DocumentGrid").IsVisible = _hasDocument; C<ScrollViewer>("EmptyPanel").IsVisible = !_hasDocument;
        UpdateCompactIcons();
        var showOutline = _outlineVisible && !_focusMode && Session.Mode == DocumentMode.Read && Bounds.Width >= 850;
        var showType = _typeVisible && !_focusMode;
        var signature = $"{_hasDocument}:{showOutline}:{showType}:{Session.Mode}:{Bounds.Width >= 1000}:{_focusMode}";
        if (_layoutSignature == signature) return;
        _layoutSignature = signature;
        C<Grid>("DocumentGrid").ColumnDefinitions = new ColumnDefinitions($"{(showOutline ? "208" : "0")},*,{(showType ? "272" : "0")}");
        C<Border>("OutlinePanel").IsVisible = showOutline; C<Border>("TypePanel").IsVisible = showType;
        UpdateCompactIcons();
        C<Button>("OutlineButton").IsVisible = !_focusMode; C<Button>("TypeButton").IsVisible = !_focusMode; C<Button>("FindButton").IsVisible = !_focusMode;
        var split = Session.Mode == DocumentMode.Split && Bounds.Width >= 1000 && !showType;
        var editing = Session.Mode == DocumentMode.Edit || Session.Mode == DocumentMode.Split && !split;
        C<Grid>("Workspace").ColumnDefinitions = new ColumnDefinitions(split ? "*,5,*" : "*,0,0");
        Grid.SetColumn(C<DockPanel>("EditorHost"), 0); Grid.SetColumn(C<ScrollViewer>("DocumentScroll"), split ? 2 : 0);
        C<DockPanel>("EditorHost").IsVisible = editing || split; C<ScrollViewer>("DocumentScroll").IsVisible = !editing || split; C<GridSplitter>("PaneSplitter").IsVisible = split;
        if (Session.Mode == DocumentMode.Split && !split) SetStatus("Narrow workspace · use Read / Edit to switch panes");
    }
    private void ShowFind() { C<Border>("FindBar").IsVisible = true; C<TextBox>("FindText").Focus(); }
    private void Find(bool backwards)
    {
        if (Session.Mode != DocumentMode.Edit && !_reader.Document.IsComplete) { SetStatus("Indexing the full document · Find will be ready shortly"); return; }
        var query = C<TextBox>("FindText").Text ?? "";
        if (Session.Mode == DocumentMode.Edit && _editor is not null && query.Length > 0)
        {
            var text = _editor.Text;
            var start = backwards ? Math.Max(0, _editor.SelectionStart - 1) : Math.Min(text.Length, _editor.SelectionStart + _editor.SelectionLength);
            var found = backwards && text.Length > 0 ? text.LastIndexOf(query, Math.Min(text.Length - 1, start), StringComparison.OrdinalIgnoreCase) : text.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
            if (found < 0) found = backwards ? text.LastIndexOf(query, StringComparison.OrdinalIgnoreCase) : text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            if (found >= 0) { _editor.Select(found, query.Length); _editor.ScrollToLine(_editor.Document.GetLineByOffset(found).LineNumber); return; }
        }
        else if (_reader.Find(query, backwards)) return;
        SetStatus("No matching text");
    }
    private void OpenLink(string target)
    {
        try
        {
            if (target.StartsWith('#'))
            {
                var slug = Uri.UnescapeDataString(target[1..]);
                var entry = _reader.Document.Outline.FirstOrDefault(x => Regex.Replace(x.Title.ToLowerInvariant(), @"[^\p{L}\p{N}\s-]", "").Replace(' ', '-') == slug);
                if (entry is not null) _reader.GoToSource(entry.SourceStart); return;
            }
            if (Uri.TryCreate(target, UriKind.Absolute, out var uri)) { _platform.OpenExternalLink(uri.AbsoluteUri); return; }
            if (Session.FilePath is { } path)
            {
                var local = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, Uri.UnescapeDataString(target)));
                if (Path.GetExtension(local).ToLowerInvariant() is ".md" or ".markdown") ((App)Application.Current!).CreateWindow(local).Show();
            }
        }
        catch (Exception ex) { ShowError(ex); }
    }
    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var command = e.KeyModifiers.HasFlag(OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control);
        if (command && e.Key == Key.O) { await OpenPickerAsync(); e.Handled = true; }
        else if (command && e.Key == Key.S) { await SaveAsync(e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; }
        else if (command && e.Key == Key.F) { ShowFind(); e.Handled = true; }
        else if (command && e.Key == Key.E) { SetMode(e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? DocumentMode.Split : Session.Mode == DocumentMode.Read ? DocumentMode.Edit : DocumentMode.Read); e.Handled = true; }
        else if (e.Key == Key.F3) { Find(e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; }
        else if (e.Key == Key.F11) { _focusMode = !_focusMode; UpdateLayoutMode(); e.Handled = true; }
        else if (e.Key == Key.Escape) { C<Border>("FindBar").IsVisible = false; _typeVisible = false; _focusMode = false; UpdateLayoutMode(); }
    }
    private void ConfigureIcons()
    {
        var command = OperatingSystem.IsMacOS() ? "Command" : "Ctrl";
        foreach (var (name, kind, label) in new[] {
            ("OpenButton", "open", "Open"), ("EmptyOpenButton", "open", "Open a Markdown file"),
            ("ReadButton", "read", "Read"), ("EditButton", "edit", "Edit"), ("SplitButton", "split", "Split"),
            ("SaveButton", "save", "Save"), ("SaveAsButton", "save-as", "Save as") })
            IconButtons.Set(C<Button>(name), kind, label, shortcut: kind == "open" ? command + "+O" : kind == "save" ? command + "+S" : null);
        foreach (var (name, kind, label) in new[] {
            ("UndoButton", "undo", "Undo"), ("RedoButton", "redo", "Redo"), ("HeadingButton", "heading", "Heading"),
            ("BoldButton", "bold", "Bold"), ("ItalicButton", "italic", "Italic"), ("QuoteButton", "quote", "Quote"),
            ("ListButton", "list", "List"), ("CodeButton", "code", "Code"), ("LinkButton", "link", "Link"),
            ("CloseFindButton", "close", "Close find"), ("PreviousFindButton", "previous", "Previous match"),
            ("NextFindButton", "next", "Next match"), ("AboutButton", "information", "About Margin") })
            IconButtons.Set(C<Button>(name), kind, label, true);
        IconButtons.Set(C<Button>("CloseTypeButton"), "close", "Close typography");
        IconButtons.Set(C<Button>("DismissBanner"), "close", "Dismiss");
        IconButtons.Set(C<Button>("ReloadButton"), "external-change", "Reload");
        C<Button>("AboutButton").Click += async (_, _) => await new AboutWindow().ShowDialog(this);
        UpdateCompactIcons();
    }
    private void UpdateCompactIcons()
    {
        var compact = Bounds.Width < 1500;
        foreach (var (name, kind, label) in new[] {
            ("OutlineButton", "outline", "Outline"), ("TypeButton", "typography", "Typography"),
            ("FindButton", "find", "Find"), ("AppearanceButton", "appearance", "Appearance") })
            IconButtons.Update(C<Button>(name), kind, label, compact);
        IconButtons.Update(C<Button>("FocusButton"), _focusMode ? "exit-focus" : "focus", _focusMode ? "Exit focus" : "Focus", compact && !_focusMode);
    }
    private void BuildNativeMenu()
    {
        if (!OperatingSystem.IsMacOS()) return;
        var root = new NativeMenu(); var file = new NativeMenu(); var menu = new NativeMenuItem("File") { Menu = file };
        var open = new NativeMenuItem("Open…"); open.Click += async (_, _) => await OpenPickerAsync(); file.Add(open);
        var save = new NativeMenuItem("Save"); save.Click += async (_, _) => await SaveAsync(); file.Add(save);
        var saveAs = new NativeMenuItem("Save as…"); saveAs.Click += async (_, _) => await SaveAsync(true); file.Add(saveAs);
        var close = new NativeMenuItem("Close"); close.Click += (_, _) => Close(); file.Add(close); root.Add(menu); NativeMenu.SetMenu(this, root);
    }
    private void ShowError(Exception exception) { ShowMessage(exception.Message); C<AppIcon>("BannerIcon").Kind = "error"; C<AppIcon>("BannerIcon")[!TemplatedControl.ForegroundProperty] = new DynamicResourceExtension("ErrorBrush"); SetStatus("Action could not be completed · your document buffer is preserved"); }
    private void ShowMessage(string message) { C<AppIcon>("BannerIcon").Kind = "information"; C<AppIcon>("BannerIcon")[!TemplatedControl.ForegroundProperty] = new DynamicResourceExtension("AccentBlueBrush"); C<Border>("Banner").IsVisible = true; C<TextBlock>("BannerText").Text = message; C<Button>("ReloadButton").IsVisible = false; }
    private void SetStatus(string message) { C<AppIcon>("StatusIcon").IsVisible = message.StartsWith("Saved", StringComparison.Ordinal); _vm.Status = message; C<TextBlock>("StatusLabel").Text = message; }
    private static int WordCount(string text) { var count = 0; var word = false; foreach (var c in text) { if (char.IsWhiteSpace(c)) word = false; else if (!word) { count++; word = true; } } return count; }
    private static string EncodingName(int codePage) => codePage switch { 65001 => "UTF-8", 1200 => "UTF-16 LE", 1201 => "UTF-16 BE", 12000 => "UTF-32 LE", 12001 => "UTF-32 BE", _ => "Unicode" };
    private static string LineEndingName(string text) { var crlf = text.Contains("\r\n"); var loneLf = text.Replace("\r\n", "").Contains('\n'); return crlf && loneLf ? "Mixed newlines" : crlf ? "CRLF" : "LF"; }
}
