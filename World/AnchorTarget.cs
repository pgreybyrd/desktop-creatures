using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public class AnchorTarget
{
    public PointOfInterest PointOfInterest { get; }
    public AnchorPoint Anchor { get; }
    public Point Position { get; }

    public AnchorTarget(
        PointOfInterest pointOfInterest,
        AnchorPoint anchor,
        Point position)
    {
        PointOfInterest = pointOfInterest;
        Anchor = anchor;
        Position = position;
    }
}