namespace Desktop_Creatures.Creatures.Movement;

public interface ICreatureMovement
{
    MovementCapability Capability { get; }

    void Initialize();

    bool HandlesAction(
        CreatureAction action);

    void Update();

    void Release();

    void PickNewTarget();

    bool CanReach(
        MovementDestination destination);

    bool TrySetDestination(
        MovementDestination destination);
}