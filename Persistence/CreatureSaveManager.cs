using System.IO;
using System.Text.Json;

namespace Desktop_Creatures.Persistence;

public static class CreatureSaveManager
{
    private static readonly string SaveDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "DesktopCreatures");

    private static readonly string SavePath =
        Path.Combine(
            SaveDirectory,
            "creatures.json");

    public static void Save(
        IEnumerable<CreatureSaveData> creatures)
    {
        Directory.CreateDirectory(
            SaveDirectory);

        var options =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        string json =
            JsonSerializer.Serialize(
                creatures,
                options);

        File.WriteAllText(
            SavePath,
            json);
    }

    public static List<CreatureSaveData> Load()
    {
        if (!File.Exists(SavePath))
            return new List<CreatureSaveData>();

        string json =
            File.ReadAllText(
                SavePath);

        return JsonSerializer.Deserialize<
                   List<CreatureSaveData>>(json)
               ?? new List<CreatureSaveData>();
    }
}