using Desktop_Creatures.Rendering.Fonts;
using System.IO;

namespace Desktop_Creatures.Tools.Fonts;

public static class FontTool
{
    public static BitmapFont Generate(FontDefinition font)
    {
        string projectRoot = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                @"..\..\..\"));

        string fontsFolder = Path.Combine(
            projectRoot,
            "Assets",
            "Fonts");

        string imagePath = Path.Combine(
            fontsFolder,
            $"font-{font.Name}.png");

        string outputPath = Path.Combine(
            fontsFolder,
            $"font-{font.Name}.json");

        BitmapFontJsonGenerator.Generate(
            imagePath,
            outputPath,
            font.Name,
            font.CharacterRows,
            font.Baseline,
            font.BaselineAdjustments,
            font.SpaceAdvance,
            font.GlyphSpacing);

        return BitmapFontLoader.Load(outputPath);
    }
}