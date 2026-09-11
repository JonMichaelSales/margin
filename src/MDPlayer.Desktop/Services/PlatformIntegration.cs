using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MDPlayer.Desktop.Services;

public interface IPlatformIntegration
{
    Task<IReadOnlyList<string>> PickOpenFilesAsync(Window owner);
    Task<string?> PickSaveFileAsync(Window owner, string suggestedName);
    void OpenExternalLink(string target);
}
public sealed class PlatformIntegration : IPlatformIntegration
{
    private static readonly FilePickerFileType Markdown = new("Markdown documents") { Patterns = ["*.md", "*.markdown"], AppleUniformTypeIdentifiers = ["net.daringfireball.markdown", "public.plain-text"], MimeTypes = ["text/markdown", "text/plain"] };
    public async Task<IReadOnlyList<string>> PickOpenFilesAsync(Window owner)
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Open Markdown", AllowMultiple = true, FileTypeFilter = [Markdown, FilePickerFileTypes.All] });
        return files.Select(file => file.TryGetLocalPath()).Where(path => path is not null).Cast<string>().ToArray();
    }
    public async Task<string?> PickSaveFileAsync(Window owner, string suggestedName)
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { Title = "Save Markdown", SuggestedFileName = suggestedName, DefaultExtension = "md", FileTypeChoices = [Markdown], ShowOverwritePrompt = true });
        return file?.TryGetLocalPath();
    }
    public void OpenExternalLink(string target)
    {
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http" or "mailto"))
            throw new InvalidOperationException("Only web and email links can be opened externally.");
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
}
