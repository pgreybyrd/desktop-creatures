using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;

namespace Desktop_Creatures.Creatures;

public static class CreatureFactory
{
    public static Creature Create(
        CreatureDefinition definition,
        double x,
        double y,
        CreatureSettings settings,
        PointOfInterestManager pointOfInterestManager,
        SurfaceManager surfaceManager,
        Guid? id = null,
        string? name = null,
        CreatureAppearanceTraits? appearanceTraits = null,
        string? appearanceId = null)
    {
        return definition.Id.ToLowerInvariant() switch
        {
            "rat" =>
                new Rat(
                    x,
                    y,
                    settings,
                    pointOfInterestManager,
                    surfaceManager,
                    id,
                    name,
                    appearanceTraits,
                    appearanceId),

            _ =>
                throw new NotSupportedException(
                    $"Creature type '{definition.Id}' " +
                    "is not supported.")
        };
    }
}