using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures
{
    public class Ocelot : Creature
    {
        public CreatureAppearance Appearance { get; }
        public string? AppearanceId { get; }
        public CreatureAppearanceTraits AppearanceTraits =>
            Appearance.Traits;

        public override Point PickupAnchor =>
            new(33, 11);

        public Ocelot(
            CreatureDefinition definition,
            double x,
            double y,
            CreatureSettings settings,
            PointOfInterestManager pointOfInterestManager,
            SurfaceManager surfaceManager,
            Guid? id = null,
            string? name = null,
            CreatureAppearanceTraits? appearanceTraits = null,
            string? appearanceId = null)
            : base(
                settings,
                pointOfInterestManager,
                surfaceManager,
                id,
                name)
        {
            CreatureAppearanceTraits selectedTraits;

            AppearanceId = appearanceId;

            if (appearanceId is not null)
            {
                selectedTraits =
                    CreatureAppearanceFactory.LoadTraits(
                        definition,
                        appearanceId);
            }
            else if (appearanceTraits is not null)
            {
                selectedTraits = appearanceTraits;
            }
            else
            {
                selectedTraits =
                    CreateRandomAppearanceTraits();
            }

            Appearance =
                CreatureAppearanceFactory.Create(
                    definition,
                    selectedTraits);

            var sheet = 
                SpriteSheetLoader.Load(
                Appearance.SpriteSheet,
                $"{definition.AssetFolder}/Appearance/{definition.Id}.json");

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
            return new CreatureAppearanceTraits(
                Palette: null,
                Patterns: [],
                Accessories: [],
                Effects: []);
        }

        public override void OnPickedUp()
        {
            //PlaySound(
            //    CreatureSoundEvent.Pickup);

            SetAction(
                CreatureAction.Held,
                "dangle");
        }
    }
}
