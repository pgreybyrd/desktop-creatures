using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace Desktop_Creatures;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    private double _x;
    private double _y;

    private double _targetX;
    private double _targetY;
    private const double Speed = 2.5;
    private const double ArrivalDistance = 20;

    private readonly Random _random = new();

    private double _speedX = 2;
    private double _speedY = 1;

    private int _directionChangeCounter = 0;

    private readonly double _eagleWidth = 29;
    private readonly double _eagleHeight = 13;

    private readonly BitmapImage[] _flyFrames =
{
    new(new Uri("pack://application:,,,/Assets/Eagle/fly_0.png")),
    new(new Uri("pack://application:,,,/Assets/Eagle/fly_1.png")),
    new(new Uri("pack://application:,,,/Assets/Eagle/fly_2.png")),
    new(new Uri("pack://application:,,,/Assets/Eagle/fly_3.png")),
};

    private int _frameIndex = 0;
    private int _animationTick = 0;

    private bool _isFlapping = true;
    private int _flapTicksRemaining = 60;
    private int _glideTicksRemaining = 120;

    public MainWindow()
    {
        InitializeComponent();

        // Pick monitor: 0 = primary, 1 = second, etc.
        var screen = Screen.AllScreens[3];

        _x = screen.WorkingArea.Left + 200;
        _y = screen.WorkingArea.Top + 200;

        PickNewTarget();

        Left = _x;
        Top = _y;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += Update;
        _timer.Start();
    }

    private void Update(object? sender, EventArgs e)
    {
        var screen = Forms.Screen.FromPoint(new System.Drawing.Point((int)_x, (int)_y));
        var area = screen.WorkingArea;

        double dx = _targetX - _x;
        double dy = _targetY - _y;

        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < ArrivalDistance)
        {
            PickNewTarget();
            return;
        }

        _speedX = dx / distance * Speed;
        _speedY = dy / distance * Speed;

        _x += _speedX;
        _y += _speedY;

        FlipTransform.ScaleX = _speedX >= 0 ? 1 : -1;

        UpdateAnimation();

        Left = _x;
        Top = _y;
    }

    private void PickNewTarget()
    {
        var screen = Forms.Screen.FromPoint(new System.Drawing.Point((int)_x, (int)_y));
        var area = screen.WorkingArea;

        _targetX = _random.Next(area.Left, area.Right - (int)_eagleWidth);
        _targetY = _random.Next(area.Top, area.Bottom - (int)_eagleHeight);
    }

    private void UpdateAnimation()
    {
        if (_isFlapping)
        {
            _flapTicksRemaining--;

            _animationTick++;

            // Change frame every ~6 ticks
            if (_animationTick >= 6)
            {
                _animationTick = 0;
                _frameIndex = (_frameIndex + 1) % _flyFrames.Length;
                Eagle.Source = _flyFrames[_frameIndex];
            }

            if (_flapTicksRemaining <= 0)
            {
                _isFlapping = false;
                _glideTicksRemaining = _random.Next(90, 220);

                // Pick a nice wings-out glide frame
                _frameIndex = 1;
                Eagle.Source = _flyFrames[_frameIndex];
            }
        }
        else
        {
            _glideTicksRemaining--;

            if (_glideTicksRemaining <= 0)
            {
                _isFlapping = true;
                _flapTicksRemaining = _random.Next(25, 70);
            }
        }
    }
}