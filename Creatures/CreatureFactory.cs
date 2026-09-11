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
        return new DataDrivenCreature(
            definition,
            context,
            settings,
            services);
    }
}