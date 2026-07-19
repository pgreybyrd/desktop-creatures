using Point = System.Windows.Point;
using Desktop_Creatures.Config;

namespace Desktop_Creatures.World;

public enum PointOfInterestType
{
    Food,
    Water,
    Home,
    Shelter,
    Rest,
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

    public bool IsEnabled { get; set; }
    public List<WorldInteractionPoint> AnchorPoints { get; set; } = new();

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
        Settings = settings;
        AppSettings = appSettings;
        IsEnabled = true;
    }

    public void AddWorldInteractionPoint(WorldInteractionPoint point)
    { 
        AnchorPoints.Add(point); 
    }

    public Point GetWorldInteractionPointPosition(WorldInteractionPoint anchor)
    {
        double scale = AppSettings.Scale;

        return new Point(
            Position.X + anchor.Offset.X * scale,
            Position.Y + anchor.Offset.Y * scale);
    }

    public IEnumerable<WorldInteractionPoint> GetWorldInteractionPoints(
        WorldInteractionPointType type,
        bool availableOnly = true)
    {
        return AnchorPoints.Where(anchor =>
            anchor.Type == type &&
            (!availableOnly || anchor.IsAvailable));
    }
}