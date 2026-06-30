using Desktop_Creatures.Config;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Diagnostics;
using System.Windows.Media.Animation;
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
            _workingArea = workingArea;
            _surfaceManager = surfaceManager;
            _pointsOfInterest = pointsOfInterest;
            _pointOfInterestManager = pointOfInterestManager;

            var variants = new[] { "Chocolate", "GreyHooded", "Albino", "Rainbow", "Black", "Cinnamon"};
            var variant = variants[_random.Next(variants.Length)];
            /*
            int roll = _random.Next(100);

            string variant =
                roll < 30 ? "GreyHooded" : // 30%
                roll < 55 ? "Black" : // 25%
                roll < 75 ? "Chocolate" : // 20%
                roll < 90 ? "Albino" : // 15%
                roll < 99 ? "Cinnamon" : // 9%
                              "Rainbow";   // 1%
            */

            LoadAssets($"Assets/Creatures/Rat/{variant}");

            X = startX;
            Y = startY;

            _currentSurface = _surfaceManager.FindSurfaceBelow(
                X,
                Y,
                Settings.SpriteWidth,
                Settings.SpriteHeight);

            if (_currentSurface is not null)
                Y = _currentSurface.Top - GetCurrentFootY();

            SetAction(CreatureAction.Running, "Run");
            PickNewTarget();
        }

        public void PlaceOnSurface(Surface surface)
        {
            _currentSurface = surface;

            X = surface.Left + (surface.Width - SpriteWidth) / 2.0;
            Y = surface.Top - GetCurrentFootY();
        }

        protected override void UpdateState()
        {
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
        }
        /*
         * protected override void UpdateState()
            {
                switch (CurrentAction)
                {
                    case CreatureAction.Idle:
                        UpdateIdle();
                        break;

                    case CreatureAction.Running:
                        UpdateRunning();
                        break;

                    case CreatureAction.Eating:
                        UpdateEating();
                        break;

                    case CreatureAction.Falling:
                        UpdateFalling();
                        break;
                }
            }
        */

        private void UpdateEating()
        {
            _eatingTicksRemaining--;
            AdvanceAnimation(Eat.EatFrameTicks);

            if (_eatingTicksRemaining <= 0)
            {
                Needs.Eat();

                _eatingPoi = null;
                _targetPoi = null;

                _eatCooldownTicks = EatCooldownDurationTicks;

                PickPostEatWanderTarget();
            }
        }

        private void UpdateRunning()
        {
            TryFindFood();

            if (!ValidateSurface())
                return;

            MoveTowardsTarget();

            UpdateRunningTimer();
        }

        private void TryFindFood()
        {
            if (Needs.IsHungry &&
                _targetPoi is null &&
                _foodSearchCooldownTicks <= 0 &&
                _eatCooldownTicks <= 0)
            {
                _targetPoi = _pointOfInterestManager.FindNearest(
                    new Point(X, Y),
                    PointOfInterestType.Food);

                if (_targetPoi is not null && !PoiIsReachableOnCurrentSurface(_targetPoi))
                {
                    _targetPoi = null;
                    _foodSearchCooldownTicks = 120; // about 2 seconds at 60fps
                    return;
                }

                if (_targetPoi is not null && _currentSurface is not null)
                {
                    _targetX = _targetPoi.Position.X;
                    _targetY = _currentSurface.Top - GetCurrentFootY();
                    _speed = Run.RunSpeed;

                    SetAction(CreatureAction.Running, "Run");
                }
            }
        }

        private bool ValidateSurface()
        {
            if (!IsStillOnSurface())
            {
                StartFalling();
                return false;
            }

            if (_targetPoi is null && !TargetStillOnCurrentSurface())
            {
                PickNewTarget();
                return false;
            }

            return true;
        }

        private void UpdateRunningTimer()
        {
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

            Logger.LogDebug(
                DebugCategory.Animation,
                $"StartIdle selected animation={animation.Name}");

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

            //AdvanceAnimation(Idle.IdleFrameTicks);

            if (_stateTicksRemaining <= 0)
                PickNewTarget();
        }

        private void StartFalling()
        {
            Logger.LogDebug(
                DebugCategory.Animation,
                $"StartFalling");

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
                DebugCategory.Movement,
                $"[{GetType().Name}] " +
                $"Rat=({X:F1}, {Y:F1}) " +
                $"Target=({_targetX:F1}, {_targetY:F1}) " +
                $"Distance={distance:F1}");

            if (distance < Run.ArrivalDistance)
            {
                if (_targetPoi?.Type == PointOfInterestType.Food)
                {
                    StartEating(_targetPoi);
                    return;
                }

                StartIdle();
                return;
            }

            SpeedX = dx / distance * (_speed * Settings.Scale);
            double speedY = dy / distance * (_speed * Settings.Scale);

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

           Logger.LogDebug(
               DebugCategory.Behavior,
               $"[{GetType().Name}] Ate. Wandering away from food.");

            SetAction(CreatureAction.Running, "Run");
        }

        protected override void UpdateAnimation()
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

            Logger.LogDebug(DebugCategory.Surface,
                $"Surface L={_currentSurface.Left} R={_currentSurface.Right} T={_currentSurface.Top} " +
                $"Rat X={X} Y={Y} TargetX={_targetX} TargetY={_targetY}");

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
                CreatureAction.Running => SpriteHeight - (5 * Settings.Scale),
                CreatureAction.Idle => SpriteHeight - (5 * Settings.Scale),
                CreatureAction.Falling => SpriteHeight - (5 * Settings.Scale),
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
            Logger.LogDebug(DebugCategory.Behavior,
                "StartEating()" +
                $"Animation keys: {string.Join(", ", Animations.Keys)}" +
                $"Eat frame count: {Animations["Eat"].Length}");

            _eatingPoi = poi;
            _eatingTicksRemaining = 180;
            SpeedX = 0;
            _stateTicksRemaining = 0;
            SetAction(CreatureAction.Eating, "Eat");
        }
    }   
}
