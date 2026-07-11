using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public enum AnchorPointType
{
    Stand,
    Eat,
    Drink,
    Perch,
    Sleep,
    Rest,
    Enter,
    Exit,
    Observe,
    Play
}

public class AnchorPoint
{
    public string Name { get; }
    public AnchorPointType Type { get; }
    public Point Offset { get; }

    public bool IsAvailable { get; set; } = true;

    public AnchorPoint(
        string name,
        AnchorPointType type,
        Point offset)
    {
        Name = name;
        Type = type;
        Offset = offset;
    }
}