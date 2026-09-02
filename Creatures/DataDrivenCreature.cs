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

        InitializeGroundCreature(
            context.X,
            context.Y);

        PlaySound(
            CreatureSoundEvent.Spawn);
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