using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Diagnostics;
using Desktop_Creatures.Utilities;

using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures
{
    public class Rat : Creature
    {
        private readonly Random _random = new();

        //private DestinationType _targetType;

        private readonly List<PointOfInterest> _pointsOfInterest;
        private PointOfInterestManager _pointOfInterestManager;
        private PointOfInterest? _targetPoi;

        //private readonly CreatureSettings _settings;

        private readonly Rectangle _workingArea;

        private double _targetX;
        private double _targetY;

        private double _speed;
        private int _stateTicksRemaining;

        private double _fallSpeed = 0;

        private SurfaceManager _surfaceManager;
        private Surface? _currentSurface;

        private int _foodSearchCooldownTicks = 0;
        private int _eatCooldownTicks = 0;
        private const int EatCooldownDurationTicks = 300; // ~5 seconds at 60fps
        private const int PostEatWanderDistance = 120;
        private int _eatingTicksRemaining;
        private PointOfInterest? _eatingPoi;

        private WalkSettings Walk =>
            Settings.Walk
            ?? throw new InvalidOperationException(
                "Rat requires WalkSettings.");

        private IdleSettings Idle =>
            Settings.Idle
            ?? throw new InvalidOperationException(
                "Rat requires IdleSettings.");

        private RunSettings Run =>
            Settings.Run
            ?? throw new InvalidOperationException(
                "Rat requires RunSettings.");

        private FallSettings Fall =>
            Settings.Fall
            ?? throw new InvalidOperationException(
                "Rat requires FallSettings.");

        private EatSettings Eat =>
            Settings.Eat
            ?? throw new InvalidOperationException(
                "Rat requires EatSettings.");

        public Rat(
            double startX,
            double startY,
            List<PointOfInterest> pointsOfInterest,
            PointOfInterestManager pointOfInterestManager,
            CreatureSettings settings,
            Rectangle workingArea,
            SurfaceManager surfaceManager)
            : base(settings)
        {
            //_settings = settings;
            _workingArea = workingArea;
            _surfaceManager = surfaceManager;
            _pointsOfInterest = pointsOfInterest;
            _pointOfInterestManager = pointOfInterestManager;

            LoadAssets("Assets/Creatures/Rat");

            X = startX;
            Y = startY;

            _currentSurface = _surfaceManager.FindSurfaceBelow(
                X,
                Y,
                Settings.SpriteWidth,
                Settings.SpriteHeight);

            if (_currentSurface is not null)
                Y = _currentSurface.Top - GetCurrentFootY();
            //Y = _currentSurface.Top - Settings.SpriteHeight;

            SetAction(CreatureAction.Running, "Run");
            PickNewTarget();
        }

        public void PlaceOnSurface(Surface surface)
        {
            _currentSurface = surface;

            X = surface.Left + (surface.Width - SpriteWidth) / 2.0;
            Y = surface.Top - GetCurrentFootY();
            //Y = surface.Top - SpriteHeight;
        }

        public override void Update()
        {
            //Logger.LogDebug("Rat.Update started");
            Logger.LogDebug(
    $"Current action = {CurrentAction}");

            Needs.Update();

            if (_eatCooldownTicks > 0)
                _eatCooldownTicks--;

            if (_foodSearchCooldownTicks > 0)
                _foodSearchCooldownTicks--;

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

            UpdateAnimation();
        }

        private void UpdateEating()
        {
            Logger.LogDebug(
    $"UpdateEating() ticks={_eatingTicksRemaining}");

            _eatingTicksRemaining--;
            AdvanceAnimation(Eat.EatFrameTicks);
            //SetAction(CreatureAction.Eating, "Eat");

            if (_eatingTicksRemaining <= 0)
            {
                Needs.Eat();

                Logger.LogDebug(
                    $"[{GetType().Name}] Finished eating.");

                Logger.LogDebug(
                    $"[{GetType().Name}] Hunger reset to {Needs.Hunger:F2}");

                _eatingPoi = null;
                _targetPoi = null;

                _eatCooldownTicks = EatCooldownDurationTicks;

                PickPostEatWanderTarget();
            }
        }

        private void UpdateRunning()
        {
            if (Needs.IsHungry &&
                _targetPoi is null &&
                _foodSearchCooldownTicks <= 0 &&
                _eatCooldownTicks <= 0)
            {
                Logger.LogDebug($"[{GetType().Name}] Hungry. Searching for food...");

                _targetPoi = _pointOfInterestManager.FindNearest(
                    new Point(X, Y),
                    PointOfInterestType.Food);

                if (_targetPoi is not null && !PoiIsReachableOnCurrentSurface(_targetPoi))
                {
                    Logger.LogDebug(
                        $"[{GetType().Name}] Food found but not reachable on current surface: {_targetPoi.Name}");

                    _targetPoi = null;
                    _foodSearchCooldownTicks = 120; // about 2 seconds at 60fps
                    return;
                }

                if (_targetPoi is not null && _currentSurface is not null)
                {
                    _targetX = _targetPoi.Position.X;
                    _targetY = _currentSurface.Top - GetCurrentFootY();
                    _speed = Run.RunSpeed;

                    Logger.LogDebug($"[{GetType().Name}] Heading toward {_targetPoi.Name}");
                    SetAction(CreatureAction.Running, "Run");
                }
            }

            if (!IsStillOnSurface())
            {
                StartFalling();
                return;
            }

            if (_targetPoi is null && !TargetStillOnCurrentSurface())
            {
                PickNewTarget();
                return;
            }

            MoveTowardsTarget();

            if (CurrentAction != CreatureAction.Running)
                return;

            _stateTicksRemaining--;

            if (_stateTicksRemaining <= 0)
                StartIdle();
        }

        private void StartIdle()
        {
            var animation = Idle.Animations[
                _random.Next(Idle.Animations.Count)
            ];

            SetAction(
                CreatureAction.Idle,
                animation.Name);

            SpeedX = 0;

            _stateTicksRemaining = _random.Next(
                Idle.MinIdleTicks,
                Idle.MaxIdleTicks);
        }

        private void UpdateIdle()
        {
            if (!IsStillOnSurface())
            {
                StartFalling();
                return;
            }

            _stateTicksRemaining--;

            AdvanceAnimation(Idle.IdleFrameTicks);

            if (_stateTicksRemaining <= 0)
                PickNewTarget();
        }

        private void StartFalling()
        {
            SetAction(CreatureAction.Falling, "Fall");
            SpeedX = 0;
            _fallSpeed = 0;
        }
        private void UpdateFalling()
        {
            double previousFeetY = Y + GetCurrentFootY();

            _fallSpeed = Math.Min(
                _fallSpeed + Fall.Gravity,
                Fall.MaxFallSpeed);

            Y += _fallSpeed;

            double currentFeetY = Y + GetCurrentFootY();

            var surface = _surfaceManager.Surfaces
                .Where(s =>
                    X + Settings.SpriteWidth / 2.0 >= s.Left &&
                    X + Settings.SpriteWidth / 2.0 <= s.Right &&
                    previousFeetY <= s.Top &&
                    currentFeetY >= s.Top)
                .OrderBy(s => s.Top)
                .FirstOrDefault();

            if (surface is null)
                return;

            _currentSurface = surface;
            Y = surface.Top - GetCurrentFootY();
            _fallSpeed = 0;

            StartIdle();
        }

        private void MoveTowardsTarget()
        {
            double dx = _targetX - X;
            double dy = _targetY - Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            Logger.LogDebug(
                $"[{GetType().Name}] " +
                $"Rat=({X:F1}, {Y:F1}) " +
                $"Target=({_targetX:F1}, {_targetY:F1}) " +
                $"Distance={distance:F1}");

            if (distance < Run.ArrivalDistance)
            {
                if (_targetPoi?.Type == PointOfInterestType.Food)
                {
                    Logger.LogDebug(
                        $"[{GetType().Name}] Arrived at {_targetPoi?.Name}");
                    Logger.LogDebug(
                        $"[{GetType().Name}] Eating.");

                    Logger.LogDebug(
                        $"[{GetType().Name}] " +
                        $"Bowl actual position = ({_targetPoi.Position.X:F1}, {_targetPoi.Position.Y:F1})");

                    Logger.LogDebug(
                        $"[{GetType().Name}] " +
                        $"Rat position = ({X:F1}, {Y:F1})");

                    Logger.LogDebug(
                        $"[{GetType().Name}] " +
                        $"Target position = ({_targetX:F1}, {_targetY:F1})");

                    Logger.LogDebug(
                        $"[{GetType().Name}] " +
                        $"Arrival distance = {distance:F1}");

                    StartEating(_targetPoi);
                    return;
                    /*
                    Needs.Eat();

                    Logger.LogDebug(
                        $"[{GetType().Name}] Hunger reset to {Needs.Hunger:F2}");

                    _targetPoi = null;
                    _eatCooldownTicks = EatCooldownDurationTicks;

                    PickPostEatWanderTarget();
                    return;
                    */
                }

                StartIdle();
                return;
            }

            SpeedX = dx / distance * _speed;
            double speedY = dy / distance * _speed;

            X += SpeedX;
            Y += speedY;

            if (_currentSurface is not null)
            {
                X = Math.Clamp(
                    X,
                    _currentSurface.Left,
                    _currentSurface.Right - Settings.SpriteWidth);
            }
        }

        private void PickPostEatWanderTarget()
        {
            if (_currentSurface is null)
            {
                StartIdle();
                return;
            }

            int minX = _currentSurface.Left;
            int maxX = _currentSurface.Right - Settings.SpriteWidth;

            if (maxX <= minX)
            {
                StartIdle();
                return;
            }

            double direction = _random.Next(0, 2) == 0 ? -1 : 1;
            double desiredX = X + direction * PostEatWanderDistance;

            _targetX = Math.Clamp(desiredX, minX, maxX);
            _targetY = _currentSurface.Top - GetCurrentFootY();
            _speed = Run.RunSpeed;

            _stateTicksRemaining = _random.Next(
                Run.MinRunTicks,
                Run.MaxRunTicks);

            Logger.LogDebug($"[{GetType().Name}] Ate. Wandering away from food.");

            SetAction(CreatureAction.Running, "Run");
        }

        private void UpdateAnimation()
        {
            if (CurrentAction == CreatureAction.Running)
                AdvanceAnimation(Run.RunFrameTicks);

            else if (CurrentAction == CreatureAction.Idle)
                AdvanceAnimation(Idle.IdleFrameTicks);

            else if (CurrentAction == CreatureAction.Falling)
                AdvanceAnimation(Fall.FallFrameTicks);
        }

        public override void PickNewTarget()
        {
            if (_currentSurface is null)
                return;

            int minX = _currentSurface.Left;
            int maxX = _currentSurface.Right - Settings.SpriteWidth;

            if (maxX <= minX)
            {
                StartIdle();
                return;
            }

            _targetX = _random.Next(minX, maxX);
            _targetY = _currentSurface.Top - GetCurrentFootY();

            _speed = Run.RunSpeed;

            _stateTicksRemaining = _random.Next(
                Run.MinRunTicks,
                Run.MaxRunTicks);

            //Debug.WriteLine(
               //$"Surface L={_currentSurface.Left} R={_currentSurface.Right} T={_currentSurface.Top} " +
               //$"Rat X={X} Y={Y} TargetX={_targetX} TargetY={_targetY}");

            SetAction(CreatureAction.Running, "Run");
        }

        private bool IsStillOnSurface()
        {
            var surface = _surfaceManager.FindSurfaceAtFeet(
                X,
                Y,
                Settings.SpriteWidth,
                GetCurrentFootY(),
                LandingTolerance);

            if (surface is null)
                return false;

            _currentSurface = surface;
            return true;
        }

        private bool TargetStillOnCurrentSurface()
        {
            if (_currentSurface is null)
                return false;

            return
                _targetX >= _currentSurface.Left &&
                _targetX <= _currentSurface.Right - Settings.SpriteWidth &&
                Math.Abs(_targetY - (_currentSurface.Top - GetCurrentFootY())) <= LandingTolerance;
        }

        public override void Release()
        {
            _surfaceManager.Refresh();
            StartFalling();
        }

        protected virtual int GetCurrentFootY()
        {
            return CurrentAction switch
            {
                CreatureAction.Running => SpriteHeight - 5,
                CreatureAction.Idle => SpriteHeight - 5,
                CreatureAction.Falling => SpriteHeight - 5,
                _ => SpriteHeight
            };
        }

        private bool PoiIsReachableOnCurrentSurface(PointOfInterest poi)
        {
            if (_currentSurface is null)
                return false;

            double poiFeetY = poi.Position.Y + poi.Settings.Height;
            double surfaceY = _currentSurface.Top;

            return
                poi.Position.X >= _currentSurface.Left &&
                poi.Position.X <= _currentSurface.Right &&
                Math.Abs(poiFeetY - surfaceY) <= LandingTolerance;
        }

        private void StartEating(PointOfInterest poi)
        {
            Logger.LogDebug("StartEating()");

            Logger.LogDebug(
                $"Animation keys: {string.Join(", ", Animations.Keys)}");

            Logger.LogDebug(
                $"Eat frame count: {Animations["Eat"].Length}");

            _eatingPoi = poi;
            _eatingTicksRemaining = 180;
            SpeedX = 0;
            _stateTicksRemaining = 0;
            SetAction(CreatureAction.Eating, "Eat");
        }
    }   
}
