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
        public float Value { get; set; }

        public float Threshold { get; set; }

        public float Rate { get; set; }

        public bool IsSatisfied => Value < Threshold;
    }

    public class NeedManager
    {
        public float Hunger = 0.0f;
        public float HungerRate = 0.005f;
        public float HungerThreshold = 0.75f;

        public bool IsHungry => Hunger >= HungerThreshold;

        //public float Energy = 0.8f;
        //public float Curiosit = 0.8f;
        //public Personality Personality { get; set; }


        public void Update()
        {
            /*
            hunger += baseHungerRate * Personality.FoodMotivation;
            energy -= baseEnergyDrain * Personality.Laziness;
            curiosity += baseCuriosityRate * Personality.Curiosity;
            */

            //Hunger = Math.Min(1.0f, Hunger + HungerRate);
            Hunger = Math.Clamp(Hunger + HungerRate, 0f, 1f);
        }

        public void Eat()
        {
            Hunger = 0f;
        }

        public Need GetHighestNeed()
        {
            return new Need();
        }       
    }
}
