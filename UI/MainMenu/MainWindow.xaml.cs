using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Graphics;
using Desktop_Creatures.Persistence;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using Desktop_Creatures.Audio;
using Desktop_Creatures.Windowing;
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

    //private double _x;
    //private double _y;

    private bool _isDragging = false;
    //private Point _dragOffset;

    private AppSettings _settings = null!;
    private SettingsWindow? _settingsWindow;

    private double _lastMainLeft;
    private double _lastMainTop;
    private bool _mainLocationInitialized;

    private FieldGuideMenu? _fieldGuideMenu;

    //private UiButtonImages _spawnRatImages = null!;
    private readonly UiButtonImages _fieldGuideImages = null!;
    private readonly UiButtonImages _clearCreaturesImages = null!;
    private readonly UiButtonImages _settingsImages = null!;
    private readonly UiButtonImages _exitImages = null!;
    private readonly UiButtonImages _minimizeImages = null!;
    private readonly UiButtonImages _closeImages = null!;

    private readonly List<POIWindow> _poiWindows = new();
    private readonly List<CreatureWindow> _creatureWindows = new();
    //private Creature _activeCreature;

    public int _moniterIndex = 0;

    private Rectangle _workingArea;

    private Dictionary<string, CreatureSettings> _creatureSettings = new();
    private Dictionary<string, PointOfInterestSettings> _pointOfInterestSettings = new();
    private Dictionary<string, CreatureDefinition> _creatureDefinitions = new();

    private List<PointOfInterest> _pointsOfInterest = new();
    private PointOfInterestManager _pointOfInterestManager;

    private readonly SurfaceManager _surfaceManager = new();

    private readonly ZOrderManager _zOrderManager = new();

    private const int MaxRats = 20;
    private const int MaxEagles = 20;
    private const int MaxOcelots = 20;

    private int _uiScale = 1;

    public MainWindow()
    {
        InitializeComponent();

        UiSounds.Initialize();

        LocationChanged += MainWindow_LocationChanged;

        Closing += (_, _) =>
        {
            SaveCreatures();
        };

        _workingArea = LoadSettings();

        _zOrderManager.Register(
            this,
            ZOrderManager.WindowLayer.MainMenu);

        _zOrderManager.SetPolicy(
            _settings.EcosystemAlwaysOnTop,
            _settings.MenusAlwaysOnTop);

        _surfaceManager.Refresh();

        MainCanvasImage.Source =
            AssetImageLoader.Load(
                "Assets/UI/MainMenu/menu_background.png");

        VersionImage.Source =
            AssetImageLoader.Load(
                "Assets/UI/MainMenu/version.png");

        _fieldGuideImages = LoadButtonImages("field_guide");
        _clearCreaturesImages = LoadButtonImages("clear_creatures");
        _settingsImages = LoadButtonImages("settings");
        _exitImages = LoadButtonImages("exit");
        _minimizeImages = LoadButtonImages("minimize");
        _closeImages = LoadButtonImages("X");

        FieldGuideImage.Source = _fieldGuideImages.Normal;
        ClearCreaturesImage.Source = _clearCreaturesImages.Normal;
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

            LoadSavedCreatures();

            ApplyTopmostSettings();
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
                _surfaceManager);

        bowlWindow.Show();

        _zOrderManager.Register(
            bowlWindow,
            ZOrderManager.WindowLayer.Ecosystem);

        _poiWindows.Add(
            bowlWindow);
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
                _surfaceManager);

        dishWindow.Show();

        _zOrderManager.Register(
            dishWindow,
            ZOrderManager.WindowLayer.Ecosystem);

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
            Screen.AllScreens.Length - 1
        );

        var screen = Screen.AllScreens[_moniterIndex];
        var area = screen.WorkingArea;

        _uiScale = _settings.Scale;

        MainCanvas.LayoutTransform = 
            new ScaleTransform(_uiScale, _uiScale);

        Width = MainCanvas.Width * _uiScale;
        Height = MainCanvas.Height * _uiScale;

        _creatureDefinitions["rat"] =
            CreatureDefinitionLoader.Load("rat");

        _creatureDefinitions["eagle"] =
            CreatureDefinitionLoader.Load("eagle");

        _creatureDefinitions["ocelot"] =
            CreatureDefinitionLoader.Load("ocelot");

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

    private void LoadSavedCreatures()
    {
        var savedCreatures =
            CreatureSaveManager.Load();

        foreach (var saveData in savedCreatures)
        {
            SpawnCreature(
                saveData.CreatureType,
                saveData);
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
            creatureId =>
                SpawnCreature(creatureId),
            _uiScale);

        _fieldGuideMenu.Closed += (_, _) =>
        {
            _surfaceManager.RemoveAppSurface(
                "field-guide");

            _fieldGuideMenu = null;
        };

        _fieldGuideMenu.Show();

        _surfaceManager.RegisterAppSurface(
            "field-guide",
            () =>
                GetElementSurface(
                    _fieldGuideMenu,
                    _fieldGuideMenu
                        .CreatureSurfaceAnchor));

        _zOrderManager.Register(
            _fieldGuideMenu,
            ZOrderManager.WindowLayer.ToolWindow);
    }

    private void SpawnCreature(
        string creatureId,
        CreatureSaveData? saveData = null)
    {
        var services =
            new CreatureServices
            {
                PointOfInterestManager =
                    _pointOfInterestManager,

                SurfaceManager =
                    _surfaceManager,

                PointsOfInterest =
                    _pointsOfInterest,

                MonitorWorkingAreas =
                    _surfaceManager.GetMonitorWorkingAreas()
            };

        CreatureDefinition definition =
            _creatureDefinitions[creatureId];

        CreatureSettings settings =
            _creatureSettings.GetValueOrDefault(
                creatureId,
                new CreatureSettings());

        CreatureSpawnContext context =
            CreateSpawnContext(
                definition,
                settings,
                saveData);

        Creature creature =
            CreatureFactory.Create(
                definition,
                context,
                settings,
                services);

        var creatureWindow =
            new CreatureWindow(
                creature,
                _surfaceManager);

        creatureWindow.DespawnRequested +=
            DespawnCreature;

        creatureWindow.SetDisplayScale(
            _settings.CreatureDisplayScale);

        creatureWindow.Show();

        _zOrderManager.Register(
            creatureWindow,
            ZOrderManager.WindowLayer.Ecosystem);

        _creatureWindows.Add(
            creatureWindow);
    }

    private void DespawnCreature(
        CreatureWindow creatureWindow)
    {
        creatureWindow.DespawnRequested -=
            DespawnCreature;

        _creatureWindows.Remove(
            creatureWindow);

        creatureWindow.Close();
    }

    private CreatureSpawnContext CreateSpawnContext(
        CreatureDefinition definition,
        CreatureSettings settings,
        CreatureSaveData? saveData)
    {
        if (saveData is not null)
        {
            return new CreatureSpawnContext
            {
                X = saveData.X,
                Y = saveData.Y,
                Id = saveData.Id,
                Name = saveData.Name,

                AppearanceTraits =
                    saveData.AppearanceId is null
                        ? saveData.AppearanceTraits
                        : null,

                AppearanceId =
                    saveData.AppearanceId
            };
        }

        return definition.MovementCapabilities.Contains(
            MovementCapability.Flight)
            ? CreateFlyingSpawnContext(
                definition,
                settings)
            : CreateGroundSpawnContext(
                definition,
                settings);
    }

    private CreatureSpawnContext CreateGroundSpawnContext(
        CreatureDefinition definition,
        CreatureSettings settings)
    {
        var menuSurface =
            _surfaceManager.MenuSurface
            ?? throw new InvalidOperationException(
                "Menu surface was not set.");

        double width =
            settings.SpriteWidth *
            settings.Scale;

        double height =
            settings.SpriteHeight *
            settings.Scale;

        return new CreatureSpawnContext
        {
            X =
                menuSurface.Left +
                (menuSurface.Width - width) / 2.0,

            Y =
                menuSurface.Top -
                height
        };
    }

    private CreatureSpawnContext CreateFlyingSpawnContext(
        CreatureDefinition definition,
        CreatureSettings settings)
    {
        var areas =
            _surfaceManager.GetMonitorWorkingAreas();

        Rectangle area =
            areas[
                Random.Shared.Next(
                    areas.Count)];

        return new CreatureSpawnContext
        {
            X =
                area.Left + 100,

            Y =
                area.Top + 300
        };
    }

    private void ClearCreatures_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        ClearCreatures();
    }

    private void ClearCreatures()
    {
        foreach (var window in _creatureWindows.ToList())
        {
            window.Close();
            _creatureWindows.Remove(window);
        }
    }

    private static Rectangle?
        GetElementSurface(
            Window window,
            FrameworkElement element)
    {
        if (!window.IsVisible ||
            window.WindowState ==
                WindowState.Minimized ||
            element.ActualWidth <= 0)
        {
            return null;
        }

        Point leftScreen =
            element.PointToScreen(
                new Point(0, 0));

        Point rightScreen =
            element.PointToScreen(
                new Point(
                    element.ActualWidth,
                    0));

        PresentationSource? source =
            PresentationSource.FromVisual(
                window);

        if (source?.CompositionTarget is null)
            return null;

        Matrix transform =
            source.CompositionTarget
                .TransformFromDevice;

        Point leftDip =
            transform.Transform(
                leftScreen);

        Point rightDip =
            transform.Transform(
                rightScreen);

        int width =
            (int)Math.Round(
                rightDip.X -
                leftDip.X);

        if (width <= 0)
            return null;

        return new Rectangle(
            (int)Math.Round(leftDip.X),
            (int)Math.Round(leftDip.Y),
            width,
            1);
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

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        System.Windows.Application.Current.Shutdown();
    }

    private void ApplyTopmostSettings()
    {
        _zOrderManager.SetPolicy(
            _settings.EcosystemAlwaysOnTop,
            _settings.MenusAlwaysOnTop);
    }

    private void SettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

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
                Owner = this
            };

        _settingsWindow.ScaleChanged +=
            OnCreatureDisplayScaleChanged;

        _settingsWindow.EcosystemAlwaysOnTopChanged +=
            OnEcosystemAlwaysOnTopChanged;

        _settingsWindow.MenusAlwaysOnTopChanged +=
            OnMenusAlwaysOnTopChanged;

        _settingsWindow.Left =
            Left + (108 * _uiScale);

        _settingsWindow.Top =
            Top + (12 * _uiScale);

        _settingsWindow.Closed += (_, _) =>
        {
            _surfaceManager.RemoveAppSurface(
                "settings");

            _settingsWindow = null;
        };

        _settingsWindow.Show();

        _surfaceManager.RegisterAppSurface(
            "settings",
            () =>
                GetElementSurface(
                    _settingsWindow,
                    _settingsWindow
                        .CreatureSurfaceAnchor));

        _zOrderManager.Register(
            _settingsWindow,
            ZOrderManager.WindowLayer.ToolWindow);
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

    private void OnEcosystemAlwaysOnTopChanged(
        bool isAlwaysOnTop)
    {
        ApplyTopmostSettings();
    }

    private void OnMenusAlwaysOnTopChanged(
        bool isAlwaysOnTop)
    {
        ApplyTopmostSettings();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

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

    private void ClearCreatures_MouseEnter(
        object sender, 
        WpfMouseEventArgs e)
    {
        ClearCreaturesImage.Source = _clearCreaturesImages.Hover;
    }
    private void ClearCreatures_MouseLeave
        (object sender, 
        WpfMouseEventArgs e)
    {
        ClearCreaturesImage.Source = _clearCreaturesImages.Normal;
    }
    private void ClearCreatures_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearCreaturesImage.Source = _clearCreaturesImages.Pressed;
    }
    private void ClearCreatures_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearCreaturesImage.Source = _clearCreaturesImages.Hover;
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