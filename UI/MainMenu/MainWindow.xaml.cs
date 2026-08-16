using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Graphics;
using Desktop_Creatures.Persistence;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Point = System.Windows.Point;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

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

    private double _lastMainLeft;
    private double _lastMainTop;
    private bool _mainLocationInitialized;

    private FieldGuideMenu? _fieldGuideMenu;

    private UiButtonImages _spawnRatImages = null!;
    private UiButtonImages _fieldGuideImages = null!;
    private UiButtonImages _clearRatsImages = null!; 
    private UiButtonImages _settingsImages = null!;
    private UiButtonImages _exitImages = null!;
    private UiButtonImages _minimizeImages = null!;
    private UiButtonImages _closeImages = null!;

    private readonly List<POIWindow> _poiWindows = new();
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
    private const int MaxEagles = 10;

    private int _uiScale = 1;

    public MainWindow()
    {
        InitializeComponent();

        LocationChanged += MainWindow_LocationChanged;

        Closing += (_, _) =>
        {
            SaveCreatures();
        };

        _workingArea = LoadSettings();

        _surfaceManager.Refresh();

        MainCanvasImage.Source =
            AssetImageLoader.Load(
                "Assets/UI/MainMenu/menu_background.png");

        VersionImage.Source =
            AssetImageLoader.Load(
                "Assets/UI/MainMenu/version.png");

        _fieldGuideImages = LoadButtonImages("field_guide");
        _clearRatsImages = LoadButtonImages("clear_rats");
        _settingsImages = LoadButtonImages("settings");
        _exitImages = LoadButtonImages("exit");
        _minimizeImages = LoadButtonImages("minimize");
        _closeImages = LoadButtonImages("X");

        FieldGuideImage.Source = _fieldGuideImages.Normal;
        ClearRatsImage.Source = _clearRatsImages.Normal;
        SettingsButtonImage.Source = _settingsImages.Normal;
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

            // TODO: Re-enable when POIs are ready for release.
            //CreateFoodBowl(); 
            //CreateWaterDish();

            bool loadedCreatures =
                LoadSavedCreatures();

            if (!loadedCreatures)
            {
                SpawnRat();
            }

            ApplyTopmostSettings();

            ApplyTopmostSettings();
        };

        Activated += (_, _) =>
        {
            KeepCreaturesAboveMainMenu();
        };

        PreviewMouseDown += (_, _) =>
        {
            KeepCreaturesAboveMainMenu();
        };
    }

    private void MainWindow_LocationChanged(
        object? sender,
        EventArgs e)
    {
        if (!_mainLocationInitialized)
        {
            _lastMainLeft = Left;
            _lastMainTop = Top;
            _mainLocationInitialized = true;
            return;
        }

        double deltaX =
            Left - _lastMainLeft;

        double deltaY =
            Top - _lastMainTop;

        _lastMainLeft = Left;
        _lastMainTop = Top;

        if (_settingsWindow is not null)
        {
            _settingsWindow.Left += deltaX;
            _settingsWindow.Top += deltaY;
        }
    }

    private void CreateFoodBowl()
    {
        if (!_pointOfInterestSettings.TryGetValue(
            "food_bowl",
            out var bowlSettings))
        {
            System.Windows.MessageBox.Show("food_bowl settings not found!");
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
            "Food Bowl",
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

        var bowlWindow =
            new POIWindow(
                bowl,
                _surfaceManager)
            {
            Topmost =
                _settings.EcosystemAlwaysOnTop ||
                _settings.MenusAlwaysOnTop
        };

        bowlWindow.Show();
        _poiWindows.Add(bowlWindow);
    }

    private void CreateWaterDish()
    {
        if (!_pointOfInterestSettings.TryGetValue(
            "water_dish",
            out var dishSettings))
        {
            System.Windows.MessageBox.Show(
                "water_dish settings not found!");

            return;
        }

        var menuSurface =
            _surfaceManager.MenuSurface
            ?? throw new InvalidOperationException(
                "Menu surface must exist before creating the water dish.");

        double dishWidth =
            dishSettings.Width * _uiScale;

        double dishHeight =
            dishSettings.Height * _uiScale;

        // Put it slightly left of the food bowl for now.
        double dishX =
            menuSurface.Left +
            (menuSurface.Width * 0.25) -
            (dishWidth / 2.0);

        double dishY =
            menuSurface.Top -
            dishHeight;

        var dish =
            new PointOfInterest(
                "Water Dish",
                new Point(
                    dishX,
                    dishY),
                PointOfInterestType.Water,
                dishSettings,
                _settings);

        dish.AddWorldInteractionPoint(
            new WorldInteractionPoint(
                "Drink Center",
                WorldInteractionPointType.Drink,
                new Point(
                    dishSettings.Width / 2.0,
                    dishSettings.Height)));

        _pointOfInterestManager.Add(
            dish);

        var dishWindow =
            new POIWindow(
                dish,
                _surfaceManager)
            {
                Topmost =
                    _settings.EcosystemAlwaysOnTop ||
                    _settings.MenusAlwaysOnTop
            };

        dishWindow.Show();

        _poiWindows.Add(
            dishWindow);
    }

    private static UiButtonImages LoadButtonImages(string buttonName)
    {
        return new UiButtonImages(
            AssetImageLoader.Load($"Assets/UI/MainMenu/Buttons/button_{buttonName}.png"),
            AssetImageLoader.Load($"Assets/UI/MainMenu/Buttons/button_hover_{buttonName}.png"),
            AssetImageLoader.Load($"Assets/UI/MainMenu/Buttons/button_pressed_{buttonName}.png"));
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

        _creaturesAlwaysOnTop = 
            _settings.EcosystemAlwaysOnTop;

        Topmost =
            _settings.EcosystemAlwaysOnTop;

        _uiScale = _settings.Scale;

        MainCanvas.LayoutTransform = 
            new ScaleTransform(_uiScale, _uiScale);

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

    private void SpawnRat(
        CreatureSaveData? saveData = null)
    {
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

        double ratWidth =
            ratSettings.SpriteWidth *
            ratSettings.Scale;

        double ratHeight =
            ratSettings.SpriteHeight *
            ratSettings.Scale;

        double spawnX;
        double spawnY;

        if (saveData is not null)
        {
            spawnX = saveData.X;
            spawnY = saveData.Y;
        }
        else
        {
            spawnX =
                menuSurface.Left +
                (menuSurface.Right -
                 menuSurface.Left -
                 ratWidth) / 2.0;

            spawnY =
                menuSurface.Top -
                ratHeight;
        }

        var rat = new Rat(
            spawnX,
            spawnY,
            ratSettings,
            _pointOfInterestManager,
            _surfaceManager,
            id: saveData?.Id,
            name: saveData?.Name,
            appearanceTraits:
                saveData?.AppearanceId is null
                    ? saveData?.AppearanceTraits
                    : null,
            appearanceId:
                saveData?.AppearanceId);

        var ratWindow =
            new CreatureWindow(
                rat,
                _surfaceManager)
            {
            Topmost = _creaturesAlwaysOnTop
            };

        ratWindow.SetDisplayScale(_settings.CreatureDisplayScale);
        ratWindow.Show();

        _creatureWindows.Add(ratWindow);
    }

    private void SaveCreatures()
    {
        var saveData =
            _creatureWindows
                .Select(window =>
                    window.GetCreature())
                .OfType<Rat>()
                .Select(rat =>
                    new CreatureSaveData
                    {
                        Id = rat.Id,
                        CreatureType = "rat",
                        Name = rat.Name,

                        AppearanceId =
                            rat.AppearanceId,

                        AppearanceTraits =
                            rat.AppearanceTraits,

                        X = rat.X,
                        Y = rat.Y
                    })
                .ToList();

        CreatureSaveManager.Save(
            saveData);
    }

    private bool LoadSavedCreatures()
    {
        var savedCreatures =
            CreatureSaveManager.Load();

        bool loadedAny = false;

        foreach (var saveData in savedCreatures)
        {
            switch (saveData.CreatureType.ToLowerInvariant())
            {
                case "rat":
                    SpawnRat(saveData);
                    loadedAny = true;
                    break;
            }
        }

        return loadedAny;
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
            _uiScale);

        _fieldGuideMenu.Topmost =
            _settings.EcosystemAlwaysOnTop;

        _fieldGuideMenu.Closed += (_, _) =>
        {
            _fieldGuideMenu = null;
        };

        _fieldGuideMenu.Activated += (_, _) =>
        {
            KeepCreaturesAboveMainMenu();
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

        int eagleCount = _creatureWindows.Count(w =>
            w.GetCreature() is Eagle);

        if (eagleCount >= MaxEagles)
        {
            System.Windows.MessageBox.Show(
                $"Maximum eagle count reached: {MaxEagles}",
                "Too many eagles!");
            return;
        }

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

        var eagleWindow =
            new CreatureWindow(
                eagle,
                _surfaceManager)
            {
            Topmost = _creaturesAlwaysOnTop
        };

        eagleWindow.SetDisplayScale(_settings.CreatureDisplayScale);
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
            creatureWindow.RefreshTopmost(isTopmost);
    }

    private void AlwaysOnTopToggle_Click(
        object sender,
        RoutedEventArgs e)
    {
        _creaturesAlwaysOnTop = !_creaturesAlwaysOnTop;

        SetCreaturesTopmost(_creaturesAlwaysOnTop);

    }

    private void ApplyTopmostSettings()
    {
        bool isAlwaysOnTop =
            _settings.EcosystemAlwaysOnTop;

        Topmost =
            isAlwaysOnTop;

        if (_fieldGuideMenu is not null)
        {
            _fieldGuideMenu.Topmost =
                isAlwaysOnTop;
        }

        if (_settingsWindow is not null)
        {
            _settingsWindow.Topmost =
                isAlwaysOnTop;
        }

        SetCreaturesTopmost(
            isAlwaysOnTop);

        foreach (var poiWindow in _poiWindows)
        {
            poiWindow.Topmost =
                isAlwaysOnTop;
        }
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
            _settingsWindow.Close();
            return;
        }

        _settingsWindow =
            new SettingsWindow(
                _settings,
                _uiScale)
            {
                Owner = this,
                Topmost =
                    _settings.EcosystemAlwaysOnTop
            };

        _settingsWindow.ScaleChanged +=
            OnCreatureDisplayScaleChanged;

        _settingsWindow.EcosystemAlwaysOnTopChanged +=
            OnEcosystemAlwaysOnTopChanged;

        _settingsWindow.Left =
            Left + (108 * _uiScale);

        _settingsWindow.Top =
            Top + (12 * _uiScale);

        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;

            KeepCreaturesAboveMainMenu();
        };

        _settingsWindow.Activated += (_, _) =>
        {
            KeepCreaturesAboveMainMenu();
        };

        _settingsWindow.Show();

        KeepCreaturesAboveMainMenu();
    }

    private void OnCreatureDisplayScaleChanged(
        int scale)
    {
        foreach (var window in _creatureWindows)
        {
            window.SetDisplayScale(
                scale);
        }
    }

    private void KeepCreaturesAboveMainMenu()
    {
        if (!_settings.EcosystemAlwaysOnTop)
            return;

        Dispatcher.BeginInvoke(() =>
        {
            foreach (var creatureWindow in _creatureWindows)
            {
                creatureWindow.RefreshTopmost(true);
            }

            if (_fieldGuideMenu is not null)
            {
                _fieldGuideMenu.Topmost = false;
                _fieldGuideMenu.Topmost = true;
            }

            if (_settingsWindow is not null)
            {
                _settingsWindow.Topmost = false;
                _settingsWindow.Topmost = true;
            }
        });
    }

    private void OnEcosystemAlwaysOnTopChanged(
        bool isAlwaysOnTop)
    {
        _creaturesAlwaysOnTop =
            isAlwaysOnTop;

        ApplyTopmostSettings();
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

            KeepCreaturesAboveMainMenu();
        }
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

    private void SettingsButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SettingsButtonImage.Source =
            _settingsImages.Hover;
    }

    private void SettingsButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SettingsButtonImage.Source =
            _settingsImages.Normal;
    }

    private void SettingsButton_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        SettingsButtonImage.Source =
            _settingsImages.Pressed;
    }

    private void SettingsButton_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        SettingsButtonImage.Source =
            _settingsImages.Hover;
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
}
