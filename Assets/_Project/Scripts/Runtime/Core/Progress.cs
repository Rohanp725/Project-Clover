using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// What stays with the player from run to run and from one sitting to the next: which weapons and elements a run
    /// may begin with, and whether the sponsor has already said its first words. Kept in the machine's own settings
    /// store, a flag at a time.
    /// </summary>
    public static class Progress
    {
        public const string MetSponsor = "met.sponsor";

        private const string Prefix = "clover.progress.";

        public static string WeaponKey(string weaponName) => "weapon." + weaponName;
        public static string ElementKey(string elementName) => "element." + elementName;

        /// <summary>
        /// The name of a stored flag that, when set before play begins, makes this play keep its progress in memory
        /// only: it starts as a player who has done nothing, and nothing it does is written down. The probes that
        /// play the game by script set it, so they neither see nor spoil what a person has really unlocked.
        /// </summary>
        public const string SandboxFlag = "clover.progress.sandbox";

        public static bool Sandboxed { get; private set; }

        private static readonly System.Collections.Generic.HashSet<string> sandbox = new System.Collections.Generic.HashSet<string>();

        // Domain reload is off: each play decides afresh whether it is sandboxed, and a sandbox starts empty.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            sandbox.Clear();
            Sandboxed = PlayerPrefs.GetInt(SandboxFlag, 0) == 1;
        }

        public static bool Has(string key) => Sandboxed ? sandbox.Contains(key) : PlayerPrefs.GetInt(Prefix + key, 0) == 1;

        public static void Unlock(string key)
        {
            if (Sandboxed)
            {
                sandbox.Add(key);
                return;
            }
            if (Has(key)) return;
            PlayerPrefs.SetInt(Prefix + key, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Forgets one thing; for starting over, and for tests.</summary>
        public static void Forget(string key)
        {
            if (Sandboxed)
            {
                sandbox.Remove(key);
                return;
            }
            PlayerPrefs.DeleteKey(Prefix + key);
            PlayerPrefs.Save();
        }
    }
}
