using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Creatures.UI.FieldGuide
{
    public sealed class FieldGuideTabEntry
    {
        public required FieldGuideTab Tab { get; init; }
        public required string CreatureId { get; init; }

        public int RightX { get; init; }
        public int RightY { get; init; }
    }
}
