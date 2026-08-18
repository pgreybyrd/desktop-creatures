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
                new Rat(
                    definition,
                    context.X,
                    context.Y,
                    settings,
                    services.PointOfInterestManager,
                    services.SurfaceManager,
                    id: context.Id,
                    name: context.Name,
                    appearanceTraits: context.AppearanceTraits,
                    appearanceId: context.AppearanceId),

            "eagle" =>
                new Eagle(
                    context.X,
                    context.Y,
                    services.PointsOfInterest,
                    settings,
                    services.PointOfInterestManager,
                    services.MonitorWorkingAreas,
                    services.SurfaceManager),

            "ocelot" =>
                new Ocelot(
                    definition,
                    context.X,
                    context.Y,
                    settings,
                    services.PointOfInterestManager,
                    services.SurfaceManager,
                    id: context.Id,
                    name: context.Name,
                    appearanceTraits: context.AppearanceTraits,
                    appearanceId: context.AppearanceId),

            _ =>
                throw new NotSupportedException(
                    $"Creature type '{definition.Id}' is not supported.")
        };
    }
}