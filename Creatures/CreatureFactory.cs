using Desktop_Creatures.Config;

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