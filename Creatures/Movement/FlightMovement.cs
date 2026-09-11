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
    private readonly PerchSettings? _perch;

    private bool _isGliding;

    private int _hoverFlipTicksRemaining;

    public MovementCapability Capability =>
        MovementCapability.Flight;

    public FlightMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        FlightSettings flight,
        HoverSettings? hover,
        GlideSettings? glide,
        PerchSettings? perch)
    {
        _context = context;
        _surfaceManager = surfaceManager;

        _flight = flight;
        _hover = hover;
        _glide = glide;
        _perch = perch;
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
        if (_perch is not null)
        {
            int roll =
                _context.NextRandom(
                    0,
                    10_000);

            double normalizedRoll =
                roll /
                10_000.0;

            if (normalizedRoll <
                _perch.PerchChance &&
                _context.TrySetPerchTarget())
            {
                return;
            }
        }

        IReadOnlyList<Rectangle> monitorBounds =
            _surfaceManager.GetMonitorBounds();

        if (monitorBounds.Count == 0)
            return;

        int spriteWidth =
            _context.GetSpriteWidth();

        int spriteHeight =
            _context.GetSpriteHeight();

        double targetX;
        double targetY;

        bool useLocalRange =
            _flight.MinTravelDistance is not null &&
            _flight.MaxTravelDistance is not null;

        if (useLocalRange)
        {
            bool foundTarget = false;

            targetX = _context.GetX();
            targetY = _context.GetY();

            const int maxAttempts = 20;

            for (int attempt = 0;
                 attempt < maxAttempts;
                 attempt++)
            {
                double angle =
                    _context.NextRandom(
                        0,
                        360) *
                    Math.PI /
                    180.0;

                int distance =
                    _context.NextRandom(
                        _flight.MinTravelDistance!.Value,
                        _flight.MaxTravelDistance!.Value);

                double candidateX =
                    _context.GetX() +
                    Math.Cos(angle) *
                    distance;

                double candidateY =
                    _context.GetY() +
                    Math.Sin(angle) *
                    distance;

                Point topLeft =
                    new(
                        candidateX,
                        candidateY);

                Point bottomRight =
                    new(
                        candidateX +
                        spriteWidth,
                        candidateY +
                        spriteHeight);

                if (!_surfaceManager.IsPointOnDesktop(
                        topLeft) ||
                    !_surfaceManager.IsPointOnDesktop(
                        bottomRight))
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

            if (!foundTarget)
            {
                PickRandomDesktopTarget(
                    monitorBounds,
                    spriteWidth,
                    spriteHeight,
                    out targetX,
                    out targetY);
            }
        }
        else
        {
            PickRandomDesktopTarget(
                monitorBounds,
                spriteWidth,
                spriteHeight,
                out targetX,
                out targetY);
        }

        _context.SetTargetX(
            targetX);

        _context.SetTargetY(
            targetY);

        SetFlightModeForTarget();
    }

    private void PickRandomDesktopTarget(
        IReadOnlyList<Rectangle> monitorBounds,
        int spriteWidth,
        int spriteHeight,
        out double targetX,
        out double targetY)
    {
        Rectangle area =
            monitorBounds[
                _context.NextRandom(
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
            _context.NextRandom(
                minX,
                maxX);

        targetY =
            _context.NextRandom(
                minY,
                maxY);
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

        if (_hover is not null)
        {
            _hoverFlipTicksRemaining--;

            if (_hoverFlipTicksRemaining <= 0)
            {
                int roll =
                    _context.NextRandom(
                        0,
                        10_000);

                double normalizedRoll =
                    roll /
                    10_000.0;

                if (normalizedRoll <
                    _hover.FlipChance)
                {
                    _context.FlipFacing();
                }

                _hoverFlipTicksRemaining =
                    _context.NextRandom(
                        _hover.MinFlipTicks,
                        _hover.MaxFlipTicks);
            }
        }

        if (ticksRemaining > 0)
            return;

        PickNewTarget();
    }

    private void OnFlightTargetReached()
    {
        _context.SetSpeedX(
            0);

        if (_context.HasInteractionTarget())
        {
            _context.OnInteractionTargetReached();
            return;
        }

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

        _hoverFlipTicksRemaining =
            _context.NextRandom(
                _hover.MinFlipTicks,
                _hover.MaxFlipTicks);

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