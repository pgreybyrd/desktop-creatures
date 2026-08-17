namespace Desktop_Creatures.Creatures;

public enum MovementCapability
{
    Ground,
    Flight,
    Swimming,
    Climbing,
    Slithering,
    Hopping,
    Floating
}

public sealed record CreatureDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public MovementCapability[] MovementCapabilities { get; init; } = [];

    public required string AssetFolder { get; init; }

    public bool UsesGeneratedAppearance { get; init; }

    public string[] Palettes { get; init; } = [];

    public double PickupAnchorX { get; init; }

    public double PickupAnchorY { get; init; }
}