using System;
using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The reactions' own numbers as the marks of their duo branches change them (the ledger's reaction-numbers): Dry Pods
    /// throws one more seed, Deep Swell widens the burst. Each changing mark carries <see cref="Upgrade.ReactionChanges"/>;
    /// for every set of a reaction's changing marks that can be in force together, the plain reaction's effects and each
    /// shape's are copied once with the changes laid over them, in the original effects' slots. The runner picks the copy
    /// for the marks in force as the reaction fires (<see cref="PlayerUpgrades.ReactionEffects"/>), so nothing is made
    /// while playing. Built from the catalogue when first asked, and again when the catalogue is gathered anew.
    /// </summary>
    public static class ReactionNumbers
    {
        /// <summary>A reaction is changed by this many of its marks at most; more are left out (no branch has more than two).</summary>
        public const int MostChangers = 4;

        private static IReadOnlyList<Upgrade> builtFrom;
        private static Upgrade[][] changers;
        // By pair, then by the set of its changers in force (a bit for each), then -1 for the plain reaction (index 0) or a shape.
        private static UpgradeEffect[][][][] changed;

        // Domain reload is off: what the last play built from its catalogue is built again from this play's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            builtFrom = null;
            changers = null;
            changed = null;
        }

        /// <summary>The marks that change the pair's reaction, in the catalogue's order; null for none.</summary>
        public static Upgrade[] Changers(int pair)
        {
            Ensure();
            return pair >= 0 && pair < changers.Length ? changers[pair] : null;
        }

        /// <summary>The pair's reaction as written: its plain effects (<paramref name="shape"/> -1) or a shape's.</summary>
        public static UpgradeEffect[] Written(int pair, int shape)
        {
            if (shape < 0)
            {
                var mark = ReactionTable.MarkFor(pair);
                return mark != null ? mark.Effects : Array.Empty<UpgradeEffect>();
            }
            var shapes = ReactionTable.ShapeMarkFor(pair);
            return shapes != null && shape < shapes.Shapes.Length && shapes.Shapes[shape] != null ? shapes.Shapes[shape].Effects : Array.Empty<UpgradeEffect>();
        }

        /// <summary>
        /// The pair's reaction with the changes of the changers in <paramref name="mask"/> (a bit for each, in the order of
        /// <see cref="Changers"/>): the plain effects (<paramref name="shape"/> -1) or a shape's. Null when nothing changes them.
        /// </summary>
        public static UpgradeEffect[] Changed(int pair, int mask, int shape)
        {
            Ensure();
            if (pair < 0 || pair >= changed.Length || changed[pair] == null || mask <= 0 || mask >= changed[pair].Length) return null;
            var lists = changed[pair][mask];
            int at = shape + 1;
            return lists != null && at >= 0 && at < lists.Length ? lists[at] : null;
        }

        private static void Ensure()
        {
            var all = Upgrade.All;
            if (builtFrom == all && changers != null) return;
            Build(all);
        }

        /// <summary>Builds the changed copies from a catalogue (the game's, or a test's).</summary>
        public static void Build(IReadOnlyList<Upgrade> marks)
        {
            builtFrom = marks;
            changers = new Upgrade[AfflictionPairs.Count][];
            changed = new UpgradeEffect[AfflictionPairs.Count][][][];
            var found = new List<Upgrade>[AfflictionPairs.Count];
            foreach (var mark in marks)
            {
                if (mark?.ReactionChanges == null || mark.ReactionChanges.Length == 0 || !mark.IsMix) continue;
                int pair = HeartLayout.PairOf(mark);
                if (pair < 0) continue;
                (found[pair] ??= new List<Upgrade>()).Add(mark);
            }
            for (int pair = 0; pair < found.Length; pair++)
            {
                if (found[pair] == null) continue;
                var list = found[pair];
                if (list.Count > MostChangers) list.RemoveRange(MostChangers, list.Count - MostChangers);
                changers[pair] = list.ToArray();
                int sets = 1 << list.Count;
                var shapes = ReactionTable.ShapeMarkFor(pair);
                int shapeCount = shapes != null ? shapes.Shapes.Length : 0;
                changed[pair] = new UpgradeEffect[sets][][];
                for (int mask = 1; mask < sets; mask++)
                {
                    var lists = new UpgradeEffect[shapeCount + 1][];
                    for (int shape = -1; shape < shapeCount; shape++)
                    {
                        var written = Written(pair, shape);
                        var copy = new UpgradeEffect[written.Length];
                        for (int i = 0; i < copy.Length; i++) copy[i] = written[i].Twin();
                        for (int c = 0; c < list.Count; c++)
                        {
                            if ((mask & (1 << c)) == 0) continue;
                            foreach (var change in list[c].ReactionChanges)
                                if (change != null && change.Shape == shape && change.Index >= 0 && change.Index < copy.Length) change.ApplyTo(copy[change.Index]);
                        }
                        lists[shape + 1] = copy;
                    }
                    changed[pair][mask] = lists;
                }
            }
        }
    }
}
