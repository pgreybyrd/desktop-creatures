using System.Collections.Generic;

namespace Desktop_Creatures.World;

public class WorldState
{
    public List<PointOfInterest> PointsOfInterest { get; } = new();

    public string TimeBucket { get; set; } = "day";
    public string Weather { get; set; } = "clear";
    public double MoonPhase { get; set; } = 0.0;
}