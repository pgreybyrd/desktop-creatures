using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Creatures.Definitions;
using Desktop_Creatures.Ecosystem;
using Desktop_Creatures.Ecosystem.Interaction;
using Desktop_Creatures.Ecosystem.Rendering;
using Desktop_Creatures.Graphics;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.Persistence;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.UI.CreatureRoster;
using Desktop_Creatures.UI.RightClick;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.Windowing;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures;


// TODO: Ecosystem layering:
// Creatures standing on app/menu surfaces must render above the
// supporting surface. Resolve as part of unified ecosystem/UI
// z-order architecture rather than special-casing MainWindow.
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;

    private readonly Stopwatch _simulationClock = Stopwatch.StartNew();
    private double _lastSimulationSeconds;

    private bool _isDragging = false;

    public int _moniterIndex = 0;

    private Rectangle _workingArea;

    private AppSettings _settings = null!;
    private SettingsWindow? _settingsWindow;

    private double _lastMainLeft;
    private double _lastMainTop;
    private bool _mainLocationInitialized;

    private FieldGuideMenu? _fieldGuideMenu;

    private readonly SpriteSheet _mainMenuButtonSheet;
    private readonly SpriteSheet _mainMenuLabelSheet;

    private readonly BitmapSource _fieldGuideNormal;
    private readonly BitmapSource _fieldGuideHover;
    private readonly BitmapSource _fieldGuidePressed;
    private readonly BitmapSource _fieldGuideLabel;

    private readonly BitmapSource _rosterNormal;
    private readonly BitmapSource _rosterHover;
    private readonly BitmapSource _rosterPressed;
    private readonly BitmapSource _rosterLabel;

    private readonly BitmapSource _shopNormal;
    private readonly BitmapSource _shopHover;
    private readonly BitmapSource _shopPressed;
    private readonly BitmapSource _shopLabel;

    private readonly BitmapSource _clearDesktopNormal;
    private readonly BitmapSource _clearDesktopHover;
    private readonly BitmapSource _clearDesktopPressed;
    private readonly BitmapSource _clearDesktopLabel;

    private readonly BitmapSource _settingsNormal;
    private readonly BitmapSource _settingsHover;
    private readonly BitmapSource _settingsPressed;
    private readonly BitmapSource _settingsLabel;

    private readonly BitmapSource _quitNormal;
    private readonly BitmapSource _quitHover;
    private readonly BitmapSource _quitPressed;
    private readonly BitmapSource _quitLabel;

    //private readonly List<POIWindow> _poiWindows = new();

    private readonly CreatureManager _creatureManager = new();

    private CreatureRosterWindow? _creatureRosterWindow;

    private readonly Dictionary<Guid, CreatureRecord> _creatureRecords = [];
    private Dictionary<string, CreatureDefinition> _creatureDefinitions = [];

    private Dictionary<string, CreatureSettings> _creatureSettings = [];
    private Dictionary<string, PointOfInterestSettings> _pointOfInterestSettings = [];

    private List<PointOfInterest> _pointsOfInterest = [];

    private PointOfInterestManager _pointOfInterestManager = new();

    private readonly SurfaceManager _surfaceManager = new();
    private readonly ZOrderManager _zOrderManager = new();

    private readonly CreatureDragController _creatureDragController;
    private readonly PointOfInterestDragController _pointOfInterestDragController;

    private readonly EcosystemHost _ecosystemHost;
    private readonly EcosystemRenderer _ecosystemRenderer;

    private readonly EcosystemInputRouter _ecosystemInputRouter;

    private readonly CreatureContextMenuController _creatureContextMenuController = new();

    private const int MenuTitleBarTop = 57;
    private const int MenuTitleBarLeft = 108;
    private const int MenuTitleBarWidth = 151;

    private int _uiScale = 1;

    public MainWindow()
    {
        InitializeComponent();

        _creatureDragController =
            new CreatureDragController(
                _surfaceManager);

        _pointOfInterestDragController =
            new PointOfInterestDragController(
                _surfaceManager);

        _ecosystemHost =
            new EcosystemHost(
                _creatureManager,
                _surfaceManager);

        _ecosystemRenderer =
            new EcosystemRenderer(
                _surfaceManager,
                _creatureManager,
                _pointOfInterestManager,
                _zOrderManager);

        _ecosystemInputRouter =
            new EcosystemInputRouter(
                () => _ecosystemRenderer.RenderItems,
                _creatureManager.FindCreature,
                _pointOfInterestManager.FindPointOfInterest);

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
                "Assets/UI/MainMenu/main_menu.png");

        VersionImage.Source =
            AssetImageLoader.Load(
                "Assets/UI/MainMenu/version.png");

        _mainMenuButtonSheet =
            SpriteSheetLoader.Load(
                "Assets/UI/MainMenu/buttons.png",
                "Assets/UI/MainMenu/buttons.json");

        _mainMenuLabelSheet =
            SpriteSheetLoader.Load(
                "Assets/UI/MainMenu/labels.png",
                "Assets/UI/MainMenu/labels.json");

        //Field Guide
        _fieldGuideNormal =
            _mainMenuButtonSheet
                .GetFrame("fieldguide_normal")
                .Image;

        _fieldGuideHover =
            _mainMenuButtonSheet
                .GetFrame("fieldguide_hover")
                .Image;

        _fieldGuidePressed =
            _mainMenuButtonSheet
                .GetFrame("fieldguide_pressed")
                .Image;

        _fieldGuideLabel =
            _mainMenuLabelSheet
                .GetFrame("fieldguide")
                .Image;

        FieldGuideImage.Source = _fieldGuideNormal;
        FieldGuideLabelImage.Source = _fieldGuideLabel;

        //Roster
        _rosterNormal =
            _mainMenuButtonSheet
                .GetFrame("roster_normal")
                .Image;

        _rosterHover =
            _mainMenuButtonSheet
                .GetFrame("roster_hover")
                .Image;

        _rosterPressed =
            _mainMenuButtonSheet
                .GetFrame("roster_pressed")
                .Image;

        _rosterLabel =
            _mainMenuLabelSheet
                .GetFrame("roster")
                .Image;

        RosterImage.Source = _rosterNormal;
        RosterLabelImage.Source = _rosterLabel;

        //Shop
        _shopNormal =
            _mainMenuButtonSheet
                .GetFrame("shop_normal")
                .Image;
        _shopHover = 
            _mainMenuButtonSheet
                .GetFrame("shop_hover")
                .Image; 
        _shopPressed = 
            _mainMenuButtonSheet
                .GetFrame("shop_pressed")
                .Image; 
        _shopLabel =
            _mainMenuLabelSheet
                .GetFrame("shop")
                .Image; 

        ShopImage.Source = _shopNormal;
        ShopLabelImage.Source = _shopLabel;

        //Clear Desktop
        _clearDesktopNormal =
            _mainMenuButtonSheet
                .GetFrame("clear_normal")
                .Image;

        _clearDesktopHover =
            _mainMenuButtonSheet
                .GetFrame("clear_hover")
                .Image;

        _clearDesktopPressed =
            _mainMenuButtonSheet
                .GetFrame("clear_pressed")
                .Image;

        _clearDesktopLabel =
            _mainMenuLabelSheet
                .GetFrame("clear")
                .Image;

        ClearDesktopImage.Source = _clearDesktopNormal;
        ClearDesktopLabelImage.Source = _clearDesktopLabel;

        //Settings
        _settingsNormal =
            _mainMenuButtonSheet
                .GetFrame("settings_normal")
                .Image;

        _settingsHover =
            _mainMenuButtonSheet
                .GetFrame("settings_hover")
                .Image;

        _settingsPressed =
            _mainMenuButtonSheet
                .GetFrame("settings_pressed")
                .Image;

        _settingsLabel =
            _mainMenuLabelSheet
                .GetFrame("settings")
                .Image;

        SettingsImage.Source = _settingsNormal;
        SettingsLabelImage.Source = _settingsLabel;

        //Quit
        _quitNormal =
            _mainMenuButtonSheet
                .GetFrame("quit_normal")
                .Image;

        _quitHover =
            _mainMenuButtonSheet
                .GetFrame("quit_hover")
                .Image;

        _quitPressed =
            _mainMenuButtonSheet
                .GetFrame("quit_pressed")
                .Image;

        _quitLabel =
            _mainMenuLabelSheet
                .GetFrame("quit")
                .Image;

        QuitImage.Source = _quitNormal;
        QuitLabelImage.Source = _quitLabel;

        //===== End of Images =====

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

            _ecosystemRenderer.CreateSurfaces(
                _ecosystemInputRouter,
                _creatureDragController,
                _pointOfInterestDragController,
                _creatureContextMenuController,
                _uiScale,
                HandleCreatureContextAction,
                PutAwayCreature);

            //CreateInitialFlowers();

            //CreateInitialTree();

            // TODO: Re-enable when POIs are ready for release.
            //CreateFoodBowl(); 
            //CreateWaterDish();
            CreateTestPoi();

            LoadSavedCreatures();

            ApplyTopmostSettings();
        };
    }

    private void CreateTestPoi()
    {
        PointOfInterest poi =
            new(
                "TEST TREE",
                new Point(500, 300),
                PointOfInterestType.Tree,
                _pointOfInterestSettings["tree"],
                _settings
                );

        _pointOfInterestManager.Add(
            poi);
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

        if (_creatureRosterWindow is not null)
        {
            _creatureRosterWindow.Left +=
                deltaX;

            _creatureRosterWindow.Top +=
                deltaY;
        }
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

        _creatureDefinitions =
            CreatureDefinitionLoader.LoadAll();

        return area;
    }

    private void Update(object? sender, EventArgs e)
    {
        double nowSeconds =
            _simulationClock.Elapsed.TotalSeconds;

        double deltaSeconds =
            nowSeconds - _lastSimulationSeconds;

        _lastSimulationSeconds =
            nowSeconds;

        if (_isDragging)
            return;

        deltaSeconds =
            Math.Min(
                deltaSeconds,
                0.1);

        _ecosystemHost.Update(
            deltaSeconds,
            creature =>
            {
                if (_creatureDragController.DraggedCreatureId ==
                    creature.Id)
                {
                    creature.UpdateHeldAnimation();
                    return false;
                }

                return true;
            });

        _ecosystemRenderer.Render();
    }

    private void TitleBar_MouseLeftButtonDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        DragMove();
    }

    private void SaveCreatures()
    {
        foreach (Creature creature in
                 _creatureManager.ActiveCreatures)
        {
            UpdateCreatureRecord(
                creature);
        }

        SaveCreatureRecords();
    }

    private void UpdateCreatureRecord(
        Creature creature)
    {
        if (!_creatureRecords.TryGetValue(
                creature.Id,
                out CreatureRecord? record))
        {
            return;
        }

        record.Name =
            creature.Name;

        record.LastX =
            creature.X;

        record.LastY =
            creature.Y;

        record.AppearanceId =
            creature.AppearanceId;

        record.AppearanceTraits =
            creature.AppearanceTraits;
    }

    private void LoadSavedCreatures()
    {
        CreatureSaveFile saveFile =
            CreatureSaveManager.Load();

        _creatureRecords.Clear();

        foreach (CreatureRecord record in
                 saveFile.Creatures)
        {
            _creatureRecords[record.Id] =
                record;

            if (record.IsSpawned)
            {
                SpawnCreature(
                    record.CreatureType,
                    record.AppearanceId,
                    record);
            }
        }
    }

    private async void FieldGuide_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        if (_fieldGuideMenu is not null)
        {
            await _fieldGuideMenu
                .CloseWithAnimationAsync();

            return;
        }

        OpenFieldGuide();
    }

    private void OpenFieldGuideToCreature(
        string creatureId)
    {
        OpenFieldGuide();

        _fieldGuideMenu?.NavigateToCreature(
            creatureId);
    }

    private void OpenFieldGuide()
    {
        if (_fieldGuideMenu is not null)
        {
            _fieldGuideMenu.Activate();
            return;
        }

        _fieldGuideMenu =
            new FieldGuideMenu(
                (creatureId, appearanceId) =>
                    SpawnCreature(creatureId, appearanceId),
                _uiScale);

        _fieldGuideMenu.Left =
            Left +
            ((Width -
              _fieldGuideMenu.Width) / 2.0);

        _fieldGuideMenu.Top =
            Top +
            (20 * _uiScale);

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
        string? appearanceId = null,
        CreatureRecord? record = null)
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
                appearanceId,
                record);

        Creature creature =
            CreatureFactory.Create(
                definition,
                context,
                settings,
                services);

        creature.SetDisplayScale(_uiScale);

        _creatureManager.Add(creature);

        if (record is null)
        {
            record =
                new CreatureRecord
                {
                    Id = creature.Id,
                    CreatureType = creatureId,
                    Name = creature.Name,
                    AppearanceId = creature.AppearanceId,
                    AppearanceTraits = creature.AppearanceTraits,
                    LastX = creature.X,
                    LastY = creature.Y
                };

            _creatureRecords[record.Id] =
                record;

            SaveCreatureRecords();

            _creatureRosterWindow?.Refresh();
        }
    }

    private void HandleCreatureContextAction(
        Creature creature,
        CreatureContextMenuAction action)
    {
        switch (action)
        {
            case CreatureContextMenuAction.FieldGuide:
                OpenFieldGuideToCreature(
                    creature.CreatureType);
                break;
        }
    }

    private bool IsCreatureSpawned(
        Guid creatureId)
    {
        return _creatureManager.IsActive(
            creatureId);
    }

    private void SpawnCreatureRecord(
        Guid creatureId)
    {
        if (!_creatureRecords.TryGetValue(
                creatureId,
                out CreatureRecord? record))
        {
            return;
        }

        if (IsCreatureSpawned(creatureId))
            return;

        SpawnCreature(
            record.CreatureType,
            record.AppearanceId,
            record);

        record.IsSpawned = true;

        SaveCreatureRecords();
    }

    private void PutAwayCreature(
        Guid creatureId)
    {
        Creature? creature =
            _creatureManager.FindCreature(
                creatureId);

        if (creature is null)
            return;

        UpdateCreatureRecord(
            creature);

        if (_creatureRecords.TryGetValue(
            creature.Id,
            out CreatureRecord? record))
        {
            record.IsSpawned = false;
        }

        _creatureManager.Remove(
            creature.Id);

        SaveCreatureRecords();

        _creatureRosterWindow?.Refresh();
    }

    private void SetCreatureFavorite(
        Guid creatureId,
        bool isFavorite)
    {
        if (!_creatureRecords.TryGetValue(
                creatureId,
                out CreatureRecord? record))
        {
            return;
        }

        record.IsFavorite =
            isFavorite;

        SaveCreatureRecords();
    }

    private void OpenCreatureRoster()
    {
        if (_creatureRosterWindow is not null)
        {
            _creatureRosterWindow.Activate();
            return;
        }

        _creatureRosterWindow =
            new CreatureRosterWindow(
                () => _creatureRecords.Values.ToList(),
                _uiScale,
                IsCreatureSpawned,
                SpawnCreatureRecord,
                PutAwayCreature,
                SetCreatureFavorite)
            {
                Left =
                Left +
                Width +
                (-3 * _uiScale),

                Top =
                Top +
                (18 * _uiScale)
            };

        _creatureRosterWindow.Closed +=
            (_, _) =>
            {
                _creatureRosterWindow = null;
            };

        _creatureRosterWindow.Show();

        _zOrderManager.Register(
            _creatureRosterWindow,
            ZOrderManager.WindowLayer.ToolWindow);
    }

    private void SaveCreatureRecords()
    {
        CreatureSaveManager.Save(
            _creatureRecords.Values);
    }

    private CreatureSpawnContext CreateSpawnContext(
        CreatureDefinition definition,
        string? appearanceId,
        CreatureRecord? record)
    {
        if (record is not null)
        {
            return new CreatureSpawnContext
            {
                X = record.LastX,
                Y = record.LastY,

                Id = record.Id,
                Name = record.Name,

                AppearanceTraits =
                    record.AppearanceId is null
                        ? record.AppearanceTraits
                        : null,

                AppearanceId =
                    record.AppearanceId
            };
        }

        CreatureSpawnContext context =
            definition.Movement.Flight is not null
                ? CreateFlyingSpawnContext()
                : CreateGroundSpawnContext(
                    definition);

        return context with
        {
            AppearanceId = appearanceId
        };
    }

    private CreatureSpawnContext CreateGroundSpawnContext(
        CreatureDefinition definition)
    {
        var menuSurface =
            _surfaceManager.MenuSurface
            ?? throw new InvalidOperationException(
                "Menu surface was not set.");

        double width =
            definition.Visuals.SpriteWidth *
            definition.Visuals.Scale.Default;

        double height =
            definition.Visuals.SpriteHeight *
            definition.Visuals.Scale.Default;

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

    private CreatureSpawnContext CreateFlyingSpawnContext()
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

    private void ClearDesktop()
    {
        PutAwayAllCreatures();
        ClearActivePois();
    }

    private void PutAwayAllCreatures()
    {
        foreach (Creature creature in
                 _creatureManager.ActiveCreatures.ToList())
        {
            PutAwayCreature(
                creature.Id);
        }
    }

    private void ResetAllCreatures()
    {
        foreach (Creature creature in
                 _creatureManager.ActiveCreatures.ToList())
        {
            _creatureManager.Remove(
                creature.Id);
        }

        _creatureRecords.Clear();

        SaveCreatureRecords();

        _creatureRosterWindow?.Refresh();

        MessageBox.Show(
            "All saved creatures have been reset.",
            "Reset Complete",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ClearActivePois()
    {
        _pointOfInterestManager.Clear();
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
        int surfaceX = (int)(Left + MenuTitleBarLeft * _uiScale);
        int surfaceY = (int)(Top + MenuTitleBarTop * _uiScale);
        int surfaceWidth = MenuTitleBarWidth * _uiScale;

        _surfaceManager.SetMenuSurface(
            new Rectangle(
                surfaceX,
                surfaceY,
                surfaceWidth,
                1));
    }

    private void Quit_Click(object sender, RoutedEventArgs e)
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

        _settingsWindow.ResetCreaturesRequested +=
            ResetAllCreatures;

        _settingsWindow.EcosystemAlwaysOnTopChanged +=
            OnEcosystemAlwaysOnTopChanged;

        _settingsWindow.MenusAlwaysOnTopChanged +=
            OnMenusAlwaysOnTopChanged;

        _settingsWindow.Left =
            Left -
            _settingsWindow.Width -
            (-20 * _uiScale);

        _settingsWindow.Top =
            Top +
            (12 * _uiScale);

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
        foreach (Creature creature in
                 _creatureManager.ActiveCreatures)
        {
            creature.SetDisplayScale(
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
        FieldGuideImage.Source =
            _fieldGuideHover;
    }
    private void FieldGuide_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideImage.Source =
            _fieldGuideNormal;
    }
    private void FieldGuide_MouseLeftButtonDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        FieldGuideImage.Source =
            _fieldGuidePressed;
    }
    private void FieldGuide_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        FieldGuideImage.Source =
            _fieldGuideHover;
    }

    private void Roster_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        if (_creatureRosterWindow is not null)
        {
            _creatureRosterWindow.Close();
            return;
        }

        OpenCreatureRoster();
    }
    private void Roster_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        RosterImage.Source =
            _rosterHover;
    }
    private void Roster_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        RosterImage.Source =
            _rosterNormal;
    }
    private void Roster_MouseLeftButtonDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        RosterImage.Source =
            _rosterPressed;
    }
    private void Roster_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        RosterImage.Source =
            _rosterHover;
    }

    private void Shop_Click(
       object sender,
       RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();
        //OpenCreatureRoster();
    }
    private void Shop_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        ShopImage.Source =
            _shopHover;
    }
    private void Shop_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        ShopImage.Source =
            _shopNormal;
    }
    private void Shop_MouseLeftButtonDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ShopImage.Source =
            _shopPressed;
    }
    private void Shop_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ShopImage.Source =
            _shopHover;
    }

    private void ClearDesktop_MouseEnter(
        object sender, 
        WpfMouseEventArgs e)
    {
        ClearDesktopImage.Source = _clearDesktopHover;
    }
    private void ClearDesktop_MouseLeave
        (object sender, 
        WpfMouseEventArgs e)
    {
        ClearDesktopImage.Source = _clearDesktopNormal;
    }
    private void ClearDesktop_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearDesktopImage.Source = _clearDesktopPressed;
    }
    private void ClearDesktop_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        ClearDesktopImage.Source = _clearDesktopHover;
    }

    private void ClearDesktop_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        ClearDesktop();
    }

    private void SettingsButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SettingsImage.Source =
            _settingsHover;
    }

    private void SettingsButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SettingsImage.Source =
            _settingsNormal;
    }

    private void SettingsButton_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        SettingsImage.Source =
            _settingsPressed;
    }

    private void SettingsButton_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        SettingsImage.Source =
            _settingsHover;
    }

    private void Quit_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        QuitImage.Source = _quitHover;
    }

    private void Quit_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        QuitImage.Source = _quitNormal;
    }

    private void Quit_MouseDown(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        QuitImage.Source = _quitPressed;
    }

    private void Quit_MouseUp(
        object sender,
        WpfMouseButtonEventArgs e)
    {
        QuitImage.Source = _quitHover;
    }
}