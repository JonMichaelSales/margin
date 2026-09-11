using System.Text;
using MDPlayer.Core;
using MDPlayer.Infrastructure;
using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;

public sealed class DocumentTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "MDPlayer-tests-" + Guid.NewGuid().ToString("N"));
    public DocumentTests() => Directory.CreateDirectory(_directory);
    private string FilePath(string name = "document.md") => Path.Combine(_directory, name);
    public void Dispose() => Directory.Delete(_directory, true);

    [Fact]
    public void EveryOpenReturnsToReadAndOlderSaveDoesNotClearNewerEdits()
    {
        var session = new DocumentSession();
        var disk = new FileRevision(FilePath(), "hash", 1, DateTime.UtcNow, 65001, false);
        session.Open(new("original", disk));
        session.Mode = DocumentMode.Edit;
        session.Text = "first edit";
        var saving = session.Capture();
        session.Text = "newer edit";
        session.MarkSaved(saving, disk);
        Assert.True(session.IsDirty);
        Assert.Equal("newer edit", session.Text);
        session.Mode = DocumentMode.Read;
        Assert.True(session.IsDirty);
        session.Open(new("another document", disk));
        Assert.Equal(DocumentMode.Read, session.Mode);
        Assert.False(session.IsDirty);
    }

    [Theory]
    [InlineData(65001, false)]
    [InlineData(65001, true)]
    [InlineData(1200, true)]
    [InlineData(1201, true)]
    [InlineData(12000, true)]
    [InlineData(12001, true)]
    public async Task SavingPreservesEncodingBomAndMixedNewlines(int codePage, bool bom)
    {
        Encoding encoding = codePage switch
        {
            65001 => new UTF8Encoding(bom), 1200 => new UnicodeEncoding(false, bom),
            1201 => new UnicodeEncoding(true, bom), 12000 => new UTF32Encoding(false, bom),
            _ => new UTF32Encoding(true, bom)
        };
        const string source = "# Résumé\r\n\n日本語 👩🏽‍💻\rالعربية\n";
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(source)).ToArray();
        await File.WriteAllBytesAsync(FilePath(), bytes);
        var service = new DocumentFileService();
        var opened = await service.OpenAsync(FilePath());
        Assert.Equal(source, opened.Text);
        await service.SaveAsync(FilePath(), new(source, 1), opened.Revision);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(FilePath()));
    }

    [Fact]
    public async Task ExternalChangesAndUnconfirmedOverwriteAreRejected()
    {
        await File.WriteAllTextAsync(FilePath(), "original");
        var service = new DocumentFileService();
        var opened = await service.OpenAsync(FilePath());
        await File.WriteAllTextAsync(FilePath(), "external writer");
        await Assert.ThrowsAsync<DocumentConflictException>(() => service.SaveAsync(FilePath(), new("my edits", 2), opened.Revision));
        await Assert.ThrowsAsync<DocumentConflictException>(() => service.SaveAsync(FilePath(), new("my edits", 2), null));
        Assert.Equal("external writer", await File.ReadAllTextAsync(FilePath()));
    }

    [Fact]
    public async Task FailedReplacementLeavesOriginalAndRemovesTemporaryFile()
    {
        await File.WriteAllTextAsync(FilePath(), "original");
        var service = new DocumentFileService(new FailingCommitter());
        var opened = await service.OpenAsync(FilePath());
        await Assert.ThrowsAsync<IOException>(() => service.SaveAsync(FilePath(), new("my edits", 2), opened.Revision));
        Assert.Equal("original", await File.ReadAllTextAsync(FilePath()));
        Assert.Single(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task DeletedFileAndCancelledSavePreserveBufferAndDisk()
    {
        var service = new DocumentFileService();
        await File.WriteAllTextAsync(FilePath(), "original");
        var opened = await service.OpenAsync(FilePath());
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SaveAsync(FilePath(), new("edit", 2), opened.Revision, cancelled.Token));
        Assert.Equal("original", await File.ReadAllTextAsync(FilePath()));
        File.Delete(FilePath());
        Assert.True(await service.HasChangedAsync(opened.Revision));
        await Assert.ThrowsAsync<DocumentConflictException>(() => service.SaveAsync(FilePath(), new("edit", 2), opened.Revision));
        Assert.False(File.Exists(FilePath()));
    }

    [Fact]
    public void ParserKeepsSourceSpansAndSafeHtmlAcrossRichBlocks()
    {
        const string text = "# Heading\n\nFirst **bold** paragraph.\n\nSecond العربية 👩🏽‍💻.\n\n- [x] Done\n\n| A | B |\n|---|---|\n| 1 | 2 |\n\n<script>alert('x')</script>";
        var result = new MarkdownParser().Parse(text, 42);
        Assert.Equal(42, result.Revision);
        Assert.Single(result.Outline);
        Assert.Contains(result.Blocks, b => b.Kind == DocumentBlockKind.TableRow);
        Assert.Contains("☑", result.PlainText);
        Assert.Contains("<script>", result.PlainText);
        Assert.All(result.Blocks, b => Assert.Equal(b.Text, result.PlainText.Substring(b.TextStart, b.Text.Length)));
        Assert.All(result.Blocks, b => Assert.InRange(b.SourceStart, 0, text.Length - 1));
        Assert.Contains(result.Blocks, b => b.Runs?.Any(r => r.Style.HasFlag(InlineStyle.Bold)) == true);
    }

    private sealed class FailingCommitter : IFileCommitter
    {
        public void Commit(string temporaryPath, string destination, bool exists) => throw new IOException("Simulated locked destination or exhausted storage.");
    }
}
