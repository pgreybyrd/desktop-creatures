using Desktop_Creatures.Tools.Images;
using Desktop_Creatures.Tools.Text;
using PixelRecolor.Core;
using PixelRecolor.Wpf;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public static class CreatureAppearanceFactory
{
    public static CreatureAppearance Create(
        CreatureDefinition definition,
        CreatureAppearanceTraits traits)
    {
        string creatureFolder =
            $"{definition.AssetFolder}/Appearance";

        string creatureId =
            definition.Id;

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

        RegionPalette? palette = null;

        if (!string.IsNullOrWhiteSpace(traits.Palette))
        {
            string paletteJson =
                AssetTextLoader.Load(
                    $"{creatureFolder}/Palettes/{traits.Palette}.json");

            palette =
                RegionPaletteLoader.Load(
                    paletteJson);
        }

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
        CreatureDefinition definition,
        string appearanceId)
    {
        string creatureFolder =
            $"{definition.AssetFolder}/Appearance";

        string appearanceJson =
            AssetTextLoader.Load(
                $"{creatureFolder}/Appearances/{appearanceId}.json");

        CreatureAppearanceDefinition appearanceDefinition =
            CreatureAppearanceLoader.Load(
                appearanceJson);

        return appearanceDefinition.Traits;
    }
}