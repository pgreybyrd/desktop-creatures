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
        Thirst,
        Sleep,
        Fun,
        Comfort,
        Social,
        Exploration,
        Safety
    }

    public class Need
    {
        public NeedType Type { get; }

        public float Value { get; set; }
        public float Threshold { get; set; }
        public float Rate { get; set; }

        public bool IsActive =>
            Value >= Threshold;

        public Need(
            NeedType type,
            float value,
            float threshold,
            float rate)
        {
            Type = type;
            Value = value;
            Threshold = threshold;
            Rate = rate;
        }
    }

    public class NeedManager
    {
        private readonly Dictionary<NeedType, Need> _needs;

        //temp-----------------
        public bool IsHungry =>
            IsActive(NeedType.Hunger);

        public bool IsThirsty =>
            IsActive(NeedType.Thirst);

        public void Eat()
        {
            Satisfy(NeedType.Hunger);
        }

        public void Drink()
        {
            Satisfy(NeedType.Thirst);
        }

        public Need GetNeed(
            NeedType type)
        {
            return _needs[type];
        }
        //------------------

        public NeedManager()
        {
            _needs = new Dictionary<NeedType, Need>
            {
                [NeedType.Hunger] =
                    new Need(
                        NeedType.Hunger,
                        value: 0f,
                        threshold: 0.75f,
                        rate: 0.005f),

                [NeedType.Thirst] =
                    new Need(
                        NeedType.Thirst,
                        value: 0f,
                        threshold: 0.70f,
                        rate: 0.006f)
            };
        }

        public void Update()
        {
            foreach (var need in _needs.Values)
            {
                need.Value =
                    Math.Clamp(
                        need.Value + need.Rate,
                        0f,
                        1f);
            }
        }

        public bool IsActive(
            NeedType type)
        {
            return _needs[type].IsActive;
        }
        public void Satisfy(
            NeedType type)
        {
            _needs[type].Value = 0f;
        }

        public Need? GetHighestNeed()
        {
            return _needs.Values
                .Where(need =>
                    need.IsActive)
                .OrderByDescending(need =>
                    need.Value)
                .FirstOrDefault();
        }
    }
}
