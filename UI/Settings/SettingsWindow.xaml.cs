using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Config;
using Desktop_Creatures.Audio;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using Desktop_Creatures.Graphics;

namespace Desktop_Creatures
{
    public partial class SettingsWindow : Window
    {
        public event Action<int>? ScaleChanged;
        public event Action<bool>? EcosystemAlwaysOnTopChanged;
        public event Action<bool>? MenusAlwaysOnTopChanged;

        private readonly AppSettings _settings;
        private readonly int _uiScale;

        private readonly UiButtonImages _exitImages = null!;
        private readonly UiButtonImages _scale1Images = null!;
        private readonly UiButtonImages _scale2Images = null!;
        private readonly UiButtonImages _scale3Images = null!;
        private readonly UiButtonImages _scale4Images = null!;
        private readonly UiButtonImages _toggleOnImages = null!;
        private readonly UiButtonImages _toggleOffImages = null!;

        public SettingsWindow(
            AppSettings settings,
            int uiScale)
        {
            InitializeComponent();

            SettingsBackgroundImage.Source =
                AssetImageLoader.Load(
                    "Assets/UI/Settings/settings_window.png");

            ExitImage.Source =
                AssetImageLoader.Load(
                    "Assets/UI/Settings/Buttons/exit.png");

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

            _exitImages = LoadButtonImages("exit");
            _scale1Images = LoadButtonImages("scale_1x");
            _scale2Images = LoadButtonImages("scale_2x");
            _scale3Images = LoadButtonImages("scale_3x");
            _scale4Images = LoadButtonImages("scale_4x");
            _toggleOnImages = LoadButtonImages("toggle_on");
            _toggleOffImages = LoadButtonImages("toggle_off");

            RefreshScaleButtons();
            RefreshEcosystemAlwaysOnTopButton();
            RefreshMenusAlwaysOnTopButton();
        }

        private static UiButtonImages LoadButtonImages(string buttonName)
        {
            return new UiButtonImages(
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}.png"),
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}_hover.png"),
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}_pressed.png"));
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
                    ? _scale1Images.Pressed
                    : _scale1Images.Normal;

            Scale2Image.Source =
                scale == 2
                    ? _scale2Images.Pressed
                    : _scale2Images.Normal;

            Scale3Image.Source =
                scale == 3
                    ? _scale3Images.Pressed
                    : _scale3Images.Normal;

            Scale4Image.Source =
                scale == 4
                    ? _scale4Images.Pressed
                    : _scale4Images.Normal;
        }

        //private void ToggleAlwaysOnTop()
        //{
        //    _settings.EcosystemAlwaysOnTop =
        //        !_settings.EcosystemAlwaysOnTop;

        //    SettingsLoader.Save(
        //        _settings);

        //    RefreshAlwaysOnTopButton();

        //    EcosystemAlwaysOnTopChanged?.Invoke(
        //        _settings.EcosystemAlwaysOnTop);
        //}

        private void ToggleEcosystemAlwaysOnTop()
        {
            _settings.EcosystemAlwaysOnTop =
                !_settings.EcosystemAlwaysOnTop;

            SettingsLoader.Save(_settings);

            RefreshEcosystemAlwaysOnTopButton();

            EcosystemAlwaysOnTopChanged?.Invoke(
                _settings.EcosystemAlwaysOnTop);
        }

        private void ToggleMenusAlwaysOnTop()
        {
            _settings.MenusAlwaysOnTop =
                !_settings.MenusAlwaysOnTop;

            SettingsLoader.Save(_settings);

            RefreshMenusAlwaysOnTopButton();

            MenusAlwaysOnTopChanged?.Invoke(
                _settings.MenusAlwaysOnTop);
        }

        //private void RefreshAlwaysOnTopButton()
        //{
        //    AlwaysOnTopImage.Source =
        //        _settings.EcosystemAlwaysOnTop
        //            ? _toggleOnImages.Normal
        //            : _toggleOffImages.Normal;
        //}

        private void RefreshEcosystemAlwaysOnTopButton()
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnImages.Normal
                    : _toggleOffImages.Normal;
        }

        private void RefreshMenusAlwaysOnTopButton()
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnImages.Normal
                    : _toggleOffImages.Normal;
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
            Scale1Image.Source = _scale1Images.Hover;
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
            Scale1Image.Source = _scale1Images.Pressed;
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
            Scale2Image.Source = _scale2Images.Hover;
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
            Scale2Image.Source = _scale2Images.Pressed;
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
            Scale3Image.Source = _scale3Images.Hover;
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
            Scale3Image.Source = _scale3Images.Pressed;
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
            Scale4Image.Source = _scale4Images.Hover;
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
            Scale4Image.Source = _scale4Images.Pressed;
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
            UiSounds.PlayButtonClick();

            ExitImage.Source = _exitImages.Pressed;
        }

        private void Exit_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            ExitImage.Source = _exitImages.Hover;

            Close();
        }

        private void EcosystemAlwaysOnTop_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            EcosystemAlwaysOnTopImage.Source =
                _settings.EcosystemAlwaysOnTop
                    ? _toggleOnImages.Hover
                    : _toggleOffImages.Hover;
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
                    ? _toggleOnImages.Pressed
                    : _toggleOffImages.Pressed;
        }

        private void EcosystemAlwaysOnTop_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();
            ToggleEcosystemAlwaysOnTop();
        }

        private void EcosystemAlwaysOnTop_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            RefreshMenusAlwaysOnTopButton();
        }

        private void MenusAlwaysOnTop_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            MenusAlwaysOnTopImage.Source =
                _settings.MenusAlwaysOnTop
                    ? _toggleOnImages.Hover
                    : _toggleOffImages.Hover;
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
                    ? _toggleOnImages.Pressed
                    : _toggleOffImages.Pressed;
        }

        private void MenusAlwaysOnTop_Click(
            object sender,
            RoutedEventArgs e)
        {
            UiSounds.PlayButtonClick();
            ToggleMenusAlwaysOnTop();
        }

        private void MenusAlwaysOnTop_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            RefreshMenusAlwaysOnTopButton();
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