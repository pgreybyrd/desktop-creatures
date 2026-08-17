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
                $"{creatureId}.json");

        string json =
            File.ReadAllText(path);

        return JsonSerializer.Deserialize<CreatureDefinition>(
            json,
            JsonOptions)
            ?? throw new InvalidOperationException(
                $"Could not load creature definition '{creatureId}'.");
    }
}