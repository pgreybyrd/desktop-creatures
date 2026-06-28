using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;

namespace Desktop_Creatures;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    private double _x;
    private double _y;

    private bool _isDragging = false;
    private System.Windows.Point _dragOffset;

    private BitmapImage _spawnRatNormal = null!;
    private BitmapImage _spawnRatHover = null!;
    private BitmapImage _spawnRatPressed = null!;
    private BitmapImage _clearRatsNormal = null!;
    private BitmapImage _clearRatsHover = null!;
    private BitmapImage _clearRatsPressed = null!;
    private BitmapImage _alwaysOnTopOnNormal = null!;
    private BitmapImage _alwaysOnTopOnHover = null!;
    private BitmapImage _alwaysOnTopOnPressed = null!;
    private BitmapImage _alwaysOnTopOffNormal = null!;
    private BitmapImage _alwaysOnTopOffHover = null!;
    private BitmapImage _alwaysOnTopOffPressed = null!;
    private BitmapImage _exitNormal = null!;
    private BitmapImage _exitHover = null!;
    private BitmapImage _exitPressed = null!;
    private BitmapImage _minimizeNormal = null!;
    private BitmapImage _minimizeHover = null!;
    private BitmapImage _minimizePressed = null!;
    private BitmapImage _xNormal = null!;
    private BitmapImage _xHover = null!;
    private BitmapImage _xPressed = null!;

    private readonly List<CreatureWindow> _creatureWindows = new();
    private Creature _activeCreature;

    public int moniterIndex = 0;

    private bool _creaturesAlwaysOnTop = true;

    private Rectangle _workingArea;
    private Dictionary<string, CreatureSettings> _creatureSettings = new();
    private List<PointOfInterest> _pointsOfInterest = new();
    private PointOfInterestManager _pointOfInterestManager;

    private readonly SurfaceManager _surfaceManager = new();

    private const int MaxRats = 20;

    public MainWindow()
    {
        InitializeComponent();

        _workingArea = LoadSettings();
        _surfaceManager.Refresh();

        _spawnRatNormal = LoadUiImage("Assets/UI/button_spawn_rat.png");
        _spawnRatHover = LoadUiImage("Assets/UI/button_hover_spawn_rat.png");
        _spawnRatPressed = LoadUiImage("Assets/UI/button_pressed_spawn_rat.png");

        _clearRatsNormal = LoadUiImage("Assets/UI/button_clear_rats.png");
        _clearRatsHover = LoadUiImage("Assets/UI/button_hover_clear_rats.png");
        _clearRatsPressed = LoadUiImage("Assets/UI/button_pressed_clear_rats.png");

        _alwaysOnTopOnNormal = LoadUiImage("Assets/UI/button_always_on_top_on.png");
        _alwaysOnTopOnHover = LoadUiImage("Assets/UI/button_hover_always_on_top_on.png");
        _alwaysOnTopOnPressed = LoadUiImage("Assets/UI/button_pressed_always_on_top_on.png");

        _alwaysOnTopOffNormal = LoadUiImage("Assets/UI/button_always_on_top_off.png");
        _alwaysOnTopOffHover = LoadUiImage("Assets/UI/button_hover_always_on_top_off.png");
        _alwaysOnTopOffPressed = LoadUiImage("Assets/UI/button_pressed_always_on_top_off.png");

        _exitNormal = LoadUiImage("Assets/UI/button_exit.png");
        _exitHover = LoadUiImage("Assets/UI/button_hover_exit.png");
        _exitPressed = LoadUiImage("Assets/UI/button_pressed_exit.png");

        _minimizeNormal = LoadUiImage("Assets/UI/button_minimize.png");
        _minimizeHover = LoadUiImage("Assets/UI/button_hover_minimize.png");
        _minimizePressed = LoadUiImage("Assets/UI/button_pressed_minimize.png");

        _xNormal = LoadUiImage("Assets/UI/button_X.png");
        _xHover = LoadUiImage("Assets/UI/button_hover_X.png");
        _xPressed = LoadUiImage("Assets/UI/button_pressed_X.png");

        SpawnRatImage.Source = _spawnRatNormal;
        ClearRatsImage.Source = _clearRatsNormal;
        AlwaysOnTopToggleImage.Source = _alwaysOnTopOnNormal;
        ExitImage.Source = _exitNormal;
        MinimizeImage.Source = _minimizeNormal;
        XImage.Source = _xNormal;

        _pointOfInterestManager = new PointOfInterestManager();

        
        var bowl = new PointOfInterest(
            "Rat Bowl",
            new Point(900, 900),
            PointOfInterestType.Food,
            "Assets/World/Food/bowl_full.png");
        bowl.EmptyAssetPath = "Assets/World/Food/bowl_empty.png";

        _pointOfInterestManager.Add(bowl);
        
        /*
        var tree = new PointOfInterest(
            "Oak",
            new Point(1000 ,1000),
            PointOfInterestType.Rest,
            "Assets/World/Trees/tree_1.png");

        tree.AnchorPoints.Add(new AnchorPoint("perch", AnchorPointType.Perch, new Point(10, 10)));

        _pointOfInterestManager.Add(tree);
        */

        var screen = Forms.Screen.PrimaryScreen!;

        _creatureSettings = CreatureSettingsLoader.Load();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += Update;
        _timer.Start();

        
        foreach (var point in _pointOfInterestManager.Points) {
            var window = new POIWindow(point);
            window.Show();
        }
        

        ContentRendered += (_, _) =>
        {
            UpdateMenuSurface();
            _surfaceManager.Refresh();

            SpawnRat();
            SetCreaturesTopmost(_creaturesAlwaysOnTop);
        };
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

        _surfaceManager.Update();

        foreach (var creatureWindow in _creatureWindows)
        {
            creatureWindow.UpdateCreature();
        }
    }

    private void TitleBar_MouseLeftButtonDown(
    object sender,
    MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void SpawnRat()
    {
        //UpdateMenuSurface();
        //_surfaceManager.Refresh();

        var menuSurface = _surfaceManager.MenuSurface
            ?? throw new InvalidOperationException("Menu surface was not set.");

        int ratCount = _creatureWindows.Count(w =>
        w.GetCreature() is Rat);

        if (ratCount >= MaxRats)
        {
            System.Windows.MessageBox.Show(
                $"Maximum rat count reached: {MaxRats}",
                "Too many rats!");
            return;
        }

        var ratSettings = _creatureSettings.GetValueOrDefault(
            "rat",
            new CreatureSettings());

        double spawnX = menuSurface.Left + 
            (menuSurface.Right - menuSurface.Left - ratSettings.SpriteWidth) / 2.0;

        double spawnY = menuSurface.Top - ratSettings.SpriteHeight;

        var rat = new Rat(
            spawnX,
            spawnY,
            _pointsOfInterest,
            _pointOfInterestManager,
            ratSettings,
            new Rectangle((int)Left, (int)Top, (int)Width, (int)Height),
            _surfaceManager);

        var ratWindow = new CreatureWindow(rat)
        {
            Topmost = _creaturesAlwaysOnTop
        };

        //System.Windows.MessageBox.Show(
            //$"Rat spawning at X={rat.X}, Y={rat.Y}\n" +
            //$"Primary screen: L={area.Left}, T={area.Top}, R={area.Right}, B={area.Bottom}");
        ratWindow.Show();

        _creatureWindows.Add(ratWindow);
    }


    private void ClearRats()
    {
        var ratWindows = _creatureWindows
            .Where(w => w.GetCreature() is Rat)
            .ToList();

        foreach (var window in ratWindows)
        {
            window.Close();
            _creatureWindows.Remove(window);
        }
    }

    private void ClearEagles()
    {
        var eagleWindows = _creatureWindows
            .Where(w => w.GetCreature() is Eagle)
            .ToList();

        foreach (var window in eagleWindows)
        {
            window.Close();
            _creatureWindows.Remove(window);
        }
    }
    private void ClearRats_Click(object sender, RoutedEventArgs e)
    {
        ClearRats();
    }
    private void ClearEagles_Click(object sender, RoutedEventArgs e)
    {
        ClearEagles();
    }
    private void SpawnEagle()
    {
        var eagleSettings = _creatureSettings.GetValueOrDefault(
            "eagle",
            new CreatureSettings());

        var screen = Forms.Screen.PrimaryScreen!;
        var area = screen.WorkingArea;

        var eagle = new Eagle(
            area.Left + 100,
            area.Top + 300,
            _pointsOfInterest,
            eagleSettings,
            area);

        var eagleWindow = new CreatureWindow(eagle)
        {
           Topmost = _creaturesAlwaysOnTop
        };
        eagleWindow.Show();

        _creatureWindows.Add(eagleWindow);
    }

    private void UpdateMenuSurface()
    {
        int surfaceX = (int)(Left + 111);
        int surfaceY = (int)(Top + 42);
        int surfaceWidth = 151;

        _surfaceManager.SetMenuSurface(
            new Rectangle(
                surfaceX,
                surfaceY,
                surfaceWidth,
                1));
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

private void AlwaysOnTopToggle_Click(object sender, RoutedEventArgs e)
{
    _creaturesAlwaysOnTop = !_creaturesAlwaysOnTop;

    SetCreaturesTopmost(_creaturesAlwaysOnTop);

    AlwaysOnTopToggleImage.Source =
        _creaturesAlwaysOnTop
            ? _alwaysOnTopOnHover
            : _alwaysOnTopOffHover;
}

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
            UpdateMenuSurface();
            //System.Windows.MessageBox.Show($"Menu title at X={_surfaceManager.MenuSurface.Left}, Y ={_surfaceManager.MenuSurface.Top}\n");
            _surfaceManager.Refresh();
        }
    }

    private BitmapImage LoadUiImage(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri($"pack://application:,,,/{path}");
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private void SpawnRat_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnRatHover;
    }
    private void SpawnRat_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnRatNormal;
    }
    private void SpawnRat_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SpawnRatImage.Source = _spawnRatPressed;
    }
    private void SpawnRat_MouseUp(object sender, MouseButtonEventArgs e)
    {
        SpawnRatImage.Source = _spawnRatHover;
    }

    private void ClearRats_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsHover;
    }
    private void ClearRats_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsNormal;
    }
    private void ClearRats_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsPressed;
    }
    private void ClearRats_MouseUp(object sender, MouseButtonEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsHover;
    }

    private void AlwaysOnTop_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AlwaysOnTopToggleImage.Source =
            _creaturesAlwaysOnTop
                ? _alwaysOnTopOnHover
                : _alwaysOnTopOffHover;
    }
    private void AlwaysOnTop_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        AlwaysOnTopToggleImage.Source =
            _creaturesAlwaysOnTop
                ? _alwaysOnTopOnNormal
                : _alwaysOnTopOffNormal;
    }

    private void AlwaysOnTop_MouseDown(object sender, MouseButtonEventArgs e)
    {
        AlwaysOnTopToggleImage.Source =
            _creaturesAlwaysOnTop
                ? _alwaysOnTopOnPressed
                : _alwaysOnTopOffPressed;
    }

    private void AlwaysOnTop_MouseUp(object sender, MouseButtonEventArgs e)
    {
        AlwaysOnTopToggleImage.Source =
            _creaturesAlwaysOnTop
                ? _alwaysOnTopOnHover
                : _alwaysOnTopOffHover;
    }

    private void Exit_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ExitImage.Source = _exitHover;
    }
    private void Exit_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        ExitImage.Source = _exitNormal;
    }
    private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ExitImage.Source = _exitPressed;
    }
    private void Exit_MouseUp(object sender, MouseButtonEventArgs e)
    {
        ExitImage.Source = _exitHover;
    }

    private void Minimize_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        MinimizeImage.Source = _minimizeHover;
    }
    private void Minimize_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        MinimizeImage.Source = _minimizeNormal;
    }
    private void Minimize_MouseDown(object sender, MouseButtonEventArgs e)
    {
        MinimizeImage.Source = _minimizePressed;
    }
    private void Minimize_MouseUp(object sender, MouseButtonEventArgs e)
    {
        MinimizeImage.Source = _minimizeHover;
    }

    private void X_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        XImage.Source = _xHover;
    }
    private void X_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        XImage.Source = _xNormal;
    }
    private void X_MouseDown(object sender, MouseButtonEventArgs e)
    {
        XImage.Source = _xPressed;
    }
    private void X_MouseUp(object sender, MouseButtonEventArgs e)
    {
        XImage.Source = _xHover;
    }
}