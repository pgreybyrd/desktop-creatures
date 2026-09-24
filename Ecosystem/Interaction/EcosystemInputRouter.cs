using Desktop_Creatures.Ecosystem.Rendering;

using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Interaction;

public sealed class EcosystemInputRouter
{
    private readonly EcosystemRenderer
        _renderer;

    private readonly Func<
        IReadOnlyList<EcosystemRenderItem>>
            _getRenderItems;

    public EcosystemInputRouter(
        Func<IReadOnlyList<EcosystemRenderItem>>
            getRenderItems)
    {
        _getRenderItems =
            getRenderItems;
    }

    public EcosystemRenderItem? HitTest(
        Point worldPoint)
    {
        foreach (EcosystemRenderItem item in
                 _getRenderItems()
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
        //got rid of alpha stuff for now! 

        return true;
    }
}