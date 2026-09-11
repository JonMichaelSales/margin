using System.Text.Json;
using MDPlayer.Core;

namespace MDPlayer.Infrastructure;

public sealed class UserPreferencesStore : IUserPreferencesStore
{
    private readonly string _path;
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public UserPreferences Current { get; private set; }
    public UserPreferencesStore(string? path = null)
    {
        // Retain the original private settings location so installed users keep their preferences.
        _path = path ?? Path.Combine(OperatingSystem.IsMacOS()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "MDPlayer")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MDPlayer"), "preferences.json");
        try { Current = File.Exists(_path) ? JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(_path), Json) ?? new() : new(); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { Current = new(); }
        Current = Current with { Reading = (Current.Reading ?? new()).Sanitize(), Theme = (Current.Theme ?? new()).Normalize(), Window = Current.Window ?? new() };
    }
    public void Save(UserPreferences preferences)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(preferences, Json));
                File.Move(temporary, _path, true);
                Current = preferences;
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
