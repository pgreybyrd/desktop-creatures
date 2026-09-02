using Desktop_Creatures.Audio;
using Desktop_Creatures.Config;
using Desktop_Creatures.Graphics.Animation;
using Desktop_Creatures.World;
using Desktop_Creatures.World.Surfaces;
using PixelRecolor.Core;
using Point = System.Windows.Point;

namespace Desktop_Creatures.Creatures
{
    public class Skunk : Creature
    {
        public override Point PickupAnchor =>
            new(31, 16);

        public Skunk(
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
                $"{definition.AssetFolder}/Sounds/chatter_01.wav",
                $"{definition.AssetFolder}/Sounds/chatter_02.wav",
                $"{definition.AssetFolder}/Sounds/chatter_03.wav");

            soundSet.Add(
                CreatureSoundEvent.Pickup,
                $"{definition.AssetFolder}/Sounds/chatter_01.wav",
                $"{definition.AssetFolder}/Sounds/chatter_02.wav",
                $"{definition.AssetFolder}/Sounds/chatter_03.wav");

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
