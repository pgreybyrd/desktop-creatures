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

    private readonly FlightDefinition _flight;
    private readonly HoverDefinition? _hover;
    private readonly GlideDefinition? _glide;
    private readonly PerchDefinition? _perch;

    private bool _isGliding;

    private double _hoverFlipTimeRemaining;

    public FlightMovement(
        CreatureMovementContext context,
        SurfaceManager surfaceManager,
        FlightDefinition flight,
        PerchDefinition? perch)
    {
        _context = context;

        _movementController =
            new CreatureMovementController(
                context);

        _surfaceManager =
            surfaceManager;

        _flight =
            flight;

        _hover =
            flight.Hover;

        _glide =
            flight.Glide;

        _perch =
            perch;

        _movementSpace =
            new AirMovementSpace(
                surfaceManager,
                flight);
    }

    public void Initialize()
    {
        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.Speed);

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
                _perch.Chance &&
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
            _context.NextRandomDouble(
                _flight.MinDurationSeconds,
                _flight.MaxDurationSeconds));

        SetFlightModeForTarget();

        return true;
    }

    public void Release()
    {
        _isGliding = false;

        _context.SetMovementSpeed(
            _flight.Speed);

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
                ? _glide.Speed
                : _flight.Speed;

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
                    GetNextHoverFlipSeconds();
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
            _hover.Chance;
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
            _context.NextRandomDouble(
                _hover.MinDurationSeconds,
                _hover.MaxDurationSeconds));

        _hoverFlipTimeRemaining =
            GetNextHoverFlipSeconds();

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
                _glide!.Speed);

            _context.SetStateTimeRemaining(
                _context.NextRandomDouble(
                    _glide.MinDurationSeconds,
                    _glide.MaxDurationSeconds));

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
            _flight.Speed);

        _context.SetStateTimeRemaining(
            _context.NextRandomDouble(
                _flight.MinDurationSeconds,
                _flight.MaxDurationSeconds));

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
            _glide.Chance;
    }

    private double GetNextHoverFlipSeconds()
    {
        if (_hover?.MinFlipSeconds is not double min ||
            _hover.MaxFlipSeconds is not double max)
        {
            return double.PositiveInfinity;
        }

        return
            _context.NextRandomDouble(
                min,
                max);
    }
}