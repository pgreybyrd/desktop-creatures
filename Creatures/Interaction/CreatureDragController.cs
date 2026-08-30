using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Interaction;

public sealed class CreatureDragController(
    SurfaceManager surfaceManager)
{
    public bool IsDragging { get; private set; }

    public Point DragOffset { get; private set; }

    public bool IsOnDesktop(
        Point point)
    {
        return surfaceManager.IsPointOnDesktop(
            point);
    }

    public void Begin(
        Creature creature)
    {
        IsDragging = true;

        DragOffset =
            new Point(
                creature.PickupAnchor.X *
                    creature.DisplayScale,
                creature.PickupAnchor.Y *
                    creature.DisplayScale);

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

    public Point GetCreaturePosition(
        Creature creature,
        Point windowPosition)
    {
        double displayScale =
            creature.DisplayScale;

        double extraWidth =
            creature.SpriteWidth *
            (displayScale - 1);

        double extraHeight =
            creature.CurrentFootY *
            (displayScale - 1);

        return new Point(
            windowPosition.X +
                (extraWidth / 2.0),
            windowPosition.Y +
                extraHeight);
    }

    public Rectangle GetMonitorBoundsUnderCursor()
    {
        return surfaceManager.GetMonitorBoundsUnderCursor();
    }

    public Point ConstrainPosition(
        Point position,
        double width,
        double height,
        Point cursorPosition)
    {
        var monitor =
            GetMonitorBoundsUnderCursor();

        double x =
            ConstrainHorizontalPosition(
                position.X,
                width,
                cursorPosition.Y,
                monitor.Left,
                monitor.Right);

        double y =
            ConstrainVerticalPosition(
                position.Y,
                height,
                cursorPosition.X,
                monitor.Top,
                monitor.Bottom);

        return new Point(
            x,
            y);
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

    public double ConstrainVerticalPosition(
        double y,
        double height,
        double cursorX,
        double monitorTop,
        double monitorBottom)
    {
        Point topProbe =
            new (
                cursorX,
                y - 1);

        Point bottomProbe =
            new (
                cursorX,
                y + height + 1);

        if (!IsOnDesktop(
            topProbe))
        {
            y = Math.Max(
                y,
                monitorTop);
        }

        if (!IsOnDesktop(
            bottomProbe))
        {
            y = Math.Min(
                y,
                monitorBottom - height);
        }

        return y;
    }
}