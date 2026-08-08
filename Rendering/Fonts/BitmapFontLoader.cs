using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Rendering.Fonts;

public static class BitmapFontLoader
{
    public static BitmapFont Load(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);

        var config = JsonSerializer.Deserialize<BitmapFontConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException(
                $"Could not deserialize bitmap font config: {jsonPath}");

        string directory =
            Path.GetDirectoryName(jsonPath)
            ?? string.Empty;

        string imagePath =
            Path.Combine(directory, config.Image);

        var atlas = LoadImage(imagePath);

        var glyphs = config.Glyphs.Select(
            pair => new BitmapGlyph
            {
                Character = pair.Key[0],

                Source = new Int32Rect(
                    pair.Value.X,
                    pair.Value.Y,
                    pair.Value.Width,
                    pair.Value.Height),

                XOffset = pair.Value.XOffset,
                YOffset = pair.Value.YOffset,
                XAdvance = pair.Value.XAdvance
            });

        return new BitmapFont(
            config.Name,
            atlas,
            config.LineHeight,
            config.Baseline,
            glyphs);
    }

    private static BitmapImage LoadImage(string path)
    {
        var image = new BitmapImage();

        image.BeginInit();

        image.UriSource = new Uri(
            Path.GetFullPath(path),
            UriKind.Absolute);

        image.CacheOption = BitmapCacheOption.OnLoad;

        image.EndInit();
        image.Freeze();

        return image;
    }

    private sealed class BitmapFontConfig
    {
        public string Name { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

        public int LineHeight { get; set; }

        public int Baseline { get; set; }

        public Dictionary<string, GlyphConfig> Glyphs { get; set; } = new();
    }

    private sealed class GlyphConfig
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public int XAdvance { get; set; }
    }
}