
using System.Windows;

namespace Desktop_Creatures.Rendering.Fonts
{
    public sealed class BitmapGlyph
    {
        public char Character { get; init; }

        public Int32Rect Source { get; init; }

        public int XOffset { get; init; }
        public int YOffset { get; init; }

        public int XAdvance { get; init; }
    }
}
