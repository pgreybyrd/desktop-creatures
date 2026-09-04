using Desktop_Creatures.Config;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class GroundMovement : ICreatureMovement
{
    private readonly CreatureMovementContext _context;
    private readonly SurfaceManager _surfaceManager;
    private readonly RunSettings _run;
    private readonly FallSettings _fall;

    public MovementCapability Capability =>
        MovementCapability.Ground;

    public GroundMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        RunSettings run,
        FallSettings fall)
    {
        _context = context;
        _surfaceManager = surfaceManager;
        _run = run;
        _fall = fall;
    }

    public void Initialize()
    {
        Surface? surface =
            _surfaceManager.FindSurfaceBelow(
                _context.GetX(),
                _context.GetY(),
                _context.GetSpriteWidth(),
                _context.GetFootY());

        _context.SetCurrentSurface(
            surface);

        if (surface is not null)
        {
            _context.SetY(
                surface.Top -
                _context.GetFootY());
        }

        _context.SetAction(
            CreatureAction.Running,
            "Run");
    }

    public bool HandlesAction(
        CreatureAction action)
    {
        return action is
            CreatureAction.Running or
            CreatureAction.Idle or
            CreatureAction.Falling;
    }

    public void Update()
    {
        switch (_context.GetAction())
        {
            case CreatureAction.Running:
                UpdateRunning();
                break;

            case CreatureAction.Idle:
                UpdateIdle();
                break;

            case CreatureAction.Falling:
                UpdateFalling();
                break;
        }
    }

    private void UpdateRunning()
    {
        if (!_context.IsStillOnSurface())
        {
            _context.StartFalling();
            return;
        }

        if (_context.HasInteractionTarget())
        {
            Logger.LogDebug(
                DebugCategory.Movement,
                $"INTERACTION TRAVEL: " +
                $"position=({_context.GetX():F1},{_context.GetY():F1}) " +
                $"target=({_context.GetTargetX():F1},{_context.GetTargetY():F1}) " +
                $"surfaceTop={_context.GetCurrentSurface()?.Top.ToString("F1") ?? "none"}");
        }

        if (!_context.HasInteractionTarget() &&
            !TargetStillOnCurrentSurface())
        {
            PickNewTarget();
            return;
        }

        MoveTowardsTarget();

        if (_context.GetAction() !=
            CreatureAction.Running)
        {
            return;
        }

        if (!_context.HasInteractionTarget() &&
            _context.GetStateTicksRemaining() <= 0)
        {
            _context.OnOrdinaryTargetReached();
        }
    }

    private void UpdateIdle()
    {
        if (!_context.IsStillOnSurface())
        {
            _context.StartFalling();
            return;
        }

        if (_context.GetStateTicksRemaining() <= 0)
        {
            PickNewTarget();
        }
    }

    private void UpdateFalling()
    {
        double previousFeetY =
            _context.GetY() +
            _context.GetFootY();

        double fallSpeed =
            Math.Min(
                _context.GetFallSpeed() +
                _fall.Gravity,
                _fall.MaxFallSpeed);

        _context.SetFallSpeed(
            fallSpeed);

        _context.SetY(
            _context.GetY() +
            fallSpeed);

        double currentFeetY =
            _context.GetY() +
            _context.GetFootY();

        Surface? surface =
            _surfaceManager.Surfaces
                .Where(surface =>
                    _context.GetDisplayCenterX() >= surface.Left &&
                    _context.GetDisplayCenterX() <= surface.Right &&
                    previousFeetY <= surface.Top &&
                    currentFeetY >= surface.Top)
                .OrderBy(surface => surface.Top)
                .FirstOrDefault();

        if (surface is null)
        {
            int? lowestSurfaceTop =
                _surfaceManager.Surfaces.Count > 0
                    ? _surfaceManager.Surfaces.Max(
                        surface => surface.Top)
                    : null;

            if (lowestSurfaceTop is null ||
                currentFeetY >
                lowestSurfaceTop.Value + 100)
            {
                Logger.LogDebug(
                    DebugCategory.Surface,
                    $"FALLING BELOW KNOWN SURFACES: " +
                    $"position=({_context.GetX():F1},{_context.GetY():F1}) " +
                    $"feetY={currentFeetY:F1} " +
                    $"centerX={_context.GetDisplayCenterX():F1} " +
                    $"lowestSurfaceTop={lowestSurfaceTop?.ToString() ?? "none"} " +
                    $"surfaceCount={_surfaceManager.Surfaces.Count}");
            }

            return;
        }

        Logger.LogDebug(
            DebugCategory.Surface,
            $"GROUND LANDING: " +
            $"position=({_context.GetX():F1},{_context.GetY():F1}) " +
            $"surface=[{surface.Left},{surface.Right}] top={surface.Top} " +
            $"centerX={_context.GetDisplayCenterX():F1}");

        _context.SetCurrentSurface(
            surface);

        _context.SetY(
            surface.Top -
            _context.GetFootY());

        _context.SetFallSpeed(
            0);

        _context.OnOrdinaryTargetReached();
    }

    public void Release()
    {
        _surfaceManager.Refresh();

        _context.SetCurrentSurface(
            null);

        _context.StartFalling();
    }

    public void PickNewTarget()
    {
        Surface? surface =
            _context.GetCurrentSurface();

        if (surface is null)
            return;

        int minX =
            surface.Left;

        int maxX =
            surface.Right -
            _context.GetSpriteWidth();

        if (maxX <= minX)
            return;

        double targetX =
            _context.NextRandom(
                minX,
                maxX);

        double targetY =
            surface.Top -
            _context.GetFootY();

        _context.SetTargetX(
            targetX);

        _context.SetTargetY(
            targetY);

        _context.SetMovementSpeed(
            _run.RunSpeed *
            _context.GetDisplayScale());

        _context.SetStateTicksRemaining(
            _context.NextRandom(
                _run.MinRunTicks,
                _run.MaxRunTicks));

        _context.SetAction(
            CreatureAction.Running,
            "Run");
    }

    public bool TrySetDestination(
        MovementDestination destination)
    {
        Surface? currentSurface =
            _context.GetCurrentSurface();

        if (currentSurface is null)
            return false;

        MovementDestination? resolved =
            ResolveDestination(
                destination);

        if (resolved is null)
            return false;

        double destinationFeetY =
            resolved.Y +
            _context.GetFootY();

        if (destinationFeetY <
            currentSurface.Top -
            _context.GetLandingTolerance())
        {
            return false;
        }

        _context.SetTargetX(
            resolved.X);

        _context.SetTargetY(
            resolved.Y);

        _context.SetMovementSpeed(
            _run.RunSpeed *
            _context.GetDisplayScale());

        Logger.LogDebug(
            DebugCategory.Movement,
            $"GROUND DESTINATION SET: " +
            $"from=({_context.GetX():F1},{_context.GetY():F1}) " +
            $"to=({resolved.X:F1},{resolved.Y:F1}) " +
            $"surfaceTop={currentSurface.Top:F1}");

        _context.SetAction(
            CreatureAction.Running,
            "Run");

        return true;
    }

    private MovementDestination? ResolveDestination(
        MovementDestination destination)
    {
        Point? snappedPosition =
            _surfaceManager.SnapToSurface(
                new Point(
                    destination.X,
                    destination.Y),
                _context.GetSpriteWidth(),
                _context.GetFootY(),
                10);

        if (snappedPosition is null)
        {
            Logger.LogDebug(
                DebugCategory.Movement,
                $"GROUND RESOLVE FAILED: " +
                $"raw=({destination.X:F1},{destination.Y:F1})");

            return null;
        }

        Logger.LogDebug(
            DebugCategory.Movement,
            $"GROUND RESOLVED: " +
            $"raw=({destination.X:F1},{destination.Y:F1}) " +
            $"snapped=({snappedPosition.Value.X:F1},{snappedPosition.Value.Y:F1})");

        return new MovementDestination(
            snappedPosition.Value.X,
            snappedPosition.Value.Y);
    }

    public bool CanReach(
        MovementDestination destination)
    {
        Surface? currentSurface =
            _context.GetCurrentSurface();

        if (currentSurface is null)
        {
            Logger.LogDebug(
                DebugCategory.Movement,
                "GROUND CANREACH: false - no current surface");

            return false;
        }

        MovementDestination? resolved =
            ResolveDestination(
                destination);

        if (resolved is null)
        {
            Logger.LogDebug(
                DebugCategory.Movement,
                "GROUND CANREACH: false - destination could not resolve");

            return false;
        }

        double destinationFeetY =
            resolved.Y +
            _context.GetFootY();

        double minimumReachableY =
            currentSurface.Top -
            _context.GetLandingTolerance();

        bool reachable =
            destinationFeetY >=
            minimumReachableY;

        Logger.LogDebug(
            DebugCategory.Movement,
            $"GROUND CANREACH: {reachable} " +
            $"surfaceTop={currentSurface.Top:F1} " +
            $"destinationFeetY={destinationFeetY:F1} " +
            $"minimum={minimumReachableY:F1}");

        return reachable;
    }

    private bool TargetStillOnCurrentSurface()
    {
        Surface? surface =
            _context.GetCurrentSurface();

        if (surface is null)
            return false;

        double targetX =
            _context.GetTargetX();

        return
            targetX >= surface.Left &&
            targetX <=
                surface.Right -
                _context.GetSpriteWidth();
    }

    private void MoveTowardsTarget()
    {
        Surface? surface =
            _context.GetCurrentSurface();

        if (surface is null)
            return;

        double dx =
            _context.GetTargetX() -
            _context.GetX();

        double dy =
            _context.GetTargetY() -
            _context.GetY();

        double distance =
            Math.Sqrt(
                dx * dx +
                dy * dy);

        double currentStep =
            _context.GetMovementSpeed() *
            _context.GetScale() *
            _context.GetFrameMovement();

        double arrivalDistance =
            Math.Max(
                _run.ArrivalDistance,
                currentStep);

        if (distance <= arrivalDistance)
        {
            _context.SetX(
                _context.GetTargetX());

            _context.SetY(
                _context.GetTargetY());

            _context.SetSpeedX(
                0);

            if (_context.HasInteractionTarget())
            {
                _context.OnInteractionTargetReached();
            }
            else
            {
                _context.OnOrdinaryTargetReached();
            }

            return;
        }

        double moveSpeed =
            _context.GetMovementSpeed() *
            _context.GetScale() *
            _context.GetFrameMovement();

        double step =
            Math.Min(
                moveSpeed,
                distance);

        double speedX =
            dx / distance *
            step;

        double speedY =
            dy / distance *
            step;

        _context.SetSpeedX(
            speedX);

        double nextX =
            _context.GetX() +
            speedX;

        double nextY =
            _context.GetY() +
            speedY;

        nextX =
            Math.Clamp(
                nextX,
                surface.Left,
                surface.Right -
                _context.GetSpriteWidth());

        _context.SetX(
            nextX);

        _context.SetY(
            nextY);
    }
}