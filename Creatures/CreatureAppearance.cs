using PixelRecolor.Core;
using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public sealed class CreatureAppearance
{
    public CreatureAppearanceTraits Traits { get; }

    public BitmapSource SpriteSheet { get; }

    public CreatureAppearance(
        CreatureAppearanceTraits traits,
        BitmapSource spriteSheet)
    {
        Traits = traits;
        SpriteSheet = spriteSheet;
    }
}