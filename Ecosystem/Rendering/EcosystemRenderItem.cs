using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Ecosystem.Rendering;

public enum EcosystemEntityKind
{
    Creature,
    PointOfInterest
}

public sealed record EcosystemRenderItem
{
    public required Guid EntityId { get; init; }

    public required BitmapSource Image { get; init; }

    public required Rect WorldBounds { get; init; }

    public required EcosystemEntityKind EntityKind { get; init; }

    public bool IsMirrored { get; init; }

    public bool IsInteractive { get; init; } = true;

    public int ZIndex { get; init; }
}