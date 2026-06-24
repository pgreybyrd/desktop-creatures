using Desktop_Creatures.Creatures;
using Desktop_Creatures.World;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Desktop_Creatures.Config;
using System.Runtime.CompilerServices;

namespace Desktop_Creatures;

public partial class MainWindow : Window
{
    //private Eagle _eagle;

    private readonly DispatcherTimer _timer;

    private double _x;
    private double _y;

    private bool _isDragging = false;
    private System.Windows.Point _dragOffset;

    //private readonly List<Creature> _creatures = new();
    //private List<CreatureWindow> _creatureWindows;
    private Creature _activeCreature;

    public int moniterIndex = 0;

    public MainWindow()
    {
        InitializeComponent();

        var workingArea = LoadSettings();

        var screen = Forms.Screen.AllScreens[moniterIndex];

        //Topmost = settings.AlwaysOnTop;

        _x = screen.WorkingArea.Left + 100;
        _y = screen.WorkingArea.Top + 300;

        var treeWindow_0 = new TreeWindow("Assets/World/Trees/tree_0.png")
        {
            Left = _x + 0,
            Top = _y + 600
        };

        var treeWindow_1 = new TreeWindow("Assets/World/Trees/tree_1.png")
        {
            Left = _x + 400,
            Top = _y + 300
        };

        treeWindow_0.Show();
        treeWindow_1.Show();

        var treePoi_0 = new PointOfInterest(
            "Tree_0",
            new System.Windows.Point(treeWindow_0.Left, treeWindow_0.Top),
            PointOfInterestType.Rest
        );
        var treePoi_1 = new PointOfInterest(
            "Tree_1",
            new System.Windows.Point(treeWindow_1.Left, treeWindow_1.Top),
            PointOfInterestType.Rest
        );

        treePoi_0.AnchorPoints.Add(
            new AnchorPoint(
                "Upper Branch",
                AnchorPointType.Perch,
                new System.Windows.Point(35, 16)
            )
        );
        treePoi_1.AnchorPoints.Add(
            new AnchorPoint(
                "Upper Branch",
                AnchorPointType.Perch,
                new System.Windows.Point(35, 16)
            )
        );

        var pointsOfInterest = new List<PointOfInterest> { treePoi_0, treePoi_1 };

        //_creatures.Add(new Eagle(_x, _y, pointsOfInterest)); 

        //_eagle
        var creatureSettings = CreatureSettingsLoader.Load();

        var eagleSettings = creatureSettings.GetValueOrDefault(
            "eagle",
            new CreatureSettings()
        );

        _activeCreature = new Eagle(_x, _y, pointsOfInterest, eagleSettings, workingArea);
        Eagle.Source = _activeCreature.CurrentFrame;

        Left = _x;
        Top = _y;

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

    private Rectangle LoadSettings()
    {
        var settings = SettingsLoader.Load();

        moniterIndex = Math.Clamp(
            settings.WorkingMonitor,
            0,
            Forms.Screen.AllScreens.Length - 1
        );

        var screen = Forms.Screen.AllScreens[moniterIndex];
        var area = screen.WorkingArea;

        Topmost = settings.AlwaysOnTop;

        return area;
    }

    private void Update(object? sender, EventArgs e)
    {
        if (_isDragging)
            return;

        if (Eagle.Source != _activeCreature.CurrentFrame)
            Eagle.Source = _activeCreature.CurrentFrame;

        _activeCreature.Update();
        //foreach (var creature in _creatures)
        //{
        //    creature.Update();
        //}

        _x = _activeCreature.X;
        _y = _activeCreature.Y;

        
        FlipTransform.ScaleX = _activeCreature.SpeedX >= 0 ? 1 : -1;

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

        _activeCreature.DragTo(_x, _y);

        Left = _x;
        Top = _y;
    }

    private void OnMouseLeftButtonUp(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
    {
        _isDragging = false;
        ReleaseMouseCapture();
        _activeCreature.Release();
    }
}