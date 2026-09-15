using Desktop_Creatures.Creatures;
using Desktop_Creatures.World.Surfaces;

namespace Desktop_Creatures.Ecosystem;

public sealed class EcosystemHost
{
    private readonly CreatureManager
        _creatureManager;

    private readonly SurfaceManager
        _surfaceManager;

    public EcosystemHost(
        CreatureManager creatureManager,
        SurfaceManager surfaceManager)
    {
        _creatureManager =
            creatureManager;

        _surfaceManager =
            surfaceManager;
    }

    public void Update(
        double deltaSeconds,
        Func<Creature, bool>? shouldUpdateCreature = null)
    {
        _surfaceManager.Update(
            deltaSeconds);

        _creatureManager.Update(
            deltaSeconds,
            shouldUpdateCreature);
    }
}