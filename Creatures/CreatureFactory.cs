using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;

namespace Desktop_Creatures.Creatures;

public static class CreatureFactory
{
    public static Creature Create(
        CreatureDefinition definition,
        CreatureSpawnContext context,
        CreatureSettings settings,
        CreatureServices services)
    {
        return definition.Id.ToLowerInvariant() switch
        {
            "rat" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            "squirrel" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            "skunk" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            "hummingbird" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            "eagle" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            "ocelot" =>
                new DataDrivenCreature(
                    definition,
                    context,
                    settings,
                    services),

            _ =>
                throw new NotSupportedException(
                    $"Creature type '{definition.Id}' is not supported.")
        };
    }
}