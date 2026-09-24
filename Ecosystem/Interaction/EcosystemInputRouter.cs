using Desktop_Creatures.Creatures;
using Desktop_Creatures.Ecosystem.Rendering;

using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Interaction;

public sealed class EcosystemInputRouter
{
    private readonly Func<
        IReadOnlyList<EcosystemRenderItem>>
            _getRenderItems;

    private readonly Func<Guid, Creature?>
        _findCreature;

    public IReadOnlyList<EcosystemRenderItem>
        RenderItems =>
            _getRenderItems();

    public EcosystemInputRouter(
        Func<IReadOnlyList<EcosystemRenderItem>>
            getRenderItems,
        Func<Guid, Creature?>
            findCreature)
    {
        _getRenderItems =
            getRenderItems;

        _findCreature =
            findCreature;
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

    public Creature? FindCreature(
        Guid entityId)
    {
        return _findCreature(
            entityId);
    }

    private static bool IsOpaqueAt(
        EcosystemRenderItem item,
        Point worldPoint)
    {
        //got rid of alpha stuff for now! 

        return true;
    }
}