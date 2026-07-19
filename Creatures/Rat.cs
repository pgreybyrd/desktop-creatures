using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;

namespace Desktop_Creatures.Creatures
{
    public class Rat : Creature
    {
        private static readonly string[] Variants =
        [
            "Chocolate",
            "GreyHooded",
            "Albino",
            "Rainbow",
            "Black",
            "Cinnamon"
        ];

        public Rat(
            double startX,
            double startY,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            SurfaceManager surfaceManager)
            : base(settings, pointOfInterestManager, surfaceManager)
        {
            var variant = Variants[Random.Next(Variants.Length)];

            LoadAssets($"Assets/Creatures/Rat/{variant}");
            InitializeGroundCreature(startX, startY);
        }
    }
}
