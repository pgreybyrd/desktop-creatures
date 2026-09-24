using Desktop_Creatures.Creatures;
using Desktop_Creatures.Creatures.Interaction;
using Desktop_Creatures.Ecosystem.Interaction;
using Desktop_Creatures.UI.RightClick;
using Desktop_Creatures.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Desktop_Creatures.Ecosystem.Rendering;

public partial class EcosystemSurface : Window
{
    private readonly Dictionary<Guid, Image>
        _entityImages = [];

    private readonly EcosystemInputRouter
        _inputRouter;

    private readonly CreatureDragController
        _dragController;

    private Creature? _draggedCreature;

    private readonly CreatureContextMenuController
        _contextMenuController;

    private readonly int _uiScale;

    private readonly Action<
        Creature,
        CreatureContextMenuAction>
            _contextActionRequested;

    private readonly Action<Guid>
        _putAwayRequested;

    public Rect WorldBounds { get; }

    public EcosystemSurface(
        Rect worldBounds,
        EcosystemInputRouter inputRouter,
        CreatureDragController dragController,
        CreatureContextMenuController contextMenuController,
        int uiScale,
        Action<Creature, CreatureContextMenuAction>
            contextActionRequested,
        Action<Guid> putAwayRequested)
    {
        InitializeComponent();

        _inputRouter =
            inputRouter;

        _dragController =
            dragController;

        _contextMenuController =
            contextMenuController;

        _uiScale =
            uiScale;

        _contextActionRequested =
            contextActionRequested;

        _putAwayRequested =
            putAwayRequested;

        EcosystemCanvas.MouseRightButtonDown +=
            EcosystemCanvas_MouseRightButtonDown;

        EcosystemCanvas.MouseLeftButtonDown +=
            EcosystemCanvas_MouseLeftButtonDown;

        EcosystemCanvas.MouseMove +=
            EcosystemCanvas_MouseMove;

        EcosystemCanvas.MouseLeftButtonUp +=
            EcosystemCanvas_MouseLeftButtonUp;

        WorldBounds =
            worldBounds;

        Left =
            worldBounds.Left;

        Top =
            worldBounds.Top;

        Width =
            worldBounds.Width;

        Height =
            worldBounds.Height;
    }

    public void Draw(
        EcosystemRenderItem renderItem)
    {
        if (!_entityImages.TryGetValue(
                renderItem.EntityId,
                out Image? image))
        {
            image =
                new Image
                {
                    Stretch =
                        Stretch.Fill,

                    SnapsToDevicePixels =
                        true,

                    IsHitTestVisible =
                        false
                };

            RenderOptions.SetBitmapScalingMode(
                image,
                BitmapScalingMode.NearestNeighbor);

            _entityImages[
                renderItem.EntityId] =
                    image;

            EcosystemCanvas.Children.Add(
                image);
        }

        if (image.Source != renderItem.Image)
        {
            image.Source =
                renderItem.Image;
        }

        image.Width =
            renderItem.WorldBounds.Width;

        image.Height =
            renderItem.WorldBounds.Height;

        image.RenderTransformOrigin =
            new Point(
                0.5,
                0.5);

        image.RenderTransform =
            new ScaleTransform(
                renderItem.IsMirrored
                    ? -1
                    : 1,
                1);

        Canvas.SetLeft(
            image,
            renderItem.WorldBounds.Left -
                WorldBounds.Left);

        Canvas.SetTop(
            image,
            renderItem.WorldBounds.Top -
                WorldBounds.Top);

        Panel.SetZIndex(
            image,
            renderItem.ZIndex);
    }

    private void EcosystemCanvas_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        Point localPoint =
            e.GetPosition(EcosystemCanvas);

        Point worldPoint =
            new(
                WorldBounds.Left +
                    localPoint.X,
                WorldBounds.Top +
                    localPoint.Y);

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] CLICK " +
            $"world=({worldPoint.X:F0}, {worldPoint.Y:F0})");

        foreach (EcosystemRenderItem item in
                 _inputRouter.RenderItems)
        {
            Logger.LogDebug(
                DebugCategory.Input,
                $"    item={item.EntityId} " +
                $"bounds=(" +
                $"{item.WorldBounds.Left:F0}," +
                $"{item.WorldBounds.Top:F0}," +
                $"{item.WorldBounds.Width:F0}," +
                $"{item.WorldBounds.Height:F0}) " +
                $"interactive={item.IsInteractive} " +
                $"z={item.ZIndex}");
        }

        EcosystemRenderItem? hit =
            _inputRouter.HitTest(
                worldPoint);

        if (hit is null)
        {
            Logger.LogDebug(
                DebugCategory.Input,
                $"[Ecosystem] Hit test: empty " +
                $"world=({worldPoint.X:F0}, " +
                $"{worldPoint.Y:F0})");

            return;
        }

        Creature? creature =
            _inputRouter.FindCreature(
                hit.EntityId);

        if (creature is null)
        {
            Logger.LogDebug(
                DebugCategory.Input,
                $"[Ecosystem] Entity " +
                $"{hit.EntityId} has no creature.");

            return;
        }

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] Resolved creature " +
            $"{creature.Id}");

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] Hit test: entity " +
            $"{hit.EntityId} " +
            $"world=({worldPoint.X:F0}, " +
            $"{worldPoint.Y:F0})");

        _draggedCreature =
            creature;

        _dragController.Begin(
            creature);

        EcosystemCanvas.CaptureMouse();

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] Drag begin " +
            $"creature={creature.Id}");
    }

    private void EcosystemCanvas_MouseMove(
        object sender,
        WpfMouseEventArgs e)
    {
        if (_draggedCreature is null ||
            !_dragController.IsDragging)
        {
            return;
        }

        Point localPoint =
            e.GetPosition(EcosystemCanvas);

        Point worldPoint =
            new(
                WorldBounds.Left +
                    localPoint.X,
                WorldBounds.Top +
                    localPoint.Y);

        double width =
            _draggedCreature.SpriteWidth *
            _draggedCreature.DisplayScale;

        double height =
            _draggedCreature.SpriteHeight *
            _draggedCreature.DisplayScale;

        _dragController.UpdateDrag(
            _draggedCreature,
            worldPoint,
            width,
            height);
    }

    private void EcosystemCanvas_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_draggedCreature is null)
        {
            return;
        }

        Creature creature =
            _draggedCreature;

        _draggedCreature =
            null;

        EcosystemCanvas.ReleaseMouseCapture();

        _dragController.End(
            creature);

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] Drag end " +
            $"creature={creature.Id}");
    }

    private void EcosystemCanvas_MouseRightButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        Point localPoint =
            e.GetPosition(
                EcosystemCanvas);

        Point worldPoint =
            new(
                WorldBounds.Left +
                    localPoint.X,
                WorldBounds.Top +
                    localPoint.Y);

        EcosystemRenderItem? hit =
            _inputRouter.HitTest(
                worldPoint);

        if (hit is null)
        {
            return;
        }

        Creature? creature =
            _inputRouter.FindCreature(
                hit.EntityId);

        if (creature is null)
        {
            return;
        }

        e.Handled =
            true;

        _contextMenuController.Open(
            _uiScale,
            this,
            action =>
                _contextActionRequested(
                    creature,
                    action),
            () =>
                _putAwayRequested(
                    creature.Id));

        Logger.LogDebug(
            DebugCategory.Input,
            $"[Ecosystem] Context menu open " +
            $"creature={creature.Id}");
    }

    public void RemoveEntity(
        Guid entityId)
    {
        if (!_entityImages.Remove(
                entityId,
                out Image? image))
        {
            return;
        }

        EcosystemCanvas.Children.Remove(
            image);
    }

    public bool ContainsEntity(
        Guid entityId)
    {
        return _entityImages.ContainsKey(
            entityId);
    }

    public IReadOnlyCollection<Guid>
        EntityIds =>
            _entityImages.Keys;
}