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
    Eating,
    Running
}
public abstract class Creature
{
    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double SpeedX { get; protected set; }

    protected CreatureSettings Settings { get; }
    public bool SpriteFacesRight => Settings.SpriteFacesRight;

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
        //Animations["Idle"] = LoadFrames(assetFolder, "idle", Settings.Idle.IdleFrameCount);

        if (Settings.Swim is not null)
            Animations["Swim"] = LoadFrames(assetFolder, "swim", Settings.Swim.SwimFrameCount);

        if (Settings.Perch is not null)
            Animations["Perch"] = LoadFrames(assetFolder, "perch", Settings.Perch.PerchFrameCount);

        if (Settings.Sleep is not null)
            Animations["Sleep"] = LoadFrames(assetFolder, "sleep", Settings.Sleep.SleepFrameCount);
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