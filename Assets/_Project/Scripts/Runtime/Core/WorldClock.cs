using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// Time as the enemies feel it, apart from the player's own. Slowing it for a moment makes everything but the
    /// player drag, which is what Lightning's Overclock does. Enemies multiply their time by <see cref="EnemyScale"/>.
    /// </summary>
    public static class WorldClock
    {
        private static float slowUntil;
        private static float slowScale = 1f;

        // Domain reload is off: a slow left running must not carry into the next play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            slowUntil = 0f;
            slowScale = 1f;
        }

        /// <summary>How fast the enemies' time runs now: 1 normally, less while slowed.</summary>
        public static float EnemyScale => Time.time < slowUntil ? slowScale : 1f;

        /// <summary>Slows the enemies' time to a share of its speed for a number of seconds.</summary>
        public static void SlowEnemies(float seconds, float scale)
        {
            slowUntil = Time.time + seconds;
            slowScale = Mathf.Clamp(scale, 0.05f, 1f);
        }
    }
}
