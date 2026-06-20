using System.IO;
using System.Text.Json;

namespace Desktop_Creatures.Config;

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
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new();
    }
}