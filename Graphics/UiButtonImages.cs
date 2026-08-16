using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Graphics
{
    public sealed class UiButtonImages
    {
        public BitmapImage Normal { get; }
        public BitmapImage Hover { get; }
        public BitmapImage Pressed { get; }

        public UiButtonImages(
            BitmapImage normal,
            BitmapImage hover,
            BitmapImage pressed)
        {
            Normal = normal;
            Hover = hover;
            Pressed = pressed;
        }
    }
}
