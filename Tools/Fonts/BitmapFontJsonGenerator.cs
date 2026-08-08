using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Tools.Fonts;

public static class BitmapFontJsonGenerator
{
    public static void Generate(
        string imagePath,
        string outputPath,
        string fontName,
        string[] characterRows,
        int baseline,
        Dictionary<char, int>? baselineAdjustments = null,
        int spaceAdvance = 4,
        int glyphSpacing = 1)
    {

        BitmapSource bitmap = LoadBitmap(imagePath);

        byte[] pixels = GetPixels(bitmap, out int stride);

        List<PixelRegion> detectedRows =
            FindRows(bitmap, pixels, stride);

        if (detectedRows.Count != characterRows.Length)
        {
            throw new InvalidOperationException(
                $"Font atlas contains {detectedRows.Count} detected rows, " +
                $"but {characterRows.Length} character rows were supplied.");
        }

        var glyphs = new Dictionary<string, GlyphConfig>();

        int lineHeight = detectedRows.Max(row => row.Height);

        for (int rowIndex = 0;
             rowIndex < detectedRows.Count;
             rowIndex++)
        {
            PixelRegion row = detectedRows[rowIndex];
            string characters = characterRows[rowIndex];

            List<PixelRegion> detectedGlyphs =
                FindGlyphsInRow(
                    bitmap,
                    pixels,
                    stride,
                    row);

            if (detectedGlyphs.Count != characters.Length)
            {
                throw new InvalidOperationException(
                    $"Row {rowIndex + 1} contains " +
                    $"{detectedGlyphs.Count} detected glyphs, " +
                    $"but {characters.Length} characters were supplied.\n" +
                    $"Characters: {characters}");
            }

            for (int glyphIndex = 0;
                 glyphIndex < detectedGlyphs.Count;
                 glyphIndex++)
            {
                char character = characters[glyphIndex];
                PixelRegion region = detectedGlyphs[glyphIndex];

                int baselineAdjustment = 0;

                if (baselineAdjustments is not null)
                {
                    baselineAdjustments.TryGetValue(
                        character,
                        out baselineAdjustment);
                }

                int yOffset =
                    baseline - region.Height + baselineAdjustment;

                glyphs[character.ToString()] = new GlyphConfig
                {
                    X = region.X,
                    Y = region.Y,
                    Width = region.Width,
                    Height = region.Height,

                    XOffset = 0,

                    // Preserve vertical position relative to the row.
                    YOffset = yOffset,

                    XAdvance = region.Width + glyphSpacing
                };
            }
        }

        // Space has no artwork, but it still moves the cursor.
        glyphs[" "] = new GlyphConfig
        {
            X = 0,
            Y = 0,
            Width = 0,
            Height = 0,
            XOffset = 0,
            YOffset = 0,
            XAdvance = spaceAdvance
        };

        var config = new FontConfig
        {
            Name = fontName,
            Image = Path.GetFileName(imagePath),
            LineHeight = lineHeight,
            Baseline = baseline,
            Glyphs = glyphs
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json =
            JsonSerializer.Serialize(config, options);

        File.WriteAllText(outputPath, json);
    }

    private static List<PixelRegion> FindRows(
        BitmapSource bitmap,
        byte[] pixels,
        int stride)
    {
        var rows = new List<PixelRegion>();

        bool insideRow = false;
        int rowStart = 0;

        for (int y = 0; y < bitmap.PixelHeight; y++)
        {
            bool hasPixel =
                RowHasVisiblePixel(
                    bitmap,
                    pixels,
                    stride,
                    y);

            if (hasPixel && !insideRow)
            {
                rowStart = y;
                insideRow = true;
            }
            else if (!hasPixel && insideRow)
            {
                rows.Add(new PixelRegion
                {
                    X = 0,
                    Y = rowStart,
                    Width = bitmap.PixelWidth,
                    Height = y - rowStart
                });

                insideRow = false;
            }
        }

        if (insideRow)
        {
            rows.Add(new PixelRegion
            {
                X = 0,
                Y = rowStart,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight - rowStart
            });
        }

        return rows;
    }

    private static List<PixelRegion> FindGlyphsInRow(
        BitmapSource bitmap,
        byte[] pixels,
        int stride,
        PixelRegion row)
    {
        var glyphs = new List<PixelRegion>();

        bool insideGlyph = false;
        int glyphStart = 0;

        for (int x = 0; x < bitmap.PixelWidth; x++)
        {
            bool hasPixel =
                ColumnHasVisiblePixel(
                    pixels,
                    stride,
                    x,
                    row.Y,
                    row.Height);

            if (hasPixel && !insideGlyph)
            {
                glyphStart = x;
                insideGlyph = true;
            }
            else if (!hasPixel && insideGlyph)
            {
                glyphs.Add(
                    FindTightGlyphBounds(
                        pixels,
                        stride,
                        glyphStart,
                        x - 1,
                        row));

                insideGlyph = false;
            }
        }

        if (insideGlyph)
        {
            glyphs.Add(
                FindTightGlyphBounds(
                    pixels,
                    stride,
                    glyphStart,
                    bitmap.PixelWidth - 1,
                    row));
        }

        return glyphs;
    }

    private static PixelRegion FindTightGlyphBounds(
        byte[] pixels,
        int stride,
        int startX,
        int endX,
        PixelRegion row)
    {
        int minY = row.Y + row.Height;
        int maxY = row.Y;

        for (int y = row.Y;
             y < row.Y + row.Height;
             y++)
        {
            for (int x = startX;
                 x <= endX;
                 x++)
            {
                if (!IsVisible(
                    pixels,
                    stride,
                    x,
                    y))
                {
                    continue;
                }

                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
        }

        return new PixelRegion
        {
            X = startX,
            Y = minY,
            Width = endX - startX + 1,
            Height = maxY - minY + 1
        };
    }

    private static bool RowHasVisiblePixel(
        BitmapSource bitmap,
        byte[] pixels,
        int stride,
        int y)
    {
        for (int x = 0; x < bitmap.PixelWidth; x++)
        {
            if (IsVisible(
                pixels,
                stride,
                x,
                y))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ColumnHasVisiblePixel(
        byte[] pixels,
        int stride,
        int x,
        int startY,
        int height)
    {
        for (int y = startY;
             y < startY + height;
             y++)
        {
            if (IsVisible(
                pixels,
                stride,
                x,
                y))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVisible(
        byte[] pixels,
        int stride,
        int x,
        int y)
    {
        int index = y * stride + x * 4;

        byte alpha = pixels[index + 3];

        return alpha > 0;
    }

    private static byte[] GetPixels(
        BitmapSource bitmap,
        out int stride)
    {
        BitmapSource converted =
            new FormatConvertedBitmap(
                bitmap,
                PixelFormats.Bgra32,
                null,
                0);

        stride = converted.PixelWidth * 4;

        byte[] pixels =
            new byte[stride * converted.PixelHeight];

        converted.CopyPixels(
            pixels,
            stride,
            0);

        return pixels;
    }

    private static BitmapImage LoadBitmap(string path)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();

        bitmap.UriSource =
            new Uri(
                Path.GetFullPath(path),
                UriKind.Absolute);

        bitmap.CacheOption =
            BitmapCacheOption.OnLoad;

        bitmap.EndInit();
        bitmap.Freeze();

        return bitmap;
    }

    private sealed class PixelRegion
    {
        public int X { get; init; }
        public int Y { get; init; }

        public int Width { get; init; }
        public int Height { get; init; }
    }

    private sealed class FontConfig
    {
        public string Name { get; set; } = "";
        public string Image { get; set; } = "";

        public int LineHeight { get; set; }

        public int Baseline { get; set; }

        public Dictionary<string, GlyphConfig> Glyphs
        { get; set; } = new();
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