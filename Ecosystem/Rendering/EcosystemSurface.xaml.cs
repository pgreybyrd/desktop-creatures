using System.Windows;

namespace Desktop_Creatures.Ecosystem.Rendering;

public partial class EcosystemSurface : Window
{
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
}