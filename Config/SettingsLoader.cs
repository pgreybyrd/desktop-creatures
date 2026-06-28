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

public static class CreatureSettingsLoader
{
    public static Dictionary<string, CreatureSettings> Load()
    {
        const string path = "Config/creature_settings.json";

        if (!File.Exists(path))
            return new();

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<Dictionary<string, CreatureSettings>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new();
    }
}

public static class PointOfInterestSettingsLoader
{
    public static Dictionary<string, PointOfInterestSettings> Load()
    {
        const string path = "Config/point_of_interest_settings.json";

        if (!File.Exists(path))
            return new();

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<Dictionary<string, PointOfInterestSettings>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new();
    }
}