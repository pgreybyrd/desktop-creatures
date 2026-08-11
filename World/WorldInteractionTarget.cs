using Desktop_Creatures.World;

public class WorldInteractionTarget
{
    public PointOfInterest PointOfInterest { get; }

    public WorldInteractionPoint InteractionPoint { get; }

    public System.Windows.Point Position =>
        PointOfInterest.GetWorldInteractionPointPosition(
            InteractionPoint);

    public bool IsValid =>
        PointOfInterest.IsEnabled;

    public WorldInteractionTarget(
        PointOfInterest pointOfInterest,
        WorldInteractionPoint interactionPoint)
    {
        PointOfInterest = pointOfInterest;
        InteractionPoint = interactionPoint;
    }
}