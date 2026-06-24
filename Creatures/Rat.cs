using Desktop_Creatures.Config;
using Desktop_Creatures.World;
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

        private WalkSettings Walk =>
            Settings.Walk
            ?? throw new InvalidOperationException(
                "Rat requires WalkSettings.");

        private IdleSettings Idle =>
            Settings.Idle
            ?? throw new InvalidOperationException(
                "Rat requires PerchSettings.");


        private RunSettings Run =>
            Settings.Run
            ?? throw new InvalidOperationException(
                "Rat requires PerchSettings.");

        public Rat(
            double startX,
            double startY,
            List<PointOfInterest> pointsOfInterest,
            CreatureSettings settings,
            Rectangle workingArea)
            : base(settings)
        {
            _settings = settings;
            _workingArea = workingArea;

            X = startX;
            Y = startY;
            _pointsOfInterest = pointsOfInterest;

            LoadAssets("Assets/Creatures/Rat");

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
        }

        public override void PickNewTarget()
        {
            /*
            // Randomly choose a point of interest to walk to
            var poi = _pointsOfInterest[_random.Next(_pointsOfInterest.Count)];
            _targetX = poi.Position.X;
            _targetY = poi.Position.Y;
            // Randomly choose a speed for walking
            _speed = Run.RunSpeed;
            // Set the number of ticks to walk before picking a new target
            _stateTicksRemaining = _random.Next(Run.MinRunTicks, Run.MaxRunTicks);

            SetAction(CreatureAction.Running, "Run");
            */

            _targetX = _random.Next(
                _workingArea.Left,
                _workingArea.Right - Settings.SpriteWidth);

            _targetY = Y; // flat little rat runway

            _speed = Run.RunSpeed;
            _stateTicksRemaining = _random.Next(Run.MinRunTicks, Run.MaxRunTicks);

            SetAction(CreatureAction.Running, "Run");
        }
    }   
}
