using Desktop_Creatures.World;

public class WorldInteractionTarget
{
    public PointOfInterest PointOfInterest { get; }
    public WorldInteractionPoint InteractionPoint { get; }

    public System.Windows.Point Position { get; }

    public WorldInteractionTarget(
        PointOfInterest pointOfInterest,
        WorldInteractionPoint interactionPoint,
        System.Windows.Point position)
    {
        PointOfInterest = pointOfInterest;
        InteractionPoint = interactionPoint;
        Position = position;
    }
}