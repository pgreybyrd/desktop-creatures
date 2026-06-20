using Desktop_Creatures.Config;
using Desktop_Creatures.World;
using System;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Desktop_Creatures.Creatures
{
    public enum BirdState
    {
        Flying,
        Perching,
        Sleeping
    }

    public class Bird : Creature
    {

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
