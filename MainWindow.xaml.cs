using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using Desktop_Creatures.Creatures;

namespace Desktop_Creatures;

public partial class MainWindow : Window
{
    private Eagle _eagle;

    private readonly DispatcherTimer _timer;

    private double _x;
    private double _y;

    private bool _isDragging = false;
    private System.Windows.Point _dragOffset;

    public MainWindow()
    {
        InitializeComponent();

        // Pick monitor: 0 = primary, 1 = second, etc.
        var screen = Forms.Screen.AllScreens[3];

        _x = screen.WorkingArea.Left + 200;
        _y = screen.WorkingArea.Top + 200;

        _eagle = new Eagle(_x, _y);

        Left = _x;
        Top = _y;

        Eagle.Source = _eagle.CurrentFrame;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += Update;
        _timer.Start();

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    private void Update(object? sender, EventArgs e)
    {
        if (_isDragging)
            return;

        _eagle.Update();

        _x = _eagle.X;
        _y = _eagle.Y;

        Eagle.Source = _eagle.CurrentFrame;
        FlipTransform.ScaleX = _eagle.SpeedX >= 0 ? 1 : -1;

        Left = _x;
        Top = _y;
    }

    private void OnMouseLeftButtonDown(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDragging = true;

        _dragOffset = e.GetPosition(this);

        CaptureMouse();
    }

    private void OnMouseMove(
        object sender,
        System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDragging)
            return;

        var mousePosition = System.Windows.Forms.Control.MousePosition;

        _x = mousePosition.X - _dragOffset.X;
        _y = mousePosition.Y - _dragOffset.Y;

        _eagle.DragTo(_x, _y);

        Left = _x;
        Top = _y;
    }

    private void OnMouseLeftButtonUp(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDragging = false;
        ReleaseMouseCapture();
        _eagle.Release();
    }
}