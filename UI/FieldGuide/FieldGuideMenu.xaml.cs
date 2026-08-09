using Desktop_Creatures.Assets.UI;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfImage = System.Windows.Controls.Image;
using WpfButton = System.Windows.Controls.Button;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;

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

public enum FieldGuidePage
{
    Cover,
    Rat,
    Eagle
}

public partial class FieldGuideMenu : Window
{
    private const string FieldGuideAssetPath =
        "Assets/UI/FieldGuide";

    private readonly Action _spawnRat;

    private readonly int _uiScale;

    private readonly BitmapImage[] _openingFrames;
    private readonly BitmapImage _bookBase;
    private readonly BitmapImage[] _pageTurnFrames;

    //private readonly BitmapImage[] _ratPortraitFrames;
    //private CancellationTokenSource? _ratPortraitAnimationCancellation;

    private readonly TabSpriteSheet _tabSpriteSheet;
    private const int TabWidth = 7;
    private const int TabHeight = 8;

    private bool _isPageTurning;
    private FieldGuideTab _currentTab = FieldGuideTab.Red;
    private FieldGuidePage _currentPage = FieldGuidePage.Cover;

    //creature pages
    //private readonly BitmapImage _ratPage;
    private readonly BitmapImage _spawnButton;
    private readonly BitmapImage _spawnButtonPressed;
    private readonly BitmapImage _spawnButtonHover;

    private bool _isOpening;

    private readonly Dictionary<FieldGuideTab, FieldGuidePage>
        _pagesByTab = new()
        {
            [FieldGuideTab.Red] = FieldGuidePage.Rat,
            //[FieldGuideTab.Blue] = FieldGuidePage.Eagle
            //add all the other tabs and pages as needed
        };

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

    public FieldGuideMenu(
        Action spawnRat,
        int uiScale)
    {
        InitializeComponent();

        _spawnRat = spawnRat;
        _uiScale = uiScale + 1;

        //_ratPage = LoadUiImage(
        //    $"{FieldGuideAssetPath}/Common/RatPage.png");
        _spawnButton = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_spawn.png");
        _spawnButtonPressed = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_pressed_spawn.png");
        _spawnButtonHover = LoadUiImage(
            $"{FieldGuideAssetPath}/Common/button_hover_spawn.png");

        //_ratPortraitFrames = Enumerable
        //    .Range(0, 26) 
        //    .Select(index => LoadUiImage(
        //        $"{FieldGuideAssetPath}/Pages/Sprites/Rat/ratPortrait_{index}.png"))
        //    .ToArray();

        MainCanvas.LayoutTransform =
            new ScaleTransform(_uiScale, _uiScale);

        Width = MainCanvas.Width * _uiScale;
        Height = MainCanvas.Height * _uiScale;

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

        //CreatureTitleText.Visibility =
        //    Visibility.Collapsed;

        //CreatureDescriptionText.Visibility =
        //    Visibility.Collapsed;

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

    private void SetSpawnButtonVisible(
        bool visible)
    {
        var visibility = visible
            ? Visibility.Visible
            : Visibility.Collapsed;

        SpawnRatButton.Visibility = visibility;
        SpawnRatImage.Visibility = visibility;
    }

    private void SpawnRat_Click(
        object sender,
        RoutedEventArgs e)
    {
        _spawnRat();
    }

    private async void RightTabButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var destination = _pagesByTab[_currentTab];

        await TurnToPageAsync(
            _currentTab,
            destination);
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

        var destination = _pagesByTab[tab];

        await TurnToPageAsync(
            tab,
            destination);
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

        if (_currentPage == FieldGuidePage.Cover)
            return;

        if (!_tabTurnPaths.TryGetValue(
            _currentTab,
            out var path))
        {
            return;
        }

        //StopRatPortraitAnimation();

        _isPageTurning = true;

        LeftTabButton.IsEnabled = false;
        RightTabButton.IsEnabled = false;

        LeftRestingTabImage.Visibility =
            Visibility.Collapsed;
        SpawnRatButton.Visibility = Visibility.Collapsed;

        SectionDetailImage.Source = null;

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

            _currentPage = FieldGuidePage.Cover;

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
        }
    }

    private void ShowRightRestingTab(
        FieldGuideTab tab)
    {
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

    private async Task TurnToPageAsync(
        FieldGuideTab tab,
        FieldGuidePage destination)
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_currentPage == destination)
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
            PageTurnImage.Visibility = Visibility.Visible;

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

            TurningTabImage.Visibility = Visibility.Collapsed;

            _currentTab = tab;
            _currentPage = destination;

            ShowLeftRestingTab(tab, path[^1]);

            ShowPageContent(destination);
        }
        finally
        {
            TurningTabImage.Visibility = Visibility.Collapsed;

            _isPageTurning = false;

            RightTabButton.IsEnabled = false;
            LeftTabButton.IsEnabled = true;
        }
    }

    private void ShowPageContent(FieldGuidePage page)
    {
        SectionDetailImage.Source = null;

        //CreatureTitleText.Visibility =
        //    Visibility.Collapsed;

        //CreatureDescriptionText.Visibility =
        //    Visibility.Collapsed;

        SetSpawnButtonVisible(false);

        switch (page)
        {
            case FieldGuidePage.Rat:
                //SectionDetailImage.Source = _ratPage;

                //CreatureTitleText.Text = "RAT";

                //CreatureDescriptionText.Text =
                //    "Tiny paws.\r\nLarge appetite.\r\nQuestionable priorities.";

                ////CreatureTitleText.Visibility =
                ////    Visibility.Visible;

                //CreatureDescriptionText.Visibility =
                //    Visibility.Visible;

                SetSpawnButtonVisible(true);
                //StartRatPortraitAnimation();
                break;
        }
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

    private void SpawnRatButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnButtonHover;
        SpawnRatImage.Visibility = Visibility.Visible;
    }
    private void SpawnRatButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnButton;
        SpawnRatImage.Visibility = Visibility.Visible;
    }
    private void SpawnRatButton_MouseLeftButtonDown(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnButtonPressed;
        SpawnRatImage.Visibility = Visibility.Visible;
    }
    private void SpawnRatButton_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnRatImage.Source = _spawnButtonHover;
        SpawnRatImage.Visibility = Visibility.Visible;
    }

    //private void StartRatPortraitAnimation()
    //{
    //    StopRatPortraitAnimation();

    //    _ratPortraitAnimationCancellation =
    //        new CancellationTokenSource();

    //    _ = PlayRatPortraitAnimationAsync(
    //        _ratPortraitAnimationCancellation.Token);
    //}

    //private void StopRatPortraitAnimation()
    //{
    //    _ratPortraitAnimationCancellation?.Cancel();
    //    _ratPortraitAnimationCancellation?.Dispose();
    //    _ratPortraitAnimationCancellation = null;

    //    RatPortraitImage.Visibility =
    //        Visibility.Collapsed;
    //}

    //private async Task PlayRatPortraitAnimationAsync(
    //    CancellationToken cancellationToken)
    //{
    //    RatPortraitImage.Visibility =
    //        Visibility.Visible;

    //    while (!cancellationToken.IsCancellationRequested &&
    //           _currentPage == FieldGuidePage.Rat)
    //    {
    //        foreach (var frame in _ratPortraitFrames)
    //        {
    //            cancellationToken.ThrowIfCancellationRequested();

    //            RatPortraitImage.Source = frame;

    //            await Task.Delay(
    //                120,
    //                cancellationToken);
    //        }

    //        // A small pause prevents the idle from feeling frantic.
    //        await Task.Delay(
    //            700,
    //            cancellationToken);
    //    }
    //}

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

    public sealed record FieldGuideTabDefinition(
        FieldGuideTab Tab,
        FieldGuidePage Page,
        double RightX,
        double RightY);

    private readonly FieldGuideTabDefinition[] _tabDefinitions =
    [
        new(FieldGuideTab.Red,
            FieldGuidePage.Rat,
            181,
            40),

        new(FieldGuideTab.Green,
            FieldGuidePage.Eagle,
            181,
            50)

        // Remaining tabs later.
    ];
}