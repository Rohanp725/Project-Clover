using System.Collections.Generic;
using Clover.Core.Save;
using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// What stays with the player from run to run and from one sitting to the next: which weapons and elements a run
    /// may begin with, whether the sponsor has already said its first words, and what the world remembers (doors,
    /// ladders, walls, the elites and gods felled). Kept as flags in the save (<see cref="SaveData.Flags"/>, through
    /// <see cref="SaveStore"/>); the first time a flag is wanted it is also looked for where older builds kept it,
    /// PlayerPrefs, which is never written. Setting or forgetting a flag writes the save at once, as the old store did.
    /// </summary>
    public static class Progress
    {
        public const string MetSponsor = "met.sponsor";
        public const string WeaponPrefix = "weapon.";
        public const string ElementPrefix = "element.";

        public static string WeaponKey(string weaponName) => WeaponPrefix + weaponName;
        public static string ElementKey(string elementName) => ElementPrefix + elementName;

        /// <summary>
        /// The name of a stored flag that, when set before play begins, makes this play keep its progress in memory
        /// only: it starts as a player who has done nothing, and nothing it does is written down. The probes that
        /// play the game by script set it, so they neither see nor spoil what a person has really unlocked.
        /// </summary>
        public const string SandboxFlag = "clover.progress.sandbox";

        public static bool Sandboxed { get; private set; }
        /// <summary>Goes up whenever a flag is set or forgotten, or another save comes into force, so what is read from the flags can be kept until it does.</summary>
        public static int Version { get; private set; }

        private static SaveStore store;
        // The save the two sets below were filled from, so asking a flag is a set lookup rather than a list search.
        private static SaveData indexed;
        private static readonly HashSet<string> flags = new HashSet<string>();
        private static readonly HashSet<string> asked = new HashSet<string>();

        // Domain reload is off: each play decides afresh whether it is sandboxed, reads the save afresh, and a sandbox starts empty.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            store = null;
            indexed = null;
            flags.Clear();
            asked.Clear();
            Sandboxed = PlayerPrefs.GetInt(SandboxFlag, 0) == 1;
            Version = 0;
        }

        /// <summary>The store behind the flags, made when first wanted.</summary>
        public static SaveStore Store => store ??= SaveStore.ForThisMachine();

        /// <summary>The save in force: the sandbox's, the playtest's or the game's (<see cref="PlaytestMode.On"/>).</summary>
        public static SaveData Data => Current();

        public static bool Has(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            var data = Current();
            if (flags.Contains(key)) return true;
            if (Sandboxed || !asked.Add(key)) return false;
            // Never asked before: what an older build kept in PlayerPrefs counts, once.
            if (!Store.AskPrefs(data, key)) return false;
            flags.Add(key);
            return true;
        }

        public static void Unlock(string key)
        {
            if (string.IsNullOrEmpty(key) || Has(key)) return;
            Current().SetFlag(key);
            flags.Add(key);
            Version++;
            Save();
        }

        /// <summary>Forgets one thing; for starting over, and for tests.</summary>
        public static void Forget(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            var data = Current();
            bool had = data.ClearFlag(key);
            flags.Remove(key);
            if (Sandboxed)
            {
                if (had) Version++;
                return;
            }
            // Never to be looked for in PlayerPrefs again, or it would come back from there.
            if (asked.Add(key) && !data.PrefsAsked.Contains(key)) data.PrefsAsked.Add(key);
            Version++;
            Save();
        }

        /// <summary>Writes the save in force to its file (never while sandboxed). The save points call this.</summary>
        public static bool Save() => Store.Save(Sandboxed, PlaytestMode.On);

        // The save in force; when it is another one than last time (the first ask, or the playtest switched on), the sets
        // are filled again from it and the version moves, so whatever was read from the old one is read again.
        private static SaveData Current()
        {
            var data = Store.Get(Sandboxed, PlaytestMode.On);
            if (ReferenceEquals(data, indexed)) return data;
            indexed = data;
            flags.Clear();
            asked.Clear();
            foreach (var flag in data.Flags) flags.Add(flag);
            foreach (var key in data.PrefsAsked) asked.Add(key);
            Version++;
            return data;
        }
    }
}
