using System;
using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The reactions as the catalogue holds them (IMPLEMENTATION.md 2.4, 3.7): for each of the 55 pairs its plain reaction
    /// mark (when it is built), the effects a reaction runs, and how long it rests on one enemy. Looked up from
    /// <see cref="Upgrade.All"/> and made again whenever the catalogue is gathered again.
    /// </summary>
    public static class ReactionTable
    {
        // Rests the pair files give their own entries (README.md: "An entry may set its own rest, 3 to 8 s"); every
        // other pair rests the shared 4 s, and every distance-3 pair 6 s (AfflictionPairs.Rest). A built mark's own
        // ReactionDef.Rest, when set, is read before these.
        private static readonly Dictionary<string, float> entryRests = new Dictionary<string, float>
        {
            ["Shut in the Dark"] = 8f, ["Burning Sap"] = 6f, ["The Grudge"] = 8f,
            ["Steam"] = 3f, ["Logan Stone"] = 6f, ["Cold Short"] = 5f, ["Caught Light"] = 6f, ["Girdled"] = 6f,
            ["Red Hot"] = 5f, ["The Bolt Runs True"] = 5f, ["Breath Taken"] = 6f,
            ["Burnt Mound"] = 6f, ["Stolen Warmth"] = 6f, ["Fish Poison"] = 6f,
        };

        private static readonly Upgrade[] marks = new Upgrade[AfflictionPairs.Count];
        private static readonly Dictionary<string, int> pairOf = new Dictionary<string, int>();
        private static IReadOnlyList<Upgrade> readFrom;

        // Domain reload is off: the catalogue read last play may have been gathered again with a probe's marks.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            readFrom = null;
            Array.Clear(marks, 0, marks.Length);
            pairOf.Clear();
        }

        /// <summary>The pair whose plain reaction has this id ("react.steam"); -1 for none.</summary>
        public static int PairOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            if (pairOf.Count == 0)
                for (int pair = 0; pair < AfflictionPairs.Count; pair++) pairOf[AfflictionPairs.Id(pair)] = pair;
            return pairOf.TryGetValue(id, out int found) ? found : -1;
        }

        /// <summary>The pair's plain reaction mark, or null while it is not built.</summary>
        public static Upgrade MarkFor(int pair)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return null;
            Read();
            return marks[pair];
        }

        /// <summary>How many of the 55 are built.</summary>
        public static int Built
        {
            get
            {
                Read();
                int built = 0;
                foreach (var mark in marks)
                    if (mark != null) built++;
                return built;
            }
        }

        /// <summary>
        /// What a reaction of the pair runs under the loadout: its plain mark's effects. NEEDS (Phase 4): the duo branch's
        /// Shape mark, when the loadout sets a shape for the pair, runs that shape's effects instead.
        /// </summary>
        public static IReadOnlyList<UpgradeEffect> EffectsFor(int pair, Build.Loadout loadout)
        {
            var mark = MarkFor(pair);
            return mark != null ? mark.Effects : Array.Empty<UpgradeEffect>();
        }

        /// <summary>How long the pair rests on one enemy: its mark's own rest, its entry's, or the shared default.</summary>
        public static float RestFor(int pair)
        {
            var mark = MarkFor(pair);
            if (mark != null && mark.Reaction != null && mark.Reaction.Rest > 0f) return mark.Reaction.Rest;
            return entryRests.TryGetValue(AfflictionPairs.Name(pair), out float rest) ? rest : AfflictionPairs.Rest(pair);
        }

        /// <summary>Gives the engine every pair's rest (the shared engine is made new each play, so the player's reactions do this as they wake).</summary>
        public static void ApplyRests(ReactionEngine engine)
        {
            if (engine == null) return;
            for (int pair = 0; pair < AfflictionPairs.Count; pair++) engine.SetRest(pair, RestFor(pair));
        }

        private static void Read()
        {
            var all = Upgrade.All;
            if (ReferenceEquals(all, readFrom)) return;
            readFrom = all;
            Array.Clear(marks, 0, marks.Length);
            foreach (var mark in all)
            {
                if (mark.Kind != UpgradeKind.Reaction) continue;
                int pair = PairOf(mark.Id);
                if (pair >= 0) marks[pair] = mark;
            }
        }
    }
}
