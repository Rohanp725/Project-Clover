using System;

namespace Clover.Player
{
    /// <summary>
    /// The player's health. A blow that is guarded still takes a little, but that part stays "grey": it can be won back
    /// by landing hits soon after, and fades if the player hangs back. A blow that is not guarded takes its damage for
    /// good and the grey part with it. Guarding alone can never kill.
    /// </summary>
    [Serializable]
    public class RegainHealth
    {
        public float Maximum = 100f;
        /// <summary>Seconds after the last guarded blow or regain before the grey part starts to fade.</summary>
        public float GreyDelay = 2.5f;
        public float GreyFadePerSecond = 10f;

        public float Current { get; private set; }
        /// <summary>Health lost to guarded blows that can still be won back.</summary>
        public float Grey { get; private set; }
        public float Fraction => Maximum > 0f ? Current / Maximum : 0f;
        /// <summary>Health and grey together, as a fraction of the maximum.</summary>
        public float GreyFraction => Maximum > 0f ? (Current + Grey) / Maximum : 0f;
        public bool IsDead => Current <= 0f;

        private float sinceGrey;

        /// <summary>Changes the greatest health. More of it comes filled; less of it takes what no longer fits.</summary>
        public void SetMaximum(float maximum)
        {
            if (maximum == Maximum) return;
            float gained = maximum - Maximum;
            Maximum = maximum;
            if (IsDead) return;
            Current = Math.Min(Maximum, gained > 0f ? Current + gained : Current);
            Grey = Math.Min(Grey, Maximum - Current);
        }

        public void Fill()
        {
            Current = Maximum;
            Grey = 0f;
            sinceGrey = 0f;
        }

        /// <summary>An unguarded blow: the damage is lost for good, and so is whatever was grey.</summary>
        public void Damage(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Grey = 0f;
            Current = Math.Max(0f, Current - amount);
        }

        /// <summary>What a guarded blow lets through. It becomes grey, and always leaves the player alive.</summary>
        public void Chip(float amount)
        {
            if (amount <= 0f || IsDead) return;
            float lost = Math.Min(amount, Math.Max(0f, Current - 1f));
            Current -= lost;
            Grey += lost;
            sinceGrey = 0f;
        }

        /// <summary>Wins back up to this much grey health; returns what was won.</summary>
        public float Regain(float amount)
        {
            if (amount <= 0f || IsDead || Grey <= 0f) return 0f;
            float won = Math.Min(Grey, amount);
            Current += won;
            Grey -= won;
            sinceGrey = 0f;
            return won;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Current = Math.Min(Maximum, Current + amount);
            Grey = Math.Min(Grey, Maximum - Current);
        }

        public void Tick(float deltaTime)
        {
            if (Grey <= 0f) return;
            sinceGrey += deltaTime;
            if (sinceGrey >= GreyDelay) Grey = Math.Max(0f, Grey - GreyFadePerSecond * deltaTime);
        }
    }
}
