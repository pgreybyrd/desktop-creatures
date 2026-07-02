using Point = System.Windows.Point;
using Desktop_Creatures.Config;

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
    public PointOfInterestSettings Settings { get; }
    public AppSettings AppSettings { get; }

    public bool IsAvailable { get; set; }
    public List<AnchorPoint> AnchorPoints { get; set; } = new();

    public PointOfInterest(
        string name, 
        Point position, 
        PointOfInterestType type, 
        PointOfInterestSettings settings,
        AppSettings appSettings)
    {
        Name = name;
        Position = position;
        Type = type;
        IsAvailable = true;
        Settings = settings;
        AppSettings = appSettings;
    }

    public void AddAnchor(AnchorPoint point)
        { AnchorPoints.Add(point); }

    public Point GetAnchorPosition(AnchorPoint anchor)
    {
        return new Point(
            Position.X + anchor.Offset.X,
            Position.Y + anchor.Offset.Y
        );
    }
}