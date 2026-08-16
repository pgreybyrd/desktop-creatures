using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Tools.Text;
using PixelRecolor.Core;
using PixelRecolor.Wpf;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public static class CreatureAppearanceFactory
{
    public static CreatureAppearance Create(
        string creatureType,
        string variant)
    {
        string creatureFolder =
            $"Assets/Creatures/{creatureType}/Appearance";

        string creatureId =
            creatureType.ToLowerInvariant();

        // Base grayscale spritesheet
        var source =
            AssetImageLoader.Load(
                $"{creatureFolder}/{creatureId}.png");

        // Region mask
        var regionMask =
            AssetImageLoader.Load(
                $"{creatureFolder}/{creatureId}-regions.png");

        // Region definitions
        string regionsJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/{creatureId}-regions.json");

        var regions =
            RegionDefinitionLoader.Load(
                regionsJson);

        // Appearance recipe
        string appearanceJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/Appearances/{variant}.json");

        var appearance =
            CreatureAppearanceLoader.Load(
                appearanceJson);

        // Palette selected by the appearance recipe
        string paletteJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/Palettes/{appearance.Palette}.json");

        var palette =
            RegionPaletteLoader.Load(
                paletteJson);

        // BUILD THE BEAST.
        BitmapSource spriteSheet =
            CreatureAppearanceRenderer.Build(
                source,
                regionMask,
                regions,
                palette,
                appearance,

                patternId =>
                    AssetImageLoader.Load(
                        $"{creatureFolder}/Patterns/{patternId}.png"),

                accessoryId =>
                    AssetImageLoader.Load(
                        $"{creatureFolder}/Accessories/{accessoryId}.png"),

                effectId =>
                    AssetImageLoader.Load(
                        $"{creatureFolder}/Effects/{effectId}.png"));

        return new CreatureAppearance(
            variant,
            spriteSheet);
    }
}