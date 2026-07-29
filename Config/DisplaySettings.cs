using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Creatures.Config
{
    public sealed class DisplaySettings
    {
        public int WorldScale { get; private set; } = 2;
        public int UiScale { get; private set; } = 2;

        public event EventHandler? ScaleChanged;

        public void SetScale(int scale)
        {
            if (scale < 1)
                throw new ArgumentOutOfRangeException(nameof(scale));

            if (WorldScale == scale)
                return;

            WorldScale = scale;
            ScaleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
