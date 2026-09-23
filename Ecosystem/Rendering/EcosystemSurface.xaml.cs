using Desktop_Creatures.Creatures;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Rendering;

public partial class EcosystemSurface : Window
{
    private readonly Dictionary<Guid, Image>
        _entityImages = [];

    public Rect WorldBounds { get; }

    public EcosystemSurface(
        Rect worldBounds)
    {
        InitializeComponent();

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