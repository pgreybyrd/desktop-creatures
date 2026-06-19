using System.IO;
using System.Text.Json;

namespace Desktop_Creatures.Config;

public static class SettingsLoader
{
    public static AppSettings Load()
    {
        const string path = "Config/settings.json";

        if (!File.Exists(path))
            return new AppSettings();

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<AppSettings>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new AppSettings();
    }
}