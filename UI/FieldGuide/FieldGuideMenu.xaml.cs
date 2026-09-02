using Desktop_Creatures.Audio;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.UI.FieldGuide;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using ToolTip = System.Windows.Controls.ToolTip;
using WpfButton = System.Windows.Controls.Button;
using WpfImage = System.Windows.Controls.Image;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures;

public enum FieldGuideTab
{
    Crimson,
    Moss,
    Cobalt,
    Marigold,
    Amethyst,
    Silver,
    Aqua,
    Tangerine,
    Azure,
    Emerald,
    Raspberry,
    Charcoal
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

    private readonly FieldGuideSounds _sounds = new();

    private readonly Action<string> _spawnCreature;

    private readonly int _uiScale;
    private readonly int _titleScale;
    private readonly int _bookScale;

    private readonly BitmapImage[] _openingFrames;
    private readonly BitmapImage _bookBase;
    private readonly BitmapImage[] _pageTurnFrames;

    private readonly SpriteSheet _tabsSheet;
    private readonly SpriteSheet _subTabsSheet;
    private readonly SpriteSheet _toolTextSheet;
    private readonly SpriteSheet _buttonsSheet;
    private readonly SpriteSheet _framesSheet;

    private const int TabWidth = 7;
    private const int TabHeight = 8;
    private const int RightTabX = 181;
    private const int FrontPageIndex = -1;
    private int _currentTabIndex = -1;

    private const int SubTabWidth = 10;
    private const int SubTabHeight = 8;

    private const int SubTabStartX = 31;
    private const int SubTabY = 28;
    private const int SubTabSpacing = 1;

    private sealed record CreatureSubTabControl(
        string CreatureId,
        WpfImage Image,
        WpfButton Button);

    private readonly Dictionary<string, CreatureSubTabControl>
        _creatureSubTabControls =
            new(StringComparer.OrdinalIgnoreCase);

    private const int PageTurnFrameDelayMs = 40;

    private bool _isPageTurning;
    private bool _isBookOpen = true;
    private FieldGuideTab _currentTab = FieldGuideTab.Crimson;

    //creature pages
    private readonly BitmapImage _closeButton;
    private readonly BitmapImage _closeButtonHover;
    private readonly BitmapImage _closeButtonPressed;

    private readonly BitmapImage _exitButton;
    private readonly BitmapImage _exitButtonHover;
    private readonly BitmapImage _exitButtonPressed;

    private readonly Dictionary<string, FieldGuideEntry> _creatureEntries;
    private readonly Dictionary<FieldGuideTab, FieldGuideCategoryEntry>
        _categoriesByTab;

    private string? _currentCreatureId;

    private bool _isOpening;

    public readonly record struct TabTurnPose(
        double X,
        double Y,
        bool IsMirrored);

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
        int startY)
    {
        int yOffset = startY - 40;

        return
        [
            new(181, 40 + yOffset, false),
            new(176, 38 + yOffset, false),
            new(169, 34 + yOffset, false),
            new(158, 30 + yOffset, false),
            new(144, 24 + yOffset, false),
            new(120, 21 + yOffset, false),

            new(87,  20 + yOffset, true),
            new(58,  24 + yOffset, true),
            new(35,  29 + yOffset, true),
            new(20,  35 + yOffset, true),
            new(13,  40 + yOffset, true)
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
            "Entries",
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
        Action<string> spawnCreature,
        int uiScale)
    {
        InitializeComponent();

        _spawnCreature = spawnCreature;

        _uiScale = uiScale;
        _titleScale = uiScale + 1;
        _bookScale = uiScale + 2;

        _tabsSheet = SpriteSheetLoader.Load(
            $"{FieldGuideAssetPath}/Common/tabs.png",
            $"{FieldGuideAssetPath}/Common/tabs.json");

        _subTabsSheet = SpriteSheetLoader.Load(
            $"{FieldGuideAssetPath}/Common/subTabs.png",
            $"{FieldGuideAssetPath}/Common/subTabs.json");

        _toolTextSheet = SpriteSheetLoader.Load(
            $"{FieldGuideAssetPath}/Common/tooltext.png",
            $"{FieldGuideAssetPath}/Common/tooltext.json");

        _buttonsSheet = SpriteSheetLoader.Load(
            $"{FieldGuideAssetPath}/Common/buttons.png",
            $"{FieldGuideAssetPath}/Common/buttons.json");

        _framesSheet = SpriteSheetLoader.Load(
            $"{FieldGuideAssetPath}/Common/frames.png",
            $"{FieldGuideAssetPath}/Common/frames.json");

        FieldGuideDefinition guide =
            LoadFieldGuideDefinition();

        _categoriesByTab =
            guide.Categories.ToDictionary(
                category => category.Tab);

        string entriesPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data",
            "FieldGuide",
            "Entries");

        _creatureEntries =
            Directory
                .EnumerateFiles(entriesPath, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(id => id is not null)
                .ToDictionary(
                    id => id!,
                    id => LoadFieldGuideEntry(id!));

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

        SpawnButtonImage.Source =
            _buttonsSheet
                .GetFrame("spawn_normal")
                .Image;

        SpriteFrame spawnFrame =
            _buttonsSheet.GetFrame("spawn_normal");

        SpawnButtonImage.Width =
            spawnFrame.Image.PixelWidth * _uiScale;

        SpawnButtonImage.Height =
            spawnFrame.Image.PixelHeight * _uiScale;

        _closeButton = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/close.png");

        _closeButtonHover = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/close-hover.png");

        _closeButtonPressed = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/close-pressed.png");

        CloseButtonImage.Source =
            _closeButton;

        _exitButton = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/exit.png");

        _exitButtonHover = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/exit-hover.png");

        _exitButtonPressed = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/exit-pressed.png");

        ExitButtonImage.Source =
            _exitButton;

        //EXIT BUTTON
        int exitX =
            BookX(182);

        int exitY =
            BookY(21);

        Canvas.SetLeft(
            ExitButtonImage,
            exitX);

        Canvas.SetTop(
            ExitButtonImage,
            exitY);

        Canvas.SetLeft(
            ExitFieldGuideButton,
            exitX);

        Canvas.SetTop(
            ExitFieldGuideButton,
            exitY);

        ExitButtonImage.Width =
            _exitButton.PixelWidth * _uiScale;

        ExitButtonImage.Height =
            _exitButton.PixelHeight * _uiScale;

        ExitFieldGuideButton.Width =
            ExitButtonImage.Width;

        ExitFieldGuideButton.Height =
            ExitButtonImage.Height;

        Width = BookCanvas.Width * _bookScale;
        Height = BookCanvas.Height * _bookScale;

        //CLOSE BUTTON
        int closeX =
            BookX(7);

        int closeY =
            BookY(21);

        Canvas.SetLeft(
            CloseButtonImage,
            closeX);

        Canvas.SetTop(
            CloseButtonImage,
            closeY);

        Canvas.SetLeft(
            CloseFieldGuideButton,
            closeX);

        Canvas.SetTop(
            CloseFieldGuideButton,
            closeY);

        CloseButtonImage.Width =
            _closeButton.PixelWidth * _uiScale;

        CloseButtonImage.Height =
            _closeButton.PixelHeight * _uiScale;

        CloseFieldGuideButton.Width =
            CloseButtonImage.Width;

        CloseFieldGuideButton.Height =
            CloseButtonImage.Height;

        //front page
        Canvas.SetLeft(
            FrontPageTitleText,
            BookX(45));

        Canvas.SetTop(
            FrontPageTitleText,
            BookY(45));

        FrontPageTitleText.FontScale =
            _titleScale;

        Canvas.SetLeft(
            FrontPageBodyText,
            BookX(35));

        Canvas.SetTop(
            FrontPageBodyText,
            BookY(80));

        FrontPageBodyText.FontScale =
            _uiScale;
        //----------

        _openingFrames =
        [
            AssetImageLoader.Load(
                $"{FieldGuideAssetPath}/Book/opening_0.png"),

            AssetImageLoader.Load(
                $"{FieldGuideAssetPath}/Book/opening_1.png"),

            AssetImageLoader.Load(
                $"{FieldGuideAssetPath}/Book/opening_2.png"),

            //AssetImageLoader.Load(
            //    $"{FieldGuideAssetPath}/open.png")
        ];

        _bookBase = AssetImageLoader.Load(
            $"{FieldGuideAssetPath}/Book/BookBase.png");

        _pageTurnFrames = Enumerable
            .Range(0, 11)
            .Select(index => AssetImageLoader.Load(
                $"{FieldGuideAssetPath}/Book/PageTurnBase_{index}.png"))
            .ToArray();

        _tabTurnPaths =
            _categoriesByTab.Values
                .ToDictionary(
                    entry => entry.Tab,
                    entry => CreateTabTurnPath(
                        entry.RightY));

        BuildTabs();

        BookBaseImage.Source = _openingFrames[0];

        _currentTab = FieldGuideTab.Crimson;

        TurningTabImage.Source =
            GetCategoryTabFrame(
                FieldGuideTab.Crimson,
                ButtonState.Normal);

        TurningTabImage.Visibility = 
            Visibility.Collapsed;

        Loaded += FieldGuideMenu_Loaded;
    }

    private void BuildTabs()
    {
        foreach (FieldGuideCategoryEntry entry in
                 _categoriesByTab.Values.OrderBy(e => e.Order))
        {
            var rightImage =
                CreateTabImage(entry.Tab);

            var rightButton =
                CreateTabButton(
                    entry,
                    RightTab_Click);

            var leftImage =
                CreateTabImage(entry.Tab);

            leftImage.RenderTransformOrigin =
                new System.Windows.Point(0.5, 0.5);

            leftImage.RenderTransform =
                new ScaleTransform(-1, 1);

            var leftButton =
                CreateTabButton(
                    entry,
                    LeftTab_Click);

            Canvas.SetLeft(
                rightImage,
                RightTabX + entry.RightX);
            Canvas.SetTop(rightImage, entry.RightY);

            Canvas.SetLeft(
                rightButton,
                RightTabX + entry.RightX);
            Canvas.SetTop(rightButton, entry.RightY);

            // For now use the final position from that tab's turn path.
            TabTurnPose leftPose =
                _tabTurnPaths[entry.Tab][^1];

            Canvas.SetLeft(leftImage, leftPose.X);
            Canvas.SetTop(leftImage, leftPose.Y);

            Canvas.SetLeft(leftButton, leftPose.X);
            Canvas.SetTop(leftButton, leftPose.Y);

            RightTabsCanvas.Children.Add(rightImage);
            RightTabsCanvas.Children.Add(rightButton);

            LeftTabsCanvas.Children.Add(leftImage);
            LeftTabsCanvas.Children.Add(leftButton);

            _tabControls[entry.Tab] =
                new FieldGuideTabControl(
                    leftImage,
                    leftButton,
                    rightImage,
                    rightButton);
        }

        UpdateRestingTabs();
    }

    public async Task NavigateToCreature(
        string creatureId)
    {
        if (!_creatureEntries.ContainsKey(
                creatureId))
        {
            return;
        }

        CreatureDefinition definition =
            CreatureDefinitionLoader.Load(creatureId);

        FieldGuideCategoryEntry? category =
            _categoriesByTab.Values
                .FirstOrDefault(
                    entry =>
                        string.Equals(
                            entry.Id,
                            definition.Category,
                            StringComparison.OrdinalIgnoreCase));

        if (category is null)
            return;

        if (_isOpening)
        {
            while (_isOpening)
            {
                await Task.Delay(25);
            }
        }

        if (!_isBookOpen)
        {
            await PlayOpeningAnimationAsync();
            _isBookOpen = true;
        }

        _currentTabIndex =
            category.Order;

        _currentTab =
            category.Tab;

        _currentCreatureId =
            creatureId;

        FrontPageCanvas.Visibility =
            Visibility.Collapsed;

        UpdateRestingTabs();

        ShowCreatureEntry(
            creatureId);
    }

    private WpfImage CreateTabImage(
        FieldGuideTab tab)
    {
        return new WpfImage
        {
            Width = TabWidth,
            Height = TabHeight,
            Stretch = Stretch.None,
            IsHitTestVisible = false,
            Source = GetCategoryTabFrame(
                tab,
                ButtonState.Normal)
        };
    }

    private ToolTip CreateFieldGuideToolTip(
        FieldGuideCategoryEntry family,
        WpfButton targetButton)
    {
        var source =
            _toolTextSheet
                .GetFrame(family.Id)
                .Image;

        var tooltipImage = new WpfImage
        {
            Source = source,
            Width = source.PixelWidth * _uiScale,
            Height = source.PixelHeight * _uiScale,
            Stretch = Stretch.Fill,
            SnapsToDevicePixels = true
        };

        RenderOptions.SetBitmapScalingMode(
            tooltipImage,
            BitmapScalingMode.NearestNeighbor);

        return new ToolTip
        {
            Content = tooltipImage,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),

            PlacementTarget = targetButton,
            Placement =
                System.Windows.Controls.Primitives.PlacementMode.Right,

            HorizontalOffset = _uiScale,
            VerticalOffset = -1 *_uiScale
        };
    }

    private ToolTip CreateCreatureSubTabToolTip(
        string creatureId,
        WpfButton targetButton)
    {
        BitmapSource source =
            _toolTextSheet
                .GetFrame(creatureId)
                .Image;

        WpfImage tooltipImage =
            new()
            {
                Source = source,
                Width = source.PixelWidth * _uiScale,
                Height = source.PixelHeight * _uiScale,
                Stretch = Stretch.Fill,
                SnapsToDevicePixels = true
            };

        RenderOptions.SetBitmapScalingMode(
            tooltipImage,
            BitmapScalingMode.NearestNeighbor);

        return new ToolTip
        {
            Content = tooltipImage,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            PlacementTarget = targetButton,
            Placement =
                System.Windows.Controls.Primitives
                    .PlacementMode.Top,
            HorizontalOffset = 0,
            VerticalOffset = -_uiScale
        };
    }

    private WpfButton CreateTabButton(
        FieldGuideCategoryEntry family,
        RoutedEventHandler clickHandler)
    {
        var button = new WpfButton
        {
            Width = TabWidth,
            Height = TabHeight,
            Tag = family.Tab,
            Style = (Style)FindResource(
                "InvisibleTabButton")
        };

        button.ToolTip =
            CreateFieldGuideToolTip(
                family,
                button);

        ToolTipService.SetInitialShowDelay(button, 150);
        ToolTipService.SetBetweenShowDelay(button, 150);
        ToolTipService.SetShowDuration(button, 5000);
        ToolTipService.SetHasDropShadow(button, false);

        button.Click += clickHandler;
        button.MouseEnter += Tab_MouseEnter;
        button.MouseLeave += Tab_MouseLeave;
        button.PreviewMouseLeftButtonDown += Tab_MouseDown;
        button.PreviewMouseLeftButtonUp += Tab_MouseUp;

        return button;
    }

    private async void RightTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not WpfButton button ||
            button.Tag is not FieldGuideTab tab)
        {
            return;
        }

        FieldGuideCategoryEntry destination =
            _categoriesByTab[tab];

        await TurnForwardToAsync(
            destination);
    }

    private async void LeftTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not WpfButton button ||
            button.Tag is not FieldGuideTab tab)
        {
            return;
        }

        FieldGuideCategoryEntry destination =
            _categoriesByTab[tab];

        await TurnBackwardToAsync(
            destination);
    }

    private BitmapSource GetCategoryTabFrame(
        FieldGuideTab tab,
        ButtonState state)
    {
        string frameName =
            $"{tab.ToString().ToLowerInvariant()}_" +
            $"{state.ToString().ToLowerInvariant()}";

        return _tabsSheet
            .GetFrame(frameName)
            .Image;
    }

    private async Task TurnBackwardToFrontPageAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        FieldGuideCategoryEntry current =
            _categoriesByTab[_currentTab];

        if (!_tabTurnPaths.TryGetValue(
            current.Tab,
            out var path))
        {
            return;
        }

        _isPageTurning = true;

        _sounds.PlayPageFlip();

        SetCreaturePageVisible(false);

        FieldGuideTabControl controls =
            _tabControls[current.Tab];

        controls.LeftImage.Visibility =
            Visibility.Collapsed;

        controls.LeftButton.Visibility =
            Visibility.Collapsed;

        TurningTabImage.Source =
            GetCategoryTabFrame(
                current.Tab,
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

                await Task.Delay(
                    PageTurnFrameDelayMs);
            }

            _currentTabIndex =
                FrontPageIndex;

            _currentCreatureId = null;

            UpdateRestingTabs();

            FrontPageCanvas.Visibility =
                Visibility.Visible;
        }
        finally
        {
            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _isPageTurning = false;
        }
    }

    public async Task CloseWithAnimationAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_isBookOpen)
        {
            await PlayClosingAnimationAsync();
        }

        Close();
    }

    private async void CloseFieldGuide_Click(
        object sender,
        RoutedEventArgs e)
    {
        await PlayClosingAnimationAsync();
    }

    private async Task PlayClosingAnimationAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        _isOpening = true;

        ExitButtonImage.Visibility =
            Visibility.Collapsed;

        ExitFieldGuideButton.Visibility =
            Visibility.Collapsed;

        CloseButtonImage.Visibility =
            Visibility.Collapsed;

        CloseFieldGuideButton.Visibility =
            Visibility.Collapsed;

        _sounds.PlayBookClose();

        FrontPageCanvas.Visibility =
            Visibility.Collapsed;

        SetCreaturePageVisible(false);

        LeftTabsCanvas.Visibility =
            Visibility.Collapsed;

        RightTabsCanvas.Visibility =
            Visibility.Collapsed;

        BookBaseImage.Visibility =
            Visibility.Collapsed;

        PageTurnImage.Visibility =
            Visibility.Visible;

        try
        {
            for (int i = _openingFrames.Length - 1;
                 i >= 0;
                 i--)
            {
                PageTurnImage.Source =
                    _openingFrames[i];

                await Task.Delay(100);
            }
        }
        finally
        {
            PageTurnImage.Source =
                _openingFrames[0];

            PageTurnImage.Visibility =
                Visibility.Visible;

            _isOpening = false;
            _isBookOpen = false;



            ExitButtonImage.Visibility =
                Visibility.Visible;

            ExitFieldGuideButton.Visibility =
                Visibility.Visible;

            OpenBookButton.Visibility =
                Visibility.Visible;
        }
    }

    private async void OpenBook_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_isBookOpen || _isOpening)
            return;

        OpenBookButton.Visibility =
            Visibility.Collapsed;

        _currentTabIndex =
            FrontPageIndex;

        _currentCreatureId =
            null;

        UpdateRestingTabs();

        SetCreaturePageVisible(false);

        await PlayOpeningAnimationAsync();

        _isBookOpen = true;

        CloseButtonImage.Visibility =
            Visibility.Visible;

        CloseFieldGuideButton.Visibility =
            Visibility.Visible;
    }

    private void CloseFieldGuideButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        CloseButtonImage.Source =
            _closeButtonHover;
    }

    private void CloseFieldGuideButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        CloseButtonImage.Source =
            _closeButton;
    }

    private void CloseFieldGuideButton_MouseDown(
        object sender,
        WpfMouseEventArgs e)
    {
        CloseButtonImage.Source =
            _closeButtonPressed;
    }

    private void CloseFieldGuideButton_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        CloseButtonImage.Source =
            _closeButtonHover;
    }

    private async void ExitFieldGuide_Click(
        object sender,
        RoutedEventArgs e)
    {
        UiSounds.PlayButtonClick();

        await CloseWithAnimationAsync();
    }

    private void ExitFieldGuideButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitButtonImage.Source =
            _exitButtonHover;
    }

    private void ExitFieldGuideButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitButtonImage.Source =
            _exitButton;
    }

    private void ExitFieldGuideButton_MouseDown(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitButtonImage.Source =
            _exitButtonPressed;
    }

    private void ExitFieldGuideButton_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        ExitButtonImage.Source =
            _exitButtonHover;
    }

    private void Tab_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideTab? tab =
            GetTabFromButton(sender);

        if (tab is null)
            return;

        FieldGuideTabControl controls =
            _tabControls[tab.Value];

        WpfImage image =
            controls.LeftButton.Visibility ==
                Visibility.Visible
                ? controls.LeftImage
                : controls.RightImage;

        image.Source =
            GetCategoryTabFrame(
                tab.Value,
                ButtonState.Hover);
    }

    private void Tab_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideTab? tab =
            GetTabFromButton(sender);

        if (tab is null)
            return;

        FieldGuideTabControl controls =
            _tabControls[tab.Value];

        WpfImage image =
            controls.LeftButton.Visibility ==
                Visibility.Visible
                ? controls.LeftImage
                : controls.RightImage;

        image.Source =
            GetCategoryTabFrame(
                tab.Value,
                ButtonState.Normal);
    }

    private void Tab_MouseDown(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideTab? tab =
            GetTabFromButton(sender);

        if (tab is null)
            return;

        FieldGuideTabControl controls =
            _tabControls[tab.Value];

        WpfImage image =
            controls.LeftButton.Visibility ==
                Visibility.Visible
                ? controls.LeftImage
                : controls.RightImage;

        image.Source =
            GetCategoryTabFrame(
                tab.Value,
                ButtonState.Pressed);
    }

    private void Tab_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        FieldGuideTab? tab =
            GetTabFromButton(sender);

        if (tab is null)
            return;

        FieldGuideTabControl controls =
            _tabControls[tab.Value];

        WpfImage image =
            controls.LeftButton.Visibility ==
                Visibility.Visible
                ? controls.LeftImage
                : controls.RightImage;

        image.Source =
            GetCategoryTabFrame(
                tab.Value,
                ButtonState.Hover);
    }

    private sealed record FieldGuideTabControl(
        WpfImage LeftImage,
        WpfButton LeftButton,
        WpfImage RightImage,
        WpfButton RightButton);

    private readonly Dictionary<
        FieldGuideTab,
        FieldGuideTabControl> _tabControls = new();

    private void UpdateRestingTabs()
    {
        foreach (FieldGuideCategoryEntry entry in
                 _categoriesByTab.Values)
        {
            var controls =
                _tabControls[entry.Tab];

            bool isOnLeft =
                entry.Order <= _currentTabIndex;

            controls.LeftImage.Visibility =
                isOnLeft
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            controls.LeftButton.Visibility =
                isOnLeft
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            controls.RightImage.Visibility =
                isOnLeft
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            controls.RightButton.Visibility =
                isOnLeft
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }

    private FieldGuideTab? GetTabFromButton(
        object sender)
    {
        if (sender is WpfButton button &&
            button.Tag is FieldGuideTab tab)
        {
            return tab;
        }

        return null;
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

        _sounds.PlayBookOpen();

        WindowControlsCanvas.Visibility =
            Visibility.Collapsed;

        BookBaseImage.Visibility = 
            Visibility.Collapsed;

        RightTabsCanvas.Visibility =
            Visibility.Collapsed;

        SetSpawnButtonVisible(false);

        PageTurnImage.Source = _openingFrames[0];
        PageTurnImage.Visibility = 
            Visibility.Visible;

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



            WindowControlsCanvas.Visibility =
                Visibility.Visible;

            FrontPageCanvas.Visibility =
                Visibility.Visible;

            PageTurnImage.Visibility =
                Visibility.Collapsed;

            BookBaseImage.Source =
                _bookBase;

            BookBaseImage.Visibility =
                Visibility.Visible;

            LeftTabsCanvas.Visibility =
                Visibility.Visible;

            RightTabsCanvas.Visibility =
                Visibility.Visible;

            UpdateRestingTabs();
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
        UiSounds.PlayButtonClick();

        if (_currentCreatureId is null)
            return;

        _spawnCreature(_currentCreatureId);
    }

    private void SpawnCreatureButton_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source =
            _buttonsSheet
                .GetFrame("spawn_hover")
                .Image;
    }

    private void SpawnCreatureButton_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source =
            _buttonsSheet
                .GetFrame("spawn_normal")
                .Image;
    }

    private void SpawnCreatureButton_MouseLeftButtonDown(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source =
            _buttonsSheet
                .GetFrame("spawn_pressed")
                .Image;
    }

    private void SpawnCreatureButton_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        SpawnButtonImage.Source =
            _buttonsSheet
                .GetFrame("spawn_hover")
                .Image;
    }

    private async Task TurnBackwardToAsync(
        FieldGuideCategoryEntry destination)
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_currentCreatureId is null)
            return;

        FieldGuideCategoryEntry current =
            _categoriesByTab[_currentTab];

        if (!_tabTurnPaths.TryGetValue(
            current.Tab,
            out var path))
        {
            return;
        }

        _isPageTurning = true;

        _sounds.PlayPageFlip();

        FrontPageCanvas.Visibility =
            Visibility.Collapsed;

        SetCreaturePageVisible(false);

        FieldGuideTabControl controls =
            _tabControls[current.Tab];

        controls.LeftImage.Visibility =
            Visibility.Collapsed;

        controls.LeftButton.Visibility =
            Visibility.Collapsed;

        TurningTabImage.Source =
            GetCategoryTabFrame(
                current.Tab,
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

                await Task.Delay(
                    PageTurnFrameDelayMs);
            }

            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _currentTabIndex =
                destination.Order;

            _currentTab =
                destination.Tab;

            string? creatureId =
                GetFirstCreatureIdForCategory(destination);

            if (creatureId is null)
                return;

            _currentCreatureId =
                creatureId;

            UpdateRestingTabs();

            ShowCreatureEntry(
                creatureId);
        }
        finally
        {
            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _isPageTurning = false;
        }
    }

    private static string? GetFirstCreatureIdForCategory(
        FieldGuideCategoryEntry category)
    {
        return category.Creatures.FirstOrDefault();
    }

    //private static string? GetFirstCreatureIdForCategory(
    //    string categoryId)
    //{
    //    string definitionsPath = Path.Combine(
    //        AppContext.BaseDirectory,
    //        "Assets",
    //        "Data",
    //        "Creatures",
    //        "Definitions");

    //    foreach (string file in
    //             Directory.EnumerateFiles(
    //                 definitionsPath,
    //                 "*.json"))
    //    {
    //        string creatureId =
    //            Path.GetFileNameWithoutExtension(file);

    //        CreatureDefinition definition =
    //            CreatureDefinitionLoader.Load(creatureId);

    //        if (string.Equals(
    //                definition.Category,
    //                categoryId,
    //                StringComparison.OrdinalIgnoreCase))
    //        {
    //            return creatureId;
    //        }
    //    }

    //    return null;
    //}

    private async Task TurnForwardToAsync(
        FieldGuideCategoryEntry destination)
    {
        if (_isOpening || _isPageTurning)
            return;

        if (!_tabTurnPaths.TryGetValue(
            destination.Tab,
            out var path))
        {
            throw new InvalidOperationException(
                $"No page-turn path exists for tab {destination.Tab}.");
        }

        if (path.Length != _pageTurnFrames.Length)
        {
            throw new InvalidOperationException(
                $"Tab path for {destination.Tab} has {path.Length} poses, " +
                $"but the page animation has {_pageTurnFrames.Length} frames.");
        }

        _isPageTurning = true;

        _sounds.PlayPageFlip();

        FrontPageCanvas.Visibility =
            Visibility.Collapsed;

        FieldGuideTabControl controls =
            _tabControls[destination.Tab];

        controls.RightImage.Visibility =
            Visibility.Collapsed;

        controls.RightButton.Visibility =
            Visibility.Collapsed;

        TurningTabImage.Source =
            GetCategoryTabFrame(
                destination.Tab,
                ButtonState.Normal);

        TurningTabImage.Visibility =
            Visibility.Visible;

        try
        {
            SetCreaturePageVisible(false);

            FrontPageCanvas.Visibility =
                Visibility.Collapsed;

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

                await Task.Delay(
                    PageTurnFrameDelayMs);
            }

            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _currentTabIndex =
                destination.Order;

            _currentTab =
                destination.Tab;

            string? creatureId =
                GetFirstCreatureIdForCategory(destination);

            if (creatureId is null)
                return;

            _currentCreatureId =
                creatureId;

            UpdateRestingTabs();

            ShowCreatureEntry(
                creatureId);
        }
        finally
        {
            PageTurnImage.Visibility =
                Visibility.Collapsed;

            TurningTabImage.Visibility =
                Visibility.Collapsed;

            _isPageTurning = false;
        }
    }

    private void ShowCreatureEntry(
        string creatureId)
    {
        FieldGuideEntry entry =
            _creatureEntries[creatureId];

        FieldGuideCategoryEntry category =
            _categoriesByTab[_currentTab];

        BuildCreatureSubTabs(category);

        CreatureContentCanvas.DataContext =
            entry;

        string creatureFolder =
            char.ToUpperInvariant(creatureId[0]) +
            creatureId[1..];

        string fieldGuideCreaturePath =
            $"Assets/UI/FieldGuide/Creatures/{creatureFolder}";

        CreaturePortraitFrameImage.Source =
            _framesSheet
                .GetFrame("basic")
                .Image;

        CreaturePortraitImage.Source =
            AssetImageLoader.Load(
                $"{fieldGuideCreaturePath}/Portrait/portrait-{creatureId}.png");


        SetCreaturePageVisible(true);

        //Dispatcher.BeginInvoke(
         //   CenterCreatureName);
    }

    private void StartCreaturePortraitAnimation()
    {
        // Implementation for starting the creature portrait animation.
        // This could involve setting up a timer or task to cycle through frames.
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

    private void BuildCreatureSubTabs(
        FieldGuideCategoryEntry category)
    {
        CreatureSubTabsCanvas.Children.Clear();
        _creatureSubTabControls.Clear();

        if (category.Creatures.Count <= 1)
        {
            CreatureSubTabsCanvas.Visibility =
                Visibility.Collapsed;

            return;
        }

        CreatureSubTabsCanvas.Visibility =
            Visibility.Visible;

        for (int i = 0;
             i < category.Creatures.Count;
             i++)
        {
            string creatureId =
                category.Creatures[i];

            bool isCurrent =
                string.Equals(
                    creatureId,
                    _currentCreatureId,
                    StringComparison.OrdinalIgnoreCase);

            WpfImage image =
                new()
                {
                    Width = SubTabWidth,
                    Height = SubTabHeight,
                    Stretch = Stretch.None,
                    IsHitTestVisible = false,
                    Source = _subTabsSheet
                        .GetFrame(
                            isCurrent
                                ? "current"
                                : "other_normal")
                        .Image
                };

            WpfButton button =
                new()
                {
                    Width = SubTabWidth,
                    Height = SubTabHeight,
                    Tag = creatureId,
                    Style = (Style)FindResource(
                        "InvisibleTabButton")
                };

            button.ToolTip =
                CreateCreatureSubTabToolTip(
                    creatureId,
                    button);

            ToolTipService.SetInitialShowDelay(button, 150);
            ToolTipService.SetBetweenShowDelay(button, 150);
            ToolTipService.SetShowDuration(button, 5000);
            ToolTipService.SetHasDropShadow(button, false);

            button.MouseEnter += CreatureSubTab_MouseEnter;
            button.MouseLeave += CreatureSubTab_MouseLeave;
            button.PreviewMouseLeftButtonDown += CreatureSubTab_MouseDown;
            button.PreviewMouseLeftButtonUp += CreatureSubTab_MouseUp;
            button.Click += CreatureSubTab_Click;

            int x =
                SubTabStartX +
                (i * (SubTabWidth + SubTabSpacing));

            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, SubTabY);

            Canvas.SetLeft(button, x);
            Canvas.SetTop(button, SubTabY);

            CreatureSubTabsCanvas.Children.Add(image);
            CreatureSubTabsCanvas.Children.Add(button);

            _creatureSubTabControls[creatureId] =
                new CreatureSubTabControl(
                    creatureId,
                    image,
                    button);
        }
    }

    private void CreatureSubTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not WpfButton button ||
            button.Tag is not string creatureId)
        {
            return;
        }

        if (string.Equals(
                creatureId,
                _currentCreatureId,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        UiSounds.PlayButtonClick();

        _currentCreatureId = creatureId;

        ShowCreatureEntry(creatureId);
    }

    private void CreatureSubTab_MouseEnter(
        object sender,
        WpfMouseEventArgs e)
    {
        SetCreatureSubTabState(
            sender,
            "other_hover");
    }

    private void CreatureSubTab_MouseLeave(
        object sender,
        WpfMouseEventArgs e)
    {
        SetCreatureSubTabState(
            sender,
            "other_normal");
    }

    private void CreatureSubTab_MouseDown(
        object sender,
        WpfMouseEventArgs e)
    {
        SetCreatureSubTabState(
            sender,
            "other_pressed");
    }

    private void CreatureSubTab_MouseUp(
        object sender,
        WpfMouseEventArgs e)
    {
        SetCreatureSubTabState(
            sender,
            "other_hover");
    }

    private void SetCreatureSubTabState(
        object sender,
        string frameName)
    {
        if (sender is not WpfButton button ||
            button.Tag is not string creatureId ||
            !_creatureSubTabControls.TryGetValue(
                creatureId,
                out CreatureSubTabControl? control))
        {
            return;
        }

        // Current tab always remains current.
        if (string.Equals(
                creatureId,
                _currentCreatureId,
                StringComparison.OrdinalIgnoreCase))
        {
            control.Image.Source =
                _subTabsSheet
                    .GetFrame("current")
                    .Image;

            return;
        }

        control.Image.Source =
            _subTabsSheet
                .GetFrame(frameName)
                .Image;
    }
}