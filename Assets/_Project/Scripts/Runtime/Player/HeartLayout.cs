using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The chart's heart (IMPLEMENTATION.md 4.2, task 4.5): the ring's middle, inside the emblems, where the duo branches
    /// sit. One pair's branch is drawn there at a time, upright, in the places of <see cref="HeartPattern"/>: the pair of
    /// the two elements carried, or in the playtest's free chart the pair picked with the shoulder buttons. Every branch
    /// uses the same places, so the 55 look alike; the heart shows the pair's reaction name over its branch. Places are
    /// in steps round the ring's middle as the heart is seen on the screen; the menu turns the heart against the ring so
    /// it stays upright whichever tree is. Stateless, so one instance serves every menu; the rules for which pair is shown
    /// are plain statics, tested without a scene.
    /// </summary>
    public sealed class HeartLayout : IChartLayout
    {
        public static readonly HeartLayout Instance = new HeartLayout();

        /// <summary>Where the line to a branch's foot starts: a knot just under the Shape mark.</summary>
        public static readonly Vector2 FootPlace = new Vector2(0f, -4.55f);
        /// <summary>Where the pair's name and its reaction's are written, over the branch.</summary>
        public static readonly Vector2 TitlePlace = new Vector2(0f, 3.75f);

        /// <summary>Only the duo branches written into the heart (<see cref="MarkTree.Duo"/>): the ring keeps the mixes not reshaped yet.</summary>
        public bool Draws(Upgrade mark) => mark != null && !mark.IsHidden && mark.IsMix && mark.Tree == MarkTree.Duo;

        public Vector2 Place(Upgrade mark) => new Vector2(mark.X, mark.Y);

        public Vector2 Foot(Upgrade mark) => FootPlace;

        // ---- Which pair a mark is on, and which pair the heart shows.

        /// <summary>The pair (0 to 54, <see cref="AfflictionPairs"/>) of two elements, in either order; -1 for none (the same twice, or an unknown name).</summary>
        public static int PairOf(string one, string other)
        {
            var a = Elements.ByName(one);
            var b = Elements.ByName(other);
            return a == null || b == null ? -1 : AfflictionPairs.Index(a.Affliction, b.Affliction);
        }

        /// <summary>The pair a duo mark is on; -1 for a mark of one element.</summary>
        public static int PairOf(Upgrade mark) => mark != null && mark.IsMix ? PairOf(mark.Element, mark.Element2) : -1;

        /// <summary>True when the mark is drawn in the heart while it shows this pair.</summary>
        public bool Shows(Upgrade mark, int pair) => pair >= 0 && Draws(mark) && PairOf(mark) == pair;

        /// <summary>True when the pair has a branch written into the heart.</summary>
        public static bool Built(int pair, IReadOnlyList<Upgrade> marks)
        {
            if (pair < 0 || marks == null) return false;
            foreach (var mark in marks)
                if (Instance.Shows(mark, pair)) return true;
            return false;
        }

        /// <summary>
        /// The partner whose branch with <paramref name="viewed"/> the heart shows: the other element carried, when both are
        /// carried; else, in the playtest, the one picked if its branch is built, or the first built one round the ring
        /// after the element looked at. "" when the heart shows nothing (outside the playtest, a Hand of one element).
        /// </summary>
        public static string PartnerFor(string viewed, Build.Loadout loadout, bool playtest, string picked, IReadOnlyList<Upgrade> marks)
        {
            if (Elements.ByName(viewed) == null) return "";
            if (loadout != null && loadout.Mixed && loadout.Carries(viewed))
            {
                string other = loadout.First == viewed ? loadout.Second : loadout.First;
                if (!playtest || string.IsNullOrEmpty(picked)) return other;
            }
            if (!playtest) return "";
            if (!string.IsNullOrEmpty(picked) && picked != viewed && Built(PairOf(viewed, picked), marks)) return picked;
            return NextPartner(viewed, "", 1, marks);
        }

        /// <summary>
        /// The next partner round the ring from <paramref name="current"/> (by 1) or the one before (by -1) whose branch with
        /// <paramref name="viewed"/> is built; <paramref name="current"/> itself when no other is, "" when none is.
        /// </summary>
        public static string NextPartner(string viewed, string current, int by, IReadOnlyList<Upgrade> marks)
        {
            var at = Elements.ByName(viewed);
            if (at == null) return "";
            int count = Elements.Count;
            var from = Elements.ByName(current);
            int start = from != null ? from.Index : at.Index;
            int step = by < 0 ? -1 : 1;
            for (int i = 1; i <= count; i++)
            {
                var next = Elements.All[((start + step * i) % count + count) % count];
                if (next.Name == viewed) continue;
                if (Built(PairOf(viewed, next.Name), marks)) return next.Name;
            }
            return "";
        }

        /// <summary>
        /// A duo branch's name: the ring's mix name for neighbours ("Plasma"), else its reaction's ("Sleet Charge"), as the
        /// new branches take their reaction's name (IMPLEMENTATION.md 2.3). "" for no pair.
        /// </summary>
        public static string BranchName(string one, string other)
        {
            string mix = Elements.MixName(one, other);
            if (!string.IsNullOrEmpty(mix)) return mix;
            int pair = PairOf(one, other);
            return pair < 0 ? "" : AfflictionPairs.Name(pair);
        }

        /// <summary>The words over the heart: the pair and its reaction ("Fire and Lightning: Flashover").</summary>
        public static string Title(string one, string other)
        {
            int pair = PairOf(one, other);
            return pair < 0 ? "" : one + " and " + other + ": " + AfflictionPairs.Name(pair);
        }
    }
}
