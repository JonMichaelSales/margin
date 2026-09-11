using System.Text;
using MDPlayer.Core;
using MDPlayer.Infrastructure;
using Xunit;

namespace MDPlayer.Tests;
public sealed class PreferencesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "MDPlayer-preferences-" + Guid.NewGuid().ToString("N"));
    public PreferencesTests() => Directory.CreateDirectory(_root);
    public void Dispose() => Directory.Delete(_root, true);
    [Fact]
    public void SettingsRoundTripWithoutDocumentContentsOrHistory()
    {
        var path = Path.Combine(_root, "preferences.json");
        var store = new UserPreferencesStore(path);
        store.Save(new() { Theme = new(false, "Margin Slate Dark"), Reading = new() { FontSize = 22, ParagraphGap = 1.1 }, Window = new(1000, 700) });
        var reloaded = new UserPreferencesStore(path);
        Assert.Equal(store.Current, reloaded.Current);
        var json = File.ReadAllText(path);
        Assert.DoesNotContain("Document", json); Assert.DoesNotContain("Recent", json); Assert.DoesNotContain("History", json);
        Assert.Single(Directory.GetFiles(_root));
    }
    [Fact]
    public void CorruptPreferencesFallBackToDefaults()
    {
        var path = Path.Combine(_root, "preferences.json"); File.WriteAllText(path, "{broken");
        Assert.Equal(new UserPreferences(), new UserPreferencesStore(path).Current);
    }
    [Fact]
    public async Task SaveAsUsesOriginalEncodingInsteadOfDestinationEncoding()
    {
        var source = Path.Combine(_root, "source.md"); var destination = Path.Combine(_root, "destination.md");
        var encoding = new UnicodeEncoding(true, true);
        await File.WriteAllBytesAsync(source, encoding.GetPreamble().Concat(encoding.GetBytes("日本語\r\n")).ToArray());
        await File.WriteAllTextAsync(destination, "old UTF8 destination");
        var files = new DocumentFileService(); var opened = await files.OpenAsync(source); var previous = await files.OpenAsync(destination);
        await files.SaveAsync(destination, new(opened.Text, 1), previous.Revision, sourceFormat: opened.Revision);
        Assert.Equal(await File.ReadAllBytesAsync(source), await File.ReadAllBytesAsync(destination));
    }
}
