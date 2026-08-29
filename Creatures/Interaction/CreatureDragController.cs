using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Interaction;

public sealed class CreatureDragController
{
    private readonly SurfaceManager _surfaceManager;

    public bool IsDragging { get; private set; }

    public Point DragOffset { get; private set; }

    public CreatureDragController(
        SurfaceManager surfaceManager)
    {
        _surfaceManager = surfaceManager;
    }

    public bool IsOnDesktop(
        Point point)
    {
        return _surfaceManager.IsPointOnDesktop(
            point);
    }

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

    public double ConstrainHorizontalPosition(
        double x,
        double width,
        double cursorY,
        double monitorLeft,
        double monitorRight)
    {
        Point leftProbe =
            new (
                x - 1,
                cursorY);

        Point rightProbe =
            new (
                x + width + 1,
                cursorY);

        if (!IsOnDesktop(
            leftProbe))
        {
            x = Math.Max(
                x,
                monitorLeft);
        }

        if (!IsOnDesktop(
            rightProbe))
        {
            x = Math.Min(
                x,
                monitorRight - width);
        }

        return x;
    }
}