using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MDPlayer.Core;

public enum DocumentMode { Read, Edit, Split }
public sealed record FileRevision(string Path, string Hash, long Length, DateTime LastWriteUtc, int CodePage, bool HasBom);
public sealed record OpenedDocument(string Text, FileRevision Revision);
public sealed record BufferRevision(string Text, long Version);

/// <summary>The source buffer is the only authority for saving; rendering never writes to it.</summary>
public sealed class DocumentSession : INotifyPropertyChanged
{
    private string _text = "", _savedText = "";
    private DocumentMode _mode;
    public string Text { get => _text; set { if (_text == value) return; _text = value; Version++; Notify(); Notify(nameof(IsDirty)); } }
    public long Version { get; private set; }
    public FileRevision? DiskRevision { get; private set; }
    public string? FilePath => DiskRevision?.Path;
    public string FileName => FilePath is null ? "Untitled" : System.IO.Path.GetFileName(FilePath);
    public bool IsDirty => !string.Equals(_text, _savedText, StringComparison.Ordinal);
    public DocumentMode Mode { get => _mode; set { if (_mode == value) return; _mode = value; Notify(); } }
    public BufferRevision Capture() => new(Text, Version);
    public void Open(OpenedDocument document)
    {
        _text = _savedText = document.Text; DiskRevision = document.Revision; Version++;
        _mode = DocumentMode.Read; NotifyAll();
    }
    public void MarkSaved(BufferRevision saved, FileRevision disk) { _savedText = saved.Text; DiskRevision = disk; Notify(nameof(IsDirty)); Notify(nameof(FilePath)); Notify(nameof(FileName)); Notify(nameof(DiskRevision)); }
    public void Discard() { Text = _savedText; }
    private void NotifyAll() { foreach (var property in new[] { nameof(Text), nameof(IsDirty), nameof(FilePath), nameof(FileName), nameof(Mode), nameof(DiskRevision) }) Notify(property); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}
public interface IDocumentFileService
{
    Task<OpenedDocument> OpenAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> HasChangedAsync(FileRevision revision, CancellationToken cancellationToken = default);
    Task<FileRevision> SaveAsync(string path, BufferRevision buffer, FileRevision? expected, CancellationToken cancellationToken = default, FileRevision? sourceFormat = null);
}
public sealed class DocumentConflictException(string message) : IOException(message);
