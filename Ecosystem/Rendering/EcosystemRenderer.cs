using Desktop_Creatures.World.Surfaces;
using System.Windows;

namespace Desktop_Creatures.Ecosystem.Rendering;

public sealed class EcosystemRenderer
{
    private readonly SurfaceManager
        _surfaceManager;

    private readonly List<EcosystemSurface>
        _surfaces = [];

    public IReadOnlyList<EcosystemSurface>
        Surfaces =>
            _surfaces;

    public EcosystemRenderer(
        SurfaceManager surfaceManager)
    {
        _surfaceManager =
            surfaceManager;
    }

    public void CreateSurfaces()
    {
        CloseSurfaces();

        foreach (System.Drawing.Rectangle bounds in
                 _surfaceManager.GetMonitorBounds())
        {
            var surface =
                new EcosystemSurface(
                    new Rect(
                        bounds.Left,
                        bounds.Top,
                        bounds.Width,
                        bounds.Height));

            _surfaces.Add(
                surface);

            surface.Show();
        }
    }

    public void CloseSurfaces()
    {
        foreach (EcosystemSurface surface in
                 _surfaces)
        {
            surface.Close();
        }

        _surfaces.Clear();
    }
}