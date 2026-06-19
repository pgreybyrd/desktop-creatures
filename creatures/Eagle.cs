using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using System;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Desktop_Creatures.Creatures
{
    public enum EagleState
    {
        Flying,
        Perching,
        Sleeping
    }

    public class Eagle
    {
        private readonly Random _random = new();

        public double X { get; private set; }
        public double Y { get; private set; }
        public double SpeedX { get; private set; }
        public EagleState State { get; private set; } = EagleState.Flying;
        public enum DestinationType
        {
            Flying,
            Perching,
            Sleeping
        }

        private DestinationType _targetType;

        private readonly List<PointOfInterest> _pointsOfInterest;

        private bool _isGliding = false;
        private int _flightModeTicksRemaining = 0;

        private double _targetX;
        private double _targetY;

        private double _speed = 2.5;
        private const double ArrivalDistance = 20;

        private int _stateTicksRemaining;

        private int _frameIndex;
        private int _animationTick;

        public BitmapImage CurrentFrame { get; private set; }

        private readonly BitmapImage[] _flyFrames =
        {
        Load("Assets/Creatures/Eagle/fly_0.png"),
        Load("Assets/Creatures/Eagle/fly_1.png"),
        Load("Assets/Creatures/Eagle/fly_2.png"),
        Load("Assets/Creatures/Eagle/fly_3.png"),
    };

        private readonly BitmapImage[] _perchFrames =
        {
        Load("Assets/Creatures/Eagle/perch_left.png"),
        Load("Assets/Creatures/Eagle/perch_right.png"),
    };

        private readonly BitmapImage[] _ruffleFrames =
        {
        Load("Assets/Creatures/Eagle/ruffle_0.png"),
        Load("Assets/Creatures/Eagle/ruffle_1.png"),
        Load("Assets/Creatures/Eagle/ruffle_2.png"),
        Load("Assets/Creatures/Eagle/ruffle_3.png"),
    };

        private readonly BitmapImage _sleepFrame =
            Load("Assets/Creatures/Eagle/sleep_0.png");

        private readonly BitmapImage _glideFrame =
            Load("Assets/Creatures/Eagle/glide_0.png");

        public Eagle(double startX, double startY, List<PointOfInterest> pointsOfInterest)
        {
            X = startX;
            Y = startY;
            _pointsOfInterest = pointsOfInterest;

            CurrentFrame = _flyFrames[0];
            PickNewTarget();
        }

        public void Update()
        {
            if (State == EagleState.Flying)
                UpdateFlying();
            else if (State == EagleState.Perching)
                UpdatePerching();
            else if (State == EagleState.Sleeping)
                UpdateSleeping();

            UpdateAnimation();
        }

        public void DragTo(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Release()
        {
            State = EagleState.Flying;

            _isGliding = false;
            _flightModeTicksRemaining = _random.Next(60, 140);

            PickNewTarget();
        }

        private void UpdateFlying()
        {
            double dx = _targetX - X;
            double dy = _targetY - Y;

            _flightModeTicksRemaining--;

            if (_flightModeTicksRemaining <= 0)
            {
                ChooseFlightMode(dy);
            }

            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < ArrivalDistance)
            {
                if (_targetType == DestinationType.Perching)
                    StartPerching();
                else
                    PickNewTarget();

                return;
            }

            if (_isGliding)
            {
                _speed = 3.5;
            }
            else
            {
                _speed = 2.5;
            }

            SpeedX = dx / distance * _speed;
            double speedY = dy / distance * _speed;

            X += SpeedX;
            Y += speedY;
        }

        private void ChooseFlightMode(double dy)
        {
            if (dy > 5)
            {
                // Flying downward

                if (_random.NextDouble() < 0.6)
                {
                    _isGliding = true;

                    // Glide for 2-5 seconds
                    _flightModeTicksRemaining =
                        _random.Next(120, 300);
                }
                else
                {
                    _isGliding = false;

                    // Flap for 1-3 seconds
                    _flightModeTicksRemaining =
                        _random.Next(60, 180);
                }
            }
            else
            {
                // Going upward or level

                _isGliding = false;

                // Must flap
                _flightModeTicksRemaining =
                    _random.Next(90, 240);
            }
        }

        private void StartPerching()
        {
            State = EagleState.Perching;

            // 5-12 seconds at 60 FPS
            _stateTicksRemaining = _random.Next(300, 720);

            SpeedX = 0;
            CurrentFrame = _perchFrames[_random.Next(_perchFrames.Length)];
        }

        private void UpdatePerching()
        {
            _stateTicksRemaining--;

            if (_stateTicksRemaining <= 0)
            {
                StartFlyingFromPerch();
            }
        }
        private void StartFlyingFromPerch()
        {
            State = EagleState.Flying;

            _isGliding = false;
            _flightModeTicksRemaining = _random.Next(60, 140);

            _animationTick = 0;
            _frameIndex = 0;
            CurrentFrame = _flyFrames[0];

            PickNewTarget();
        }

        private void UpdateSleeping()
        {
            CurrentFrame = _sleepFrame;
        }

        private void UpdateAnimation()
        {
            _animationTick++;

            if (State == EagleState.Flying)
            {
                if (_isGliding)
                {
                    CurrentFrame = _glideFrame;
                }
                else if (_animationTick >= 8)
                {
                    _animationTick = 0;
                    _frameIndex = (_frameIndex + 1) % _flyFrames.Length;
                    CurrentFrame = _flyFrames[_frameIndex];
                }
            }
            else if (State == EagleState.Perching)
            {
                if (_animationTick >= 60)
                {
                    _animationTick = 0;

                    // Mostly look around, sometimes ruffle.
                    if (_random.NextDouble() < 0.25)
                        CurrentFrame = _ruffleFrames[_random.Next(_ruffleFrames.Length)];
                    else
                        CurrentFrame = _perchFrames[_random.Next(_perchFrames.Length)];
                }
            }
        }

        private void PickNewTarget()
        {
            // 35% chance to pick a perch anchor if one exists
            if (_random.NextDouble() < 0.35 && TryPickPerchTarget())
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
            var settings = SettingsLoader.Load();

            var monitorIndex = Math.Clamp(
                settings.WorkingMonitor,
                0,
                Forms.Screen.AllScreens.Length - 1
            );

            var screen = Forms.Screen.AllScreens[monitorIndex];
            var area = screen.WorkingArea;

            _targetX = _random.Next(area.Left, area.Right - 32);
            _targetY = _random.Next(area.Top, area.Bottom - 32);
            _targetType = DestinationType.Flying;
        }

        private static BitmapImage Load(string path)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/{path}"));
        }
    }
}
