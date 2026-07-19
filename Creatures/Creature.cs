using Desktop_Creatures.Behaviors;
using Desktop_Creatures.Config;
using Desktop_Creatures.Needs;
using Desktop_Creatures.Personality;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

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
    protected double MovementSpeed;

    protected int StateTicksRemaining;
    protected int EatingTicksRemaining;

    protected CreatureSettings Settings { get; }

    public int Scale => Settings.Scale;

    public bool SpriteFacesRight => Settings.SpriteFacesRight;
    public int SpriteWidth => Settings.SpriteWidth * Settings.Scale;
    public int SpriteHeight => Settings.SpriteHeight * Settings.Scale;
    public double LandingTolerance => Settings.LandingTolerance;

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

    protected WalkSettings Walk =>
        Settings.Walk
        ?? throw new InvalidOperationException(
            "Creature requires WalkSettings.");

    protected IdleSettings Idle =>
        Settings.Idle
        ?? throw new InvalidOperationException(
            "Creature requires IdleSettings.");

    protected RunSettings Run =>
        Settings.Run
        ?? throw new InvalidOperationException(
            "Creature requires RunSettings.");

    protected FallSettings Fall =>
        Settings.Fall
        ?? throw new InvalidOperationException(
            "Creature requires FallSettings.");

    protected Dictionary<string, BitmapImage[]> Animations { get; } = new();

    public BitmapImage? CurrentFrame =>
        CurrentFrames.Length > 0
            ? CurrentFrames[CurrentFrameIndex]
            : null;

    protected BitmapImage[] CurrentFrames = [];
    protected int CurrentFrameIndex;
    protected int AnimationTick;

    protected Creature(
        CreatureSettings settings,
        PointOfInterestManager pointOfInterestManager,
        SurfaceManager surfaceManager)
    {
        Settings = settings;
        PointOfInterestManager = pointOfInterestManager;
        SurfaceManager = surfaceManager;

        ConfigureDefaultBehaviors();
    }

    private void ConfigureDefaultBehaviors()
    {
        if (Settings.Eat is null)
            return;

        BehaviorController.AddBehavior(new EatBehavior(
            Needs,
            Eat,
            PointOfInterestManager,
            () => new Point(X, Y),
            CanSearchForFood,
            TrySetFoodTarget));
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

    protected void InitializeGroundCreature(double startX, double startY)
    {
        X = startX;
        Y = startY;

        CurrentSurface = SurfaceManager.FindSurfaceBelow(
            X,
            Y,
            Settings.SpriteWidth,
            Settings.SpriteHeight);

        if (CurrentSurface is not null)
            Y = CurrentSurface.Top - GetCurrentFootY();

        SetAction(CreatureAction.Running, "Run");
        PickNewTarget();
    }

    protected void AdvanceAnimation(int frameTicks)
    {
        if (CurrentFrames.Length <= 1)
            return;

        AnimationTick++;

        if (AnimationTick < frameTicks)
            return;

        AnimationTick = 0;
        CurrentFrameIndex = (CurrentFrameIndex + 1) % CurrentFrames.Length;
    }

    protected void StartFalling()
    {
        Logger.LogDebug(
            DebugCategory.Animation,
            "StartFalling");

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
        if (CurrentAction == CreatureAction.Eating)
            TickDown(ref EatingTicksRemaining);

        if (CurrentAction is CreatureAction.Running or CreatureAction.Idle)
            TickDown(ref StateTicksRemaining);
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

    protected virtual bool TryPickPerchTarget()
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
            SpriteWidth,
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
            CreatureAction.Running => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            CreatureAction.Idle => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            CreatureAction.Eating => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            CreatureAction.Falling => SpriteHeight - (Settings.FootOffsetY * Settings.Scale),
            _ => SpriteHeight
        };
    }

    public void Update()
    {
        UpdateTimers();
        UpdateNeeds();
        UpdateBehavior();
        UpdateState();
        UpdateAnimation();
    }

    protected virtual void UpdateNeeds()
    {
        Needs.Update();
    }

    protected virtual void UpdateBehavior()
    {
        BehaviorController.Update();
    }

    protected virtual void UpdateState()
    {
        switch (CurrentAction)
        {
            case CreatureAction.Running:
                UpdateRunning();
                break;
            case CreatureAction.Idle:
                UpdateIdle();
                break;
            case CreatureAction.Falling:
                UpdateFalling();
                break;
            case CreatureAction.Eating:
                UpdateEating();
                break;
        }
    }

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

    protected virtual bool CanSearchForFood()
    {
        return Settings.Run is not null &&
               CurrentSurface is not null &&
               TargetPoi is null &&
               CurrentAction is CreatureAction.Running or CreatureAction.Idle;
    }

    protected virtual bool TrySetFoodTarget(WorldInteractionTarget target)
    {
        if (!CanSearchForFood())
            return false;

        Point? snappedPosition = SurfaceManager.SnapToSurface(
            target.Position,
            SpriteWidth,
            GetCurrentFootY());

        if (snappedPosition is null)
            return false;

        TargetPoi = target.PointOfInterest;
        TargetX = snappedPosition.Value.X;
        TargetY = snappedPosition.Value.Y;
        MovementSpeed = Run.RunSpeed;

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"Trying food target: " +
            $"interaction=({target.Position.X:F1}, {target.Position.Y:F1}), " +
            $"snapped={snappedPosition}");

        SetAction(CreatureAction.Running, "Run");

        return true;
    }

    protected virtual void StartEating(PointOfInterest poi)
    {
        int eatFrameCount = Animations.TryGetValue("Eat", out var eatFrames)
            ? eatFrames.Length
            : 0;

        Logger.LogDebug(
            DebugCategory.Animation,
            "StartEating()" +
            $"Animation keys: {string.Join(", ", Animations.Keys)}\n" +
            $"Eat frame count: {eatFrameCount},\n" +
            $"Eat frame ticks: {Eat.EatFrameTicks}");

        EatingPoi = poi;
        EatingTicksRemaining = Eat.EatingTicksRemaining;

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"EatingTicksRemaining loaded as: {Eat.EatingTicksRemaining}");

        SpeedX = 0;
        StateTicksRemaining = 0;

        SetAction(CreatureAction.Eating, "Eat");
    }

    protected virtual void UpdateEating()
    {
        Logger.LogDebug(
            DebugCategory.Behavior,
            $"UpdateEating decrement timer to {EatingTicksRemaining}, FrameIndex={CurrentFrameIndex}");

        if (EatingTicksRemaining <= 0)
            FinishEating();
    }

    protected virtual void UpdateRunning()
    {
        if (!ValidateSurface())
            return;

        MoveTowardsTarget();

        if (CurrentAction != CreatureAction.Running)
            return;

        if (StateTicksRemaining <= 0)
            StartIdle();
    }

    protected virtual void StartIdle()
    {
        var animation = Idle.Animations[
            Random.Next(Idle.Animations.Count)
        ];

        SetAction(
            CreatureAction.Idle,
            animation.Name);

        Logger.LogDebug(
            DebugCategory.Animation,
            $"StartIdle selected animation={animation.Name}");

        SpeedX = 0;

        StateTicksRemaining = Random.Next(
            Idle.MinIdleTicks,
            Idle.MaxIdleTicks);
    }

    protected virtual void UpdateIdle()
    {
        if (!IsStillOnSurface())
        {
            StartFalling();
            return;
        }

        if (StateTicksRemaining <= 0)
            PickNewTarget();
    }

    protected virtual void UpdateFalling()
    {
        double previousFeetY = Y + GetCurrentFootY();

        FallSpeed = Math.Min(
            FallSpeed + Fall.Gravity,
            Fall.MaxFallSpeed);

        Y += FallSpeed;

        double currentFeetY = Y + GetCurrentFootY();

        var surface = SurfaceManager.Surfaces
            .Where(s =>
                X + Settings.SpriteWidth / 2.0 >= s.Left &&
                X + Settings.SpriteWidth / 2.0 <= s.Right &&
                previousFeetY <= s.Top &&
                currentFeetY >= s.Top)
            .OrderBy(s => s.Top)
            .FirstOrDefault();

        if (surface is null)
            return;

        CurrentSurface = surface;
        Y = surface.Top - GetCurrentFootY();
        FallSpeed = 0;

        StartIdle();
    }

    protected virtual bool ValidateSurface()
    {
        if (!IsStillOnSurface())
        {
            StartFalling();
            return false;
        }

        if (TargetPoi is null && !TargetStillOnCurrentSurface())
        {
            PickNewTarget();
            return false;
        }

        return true;
    }

    protected virtual void MoveTowardsTarget()
    {
        double dx = TargetX - X;
        double dy = TargetY - Y;

        double distance = Math.Sqrt(dx * dx + dy * dy);

        Logger.LogDebug(
            DebugCategory.Movement,
            $"[{GetType().Name}] " +
            $"Position=({X:F1}, {Y:F1}) " +
            $"Target=({TargetX:F1}, {TargetY:F1}) " +
            $"Distance={distance:F1}");

        if (distance < Run.ArrivalDistance)
        {
            if (TargetPoi?.Type == PointOfInterestType.Food)
            {
                Logger.LogDebug(
                    DebugCategory.Behavior,
                    $"ARRIVED! distance={distance}");

                StartEating(TargetPoi);
                return;
            }

            StartIdle();
            return;
        }

        SpeedX = dx / distance * (MovementSpeed * Settings.Scale);
        double speedY = dy / distance * (MovementSpeed * Settings.Scale);

        X += SpeedX;
        Y += speedY;

        if (CurrentSurface is not null)
        {
            X = Math.Clamp(
                X,
                CurrentSurface.Left,
                CurrentSurface.Right - Settings.SpriteWidth);
        }
    }

    public virtual void DragTo(double x, double y)
    {
        X = x;
        Y = y;
    }

    public virtual void Release()
    {
        SurfaceManager.Refresh();
        StartFalling();
    }

    public virtual void PickNewTarget()
    {
        if (!TryPickTargetOnCurrentSurface())
        {
            StartIdle();
            return;
        }

        MovementSpeed = Run.RunSpeed;

        StateTicksRemaining = Random.Next(
            Run.MinRunTicks,
            Run.MaxRunTicks);

        SetAction(CreatureAction.Running, "Run");
    }

    protected virtual void FinishEating()
    {
        Needs.Eat();

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"After Eat(): Hunger={Needs.Hunger:F2}, IsHungry={Needs.IsHungry}");

        EatingPoi = null;
        TargetPoi = null;

        PickPostEatTarget();
    }

    protected virtual void PickPostEatTarget()
    {
        if (CurrentSurface is null)
        {
            StartIdle();
            return;
        }

        int minX = CurrentSurface.Left;
        int maxX = CurrentSurface.Right - Settings.SpriteWidth;

        if (maxX <= minX)
        {
            StartIdle();
            return;
        }

        double direction = Random.Next(0, 2) == 0 ? -1 : 1;
        double desiredX = X + direction * Eat.LeaveFoodDistance;

        TargetX = Math.Clamp(desiredX, minX, maxX);
        TargetY = CurrentSurface.Top - GetCurrentFootY();
        MovementSpeed = Run.RunSpeed;

        StateTicksRemaining = Random.Next(
            Run.MinRunTicks,
            Run.MaxRunTicks);

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"[{GetType().Name}] Ate. Wandering away from food.");

        SetAction(CreatureAction.Running, "Run");
    }

    private bool TargetStillOnCurrentSurface()
    {
        return CurrentSurface is not null &&
               PositionFitsOnSurface(TargetX, TargetY, CurrentSurface);
    }

    private bool PoiIsOnSameSurface(PointOfInterest poi)
    {
        return CurrentSurface is not null &&
               PoiIsOnSurface(poi, CurrentSurface);
    }
}
