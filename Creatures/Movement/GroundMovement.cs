using Desktop_Creatures.World.Surfaces;
using Desktop_Creatures.Config;

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
            return;

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

        if (distance < _run.ArrivalDistance)
        {
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

        if (distance <= 0)
            return;

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