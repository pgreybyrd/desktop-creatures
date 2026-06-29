using Desktop_Creatures.Behaviors;
using Desktop_Creatures.Config;
using Desktop_Creatures.Needs;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World.Surfaces;
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
    Eating,
    Running,
    Falling,
    Chasing,
    Carrying
}
public abstract class Creature
{
    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double SpeedX { get; protected set; }

    protected CreatureSettings Settings { get; }

    public bool SpriteFacesRight => Settings.SpriteFacesRight;
    public int SpriteWidth => Settings.SpriteWidth;
    public int SpriteHeight => Settings.SpriteHeight;
    public double LandingTolerance => Settings.LandingTolerance;

    protected virtual int FootOffsetY => SpriteHeight;

    protected Dictionary<string, BitmapImage[]> Animations { get; } = new();

    public CreatureAction CurrentAction { get; protected set; }

    public Personality Personality { get; } = new();

    public NeedManager Needs { get; } = new();

    public BehaviorController BehaviorController { get; } = new();

    protected BitmapImage[] CurrentFrames = [];
    public BitmapImage? CurrentFrame;// => CurrentFrames.Length > 0 ? CurrentFrames[CurrentFrameIndex] : null;
    protected int CurrentFrameIndex;
    protected int AnimationTick;

    protected Creature(CreatureSettings settings)
    {
        Settings = settings;
    }

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

        if (Settings.Run is not null)
            Animations["Run"] = LoadFrames(assetFolder, "run", Settings.Run.RunFrameCount);

        if (Settings.Idle is not null)
        {
            foreach (var animation in Settings.Idle.Animations)
            {
                Animations[animation.Name] =
                    LoadFrames(
                        assetFolder,
                        animation.Name,
                        animation.FrameCount);
            }
        }

        if (Settings.Swim is not null)
            Animations["Swim"] = LoadFrames(assetFolder, "swim", Settings.Swim.SwimFrameCount);

        if (Settings.Perch is not null)
            Animations["Perch"] = LoadFrames(assetFolder, "perch", Settings.Perch.PerchFrameCount);

        if (Settings.Sleep is not null)
            Animations["Sleep"] = LoadFrames(assetFolder, "sleep", Settings.Sleep.SleepFrameCount);

        if (Settings.Fall is not null)
            Animations["Fall"] = LoadFrames(assetFolder, "fall", Settings.Fall.FallFrameCount);

        if (Settings.Eat is not null)
            Animations["Eat"] = LoadFrames(assetFolder, "eat", Settings.Eat.EatFrameCount);
    }
    protected static BitmapImage LoadImage(string path)
    {
        var image = new BitmapImage();

        image.BeginInit();
        image.UriSource = new Uri($"pack://application:,,,/{path}");
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();

        image.Freeze();

        return image;
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
        Logger.LogDebug(
            DebugCategory.Animation,
            $"SetAction: {CurrentAction} -> {action} ({animationName})");

        if (!Animations.TryGetValue(animationName, out var frames))
        {
            throw new InvalidOperationException(
                $"Animation '{animationName}' was not loaded. " +
                $"Loaded animations: {string.Join(", ", Animations.Keys)}");
        }

        CurrentAction = action;
        CurrentFrames = frames;
        CurrentFrameIndex = 0;
        AnimationTick = 0;
        CurrentFrame = CurrentFrames[CurrentFrameIndex];
    }

    protected void AdvanceAnimation(int frameTicks)
    {
        if (CurrentFrames.Length == 0)
            return;

        if (CurrentFrames.Length == 1)
        {
            CurrentFrame = CurrentFrames[0];
            return;
        }

        AnimationTick++;

        if (AnimationTick >= frameTicks)
        {
            AnimationTick = 0;
            CurrentFrameIndex = (CurrentFrameIndex + 1) % CurrentFrames.Length;
        }

        CurrentFrame = CurrentFrames[CurrentFrameIndex];
    }

    public bool IsStandingOn(Surface surface)
    {
        return
            Math.Abs(
                (Y + Settings.SpriteHeight) - surface.Top)
                < LandingTolerance
            &&
            X >= surface.Left
            &&
            X <= surface.Right;
    }

    public void Update()
    {
        UpdateNeeds();
        UpdateState();
        UpdateAnimation();
    }

    protected virtual void UpdateNeeds()
    {
        Needs.Update();
    }

    protected abstract void UpdateState();

    protected virtual void UpdateAnimation()
    {
    }

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