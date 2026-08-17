using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;

namespace Desktop_Creatures.Creatures;

public sealed record CreatureServices
{
    public required PointOfInterestManager PointOfInterestManager { get; init; }

    public required SurfaceManager SurfaceManager { get; init; }

    public required List<PointOfInterest> PointsOfInterest { get; init; }

    public required IReadOnlyList<Rectangle> MonitorWorkingAreas { get; init; }
}