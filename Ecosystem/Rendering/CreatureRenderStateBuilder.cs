using Desktop_Creatures.Creatures;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Ecosystem.Rendering;

public static class CreatureRenderStateBuilder
{
    public static EcosystemRenderItem? Build(
        Creature creature)
    {
        BitmapSource? frame =
            creature.CurrentFrame;

        if (frame is null)
        {
            return null;
        }

        Point visualPosition =
            GetVisualPosition(
                creature);

        double displayScale =
            creature.DisplayScale;

        Rect worldBounds =
            new(
                visualPosition.X,
                visualPosition.Y,
                creature.VisualWidth *
                    displayScale,
                creature.VisualHeight *
                    displayScale);

        return new EcosystemRenderItem
        {
            EntityId =
                creature.Id,

            EntityKind =
                EcosystemEntityKind.Creature,

            Image =
                frame,

            WorldBounds =
                worldBounds,

            IsMirrored =
                creature.IsVisualMirrored,

            IsInteractive =
                true,

            ZIndex =
                0,
        };
    }

    private static Point GetVisualPosition(
        Creature creature)
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

            return new Point(
                logicalCenterX -
                    (creature.VisualBodyCenterX *
                     displayScale),

                logicalCenterY -
                    (creature.VisualBodyCenterY *
                     displayScale));
        }

        double extraWidth =
            creature.SpriteWidth *
            (displayScale - 1);

        double extraHeight =
            creature.CurrentFootY *
            (displayScale - 1);

        return new Point(
            creature.X -
                (extraWidth / 2.0),

            creature.Y -
                extraHeight);
    }
}