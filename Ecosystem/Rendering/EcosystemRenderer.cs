using Desktop_Creatures.Creatures;
using Desktop_Creatures.World.Surfaces;
using System.Windows;

namespace Desktop_Creatures.Ecosystem.Rendering;

public sealed class EcosystemRenderer
{
    private readonly SurfaceManager
        _surfaceManager;

    private readonly CreatureManager
        _creatureManager;

    private readonly List<EcosystemSurface>
        _surfaces = [];

    public IReadOnlyList<EcosystemSurface>
        Surfaces =>
            _surfaces;

    public EcosystemRenderer(
        SurfaceManager surfaceManager,
        CreatureManager creatureManager)
    {
        _surfaceManager =
            surfaceManager;

        _creatureManager =
            creatureManager;
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

    public void Render()
    {
        foreach (Creature creature in
                 _creatureManager.ActiveCreatures)
        {
            GetCreatureVisualPosition(
                creature,
                out double worldLeft,
                out double worldTop);

            Rect visualBounds =
                GetCreatureVisualBounds(
                    creature,
                    worldLeft,
                    worldTop);

            foreach (EcosystemSurface surface in
                     _surfaces)
            {
                bool intersectsSurface =
                    surface.WorldBounds.IntersectsWith(
                        visualBounds);

                if (intersectsSurface)
                {
                    surface.DrawCreature(
                        creature,
                        worldLeft,
                        worldTop);

                    continue;
                }

                if (surface.ContainsCreature(
                        creature.Id))
                {
                    surface.RemoveCreature(
                        creature.Id);
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
                surface.CreatureIds
                    .Where(
                        id =>
                            !activeCreatureIds.Contains(
                                id))
                    .ToArray();

            foreach (Guid creatureId in
                     inactiveCreatureIds)
            {
                surface.RemoveCreature(
                    creatureId);
            }
        }
    }

    private static void GetCreatureVisualPosition(
        Creature creature,
        out double worldLeft,
        out double worldTop)
    {
        double displayScale =
            creature.DisplayScale;

        if (creature.HasBodyBounds)
        {
            double logicalCenterX =
                creature.X +
                (creature.SpriteWidth / 2.0);

            double logicalCenterY =
                creature.Y +
                (creature.SpriteHeight / 2.0);

            worldLeft =
                logicalCenterX -
                (creature.VisualBodyCenterX *
                 displayScale);

            worldTop =
                logicalCenterY -
                (creature.VisualBodyCenterY *
                 displayScale);

            return;
        }

        double extraWidth =
            creature.SpriteWidth *
            (displayScale - 1);

        double extraHeight =
            creature.CurrentFootY *
            (displayScale - 1);

        worldLeft =
            creature.X -
            (extraWidth / 2.0);

        worldTop =
            creature.Y -
            extraHeight;
    }

    private static Rect GetCreatureVisualBounds(
        Creature creature,
        double worldLeft,
        double worldTop)
    {
        double displayScale =
            creature.DisplayScale;

        return new Rect(
            worldLeft,
            worldTop,
            creature.VisualWidth *
                displayScale,
            creature.VisualHeight *
                displayScale);
    }
}