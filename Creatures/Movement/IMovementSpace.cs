namespace Desktop_Creatures.Creatures.Movement;

public interface IMovementSpace
{
    bool TryPickTarget(
        CreatureMovementContext context,
        out MovementDestination destination);

    bool CanReach(
        CreatureMovementContext context,
        MovementDestination destination);

    bool TryResolveDestination(
        CreatureMovementContext context,
        MovementDestination requested,
        out MovementDestination resolved);

    bool IsPositionValid(
        CreatureMovementContext context,
        double x,
        double y);
}