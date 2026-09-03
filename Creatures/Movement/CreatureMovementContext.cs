namespace Desktop_Creatures.Creatures.Movement;

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

    public required Func<int> GetDisplayScale { get; init; }

    public required Func<int, int, int> NextRandom { get; init; }

    public required Action<
        CreatureAction,
        string> SetAction
    { get; init; }
}