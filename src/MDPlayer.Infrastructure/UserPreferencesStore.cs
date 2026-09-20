using System.Text.Json;
using MDPlayer.Core;

namespace MDPlayer.Infrastructure;

public sealed class UserPreferencesStore : IUserPreferencesStore
{
    private readonly string _path;
    private readonly string? _legacyPath;
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private string? _loadNotice;
    public UserPreferences Current { get; private set; }
    public UserPreferencesStore(string? path = null, string? legacyPath = null)
    {
        _path = path ?? DefaultPath("Margin");
        _legacyPath = legacyPath ?? (path is null ? DefaultPath("MDPlayer") : null);
        Current = Load();
    }
    public void Save(UserPreferences preferences)
    {
        lock (_gate)
        {
            var normalized = Normalize(preferences);
            Write(normalized);
            Current = normalized;
        }
    }

    public string? TakeLoadNotice()
    {
        lock (_gate)
        {
            var notice = _loadNotice;
            _loadNotice = null;
            return notice;
        }
    }

    private UserPreferences Load()
    {
        if (!File.Exists(_path) && _legacyPath is not null && File.Exists(_legacyPath))
        {
            try
            {
                var migrated = Normalize(Read(_legacyPath));
                Write(migrated);
                _loadNotice = "Your reading and appearance settings were moved to Margin. The original MDPlayer settings were preserved as a backup.";
                return migrated;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                _loadNotice = "Margin could not import the previous MDPlayer settings. Defaults are in use and the original settings file was preserved. " + ex.Message;
                return new UserPreferences();
            }
        }

        if (!File.Exists(_path)) return new UserPreferences();
        try { return Normalize(Read(_path)); }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            var backup = PreserveInvalidFile();
            var recovered = RecoverLastValidPreferences();
            if (recovered is not null)
            {
                _loadNotice = "Margin recovered the last valid saved settings after the current settings file became unreadable. The unreadable file was preserved" +
                    (backup is null ? "." : $" at {backup}.");
                return recovered;
            }
            _loadNotice = "Margin could not read its saved settings. Defaults are in use and the unreadable file was preserved" +
                (backup is null ? ". " : $" at {backup}. ") + ex.Message;
            return new UserPreferences();
        }
    }

    private UserPreferences Read(string path) => JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(path), Json)
        ?? throw new JsonException("The settings file did not contain preferences.");

    private static UserPreferences Normalize(UserPreferences preferences) => preferences with
    {
        SchemaVersion = UserPreferences.CurrentSchemaVersion,
        Reading = (preferences.Reading ?? new()).Sanitize(),
        Theme = (preferences.Theme ?? new()).Normalize(),
        Window = preferences.Window ?? new()
    };

    private void Write(UserPreferences preferences)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(preferences, Json));
            if (File.Exists(_path)) File.Copy(_path, _path + ".backup", true);
            File.Move(temporary, _path, true);
            if (Normalize(Read(_path)) != preferences) throw new IOException("The saved settings could not be verified.");
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private string? PreserveInvalidFile()
    {
        try
        {
            var backup = _path + ".invalid.bak";
            File.Copy(_path, backup, true);
            return backup;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }

    private UserPreferences? RecoverLastValidPreferences()
    {
        var backup = _path + ".backup";
        if (!File.Exists(backup)) return null;
        try
        {
            var recovered = Normalize(Read(backup));
            File.Copy(backup, _path, true);
            return recovered;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return null; }
    }

    private static string DefaultPath(string productName) => Path.Combine(OperatingSystem.IsMacOS()
        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", productName)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), productName), "preferences.json");
}
