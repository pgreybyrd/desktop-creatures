using Desktop_Creatures.Config;
using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class FlightMovement : ICreatureMovement
{
    private readonly CreatureMovementContext _context;
    private readonly SurfaceManager _surfaceManager;

    private readonly FlightSettings _flight;
    private readonly HoverSettings? _hover;
    private readonly GlideSettings? _glide;

    private bool _isGliding;

    public MovementCapability Capability =>
        MovementCapability.Flight;

    public FlightMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        FlightSettings flight,
        HoverSettings? hover,
        GlideSettings? glide)
    {
        _context = context;
        _surfaceManager = surfaceManager;

        _flight = flight;
        _hover = hover;
        _glide = glide;
    }

    public void Initialize()
    {
        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.FlySpeed);

        _context.SetAction(
            CreatureAction.Flying,
            "Fly");
    }

    public bool HandlesAction(
        CreatureAction action)
    {
        return action is
            CreatureAction.Flying or
            CreatureAction.Gliding or
            CreatureAction.Hovering;
    }

    public void Update()
    {
        switch (_context.GetAction())
        {
            case CreatureAction.Flying:
            case CreatureAction.Gliding:
                UpdateFlight();
                break;

            case CreatureAction.Hovering:
                UpdateHover();
                break;
        }
    }

    public void PickNewTarget()
    {
        IReadOnlyList<Rectangle> monitorBounds =
            _surfaceManager.GetMonitorBounds();

        if (monitorBounds.Count == 0)
            return;

        Rectangle area =
            monitorBounds[
                _context.NextRandom(
                    0,
                    monitorBounds.Count)];

        int spriteWidth =
            _context.GetSpriteWidth();

        int spriteHeight =
            _context.GetSpriteHeight();

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

        if (maxX <= minX ||
            maxY <= minY)
        {
            return;
        }

        double targetX =
            _context.NextRandom(
                minX,
                maxX);

        double targetY =
            _context.NextRandom(
                minY,
                maxY);

        _context.SetTargetX(
            targetX);

        _context.SetTargetY(
            targetY);

        _context.SetStateTicksRemaining(
            _context.NextRandom(
                _flight.MinFlyTicks,
                _flight.MaxFlyTicks));

        SetFlightModeForTarget();
    }

    public bool CanReach(
        MovementDestination destination)
    {
        Point destinationPoint =
            new(
                destination.X,
                destination.Y);

        return _surfaceManager
            .IsPointOnDesktop(
                destinationPoint);
    }

    public bool TrySetDestination(
        MovementDestination destination)
    {
        if (!CanReach(destination))
            return false;

        _context.SetTargetX(
            destination.X);

        _context.SetTargetY(
            destination.Y);

        _context.SetStateTicksRemaining(
            _context.NextRandom(
                _flight.MinFlyTicks,
                _flight.MaxFlyTicks));

        SetFlightModeForTarget();

        return true;
    }

    public void Release()
    {
        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.FlySpeed);

        _context.SetAction(
            CreatureAction.Flying,
            "Fly");

        PickNewTarget();
    }

    private void UpdateFlight()
    {
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

        if (distance <=
            _flight.ArrivalDistance)
        {
            OnFlightTargetReached();
            return;
        }

        double speed =
            _isGliding &&
            _glide is not null
                ? _glide.GlideSpeed
                : _flight.FlySpeed;

        double step =
            speed *
            _context.GetScale() *
            _context.GetFrameMovement();

        step =
            Math.Min(
                step,
                distance);

        double speedX =
            dx / distance *
            step;

        double speedY =
            dy / distance *
            step;

        double nextX =
            _context.GetX() +
            speedX;

        double nextY =
            _context.GetY() +
            speedY;

        Point nextPosition =
            new(
                nextX,
                nextY);

        Point nextBottomRight =
            new(
                nextX +
                _context.GetSpriteWidth(),
                nextY +
                _context.GetSpriteHeight());

        if (!_surfaceManager.IsPointOnDesktop(
                nextPosition) ||
            !_surfaceManager.IsPointOnDesktop(
                nextBottomRight))
        {
            PickNewTarget();
            return;
        }

        _context.SetSpeedX(
            speedX);

        _context.SetX(
            nextX);

        _context.SetY(
            nextY);

        int ticksRemaining =
            _context.GetStateTicksRemaining() -
            1;

        _context.SetStateTicksRemaining(
            ticksRemaining);

        if (ticksRemaining <= 0)
        {
            SetFlightModeForTarget();
        }
    }

    private void UpdateHover()
    {
        int ticksRemaining =
            _context.GetStateTicksRemaining() -
            1;

        _context.SetStateTicksRemaining(
            ticksRemaining);

        _context.SetSpeedX(
            0);

        if (ticksRemaining > 0)
            return;

        PickNewTarget();
    }

    private void OnFlightTargetReached()
    {
        _context.SetSpeedX(
            0);

        if (ShouldHover())
        {
            StartHovering();
            return;
        }

        PickNewTarget();
    }

    private bool ShouldHover()
    {
        if (_hover is null)
            return false;

        int roll =
            _context.NextRandom(
                0,
                10_000);

        double normalizedRoll =
            roll / 10_000.0;

        return normalizedRoll <
            _hover.HoverChance;
    }

    private void StartHovering()
    {
        if (_hover is null)
        {
            PickNewTarget();
            return;
        }

        _isGliding = false;

        _context.SetStateTicksRemaining(
            _context.NextRandom(
                _hover.MinHoverTicks,
                _hover.MaxHoverTicks));

        _context.SetAction(
            CreatureAction.Hovering,
            "Hover");
    }

    private void SetFlightModeForTarget()
    {
        double dy =
            _context.GetTargetY() -
            _context.GetY();

        if (ShouldGlide(
                dy))
        {
            _isGliding = true;

            _context.SetMovementSpeed(
                _glide!.GlideSpeed);

            _context.SetStateTicksRemaining(
                _context.NextRandom(
                    _glide.MinGlideTicks,
                    _glide.MaxGlideTicks));

            _context.SetAction(
                CreatureAction.Gliding,
                "Glide");

            return;
        }

        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.FlySpeed);

        _context.SetStateTicksRemaining(
            _context.NextRandom(
                _flight.MinFlyTicks,
                _flight.MaxFlyTicks));

        _context.SetAction(
            CreatureAction.Flying,
            "Fly");
    }

    private bool ShouldGlide(
        double dy)
    {
        if (_glide is null)
            return false;

        if (dy <
            _glide.MinDownwardGlideDy)
        {
            return false;
        }

        int roll =
            _context.NextRandom(
                0,
                10_000);

        double normalizedRoll =
            roll / 10_000.0;

        return normalizedRoll <
            _glide.GlideChance;
    }
}