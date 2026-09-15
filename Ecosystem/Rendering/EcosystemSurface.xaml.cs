using Desktop_Creatures.Creatures;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Rendering;

public partial class EcosystemSurface : Window
{
    private readonly Dictionary<Guid, Image>
        _creatureImages = [];

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

    public void DrawCreature(
        Creature creature,
        double worldLeft,
        double worldTop)
    {
        if (!_creatureImages.TryGetValue(
                creature.Id,
                out Image? image))
        {
            image =
                new Image
                {
                    Stretch =
                        Stretch.Fill,

                    SnapsToDevicePixels =
                        true
                };

            RenderOptions.SetBitmapScalingMode(
                image,
                BitmapScalingMode.NearestNeighbor);

            _creatureImages[
                creature.Id] =
                    image;

            EcosystemCanvas.Children.Add(
                image);
        }

        BitmapSource? frame =
            creature.CurrentFrame;

        if (image.Source != frame)
        {
            image.Source =
                frame;
        }

        double displayScale =
            creature.DisplayScale;

        image.Width =
            creature.VisualWidth *
            displayScale;

        image.Height =
            creature.VisualHeight *
            displayScale;

        image.RenderTransformOrigin =
            new Point(
                0.5,
                0.5);

        image.RenderTransform =
            new ScaleTransform(
                creature.IsVisualMirrored
                    ? -1
                    : 1,
                1);

        Canvas.SetLeft(
            image,
            worldLeft -
                WorldBounds.Left);

        Canvas.SetTop(
            image,
            worldTop -
                WorldBounds.Top);
    }

    public void RemoveCreature(
        Guid creatureId)
    {
        if (!_creatureImages.Remove(
                creatureId,
                out Image? image))
        {
            return;
        }

        EcosystemCanvas.Children.Remove(
            image);
    }

    public bool ContainsCreature(
        Guid creatureId)
    {
        return _creatureImages.ContainsKey(
            creatureId);
    }

    public IReadOnlyCollection<Guid>
        CreatureIds =>
            _creatureImages.Keys;
}