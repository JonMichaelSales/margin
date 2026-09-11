using System.Security.Cryptography;
using System.Text;
using MDPlayer.Core;

namespace MDPlayer.Infrastructure;

public interface IFileCommitter { void Commit(string temporaryPath, string destination, bool exists); }
public sealed class AtomicFileCommitter : IFileCommitter
{
    public void Commit(string temporaryPath, string destination, bool exists)
    {
        if (exists) File.Replace(temporaryPath, destination, null);
        else File.Move(temporaryPath, destination);
    }
}
public sealed class DocumentFileService(IFileCommitter? committer = null) : IDocumentFileService
{
    private readonly IFileCommitter _committer = committer ?? new AtomicFileCommitter();
    public async Task<OpenedDocument> OpenAsync(string path, CancellationToken cancellationToken = default)
    {
        path = ResolvePath(path);
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var (encoding, skip) = DetectEncoding(bytes);
        var text = encoding.GetString(bytes, skip, bytes.Length - skip);
        if (text.Contains('\0')) throw new InvalidDataException("This file contains binary data or an unsupported encoding. Open a UTF-8 or BOM-marked Unicode Markdown file.");
        return new(text, Revision(path, bytes, encoding.CodePage, skip > 0));
    }
    public async Task<bool> HasChangedAsync(FileRevision revision, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = new FileStream(revision.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 65536, true);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
            return hash != revision.Hash;
        }
        catch (FileNotFoundException) { return true; }
        catch (DirectoryNotFoundException) { return true; }
    }
    public async Task<FileRevision> SaveAsync(string path, BufferRevision buffer, FileRevision? expected, CancellationToken cancellationToken = default, FileRevision? sourceFormat = null)
    {
        path = ResolvePath(path);
        if (expected is not null && !SamePath(path, expected.Path)) throw new ArgumentException("The expected revision belongs to a different file.");
        if (expected is null && File.Exists(path)) throw new DocumentConflictException("The destination already exists. Confirm it through Save as before replacing it.");
        if (expected is not null && await HasChangedAsync(expected, cancellationToken)) throw new DocumentConflictException("The file changed outside Margin. Reload it or save a copy.");
        var format = sourceFormat ?? expected;
        var encoding = EncodingFor(format?.CodePage ?? 65001, format?.HasBom ?? false);
        var content = encoding.GetBytes(buffer.Text);
        var preamble = encoding.GetPreamble();
        var bytes = new byte[preamble.Length + content.Length];
        preamble.CopyTo(bytes, 0); content.CopyTo(bytes, preamble.Length);
        var temporary = Path.Combine(Path.GetDirectoryName(path)!, "." + Path.GetFileName(path) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (expected is not null && await HasChangedAsync(expected, cancellationToken)) throw new DocumentConflictException("Another application changed the file during saving. Your edits are still available.");
            _committer.Commit(temporary, path, expected is not null);
            var actual = await File.ReadAllBytesAsync(path, CancellationToken.None);
            if (!actual.AsSpan().SequenceEqual(bytes)) throw new DocumentConflictException("The file changed immediately after saving. Your buffer has been retained.");
            return Revision(path, actual, encoding.CodePage, preamble.Length > 0);
        }
        finally { try { if (File.Exists(temporary)) File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }
    private static (Encoding, int) DetectEncoding(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(new byte[] { 0xff, 0xfe, 0, 0 })) return (new UTF32Encoding(false, true, true), 4);
        if (bytes.AsSpan().StartsWith(new byte[] { 0, 0, 0xfe, 0xff })) return (new UTF32Encoding(true, true, true), 4);
        if (bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf })) return (new UTF8Encoding(true, true), 3);
        if (bytes.AsSpan().StartsWith(new byte[] { 0xff, 0xfe })) return (new UnicodeEncoding(false, true, true), 2);
        if (bytes.AsSpan().StartsWith(new byte[] { 0xfe, 0xff })) return (new UnicodeEncoding(true, true, true), 2);
        return (new UTF8Encoding(false, true), 0);
    }
    private static Encoding EncodingFor(int codePage, bool bom) => codePage switch
    {
        65001 => new UTF8Encoding(bom, true), 1200 => new UnicodeEncoding(false, bom, true),
        1201 => new UnicodeEncoding(true, bom, true), 12000 => new UTF32Encoding(false, bom, true),
        12001 => new UTF32Encoding(true, bom, true), _ => throw new InvalidDataException("Unsupported file encoding.")
    };
    private static FileRevision Revision(string path, byte[] bytes, int codePage, bool bom) => new(path, Convert.ToHexString(SHA256.HashData(bytes)), bytes.LongLength, File.GetLastWriteTimeUtc(path), codePage, bom);
    public static string ResolvePath(string path)
    {
        var full = Path.GetFullPath(path);
        if (!File.Exists(full)) return full;
        return new FileInfo(full).ResolveLinkTarget(true)?.FullName ?? full;
    }
    private static bool SamePath(string a, string b) => string.Equals(a, b, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}
