using Desktop_Creatures.Config;
using Desktop_Creatures.World.Surfaces;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures.Movement;

public sealed class FlightMovement : ICreatureMovement
{
    private readonly CreatureMovementContext _context;
    private readonly CreatureMovementController
        _movementController;
    private readonly AirMovementSpace
        _movementSpace;
    private readonly SurfaceManager _surfaceManager;

    private readonly FlightSettings _flight;
    private readonly HoverSettings? _hover;
    private readonly GlideSettings? _glide;
    private readonly PerchSettings? _perch;

    private bool _isGliding;

    private double _hoverFlipTimeRemaining;

    public FlightMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        FlightSettings flight,
        HoverSettings? hover,
        GlideSettings? glide,
        PerchSettings? perch)
    {
        _context = context;

        _movementController =
            new CreatureMovementController(
                context);

        _surfaceManager = surfaceManager;

        _flight = flight;
        _hover = hover;
        _glide = glide;
        _perch = perch;

        _movementSpace =
            new AirMovementSpace(
                surfaceManager,
                flight);
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

        if (!_movementSpace.TryPickTarget(
                _context,
                out MovementDestination destination))
        {
            return;
        }

        _context.SetTargetX(
            destination.X);

        _context.SetTargetY(
            destination.Y);

        SetFlightModeForTarget();
    }

    public bool CanReach(
        MovementDestination destination)
    {
        return
            _movementSpace.CanReach(
                _context,
                destination);
    }

    public bool TrySetDestination(
        MovementDestination destination)
    {
        if (!_movementSpace.CanReach(
                _context,
                destination))
        {
            return false;
        }

        _context.SetTargetX(
            destination.X);

        _context.SetTargetY(
            destination.Y);

        _context.SetStateTimeRemaining(
            _context.NextRandom(
                _flight.MinFlySeconds,
                _flight.MaxFlySeconds + 1));

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
        double speed =
            _isGliding &&
            _glide is not null
                ? _glide.GlideSpeed
                : _flight.FlySpeed;

        MovementStep movementStep =
            _movementController.CalculateStep(
                speed,
                deltaSeconds);

        if (movementStep.Distance <=
            _flight.ArrivalDistance)
        {
            OnFlightTargetReached();
            return;
        }

        double nextX =
            movementStep.NextX;

        double nextY =
            movementStep.NextY;

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
            movementStep.SpeedX);

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
                    _context.NextRandom(
                        _hover.MinFlipSeconds,
                        _hover.MaxFlipSeconds + 1);
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
            _context.NextRandom(
                _hover.MinHoverSeconds,
                _hover.MaxHoverSeconds + 1));

        _hoverFlipTimeRemaining =
            _context.NextRandom(
                _hover.MinFlipSeconds,
                _hover.MaxFlipSeconds + 1);

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
                _context.NextRandom(
                    _glide.MinGlideSeconds,
                    _glide.MaxGlideSeconds + 1));

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
            _context.NextRandom(
                _flight.MinFlySeconds,
                _flight.MaxFlySeconds + 1));

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
}