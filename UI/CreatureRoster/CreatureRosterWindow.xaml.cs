using Desktop_Creatures.Audio;
using Desktop_Creatures.Graphics;
using Desktop_Creatures.Persistence;
using Desktop_Creatures.Tools.Images;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures.UI.CreatureRoster
{
    public partial class CreatureRosterWindow : Window
    {
        private readonly Func<Guid, bool> _isSpawned;
        private readonly Action<Guid> _spawn;
        private readonly Action<Guid> _putAway;
        private readonly Action<Guid, bool> _setFavorite;

        private int _currentIndex;

        private readonly int _uiScale;

        private readonly BitmapSource _buttonNormal;
        private readonly BitmapSource _buttonHover;
        private readonly BitmapSource _buttonPressed;

        private readonly BitmapSource _spawnText;
        private readonly BitmapSource _putAwayText;
        private readonly BitmapSource _renameText;

        private readonly BitmapSource _favoriteOff;
        private readonly BitmapSource _favoriteOn;

        private readonly BitmapSource _arrowUpNormal;
        private readonly BitmapSource _arrowUpHover;
        private readonly BitmapSource _arrowUpPressed;

        private readonly BitmapSource _arrowDownNormal;
        private readonly BitmapSource _arrowDownHover;
        private readonly BitmapSource _arrowDownPressed;

        private readonly UiButtonImages _exitImages = null!;

        private readonly BitmapSource _scrollThumbMiddle;

        private readonly Func<IReadOnlyList<CreatureRecord>> _recordsProvider;

        private IReadOnlyList<CreatureRecord>
            Records =>
                _recordsProvider();

        private CreatureRecord CurrentRecord =>
            Records[_currentIndex];

        public CreatureRosterWindow(
            Func<IReadOnlyList<CreatureRecord>> recordsProvider,
            int uiScale,
            Func<Guid, bool> isSpawned,
            Action<Guid> spawn,
            Action<Guid> putAway,
            Action<Guid, bool> setFavorite)
        {
            InitializeComponent();

            _uiScale = uiScale;
            _isSpawned = isSpawned;
            _spawn = spawn;
            _putAway = putAway;
            _setFavorite = setFavorite;
            _recordsProvider = recordsProvider;

            RosterBaseImage.Source =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/roster_base.png");

            RosterCanvas.LayoutTransform =
                new ScaleTransform(
                    _uiScale,
                    _uiScale);

            Width =
                RosterCanvas.Width *
                _uiScale;

            Height =
                RosterCanvas.Height *
                _uiScale;

            _exitImages = LoadButtonImages("exit");

            _buttonNormal =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/button_normal.png");

            _buttonHover =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/button_hover.png");

            _buttonPressed =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/button_pressed.png");

            _spawnText =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/text_spawn.png");

            _putAwayText =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/text_put_away.png");

            _renameText =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/text_rename.png");

            _favoriteOff =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/favorite_off.png");

            _favoriteOn =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/favorite_on.png");

            _arrowUpNormal =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_up.png");

            _arrowUpHover =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_up-hover.png");

            _arrowUpPressed =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_up-pressed.png");

            _arrowDownNormal =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_down.png");

            _arrowDownHover =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_down-hover.png");

            _arrowDownPressed =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/arrow_down-pressed.png");

            ScrollThumbTop.Source =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/scroll_thumb-top.png");

            _scrollThumbMiddle =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/scroll_thumb-middle.png");

            ScrollThumbBottom.Source =
                AssetImageLoader.Load(
                    "Assets/UI/CreatureRoster/scroll_thumb-bottom.png");

            ExitImage.Source =
                AssetImageLoader.Load(
                    "Assets/UI/Settings/Buttons/exit.png");

            RenameTextImage.Source =
                _renameText;

            MouseWheel +=
                CreatureRosterWindow_MouseWheel;

            if (Records.Count > 0)
            {
                RefreshCreature();
            }
        }

        private static UiButtonImages LoadButtonImages(string buttonName)
        {
            return new UiButtonImages(
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}.png"),
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}_hover.png"),
                AssetImageLoader.Load($"Assets/UI/Settings/Buttons/{buttonName}_pressed.png"));
        }

        private void RefreshCreature()
        {
            IReadOnlyList<CreatureRecord> records =
                Records;

            if (records.Count == 0)
                return;

            _currentIndex =
                Math.Clamp(
                    _currentIndex,
                    0,
                    records.Count - 1);

            CreatureRecord record =
                records[_currentIndex];

            NameText.Text =
                string.IsNullOrWhiteSpace(record.Name)
                    ? "(unnamed)"
                    : record.Name.ToUpperInvariant();

            SpeciesText.Text =
                record.CreatureType.ToLowerInvariant();

            bool spawned =
                _isSpawned(record.Id);

            ActionButtonImage.Source =
                _buttonNormal;

            ActionTextImage.Source =
                spawned
                    ? _putAwayText
                    : _spawnText;

            RenameButtonImage.Source =
                _buttonNormal;

            FavoriteImage.Source =
                record.IsFavorite
                    ? _favoriteOn
                    : _favoriteOff;

            ArrowUpImage.Source =
                _arrowUpNormal;

            ArrowDownImage.Source =
                _arrowDownNormal;

            UpdateScrollThumb();
        }

        public void Refresh()
        {
            RefreshCreature();
        }

        private void ToggleSpawnState()
        {
            CreatureRecord record =
                CurrentRecord;

            if (_isSpawned(record.Id))
            {
                _putAway(record.Id);
            }
            else
            {
                _spawn(record.Id);
            }

            RefreshCreature();
        }

        private void Favorite_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            CreatureRecord record =
                CurrentRecord;

            _setFavorite(
                record.Id,
                !record.IsFavorite);

            RefreshCreature();
        }

        private void ActionButton_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            ActionButtonImage.Source =
                _buttonHover;
        }

        private void ActionButton_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            ActionButtonImage.Source =
                _buttonNormal;
        }

        private void ActionButton_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            ActionButtonImage.Source =
                _buttonPressed;
        }

        private void ActionButton_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            ToggleSpawnState();

            ActionButtonImage.Source =
                _buttonHover;
        }

        private void RenameButton_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            RenameButtonImage.Source =
                _buttonHover;
        }

        private void RenameButton_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            RenameButtonImage.Source =
                _buttonNormal;
        }

        private void RenameButton_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            RenameButtonImage.Source =
                _buttonPressed;
        }

        private void RenameButton_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            RenameButtonImage.Source =
                _buttonHover;

            // Rename dialog next.
        }

        private void ArrowUp_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            ArrowUpImage.Source =
                _arrowUpHover;
        }

        private void ArrowUp_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            ArrowUpImage.Source =
                _arrowUpNormal;
        }

        private void ArrowUp_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            ArrowUpImage.Source =
                _arrowUpPressed;
        }

        private void ArrowUp_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            MovePrevious();

            ArrowUpImage.Source =
                _arrowUpHover;
        }

        private void ArrowDown_MouseEnter(
            object sender,
            WpfMouseEventArgs e)
        {
            ArrowDownImage.Source =
                _arrowDownHover;
        }

        private void ArrowDown_MouseLeave(
            object sender,
            WpfMouseEventArgs e)
        {
            ArrowDownImage.Source =
                _arrowDownNormal;
        }

        private void ArrowDown_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            ArrowDownImage.Source =
                _arrowDownPressed;
        }

        private void ArrowDown_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            UiSounds.PlayButtonClick();

            MoveNext();

            ArrowDownImage.Source =
                _arrowDownHover;
        }

        private void CreatureRosterWindow_MouseWheel(
            object sender,
            MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MovePrevious();
            }
            else if (e.Delta < 0)
            {
                MoveNext();
            }
        }

        private void MovePrevious()
        {
            IReadOnlyList<CreatureRecord> records =
                Records;

            if (records.Count <= 1)
                return;

            _currentIndex--;

            if (_currentIndex < 0)
            {
                _currentIndex =
                    records.Count - 1;
            }

            RefreshCreature();
        }

        private void MoveNext()
        {
            IReadOnlyList<CreatureRecord> records =
                Records;

            if (records.Count <= 1)
                return;

            _currentIndex =
                (_currentIndex + 1) %
                records.Count;

            RefreshCreature();
        }

        private void UpdateScrollThumb()
        {
            IReadOnlyList<CreatureRecord> records =
                Records;

            const double trackTop = 75;
            const double trackBottom = 166;

            double trackHeight =
                trackBottom -
                trackTop;

            if (records.Count == 0)
                return;

            double topHeight =
                ((BitmapSource)ScrollThumbTop.Source)
                    .PixelHeight;

            double bottomHeight =
                ((BitmapSource)ScrollThumbBottom.Source)
                    .PixelHeight;

            double middlePieceHeight =
                _scrollThumbMiddle.PixelHeight;

            double desiredThumbHeight =
                Math.Clamp(
                    trackHeight / records.Count,
                    topHeight + bottomHeight,
                    trackHeight);

            double availableMiddleHeight =
                desiredThumbHeight -
                topHeight -
                bottomHeight;

            int middleCount =
                Math.Max(
                    0,
                    (int)Math.Floor(
                        availableMiddleHeight /
                        middlePieceHeight));

            ScrollThumbMiddleContainer
                .Children
                .Clear();

            for (int i = 0;
                 i < middleCount;
                 i++)
            {
                ScrollThumbMiddleContainer
                    .Children
                    .Add(
                        new Image
                        {
                            Source = _scrollThumbMiddle,
                            Stretch = Stretch.None
                        });
            }

            double actualThumbHeight =
                topHeight +
                bottomHeight +
                (middleCount *
                 middlePieceHeight);

            double usableTravel =
                trackHeight -
                actualThumbHeight;

            double progress =
                records.Count <= 1
                    ? 0
                    : (double)_currentIndex /
                      (records.Count - 1);

            Canvas.SetTop(
                ScrollThumb,
                trackTop +
                (usableTravel * progress));
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