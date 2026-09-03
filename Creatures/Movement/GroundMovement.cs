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
            CreatureAction.Idle or
            CreatureAction.Falling;
    }

    public void Update()
    {
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
}