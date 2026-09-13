using Desktop_Creatures.Config;
using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class AirMovementSpace : IMovementSpace
{
    private readonly SurfaceManager _surfaceManager;
    private readonly FlightSettings _flight;

    public AirMovementSpace(
        SurfaceManager surfaceManager,
        FlightSettings flight)
    {
        _surfaceManager =
            surfaceManager;

        _flight =
            flight;
    }

    public bool TryPickTarget(
        CreatureMovementContext context,
        out MovementDestination destination)
    {
        IReadOnlyList<Rectangle> monitorBounds =
            _surfaceManager.GetMonitorBounds();

        if (monitorBounds.Count == 0)
        {
            destination =
                new MovementDestination(
                    context.GetX(),
                    context.GetY());

            return false;
        }

        int spriteWidth =
            context.GetSpriteWidth();

        int spriteHeight =
            context.GetSpriteHeight();

        double targetX;
        double targetY;

        bool useLocalRange =
            _flight.MinTravelDistance is not null &&
            _flight.MaxTravelDistance is not null;

        if (useLocalRange)
        {
            bool foundTarget =
                false;

            targetX =
                context.GetX();

            targetY =
                context.GetY();

            const int maxAttempts =
                20;

            for (int attempt = 0;
                 attempt < maxAttempts;
                 attempt++)
            {
                double angle =
                    context.NextRandom(
                        0,
                        360) *
                    Math.PI /
                    180.0;

                int distance =
                    context.NextRandom(
                        _flight.MinTravelDistance!.Value,
                        _flight.MaxTravelDistance!.Value);

                double candidateX =
                    context.GetX() +
                    Math.Cos(angle) *
                    distance;

                double candidateY =
                    context.GetY() +
                    Math.Sin(angle) *
                    distance;

                if (!CanTraverseTo(
                        context,
                        candidateX,
                        candidateY))
                {
                    continue;
                }

                targetX =
                    candidateX;

                targetY =
                    candidateY;

                foundTarget =
                    true;

                break;
            }

            if (!foundTarget &&
                !TryPickReachableDesktopTarget(
                    context,
                    monitorBounds,
                    spriteWidth,
                    spriteHeight,
                    out targetX,
                    out targetY))
            {
                destination =
                    new MovementDestination(
                        context.GetX(),
                        context.GetY());

                return false;
            }
        }
        else
        {
            if (!TryPickReachableDesktopTarget(
                    context,
                    monitorBounds,
                    spriteWidth,
                    spriteHeight,
                    out targetX,
                    out targetY))
            {
                destination =
                    new MovementDestination(
                        context.GetX(),
                        context.GetY());

                return false;
            }
        }

        destination =
            new MovementDestination(
                targetX,
                targetY);

        return true;
    }

    public bool CanReach(
        CreatureMovementContext context,
        MovementDestination destination)
    {
        return
            CanTraverseTo(
                context,
                destination.X,
                destination.Y);
    }

    public bool TryResolveDestination(
        CreatureMovementContext context,
        MovementDestination requested,
        out MovementDestination resolved)
    {
        resolved =
            requested;

        return
            CanReach(
                context,
                requested);
    }

    public bool IsPositionValid(
        CreatureMovementContext context,
        double x,
        double y)
    {
        return
            PositionFitsOnDesktop(
                context,
                x,
                y);
    }

    private void PickRandomDesktopTarget(
        CreatureMovementContext context,
        IReadOnlyList<Rectangle> monitorBounds,
        int spriteWidth,
        int spriteHeight,
        out double targetX,
        out double targetY)
    {
        Rectangle area =
            monitorBounds[
                context.NextRandom(
                    0,
                    monitorBounds.Count)];

        int minX =
            area.Left;

        int maxX =
            area.Right -
            spriteWidth;

        int minY =
            area.Top;

        int maxY =
            area.Bottom -
            spriteHeight;

        targetX =
            context.NextRandom(
                minX,
                maxX);

        targetY =
            context.NextRandom(
                minY,
                maxY);
    }

    private bool TryPickReachableDesktopTarget(
        CreatureMovementContext context,
        IReadOnlyList<Rectangle> monitorBounds,
        int spriteWidth,
        int spriteHeight,
        out double targetX,
        out double targetY)
    {
        const int maxAttempts =
            30;

        for (int attempt = 0;
             attempt < maxAttempts;
             attempt++)
        {
            PickRandomDesktopTarget(
                context,
                monitorBounds,
                spriteWidth,
                spriteHeight,
                out double candidateX,
                out double candidateY);

            if (!CanTraverseTo(
                    context,
                    candidateX,
                    candidateY))
            {
                continue;
            }

            targetX =
                candidateX;

            targetY =
                candidateY;

            return true;
        }

        targetX =
            context.GetX();

        targetY =
            context.GetY();

        return false;
    }

    private bool PositionFitsOnDesktop(
        CreatureMovementContext context,
        double x,
        double y)
    {
        double right =
            x +
            context.GetSpriteWidth();

        double bottom =
            y +
            context.GetSpriteHeight();

        return
            _surfaceManager.IsPointOnDesktop(
                new Point(x, y)) &&
            _surfaceManager.IsPointOnDesktop(
                new Point(right, y)) &&
            _surfaceManager.IsPointOnDesktop(
                new Point(x, bottom)) &&
            _surfaceManager.IsPointOnDesktop(
                new Point(right, bottom));
    }

    private bool CanTraverseTo(
        CreatureMovementContext context,
        double targetX,
        double targetY)
    {
        double startX =
            context.GetX();

        double startY =
            context.GetY();

        double dx =
            targetX -
            startX;

        double dy =
            targetY -
            startY;

        double distance =
            Math.Sqrt(
                dx * dx +
                dy * dy);

        const double sampleSpacing =
            12.0;

        int sampleCount =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    distance /
                    sampleSpacing));

        for (int i = 1;
             i <= sampleCount;
             i++)
        {
            double progress =
                (double)i /
                sampleCount;

            double x =
                startX +
                (dx * progress);

            double y =
                startY +
                (dy * progress);

            if (!PositionFitsOnDesktop(
                    context,
                    x,
                    y))
            {
                return false;
            }
        }

        return true;
    }
}