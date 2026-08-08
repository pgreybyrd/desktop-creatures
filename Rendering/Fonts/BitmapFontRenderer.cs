using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfPoint = System.Windows.Point;

namespace Desktop_Creatures.Rendering.Fonts;

public static class BitmapFontRenderer
{
    public static void DrawText(
        DrawingContext drawingContext,
        BitmapFont font,
        string text,
        WpfPoint position,
        double scale = 1.0)
    {
        double cursorX = position.X;
        double cursorY = position.Y;

        foreach (char character in text)
        {
            if (character == '\n')
            {
                cursorX = position.X;
                cursorY += font.LineHeight * scale;
                continue;
            }

            if (!font.TryGetGlyph(character, out var glyph))
                continue;

            if (glyph.Source.Width > 0 &&
                glyph.Source.Height > 0)
            {
                var croppedGlyph = new CroppedBitmap(
                    font.Atlas,
                    glyph.Source);

                var destination = new Rect(
                    cursorX + glyph.XOffset * scale,
                    cursorY + glyph.YOffset * scale,
                    glyph.Source.Width * scale,
                    glyph.Source.Height * scale);

                drawingContext.DrawImage(
                    croppedGlyph,
                    destination);
            }

            cursorX += glyph.XAdvance * scale;
        }
    }
}