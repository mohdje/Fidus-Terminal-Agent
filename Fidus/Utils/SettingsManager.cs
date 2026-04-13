using System.Text.Json;

public static class SettingsManager
{
    readonly static string SettingsFilePath = Path.Join(AppContext.BaseDirectory, "fidus_settings.json");

    public static Settings ReadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
                return null;

            var settingsJson = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<Settings>(settingsJson);
        }
        catch
        {
            return null;
        }
    }

    public static void SaveSettings(Settings settings)
    {
        var settingsJson = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsFilePath, settingsJson);
    }
}
