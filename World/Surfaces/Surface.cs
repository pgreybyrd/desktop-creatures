
namespace Desktop_Creatures.World.Surfaces;

public class Surface
{
    public Rectangle Bounds { get; }

    public int Left => Bounds.Left;
    public int Right => Bounds.Right;
    public int Top => Bounds.Top;
    public int Bottom => Bounds.Bottom;
    public int Width => Bounds.Width;

    public string Kind { get; set;  }

    public bool IsWindow { get; }
    public bool IsBranch { get; }
    public bool IsGround { get; }

    public Surface(Rectangle bounds, string kind = "Unknown")
    {
        Bounds = bounds;
        Kind = kind;
    }
}