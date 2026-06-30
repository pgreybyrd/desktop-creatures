using System.ComponentModel;

namespace Desktop_Creatures.Config;

public class CreatureSettings
{
    public int SpriteWidth { get; set; } = 32;
    public int SpriteHeight { get; set; } = 32;
    public bool SpriteFacesRight { get; set; } = true;
    public int Scale { get; set; } = 1;
    public double LandingTolerance { get; set; } = 5.0;

    public FlightSettings? Flight { get; set; }
    public WalkSettings? Walk { get; set; }
    public RunSettings? Run { get; set; }
    public IdleSettings? Idle { get; set; }
    public SwimSettings? Swim { get; set; }
    public PerchSettings? Perch { get; set; }
    public SleepSettings? Sleep { get; set; }
    public FallSettings? Fall { get; set; }
    public EatSettings? Eat { get; set; }
}
public class FlightSettings
{
    public double FlySpeed { get; set; } = 2.5;
    public double GlideSpeed { get; set; } = 3.5;
    public double MinDownwardGlideDy { get; set; } = 5;
    public double GlideChance { get; set; } = 0.5;

    public int MinGlideTicks { get; set; } = 200;
    public int MaxGlideTicks { get; set; } = 500;
    public int GlideFrameCount { get; set; } = 1;

    public int MinFlapTicks { get; set; } = 60;
    public int MaxFlapTicks { get; set; } = 180;

    public int MinUpwardFlapTicks { get; set; } = 90;
    public int MaxUpwardFlapTicks { get; set; } = 240;

    public int MinDownwardFlapTicks { get; set; } = 60;
    public int MaxDownwardFlapTicks { get; set; } = 180;

    public int MinTakeoffFlapTicks { get; set; } = 120;
    public int MaxTakeoffFlapTicks { get; set; } = 300;

    public int FlyFrameCount { get; set; } = 4;  
    public int FlyingFrameTicks { get; set; } = 8;
  
    public double ArrivalDistance { get; set; } = 10.0;
}
public class WalkSettings
{
    public double WalkSpeed { get; set; } = 1.0;
    public int MinWalkTicks { get; set; } = 100;
    public int MaxWalkTicks { get; set; } = 300;
    public int WalkFrameCount { get; set; } = 2;
    public int WalkingFrameTicks { get; set; } = 8;
    public int ArrivalDistance { get; set; } = 5;
    public int WalkFrameTicks { get; set; } = 8;
}
public class RunSettings
{
    public double RunSpeed { get; set; } = 3.0;
    public int MinRunTicks { get; set; } = 80;
    public int MaxRunTicks { get; set; } = 200;
    public int RunFrameCount { get; set; } = 4;
    public int RunningFrameTicks { get; set; } = 6; 
    public int ArrivalDistance { get; set; } = 5;
    public int RunFrameTicks { get; set; } = 6;
}
public class IdleSettings
{
    public double IdleChance { get; set; } = 0.5;
    public int MinIdleTicks { get; set; } = 60;
    public int MaxIdleTicks { get; set; } = 180;
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
    public int MinSwimTicks { get; set; } = 100;
    public int MaxSwimTicks { get; set; } = 300;
    public int SwimFrameCount { get; set; } = 4;
}
public class PerchSettings
{
    public double PerchChance { get; set; } = 0.5;
    public int MinPerchTicks { get; set; } = 200;
    public int MaxPerchTicks { get; set; } = 500;
    public int PerchFrameCount { get; set; } = 2;
    public int PerchFrameTicks { get; set; } = 60;
    public double RuffleChance { get; set; } = 0.25;
}
public class SleepSettings
{
    public int MinSleepTicks { get; set; } = 600;
    public int MaxSleepTicks { get; set; } = 1200;
    public int SleepFrameCount { get; set; } = 1;
    public int SleepFrameTicks { get; set; }
}
public class FallSettings
{
    public double Gravity { get; set; } = 0.4;
    public double MaxFallSpeed { get; set; } = 8;
    public int FallFrameCount { get; set; } = 1;
    public int FallFrameTicks { get; set; } = 5;
}
public class EatSettings
{
    public int EatFrameCount { get; set; } = 5;
    public int EatFrameTicks { get; set; } = 5;
    public int EatingTicksRemaining { get; set; } = 50;
}