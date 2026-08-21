using PixelRecolor.Core;

namespace Desktop_Creatures.Persistence;

public sealed class CreatureRecord
{
    public Guid Id { get; set; }

    public string CreatureType { get; set; } = "";

    public string Name { get; set; } = "";

    public string? AppearanceId { get; set; }

    public CreatureAppearanceTraits? AppearanceTraits
    {
        get;
        set;
    }

    public bool IsFavorite { get; set; }

    public double LastX { get; set; }

    public double LastY { get; set; }
}