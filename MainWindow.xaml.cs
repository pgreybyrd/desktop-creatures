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

    private readonly List<CreatureWindow> _creatureWindows = new();
    private Creature _activeCreature;

    public int moniterIndex = 0;

    private Rectangle _workingArea;
    private Dictionary<string, CreatureSettings> _creatureSettings = new();
    private List<PointOfInterest> _pointsOfInterest = new();

    public MainWindow()
    {
        InitializeComponent();

        _workingArea = LoadSettings();

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

        _creatureSettings = CreatureSettingsLoader.Load();

        Left = _x;
        Top = _y;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += Update;
        _timer.Start();

        //MouseLeftButtonDown += OnMouseLeftButtonDown;
        //MouseMove += OnMouseMove;
        //MouseLeftButtonUp += OnMouseLeftButtonUp;
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

        foreach (var creatureWindow in _creatureWindows)
        {
            creatureWindow.UpdateCreature();
        }
    }
    private void SpawnRat()
    {
        var ratSettings = _creatureSettings.GetValueOrDefault(
            "rat",
            new CreatureSettings());

        var rat = new Rat(
            _workingArea.Left + 200,
            _workingArea.Top + 1540,
            _pointsOfInterest,
            ratSettings,
            _workingArea);

        var ratWindow = new CreatureWindow(rat)
        {
            Topmost = AlwaysOnTopToggle.IsChecked == true
        };
        ratWindow.Show();

        _creatureWindows.Add(ratWindow);
    }

    private void SpawnEagle()
    {
        var eagleSettings = _creatureSettings.GetValueOrDefault(
            "eagle",
            new CreatureSettings());

        var eagle = new Eagle(
            _workingArea.Left + 100,
            _workingArea.Top + 300,
            _pointsOfInterest,
            eagleSettings,
            _workingArea);

        var eagleWindow = new CreatureWindow(eagle)
        {
            Topmost = AlwaysOnTopToggle.IsChecked == true
        };
        eagleWindow.Show();

        _creatureWindows.Add(eagleWindow);
    }

    private void SpawnRat_Click(object sender, RoutedEventArgs e)
    {
        SpawnRat();
    }

    private void SpawnEagle_Click(object sender, RoutedEventArgs e)
    {
        SpawnEagle();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void SetCreaturesTopmost(bool isTopmost)
    {
        foreach (var creatureWindow in _creatureWindows)
            creatureWindow.Topmost = isTopmost;
    }

    private void AlwaysOnTopToggle_Checked(object sender, RoutedEventArgs e)
    {
        SetCreaturesTopmost(true);
    }

    private void AlwaysOnTopToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        SetCreaturesTopmost(false);
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

        foreach (var creatureWindow in _creatureWindows)
        {
            if (creatureWindow.IsMouseOver)
            {
                //_activeCreature = creatureWindow.getCreature();
                break;
            }
        }   

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