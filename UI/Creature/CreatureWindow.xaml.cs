using Desktop_Creatures.Creatures;
using Desktop_Creatures.Utilities;
using Desktop_Creatures.World.Surfaces;
using Desktop_Creatures.Creatures.Interaction;
using System.Windows;
using System.Windows.Input;
using Desktop_Creatures.UI.RightClick;
using Point = System.Windows.Point;

namespace Desktop_Creatures;

public partial class CreatureWindow : Window
{
    private readonly Creature _creature;
    private readonly SurfaceManager _surfaceManager;

    private readonly CreatureContextMenuController
        _contextMenuController = new();

    private readonly CreatureDragController
        _dragController = new();

    private int _uiScale;

    public Creature GetCreature() => _creature;

    public bool IsSimulationPaused =>
        _dragController.IsDragging ||
        _contextMenuController.IsOpen;

    public event Action<CreatureWindow>? PutAwayRequested;
    public event Action<CreatureWindow, CreatureContextMenuAction>? ContextActionRequested;

    public CreatureWindow(
        Creature creature,
        SurfaceManager surfaceManager,
        int uiScale)
    {
        InitializeComponent();

        _creature = creature;
        _surfaceManager = surfaceManager;
        _uiScale = uiScale;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseRightButtonDown += OnMouseRightButtonDown;

        _creature.InteractionStarted +=
            BringCreatureToFront;

        Closed += (_, _) =>
        {
            _creature.InteractionStarted -=
                BringCreatureToFront;
        };

        //int scale = _creature.Scale;

        //SizeToContent = SizeToContent.WidthAndHeight;

        CreatureImage.Width = _creature.SpriteWidth;
        CreatureImage.Height = _creature.SpriteHeight;

        //CreatureImage.Stretch = Stretch.None;

        Width = _creature.SpriteWidth;// * scale;
        Height = _creature.SpriteHeight;// * scale;

        CreatureImage.Source = _creature.CurrentFrame;

        UpdateWindowPosition();

        Deactivated += (_, _) =>
        {
            if (Topmost)
            {
                Topmost = false;
                Topmost = true;
            }
        };
    }

    public void UpdateCreature()
    {
        if (_dragController.IsDragging)
        {
            _creature.UpdateHeldAnimation();

            if (CreatureImage.Source != _creature.CurrentFrame)
                CreatureImage.Source = _creature.CurrentFrame;

            return;
        }

        if (_contextMenuController.IsOpen)
        {
            return;
        }

        RefreshCreaturePresentation();

        Logger.LogDebug(
            DebugCategory.Window,
            $"Window: Action={_creature.CurrentAction}, " +
            $"Sprite={_creature.SpriteWidth}x{_creature.SpriteHeight}, " +
            $"Window={Width}x{Height}, " +
            $"Image={CreatureImage.Width}x{CreatureImage.Height}, " +
            $"CurrentFrame hash = {_creature.CurrentFrame.GetHashCode()}");
    }

    private void RefreshCreaturePresentation()
    {
        if (Math.Abs(_creature.SpeedX) > 0.01)
        {
            bool movingRight =
                _creature.SpeedX > 0;

            FlipTransform.ScaleX =
                _creature.SpriteFacesRight ==
                movingRight
                    ? 1
                    : -1;
        }

        if (CreatureImage.Source !=
            _creature.CurrentFrame)
        {
            CreatureImage.Source =
                _creature.CurrentFrame;
        }

        UpdateWindowPosition();
    }

    public void SetDisplayScale(int displayScale)
    {
        displayScale =
            Math.Clamp(displayScale, 1, 4);

        _creature.SetDisplayScale(
            displayScale);

        double width =
            _creature.SpriteWidth *
            displayScale;

        double height =
            _creature.SpriteHeight *
            displayScale;

        Width = width;
        Height = height;

        CreatureImage.Width = width;
        CreatureImage.Height = height;

        UpdateWindowPosition();
    }

    private void UpdateWindowPosition()
    {
        double displayScale =
            _creature.DisplayScale;

        double extraWidth =
            _creature.SpriteWidth *
            (displayScale - 1);

        double extraHeight =
            _creature.CurrentFootY *
            (displayScale - 1);

        Left =
            _creature.X -
            (extraWidth / 2.0);

        Top =
            _creature.Y -
            extraHeight;
    }

    public void BringCreatureToFront()
    {
        if (Topmost)
        {
            Topmost = false;
            Topmost = true;
        }
    }

    public void RefreshTopmost(bool shouldBeTopmost)
    {
        if (!shouldBeTopmost)
        {
            Topmost = false;
            return;
        }

        Topmost = false;
        Topmost = true;
    }

    private void RequestContextAction(
        CreatureContextMenuAction action)
    {
        ContextActionRequested?.Invoke(
            this,
            action);
    }

    private void OpenContextMenu()
    {
        _contextMenuController.Open(
            _uiScale,
            this,
            RequestContextAction,
            () =>
                PutAwayRequested?.Invoke(
                    this));
    }

    private void OnMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        _dragController.Begin(
            new Point(
                _creature.PickupAnchor.X *
                    _creature.DisplayScale,
                _creature.PickupAnchor.Y *
                    _creature.DisplayScale));

        _creature.OnPickedUp();

        CaptureMouse();
    }

    private void OnMouseRightButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        e.Handled = true;

        OpenContextMenu();
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_dragController.IsDragging)
            return;

        //_creature.OnPickedUp();

        var source = PresentationSource.FromVisual(this);

        if (source?.CompositionTarget is null)
            return;

        var mousePixels = System.Windows.Forms.Control.MousePosition;

        var mouseDip = source.CompositionTarget.TransformFromDevice.Transform(
            new Point(mousePixels.X, mousePixels.Y));

        double x =
            mouseDip.X - _dragController.DragOffset.X;

        double y =
            mouseDip.Y - _dragController.DragOffset.Y;

        var monitor =
            _surfaceManager.GetMonitorBoundsUnderCursor();

        x = Math.Clamp(
            x,
            monitor.Left,
            monitor.Right - Width);

        y = Math.Clamp(
            y,
            monitor.Top,
            monitor.Bottom - Height);

        double displayScale =
            _creature.DisplayScale;

        double extraWidth =
            _creature.SpriteWidth *
            (displayScale - 1);

        double extraHeight =
            _creature.CurrentFootY *
            (displayScale - 1);

        double creatureX =
            x +
            (extraWidth / 2.0);

        double creatureY =
            y +
            extraHeight;

        _creature.DragTo(
            creatureX,
            creatureY);

        Left = x;
        Top = y;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragController.End();
        ReleaseMouseCapture();
        _creature.Release();
    }

}