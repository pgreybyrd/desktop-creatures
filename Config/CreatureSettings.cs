namespace Desktop_Creatures.Config;

public class CreatureSettings
{
    public double FlySpeed { get; set; } = 2.5;
    public double GlideSpeed { get; set; } = 3.5;
    public double PerchChance { get; set; } = 0.35;
    public int MinPerchTicks { get; set; } = 300;
    public int MaxPerchTicks { get; set; } = 720;
    public double ArrivalDistance { get; set; } = 10;
    public double MinDownwardGlideDy { get; set; } = 5;
    public double GlideChance { get; set; } = 0.6;

    public int MinGlideTicks { get; set; } = 120;
    public int MaxGlideTicks { get; set; } = 300;

    public int MinFlapTicks { get; set; } = 60;
    public int MaxFlapTicks { get; set; } = 180;

    public int MinUpwardFlapTicks { get; set; } = 90;
    public int MaxUpwardFlapTicks { get; set; } = 240;

    public int MinTakeoffFlapTicks { get; set; } = 60;
    public int MaxTakeoffFlapTicks { get; set; } = 140;

    public int FlyingFrameTicks { get; set; } = 8;
    public int PerchFrameTicks { get; set; } = 60;
    public double RuffleChance { get; set; } = 0.25;

    public int SpriteWidth { get; set; } = 32;
    public int SpriteHeight { get; set; } = 32;
}