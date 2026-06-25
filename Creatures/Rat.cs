using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace Desktop_Creatures.Creatures
{
    public class Rat : Creature
    {
        private readonly Random _random = new();

        //private DestinationType _targetType;

        private readonly List<PointOfInterest> _pointsOfInterest;

        private readonly CreatureSettings _settings;

        private readonly Rectangle _workingArea;

        private double _targetX;
        private double _targetY;

        private double _speed;
        private int _stateTicksRemaining;

        private double _fallSpeed = 0;

        private SurfaceManager _surfaceManager;
        private Surface? _currentSurface;

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

        public Rat(
            double startX,
            double startY,
            List<PointOfInterest> pointsOfInterest,
            CreatureSettings settings,
            Rectangle workingArea,
            SurfaceManager surfaceManager)
            : base(settings)
        {
            _settings = settings;
            _workingArea = workingArea;
            _surfaceManager = surfaceManager;
            _pointsOfInterest = pointsOfInterest;

            LoadAssets("Assets/Creatures/Rat");

            X = startX;
            Y = startY;

            _currentSurface = _surfaceManager.FindSurfaceBelow(
                X,
                Y,
                Settings.SpriteWidth,
                Settings.SpriteHeight);

            if (_currentSurface is not null)
                Y = _currentSurface.Top - Settings.SpriteHeight;

            SetAction(CreatureAction.Running, "Run");
            PickNewTarget();
        }

        public override void Update()
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
            }

            UpdateAnimation();
        }

        private void UpdateRunning()
        {
            MoveTowardsTarget();

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
            double previousFeetY = Y + Settings.SpriteHeight;

            _fallSpeed = Math.Min(
                _fallSpeed + Fall.Gravity,
                Fall.MaxFallSpeed);

            Y += _fallSpeed;

            double currentFeetY = Y + Settings.SpriteHeight;

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
            Y = surface.Top - Settings.SpriteHeight;
            _fallSpeed = 0;

            StartIdle();
        }

        private void MoveTowardsTarget()
        {
            double dx = _targetX - X;
            double dy = _targetY - Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < Run.ArrivalDistance)
            {
                StartIdle();
                //PickNewTarget();
                return;
            }

            SpeedX = dx / distance * _speed;
            double speedY = dy / distance * _speed;

            X += SpeedX;
            Y += speedY;
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

            _targetX = _random.Next(
                _currentSurface.Left,
                _currentSurface.Right - Settings.SpriteWidth);

            _targetY = _currentSurface.Top - Settings.SpriteHeight;

            _speed = Run.RunSpeed;
            _stateTicksRemaining = _random.Next(
                Run.MinRunTicks,
                Run.MaxRunTicks);

            SetAction(CreatureAction.Running, "Run");
        }
        public override void Release()
        {
            StartFalling();
        }

    }   
}
