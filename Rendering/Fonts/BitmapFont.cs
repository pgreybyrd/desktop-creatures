using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Rendering.Fonts;

public sealed class BitmapFont
{
    public string Name { get; }

    public BitmapImage Atlas { get; }

    public int LineHeight { get; }

    private readonly Dictionary<char, BitmapGlyph> _glyphs;

    public BitmapFont(
        string name,
        BitmapImage atlas,
        int lineHeight,
        IEnumerable<BitmapGlyph> glyphs)
    {
        Name = name;
        Atlas = atlas;
        LineHeight = lineHeight;

        _glyphs = glyphs.ToDictionary(
            glyph => glyph.Character,
            glyph => glyph);
    }

    public bool TryGetGlyph(
        char character,
        out BitmapGlyph glyph)
    {
        return _glyphs.TryGetValue(
            character,
            out glyph!);
    }

    public BitmapGlyph GetGlyph(char character)
    {
        if (!_glyphs.TryGetValue(
            character,
            out var glyph))
        {
            throw new KeyNotFoundException(
                $"Glyph '{character}' was not found in font '{Name}'.");
        }

        return glyph;
    }

    public int MeasureText(string text)
    {
        int width = 0;

        foreach (char character in text)
        {
            if (_glyphs.TryGetValue(
                character,
                out var glyph))
            {
                width += glyph.XAdvance;
            }
        }

        return width;
    }
}