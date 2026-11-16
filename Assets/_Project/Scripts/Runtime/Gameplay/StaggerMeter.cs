using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// How close an enemy is to being knocked open. Blows fill it; left alone it drains. When it fills, the enemy is
    /// staggered for a few seconds, takes no further stagger, and then starts again from empty.
    /// </summary>
    [Serializable]
    public class StaggerMeter
    {
        public float Maximum = 100f;
        /// <summary>Seconds without a blow before the meter starts to drain.</summary>
        public float DrainDelay = 2f;
        public float DrainPerSecond = 25f;
        public float StaggeredSeconds = 3f;

        public float Value { get; private set; }
        public float Fraction => Maximum > 0f ? Value / Maximum : 0f;
        public bool IsStaggered { get; private set; }

        private float sinceBlow;
        private float staggeredFor;

        public void Reset()
        {
            Value = 0f;
            IsStaggered = false;
            sinceBlow = 0f;
            staggeredFor = 0f;
        }

        /// <summary>Adds a blow's stagger. Returns true when this blow is the one that staggers the enemy.</summary>
        public bool Add(float amount)
        {
            if (IsStaggered || amount <= 0f) return false;
            sinceBlow = 0f;
            Value = Math.Min(Maximum, Value + amount);
            if (Value < Maximum) return false;
            IsStaggered = true;
            staggeredFor = 0f;
            return true;
        }

        /// <summary>Advances the meter. With <paramref name="recovers"/> false it does not drain: the ground is shaking under it.</summary>
        public void Tick(float deltaTime, bool recovers = true)
        {
            if (IsStaggered)
            {
                staggeredFor += deltaTime;
                if (staggeredFor >= StaggeredSeconds) Reset();
                return;
            }
            sinceBlow += deltaTime;
            if (recovers && sinceBlow >= DrainDelay) Value = Math.Max(0f, Value - DrainPerSecond * deltaTime);
        }
    }
}
