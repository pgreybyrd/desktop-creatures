using System.Windows;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Interaction;

public sealed class CreatureDragController
{
    public bool IsDragging { get; private set; }

    public Point DragOffset { get; private set; }

    public void Begin(Point dragOffset)
    {
        IsDragging = true;
        DragOffset = dragOffset;
    }

    public void End()
    {
        IsDragging = false;
    }
}