using System.Text.Json;

public static class SettingsManager
{
    readonly static string SettingsFilePath = Path.Join(AppContext.BaseDirectory, "fidus_settings.json");
    public static Settings Init(string[] commandArgs)
    {
        var settings = ReadSettings();
        settings ??= new Settings();
        if (commandArgs.HasValidCommandArgs())
        {
            var values = commandArgs.ReadArgs();
            settings.InferenceProvider ??= values.InferenceProvider;
            settings.ModelName ??= values.ModelName;
            settings.ApiToken ??= values.ApiToken;
            settings.Temperature ??= values.Temperature.GetValueOrDefault(0.7M);
            settings.TopP ??= values.TopP.GetValueOrDefault(0.9M);
            SaveSettings(settings);
        }
        return settings;
    }

    private static Settings ReadSettings()
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

    private static void SaveSettings(Settings settings)
    {
        var settingsJson = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsFilePath, settingsJson);
    }
}
