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
        private double _speed;

        private int _stateTicksRemaining;

        private double _fallSpeed = 0;

        private int _foodSearchCooldownTicks = 0;  

        #region Settings
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
        #endregion

        public Rat(
            double startX,
            double startY,
            List<PointOfInterest> pointsOfInterest,
            PointOfInterestManager pointOfInterestManager,
            CreatureSettings settings,
            SurfaceManager surfaceManager)
            : base(settings, pointOfInterestManager, surfaceManager)
        {
            var variants = new[] { "Chocolate", "GreyHooded", "Albino", "Rainbow", "Black", "Cinnamon"};
            var variant = variants[Random.Next(variants.Length)];
            /*
            int roll = Random.Next(100);

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

        /*
        public void PlaceOnSurface(Surface surface)
        {
            _currentSurface = surface;

            X = surface.Left + (surface.Width - SpriteWidth) / 2.0;
            Y = surface.Top - GetCurrentFootY();
        }
        */

        protected override void UpdateState()
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
                case CreatureAction.Walking:
                    UpdateWalking();
                    break;
            }
        }

        protected override void UpdateTimers()
        {
            TickDown(ref EatingTicksRemaining);
            TickDown(ref _foodSearchCooldownTicks);
        }

        private void StartEating(PointOfInterest poi)
        {
            Logger.LogDebug(DebugCategory.Animation,
                "StartEating()" +
                $"Animation keys: {string.Join(", ", Animations.Keys)}\n" +
                $"Eat frame count: {Animations["Eat"].Length},\n" +
                $"Eat frame ticks: {Eat.EatFrameTicks}");

            EatingPoi = poi;
            EatingTicksRemaining = Eat.EatingTicksRemaining;

            Logger.LogDebug(
                DebugCategory.Behavior,
                $"EatingTicksRemaining loaded as: {Eat.EatingTicksRemaining}");

            SpeedX = 0;
            _stateTicksRemaining = 0;
            
            SetAction(CreatureAction.Eating, "Eat");
        }
        private void UpdateEating()
        {
            Logger.LogDebug(
                DebugCategory.Behavior,
                $"UpdateEating decrement timer to {EatingTicksRemaining}, FrameIndex={CurrentFrameIndex}");

            if (EatingTicksRemaining <= 0)
            {
                Needs.Eat();

                Logger.LogDebug(
                    DebugCategory.Behavior,
                    $"After Eat(): Hunger={Needs.Hunger:F2}, IsHungry={Needs.IsHungry}");

                EatingPoi = null;
                TargetPoi = null;

                //_eatCooldownTicks = EatCooldownDurationTicks;

                PickPostEatTarget();
            }
        }

        private void UpdateWalking()
        {

        }

        private void UpdateRunning()
        {
            TryFindFood();

            if (!ValidateSurface())
                return;

            MoveTowardsTarget();

            UpdateRunningTimer();
        }

        private void StartIdle()
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

            _stateTicksRemaining = Random.Next(
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
            _fallSpeed = 0;

            StartIdle();
        }

        private void TryFindFood()
        {
            if (Needs.IsHungry &&
                TargetPoi is null &&
                _foodSearchCooldownTicks <= 0)// &&
                //_eatCooldownTicks <= 0)
            {
                TargetPoi = PointOfInterestManager.FindNearest(
                    new Point(X, Y),
                    PointOfInterestType.Food);

                if (TargetPoi is not null && !PoiIsOnSameSurface(TargetPoi))
                {
                    TargetPoi = null;
                    _foodSearchCooldownTicks = Eat.FoodSearchCooldownTicks;
                    return;
                }

                if (TargetPoi is not null && CurrentSurface is not null)
                {
                    TargetX = TargetPoi.Position.X;
                    TargetY = CurrentSurface.Top - GetCurrentFootY();
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

            if (TargetPoi is null && !TargetStillOnCurrentSurface())
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



        private void MoveTowardsTarget()
        {
            double dx = TargetX - X;
            double dy = TargetY - Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            Logger.LogDebug(
                DebugCategory.Movement,
                $"[{GetType().Name}] " +
                $"Rat=({X:F1}, {Y:F1}) " +
                $"Target=({TargetX:F1}, {TargetY:F1}) " +
                $"Distance={distance:F1}");

            if (distance < Run.ArrivalDistance)
            {
                if (TargetPoi?.Type == PointOfInterestType.Food)
                {
                    StartEating(TargetPoi);
                    return;
                }

                StartIdle();
                return;
            }

            SpeedX = dx / distance * (_speed * Settings.Scale);
            double speedY = dy / distance * (_speed * Settings.Scale);

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

        protected override void PickPostEatTarget()
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
            _speed = Run.RunSpeed;

            _stateTicksRemaining = Random.Next(
                Run.MinRunTicks,
                Run.MaxRunTicks);


            Logger.LogDebug(
               DebugCategory.Behavior,
               $"[{GetType().Name}] Ate. Wandering away from food.");

            SetAction(CreatureAction.Running, "Run");
        }


        public override void PickNewTarget()
        {
            if (!TryPickTargetOnCurrentSurface())
            {
                StartIdle();
                return;
            }

            _speed = Run.RunSpeed;

            _stateTicksRemaining = Random.Next(
                Run.MinRunTicks,
                Run.MaxRunTicks);

            SetAction(CreatureAction.Running, "Run");
        }

        private bool IsStillOnSurface()
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

        private bool TargetStillOnCurrentSurface()
        {
            return CurrentSurface is not null &&
                   PositionFitsOnSurface(TargetX, TargetY, CurrentSurface);
        }

        public override void Release()
        {
            SurfaceManager.Refresh();
            StartFalling();
        }

        private bool PoiIsOnSameSurface(PointOfInterest poi)
        {
            return CurrentSurface is not null &&
                   PoiIsOnSurface(poi, CurrentSurface);
        }
    }   
}
