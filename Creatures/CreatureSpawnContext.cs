using PixelRecolor.Core;

namespace Desktop_Creatures.Creatures;

public sealed record CreatureSpawnContext
{
    public required double X { get; init; }
    public required double Y { get; init; }

    public Guid? Id { get; init; }
    public string? Name { get; init; }

    public CreatureAppearanceTraits? AppearanceTraits { get; init; }
    public string? AppearanceId { get; init; }
}