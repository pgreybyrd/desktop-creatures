using Desktop_Creatures.Audio;
using Desktop_Creatures.Behaviors;
using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.Needs;
using Desktop_Creatures.Personality;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;
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
    Drinking,
    Running,
    Falling,
    Chasing,
    Carrying,
    Held
}

public abstract class Creature
{
    public Guid Id { get; }
    public string Name { get; protected set; }
    public string CreatureType { get; }
    public CreatureAppearance? Appearance { get; protected set; }
    public string? AppearanceId { get; protected set; }
    public CreatureAppearanceTraits? AppearanceTraits =>
        Appearance?.Traits;

    protected readonly Random Random = new();

    private CreatureSoundPlayer? _soundPlayer;

    private int _animationDirection = 1;

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
    protected int InteractionTicksRemaining;

    protected CreatureSettings Settings { get; }

    public int Scale => Settings.Scale;
    public int DisplayScale { get; private set; } = 1;
    public int SizeTier { get; init; }

    public double DisplayWidth =>
        SpriteWidth * DisplayScale;
    public double DisplayFootY =>
        CurrentFootY * DisplayScale;
    public double DisplayLeft =>
        X - ((DisplayWidth - SpriteWidth) / 2.0);
    public double DisplayCenterX =>
        DisplayLeft + (DisplayWidth / 2.0);

    public bool SpriteFacesRight => Settings.SpriteFacesRight;
    public int SpriteWidth => Settings.SpriteWidth * Settings.Scale;
    public int SpriteHeight => Settings.SpriteHeight * Settings.Scale;
    public int CurrentFootY => GetCurrentFootY();
    public double LandingTolerance => Settings.LandingTolerance;

    private readonly Point _pickupAnchor;

    protected virtual int FootOffsetY => SpriteHeight;

    public virtual Point PickupAnchor =>
        _pickupAnchor;


    protected PointOfInterest? TargetPoi;

    protected WorldInteractionTarget?
        TargetInteraction;

    protected PointOfInterest? InteractionPoi;

    protected Surface? CurrentSurface;

    public CreatureAction CurrentAction { get; protected set; }

    public event Action? InteractionStarted;

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

    protected Dictionary<string, BitmapSource[]> Animations { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public BitmapSource? CurrentFrame =>
        CurrentFrames.Length > 0
            ? CurrentFrames[CurrentFrameIndex]
            : null;

    protected BitmapSource[] CurrentFrames = [];
    protected int CurrentFrameIndex;
    protected int AnimationTick;

    protected Creature(
        CreatureDefinition definition,
        CreatureSettings settings,
        PointOfInterestManager pointOfInterestManager,
        SurfaceManager surfaceManager,
        Guid? id = null,
        string? name = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name ?? string.Empty;
        CreatureType = definition.Id;

        //_pickupAnchor =
        //    new Point(
        //        definition.PickupAnchor.X,
        //        definition.PickupAnchor.Y);
        Settings = settings;

        _pickupAnchor =
            definition.PickupAnchor is not null
                ? new Point(
                    definition.PickupAnchor.X,
                    definition.PickupAnchor.Y)
                : new Point(
                    SpriteWidth / 2.0,
                    SpriteHeight / 4.0);


        PointOfInterestManager = pointOfInterestManager;
        SurfaceManager = surfaceManager;

        ConfigureDefaultBehaviors();
    }

    protected void InitializeGeneratedAppearance(
        CreatureDefinition definition,
        CreatureAppearanceTraits? appearanceTraits = null,
        string? appearanceId = null)
    {
        CreatureAppearanceTraits selectedTraits;

        AppearanceId =
            appearanceId;

        if (appearanceId is not null)
        {
            selectedTraits =
                CreatureAppearanceFactory.LoadTraits(
                    definition,
                    appearanceId);
        }
        else if (appearanceTraits is not null)
        {
            selectedTraits =
                appearanceTraits;
        }
        else
        {
            selectedTraits =
                CreateRandomAppearanceTraits(
                    definition);
        }

        Appearance =
            CreatureAppearanceFactory.Create(
                definition,
                selectedTraits);

        LoadAppearanceAnimations(
            definition,
            Appearance);
    }

    protected virtual CreatureAppearanceTraits
        CreateRandomAppearanceTraits(
            CreatureDefinition definition)
    {
        string? palette =
            definition.Palettes.Length > 0
                ? definition.Palettes[
                    Random.Next(
                        definition.Palettes.Length)]
                : null;

        return new CreatureAppearanceTraits(
            Palette: palette,
            Patterns: [],
            Accessories: [],
            Effects: []);
    }

    protected void InitializeCreatureAssets(
        CreatureDefinition definition,
        CreatureAppearanceTraits? appearanceTraits = null,
        string? appearanceId = null)
    {
        if (definition.Appearance?.Generated == true)
        {
            InitializeGeneratedAppearance(
                definition,
                appearanceTraits,
                appearanceId);

            return;
        }

        LoadAssets(
            definition.AssetFolder);
    }

    private void LoadAppearanceAnimations(
        CreatureDefinition definition,
        CreatureAppearance appearance)
    {
        var sheet =
            SpriteSheetLoader.Load(
                appearance.SpriteSheet,
                $"{definition.AssetFolder}/Appearance/{definition.Id}.json");

        foreach (var animation in
                 sheet.Animations)
        {
            OverrideAnimation(
                animation.Key,
                animation.Value.Frames.Select(
                    frame => frame.Image));
        }
    }

    private void ConfigureDefaultBehaviors()
    {
        if (Settings.Eat is null)
            return;

        BehaviorController.AddBehavior(
            new NeedInteractionBehavior(
                needs: Needs,
                needType: NeedType.Hunger,

                poiManager: PointOfInterestManager,
                poiType: PointOfInterestType.Food,
                interactionType: WorldInteractionPointType.Eat,

                getPosition:
                    () => new Point(X, Y),

                canSearch:
                    CanSearchForInteraction,

                trySetTarget:
                    TrySetInteractionTarget,

                searchCooldownTicks:
                    Eat.FoodSearchCooldownTicks));

        BehaviorController.AddBehavior(
            new NeedInteractionBehavior(
                needs: Needs,
                needType: NeedType.Thirst,

                poiManager: PointOfInterestManager,
                poiType: PointOfInterestType.Water,
                interactionType: WorldInteractionPointType.Drink,

                getPosition:
                    () => new Point(X, Y),

                canSearch:
                    CanSearchForInteraction,

                trySetTarget:
                    TrySetInteractionTarget,

                searchCooldownTicks:
                    Eat.FoodSearchCooldownTicks));
    }

    protected void OverrideAnimation(
        string animationName,
        IEnumerable<BitmapSource> frames)
    {
        Animations[animationName] =
            frames.ToArray();
    }

    public void SetDisplayScale(int scale)
    {
        DisplayScale =
            Math.Clamp(scale, 1, 4);

        if (Settings.Run is not null)
        {
            MovementSpeed =
                Run.RunSpeed * DisplayScale;
        }
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
        {
            Animations["Eat"] =
                LoadFrames(
                    assetFolder,
                    "eat",
                    Settings.Eat.EatFrameCount);

            Animations["Drink"] =
                LoadFrames(
                    assetFolder,
                    "drink",
                    Settings.Eat.EatFrameCount);
        }
    }

    protected static BitmapSource[] LoadFrames(
        string assetFolder,
        string animationName,
        int frameCount)
    {
        return Enumerable.Range(0, frameCount)
            .Select(i =>
                AssetImageLoader.Load(
                    $"{assetFolder}/{animationName}_{i}.png"))
            .ToArray();
    }

    protected void InitializeGroundCreature(double startX, double startY)
    {
        X = startX;
        Y = startY;

        CurrentSurface = SurfaceManager.FindSurfaceBelow(
            X,
            Y,
            SpriteWidth,
            GetCurrentFootY());

        if (CurrentSurface is not null)
            Y = CurrentSurface.Top - GetCurrentFootY();

        Logger.LogDebug(
            DebugCategory.Movement,
            $"Spawn X={X:F1}, Y={Y:F1}, " +
            $"FeetY={Y + GetCurrentFootY():F1}, " +
            $"MenuTop={SurfaceManager.MenuSurface?.Top}");

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

        SetAction(
            CreatureAction.Falling,
            "fall");

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

        CurrentFrameIndex = 0;
        _animationDirection = 1;
        AnimationTick = 0;
    }

    protected virtual void UpdateTimers()
    {
        if (CurrentAction is
            CreatureAction.Eating or
            CreatureAction.Drinking)
        {
            TickDown(
                ref InteractionTicksRemaining);
        }

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
                (Y + SpriteHeight)
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
            x <= surface.Right - SpriteWidth &&
            Math.Abs(y - (surface.Top - GetCurrentFootY())) <= LandingTolerance;
    }

    protected virtual bool TryPickTargetOnCurrentSurface()
    {
        if (CurrentSurface is null)
            return false;

        int minX = CurrentSurface.Left;
        int maxX = CurrentSurface.Right - SpriteWidth;

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
            CreatureAction.Running =>
                SpriteHeight -
                (Settings.FootOffsetY * Settings.Scale),

            CreatureAction.Idle =>
                SpriteHeight -
                (Settings.FootOffsetY * Settings.Scale),

            CreatureAction.Eating =>
                SpriteHeight -
                (Settings.FootOffsetY * Settings.Scale),

            CreatureAction.Falling =>
                SpriteHeight -
                (Settings.FootOffsetY * Settings.Scale),

            CreatureAction.Held =>
                SpriteHeight -
                (Settings.FootOffsetY * Settings.Scale),

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
            case CreatureAction.Drinking:
                UpdateDrinking();
                break;
        }
    }

    protected virtual void FinishDrinking()
    {
        Needs.Drink();

        InteractionPoi = null;
        TargetPoi = null;

        ReleaseTargetInteraction();

        PickPostInteractionTarget();
    }

    protected virtual void UpdateEating()
    {
        UpdateInteraction(
            FinishEating);
    }

    protected virtual void UpdateDrinking()
    {
        UpdateInteraction(
            FinishDrinking);
    }

    private void UpdateInteraction(
    Action finishAction)
    {
        if (!IsInteractionTargetStillValid())
        {
            CancelInteraction();
            return;
        }

        if (InteractionTicksRemaining <= 0)
            finishAction();
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

            CreatureAction.Held
                => 8,

            CreatureAction.Flying when Settings.Flight is not null
                => Settings.Flight.FlyingFrameTicks,

            CreatureAction.Gliding when Settings.Flight is not null
                => Settings.Flight.FlyingFrameTicks,

            CreatureAction.Perching when Settings.Perch is not null
                => Settings.Perch.PerchFrameTicks,

            CreatureAction.Eating when Settings.Eat is not null
                => Settings.Eat.EatFrameTicks,

            CreatureAction.Drinking when Settings.Eat is not null
                => Settings.Eat.EatFrameTicks,

            CreatureAction.Sleeping when Settings.Sleep is not null
                => Settings.Sleep.SleepFrameTicks,

            _ => null
        };

        if (frameTicks is not null)
            AdvanceAnimation(frameTicks.Value);
    }

    protected virtual bool CanSearchForInteraction()
    {
        return Settings.Run is not null &&
               CurrentSurface is not null &&
               TargetInteraction is null &&
               CurrentAction is CreatureAction.Running or CreatureAction.Idle;
    }

    protected virtual bool TrySetInteractionTarget(
        WorldInteractionTarget target)
    {
        if (!CanSearchForInteraction())
            return false;

        if (!target.IsValid)
            return false;

        if (!target.InteractionPoint.TryReserve())
            return false;

        Point? snappedPosition =
            SurfaceManager.SnapToSurface(
                target.Position,
                SpriteWidth,
                GetCurrentFootY(),
                10);

        if (snappedPosition is null)
        {
            target.InteractionPoint.Release();
            return false;
        }

        if (!CanReachInteractionTarget(
                snappedPosition.Value))
        {
            target.InteractionPoint.Release();
            return false;
        }

        TargetPoi =
            target.PointOfInterest;

        TargetInteraction =
            target;

        TargetX =
            snappedPosition.Value.X;

        TargetY =
            snappedPosition.Value.Y;

        MovementSpeed =
            Run.RunSpeed * DisplayScale;

        SetAction(
            CreatureAction.Running,
            "Run");

        return true;
    }

    private bool RefreshInteractionTargetPosition()
    {
        if (TargetInteraction is null)
            return false;

        if (!TargetInteraction.IsValid)
            return false;

        var position =
            TargetInteraction.Position;

        var snappedPosition =
            SurfaceManager.SnapToSurface(
                position,
                SpriteWidth,
                GetCurrentFootY(),
                10);

        if (snappedPosition is null)
            return false;

        if (!CanReachInteractionTarget(
                snappedPosition.Value))
        {
            return false;
        }

        TargetX =
            snappedPosition.Value.X;

        TargetY =
            snappedPosition.Value.Y;

        return true;
    }

    private bool CanInteractWithTarget()
    {
        if (TargetInteraction is null)
            return false;

        if (!TargetInteraction.IsValid)
            return false;

        Point? snappedPosition =
            SurfaceManager.SnapToSurface(
                TargetInteraction.Position,
                SpriteWidth,
                GetCurrentFootY(),
                10);

        if (snappedPosition is null)
            return false;

        double dx =
            snappedPosition.Value.X - X;

        double dy =
            snappedPosition.Value.Y - Y;

        double distance =
            Math.Sqrt(
                dx * dx +
                dy * dy);

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"Interaction check: " +
            $"creature=({X:F1}, {Y:F1}) " +
            $"snappedTarget=({snappedPosition.Value.X:F1}, {snappedPosition.Value.Y:F1}) " +
            $"distance={distance:F1}, " +
            $"allowed={Eat.InteractionReach:F1}");

        return distance <=
            Eat.InteractionReach;
    }

    private void ReleaseTargetInteraction()
    {
        if (TargetInteraction is null)
            return;

        TargetInteraction.InteractionPoint.Release();
        TargetInteraction = null;
    }

    private bool IsInteractionTargetStillValid()
    {
        if (TargetInteraction is null)
            return false;

        if (!TargetInteraction.IsValid)
            return false;

        return CanInteractWithTarget();
    }

    protected virtual void StartEating(
    PointOfInterest poi)
    {
        StartInteraction(
            poi,
            CreatureAction.Eating,
            "Eat");
    }

    protected virtual void StartDrinking(
        PointOfInterest poi)
    {
        StartInteraction(
            poi,
            CreatureAction.Drinking,
            "Drink");
    }

    private void StartInteraction(
        PointOfInterest poi,
        CreatureAction action,
        string animationName)
    {
        InteractionPoi = poi;
        InteractionTicksRemaining =
            Eat.EatingTicksRemaining; // temporary shared timing

        SpeedX = 0;
        StateTicksRemaining = 0;

        InteractionStarted?.Invoke();

        SetAction(
            action,
            animationName);
    }

    private void CancelInteraction()
    {
        Logger.LogDebug(
            DebugCategory.Behavior,
            "Interaction cancelled because target is no longer valid.");

        InteractionPoi = null;
        TargetPoi = null;

        ReleaseTargetInteraction();

        InteractionTicksRemaining = 0;

        StartIdle();
    }

    protected virtual void UpdateRunning()
    {
        if (!ValidateSurface())
            return;

        MoveTowardsTarget();

        if (CurrentAction != CreatureAction.Running)
            return;

        // Interaction travel should continue until the interaction
        // succeeds, fails, or is cancelled. The normal wander timer
        // should only control ordinary wandering.
        if (TargetInteraction is null &&
            StateTicksRemaining <= 0)
        {
            StartIdle();
        }
    }

    protected virtual void StartIdle()
    {
        var idleAnimations =
            Animations.Keys
                .Where(name =>
                    name.StartsWith(
                        "idle_",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (idleAnimations.Count == 0)
        {
            throw new InvalidOperationException(
                "Creature has no loaded idle animations.");
        }

        string animationName =
            idleAnimations[
                Random.Next(idleAnimations.Count)];

        SetAction(
            CreatureAction.Idle,
            animationName);

        Logger.LogDebug(
            DebugCategory.Animation,
            $"StartIdle selected animation={animationName}");

        SpeedX = 0;

        StateTicksRemaining =
            Random.Next(
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

        var surface =
            SurfaceManager.Surfaces
                .Where(s =>
                    DisplayCenterX >= s.Left &&
                    DisplayCenterX <= s.Right &&
                    previousFeetY <= s.Top &&
                    currentFeetY >= s.Top)
                .OrderBy(s => s.Top)
                .FirstOrDefault();

        if (surface is null)
            return;

        Logger.LogDebug(
            DebugCategory.Surface,
            $"LANDING SURFACE: " +
            $"Kind={surface.Kind}, " +
            $"Type={surface.GetType().Name}, " +
            $"Bounds=({surface.Left}, {surface.Top}) - " +
            $"({surface.Right}, {surface.Bottom}), " +
            $"Rat=({X:F1}, {Y:F1}), " +
            $"Scale={DisplayScale}");

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
        if (TargetInteraction is not null)
        {
            if (!RefreshInteractionTargetPosition())
            {
                ReleaseTargetInteraction();
                TargetPoi = null;

                StartIdle();
                return;
            }
        }

        double dx = TargetX - X;
        double dy = TargetY - Y;

        double distance = Math.Sqrt(dx * dx + dy * dy);

        Logger.LogDebug(
            DebugCategory.Movement,
            $"[{GetType().Name}] " +
            $"Position=({X:F1}, {Y:F1}) " +
            $"Target=({TargetX:F1}, {TargetY:F1}) " +
            $"Distance={distance:F1} " +
            $"Interaction={TargetInteraction?.InteractionPoint.Type.ToString() ?? "none"} " +
            $"Surface=" +
            $"{(CurrentSurface is null ? "none" : $"[{CurrentSurface.Left},{CurrentSurface.Right}] Top={CurrentSurface.Top}")}");

        if (distance < Run.ArrivalDistance)
        {
            // This is an interaction destination.
            if (TargetInteraction is not null)
            {
                if (!CanInteractWithTarget())
                    return;

                Logger.LogDebug(
                    DebugCategory.Behavior,
                    $"ARRIVED! distance={distance}");

                switch (TargetInteraction.InteractionPoint.Type)
                {
                    case WorldInteractionPointType.Eat:
                        StartEating(TargetPoi!);
                        return;

                    case WorldInteractionPointType.Drink:
                        StartDrinking(TargetPoi!);
                        return;
                }

                // Unknown interaction type for now.
                StartIdle();
                return;
            }

            // This was just an ordinary wandering destination.
            StartIdle();
            return;
        }

        double moveSpeed =
            MovementSpeed *
            Settings.Scale;

        double step =
            Math.Min(
                moveSpeed,
                distance);

        SpeedX =
            dx / distance *
            step;

        double speedY =
            dy / distance *
            step;

        X += SpeedX;
        Y += speedY;

        if (CurrentSurface is not null)
        {
            double beforeClampX = X;

            X = Math.Clamp(
                X,
                CurrentSurface.Left,
                CurrentSurface.Right - SpriteWidth);

            if (Math.Abs(beforeClampX - X) > 0.01)
            {
                Logger.LogDebug(
                    DebugCategory.Movement,
                    $"X CLAMPED: {beforeClampX:F1} -> {X:F1}");
            }
        }
    }

    private bool CanReachInteractionTarget(
        Point snappedPosition)
    {
        if (CurrentSurface is null)
            return false;

        double targetSurfaceTop =
            snappedPosition.Y +
            GetCurrentFootY();

        // Ground creatures can stay on their current level
        // or descend to lower surfaces.
        // They cannot currently travel upward.
        return targetSurfaceTop >=
            CurrentSurface.Top -
            LandingTolerance;
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

        MovementSpeed = Run.RunSpeed * DisplayScale;

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
            $"After Eat(): " +
            $"Hunger={Needs.GetNeed(NeedType.Hunger).Value:F2}, " +
            $"IsHungry={Needs.IsHungry}");

        InteractionPoi = null;
        TargetPoi = null;

        ReleaseTargetInteraction();

        PickPostInteractionTarget();
    }

    protected virtual void PickPostInteractionTarget()
    {
        if (CurrentSurface is null)
        {
            StartIdle();
            return;
        }

        int minX = CurrentSurface.Left;
        int maxX = CurrentSurface.Right - SpriteWidth;

        if (maxX <= minX)
        {
            StartIdle();
            return;
        }

        double direction = Random.Next(0, 2) == 0 ? -1 : 1;
        double desiredX = X + direction * Eat.LeaveFoodDistance;

        TargetX = Math.Clamp(desiredX, minX, maxX);
        TargetY = CurrentSurface.Top - GetCurrentFootY();
        MovementSpeed = Run.RunSpeed * DisplayScale;

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

    public void UpdateHeldAnimation()
    {
        if (CurrentAction == CreatureAction.Held)
            AdvanceAnimationPingPong(8);
    }

    protected void AdvanceAnimationPingPong(
        int frameTicks)
    {
        if (CurrentFrames.Length <= 1)
            return;

        AnimationTick++;

        if (AnimationTick < frameTicks)
            return;

        AnimationTick = 0;

        CurrentFrameIndex +=
            _animationDirection;

        if (CurrentFrameIndex >= CurrentFrames.Length - 1)
        {
            CurrentFrameIndex =
                CurrentFrames.Length - 1;

            _animationDirection = -1;
        }
        else if (CurrentFrameIndex <= 0)
        {
            CurrentFrameIndex = 0;
            _animationDirection = 1;
        }
    }

    protected void SetSoundSet(
        SoundSet soundSet)
    {
        _soundPlayer =
            new CreatureSoundPlayer(
                soundSet);
    }

    protected void PlaySound(
        string soundEvent)
    {
        _soundPlayer?.PlayRandom(
            soundEvent);
    }

    public virtual void OnPickedUp()
    {
    }
}
