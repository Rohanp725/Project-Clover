using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// How long ice and holds stop a god (the owner's decision of 2026-11-19): only for a moment, to fit what a god is.
    /// The man takes a short freeze or hold, the godly form only a hitch, and after either has ended the god cannot be
    /// stopped again for a while, so it is never held over and over. What is not a god is left to its own
    /// <see cref="AfflictionSet.FreezeScale"/>, as before. Plain rules: <see cref="AfflictionSet"/> asks them.
    /// </summary>
    public static class GodHolds
    {
        // The numbers as built. The fields below start from these and go back to them at every play, so a value tried
        // live with script eval does not outlast the play it was tried in.
        public const float ManShareAsBuilt = 0.3f;
        public const float ManCapAsBuilt = 1f;
        public const float ManGuardAsBuilt = 3f;
        public const float GodlyShareAsBuilt = 0.12f;
        public const float GodlyCapAsBuilt = 0.4f;
        public const float GodlyGuardAsBuilt = 5f;

        /// <summary>The share of a freeze's or hold's plain time the man takes (an elite's is 0.4), and the most it may last, in seconds.</summary>
        public static float ManShare = ManShareAsBuilt;
        public static float ManCap = ManCapAsBuilt;
        /// <summary>Seconds after a freeze or hold on the man has ended before he can be frozen or held again.</summary>
        public static float ManGuard = ManGuardAsBuilt;
        /// <summary>The share the godly form takes, and the most it may last: a hitch, not a stun.</summary>
        public static float GodlyShare = GodlyShareAsBuilt;
        public static float GodlyCap = GodlyCapAsBuilt;
        /// <summary>Seconds after a hitch on the godly form before it can be stopped again.</summary>
        public static float GodlyGuard = GodlyGuardAsBuilt;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Restore();

        /// <summary>Puts every number back as built: each play begins so, and tests that change one put it back.</summary>
        public static void Restore()
        {
            ManShare = ManShareAsBuilt;
            ManCap = ManCapAsBuilt;
            ManGuard = ManGuardAsBuilt;
            GodlyShare = GodlyShareAsBuilt;
            GodlyCap = GodlyCapAsBuilt;
            GodlyGuard = GodlyGuardAsBuilt;
        }

        /// <summary>The share of the plain time a freeze or hold lasts on this phase; all of it on what is no god.</summary>
        public static float Share(GodPhase phase) => phase == GodPhase.Man ? ManShare : phase == GodPhase.Godly ? GodlyShare : 1f;

        /// <summary>The most seconds a freeze or hold lasts on this phase; no limit on what is no god.</summary>
        public static float Cap(GodPhase phase) => phase == GodPhase.Man ? ManCap : phase == GodPhase.Godly ? GodlyCap : float.PositiveInfinity;

        /// <summary>Seconds after a freeze or hold ends before this phase can be stopped again; none on what is no god.</summary>
        public static float Guard(GodPhase phase) => phase == GodPhase.Man ? ManGuard : phase == GodPhase.Godly ? GodlyGuard : 0f;

        /// <summary>
        /// How long a freeze or hold that would last <paramref name="asked"/> seconds on a plain enemy lasts here: on a
        /// god, its phase's share, never more than its cap; on anything else, the asked time scaled by how it resists
        /// ice (<paramref name="freezeScale"/>: 0.4 for an elite or boss, 1 for the rest), as it always was.
        /// </summary>
        public static float Seconds(float asked, GodPhase phase, float freezeScale = 1f)
        {
            if (asked <= 0f) return 0f;
            if (phase == GodPhase.None) return asked * freezeScale;
            return Math.Min(asked * Share(phase), Cap(phase));
        }
    }
}
