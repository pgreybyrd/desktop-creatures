using Desktop_Creatures.Utilities;
using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class SurfaceMovementSpace : IMovementSpace
{
    private readonly SurfaceManager _surfaceManager;

    public SurfaceMovementSpace(
        SurfaceManager surfaceManager)
    {
        _surfaceManager = surfaceManager;
    }

    public bool TryPickTarget(
        CreatureMovementContext context,
        out MovementDestination destination)
    {
        Surface? surface =
            context.GetCurrentSurface();

        if (surface is null)
        {
            destination =
                new MovementDestination(
                    context.GetX(),
                    context.GetY());

            return false;
        }

        Rectangle walkableBounds =
            _surfaceManager.GetWalkableSpan(
                surface,
                context.GetLandingTolerance());

        int minX =
            walkableBounds.Left;

        int maxX =
            walkableBounds.Right -
            context.GetSpriteWidth();

        if (maxX <= minX)
        {
            destination =
                new MovementDestination(
                    context.GetX(),
                    context.GetY());

            return false;
        }

        double targetX =
            context.NextRandom(
                minX,
                maxX);

        double targetY =
            surface.Top -
            context.GetFootY();

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
            TryResolveReachableDestination(
                context,
                destination,
                out _);
    }

    public bool TryResolveReachableDestination(
        CreatureMovementContext context,
        MovementDestination requested,
        out MovementDestination resolved)
    {
        Surface? currentSurface =
            context.GetCurrentSurface();

        if (currentSurface is null)
        {
            resolved =
                requested;

            return false;
        }

        if (!TryResolveDestination(
                context,
                requested,
                out resolved))
        {
            return false;
        }

        double destinationFeetY =
            resolved.Y +
            context.GetFootY();

        double minimumReachableY =
            currentSurface.Top -
            context.GetLandingTolerance();

        return
            destinationFeetY >=
            minimumReachableY;
    }

    public bool TryResolveDestination(
        CreatureMovementContext context,
        MovementDestination requested,
        out MovementDestination resolved)
    {
        Point? snappedPosition =
            _surfaceManager.SnapToSurface(
                new Point(
                    requested.X,
                    requested.Y),
                context.GetSpriteWidth(),
                context.GetFootY(),
                10);

        if (snappedPosition is null)
        {
            resolved =
                requested;

            return false;
        }

        resolved =
            new MovementDestination(
                snappedPosition.Value.X,
                snappedPosition.Value.Y);

        return true;
    }

    public bool IsPositionValid(
        CreatureMovementContext context,
        double x,
        double y)
    {
        Surface? surface =
            context.GetCurrentSurface();

        if (surface is null)
            return false;

        Rectangle walkableBounds =
            _surfaceManager.GetWalkableSpan(
                surface,
                context.GetLandingTolerance());

        return
            x >= walkableBounds.Left &&
            x <=
                walkableBounds.Right -
                context.GetSpriteWidth();
    }
}