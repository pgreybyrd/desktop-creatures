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

        byte[] alphaMask =
            CreateAlphaMask(
                frame);

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

            //AlphaMask =
            //    alphaMask,

            //PixelWidth =
            //    frame.PixelWidth,

            //PixelHeight =
            //    frame.PixelHeight,
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

    private static byte[] CreateAlphaMask(
        BitmapSource bitmap)
    {
        int width =
            bitmap.PixelWidth;

        int height =
            bitmap.PixelHeight;

        int stride =
            width * 4;

        byte[] pixels =
            new byte[
                stride * height];

        BitmapSource source =
            bitmap.Format ==
            System.Windows.Media.PixelFormats.Bgra32
                ? bitmap
                : new System.Windows.Media.Imaging
                    .FormatConvertedBitmap(
                        bitmap,
                        System.Windows.Media
                            .PixelFormats.Bgra32,
                        null,
                        0);

        source.CopyPixels(
            pixels,
            stride,
            0);

        byte[] alphaMask =
            new byte[
                width * height];

        for (int y = 0;
             y < height;
             y++)
        {
            for (int x = 0;
                 x < width;
                 x++)
            {
                int pixelIndex =
                    (y * stride) +
                    (x * 4);

                int alphaIndex =
                    (y * width) +
                    x;

                alphaMask[
                    alphaIndex] =
                        pixels[
                            pixelIndex + 3];
            }
        }

        return alphaMask;
    }
}