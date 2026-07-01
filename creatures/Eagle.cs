using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using Desktop_Creatures.Utilities;

namespace Desktop_Creatures.Creatures
{


    public class Eagle : Creature
    {
        private readonly Random _random = new();

        public enum DestinationType
        {
            Flying,
            Perching,
            Sleeping
        }

        private DestinationType _targetType;

        private readonly List<PointOfInterest> _pointsOfInterest;

        private readonly CreatureSettings _settings;

        private readonly Rectangle _workingArea;

        private bool _isGliding = false;
        private int _flightModeTicksRemaining = 0;

        private double _targetX;
        private double _targetY;

        private double _speed;
        private int _stateTicksRemaining;

        private FlightSettings Flight =>
        Settings.Flight
        ?? throw new InvalidOperationException(
            "Eagle requires FlightSettings.");

        private PerchSettings Perch =>
            Settings.Perch
            ?? throw new InvalidOperationException(
                "Eagle requires PerchSettings.");

        public Eagle(
            double startX,
            double startY,
            List<PointOfInterest> pointsOfInterest,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            Rectangle workingArea)
            : base(settings, pointOfInterestManager)
        {
            _settings = settings;
            _workingArea = workingArea;

            X = startX;
            Y = startY;
            _pointsOfInterest = pointsOfInterest;

            LoadAssets("Assets/Creatures/Eagle");

            SetAction(CreatureAction.Flying, "Fly");
            PickNewTarget();
        }

        //public override CreatureKind Kind => CreatureKind.Bird;

        protected override void UpdateState()
        {
            switch (CurrentAction)
            {
                case CreatureAction.Flying:
                case CreatureAction.Gliding:
                    UpdateFlight();
                    break;

                case CreatureAction.Perching:
                    UpdatePerch();
                    break;

                case CreatureAction.Sleeping:
                    UpdateSleep();
                    break;
            }

            UpdateAnimation();
        }

        private void UpdateSleep()
        {
            throw new NotImplementedException();
        }

        public override void Release()
        {
            SetAction(CreatureAction.Flying, "Fly");

            _isGliding = false;
            
            _flightModeTicksRemaining = _random.Next(Flight.MinFlapTicks, Flight.MaxFlapTicks);

            PickNewTarget();
        }

        private void UpdateFlight()
        {
            double dx = _targetX - X;
            double dy = _targetY - Y;

            _flightModeTicksRemaining--;

            if (_flightModeTicksRemaining <= 0)
            {
                ChooseFlightMode(dy);
            }

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < Flight.ArrivalDistance)
            {
                if (_targetType == DestinationType.Perching)
                    StartPerching();
                else
                    PickNewTarget();

                return;
            }

            _speed = _isGliding
                    ? Flight.GlideSpeed
                    : Flight.FlySpeed;

            SpeedX = dx / distance * (_speed * Settings.Scale);
            double speedY = dy / distance * (_speed * Settings.Scale);

            X += SpeedX;
            Y += speedY;
        }

        private void ChooseFlightMode(double dy)
        {
            if (dy > Flight.MinDownwardGlideDy)
            {
                // Flying downward

                if (_random.NextDouble() < Flight.GlideChance)
                {
                    SetAction(CreatureAction.Gliding, "Glide");
                    _isGliding = true;
                    
                    // Glide for 2-5 seconds
                    _flightModeTicksRemaining =
                        _random.Next(Flight.MinGlideTicks, Flight.MaxGlideTicks);
                }
                else
                {
                    SetAction(CreatureAction.Flying, "Fly");
                    _isGliding = false;            

                    // Flap for 1-3 seconds
                    _flightModeTicksRemaining =
                        _random.Next(Flight.MinFlapTicks, Flight.MaxFlapTicks);
                }
            }
            else
            {
                // Going upward or level
                SetAction(CreatureAction.Flying, "Fly");

                _isGliding = false;
                
                // Must flap
                _flightModeTicksRemaining =
                    _random.Next(Flight.MinUpwardFlapTicks, Flight.MaxUpwardFlapTicks);
            }
        }

        private void StartPerching()
        {
            SetAction(CreatureAction.Perching, "Perch");

            _stateTicksRemaining = _random.Next(
                Perch.MinPerchTicks,
                Perch.MaxPerchTicks);

            SpeedX = 0;
        }

        private void UpdatePerch()
        {
            _stateTicksRemaining--;

            if (_stateTicksRemaining <= 0)
            {
                StartFlyingFromPerch();
            }
        }
        private void StartFlyingFromPerch()
        {
            SetAction(CreatureAction.Flying, "Fly");

            _isGliding = false;
            _flightModeTicksRemaining = _random.Next(Flight.MinTakeoffFlapTicks, Flight.MaxTakeoffFlapTicks);

            AnimationTick = 0;
            CurrentFrameIndex = 0;

            PickNewTarget();
        }

        private void UpdateSleeping()
        {
            //CurrentFrame = _sleepFrame;
        }

        private void UpdateAnimation()
        {
            if (CurrentAction == CreatureAction.Flying)
            {
                AdvanceAnimation(Flight.FlyingFrameTicks);
            }
            else if (CurrentAction == CreatureAction.Gliding)
            {
                // probably no animation yet, unless glide has multiple frames
                AdvanceAnimation(Flight.FlyingFrameTicks);
            }
            else if (CurrentAction == CreatureAction.Perching)
            {
                AdvanceAnimation(Perch.PerchFrameTicks);
            }
        }

        private void PickNewTarget()
        {
            SetAction(CreatureAction.Flying, "Fly");

            // 35% chance to pick a perch anchor if one exists
            if (_random.NextDouble() < Perch.PerchChance && TryPickPerchTarget())
                return;

            PickRandomFlyingTarget();
        }

        private bool TryPickPerchTarget()
        {
            var perchTargets = _pointsOfInterest
                .SelectMany(poi => poi.AnchorPoints
                    .Where(anchor => anchor.Type == AnchorPointType.Perch)
                    .Select(anchor => poi.GetAnchorPosition(anchor)))
                .ToList();

            if (perchTargets.Count == 0)
                return false;

            var target = perchTargets[_random.Next(perchTargets.Count)];

            _targetX = target.X;
            _targetY = target.Y;
            _targetType = DestinationType.Perching;

            return true;
        }

        private void PickRandomFlyingTarget()
        {
            _targetX = _random.Next(
                _workingArea.Left, 
                _workingArea.Right - Settings.SpriteWidth);

            _targetY = _random.Next(
                _workingArea.Top, 
                _workingArea.Bottom - Settings.SpriteHeight);

            _targetType = DestinationType.Flying;
        }
    }
}
