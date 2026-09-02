using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Desktop_Creatures.Creatures;

public static class CreatureDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public static CreatureDefinition Load(
        string creatureId)
    {
        string path =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Data",
                "Creatures",
                "Definitions",
                $"{creatureId}.json");

        string json =
            File.ReadAllText(path);

        return JsonSerializer.Deserialize<CreatureDefinition>(
            json,
            JsonOptions)
            ?? throw new InvalidOperationException(
                $"Could not load creature definition '{creatureId}'.");
    }

    public static Dictionary<string, CreatureDefinition> LoadAll()
    {
        string directory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Data",
                "Creatures",
                "Definitions");

        var definitions =
            new Dictionary<string, CreatureDefinition>(
                StringComparer.OrdinalIgnoreCase);

        foreach (string path in
                 Directory.EnumerateFiles(
                     directory,
                     "*.json"))
        {
            string json =
                File.ReadAllText(path);

            CreatureDefinition definition =
                JsonSerializer.Deserialize<CreatureDefinition>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Could not load creature definition '{path}'.");

            if (!definitions.TryAdd(
                    definition.Id,
                    definition))
            {
                throw new InvalidOperationException(
                    $"Duplicate creature definition ID '{definition.Id}'.");
            }
        }

        return definitions;
    }
}