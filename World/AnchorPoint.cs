using System.Windows;
using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public class AnchorPoint
{
    public string Name { get; set; }
    public AnchorPointType Type { get; set; }
    public Point Offset { get; set; }

    public AnchorPoint(string name, AnchorPointType type, Point offset)
    {
        Name = name;
        Type = type;
        Offset = offset;
    }
}