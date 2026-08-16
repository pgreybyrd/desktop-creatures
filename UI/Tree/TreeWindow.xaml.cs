using Desktop_Creatures.Tools.Images;
using System.Windows;

namespace Desktop_Creatures
{
    /// <summary>
    /// Interaction logic for TreeWindow.xaml
    /// </summary>
    public partial class TreeWindow : Window
    {
        public TreeWindow(string imagePath)
        {
            InitializeComponent();

            TreeImage.Source = AssetImageLoader.Load(imagePath);
        }
    }
}
