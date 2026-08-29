using System.Windows;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Interaction;

public sealed class CreatureDragController
{
    public bool IsDragging { get; private set; }

    public Point DragOffset { get; private set; }

    public void Begin(
        Creature creature,
        Point dragOffset)
    {
        IsDragging = true;
        DragOffset = dragOffset;

        creature.OnPickedUp();
    }

    public Point GetPosition(
        Point cursorPosition)
    {
        return new Point(
            cursorPosition.X - DragOffset.X,
            cursorPosition.Y - DragOffset.Y);
    }

    public void End(Creature creature)
    {
        IsDragging = false;

        creature.Release();
    }
}