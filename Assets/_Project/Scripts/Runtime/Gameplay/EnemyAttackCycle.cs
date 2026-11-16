using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// The simplest enemy attack there is: rest, show the attack coming, strike, and again. Every few attacks one is red,
    /// which holding guard does not stop. Plain C#: the caller feeds it time and deals the blow when it says so.
    /// </summary>
    [Serializable]
    public class EnemyAttackCycle
    {
        public float RestSeconds = 1.7f;
        public float TelegraphSeconds = 0.9f;
        public float StrikeSeconds = 0.3f;
        /// <summary>Every this-many attacks one is red; 0 for never.</summary>
        public int RedEvery = 3;

        public EnemyAttackPhase Phase { get; private set; }
        public bool IsRed { get; private set; }
        public float TelegraphProgress => Phase == EnemyAttackPhase.Telegraph && TelegraphSeconds > 0f ? Math.Min(1f, elapsed / TelegraphSeconds) : 0f;
        public float StrikeProgress => Phase == EnemyAttackPhase.Strike && StrikeSeconds > 0f ? Math.Min(1f, elapsed / StrikeSeconds) : 0f;

        private float elapsed;
        private int attacks;

        /// <summary>Back to resting, as when the enemy is staggered out of its attack. The count toward red is kept.</summary>
        public void Reset()
        {
            Phase = EnemyAttackPhase.Rest;
            IsRed = false;
            elapsed = 0f;
        }

        /// <summary>
        /// Advances the cycle. A new attack begins only while <paramref name="mayAttack"/> is true (a target in range); one
        /// already shown is carried through. Returns true on the tick the blow lands.
        /// </summary>
        public bool Tick(float deltaTime, bool mayAttack)
        {
            elapsed += deltaTime;
            switch (Phase)
            {
                case EnemyAttackPhase.Rest:
                    if (elapsed < RestSeconds) return false;
                    if (!mayAttack)
                    {
                        elapsed = RestSeconds;
                        return false;
                    }
                    attacks++;
                    IsRed = RedEvery > 0 && attacks % RedEvery == 0;
                    Phase = EnemyAttackPhase.Telegraph;
                    elapsed = 0f;
                    return false;
                case EnemyAttackPhase.Telegraph:
                    if (elapsed < TelegraphSeconds) return false;
                    Phase = EnemyAttackPhase.Strike;
                    elapsed = 0f;
                    return true;
                default:
                    if (elapsed < StrikeSeconds) return false;
                    Phase = EnemyAttackPhase.Rest;
                    IsRed = false;
                    elapsed = 0f;
                    return false;
            }
        }
    }
}
