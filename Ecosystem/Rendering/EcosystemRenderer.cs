using Desktop_Creatures.Creatures;
using Desktop_Creatures.Creatures.Interaction;
using Desktop_Creatures.Ecosystem.Interaction;
using Desktop_Creatures.UI.RightClick;
using Desktop_Creatures.Windowing;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using System.Windows;

namespace Desktop_Creatures.Ecosystem.Rendering;

public sealed class EcosystemRenderer
{
    private readonly SurfaceManager
        _surfaceManager;

    private readonly CreatureManager
        _creatureManager;

    private readonly ZOrderManager
        _zOrderManager;

    private readonly PointOfInterestManager
        _pointOfInterestManager;

    private readonly PointOfInterestRenderStateBuilder
        _pointOfInterestRenderStateBuilder =
            new();

    private readonly List<EcosystemSurface>
        _surfaces = [];

    private readonly List<EcosystemRenderItem>
        _renderItems = [];

    public IReadOnlyList<EcosystemRenderItem>
        RenderItems =>
            _renderItems;

    public IReadOnlyList<EcosystemSurface>
        Surfaces =>
            _surfaces;

    public EcosystemRenderer(
        SurfaceManager surfaceManager,
        CreatureManager creatureManager,
        PointOfInterestManager pointOfInterestManager,
        ZOrderManager zOrderManager)
    {
        _surfaceManager =
            surfaceManager;

        _creatureManager =
            creatureManager;

        _pointOfInterestManager =
            pointOfInterestManager;

        _zOrderManager =
            zOrderManager;
    }

    public void CreateSurfaces(
        EcosystemInputRouter inputRouter,
        CreatureDragController dragController,
        CreatureContextMenuController contextMenuController,
        int uiScale,
        Action<Creature, CreatureContextMenuAction>
            contextActionRequested,
        Action<Guid> putAwayRequested)
    {
        CloseSurfaces();

        foreach (Rectangle bounds in
                 _surfaceManager.GetMonitorBounds())
        {
            var surface =
                new EcosystemSurface(
                    new Rect(
                        bounds.Left,
                        bounds.Top,
                        bounds.Width,
                        bounds.Height),
                    inputRouter,
                    dragController,
                    contextMenuController,
                    uiScale,
                    contextActionRequested,
                    putAwayRequested);

            _surfaces.Add(
                surface);

            _zOrderManager.Register(
                surface,
                ZOrderManager.WindowLayer.Ecosystem);

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

    public void Render()
    {
        _renderItems.Clear();

        foreach (Creature creature in
                 _creatureManager.ActiveCreatures)
        {
            EcosystemRenderItem? renderItem =
                CreatureRenderStateBuilder.Build(
                    creature);

            if (renderItem is not null)
            {
                RenderItem(
                    renderItem);
            }
        }

        foreach (PointOfInterest poi in
                 _pointOfInterestManager.Points)
        {
            EcosystemRenderItem renderItem =
                _pointOfInterestRenderStateBuilder.Build(
                    poi);

            RenderItem(
                renderItem);
        }

        RemoveInactiveEntities();
    }

    private void RenderItem(
        EcosystemRenderItem renderItem)
    {
        _renderItems.Add(
            renderItem);

        foreach (EcosystemSurface surface in
                 _surfaces)
        {
            bool intersectsSurface =
                surface.WorldBounds.IntersectsWith(
                    renderItem.WorldBounds);

            if (intersectsSurface)
            {
                surface.Draw(
                    renderItem);

                continue;
            }

            if (surface.ContainsEntity(
                    renderItem.EntityId))
            {
                surface.RemoveEntity(
                    renderItem.EntityId);
            }
        }
    }

    private void RemoveInactiveEntities()
    {
        HashSet<Guid> activeEntityIds =
            _renderItems
                .Select(item => item.EntityId)
                .ToHashSet();

        foreach (EcosystemSurface surface in
                 _surfaces)
        {
            Guid[] inactiveEntityIds =
                surface.EntityIds
                    .Where(
                        id =>
                            !activeEntityIds.Contains(id))
                    .ToArray();

            foreach (Guid entityId in
                     inactiveEntityIds)
            {
                surface.RemoveEntity(
                    entityId);
            }
        }
    }
}