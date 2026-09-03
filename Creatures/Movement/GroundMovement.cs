using Desktop_Creatures.World.Surfaces;
using Desktop_Creatures.Config;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class GroundMovement : ICreatureMovement
{
    private readonly CreatureMovementContext _context;
    private readonly SurfaceManager _surfaceManager;
    private readonly RunSettings _run;

    public MovementCapability Capability =>
        MovementCapability.Ground;

    public Surface? CurrentSurface { get; private set; }

    public void SetCurrentSurface(
        Surface? surface)
    {
        CurrentSurface = surface;
    }

    public GroundMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        RunSettings run)
    {
        _context = context;
        _surfaceManager = surfaceManager;
        _run = run;
    }

    public void Initialize()
    {
        CurrentSurface =
            _surfaceManager.FindSurfaceBelow(
                _context.GetX(),
                _context.GetY(),
                _context.GetSpriteWidth(),
                _context.GetFootY());

        if (CurrentSurface is not null)
        {
            _context.SetY(
                CurrentSurface.Top -
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
            CreatureAction.Idle;
        //CreatureAction.Running or
        //CreatureAction.Idle or
        //CreatureAction.Falling;
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
        }
    }

    private void UpdateRunning()
    {
        if (!_context.IsStillOnSurface())
        {
            _context.StartFalling();
            return;
        }

        MoveTowardsTarget();
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

    public void Release()
    {
    }

    public void PickNewTarget()
    {
        if (CurrentSurface is null)
            return;

        int minX =
            CurrentSurface.Left;

        int maxX =
            CurrentSurface.Right -
            _context.GetSpriteWidth();

        if (maxX <= minX)
            return;

        double targetX =
            _context.NextRandom(
                minX,
                maxX);

        double targetY =
            CurrentSurface.Top -
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

    private void MoveTowardsTarget()
    {
        if (CurrentSurface is null)
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
                CurrentSurface.Left,
                CurrentSurface.Right -
                _context.GetSpriteWidth());

        _context.SetX(
            nextX);

        _context.SetY(
            nextY);
    }
}