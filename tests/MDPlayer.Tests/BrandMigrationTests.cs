using System.Text.Json;
using MDPlayer.Core;
using MDPlayer.Infrastructure;
using Xunit;

namespace MDPlayer.Tests;

public sealed class BrandMigrationTests
{
    [Theory]
    [InlineData("Paper Light")]
    [InlineData("Paper Dark")]
    [InlineData("Slate Light")]
    [InlineData("Slate Dark")]
    [InlineData("Studio Light")]
    [InlineData("Studio Dark")]
    public void ExistingSkinChoiceSurvivesRenameWithoutRewritingSettings(string skin)
    {
        var path = Path.Combine(Path.GetTempPath(), "margin-migration-" + Guid.NewGuid().ToString("N") + ".json");
        var saved = new UserPreferences { Theme = new(false, "MDPlayer " + skin), Reading = new() { FontSize = 23, LineHeight = 1.9 }, Window = new(1100, 750) };
        var json = JsonSerializer.Serialize(saved);
        File.WriteAllText(path, json);
        try
        {
            var store = new UserPreferencesStore(path);
            Assert.Equal(new ThemePreference(false, "Margin " + skin), store.Current.Theme);
            Assert.Equal(saved.Reading, store.Current.Reading);
            Assert.Equal(saved.Window, store.Current.Window);
            Assert.Equal(json, File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }
}
