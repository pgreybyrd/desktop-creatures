using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public class WorldInteractionTarget
{
    public PointOfInterest PointOfInterest { get; }
    public WorldInteractionPoint InteractionPoint { get; }
    public Point Position { get; }

    public WorldInteractionTarget(
        PointOfInterest pointOfInterest,
        WorldInteractionPoint interactionPoint,
        Point position)
    {
        PointOfInterest = pointOfInterest;
        InteractionPoint = interactionPoint;
        Position = position;
    }
}