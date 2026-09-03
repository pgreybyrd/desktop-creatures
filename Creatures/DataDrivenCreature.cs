using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;

namespace Desktop_Creatures.Creatures;

public sealed class DataDrivenCreature : Creature
{
    public DataDrivenCreature(
        CreatureDefinition definition,
        CreatureSpawnContext context,
        CreatureSettings settings,
        CreatureServices services)
        : base(
            definition,
            settings,
            services.PointOfInterestManager,
            services.SurfaceManager,
            context.Id,
            context.Name)
    {
        InitializeCreatureAssets(
            definition,
            context.AppearanceTraits,
            context.AppearanceId);

        InitializeSounds(
            definition);

        InitializePosition(
            context.X,
            context.Y);

        InitializeMovement(
            definition);

        PlaySound(
            CreatureSoundEvent.Spawn);
    }

    private void InitializeMovement(
        CreatureDefinition definition)
    {
        if (definition.MovementCapabilities.Contains(
                MovementCapability.Ground))
        {
            InitializeGroundMovement();
            return;
        }

        throw new NotSupportedException(
            $"Creature '{definition.Id}' has no supported movement capability.");
    }

    public override void OnPickedUp()
    {
        PlaySound(
            CreatureSoundEvent.Pickup);

        SetAction(
            CreatureAction.Held,
            "dangle");
    }
}