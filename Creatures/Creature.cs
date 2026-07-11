using Desktop_Creatures.Behaviors;
using Desktop_Creatures.Config;
using Desktop_Creatures.Needs;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using Desktop_Creatures.Personality;
using System.Diagnostics;
using System.Windows.Documents;
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
    protected string Name = string.Empty;

    protected readonly Random Random = new();
    private readonly PersonalityManager PersonalityManager = new();
    protected PointOfInterestManager PointOfInterestManager;
    protected SurfaceManager SurfaceManager;

    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double SpeedX { get; protected set; }
    protected double FallSpeed = 0;

    protected double TargetX;
    protected double TargetY;

    //protected int EatingTicksRemaining => Settings.Eat.EatingTicksRemaining;
    protected int EatingTicksRemaining;

    protected CreatureSettings Settings { get; }

    public int Scale => Settings.Scale;

    public bool SpriteFacesRight => Settings.SpriteFacesRight;
    public int SpriteWidth => Settings.SpriteWidth * Settings.Scale;
    public int SpriteHeight => Settings.SpriteHeight * Settings.Scale;
    public double LandingTolerance => Settings.LandingTolerance;
    //protected int PostEatMoveDistance => Settings.Eat.LeaveFoodDistance;

    protected virtual int FootOffsetY => SpriteHeight;

    protected PointOfInterest? TargetPoi;
    protected PointOfInterest? EatingPoi;
    protected Surface? CurrentSurface;

    public CreatureAction CurrentAction { get; protected set; }
    protected NeedManager Needs { get; } = new();

    protected BehaviorController BehaviorController { get; } = new();

    protected EatSettings Eat =>
    Settings.Eat
    ?? throw new InvalidOperationException(
        "Creature requires EatSettings.");

    #region Animation
    protected Dictionary<string, BitmapImage[]> Animations { get; } = new();

    public BitmapImage? CurrentFrame => 
        CurrentFrames.Length > 0 
        ? CurrentFrames[CurrentFrameIndex] 
        : null;

    protected BitmapImage[] CurrentFrames = [];
    protected int CurrentFrameIndex;
    protected int AnimationTick;

    protected void AdvanceAnimation(int frameTicks)
    {
        if (CurrentFrames.Length <= 1)
            return;

        //Logger.LogDebug(
        //    DebugCategory.Animation,
        //    $"AdvanceAnimation: Action={CurrentAction}, FrameIndex={CurrentFrameIndex}, FrameTicks={frameTicks}");

        AnimationTick++;

        if (AnimationTick < frameTicks)
            return;

        AnimationTick = 0;
        CurrentFrameIndex = (CurrentFrameIndex + 1) % CurrentFrames.Length;
    }
    #endregion

    #region Fields

    #endregion

    #region Properties

    #endregion

    #region Constructor

    #endregion

    #region Update

    #endregion

    #region Movement

    #endregion

    #region Animation

    #endregion

    #region Needs

    #endregion

    #region Behavior

    #endregion

    #region Helper Methods

    #endregion

    protected Creature(CreatureSettings settings, PointOfInterestManager pointOfInterestManager, SurfaceManager surfaceManager)
    {
        Settings = settings;
        PointOfInterestManager = pointOfInterestManager;
        SurfaceManager = surfaceManager;
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

    protected void StartFalling()
    {
        Logger.LogDebug(
            DebugCategory.Animation,
            $"StartFalling");

        SetAction(CreatureAction.Falling, "Fall");
        SpeedX = 0;
        FallSpeed = 0;
    }

    protected void SetAction(CreatureAction action, string animationName)
    {
        Logger.LogDebug(
            DebugCategory.Animation,
            $"SetAction: {action} ({animationName}) frame size = {CurrentFrame?.PixelWidth}x{CurrentFrame?.PixelHeight}");

        if (!Animations.TryGetValue(animationName, out var frames))
        {
            throw new InvalidOperationException(
                $"Animation '{animationName}' was not loaded. " +
                $"Loaded animations: {string.Join(", ", Animations.Keys)}");
        }
        Logger.LogDebug(
            DebugCategory.Animation,
            $"SetAction: {CurrentAction} -> {action} ({animationName}), Frames={frames.Length}");

        CurrentAction = action;
        CurrentFrames = frames;
        CurrentFrameIndex = 0;

        if (CurrentFrameIndex >= CurrentFrames.Length)
            CurrentFrameIndex = 0;

        AnimationTick = 0;
    }

    protected virtual void UpdateTimers()
    {
    }

    protected static void TickDown(ref int timer)
    {
        if (timer > 0)
            timer--;
    }

    public bool IsStandingOn(Surface surface)
    {
        return
            Math.Abs(
                (Y + Settings.SpriteHeight) 
                - surface.Top)
            < LandingTolerance
            &&
            X >= surface.Left
            &&
            X <= surface.Right;
    }

    protected bool PositionFitsOnSurface(double x, double y, Surface surface)
    {
        return
            x >= surface.Left &&
            x <= surface.Right - Settings.SpriteWidth &&
            Math.Abs(y - (surface.Top - GetCurrentFootY())) <= LandingTolerance;
    }

    protected virtual bool TryPickTargetOnCurrentSurface()
    {
        if (CurrentSurface is null)
            return false;

        int minX = CurrentSurface.Left;
        int maxX = CurrentSurface.Right - Settings.SpriteWidth;

        if (maxX <= minX)
            return false;

        TargetX = Random.Next(minX, maxX);
        TargetY = CurrentSurface.Top - GetCurrentFootY();

        return true;
    }
    protected bool TryPickTargetPoi(PointOfInterestType type)
    {
        return false;
    }
    //OR
    protected bool TryPickPerchTarget()
    {
        return false;
    }

    protected bool PoiIsOnSurface(PointOfInterest poi, Surface surface)
    {
        double poiBottomY = poi.Position.Y + poi.Settings.Height;

        return
            poi.Position.X >= surface.Left &&
            poi.Position.X <= surface.Right &&
            Math.Abs(poiBottomY - surface.Top) <= LandingTolerance;
    }

    protected bool IsStillOnSurface()
    {
        var surface = SurfaceManager.FindSurfaceAtFeet(
            X,
            Y,
            Settings.SpriteWidth,
            GetCurrentFootY(),
            LandingTolerance);

        if (surface is null)
            return false;

        CurrentSurface = surface;
        return true;
    }

    protected virtual int GetCurrentFootY()
    {
        return CurrentAction switch
        {
            CreatureAction.Running => SpriteHeight - (Settings.FootOffsetY * Settings.Scale), //TODO use setting for offset!
            CreatureAction.Idle => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            CreatureAction.Falling => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            _ => SpriteHeight
        };
    }

    public void Update()
    {
        UpdateTimers();
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
        int? frameTicks = CurrentAction switch
        {
            CreatureAction.Running when Settings.Run is not null
                => Settings.Run.RunFrameTicks,

            CreatureAction.Idle when Settings.Idle is not null
                => Settings.Idle.IdleFrameTicks,

            CreatureAction.Falling when Settings.Fall is not null
                => Settings.Fall.FallFrameTicks,

            CreatureAction.Flying when Settings.Flight is not null
                => Settings.Flight.FlyingFrameTicks,

            CreatureAction.Gliding when Settings.Flight is not null
                => Settings.Flight.FlyingFrameTicks,

            CreatureAction.Perching when Settings.Perch is not null
                => Settings.Perch.PerchFrameTicks,

            CreatureAction.Eating when Settings.Eat is not null
                => Settings.Eat.EatFrameTicks,

            CreatureAction.Sleeping when Settings.Sleep is not null
                => Settings.Sleep.SleepFrameTicks,

            _ => null
        };

        if (frameTicks is not null)
            AdvanceAnimation(frameTicks.Value);
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

    protected virtual void FinishEating()
    {
        Needs.Eat();

        EatingPoi = null;
        TargetPoi = null;

        PickPostEatTarget();
    }

    protected virtual void PickPostEatTarget()
    {
        // Default:
        // Move LeaveFoodDistance away.
    }
}