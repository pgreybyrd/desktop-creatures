using Point = System.Windows.Point;

namespace Desktop_Creatures.World;

public enum WorldInteractionPointType
{
    Stand,
    Eat,
    Drink,
    Nectar,
    Perch,
    Sleep,
    Rest,
    Enter,
    Exit,
    Observe,
    Play
}

public enum InteractionFacing
{
    Any,
    Left,
    Right
}

public class WorldInteractionPoint
{
    public string Name { get; }
    public WorldInteractionPointType Type { get; }
    public Point Offset { get; }

    public bool IsAvailable { get; set; } = true;

    public InteractionFacing Facing { get; }

    public WorldInteractionPoint(
        string name,
        WorldInteractionPointType type,
        Point offset,
        InteractionFacing facing = InteractionFacing.Any)
    {
        Name = name;
        Type = type;
        Offset = offset;
        Facing = facing;
    }

    public bool TryReserve()
    {
        if (!IsAvailable)
            return false;

        IsAvailable = false;
        return true;
    }

    public void Release()
    {
        IsAvailable = true;
    }
}