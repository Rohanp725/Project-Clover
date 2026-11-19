using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// What weapon play takes off every running spell clock (STEEL_AND_ELEMENT.md 3.1): a blow that lands 0.3 s for a
    /// light move and 0.6 s for a heavy one unless the move says its own, a perfect guard 2 s, a perfect dodge 1.5 s, a
    /// finisher 3 s. First guesses, kept as fields so they can be tuned live.
    /// </summary>
    [Serializable]
    public class CooldownCuts
    {
        public float LightBlow = 0.3f;
        public float HeavyBlow = 0.6f;
        public float PerfectGuard = 2f;
        public float PerfectDodge = 1.5f;
        public float Finisher = 3f;
        /// <summary>What every cut is multiplied by (marks and Bond, later).</summary>
        public float Share = 1f;

        /// <summary>Seconds a landed blow of this move takes off: the move's own number when it has one.</summary>
        public float ForBlow(float moveCut, bool heavy) => Math.Max(0f, (moveCut > 0f ? moveCut : heavy ? HeavyBlow : LightBlow) * Share);

        public float ForBlow(AttackStep step) => step == null ? 0f : ForBlow(step.CooldownCut, step.Heavy);
        public float ForPerfectGuard => PerfectGuard * Share;
        public float ForPerfectDodge => PerfectDodge * Share;
        public float ForFinisher => Finisher * Share;
    }
}
