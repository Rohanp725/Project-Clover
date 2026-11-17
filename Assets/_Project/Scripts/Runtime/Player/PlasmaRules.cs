using UnityEngine;

namespace Clover.Player
{
    /// <summary>The plain sums of the Plasma branch's two bursts: Twin Break (Burn and Static spent together by a perfect guard) and Meltdown (both at full depth).</summary>
    public static class PlasmaRules
    {
        /// <summary>What each stack is worth when the two are spent together, and how much harder they strike together than apart.</summary>
        public const float BurnAStack = 10f, StaticAStack = 7f, Together = 1.5f, StaggerAStack = 5f;
        /// <summary>Meltdown: seconds an enemy shakes with light before it bursts, what the burst does to it, how far it reaches and the share the rest take.</summary>
        public const float MeltSeconds = 3f, MeltDamage = 45f, MeltStagger = 60f, MeltRadius = 5f, MeltShare = 0.5f;
        /// <summary>Seconds before the same enemy can melt down again.</summary>
        public const float MeltAgain = 10f;

        /// <summary>Twin Break's damage for these many stacks of each, spent together.</summary>
        public static float TwinDamage(int burn, int charge) => (BurnAStack * Mathf.Max(0, burn) + StaticAStack * Mathf.Max(0, charge)) * Together;

        public static float TwinStagger(int burn, int charge) => StaggerAStack * (Mathf.Max(0, burn) + Mathf.Max(0, charge));

        /// <summary>True when an enemy carrying these many stacks of each, of these many at most, is ready to melt down.</summary>
        public static bool Melts(int burn, int charge, int mostBurn, int mostCharge) => mostBurn > 0 && mostCharge > 0 && burn >= mostBurn && charge >= mostCharge;

        /// <summary>Seconds between the pulses of light on a melting enemy: they quicken as the burst comes.</summary>
        public static float Pulse(float secondsLeft) => Mathf.Lerp(0.08f, 0.45f, Mathf.Clamp01(secondsLeft / MeltSeconds));
    }
}
