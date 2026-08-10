using Desktop_Creatures.Assets.UI;
using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using Point = System.Windows.Point;

namespace Desktop_Creatures;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    private double _x;
    private double _y;

    private bool _isDragging = false;
    private System.Windows.Point _dragOffset;

    private AppSettings _settings = null!;
    private SettingsWindow? _settingsWindow;

    private FieldGuideMenu? _fieldGuideMenu;

    private UiButtonImages _spawnRatImages = null!;
    private UiButtonImages _fieldGuideImages = null!;
    private UiButtonImages _clearRatsImages = null!;
    private UiButtonImages _alwaysOnTopOnImages = null!;
    private UiButtonImages _alwaysOnTopOffImages = null!;
    private UiButtonImages _exitImages = null!;
    private UiButtonImages _minimizeImages = null!;
    private UiButtonImages _closeImages = null!;

    private readonly List<CreatureWindow> _creatureWindows = new();
    private Creature _activeCreature;

    public int _moniterIndex = 0;

    private bool _creaturesAlwaysOnTop = true;
    //when settings are implemented
    public bool EcosystemAlwaysOnTop { get; set; } = true;
    public bool UiAlwaysOnTop { get; set; } = false;
    private bool _ecosystemAlwaysOnTop = true;
    private bool _uiAlwaysOnTop = false;

    private Rectangle _workingArea;
    private Dictionary<string, CreatureSettings> _creatureSettings = new();
    private Dictionary<string, PointOfInterestSettings> _pointOfInterestSettings = new();

    private List<PointOfInterest> _pointsOfInterest = new();
    private PointOfInterestManager _pointOfInterestManager;

    private readonly SurfaceManager _surfaceManager = new();

    private const int MaxRats = 20;

    private int _uiScale = 1;

    public MainWindow()
    {
        InitializeComponent();

        _workingArea = LoadSettings();

        _surfaceManager.Refresh();

        //_spawnRatImages = LoadButtonImages("spawn_rat");
        _fieldGuideImages = LoadButtonImages("field_guide");
        _clearRatsImages = LoadButtonImages("clear_rats");
        _alwaysOnTopOnImages = LoadButtonImages("always_on_top_on");
        _alwaysOnTopOffImages = LoadButtonImages("always_on_top_off");
        _exitImages = LoadButtonImages("exit");
        _minimizeImages = LoadButtonImages("minimize");
        _closeImages = LoadButtonImages("X");

        //SpawnRatImage.Source = _spawnRatImages.Normal;
        FieldGuideImage.Source = _fieldGuideImages.Normal;
        ClearRatsImage.Source = _clearRatsImages.Normal;
        AlwaysOnTopToggleImage.Source = _alwaysOnTopOffImages.Normal;
        ExitImage.Source = _exitImages.Normal;
        MinimizeImage.Source = _minimizeImages.Normal;
        XImage.Source = _closeImages.Normal;

        _pointOfInterestManager = new PointOfInterestManager();

        var screen = Forms.Screen.PrimaryScreen!;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        _timer.Tick += Update;
        _timer.Start();       

        ContentRendered += (_, _) =>
        {
            UpdateMenuSurface();

            _surfaceManager.Refresh();

            CreateFoodBowl();

            SpawnRat();

            SetCreaturesTopmost(_creaturesAlwaysOnTop);
        };
    }

    private void CreateFoodBowl()
    {
        if (!_pointOfInterestSettings.TryGetValue(
            "rat_bowl",
            out var bowlSettings))
        {
            System.Windows.MessageBox.Show("rat_bowl settings not found!");
            return;
        }

        var menuSurface = _surfaceManager.MenuSurface
            ?? throw new InvalidOperationException(
                "Menu surface must exist before creating the bowl.");

        double bowlWidth = bowlSettings.Width * _uiScale;
        double bowlHeight = bowlSettings.Height * _uiScale;

        double bowlX =
            menuSurface.Left +
            (menuSurface.Right - menuSurface.Left - bowlWidth) / 2.0;

        double bowlY =
            menuSurface.Top - bowlHeight;

        var bowl = new PointOfInterest(
            "Rat Bowl",
            new Point(bowlX, bowlY),
            PointOfInterestType.Food,
            bowlSettings,
            _settings);

        bowl.AddWorldInteractionPoint(
            new WorldInteractionPoint(
                "Eat Center",
                WorldInteractionPointType.Eat,
                new Point(
                    bowlSettings.Width / 2.0,
                    bowlSettings.Height)));

        _pointOfInterestManager.Add(bowl);

        foreach (var point in bowl.AnchorPoints)
        {
            var worldPosition =
                bowl.GetWorldInteractionPointPosition(point);

            Logger.LogDebug(
                DebugCategory.Behavior,
                $"Bowl interaction: {point.Type} " +
                $"world=({worldPosition.X:F1}, {worldPosition.Y:F1})");
        }

        Logger.LogDebug(
            DebugCategory.Behavior,
            $"Added bowl at ({bowl.Position.X:F1}, {bowl.Position.Y:F1}) " +
            $"with {bowl.AnchorPoints.Count} interaction point(s).");

        var bowlWindow = new POIWindow(bowl);
        bowlWindow.Show();
    }

    private static UiButtonImages LoadButtonImages(string buttonName)
    {
        return new UiButtonImages(
            LoadUiImage($"Assets/UI/MainMenu/Buttons/button_{buttonName}.png"),
            LoadUiImage($"Assets/UI/MainMenu/Buttons/button_hover_{buttonName}.png"),
            LoadUiImage($"Assets/UI/MainMenu/Buttons/button_pressed_{buttonName}.png"));
    }

    private Rectangle LoadSettings()
    {
        _creatureSettings = CreatureSettingsLoader.Load();
        _pointOfInterestSettings = PointOfInterestSettingsLoader.Load();

        var debugSettings = DebugSettingsLoader.Load();
        Logger.Initialize(debugSettings);

        _settings = SettingsLoader.Load();

        _moniterIndex = Math.Clamp(
            _settings.WorkingMonitor,
            0,
            Forms.Screen.AllScreens.Length - 1
        );

        var screen = Forms.Screen.AllScreens[_moniterIndex];
        var area = screen.WorkingArea;

        _creaturesAlwaysOnTop = _settings.AlwaysOnTop;
        Topmost = false;

        _uiScale = _settings.Scale;

        MainCanvas.LayoutTransform = new ScaleTransform(_uiScale, _uiScale);

        Width = MainCanvas.Width * _uiScale;
        Height = MainCanvas.Height * _uiScale;

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
        WpfMouseButtonEventArgs e)
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
            ratSettings,
            _pointOfInterestManager,
            _surfaceManager);

        var ratWindow = new CreatureWindow(rat)
        {
            Owner = this,
            Topmost = _creaturesAlwaysOnTop
        };

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

    private void FieldGuide_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_fieldGuideMenu is not null)
        {
            _fieldGuideMenu.Activate();
            return;
        }

        _fieldGuideMenu = new FieldGuideMenu(
            SpawnCreature,
            _uiScale)
        {
            Owner = this
        };

        _fieldGuideMenu.Closed += (_, _) =>
        {
            _fieldGuideMenu = null;
        };

        _fieldGuideMenu.Show();
    }

    private void SpawnCreature(string creatureId)
    {
        switch (creatureId)
        {
            case "rat":
                SpawnRat();
                break;

            case "eagle":
                SpawnEagle();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(creatureId),
                    creatureId,
                    "Unknown creature.");
        }
    }

    private void ClearRats_Click(object sender, RoutedEventArgs e)
    {
        ClearRats();
        //TEMP
        ClearEagles();
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
            _pointOfInterestManager,
            area,
            _surfaceManager);

        var eagleWindow = new CreatureWindow(eagle)
        {
            Owner = this,
            Topmost = _creaturesAlwaysOnTop
        };

        eagleWindow.Show();

        _creatureWindows.Add(eagleWindow);
    }

    private void UpdateMenuSurface()
    {
        int surfaceX = (int)(Left + 111 * _uiScale);
        int surfaceY = (int)(Top + 42 * _uiScale);
        int surfaceWidth = 151 * _uiScale;

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

    private void AlwaysOnTopToggle_Click(
        object sender,
        RoutedEventArgs e)
    {
        _creaturesAlwaysOnTop = !_creaturesAlwaysOnTop;

        SetCreaturesTopmost(_creaturesAlwaysOnTop);

        AlwaysOnTopToggleImage.Source =
            GetAlwaysOnTopImages().Hover;
    }
    //private void EcosystemAlwaysOnTopToggle_Click(
    //    object sender,
    //    RoutedEventArgs e)
    //{
    //    _ecosystemAlwaysOnTop = !_ecosystemAlwaysOnTop;

    //    SetCreaturesTopmost(_ecosystemAlwaysOnTop);

    //    AlwaysOnTopToggleImage.Source =
    //        GetEcosystemAlwaysOnTopImages().Hover;
    //}
    private void UiAlwaysOnTopToggle_Click(
        object sender,
        RoutedEventArgs e)
    {
        _uiAlwaysOnTop = !_uiAlwaysOnTop;

        Topmost = _uiAlwaysOnTop;
    }

    private void SettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Owner = this;//settings window will minimoze/close with main window

            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow();

        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;
        };

        _settingsWindow.Show();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void DragArea_MouseLeftButtonDown(
        object sender, 
        WpfMouseButtonEventArgs e)
    {
        if (e.ButtonState == WpfMouseButtonState.Pressed)
        {
            DragMove();
            UpdateMenuSurface();
            _surfaceManager.Refresh();
        }
    }

    private static BitmapImage LoadUiImage(string path)
    {
        var image = new BitmapImage();

        image.BeginInit();
        image.UriSource = new Uri(
            $"pack://application:,,,/{path}",
            UriKind.Absolute);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();

        image.Freeze();

        return image;
    }

    private void FieldGuide_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideImage.Source = _fieldGuideImages.Hover;
    }

    private void FieldGuide_MouseLeave(
       object sender,
       WpfMouseEventArgs e)
    {
        FieldGuideImage.Source = _fieldGuideImages.Normal;
    }

    private void FieldGuide_MouseLeftButtonDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        FieldGuideImage.Source = _fieldGuideImages.Pressed;
        FieldGuide_Click(sender, e);
    }

    private void FieldGuide_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        FieldGuideImage.Source = _fieldGuideImages.Hover;
    }

    private void ClearRats_MouseEnter(
        object sender, 
        WpfMouseEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsImages.Hover;
    }
    private void ClearRats_MouseLeave
        (object sender, 
        WpfMouseEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsImages.Normal;
    }
    private void ClearRats_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsImages.Pressed;
    }
    private void ClearRats_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearRatsImage.Source = _clearRatsImages.Hover;
    }

    private void AlwaysOnTop_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        AlwaysOnTopToggleImage.Source = GetAlwaysOnTopImages().Hover;
    }

    private void AlwaysOnTop_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        AlwaysOnTopToggleImage.Source = GetAlwaysOnTopImages().Normal;
    }

    private void AlwaysOnTop_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        AlwaysOnTopToggleImage.Source = GetAlwaysOnTopImages().Pressed;
    }

    private void AlwaysOnTop_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        AlwaysOnTopToggleImage.Source = GetAlwaysOnTopImages().Hover;
    }

    private void Exit_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitImage.Source = _exitImages.Hover;
    }

    private void Exit_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitImage.Source = _exitImages.Normal;
    }

    private void Exit_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ExitImage.Source = _exitImages.Pressed;
    }

    private void Exit_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ExitImage.Source = _exitImages.Hover;
    }
    private void Minimize_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        MinimizeImage.Source = _minimizeImages.Hover;
    }

    private void Minimize_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        MinimizeImage.Source = _minimizeImages.Normal;
    }

    private void Minimize_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        MinimizeImage.Source = _minimizeImages.Pressed;
    }

    private void Minimize_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        MinimizeImage.Source = _minimizeImages.Hover;
    }

    private void X_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        XImage.Source = _closeImages.Hover;
    }

    private void X_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        XImage.Source = _closeImages.Normal;
    }

    private void X_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        XImage.Source = _closeImages.Pressed;
    }

    private void X_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        XImage.Source = _closeImages.Hover;
    }

    private UiButtonImages GetAlwaysOnTopImages()
    {
        return _creaturesAlwaysOnTop
            ? _alwaysOnTopOnImages
            : _alwaysOnTopOffImages;
    }
}
