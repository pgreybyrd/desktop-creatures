using Desktop_Creatures.Config;
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
        private readonly AppSettings _settings;
        private readonly int _uiScale;

        private readonly BitmapImage _exitNormal = LoadImage("exit.png");
        private readonly BitmapImage _exitHover = LoadImage("exit_hover.png");
        private readonly BitmapImage _exitPressed = LoadImage("exit_pressed.png");

        public SettingsWindow(
            AppSettings settings,
            int uiScale)
        {
            InitializeComponent();

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
        }

        private static BitmapImage LoadImage(
            string fileName)
        {
            return new BitmapImage(
                new Uri(
                    $"pack://application:,,,/Assets/UI/Settings/Buttons/{fileName}",
                    UriKind.Absolute));
        }

        private void Exit_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            ExitImage.Source =
                _exitHover;
        }

        private void Exit_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            ExitImage.Source =
                _exitNormal;
        }

        private void Exit_MouseDown(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            ExitImage.Source =
                _exitPressed;
        }

        private void Exit_MouseUp(
            object sender,
            WpfMouseButtonEventArgs e)
        {
            ExitImage.Source =
                _exitHover;

            Close();
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