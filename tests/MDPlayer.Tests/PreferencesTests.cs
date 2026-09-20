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
        Assert.Equal(UserPreferences.CurrentSchemaVersion, reloaded.Current.SchemaVersion);
        var json = File.ReadAllText(path);
        Assert.DoesNotContain("Document", json); Assert.DoesNotContain("Recent", json); Assert.DoesNotContain("History", json);
        Assert.Single(Directory.GetFiles(_root));
    }
    [Fact]
    public void CorruptPreferencesArePreservedAndReportedBeforeFallingBack()
    {
        var path = Path.Combine(_root, "preferences.json"); File.WriteAllText(path, "{broken");
        var store = new UserPreferencesStore(path);
        Assert.Equal(new UserPreferences(), store.Current);
        Assert.Equal("{broken", File.ReadAllText(path));
        Assert.Equal("{broken", File.ReadAllText(path + ".invalid.bak"));
        Assert.Contains("could not read", store.TakeLoadNotice(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(store.TakeLoadNotice());
    }
    [Fact]
    public void LegacyProductSettingsMigrateWithoutDeletingTheOriginal()
    {
        var current = Path.Combine(_root, "Margin", "preferences.json");
        var legacy = Path.Combine(_root, "MDPlayer", "preferences.json");
        Directory.CreateDirectory(Path.GetDirectoryName(legacy)!);
        var legacyPreferences = new UserPreferences
        {
            SchemaVersion = 1,
            Theme = new(false, "MDPlayer Studio Dark"),
            Reading = new() { FontFamily = "Georgia", FontSize = 21, ParagraphGap = 1.2 }
        };
        File.WriteAllText(legacy, System.Text.Json.JsonSerializer.Serialize(legacyPreferences));

        var migrated = new UserPreferencesStore(current, legacy);

        Assert.True(File.Exists(current));
        Assert.True(File.Exists(legacy));
        Assert.Equal("Margin Studio Dark", migrated.Current.Theme.SkinName);
        Assert.Equal(21, migrated.Current.Reading.FontSize);
        Assert.Equal(UserPreferences.CurrentSchemaVersion, migrated.Current.SchemaVersion);
        Assert.Contains("moved to Margin", migrated.TakeLoadNotice());
        Assert.Equal(migrated.Current, new UserPreferencesStore(current, legacy).Current);
    }
    [Fact]
    public void SeparateAppearanceReadingAndWindowWritesSurviveAProcessRestart()
    {
        var path = Path.Combine(_root, "preferences.json");
        var firstProcess = new UserPreferencesStore(path);
        firstProcess.Save(firstProcess.Current with { Theme = new(false, "Margin Paper Dark") });
        firstProcess.Save(firstProcess.Current with { Reading = new() { FontFamily = "Atkinson Hyperlegible", FontSize = 24, WidthCharacters = 96, WidthMode = ReadingWidthMode.Full } });
        firstProcess.Save(firstProcess.Current with { Window = new(1440, 900, 20, 30, true) });

        var nextProcess = new UserPreferencesStore(path);

        Assert.Equal("Margin Paper Dark", nextProcess.Current.Theme.SkinName);
        Assert.Equal("Atkinson Hyperlegible", nextProcess.Current.Reading.FontFamily);
        Assert.Equal(24, nextProcess.Current.Reading.FontSize);
        Assert.Equal(96, nextProcess.Current.Reading.WidthCharacters);
        Assert.Equal(ReadingWidthMode.Full, nextProcess.Current.Reading.WidthMode);
        Assert.True(nextProcess.Current.Window.Maximized);
    }
    [Fact]
    public void LastValidBackupRecoversAnUnreadableCurrentFile()
    {
        var path = Path.Combine(_root, "preferences.json");
        var store = new UserPreferencesStore(path);
        store.Save(store.Current with { Theme = new(false, "Margin Slate Dark") });
        store.Save(store.Current with { Reading = new() { FontSize = 25 } });
        File.WriteAllText(path, "{damaged");

        var recovered = new UserPreferencesStore(path);

        Assert.Equal("Margin Slate Dark", recovered.Current.Theme.SkinName);
        Assert.Equal(new ReadingPreferences(), recovered.Current.Reading);
        Assert.Contains("recovered the last valid", recovered.TakeLoadNotice(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("{damaged", File.ReadAllText(path + ".invalid.bak"));
        Assert.NotEqual("{damaged", File.ReadAllText(path));
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
