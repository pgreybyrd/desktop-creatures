using System.IO;
using System.Text.Json;

namespace Desktop_Creatures.Persistence;

public static class CreatureSaveManager
{
    public const int CurrentVersion = 2;

    private static readonly string SaveDirectory =
        AppDataPaths.RootDirectory;

    private static readonly string SavePath =
        AppDataPaths.CreatureSavePath;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    public static void Save(
        IEnumerable<CreatureRecord> creatures)
    {
        Directory.CreateDirectory(
            SaveDirectory);

        var saveFile =
            new CreatureSaveFile
            {
                Version = CurrentVersion,
                Creatures =
                    creatures.ToList()
            };

        string json =
            JsonSerializer.Serialize(
                saveFile,
                JsonOptions);

        File.WriteAllText(
            SavePath,
            json);
    }

    public static CreatureSaveFile Load()
    {
        if (!File.Exists(SavePath))
        {
            return new CreatureSaveFile
            {
                Version = CurrentVersion
            };
        }

        string json =
            File.ReadAllText(
                SavePath);

        try
        {
            CreatureSaveFile? saveFile =
                JsonSerializer.Deserialize<
                    CreatureSaveFile>(
                    json,
                    JsonOptions);

            if (saveFile is not null &&
                saveFile.Version > 0)
            {
                return saveFile;
            }
        }
        catch (JsonException)
        {
            // It may be an old v1 array.
        }

        return MigrateLegacySave(json);
    }

    private static CreatureSaveFile MigrateLegacySave(
        string json)
    {
        List<CreatureSaveData>? oldCreatures =
            JsonSerializer.Deserialize<
                List<CreatureSaveData>>(
                json,
                JsonOptions);

        if (oldCreatures is null)
        {
            return new CreatureSaveFile
            {
                Version = CurrentVersion
            };
        }

        return new CreatureSaveFile
        {
            Version = CurrentVersion,

            Creatures =
                oldCreatures
                    .Select(old =>
                        new CreatureRecord
                        {
                            Id = old.Id,

                            CreatureType =
                                old.CreatureType,

                            Name =
                                old.Name,

                            AppearanceId =
                                old.AppearanceId,

                            AppearanceTraits =
                                old.AppearanceTraits,

                            LastX =
                                old.X,

                            LastY =
                                old.Y
                        })
                    .ToList()
        };
    }
}