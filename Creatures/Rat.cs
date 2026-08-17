using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;
using Point = System.Windows.Point;

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
        public string? AppearanceId { get; }
        public CreatureAppearanceTraits AppearanceTraits =>
            Appearance.Traits;

        public override Point PickupAnchor =>
            new(33, 11);

        public Rat(
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

            PlaySound(
                CreatureSoundEvent.Spawn);

            AppearanceId = appearanceId;

            if (appearanceId is not null)
            {
                selectedTraits =
                    CreatureAppearanceFactory.LoadTraits(
                        "Rat",
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

            var soundSet =
                new SoundSet();

            soundSet.Add(
                CreatureSoundEvent.Spawn,
                "Assets/Creatures/Rat/Sounds/squeak_01.wav",
                "Assets/Creatures/Rat/Sounds/squeak_02.wav",
                "Assets/Creatures/Rat/Sounds/squeak_03.wav",
                "Assets/Creatures/Rat/Sounds/squeak_04.wav",
                "Assets/Creatures/Rat/Sounds/squeak_05.wav",
                "Assets/Creatures/Rat/Sounds/squeak_06.wav",
                "Assets/Creatures/Rat/Sounds/squeak_07.wav",
                "Assets/Creatures/Rat/Sounds/squeak_08.wav",
                "Assets/Creatures/Rat/Sounds/squeak_09.wav",
                "Assets/Creatures/Rat/Sounds/squeak_10.wav");

            soundSet.Add(
                CreatureSoundEvent.Pickup,
                "Assets/Creatures/Rat/Sounds/squeak_01.wav",
                "Assets/Creatures/Rat/Sounds/squeak_02.wav",
                "Assets/Creatures/Rat/Sounds/squeak_03.wav",
                "Assets/Creatures/Rat/Sounds/squeak_04.wav",
                "Assets/Creatures/Rat/Sounds/squeak_05.wav",
                "Assets/Creatures/Rat/Sounds/squeak_06.wav",
                "Assets/Creatures/Rat/Sounds/squeak_07.wav",
                "Assets/Creatures/Rat/Sounds/squeak_08.wav",
                "Assets/Creatures/Rat/Sounds/squeak_09.wav",
                "Assets/Creatures/Rat/Sounds/squeak_10.wav");

            SetSoundSet(
                soundSet);

            InitializeGroundCreature(
                x,
                y);

            PlaySound(
                CreatureSoundEvent.Spawn);
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

        public override void OnPickedUp()
        {
            PlaySound(
                CreatureSoundEvent.Pickup);

            SetAction(
                CreatureAction.Held,
                "dangle");
        }
    }
}
