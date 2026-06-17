using Point = System.Drawing.Point;

namespace Desktop_Creatures.World;

public class PointOfInterest
{
    public string Name { get; set; }
    public Point Position { get; set; }
    public PointOfInterestType Type { get; set; }

    public PointOfInterest(string name, Point position, PointOfInterestType type)
    {
        Name = name;
        Position = position;
        Type = type;
    }
}