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
        public override Point PickupAnchor =>
            new(33, 11);

        public Rat(
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
                definition,
                settings,
                pointOfInterestManager,
                surfaceManager,
                id,
                name)
        {
            InitializeCreatureAssets(
                definition,
                appearanceTraits,
                appearanceId);

            var soundSet =
                new SoundSet();

            soundSet.Add(
                CreatureSoundEvent.Spawn,
                $"{definition.AssetFolder}/Sounds/squeak_01.wav",
                $"{definition.AssetFolder}/Sounds/squeak_02.wav",
                $"{definition.AssetFolder}/Sounds/squeak_03.wav",
                $"{definition.AssetFolder}/Sounds/squeak_04.wav",
                $"{definition.AssetFolder}/Sounds/squeak_05.wav",
                $"{definition.AssetFolder}/Sounds/squeak_06.wav",
                $"{definition.AssetFolder}/Sounds/squeak_07.wav",
                $"{definition.AssetFolder}/Sounds/squeak_08.wav",
                $"{definition.AssetFolder}/Sounds/squeak_09.wav",
                $"{definition.AssetFolder}/Sounds/squeak_10.wav");

            soundSet.Add(
                CreatureSoundEvent.Pickup,
                $"{definition.AssetFolder}/Sounds/squeak_01.wav",
                $"{definition.AssetFolder}/Sounds/squeak_02.wav",
                $"{definition.AssetFolder}/Sounds/squeak_03.wav",
                $"{definition.AssetFolder}/Sounds/squeak_04.wav",
                $"{definition.AssetFolder}/Sounds/squeak_05.wav",
                $"{definition.AssetFolder}/Sounds/squeak_06.wav",
                $"{definition.AssetFolder}/Sounds/squeak_07.wav",
                $"{definition.AssetFolder}/Sounds/squeak_08.wav",
                $"{definition.AssetFolder}/Sounds/squeak_09.wav",
                $"{definition.AssetFolder}/Sounds/squeak_10.wav");

            SetSoundSet(soundSet);

            InitializeGroundCreature(
                x,
                y);

            PlaySound(
                CreatureSoundEvent.Spawn);
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
