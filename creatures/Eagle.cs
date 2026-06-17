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
        /***** V 0.2.5 *****/
        public enum DestinationType
        {
            Flying,
            Perching,
            Sleeping
        }

        private DestinationType _targetType;

        private readonly List<PointOfInterest> _pointsOfInterest = new()
        {
            new("Sky Perch", new Point(2600, -600), PointOfInterestType.Rest),
            new("Nest", new Point(2700, -300), PointOfInterestType.Home),
            new("Moon", new Point(2900, -850), PointOfInterestType.Magic),
        };

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
        Load("Assets/Eagle/fly_0.png"),
        Load("Assets/Eagle/fly_1.png"),
        Load("Assets/Eagle/fly_2.png"),
        Load("Assets/Eagle/fly_3.png"),
    };

        private readonly BitmapImage[] _perchFrames =
        {
        Load("Assets/Eagle/perch_left.png"),
        Load("Assets/Eagle/perch_right.png"),
    };

        private readonly BitmapImage[] _ruffleFrames =
        {
        Load("Assets/Eagle/ruffle_0.png"),
        Load("Assets/Eagle/ruffle_1.png"),
        Load("Assets/Eagle/ruffle_2.png"),
        Load("Assets/Eagle/ruffle_3.png"),
    };

        private readonly BitmapImage _sleepFrame =
            Load("Assets/Eagle/sleep_0.png");

        private readonly BitmapImage _glideFrame =
            Load("Assets/Eagle/glide_0.png");

        public Eagle(double startX, double startY)
        {
            X = startX;
            Y = startY;
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
                StartPerching();
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
            _stateTicksRemaining = _random.Next(120, 420);
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
            var screen = Forms.Screen.AllScreens[1];
            var area = screen.WorkingArea;

            _targetX = _random.Next(area.Left, area.Right - 32);
            _targetY = _random.Next(area.Top, area.Bottom - 32);
        }

        private static BitmapImage Load(string path)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/{path}"));
        }
    }
}
