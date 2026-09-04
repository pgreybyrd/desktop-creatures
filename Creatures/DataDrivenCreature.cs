using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures.Movement;

namespace Desktop_Creatures.Creatures;

public sealed class DataDrivenCreature : Creature
{
    private readonly List<ICreatureMovement> _movements = [];

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

        InitializeMovementCapabilities(
            definition,
            services);

        PlaySound(
            CreatureSoundEvent.Spawn);
    }

    protected override void UpdateState()
    {
        ICreatureMovement? movement =
            _movements.FirstOrDefault(
                movement =>
                    movement.HandlesAction(
                        CurrentAction));

        if (movement is null)
        {
            base.UpdateState();
            return;
        }

        movement.Update();
    }

    private void InitializeMovementCapabilities(
        CreatureDefinition definition,
        CreatureServices services)
    {
        if (definition.MovementCapabilities.Contains(
                MovementCapability.Ground))
        {
            var groundMovement =
                new GroundMovement(
                    CreateMovementContext(),
                    services.SurfaceManager,
                    Settings.Run
                        ?? throw new InvalidOperationException(
                            $"Ground creature '{definition.Id}' requires RunSettings."),
                    Settings.Fall
                        ?? throw new InvalidOperationException(
                            $"Ground creature '{definition.Id}' requires FallSettings."));

            _movements.Add(
                groundMovement);


            groundMovement.Initialize();
            groundMovement.PickNewTarget();
        }

        if (_movements.Count == 0)
        {
            throw new NotSupportedException(
                $"Creature '{definition.Id}' has no supported movement capability.");
        }
    }

    public override void OnPickedUp()
    {
        PlaySound(
            CreatureSoundEvent.Pickup);

        SetAction(
            CreatureAction.Held,
            "dangle");
    }

    public override void Release()
    {
        ICreatureMovement? movement =
            _movements.FirstOrDefault(
                movement =>
                    movement.Capability ==
                    MovementCapability.Ground);

        if (movement is null)
        {
            base.Release();
            return;
        }

        movement.Release();
    }
}