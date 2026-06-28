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
    public string AssetPath { get; set; }
    public string? EmptyAssetPath { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsAvailable { get; set; }
    public List<AnchorPoint> AnchorPoints { get; set; } = new();

    public PointOfInterest(
        string name, 
        Point position, 
        PointOfInterestType type, 
        string assetPath)
    {
        Name = name;
        Position = position;
        Type = type;
        AssetPath = assetPath;

        Width = 16;
        Height = 16;

        EmptyAssetPath = null;
        IsAvailable = true;
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