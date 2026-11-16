using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Burning: fire's mark on whatever it touches. It deals a steady trickle of damage for a few seconds; setting a
    /// burning thing alight again tops the time up. Some blows also make the burn deeper, a stack at a time: a deeper
    /// burn hurts more each second, and can be set off all at once.
    /// </summary>
    [Serializable]
    public class BurnStatus
    {
        public float DamagePerSecond = 5f;
        /// <summary>The longest a burn can be made to last by topping it up.</summary>
        public float MaximumSeconds = 8f;
        public int MaximumStacks = 5;
        /// <summary>Share of the plain burn's damage each stack adds.</summary>
        public float DamagePerStack = 0.3f;
        /// <summary>Damage dealt at once when the burn is set off: this for a burn with no depth, and this again for each stack.</summary>
        public float DetonationDamage = 10f;

        public float SecondsLeft { get; private set; }
        public int Stacks { get; private set; }
        public bool IsBurning => SecondsLeft > 0f;

        public void Reset()
        {
            SecondsLeft = 0f;
            Stacks = 0;
        }

        public void Ignite(float seconds, int stacks = 0)
        {
            if (seconds <= 0f) return;
            SecondsLeft = Math.Min(MaximumSeconds, Math.Max(SecondsLeft, seconds));
            Stacks = Math.Min(MaximumStacks, Stacks + Math.Max(0, stacks));
        }

        /// <summary>Sets the burn off: it ends, and the damage it had left to give in depth is returned at once.</summary>
        public float Detonate()
        {
            if (!IsBurning) return 0f;
            float damage = DetonationDamage * (1 + Stacks);
            Reset();
            return damage;
        }

        /// <summary>Advances the burn and returns the damage it deals over this step.</summary>
        public float Tick(float deltaTime)
        {
            if (SecondsLeft <= 0f) return 0f;
            float burned = Math.Min(SecondsLeft, deltaTime);
            SecondsLeft -= burned;
            float damage = DamagePerSecond * (1f + DamagePerStack * Stacks) * burned;
            if (SecondsLeft <= 0f) Stacks = 0;
            return damage;
        }
    }
}
