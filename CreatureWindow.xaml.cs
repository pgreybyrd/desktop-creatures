using Desktop_Creatures.Config;
using Desktop_Creatures.Creatures;
using Desktop_Creatures.Utilities;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures;

public partial class CreatureWindow : Window
{
    private readonly Creature _creature;

    private bool _isDragging;
    private System.Windows.Point _dragOffset;

    public Creature GetCreature() => _creature;

    public CreatureWindow(Creature creature)
    {
        InitializeComponent();

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;

        _creature = creature;

        int scale = _creature.Scale;

        //SizeToContent = SizeToContent.WidthAndHeight;

        CreatureImage.Width = _creature.SpriteWidth;
        CreatureImage.Height = _creature.SpriteHeight;

        //CreatureImage.Stretch = Stretch.None;

        Width = _creature.SpriteWidth;// * scale;
        Height = _creature.SpriteHeight;// * scale;

        CreatureImage.Source = _creature.CurrentFrame;

        Left = _creature.X;
        Top = _creature.Y;
    }
    public void UpdateCreature()
    {
        if (_isDragging)
            return;

        _creature.Update();

        if (Math.Abs(_creature.SpeedX) > 0.01)
        {
            bool movingRight = _creature.SpeedX > 0;

            FlipTransform.ScaleX = _creature.SpriteFacesRight == movingRight
                ? 1
                : -1;
        }

        if (CreatureImage.Source != _creature.CurrentFrame)
            CreatureImage.Source = _creature.CurrentFrame;

        Logger.LogDebug(
            DebugCategory.Window,
            $"Window: Action={_creature.CurrentAction}, " +
            $"Sprite={_creature.SpriteWidth}x{_creature.SpriteHeight}, " +
            $"Window={Width}x{Height}, " +
            $"Image={CreatureImage.Width}x{CreatureImage.Height}, " +
            $"CurrentFrame hash = {_creature.CurrentFrame.GetHashCode()}");

        Left = _creature.X;
        Top = _creature.Y;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _dragOffset = e.GetPosition(this);
        CaptureMouse();
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isDragging)
            return;

        var source = PresentationSource.FromVisual(this);

        if (source?.CompositionTarget is null)
            return;

        var mousePixels = System.Windows.Forms.Control.MousePosition;

        var mouseDip = source.CompositionTarget.TransformFromDevice.Transform(
            new System.Windows.Point(mousePixels.X, mousePixels.Y));

        double x = mouseDip.X - _dragOffset.X;
        double y = mouseDip.Y - _dragOffset.Y;

        _creature.DragTo(x, y);

        Left = x;
        Top = y;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        ReleaseMouseCapture();
        _creature.Release();
    }

}