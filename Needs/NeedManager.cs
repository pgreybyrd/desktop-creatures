using Desktop_Creatures.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop_Creatures.Needs
{
    public enum NeedType
    {
        Hunger,
        Energy,
        Curiosity
    }

    public class Need
    {
        public NeedType type { get; set; }
        public float Value { get; set; }
    }
    public class NeedManager
    {
        public float Hunger = 0.8f;
        //public float Energy = 0.8f;
        //public float Curiosit = 0.8f;
        //public Personality Personality { get; set; }

        public NeedManager()
        {
            //Personality = personality;
        }


        public void Update()
        {
            /*
            hunger += baseHungerRate * Personality.FoodMotivation;
            energy -= baseEnergyDrain * Personality.Laziness;
            curiosity += baseCuriosityRate * Personality.Curiosity;
            */
            Hunger += 0.01f;
        }

        public Need GetHighestNeed()
        {
            return new Need();
        }       
    }
}
