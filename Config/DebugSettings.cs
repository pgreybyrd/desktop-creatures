using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Creatures.Config
{
    public class DebugSettings
    {
        public bool Enabled { get; set; } = false;

        public bool Behavior { get; set; } = false;
        public bool Surface { get; set; } = false;
        public bool Animation { get; set; } = false;
        public bool PointOfInterest { get; set; } = false;
        public bool Needs { get; set; } = false;
        public bool Movement { get; set; } = false;
        public bool Physics { get; set; } = false;
        public bool Window { get; set; } = false;
        public bool AssetLoading { get; set; } = false;
        public bool Configuration { get; set; } = false;
        public bool Path { get; set; } = false;
        public bool Collision { get; set; } = false;
        public bool Spawning { get; set; } = false;
        public bool Input { get; set; } = false;
        public bool Performance { get; set; } = false;
    }
}
