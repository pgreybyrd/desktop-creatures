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

    public Point ClampPosition(
        Point position,
        double left,
        double top,
        double right,
        double bottom,
        double width,
        double height)
    {
        return new Point(
            Math.Clamp(
                position.X,
                left,
                right - width),
            Math.Clamp(
                position.Y,
                top,
                bottom - height));
    }

    public void End(Creature creature)
    {
        IsDragging = false;

        creature.Release();
    }
}