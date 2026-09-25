using Desktop_Creatures.Creatures;
using Desktop_Creatures.Ecosystem.Rendering;
using Desktop_Creatures.World;

using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Interaction;

public sealed class EcosystemInputRouter
{
    private readonly Func<
        IReadOnlyList<EcosystemRenderItem>>
            _getRenderItems;

    private readonly Func<Guid, Creature?>
        _findCreature;

    private readonly Func<Guid, PointOfInterest?>
        _findPointOfInterest;

    public IReadOnlyList<EcosystemRenderItem>
        RenderItems =>
            _getRenderItems();

    public EcosystemInputRouter(
        Func<IReadOnlyList<EcosystemRenderItem>>
            getRenderItems,
        Func<Guid, Creature?>
            findCreature,
        Func<Guid, PointOfInterest?>
            findPointOfInterest)
    {
        _getRenderItems =
            getRenderItems;

        _findCreature =
            findCreature;

        _findPointOfInterest =
            findPointOfInterest;
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

    public PointOfInterest? FindPointOfInterest(
        Guid entityId)
    {
        return _findPointOfInterest(
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