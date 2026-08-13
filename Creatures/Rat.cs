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
            "Grey",
            "GreyHooded",
            "Albino",
            "Rainbow",
            "Black",
            "Cinnamon"
        ];

        public string Variant { get; }

        public Rat(
            double startX,
            double startY,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            SurfaceManager surfaceManager,
            Guid? id = null,
            string? name = null,
            string? variant = null)
            : base(
                settings,
                pointOfInterestManager,
                surfaceManager,
                id,
                name)
        {
            Variant =
                variant ??
                Variants[Random.Next(Variants.Length)];

            LoadAssets(
                $"Assets/Creatures/Rat/{Variant}");

            InitializeGroundCreature(
                startX,
                startY);
        }
    }
}
