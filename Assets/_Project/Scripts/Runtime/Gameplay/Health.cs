using System;

namespace Clover.Gameplay
{
    [Serializable]
    public class Health
    {
        public float Maximum = 100f;

        public float Current { get; private set; }
        public float Fraction => Maximum > 0f ? Current / Maximum : 0f;
        public bool IsDead => Current <= 0f;

        public void Fill() => Current = Maximum;

        /// <summary>Takes the damage and returns how much was actually lost.</summary>
        public float Damage(float amount)
        {
            if (amount <= 0f || IsDead) return 0f;
            float lost = Math.Min(Current, amount);
            Current -= lost;
            return lost;
        }

        /// <summary>Gives health back, never past the most; nothing for the dead.</summary>
        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Current = Math.Min(Maximum, Current + amount);
        }
    }
}
