using System;

namespace Clover.Player
{
    [Serializable]
    public class ManaPool
    {
        public float Maximum = 100f;

        /// <summary>
        /// When set, there is no mana: whatever would be spent is asked of this instead (true: it was paid), and
        /// whatever would be gained is set aside in <see cref="Diverted"/> for whoever set it to take. Heartwood sets it.
        /// </summary>
        [NonSerialized] public Func<float, bool> Instead;

        private float current;
        /// <summary>Mana held. With no mana (<see cref="Instead"/>) every price can be asked, so this reads as full; the bar reads <see cref="Fraction"/>, which is empty.</summary>
        public float Current => Instead != null ? Maximum : current;
        public float Fraction => Instead != null ? 0f : Maximum > 0f ? current / Maximum : 0f;
        /// <summary>True when there is no mana and prices are paid another way.</summary>
        public bool Sealed => Instead != null;
        /// <summary>Mana that would have been gained while there was none, since it was last taken.</summary>
        public float Diverted { get; private set; }

        public void Empty() => current = 0f;

        public void Gain(float amount)
        {
            if (amount <= 0f) return;
            if (Instead != null)
            {
                Diverted += amount;
                return;
            }
            current = Math.Min(Maximum, current + amount);
        }

        public float TakeDiverted()
        {
            float taken = Diverted;
            Diverted = 0f;
            return taken;
        }

        public bool TrySpend(float cost)
        {
            if (Instead != null) return cost <= 0f || Instead(cost);
            if (cost > current) return false;
            current -= cost;
            return true;
        }
    }
}
