using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Burning: fire's mark on whatever it touches. It deals a steady trickle of damage for a few seconds; setting a
    /// burning thing alight again only tops the time up, it does not stack.
    /// </summary>
    [Serializable]
    public class BurnStatus
    {
        public float DamagePerSecond = 5f;
        /// <summary>The longest a burn can be made to last by topping it up.</summary>
        public float MaximumSeconds = 8f;

        public float SecondsLeft { get; private set; }
        public bool IsBurning => SecondsLeft > 0f;

        public void Reset() => SecondsLeft = 0f;

        public void Ignite(float seconds)
        {
            if (seconds <= 0f) return;
            SecondsLeft = Math.Min(MaximumSeconds, Math.Max(SecondsLeft, seconds));
        }

        /// <summary>Advances the burn and returns the damage it deals over this step.</summary>
        public float Tick(float deltaTime)
        {
            if (SecondsLeft <= 0f) return 0f;
            float burned = Math.Min(SecondsLeft, deltaTime);
            SecondsLeft -= burned;
            return DamagePerSecond * burned;
        }
    }
}
