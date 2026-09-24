using Desktop_Creatures.Creatures;
using Desktop_Creatures.Creatures.Interaction;
using Desktop_Creatures.Ecosystem.Interaction;
using Desktop_Creatures.UI.RightClick;
using Desktop_Creatures.Windowing;
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
        ZOrderManager zOrderManager)
    {
        _surfaceManager =
            surfaceManager;

        _creatureManager =
            creatureManager;

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

            if (renderItem is null)
            {
                continue;
            }

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

        RemoveInactiveCreatures();
    }

    private void RemoveInactiveCreatures()
    {
        HashSet<Guid> activeCreatureIds =
            _creatureManager
                .ActiveCreatures
                .Select(creature => creature.Id)
                .ToHashSet();

        foreach (EcosystemSurface surface in
                 _surfaces)
        {
            Guid[] inactiveCreatureIds =
                surface.EntityIds
                    .Where(
                        id =>
                            !activeCreatureIds.Contains(
                                id))
                    .ToArray();

            foreach (Guid creatureId in
                     inactiveCreatureIds)
            {
                surface.RemoveEntity(
                    creatureId);
            }
        }
    }
}