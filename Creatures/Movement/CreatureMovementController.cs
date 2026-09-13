namespace Desktop_Creatures.Creatures.Movement;

public readonly record struct MovementStep(
    double Distance,
    double RequestedStepDistance,
    double StepDistance,
    double SpeedX,
    double SpeedY,
    double NextX,
    double NextY);

public sealed class CreatureMovementController
{
    private readonly CreatureMovementContext _context;

    public CreatureMovementController(
        CreatureMovementContext context)
    {
        _context = context;
    }

    public MovementStep CalculateStep(
        double speed,
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

        if (distance <= 0)
        {
            return new MovementStep(
                0,
                0,
                0,
                0,
                0,
                _context.GetX(),
                _context.GetY());
        }

        double requestedStepDistance =
            speed *
            _context.GetSettingsScale() *
            _context.GetDisplayScale() *
            deltaSeconds *
            _context.GetFrameMovement();

        double stepDistance =
            Math.Min(
                requestedStepDistance,
                distance);

        double speedX =
            dx / distance *
            stepDistance;

        double speedY =
            dy / distance *
            stepDistance;

        double nextX =
            _context.GetX() +
            speedX;

        double nextY =
            _context.GetY() +
            speedY;

        return new MovementStep(
            distance,
            requestedStepDistance,
            stepDistance,
            speedX,
            speedY,
            nextX,
            nextY);
    }
}