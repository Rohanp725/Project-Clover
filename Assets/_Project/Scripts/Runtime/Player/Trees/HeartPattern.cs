using System;
using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// The worked layout of a duo branch in the chart's heart (IMPLEMENTATION.md 4.2, task 4.5): the ring's middle, inside
    /// the emblems, where the branch of the pair being looked at is drawn upright, one pair at a time. A branch's Shape mark
    /// stands at the foot, two columns of passives rise from it, the keystone stands between them, the duo spell above it,
    /// two places beside the spell (a duo surge, a spare) and the top mark at the crown (Plasma's own). Ten places for the
    /// five to seven marks a branch has (pairs_d1_d2.md), so every one of the 55 branches is drawn alike.
    ///
    /// Places are in steps, X across and Y up, round the ring's middle, the way the heart is seen on the screen whatever
    /// tree is turned upright. Each names the place it is meant to grow from (<see cref="BranchPattern.Place.From"/>); a
    /// writer may grow a mark from any place lower down whose line passes clear (the duo spell from either first passive,
    /// the keystone from any of the four). Proved by <c>UpgradeChartTests.TheHeartPatternIsSpacedClearAndWalkedFromEveryTree</c>.
    /// In namespace Clover.Player, not Trees, because it is no tree file.
    /// </summary>
    public static class HeartPattern
    {
        public const string Foot = "H.foot";
        public const string LeftLow = "H.L1";
        public const string RightLow = "H.R1";
        public const string LeftHigh = "H.L2";
        public const string RightHigh = "H.R2";
        public const string Keystone = "H.key";
        public const string Spell = "H.spell";
        public const string LeftTop = "H.L3";
        public const string RightTop = "H.R3";
        public const string Crown = "H.crown";

        /// <summary>No mark of the heart lies further than this from the ring's middle, so it stays clear of the spokes and the emblems.</summary>
        public const float Radius = 4.2f;

        private static readonly BranchPattern.Place[] places =
        {
            Slot(Foot, 0f, -3.6f),
            Slot(LeftLow, -1.3f, -2.4f, Foot), Slot(RightLow, 1.3f, -2.4f, Foot),
            Slot(LeftHigh, -1.95f, -0.95f, LeftLow), Slot(RightHigh, 1.95f, -0.95f, RightLow),
            Slot(Keystone, 0f, -1.15f, LeftLow),
            Slot(Spell, 0f, 0.45f, LeftLow),
            Slot(LeftTop, -1.3f, 1.5f, LeftHigh), Slot(RightTop, 1.3f, 1.5f, RightHigh),
            Slot(Crown, 0f, 2.55f, Spell),
        };

        private static BranchPattern.Place Slot(string name, float x, float y, string from = "") => new BranchPattern.Place(name, x, y, from);

        /// <summary>Every place of the heart.</summary>
        public static IReadOnlyList<BranchPattern.Place> All => places;

        /// <summary>The place by its name (<see cref="Foot"/> and the rest).</summary>
        public static BranchPattern.Place Of(string name)
        {
            for (int i = 0; i < places.Length; i++)
                if (places[i].Name == name) return places[i];
            throw new ArgumentException("No place in the heart is called " + name, nameof(name));
        }

        /// <summary>
        /// Puts a duo branch's mark at a place of the heart and returns it, for a branch file's object initialisers: its
        /// place set and its chart the heart (<see cref="MarkTree.Duo"/>).
        /// </summary>
        public static Upgrade Put(Upgrade mark, string place)
        {
            var at = Of(place);
            mark.X = at.X;
            mark.Y = at.Y;
            mark.Tree = MarkTree.Duo;
            return mark;
        }
    }
}
