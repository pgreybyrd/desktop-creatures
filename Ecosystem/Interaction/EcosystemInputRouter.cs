using Desktop_Creatures.Ecosystem.Rendering;

using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Interaction;

public sealed class EcosystemInputRouter
{
    private readonly EcosystemRenderer
        _renderer;

    public EcosystemInputRouter(
        EcosystemRenderer renderer)
    {
        _renderer =
            renderer;
    }

    public EcosystemRenderItem? HitTest(
        Point worldPoint)
    {
        foreach (EcosystemRenderItem item in
                 _renderer.RenderItems
                     .OrderByDescending(
                         item => item.ZIndex))
        {
            if (!item.IsInteractive)
            {
                continue;
            }

            if (!item.WorldBounds.Contains(
                    worldPoint))
            {
                continue;
            }

            if (!IsOpaqueAt(
                    item,
                    worldPoint))
            {
                continue;
            }

            return item;
        }

        return null;
    }

    private static bool IsOpaqueAt(
        EcosystemRenderItem item,
        Point worldPoint)
    {
        // Stage 2:
        // convert the world point into the source
        // bitmap's pixel coordinates and inspect alpha.

        return true;
    }
}