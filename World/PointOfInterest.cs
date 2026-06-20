using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public enum PointOfInterestType
{
    Rest,
    Home,
    Food,
    Water,
    Decoration,
    Magic,
    Breed
}

public class PointOfInterest
{
    public string Name { get; set; }
    public Point Position { get; set; }
    public PointOfInterestType Type { get; set; }
    public List<AnchorPoint> AnchorPoints { get; set; } = new();

    public PointOfInterest(string name, Point position, PointOfInterestType type)
    {
        Name = name;
        Position = position;
        Type = type;
    }

    public Point GetAnchorPosition(AnchorPoint anchor)
    {
        return new Point(
            Position.X + anchor.Offset.X,
            Position.Y + anchor.Offset.Y
        );
    }
}