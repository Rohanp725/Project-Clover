using System.Collections.Generic;
using Clover.Core.Save;

namespace Clover.Tests
{
    /// <summary>PlayerPrefs for the save's tests: a dictionary, counting the reads, never the machine's own.</summary>
    public class FakePrefs : IPrefsSource
    {
        public readonly Dictionary<string, int> Ints = new Dictionary<string, int>();
        public int Reads { get; private set; }

        /// <summary>Sets a flag as an older build did: <c>clover.progress.&lt;key&gt;</c> = 1.</summary>
        public FakePrefs Flag(string key)
        {
            Ints[SaveMigration.PrefsPrefix + key] = 1;
            return this;
        }

        public bool HasKey(string key)
        {
            Reads++;
            return Ints.ContainsKey(key);
        }

        public int GetInt(string key, int fallback)
        {
            Reads++;
            return Ints.TryGetValue(key, out int value) ? value : fallback;
        }

        /// <summary>A copy of what is held, to compare after.</summary>
        public Dictionary<string, int> Snapshot() => new Dictionary<string, int>(Ints);
    }
}
