using Desktop_Creatures.Config;
using System.Windows;

namespace Desktop_Creatures
{
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;

        public SettingsWindow(
            AppSettings settings)
        {
            InitializeComponent();

            _settings = settings;
        }
    }
}