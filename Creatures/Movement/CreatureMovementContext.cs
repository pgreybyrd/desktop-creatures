namespace Desktop_Creatures.Creatures.Movement;
using Desktop_Creatures.World.Surfaces;

public sealed class CreatureMovementContext
{
    public required Func<double> GetX { get; init; }
    public required Action<double> SetX { get; init; }

    public required Func<double> GetY { get; init; }
    public required Action<double> SetY { get; init; }

    public required Func<double> GetSpeedX { get; init; }
    public required Action<double> SetSpeedX { get; init; }

    public required Func<int> GetSpriteWidth { get; init; }
    public required Func<int> GetSpriteHeight { get; init; }

    public required Func<int> GetFootY { get; init; }

    public required Func<CreatureAction> GetAction { get; init; }

    public required Action<double> SetTargetX { get; init; }
    public required Action<double> SetTargetY { get; init; }

    public required Action<double> SetMovementSpeed { get; init; }

    public required Action<int> SetStateTicksRemaining { get; init; }
    public required Func<int> GetStateTicksRemaining { get; init; }

    public required Func<int> GetDisplayScale { get; init; }

    public required Func<int, int, int> NextRandom { get; init; }

    public required Func<double> GetTargetX { get; init; }
    public required Func<double> GetTargetY { get; init; }

    public required Func<double> GetMovementSpeed { get; init; }

    public required Func<double> GetFallSpeed { get; init; }
    public required Action<double> SetFallSpeed { get; init; }

    public required Func<double> GetDisplayCenterX { get; init; }

    public required Func<int> GetScale { get; init; }

    public required Func<double> GetFrameMovement { get; init; }

    public required Action<
        CreatureAction,
        string> SetAction
    { get; init; }

    public required Func<bool> HasInteractionTarget { get; init; }

    public required Action OnOrdinaryTargetReached { get; init; }

    public required Action OnInteractionTargetReached { get; init; }

    public required Func<bool> IsStillOnSurface { get; init; }

    public required Func<Surface?> GetCurrentSurface { get; init; }

    public required Action<Surface?> SetCurrentSurface { get; init; }

    public required Action StartFalling { get; init; }
}