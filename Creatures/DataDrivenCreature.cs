using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures.Movement;
using Desktop_Creatures.Utilities;

namespace Desktop_Creatures.Creatures;

public sealed class DataDrivenCreature : Creature
{
    private readonly List<ICreatureMovement> _movements = [];
    private ICreatureMovement? _activeMovement;

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

    protected override void UpdateState(
        double deltaSeconds)
    {
        ICreatureMovement? movement =
            _movements.FirstOrDefault(
                movement =>
                    movement.HandlesAction(
                        CurrentAction));

        if (movement is null)
        {
            base.UpdateState(
                deltaSeconds);

            return;
        }

        movement.Update(
            deltaSeconds);
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

        if (definition.MovementCapabilities.Contains(
                MovementCapability.Flight))
        {
            var flightMovement =
                new FlightMovement(
                    CreateMovementContext(),
                    services.SurfaceManager,
                    Settings.Flight
                        ?? throw new InvalidOperationException(
                            $"Flying creature '{definition.Id}' requires FlightSettings."),
                    Settings.Hover,
                    Settings.Glide,
                    Settings.Perch);

            _movements.Add(
                flightMovement);

            flightMovement.Initialize();
            flightMovement.PickNewTarget();
        }

        if (_movements.Count == 0)
        {
            throw new NotSupportedException(
                $"Creature '{definition.Id}' has no supported movement capability.");
        }
    }

    protected override bool TrySetMovementDestination(
        MovementDestination destination)
    {
        Logger.LogDebug(
            DebugCategory.Movement,
            $"MOVEMENT REQUEST: " +
            $"creature={CreatureType} " +
            $"destination=({destination.X:F1},{destination.Y:F1}) " +
            $"capabilities={string.Join(",", _movements.Select(m => m.Capability))}");

        foreach (ICreatureMovement movement in
                 _movements)
        {
            bool canReach =
                movement.CanReach(
                    destination);

            Logger.LogDebug(
                DebugCategory.Movement,
                $"CAPABILITY CHECK: " +
                $"creature={CreatureType} " +
                $"capability={movement.Capability} " +
                $"canReach={canReach}");

            if (!canReach)
                continue;

            bool accepted =
                movement.TrySetDestination(
                    destination);

            Logger.LogDebug(
                DebugCategory.Movement,
                $"CAPABILITY DESTINATION: " +
                $"creature={CreatureType} " +
                $"capability={movement.Capability} " +
                $"accepted={accepted}");

            if (accepted)
            {
                _activeMovement =
                    movement;

                return true;
            }
        }

        Logger.LogDebug(
            DebugCategory.Movement,
            $"MOVEMENT FAILED: " +
            $"creature={CreatureType} " +
            $"destination=({destination.X:F1},{destination.Y:F1})");

        return false;
    }

    protected override void PickPostInteractionTarget()
    {
        ICreatureMovement? movement =
            _activeMovement ??
            _movements.FirstOrDefault();

        if (movement is null)
        {
            base.PickPostInteractionTarget();
            return;
        }

        movement.PickNewTarget();
    }

    public override void OnPickedUp()
    {
        AbandonCurrentInteraction();

        PlaySound(
            CreatureSoundEvent.Pickup);

        SetAction(
            CreatureAction.Held,
            "held");
    }

    public override void Release()
    {
        ICreatureMovement? movement =
            _movements.FirstOrDefault();

        if (movement is null)
        {
            base.Release();
            return;
        }

        movement.Release();
    }
}