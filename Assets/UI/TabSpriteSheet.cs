using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Assets.UI
{
    public sealed class TabSpriteSheet
    {
        private readonly Dictionary<
            (FieldGuideTab Tab, ButtonState State),
            ImageSource> _frames = new();

        public TabSpriteSheet(
            string assetPath,
            int cellWidth,
            int cellHeight)
        {
            var sheet = AssetImageLoader.Load(assetPath);

            foreach (FieldGuideTab tab in
                     Enum.GetValues<FieldGuideTab>())
            {
                foreach (ButtonState state in
                         Enum.GetValues<ButtonState>())
                {
                    int column = (int)state;
                    int row = (int)tab;

                    var rectangle = new Int32Rect(
                        column * cellWidth,
                        row * cellHeight,
                        cellWidth,
                        cellHeight);

                    var frame = new CroppedBitmap(
                        sheet,
                        rectangle);

                    frame.Freeze();

                    _frames[(tab, state)] = frame;
                }
            }
        }

        public ImageSource GetFrame(
            FieldGuideTab tab,
            ButtonState state)
        {
            return _frames[(tab, state)];
        }
    }
}
