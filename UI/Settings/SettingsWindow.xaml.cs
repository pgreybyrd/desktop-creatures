using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.Tools.Images;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures
{
    public partial class SettingsWindow : Window
    {
        public event Action<int>? ScaleChanged;
        public event Action<bool>? EcosystemAlwaysOnTopChanged;
        public event Action<bool>? MenusAlwaysOnTopChanged;

        private readonly AppSettings _settings;
        private readonly int _uiScale;

        private readonly SpriteSheet _buttonSheet;
        private readonly SpriteSheet _textSheet;
        private readonly SpriteSheet _exitSheet;
        private readonly SpriteSheet _toggleOnSheet;
        private readonly SpriteSheet _toggleOffSheet;

        private readonly BitmapSource _buttonNormal;
        private readonly BitmapSource _buttonHover;
        private readonly BitmapSource _buttonPressed;

        private readonly BitmapSource _text1x;
        private readonly BitmapSource _text2x;
        private readonly BitmapSource _text3x;
        private readonly BitmapSource _text4x;

        private readonly BitmapSource _exitNormal;
        private readonly BitmapSource _exitHover;
        private readonly BitmapSource _exitPressed;

        private readonly BitmapSource _toggleOnNormal;
        private readonly BitmapSource _toggleOnHover;
        private readonly BitmapSource _toggleOnPressed;

        private readonly BitmapSource _toggleOffNormal;
        private readonly BitmapSource _toggleOffHover;
        private readonly BitmapSource _toggleOffPressed;

        public SettingsWindow(
            AppSettings settings,
            int uiScale)
        {
            InitializeComponent();

            SettingsBackgroundImage.Source =
                AssetImageLoader.Load(
                    "Assets/UI/Settings/settings_window.png");

            _buttonSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/Settings/buttons.png",
                    "Assets/UI/Settings/buttons.json");

            _buttonNormal =
                _buttonSheet.GetFrame("normal").Image;
            _buttonHover =
                _buttonSheet.GetFrame("hover").Image;
            _buttonPressed =
                _buttonSheet.GetFrame("pressed").Image;


            _textSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/Settings/text.png",
                    "Assets/UI/Settings/text.json");

            _text1x =
                _textSheet.GetFrame("1x").Image;
            _text2x =
                _textSheet.GetFrame("2x").Image;
            _text3x =
                _textSheet.GetFrame("3x").Image;
            _text4x =
                _textSheet.GetFrame("4x").Image;

            Scale1TextImage.Source =
                _text1x;
            Scale2TextImage.Source =
                _text2x;
            Scale3TextImage.Source =
                _text3x;
            Scale4TextImage.Source =
                _text4x;

            _exitSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/Settings/exit.png",
                    "Assets/UI/Settings/exit.json");

            _exitNormal =
                _exitSheet.GetFrame("normal").Image;
            _exitHover =
                _exitSheet.GetFrame("hover").Image;
            _exitPressed =
                _exitSheet.GetFrame("pressed").Image;


            _toggleOnSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/Settings/toggleon.png",
                    "Assets/UI/Settings/toggleon.json");

            _toggleOnNormal =
                _toggleOnSheet.GetFrame("normal").Image;
            _toggleOnHover =
                _toggleOnSheet.GetFrame("hover").Image;
            _toggleOnPressed =
                _toggleOnSheet.GetFrame("pressed").Image;


            _toggleOffSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/Settings/toggleoff.png",
                    "Assets/UI/Settings/toggleoff.json");

            _toggleOffNormal =
                _toggleOffSheet.GetFrame("normal").Image;
            _toggleOffHover =
                _toggleOffSheet.GetFrame("hover").Image;
            _toggleOffPressed =
                _toggleOffSheet.GetFrame("pressed").Image;


            ExitImage.Source =
                _exitNormal;

            _settings = settings;
            _uiScale = uiScale;

            SettingsCanvas.LayoutTransform =
                new ScaleTransform(
                    _uiScale,
                    _uiScale);

            Width =
                SettingsCanvas.Width *
                _uiScale;

            Height =
                SettingsCanvas.Height *
                _uiScale;

            RefreshScaleButtons();
            RefreshEcosystemAlwaysOnTopButton();
            RefreshMenusAlwaysOnTopButton();
        }

        private void SetScale(int scale)
        {
            _settings.CreatureDisplayScale = scale;

            SettingsLoader.Save(
                _settings);

            RefreshScaleButtons();

            ScaleChanged?.Invoke(scale);
        }

        private void RefreshScaleButtons()
        {
            int scale =
                _settings.CreatureDisplayScale;

            Scale1Image.Source =
                scale == 1
                    ? _buttonPressed
                    : _buttonNormal;

            Scale2Image.Source =
                scale == 2
                    ? _buttonPressed
                    : _buttonNormal;

            Scale3Image.Source =
                scale == 3
                    ? _buttonPressed
                    : _buttonNormal;

            Scale4Image.Source =
                scale == 4
                    ? _buttonPressed
                    : _buttonNormal;
        }

        private void ToggleEcosystemAlwaysOnTop()
        {
            _settings.EcosystemAlwaysOnTop =
                !_settings.EcosystemAlwaysOnTop;

            SettingsLoader.Save(_settings);

            EcosystemAlwaysOnTopChanged?.Invoke(
                _settings.EcosystemAlwaysOnTop);
        }

        private void ToggleMenusAlwaysOnTop()
        {
            _settings.MenusAlwaysOnTop =
                !_settings.MenusAlwaysOnTop;

            SettingsLoader.Save(_settings);

            MenusAlwaysOnTopChanged?.Invoke(
                _settings.MenusAlwaysOnTop);
        }

        private void RefreshEcosystemAlwaysOnTopButton()
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnNormal
                    : _toggleOffNormal;
        }

        private void RefreshMenusAlwaysOnTopButton()
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnNormal
                    : _toggleOffNormal;
        }

        private void Scale1_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            SetScale(1);
        }

        private void Scale1_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            Scale1Image.Source = _buttonHover;
        }

        private void Scale1_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale1_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale1_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            Scale1Image.Source = _buttonPressed;
        }

        private void Scale2_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            SetScale(2);
        }

        private void Scale2_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            Scale2Image.Source = _buttonHover;
        }

        private void Scale2_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale2_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale2_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            Scale2Image.Source = _buttonPressed;
        }

        private void Scale3_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            SetScale(3);
        }

        private void Scale3_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            Scale3Image.Source = _buttonHover;
        }

        private void Scale3_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale3_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale3_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            Scale3Image.Source = _buttonPressed;
        }

        private void Scale4_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            SetScale(4);
        }

        private void Scale4_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            Scale4Image.Source = _buttonHover;
        }

        private void Scale4_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale4_MouseUp(
            object sender,
            MouseButtonEventArgs e)
        {
            RefreshScaleButtons();
        }

        private void Scale4_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            Scale4Image.Source = _buttonPressed;
        }

        private void Exit_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            ExitImage.Source = _exitHover;
        }

        private void Exit_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            ExitImage.Source = _exitNormal;
        }

        private void Exit_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            ExitImage.Source = _exitPressed;
        }

        private void Exit_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            ExitImage.Source = _exitHover;

            Close();
        }

        private void EcosystemAlwaysOnTop_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void EcosystemAlwaysOnTop_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshEcosystemAlwaysOnTopButton();
        }

        private void EcosystemAlwaysOnTop_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnPressed
                    : _toggleOffPressed;
        }

        private void EcosystemAlwaysOnTop_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            ToggleEcosystemAlwaysOnTop();

            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void EcosystemAlwaysOnTop_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void MenusAlwaysOnTop_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void MenusAlwaysOnTop_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RefreshMenusAlwaysOnTopButton();
        }

        private void MenusAlwaysOnTop_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnPressed
                    : _toggleOffPressed;
        }

        private void MenusAlwaysOnTop_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();

            ToggleMenusAlwaysOnTop();

            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void MenusAlwaysOnTop_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnHover
                    : _toggleOffHover;
        }

        private void DragArea_MouseLeftButtonDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            if (e.ButtonState != WpfMouseButtonState.Pressed)
                return;

            DragMove();
        }
    }
}