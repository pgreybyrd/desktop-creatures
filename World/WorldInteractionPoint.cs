using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public enum WorldInteractionPointType
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

public class WorldInteractionPoint
{
    public string Name { get; }
    public WorldInteractionPointType Type { get; }
    public Point Offset { get; }

    public bool IsAvailable { get; set; } = true;

    public WorldInteractionPoint(
        string name,
        WorldInteractionPointType type,
        Point offset)
    {
        Name = name;
        Type = type;
        Offset = offset;
    }
}