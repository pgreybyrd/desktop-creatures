using Desktop_Creatures.Config;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public enum CreatureAction
{
    Idle,
    Flying,
    Gliding,
    Walking,
    Swimming,
    Perching,
    Sleeping,
    Eating
}
public abstract class Creature
{
    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double SpeedX { get; protected set; }

    protected CreatureSettings Settings { get; }
    protected Dictionary<string, BitmapImage[]> Animations { get; } = new();

    public CreatureAction CurrentAction { get; protected set; }

    protected BitmapImage[] CurrentFrames = [];
    public BitmapImage? CurrentFrame;// => CurrentFrames.Length > 0 ? CurrentFrames[CurrentFrameIndex] : null;
    protected int CurrentFrameIndex;
    protected int AnimationTick;

    protected Creature(CreatureSettings settings)
    {
        Settings = settings;
    }

    protected bool CanFly => Settings.Flight is not null;
    protected bool CanWalk => Settings.Walk is not null;
    protected bool CanSwim => Settings.Swim is not null;
    protected bool CanPerch => Settings.Perch is not null;
    protected bool CanSleep => Settings.Sleep is not null;

    public void LoadAssets(string assetFolder)
    {
        if (Settings.Flight is not null)
        {
            Animations["Fly"] = LoadFrames(assetFolder, "fly", Settings.Flight.FlyFrameCount);

            if (Settings.Flight.GlideFrameCount > 0)
                Animations["Glide"] = LoadFrames(assetFolder, "glide", Settings.Flight.GlideFrameCount);
        }

        if (Settings.Walk is not null)
            Animations["Walk"] = LoadFrames(assetFolder, "walk", Settings.Walk.WalkFrameCount);

        if (Settings.Swim is not null)
            Animations["Swim"] = LoadFrames(assetFolder, "swim", Settings.Swim.SwimFrameCount);

        if (Settings.Perch is not null)
            Animations["Perch"] = LoadFrames(assetFolder, "perch", Settings.Perch.PerchFrameCount);

        if (Settings.Sleep is not null)
            Animations["Sleep"] = LoadFrames(assetFolder, "sleep", Settings.Sleep.SleepFrameCount);
    }
    protected static BitmapImage LoadImage(string path)
    {
        return new BitmapImage(
            new Uri($"pack://application:,,,/{path}")
        );
    }

    protected static BitmapImage[] LoadFrames(
        string assetFolder,
        string animationName,
        int frameCount)
    {
        return Enumerable.Range(0, frameCount)
            .Select(i => LoadImage($"{assetFolder}/{animationName}_{i}.png"))
            .ToArray();
    }

    protected void SetAction(CreatureAction action, string animationName)
    {
        CurrentAction = action;
        CurrentFrames = Animations[animationName];
        CurrentFrameIndex = 0;
        AnimationTick = 0;
    }

    protected void AdvanceAnimation(int frameTicks)
    {
        if (CurrentFrames.Length <= 1) { 
            Debug.WriteLine($"Animation '{CurrentAction}' has only one frame, skipping animation advance.");
            return;
        }
        AnimationTick++;
        CurrentFrame = CurrentFrames[CurrentFrameIndex];
        Debug.WriteLine($"Advancing animation '{CurrentAction}': " +
            $"Tick {AnimationTick}/{frameTicks}, " +
            $"Frame {CurrentFrameIndex}/{CurrentFrames.Length}");

        if (AnimationTick < frameTicks)
            return;

        AnimationTick = 0;
        CurrentFrameIndex = (CurrentFrameIndex + 1) % CurrentFrames.Length;
    }

    public abstract void Update();

    public virtual void DragTo(double x, double y)
    {
        X = x;
        Y = y;
    }

    public virtual void Release()
    {
    }

    public virtual void PickNewTarget()
    {
    }
}