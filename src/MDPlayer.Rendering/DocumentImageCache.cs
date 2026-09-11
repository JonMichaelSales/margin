using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace MDPlayer.Rendering;

/// <summary>Document-scoped decoded images. Remote requests only run after AllowRemote is called.</summary>
public sealed class DocumentImageCache : IDisposable
{
    private static readonly HttpClient Http = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(15) };
    private readonly Dictionary<string, Bitmap> _images = [];
    private readonly HashSet<string> _loading = [], _allowed = [];
    private readonly Dictionary<string, string> _errors = [];
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _decodeSlots = new(2);
    public string? DocumentPath { get; init; }
    public event Action? Changed;
    public static bool IsRemote(string target) => target.StartsWith("//") || target.StartsWith("\\\\") || Uri.TryCreate(target, UriKind.Absolute, out var uri) && (uri.IsUnc || uri.Scheme is "http" or "https");
    public Bitmap? Get(string target) => _images.GetValueOrDefault(target);
    public string? Error(string target) => _errors.GetValueOrDefault(target);
    public void Retain(IReadOnlySet<string> targets)
    {
        foreach (var target in _images.Keys.Where(x => !targets.Contains(x)).ToArray()) { _images[target].Dispose(); _images.Remove(target); }
    }
    public void AllowRemote(string target) { _allowed.Add(target); Request(target); }
    public void Request(string target)
    {
        if (_lifetime.IsCancellationRequested || _images.ContainsKey(target) || _errors.ContainsKey(target) || _loading.Contains(target)) return;
        if (IsRemote(target) && !_allowed.Contains(target)) return;
        if (_images.Count + _loading.Count >= 6) return;
        _loading.Add(target);
        _ = LoadAsync(target);
    }
    private async Task LoadAsync(string target)
    {
        Bitmap? image = null;
        try
        {
            var token = _lifetime.Token;
            await _decodeSlots.WaitAsync(token);
            try
            {
                byte[] bytes;
                if (Uri.TryCreate(target, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
                {
                    using var response = await Http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                    response.EnsureSuccessStatusCode();
                    if (response.Content.Headers.ContentLength > 16 * 1024 * 1024) throw new IOException("Image exceeds the 16 MB limit.");
                    await using var stream = await response.Content.ReadAsStreamAsync(token);
                    bytes = await ReadBoundedAsync(stream, token);
                }
                else
                {
                    if (DocumentPath is null) throw new IOException("Save the document before using relative images.");
                    if (uri is not null && !uri.IsFile) throw new IOException("This image format or URI is unsupported.");
                    var path = uri?.IsFile == true ? uri.LocalPath : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(DocumentPath)!, Uri.UnescapeDataString(target)));
                    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, true);
                    bytes = await ReadBoundedAsync(stream, token);
                }
                image = await Task.Run(() =>
                {
                    using var stream = new MemoryStream(bytes);
                    return Bitmap.DecodeToWidth(stream, 1200);
                }, token);
            }
            finally { _decodeSlots.Release(); }
            if (_lifetime.IsCancellationRequested) { image.Dispose(); return; }
            _images[target] = image;
        }
        catch (OperationCanceledException) { image?.Dispose(); }
        catch (Exception ex) { image?.Dispose(); if (!_lifetime.IsCancellationRequested) _errors[target] = ex.Message; }
        finally
        {
            _loading.Remove(target);
            if (!_lifetime.IsCancellationRequested) Dispatcher.UIThread.Post(() => Changed?.Invoke());
        }
    }
    private static async Task<byte[]> ReadBoundedAsync(Stream stream, CancellationToken token)
    {
        using var result = new MemoryStream();
        var buffer = new byte[65536];
        while (true)
        {
            var read = await stream.ReadAsync(buffer, token);
            if (read == 0) break;
            if (result.Length + read > 16 * 1024 * 1024) throw new IOException("Image exceeds the 16 MB limit.");
            result.Write(buffer, 0, read);
        }
        return result.ToArray();
    }
    public void Dispose()
    {
        _lifetime.Cancel();
        foreach (var image in _images.Values) image.Dispose();
        _images.Clear();
    }
}
