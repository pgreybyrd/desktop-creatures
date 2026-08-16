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
        CreatureAppearanceTraits traits)
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

        string paletteJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/Palettes/{traits.Palette}.json");

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
                traits,

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
            traits,
            spriteSheet);
    }

    public static CreatureAppearanceTraits LoadTraits(
        string creatureType,
        string appearanceId)
    {
        string creatureFolder =
            $"Assets/Creatures/{creatureType}/Appearance";

        string appearanceJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/Appearances/{appearanceId}.json");

        CreatureAppearanceDefinition definition =
            CreatureAppearanceLoader.Load(
                appearanceJson);

        return definition.Traits;
    }
}