using System;
using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The reactions as the catalogue holds them (IMPLEMENTATION.md 2.4, 3.7): for each of the 55 pairs its plain reaction
    /// mark (when it is built), its duo branch's Shape mark (when that is built: the reaction's two shapes), the effects a
    /// reaction runs under a loadout, and how long it rests on one enemy. Looked up from <see cref="Upgrade.All"/> and made
    /// again whenever the catalogue is gathered again.
    ///
    /// A reaction's shape (task 4.5) is chosen at a rest fire like a spell's, kept in <c>Loadout.Shapes</c> under the
    /// reaction's id (<c>react.flashover</c>, <see cref="ShapeKey"/>). While the Shape mark is learned and in force (both
    /// elements carried), the shape worn runs <b>instead of</b> the plain reaction; before it is bought, or with the pair
    /// broken, the plain one runs.
    /// </summary>
    public static class ReactionTable
    {
        // Rests the pair files give their own entries (README.md: "An entry may set its own rest, 3 to 8 s"); every
        // other pair rests the shared 4 s, and every distance-3 pair 6 s (AfflictionPairs.Rest). A built mark's own
        // ReactionDef.Rest, when set, is read before these.
        private static readonly Dictionary<string, float> entryRests = new Dictionary<string, float>
        {
            ["Shut in the Dark"] = 8f, ["Burning Sap"] = 6f, ["The Grudge"] = 8f,
            ["Steam"] = 3f, ["Logan Stone"] = 6f, ["Cold Short"] = 5f, ["Caught Light"] = 6f, ["Girdled"] = 6f, ["Chokedamp"] = 6f,
            ["Red Hot"] = 5f, ["The Bolt Runs True"] = 5f, ["Breath Taken"] = 6f,
            ["Burnt Mound"] = 6f, ["Stolen Warmth"] = 6f, ["Fish Poison"] = 6f,
        };

        private static readonly Upgrade[] marks = new Upgrade[AfflictionPairs.Count];
        private static readonly Upgrade[] shapeMarks = new Upgrade[AfflictionPairs.Count];
        private static readonly Dictionary<string, int> pairOf = new Dictionary<string, int>();
        private static IReadOnlyList<Upgrade> readFrom;

        // Domain reload is off: the catalogue read last play may have been gathered again with a probe's marks.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            readFrom = null;
            Array.Clear(marks, 0, marks.Length);
            Array.Clear(shapeMarks, 0, shapeMarks.Length);
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

        /// <summary>The plain reaction of the pair: its mark's effects, whatever is learned. See the overload with what is learned for the shapes.</summary>
        public static IReadOnlyList<UpgradeEffect> EffectsFor(int pair, Build.Loadout loadout)
        {
            var mark = MarkFor(pair);
            return mark != null ? mark.Effects : Array.Empty<UpgradeEffect>();
        }

        /// <summary>
        /// What a reaction of the pair runs under the loadout and what is learned: the shape worn, while the pair's Shape
        /// mark is learned and in force, instead of the plain reaction; else the plain reaction.
        /// </summary>
        public static IReadOnlyList<UpgradeEffect> EffectsFor(int pair, Build.Loadout loadout, ICollection<string> learned)
        {
            var shape = WornShape(pair, loadout, learned);
            return shape != null ? shape.Effects : EffectsFor(pair, loadout);
        }

        /// <summary>The pair's duo branch's Shape mark (the reaction's two shapes), or null while its branch is not built.</summary>
        public static Upgrade ShapeMarkFor(int pair)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return null;
            Read();
            return shapeMarks[pair];
        }

        /// <summary>The key a reaction's choice of shape is kept under in <c>Loadout.Shapes</c>: the reaction's own id.</summary>
        public static string ShapeKey(int pair) => pair >= 0 && pair < AfflictionPairs.Count ? AfflictionPairs.Id(pair) : "";

        /// <summary>
        /// True for a duo branch's Shape mark: a reaction's two shapes, on no spell branch. Its shapes' effects run through
        /// <see cref="EffectsFor(int, Build.Loadout, ICollection{string})"/> when the pair reacts, never as standing effects.
        /// </summary>
        public static bool IsReactionShape(Upgrade mark) =>
            mark != null && mark.IsMix && mark.Part == BranchPart.Shape && string.IsNullOrEmpty(mark.Branch) && mark.Shapes != null && mark.Shapes.Length > 0;

        /// <summary>
        /// The shape the pair's reaction wears: the first unless the second was chosen (<see cref="ShapeKey"/>), and none while
        /// the Shape mark is not learned or not in force (one of the two elements not carried).
        /// </summary>
        public static ShapeDef WornShape(int pair, Build.Loadout loadout, ICollection<string> learned)
        {
            var mark = ShapeMarkFor(pair);
            if (mark == null || !MarkRules.InForce(mark, loadout, learned)) return null;
            int choice = loadout.ShapeOf(ShapeKey(pair));
            if (choice < 0 || choice >= mark.Shapes.Length) choice = 0;
            return mark.Shapes[choice];
        }

        /// <summary>The place of the shape the pair's reaction wears among its Shape mark's two, or -1 for the plain reaction.</summary>
        public static int WornShapeIndex(int pair, Build.Loadout loadout, ICollection<string> learned)
        {
            var mark = ShapeMarkFor(pair);
            if (mark == null || !MarkRules.InForce(mark, loadout, learned)) return -1;
            int choice = loadout.ShapeOf(ShapeKey(pair));
            return choice < 0 || choice >= mark.Shapes.Length ? 0 : choice;
        }

        /// <summary>How long the pair rests on one enemy: its mark's own rest, its entry's, or the shared default.</summary>
        public static float RestFor(int pair)
        {
            var mark = MarkFor(pair);
            if (mark != null && mark.Reaction != null && mark.Reaction.Rest > 0f) return mark.Reaction.Rest;
            return entryRests.TryGetValue(AfflictionPairs.Name(pair), out float rest) ? rest : AfflictionPairs.Rest(pair);
        }

        /// <summary>
        /// Gives the engine every pair's rest and the depth it waits for (the shared engine is made new each play, so the
        /// player's reactions do this as they wake).
        /// </summary>
        public static void ApplyRests(ReactionEngine engine)
        {
            if (engine == null) return;
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                engine.SetRest(pair, RestFor(pair));
                var mark = MarkFor(pair);
                var def = mark != null ? mark.Reaction : null;
                engine.SetDepth(pair, def != null ? def.DepthOf : AfflictionKind.None, def != null ? def.Depth : 0);
            }
        }

        private static void Read()
        {
            var all = Upgrade.All;
            if (ReferenceEquals(all, readFrom)) return;
            readFrom = all;
            Array.Clear(marks, 0, marks.Length);
            Array.Clear(shapeMarks, 0, shapeMarks.Length);
            foreach (var mark in all)
            {
                if (IsReactionShape(mark))
                {
                    int shaped = HeartLayout.PairOf(mark);
                    if (shaped >= 0) shapeMarks[shaped] = mark;
                    continue;
                }
                if (mark.Kind != UpgradeKind.Reaction) continue;
                int pair = PairOf(mark.Id);
                if (pair >= 0) marks[pair] = mark;
            }
        }
    }
}
