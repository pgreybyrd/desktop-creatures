using Desktop_Creatures.Audio;
using Desktop_Creatures.Graphics.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cursors = System.Windows.Input.Cursors;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Desktop_Creatures.UI.RightClick
{
    public partial class CreatureContextMenuWindow :
        Window
    {
        private const double TopRowYOffset = 6;

        private const double ButtonX = 7;
        private const double ButtonY = 0;

        private const double IconX = 14;
        private const double IconY = 5;

        private const double TextX = 29;
        private const double TextY = 7;

        private readonly int _menuScale;

        private bool _isClosing;

        private readonly IReadOnlyList<
            CreatureContextMenuItem> _items;

        private readonly SpriteSheet _buttonSheet;
        private readonly SpriteSheet _iconSheet;
        private readonly SpriteSheet _textSheet;
        private readonly SpriteSheet _menuSheet;

        private readonly BitmapSource _buttonNormal;
        private readonly BitmapSource _buttonHover;
        private readonly BitmapSource _buttonPressed;

        private readonly BitmapSource _menuTop;
        private readonly BitmapSource _menuMiddle;
        private readonly BitmapSource _menuBottom;
        private readonly BitmapSource _menuDivider;

        public event Action<
            CreatureContextMenuWindow>?
            ReadyToPosition;

        public CreatureContextMenuWindow(
            IReadOnlyList<
                CreatureContextMenuItem> items,
            int uiScale)
        {
            InitializeComponent();

            _items = items;

            _menuScale =
                Math.Max(
                    2,
                    uiScale - 1);

            _buttonSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/RightClick/buttons.png",
                    "Assets/UI/RightClick/buttons.json");

            _buttonNormal =
                _buttonSheet.GetFrame("normal").Image;

            _buttonHover =
                _buttonSheet.GetFrame("hover").Image;

            _buttonPressed =
                _buttonSheet.GetFrame("pressed").Image;


            _menuSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/RightClick/menu.png",
                    "Assets/UI/RightClick/menu.json");

            _menuTop =
                _menuSheet.GetFrame("top").Image;

            _menuMiddle =
                _menuSheet.GetFrame("middle").Image;

            _menuBottom =
                _menuSheet.GetFrame("bottom").Image;

            _menuDivider =
                _menuSheet.GetFrame("divider").Image;


            _iconSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/RightClick/icons.png",
                    "Assets/UI/RightClick/icons.json");

            _textSheet =
                SpriteSheetLoader.Load(
                    "Assets/UI/RightClick/text.png",
                    "Assets/UI/RightClick/text.json");


            BuildMenu();

            MenuCanvas.LayoutTransform =
                new ScaleTransform(
                    _menuScale,
                    _menuScale);
        }

        private void BuildMenu()
        {
            MenuCanvas.Children.Clear();

            double y = 0;

            for (int i = 0;
                 i < _items.Count;
                 i++)
            {
                CreatureContextMenuItem item =
                    _items[i];

                if (item.DividerBefore)
                {
                    AddImage(
                        _menuDivider,
                        0,
                        y);

                    y +=
                        _menuDivider.PixelHeight;
                }

                BitmapSource background =
                    i == 0
                        ? _menuTop
                        : i == _items.Count - 1
                            ? _menuBottom
                            : _menuMiddle;

                AddImage(
                    background,
                    0,
                    y);

                AddActionRow(
                    item,
                    y);

                y +=
                    background.PixelHeight;
            }

            MenuCanvas.Width =
                _menuTop.PixelWidth;

            MenuCanvas.Height =
                y;
        }

        private void AddActionRow(
            CreatureContextMenuItem item,
            double y)
        {
            var button =
                new Image
                {
                    Source =
                        _buttonNormal,

                    Stretch =
                        Stretch.None,

                    Cursor =
                        Cursors.Hand
                };

            double rowY =
                y +
                (item == _items[0]
                    ? TopRowYOffset
                    : 0);

            Canvas.SetLeft(
                button,
                ButtonX);

            Canvas.SetTop(
                button,
                rowY + ButtonY);

            BitmapSource icon =
                _iconSheet
                    .GetFrame(item.AssetName)
                    .Image;

            BitmapSource text =
                _textSheet
                    .GetFrame(item.AssetName)
                    .Image;

            var iconImage =
                new Image
                {
                    Source = icon,
                    Stretch = Stretch.None,
                    IsHitTestVisible = false
                };

            var textImage =
                new Image
                {
                    Source = text,
                    Stretch = Stretch.None,
                    IsHitTestVisible = false
                };

            Canvas.SetLeft(
                iconImage,
                IconX);

            Canvas.SetTop(
                iconImage,
                rowY + IconY);

            Canvas.SetLeft(
                textImage,
                TextX);

            Canvas.SetTop(
                textImage,
                rowY + TextY);

            button.MouseEnter +=
                (_, _) =>
                {
                    button.Source =
                        _buttonHover;
                };

            button.MouseLeave +=
                (_, _) =>
                {
                    button.Source =
                        _buttonNormal;
                };

            button.MouseLeftButtonDown +=
                (_, _) =>
                {
                    button.Source =
                        _buttonPressed;
                };

            button.MouseLeftButtonUp +=
                (_, _) =>
                {
                    UiSounds.PlayButtonClick();

                    item.Execute();

                    CloseMenu();
                };

            MenuCanvas.Children.Add(
                button);

            MenuCanvas.Children.Add(
                iconImage);

            MenuCanvas.Children.Add(
                textImage);
        }

        public void CloseMenu()
        {
            if (_isClosing)
                return;

            _isClosing = true;

            Close();
        }

        protected override void OnContentRendered(
            EventArgs e)
        {
            base.OnContentRendered(e);

            ReadyToPosition?.Invoke(this);

            Deactivated +=
                CreatureContextMenuWindow_Deactivated;
        }

        private void CreatureContextMenuWindow_Deactivated(
            object? sender,
            EventArgs e)
        {
            CloseMenu();
        }

        protected override void OnClosed(
            EventArgs e)
        {
            Deactivated -=
                CreatureContextMenuWindow_Deactivated;

            base.OnClosed(e);
        }

        protected override void OnKeyDown(
            KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseMenu();
                return;
            }

            base.OnKeyDown(e);
        }

        private void AddImage(
            BitmapSource source,
            double x,
            double y)
        {
            var image =
                new Image
                {
                    Source = source,
                    Stretch = Stretch.None,
                    IsHitTestVisible = false
                };

            Canvas.SetLeft(
                image,
                x);

            Canvas.SetTop(
                image,
                y);

            MenuCanvas.Children.Add(
                image);
        }
    }
}