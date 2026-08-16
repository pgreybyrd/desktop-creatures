using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;

namespace Desktop_Creatures.Creatures
{
    public class Rat : Creature
    {
        private static readonly string[] Palettes =
        [
            "chocolate",
            "grey",
            "albino",
            "black",
            "cinnamon"
        ];

        public CreatureAppearance Appearance { get; }
        public CreatureAppearanceTraits AppearanceTraits =>
            Appearance.Traits;

        public Rat(
            double x,
            double y,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            SurfaceManager surfaceManager,
            Guid? id = null,
            string? name = null,
            CreatureAppearanceTraits? appearanceTraits = null)
            : base(
                settings,
                pointOfInterestManager,
                surfaceManager,
                id,
                name)
        {
            CreatureAppearanceTraits selectedTraits =
                appearanceTraits ??
                CreateRandomAppearanceTraits();

            Appearance =
                CreatureAppearanceFactory.Create(
                    "Rat",
                    selectedTraits);

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

        private CreatureAppearanceTraits CreateRandomAppearanceTraits()
        {
            string palette =
                Palettes[Random.Next(Palettes.Length)];

            return new CreatureAppearanceTraits(
                Palette: palette,
                Patterns: [],
                Accessories: [],
                Effects: []);
        }
    }
}
