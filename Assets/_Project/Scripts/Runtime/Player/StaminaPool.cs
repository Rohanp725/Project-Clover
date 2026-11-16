using System;

namespace Clover.Player
{
    /// <summary>
    /// The player's stamina: spent by the dodge (and later by attacks and guarding), refilled over time after a short pause.
    /// Plain C# so the rules can be tested without a scene.
    /// </summary>
    [Serializable]
    public class StaminaPool
    {
        public float Maximum = 100f;
        public float RegenerationPerSecond = 30f;
        /// <summary>Seconds after the last spend before stamina starts to come back.</summary>
        public float RegenerationDelay = 0.8f;

        public float Current { get; private set; }
        public float Fraction => Maximum > 0f ? Current / Maximum : 0f;

        private float secondsSinceSpend;

        public void Fill()
        {
            Current = Maximum;
            secondsSinceSpend = RegenerationDelay;
        }

        /// <summary>Spends the cost if there is enough; a refused spend changes nothing, so mashing does not delay the refill.</summary>
        public bool TrySpend(float cost)
        {
            if (cost > Current) return false;
            Current -= cost;
            secondsSinceSpend = 0f;
            return true;
        }

        /// <summary>Takes everything, as a broken guard does.</summary>
        /// <summary>Gives some back, as a move that turned out to be free does.</summary>
        public void Restore(float amount)
        {
            if (amount > 0f) Current = System.Math.Min(Maximum, Current + amount);
        }

        public void Empty()
        {
            Current = 0f;
            secondsSinceSpend = 0f;
        }

        public void Tick(float deltaTime)
        {
            secondsSinceSpend += deltaTime;
            if (secondsSinceSpend < RegenerationDelay) return;
            Current = Math.Min(Maximum, Current + RegenerationPerSecond * deltaTime);
        }
    }
}
