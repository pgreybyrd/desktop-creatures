using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public sealed class CreatureAppearance
{
    public string Variant { get; }

    public BitmapSource SpriteSheet { get; }

    public CreatureAppearance(
        string variant,
        BitmapSource spriteSheet)
    {
        Variant = variant;
        SpriteSheet = spriteSheet;
    }
}