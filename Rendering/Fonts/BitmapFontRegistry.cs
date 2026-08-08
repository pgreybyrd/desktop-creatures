namespace Desktop_Creatures.Rendering.Fonts;

public static class BitmapFontRegistry
{
    private static readonly Dictionary<string, BitmapFont> _fonts = new();

    public static BitmapFont? DefaultFont { get; private set; }

    public static void Register(
        string name,
        BitmapFont font,
        bool setAsDefault = false)
    {
        _fonts[name] = font;

        if (setAsDefault || DefaultFont is null)
        {
            DefaultFont = font;
        }
    }

    public static BitmapFont Get(string name)
    {
        if (!_fonts.TryGetValue(name, out var font))
        {
            throw new KeyNotFoundException(
                $"Bitmap font '{name}' has not been registered.");
        }

        return font;
    }
}