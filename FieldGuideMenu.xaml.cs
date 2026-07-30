using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures;

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
    private readonly BitmapImage[] _redPageTurnFrames;

    private bool _isPageTurning;
    private FieldGuidePage _currentPage = FieldGuidePage.Cover;

    private bool _isOpening;

    public FieldGuideMenu(
        Action spawnRat,
        int uiScale)
    {
        InitializeComponent();

        _spawnRat = spawnRat;
        _uiScale = uiScale;

        MainCanvas.LayoutTransform =
            new ScaleTransform(_uiScale, _uiScale);

        Width = MainCanvas.Width * _uiScale;
        Height = MainCanvas.Height * _uiScale;

        _openingFrames =
        [
            LoadUiImage(
                $"{FieldGuideAssetPath}/opening_0.png"),

            LoadUiImage(
                $"{FieldGuideAssetPath}/opening_1.png"),

            LoadUiImage(
                $"{FieldGuideAssetPath}/opening_2.png"),

            LoadUiImage(
                $"{FieldGuideAssetPath}/open.png")
        ];

        _redPageTurnFrames = Enumerable
            .Range(0, 12)
            .Select(index => LoadUiImage(
                $"{FieldGuideAssetPath}/red_{index}.png"))
            .ToArray();

        BookImage.Source = _openingFrames[0];

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
        RightRedTabButton.IsEnabled = false;

        try
        {
            foreach (var frame in _openingFrames)
            {
                BookImage.Source = frame;

                await Task.Delay(100);
            }

            // Keep the final open-book image visible.
            BookImage.Source = _openingFrames[^1];
        }
        finally
        {
            _isOpening = false;
            RightRedTabButton.IsEnabled = true;
        }
    }

    private void SpawnRat_Click(
        object sender,
        RoutedEventArgs e)
    {
        _spawnRat();
    }

    private async void RightRedTab_Click(
        object sender,
        RoutedEventArgs e)
    {
        await TurnToRatPageAsync();
    }

    private async void LeftRedTab_Click(
    object sender,
    RoutedEventArgs e)
    {
        await TurnBackToCoverAsync();
    }

    private async Task TurnToRatPageAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_currentPage == FieldGuidePage.Rat)
            return;

        _isPageTurning = true;
        RightRedTabButton.IsEnabled = false;

        try
        {
            foreach (var frame in _redPageTurnFrames)
            {
                BookImage.Source = frame;

                await Task.Delay(60);
            }

            BookImage.Source = _redPageTurnFrames[^1];
            //when i make the rat page it's own static page!
            //BookImage.Source = LoadUiImage(
            //    $"{FieldGuideAssetPath}/rat.png");
            _currentPage = FieldGuidePage.Rat;
        }
        finally
        {
            _isPageTurning = false;
            RightRedTabButton.IsEnabled = false;
            LeftRedTabButton.IsEnabled = true;
        }
    }

    private async Task TurnBackToCoverAsync()
    {
        if (_isOpening || _isPageTurning)
            return;

        if (_currentPage == FieldGuidePage.Cover)
            return;

        _isPageTurning = true;

        LeftRedTabButton.IsEnabled = false;
        RightRedTabButton.IsEnabled = false;

        try
        {
            for (int i = _redPageTurnFrames.Length - 1; i >= 0; i--)
            {
                BookImage.Source = _redPageTurnFrames[i];

                await Task.Delay(60);
            }

            BookImage.Source = _openingFrames[^1];
            _currentPage = FieldGuidePage.Cover;

            RightRedTabButton.IsEnabled = true;
        }
        finally
        {
            _isPageTurning = false;
        }
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