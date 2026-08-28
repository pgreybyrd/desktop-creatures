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

public enum CreatureFamily
{
    Rodent,
    Bird,
    Feline
}

public sealed record CreatureAppearanceSettings
{
    public bool Generated { get; init; }
}

public sealed record CreaturePickupAnchor
{
    public double X { get; init; }
    public double Y { get; init; }
}

public sealed record CreatureDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required CreatureFamily Family { get; init; }

    public MovementCapability[] MovementCapabilities { get; init; } = [];

    public required string Category { get; init; }

    public string AssetFolder =>
    $"Assets/Creatures/{ToFolderName(Category)}/{ToFolderName(Id)}";

    private static string ToFolderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return char.ToUpperInvariant(value[0]) +
               value[1..];
    }

    public CreatureAppearanceSettings? Appearance { get; init; }

    public required CreaturePickupAnchor PickupAnchor { get; init; }

    public string[] Palettes { get; init; } = [];
}