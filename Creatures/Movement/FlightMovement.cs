using Desktop_Creatures.Config;
using Desktop_Creatures.Utilities;
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

    private double _hoverFlipTimeRemaining;

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

    public void Update(
        double deltaSeconds)
    {
        switch (_context.GetAction())
        {
            case CreatureAction.Flying:
            case CreatureAction.Gliding:
                UpdateFlight(deltaSeconds);
                break;

            case CreatureAction.Hovering:
                UpdateHover(deltaSeconds);
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

                if (!CanTraverseTo(
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
                    monitorBounds,
                    spriteWidth,
                    spriteHeight,
                    out targetX,
                    out targetY))
            {
                return;
            }
        }
        else
        {
            if (!TryPickReachableDesktopTarget(
                    monitorBounds,
                    spriteWidth,
                    spriteHeight,
                    out targetX,
                    out targetY))
            {
                return;
            }
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

    private bool TryPickReachableDesktopTarget(
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
                monitorBounds,
                spriteWidth,
                spriteHeight,
                out double candidateX,
                out double candidateY);

            if (!CanTraverseTo(
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
            _context.GetX();

        targetY =
            _context.GetY();

        return false;
    }

    public bool CanReach(
        MovementDestination destination)
    {
        return CanTraverseTo(
            destination.X,
            destination.Y);
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

        _context.SetStateTimeRemaining(
            LegacyTime.ToSeconds(
                _context.NextRandom(
                    _flight.MinFlyTicks,
                    _flight.MaxFlyTicks)));

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

    private void UpdateFlight(
        double deltaSeconds)
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
            _context.GetSettingsScale() *
            _context.GetDisplayScale() *
            deltaSeconds *
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

        double timeRemaining =
            _context.GetStateTimeRemaining() -
            deltaSeconds;

        _context.SetStateTimeRemaining(
            Math.Max(
                0,
                timeRemaining));

        if (timeRemaining <= 0)
        {
            SetFlightModeForTarget();
        }
    }

    private void UpdateHover(
        double deltaSeconds)
    {
        double timeRemaining =
            Math.Max(
                0,
                _context.GetStateTimeRemaining() -
                deltaSeconds);

        _context.SetStateTimeRemaining(
            timeRemaining);

        _context.SetSpeedX(
            0);

        if (_hover is not null)
        {
            _hoverFlipTimeRemaining -=
                deltaSeconds;

            if (_hoverFlipTimeRemaining <= 0)
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

                _hoverFlipTimeRemaining =
                    LegacyTime.ToSeconds(
                        _context.NextRandom(
                            _hover.MinFlipTicks,
                            _hover.MaxFlipTicks));
            }
        }

        if (timeRemaining > 0)
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

        _context.SetStateTimeRemaining(
            LegacyTime.ToSeconds(
                _context.NextRandom(
                    _hover.MinHoverTicks,
                    _hover.MaxHoverTicks)));

        _hoverFlipTimeRemaining =
            LegacyTime.ToSeconds(
                _context.NextRandom(
                    _hover.MinFlipTicks,
                    _hover.MaxFlipTicks));

        _context.SetAction(
            CreatureAction.Hovering,
            "Hover");
    }

    private void SetFlightModeForTarget()
    {
        double dy =
            _context.GetTargetY() -
            _context.GetY();

        if (ShouldGlide(dy))
        {
            _isGliding = true;

            _context.SetMovementSpeed(
                _glide!.GlideSpeed);

            _context.SetStateTimeRemaining(
                LegacyTime.ToSeconds(
                    _context.NextRandom(
                        _glide.MinGlideTicks,
                        _glide.MaxGlideTicks)));

            if (_context.GetAction() !=
                CreatureAction.Gliding)
            {
                _context.SetAction(
                    CreatureAction.Gliding,
                    "Glide");
            }

            return;
        }

        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.FlySpeed);

        _context.SetStateTimeRemaining(
            LegacyTime.ToSeconds(
                _context.NextRandom(
                    _flight.MinFlyTicks,
                    _flight.MaxFlyTicks)));

        if (_context.GetAction() !=
            CreatureAction.Flying)
        {
            _context.SetAction(
                CreatureAction.Flying,
                "Fly");
        }
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

    private bool PositionFitsOnDesktop(
        double x,
        double y)
    {
        double right =
            x +
            _context.GetSpriteWidth();

        double bottom =
            y +
            _context.GetSpriteHeight();

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
        double targetX,
        double targetY)
    {
        double startX =
            _context.GetX();

        double startY =
            _context.GetY();

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
                    x,
                    y))
            {
                return false;
            }
        }

        return true;
    }
}