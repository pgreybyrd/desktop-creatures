using System.IO;

namespace Desktop_Creatures.Tools.Fonts;

public static class MagicalStandardFontTool
{
    public static void Generate()
    {
        string projectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));

        string imagePath = Path.Combine(
            projectRoot,
            "Assets",
            "Fonts",
            "font-MagicalStandard.png");

        string outputPath = Path.Combine(
            projectRoot,
            "Assets",
            "Fonts",
            "font-MagicalStandard.json");

        string[] characterRows =
        {
            "{}[]|",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZbdfghjklpqy0123456789!£$&()@?/\\",
            "#%i;*",
            "acemnorstuvwxz+:",
            "<>",
            "=,",
            "'`^~",
            "_-."
        };

        BitmapFontJsonGenerator.Generate(
            imagePath,
            outputPath,
            "MagicalStandard",
            characterRows);
    }
}