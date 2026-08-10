using Desktop_Creatures.Assets.UI;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.UI.FieldGuide;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;
using WpfButton = System.Windows.Controls.Button;
using WpfImage = System.Windows.Controls.Image;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures;

public enum FieldGuideTab
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    LightGrey,
    PaleBlue,
    Orange,
    LightBlue,
    DarkGreen,
    Pink,
    DarkGrey
}

public enum ButtonState
{
    Normal,
    Hover,
    Pressed
}

public partial class FieldGuideMenu : Window
{
    private const string FieldGuideAssetPath =
        "Assets/UI/FieldGuide";

    private readonly Action _spawnRat;

    private readonly int _uiScale;
    private readonly int _titleScale;
    private readonly int _bookScale;

    private readonly BitmapImage[] _openingFrames;
    private readonly BitmapImage _bookBase;
    private readonly BitmapImage[] _pageTurnFrames;

    private readonly TabSpriteSheet _tabSpriteSheet;
    private const int TabWidth = 7;
    private const int TabHeight = 8;

    private bool _isPageTurning;
    private FieldGuideTab _currentTab = FieldGuideTab.Red;

    //creature pages
    private readonly BitmapImage _spawnButton;
    private readonly BitmapImage _spawnButtonPressed;
    private readonly BitmapImage _spawnButtonHover;

    private readonly Dictionary<string, FieldGuideEntry> _creatureEntries;

    private string? _currentCreatureId;

    private bool _isOpening;

    public readonly record struct TabTurnPose(
        double X,
        double Y,
        bool IsMirrored);

    private readonly TabTurnPose[] _redTabTurnPath =
    [
        new(181, 40, false), // frame 0
        new(176, 38, false), // frame 1
        new(169, 34, false), // frame 2
        new(158, 28, false), // frame 3
        new(144, 21, false), // frame 4
        new(120, 16, false), // frame 5

        new(87,  16, true),  // frame 6: page crosses center
        new(58,  23, true),  // frame 7
        new(35,  31, true),  // frame 8
        new(20,  38, true),  // frame 9
        new(13,  40, true)   // frame 10
    ];

    private static void ApplyTabPose(
        WpfImage tabImage,
        TabTurnPose pose)
    {
        Canvas.SetLeft(tabImage, pose.X);
        Canvas.SetTop(tabImage, pose.Y);

        tabImage.RenderTransformOrigin =
            new System.Windows.Point(0.5, 0.5);

        tabImage.RenderTransform =
            new ScaleTransform(
                pose.IsMirrored ? -1 : 1,
                1);
    }

    private readonly Dictionary<
        FieldGuideTab,
        TabTurnPose[]> _tabTurnPaths;

    private int BookX(int x) =>
        x * _bookScale;

    private int BookY(int y) =>
        y * _bookScale;

    private int ContentPadding =>
        _uiScale;

    private static TabTurnPose[] CreateTabTurnPath(
        double startY)
    {
        return
        [
            new(190, startY, false),
            new(182, startY - 2, false),
            new(172, startY - 5, false),
            new(158, startY - 8, false),
            new(143, startY - 10, false),
            new(125, startY - 11, false),

            new(105, startY - 11, true),
            new(87,  startY - 10, true),
            new(70,  startY - 8, true),
            new(56,  startY - 5, true),
            new(45,  startY - 2, true),
            new(37,  startY, true)
        ];
    }

    private static FieldGuideEntry LoadFieldGuideEntry(
        string creatureId)
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data",
            "FieldGuide",
            $"{creatureId}.json");

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<FieldGuideEntry>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException(
                $"Could not load Field Guide entry '{creatureId}'.");
    }

    private readonly Dictionary<FieldGuideTab, FieldGuideTabEntry>
        _tabsByColor;

    private FieldGuideTabEntry GetTabEntry(
        FieldGuideTab tab)
    {
        return _tabsByColor[tab];
    }
    private void CenterCreatureName()
    {
        int titleCenterX =
            BookX(59);

        Canvas.SetLeft(
            CreatureNameText,
            titleCenterX -
            (CreatureNameText.ActualWidth / 2));
    }

    public FieldGuideMenu(
        Action spawnRat,
        int uiScale)
    {
        InitializeComponent();

        _spawnRat = spawnRat;

        _uiScale = uiScale;
        _titleScale = uiScale + 1;
        _bookScale = uiScale + 2;

        FieldGuideDefinition guide =
            LoadFieldGuideDefinition();

        _tabsByColor =
            guide.Tabs.ToDictionary(
                entry => entry.Tab);

        _creatureEntries =
            guide.Tabs
                .Select(entry => entry.CreatureId)
                .Distinct()
                .ToDictionary(
                    id => id,
                    LoadFieldGuideEntry);

        BookCanvas.LayoutTransform =
            new ScaleTransform(_bookScale, _bookScale);

        CreatureContentCanvas.Width =
            BookCanvas.Width * _bookScale;

        CreatureContentCanvas.Height =
            BookCanvas.Height * _bookScale;

        Canvas.SetLeft(
            CreatureNameContainer,
            BookX(20));

        Canvas.SetTop(
            CreatureNameContainer,
            BookY(39));

        CreatureNameContainer.Width =
            BookX(78);

        CreatureNameText.FontScale =
            _titleScale;

        Canvas.SetLeft(
            CreatureFactsText,
            BookX(22) + ContentPadding);

        Canvas.SetTop(
            CreatureFactsText,
            BookY(109) + ContentPadding);

        CreatureFactsText.FontScale =
            _uiScale;

        Canvas.SetLeft(
            CreatureDescriptionText,
            BookX(106) + ContentPadding);

        Canvas.SetTop(
            CreatureDescriptionText,
            BookY(39) + ContentPadding);

        CreatureDescriptionText.FontScale =
            _uiScale;

        CreatureDescriptionText.Width =
            74 * _bookScale;

        Canvas.SetLeft(
            CreatureFieldNotesText,
            BookX(106) + ContentPadding);

        Canvas.SetTop(
            CreatureFieldNotesText,
            BookY(94) + ContentPadding);

        CreatureFieldNotesText.FontScale =
            _uiScale;

        CreatureFieldNotesText.Width =
            76 * _bookScale;

        Canvas.SetLeft(
            SpawnButtonImage,
            BookX(128));

        Canvas.SetTop(
            SpawnButtonImage,
            BookY(128));

        SpawnButtonImage.Visibility = Visibility.Visible;

        _spawnButton = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_spawn.png");
        _spawnButtonPressed = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_pressed_spawn.png");
        _spawnButtonHover = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_hover_spawn.png");

        SpawnButtonImage.Source = _spawnButton;

        SpawnButtonImage.Width =
            _spawnButton.PixelWidth * _uiScale;

        SpawnButtonImage.Height =
            _spawnButton.PixelHeight * _uiScale;

        Width = BookCanvas.Width * _bookScale;
        Height = BookCanvas.Height * _bookScale;

        _openingFrames =
        [
            LoadUiImage(
                $"{FieldGuideAssetPath}/Book/opening_0.png"),

            LoadUiImage(
                $"{FieldGuideAssetPath}/Book/opening_1.png"),

            LoadUiImage(
                $"{FieldGuideAssetPath}/Book/opening_2.png"),

            //LoadUiImage(
            //    $"{FieldGuideAssetPath}/open.png")
        ];

        _bookBase = LoadUiImage(
            $"{FieldGuideAssetPath}/Book/BookBase.png");

        _pageTurnFrames = Enumerable
            .Range(0, 11)
            .Select(index => LoadUiImage(
                $"{FieldGuideAssetPath}/Book/PageTurnBase_{index}.png"))
            .ToArray();

        _tabTurnPaths = new()
        {
            [FieldGuideTab.Red] = _redTabTurnPath
            //add other tabs and their turn paths as needed
        };
        //_tabTurnPaths[FieldGuideTab.Green] =
        //[
        //    // Its own 11 coordinates.
        //];

        _tabSpriteSheet = new TabSpriteSheet(
            "Assets/UI/FieldGuide/Common/tabs.png",
            TabWidth,
            TabHeight);

        BookBaseImage.Source = _openingFrames[0];

        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                FieldGuideTab.Red,
                ButtonState.Normal);

        _currentTab = FieldGuideTab.Red;

        PositionRightTab(
            _currentTab);

        TurningTabImage.Source =
            _tabSpriteSheet.GetFrame(
                FieldGuideTab.Red,
                ButtonState.Normal);

        TurningTabImage.Visibility = 
            Visibility.Collapsed;

        Loaded += FieldGuideMenu_Loaded;
    }

    private async void FieldGuideMenu_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await PlayOpeningAnimationAsync();
    }

    private async Task PlayOpeningAnimationAsync()
    {
        if (_isOpening)
            return;

        _isOpening = true;
        RestingTabImage.Visibility =
            Visibility.Collapsed;

        RightTabButton.IsEnabled = false;

        BookBaseImage.Visibility = Visibility.Collapsed;

        SetSpawnButtonVisible(false);

        PageTurnImage.Source = _openingFrames[0];
        PageTurnImage.Visibility = Visibility.Visible;

        try
        {
            foreach (var frame in _openingFrames)
            {
                PageTurnImage.Source = frame;
                await Task.Delay(100);
            }
        }
        finally
        {
            _isOpening = false;

            
            PageTurnImage.Visibility =
                Visibility.Collapsed;

            BookBaseImage.Source =
                _bookBase;

            BookBaseImage.Visibility =
                Visibility.Visible;

            RestingTabImage.Visibility =
                Visibility.Visible;

            RightTabButton.Visibility =
                Visibility.Visible;

            RightTabButton.IsEnabled = true;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions =
        new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        }
    };

    private static FieldGuideDefinition LoadFieldGuideDefinition()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data",
            "FieldGuide",
            "fieldGuide.json");

        string json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<FieldGuideDefinition>(
            json,
            JsonOptions)
            ?? throw new InvalidOperationException(
                "Could not load Field Guide definition.");
    }

    private void SetSpawnButtonVisible(bool visible)
    {
        var visibility =
            visible
                ? Visibility.Visible
                : Visibility.Collapsed;

        SpawnCreatureButton.Visibility = visibility;
        SpawnButtonImage.Visibility = visibility;
    }

    private void SetCreaturePageVisible(bool visible)
    {
        var visibility =
            visible
                ? Visibility.Visible
                : Visibility.Collapsed;

        CreatureEntryCanvas.Visibility = visibility;
        CreatureContentCanvas.Visibility = visibility;

        SetSpawnButtonVisible(visible);
    }

    private void SpawnCreature_Click(
        object sender,
        RoutedEventArgs e)
    {
        _spawnRat();
    }

    private void SpawnCreatureButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source = _spawnButtonHover;
    }

    private void SpawnCreatureButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source = _spawnButton;
    }

    private void SpawnCreatureButton_MouseLeftButtonDown(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source = _spawnButtonPressed;
    }

    private void SpawnCreatureButton_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source = _spawnButtonHover;
    }

    private async void RightTabButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        FieldGuideTabEntry tabEntry =
            _tabsByColor[_currentTab];

        await TurnToCreatureAsync(
            tabEntry.Tab,
            tabEntry.CreatureId);
    }

    private async void LeftTabButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await TurnBackAsync();
    }

    private async void TabButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not WpfButton button ||
            button.Tag is not FieldGuideTab tab)
        {
            return;
        }

        FieldGuideTabEntry tabEntry =
            _tabsByColor[tab];

        await TurnToCreatureAsync(
            tabEntry.Tab,
            tabEntry.CreatureId);
    }

    private void RightTabButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Hover);
    }

    private void RightTabButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Normal);
    }

    private void RightTabButton_MouseDown(
        object sender,
        MouseButtonEventArgs e)
    {
        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Pressed);
    }

    private void RightTabButton_MouseUp(
        object sender,
        MouseButtonEventArgs e)
    {
        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Hover);
    }

    private void LeftTabButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        LeftRestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Hover);
    }

    private void LeftTabButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        LeftRestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Normal);
    }

    private void LeftTabButton_MouseDown(
        object sender,
        MouseButtonEventArgs e)
    {
        LeftRestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Pressed);
    }

    private void LeftTabButton_MouseUp(
        object sender,
        MouseButtonEventArgs e)
    {
        LeftRestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Hover);
    }

    private async Task TurnBackAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_currentCreatureId is null)
            return;

        if (!_tabTurnPaths.TryGetValue(
            _currentTab,
            out var path))
        {
            return;
        }

        _isPageTurning = true;

        SetCreaturePageVisible(false);

        LeftTabButton.IsEnabled = false;
        RightTabButton.IsEnabled = false;

        LeftRestingTabImage.Visibility =
            Visibility.Collapsed;

        TurningTabImage.Source =
            _tabSpriteSheet.GetFrame(
                _currentTab,
                ButtonState.Normal);

        TurningTabImage.Visibility =
            Visibility.Visible;

        try
        {
            PageTurnImage.Visibility =
                Visibility.Visible;

            for (int i = _pageTurnFrames.Length - 1;
                 i >= 0;
                 i--)
            {
                PageTurnImage.Source =
                    _pageTurnFrames[i];

                ApplyTabPose(
                    TurningTabImage,
                    path[i]);

                await Task.Delay(60);
            }

            LeftRestingTabImage.Source =
                _tabSpriteSheet.GetFrame(
                    _currentTab,
                    ButtonState.Normal);

            LeftRestingTabImage.Visibility =
                Visibility.Collapsed;

            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            SetCreaturePageVisible(false);

            ShowRightRestingTab(_currentTab);
        }
        finally
        {
            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _isPageTurning = false;

            LeftTabButton.IsEnabled = false;
            RightTabButton.IsEnabled = true;

            _currentCreatureId = null;

            SetCreaturePageVisible(false);

            ShowRightRestingTab(
                _currentTab);
        }
    }

    private void ShowRightRestingTab(
        FieldGuideTab tab)
    {
        PositionRightTab(tab);

        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                tab,
                ButtonState.Normal);

        RestingTabImage.Visibility =
            Visibility.Visible;

        RightTabButton.Visibility =
            Visibility.Visible;

        LeftRestingTabImage.Visibility =
            Visibility.Collapsed;

        LeftTabButton.Visibility =
            Visibility.Collapsed;
    }

    private void PositionRightTab(
        FieldGuideTab tab)
    {
        FieldGuideTabEntry entry =
            GetTabEntry(tab);

        Canvas.SetLeft(
            RestingTabImage,
            entry.RightX);

        Canvas.SetTop(
            RestingTabImage,
            entry.RightY);

        Canvas.SetLeft(
            RightTabButton,
            entry.RightX);

        Canvas.SetTop(
            RightTabButton,
            entry.RightY);
    }

    private async Task TurnToCreatureAsync(
        FieldGuideTab tab,
        string creatureId)
    {
        if (_isOpening || _isPageTurning)
            return;

        if (!_tabTurnPaths.TryGetValue(
            tab,
            out var path))
        {
            throw new InvalidOperationException(
                $"No page-turn path exists for tab {tab}.");
        }

        if (path.Length != _pageTurnFrames.Length)
        {
            throw new InvalidOperationException(
                $"Tab path for {tab} has {path.Length} poses, " +
                $"but the page animation has {_pageTurnFrames.Length} frames.");
        }

        _isPageTurning = true;

        RightTabButton.IsEnabled = false;
        LeftTabButton.IsEnabled = false;

        RestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                tab,
                ButtonState.Normal);

        RestingTabImage.Visibility =
            Visibility.Collapsed;

        RestingTabImage.Visibility = Visibility.Collapsed;
        LeftRestingTabImage.Visibility = Visibility.Collapsed;

        TurningTabImage.Source =
            _tabSpriteSheet.GetFrame(
                tab,
                ButtonState.Normal);

        TurningTabImage.Visibility = Visibility.Visible;

        try
        {
            SetCreaturePageVisible(false);

            PageTurnImage.Visibility = 
                Visibility.Visible;

            for (int i = 0;
                 i < _pageTurnFrames.Length;
                 i++)
            {
                PageTurnImage.Source =
                    _pageTurnFrames[i];

                ApplyTabPose(
                    TurningTabImage,
                    path[i]);

                await Task.Delay(60);
            }

            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _currentTab = tab;
            _currentCreatureId = creatureId;

            ShowLeftRestingTab(
                tab,
                path[^1]);

            ShowCreatureEntry(
                creatureId);
        }
        finally
        {
            TurningTabImage.Visibility = Visibility.Collapsed;

            _isPageTurning = false;

            RightTabButton.IsEnabled = false;
            LeftTabButton.IsEnabled = true;
        }
    }

    private void ShowCreatureEntry(
        string creatureId)
    {
        FieldGuideEntry entry =
            _creatureEntries[creatureId];

        CreatureContentCanvas.DataContext =
            entry;

        SetCreaturePageVisible(true);

        //Dispatcher.BeginInvoke(
         //   CenterCreatureName);
    }

    private void StartCreaturePortraitAnimation()
    {
        // Implementation for starting the creature portrait animation.
        // This could involve setting up a timer or task to cycle through frames.
    }

    private void ShowLeftRestingTab(
        FieldGuideTab tab,
        TabTurnPose finalPose)
    {
        LeftRestingTabImage.Source =
            _tabSpriteSheet.GetFrame(
                tab,
                ButtonState.Normal);

        Canvas.SetLeft(
            LeftRestingTabImage,
            finalPose.X);

        Canvas.SetTop(
            LeftRestingTabImage,
            finalPose.Y);

        Canvas.SetLeft(
            LeftTabButton,
            finalPose.X);

        Canvas.SetTop(
            LeftTabButton,
            finalPose.Y);

        LeftRestingTabImage.Visibility =
            Visibility.Visible;

        LeftTabButton.Visibility =
            Visibility.Visible;
    }

    private void BookDragArea_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
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
}