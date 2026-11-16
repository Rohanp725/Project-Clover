using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Where the fight reports the moments a tip is about, and where the player's status line is given the tip once
    /// <see cref="TipRules"/> allows it. What has been said is remembered for the play session only, never on disk.
    /// </summary>
    public static class Tips
    {
        /// <summary>The scene that is for resting: no tips there.</summary>
        public const string SilentScene = "Hub";

        /// <summary>The rule and its timings, open so a probe or a tuning session can read and change them.</summary>
        public static TipRules Rules { get; private set; } = new TipRules();

        private static PlayerDefence line;
        private static int calloutsSeen;

        // Domain reload is off: without this a second play would start with every tip already said.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Rules = new TipRules();
            line = null;
            calloutsSeen = 0;
        }

        /// <summary>The player of a newly opened scene: tips go to this one's status line from now on.</summary>
        public static void BeginScene(PlayerDefence player, string sceneName)
        {
            line = player;
            calloutsSeen = player.Callouts;
            Rules.BeginScene(sceneName == SilentScene);
        }

        public static void UnguardedBlow() => Rules.UnguardedBlow();

        public static void BlowTurned() => Rules.BlowTurned();

        public static void EnemyStaggered(Vector3 where)
        {
            if (line == null) return;
            Vector3 apart = where - line.transform.position;
            apart.y = 0f;
            Rules.EnemyStaggered(apart.magnitude);
        }

        public static void FinisherLanded() => Rules.Learned(Tip.Finish);

        /// <summary>
        /// Called by the player at the end of each frame, after everything that could have put a word on the line this
        /// frame has had its turn, so a tip never replaces a word from the same frame.
        /// </summary>
        public static void EndFrame(PlayerDefence player, float deltaTime)
        {
            if (player != line) return;
            if (player.Callouts != calloutsSeen) Rules.LineUsed();
            string tip = Rules.Tick(deltaTime);
            if (tip != null) player.Callout(tip);
            calloutsSeen = player.Callouts;
        }
    }
}
