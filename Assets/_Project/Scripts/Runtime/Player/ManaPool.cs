using System;

namespace Clover.Player
{
    /// <summary>
    /// Mana pays for element spells and weapon skills. It never refills by itself: it is earned by landing hits, perfect
    /// guards and perfect dodges, which is what keeps magic in the service of close combat.
    /// </summary>
    [Serializable]
    public class ManaPool
    {
        public float Maximum = 100f;

        public float Current { get; private set; }
        public float Fraction => Maximum > 0f ? Current / Maximum : 0f;

        public void Empty() => Current = 0f;

        public void Gain(float amount)
        {
            if (amount <= 0f) return;
            Current = Math.Min(Maximum, Current + amount);
        }

        public bool TrySpend(float cost)
        {
            if (cost > Current) return false;
            Current -= cost;
            return true;
        }
    }
}
