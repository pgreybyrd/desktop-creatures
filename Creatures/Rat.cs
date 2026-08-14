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

        public string Variant { get; }
        private readonly bool _useSpriteSheetTest;

        public Rat(
            double x,
            double y,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            SurfaceManager surfaceManager,
            Guid? id = null,
            string? name = null,
            string? variant = null,
            bool useSpriteSheetTest = false)
            : base(
                settings,
                pointOfInterestManager,
                surfaceManager,
                id,
                name)
        {
            _useSpriteSheetTest = useSpriteSheetTest;

        Variant =
                variant ??
                Variants[Random.Next(Variants.Length)];

            LoadAssets(
                $"Assets/Creatures/Rat/{Variant}");

            if (_useSpriteSheetTest)
            {
                var sheet =
                    SpriteSheetLoader.Load(
                        "Assets/SpriteSheetTests/rat.png",
                        "Assets/SpriteSheetTests/rat.json");

                var run =
                    sheet.GetAnimation("run");

                OverrideAnimation(
                    "Run",
                    run.Frames.Select(
                        frame => frame.Image));
            }

            InitializeGroundCreature(
                x,
                y);
        }
    }
}
