using System.Collections.Generic;
using UnityEngine;

namespace Clover.UI
{
    /// <summary>
    /// The playtest's written texts, read from docs/design/playtest/ by Clover > Build > Playtest Texts into a generated
    /// asset, so the owner can change the words without touching code: the guide to effects (effects.md), the details of
    /// the high-level marks (marks/*.md), and the version and what is new (whats_new.md). The game loads it from
    /// Resources by name.
    /// </summary>
    public class PlaytestTexts : ScriptableObject
    {
        public const string ResourceName = "PlaytestTexts";

        /// <summary>The version line, from the first "# " heading of whats_new.md.</summary>
        public string Version = "";
        /// <summary>What is new, the rest of whats_new.md.</summary>
        public string WhatsNew = "";
        public List<PlaytestText> Effects = new List<PlaytestText>();
        public List<PlaytestText> Marks = new List<PlaytestText>();

        private static PlaytestTexts loaded;
        private static bool looked;
        [System.NonSerialized] private Dictionary<string, PlaytestText> marksById;

        // Rebuilt in place by the builder: the index is made again from what the asset now holds.
        private void OnEnable() => marksById = null;
        public void Forget() => marksById = null;

        // Domain reload is off: a new play looks the asset up again, so a rebuilt one is read.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            loaded = null;
            looked = false;
        }

        /// <summary>The texts, or null when they have not been built.</summary>
        public static PlaytestTexts Get()
        {
            if (looked) return loaded;
            looked = true;
            loaded = Resources.Load<PlaytestTexts>(ResourceName);
            return loaded;
        }

        /// <summary>The written details of a mark, or null when only the automatic ones exist.</summary>
        public PlaytestText MarkDetails(string id)
        {
            if (marksById == null)
            {
                marksById = new Dictionary<string, PlaytestText>();
                foreach (var entry in Marks)
                    if (entry != null && !string.IsNullOrEmpty(entry.Id)) marksById[entry.Id] = entry;
            }
            return id != null && marksById.TryGetValue(id, out var found) ? found : null;
        }
    }
}
