using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
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
        public CreatureAppearance Appearance { get; }
        public string Variant =>
            Appearance.Variant;

        public Rat(
            double x,
            double y,
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
            string selectedVariant =
                variant ??
                Variants[Random.Next(Variants.Length)];

            Appearance =
                CreatureAppearanceFactory.Create(
                    "Rat",
                    selectedVariant);

            var sheet = 
                SpriteSheetLoader.Load(
                Appearance.SpriteSheet,
                "Assets/Creatures/Rat/Appearance/rat.json");

            foreach (var animation in sheet.Animations)
            {
                OverrideAnimation(
                    animation.Key,
                    animation.Value.Frames.Select(
                        frame => frame.Image));
            }

            InitializeGroundCreature(
                x,
                y);
        }
    }
}
