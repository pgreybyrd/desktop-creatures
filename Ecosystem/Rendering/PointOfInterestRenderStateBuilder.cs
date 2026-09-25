using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.World;
using PixelRecolor.Wpf;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Ecosystem.Rendering;

public sealed class PointOfInterestRenderStateBuilder
{
    private readonly Dictionary<
        Guid,
        CachedPoiVisual>
            _visuals = [];

    public EcosystemRenderItem Build(
        PointOfInterest poi)
    {
        BitmapSource image =
            GetImage(poi);

        double scale =
            poi.AppSettings.Scale;

        return new EcosystemRenderItem
        {
            EntityId =
                poi.Id,

            EntityKind =
                EcosystemEntityKind.PointOfInterest,

            Image =
                image,

            WorldBounds =
                new Rect(
                    poi.Position.X,
                    poi.Position.Y,
                    poi.Settings.Width * scale,
                    poi.Settings.Height * scale),

            IsMirrored =
                false,

            IsInteractive =
                true,

            // Temporary world ordering.
            ZIndex =
                -10
        };
    }

    private BitmapSource GetImage(
        PointOfInterest poi)
    {
        if (_visuals.TryGetValue(
                poi.Id,
                out CachedPoiVisual? cached) &&
            cached.IsEnabled ==
                poi.IsEnabled)
        {
            return cached.Image;
        }

        BitmapSource image =
            LoadImage(poi);

        _visuals[poi.Id] =
            new CachedPoiVisual(
                poi.IsEnabled,
                image);

        return image;
    }

    private static BitmapSource LoadImage(
        PointOfInterest poi)
    {
        bool useEmpty =
            !poi.IsEnabled &&
            poi.Settings.EmptyAssetPath is not null;

        string assetPath =
            useEmpty
                ? poi.Settings.EmptyAssetPath!
                : poi.Settings.AssetPath;

        string? maskPath =
            useEmpty
                ? poi.Settings.EmptyMaskPath
                : poi.Settings.MaskPath;

        if (!string.IsNullOrWhiteSpace(
                poi.Settings.FrameName))
        {
            SpriteSheet sheet =
                SpriteSheetLoader.Load(
                    assetPath,
                    poi.Settings.MetadataPath);

            return sheet
                .GetFrame(
                    poi.Settings.FrameName)
                .Image;
        }

        BitmapSource source =
            AssetImageLoader.Load(
                assetPath);

        if (maskPath is null)
            return source;

        BitmapSource mask =
            AssetImageLoader.Load(
                maskPath);

        return BitmapRecolorer.RecolorGrayscale(
            source,
            mask,
            hue: 285,
            saturation: 0.8);
    }

    private sealed record CachedPoiVisual(
        bool IsEnabled,
        BitmapSource Image);
}