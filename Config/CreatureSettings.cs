using Desktop_Creatures.World;

namespace Desktop_Creatures.Config;

public class CreatureSettings
{
    public int SpriteWidth { get; set; } = 32;
    public int SpriteHeight { get; set; } = 32;
    public bool SpriteFacesRight { get; set; } = true;
    public int Scale { get; set; } = 1;
    public int FootOffsetY { get; set; } = 0;
    public double LandingTolerance { get; set; } = 5.0;

    public Dictionary<string, double> FrameMovement { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public FlightSettings? Flight { get; set; }
    public HoverSettings? Hover { get; set; }
    public GlideSettings? Glide { get; set; }

    public WalkSettings? Walk { get; set; }
    public RunSettings? Run { get; set; }
    public IdleSettings? Idle { get; set; }
    public SwimSettings? Swim { get; set; }

    public PerchSettings? Perch { get; set; }
    public NestingSettings? Nesting { get; set; }

    public SleepSettings? Sleep { get; set; }
    public FallSettings? Fall { get; set; }
    public EatSettings? Eat { get; set; }
}

public class FlightSettings
{
    public double FlySpeed { get; set; } = 2.5;

    public int MinFlySeconds { get; set; } = 1;
    public int MaxFlySeconds { get; set; } = 3;

    public int ArrivalDistance { get; set; } = 10;

    public int? MinTravelDistance { get; set; }
    public int? MaxTravelDistance { get; set; }
}

public class HoverSettings
{
    public double HoverChance { get; set; } = 0.5;

    public int MinHoverSeconds { get; set; } = 1;
    public int MaxHoverSeconds { get; set; } = 2;

    public double FlipChance { get; set; } = 0.25;
    public int MinFlipSeconds { get; set; } = 1;
    public int MaxFlipSeconds { get; set; } = 1;
}

public class GlideSettings
{
    public double GlideSpeed { get; set; } = 3.0;
    public double GlideChance { get; set; } = 0.5;
    public double MinDownwardGlideDy { get; set; } = 5.0;

    public int MinGlideSeconds { get; set; } = 3;
    public int MaxGlideSeconds { get; set; } = 8;
}

public class NestingSettings
{
    public int MinNestingSeconds { get; set; } = 5;
    public int MaxNestingSeconds { get; set; } = 15;
}

public class WalkSettings
{
    public double WalkSpeed { get; set; } = 1.0;

    public int MinWalkSeconds { get; set; } = 1;
    public int MaxWalkSeconds { get; set; } = 5;

    // Temporary animation metadata.
    // Aseprite will eventually own this entirely.
    public int WalkFrameCount { get; set; } = 2;
    public int WalkingFrameTicks { get; set; } = 8;
    public int WalkFrameTicks { get; set; } = 8;

    public int ArrivalDistance { get; set; } = 5;
}

public class RunSettings
{
    public double RunSpeed { get; set; } = 3.0;

    public int MinRunSeconds { get; set; } = 1;
    public int MaxRunSeconds { get; set; } = 3;

    // Temporary animation metadata.
    // Aseprite will eventually own this entirely.
    public int RunFrameCount { get; set; } = 4;
    public int RunningFrameTicks { get; set; } = 6;
    public int RunFrameTicks { get; set; } = 6;

    public int ArrivalDistance { get; set; } = 5;

    public Dictionary<string, double> FrameMovement { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public class IdleSettings
{
    public double IdleChance { get; set; } = 0.5;

    public int MinIdleSeconds { get; set; } = 1;
    public int MaxIdleSeconds { get; set; } = 3;

    // Temporary animation metadata.
    public int IdleFrameCount { get; set; } = 1;
    public int IdleFrameTicks { get; set; } = 60;

    public List<AnimationDefinition> Animations { get; set; } = new();
}

public class AnimationDefinition
{
    public string Name { get; set; } = "";
    public int FrameCount { get; set; } = 1;
}

public class SwimSettings
{
    public double SwimSpeed { get; set; } = 1.5;

    public int MinSwimSeconds { get; set; } = 1;
    public int MaxSwimSeconds { get; set; } = 5;

    // Temporary animation metadata.
    public int SwimFrameCount { get; set; } = 4;
}

public class PerchSettings
{
    public double PerchChance { get; set; } = 0.5;

    public int MinPerchSeconds { get; set; } = 3;
    public int MaxPerchSeconds { get; set; } = 8;

    // Temporary animation metadata.
    public int PerchFrameCount { get; set; } = 2;
    public int PerchFrameTicks { get; set; } = 60;

    public double RuffleChance { get; set; } = 0.25;
}

public class SleepSettings
{
    public int MinSleepSeconds { get; set; } = 10;
    public int MaxSleepSeconds { get; set; } = 20;

    // Temporary animation metadata.
    public int SleepFrameCount { get; set; } = 1;
    public int SleepFrameTicks { get; set; }
}

public class FallSettings
{
    public double Gravity { get; set; } = 1800.0;
    public double MaxFallSpeed { get; set; } = 3000.0;

    // Temporary animation metadata.
    public int FallFrameCount { get; set; } = 1;
    public int FallFrameTicks { get; set; } = 5;
}

public class EatSettings
{
    public int EatDurationSeconds { get; set; } = 1;
    public int FoodSearchCooldownSeconds { get; set; } = 2;

    public int LeaveFoodDistance { get; set; } = 50;
    public double InteractionReach { get; set; } = 20.0;
    public WorldInteractionPointType InteractionType { get; set; } =
        WorldInteractionPointType.Eat;

    // Temporary animation metadata.
    public int EatFrameCount { get; set; } = 5;
    public int EatFrameTicks { get; set; } = 5;
}