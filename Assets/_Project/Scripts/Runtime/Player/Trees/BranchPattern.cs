using System;
using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// The worked layout of a tree made of spell branches (IMPLEMENTATION.md 2.2, task 4.1; docs/UPGRADE_SCHEMA.md 20.4):
    /// the root, three boughs each with a foot, a middle and a crown for passives, two spell branches of four marks on each
    /// bough as short stalks, two places at the top of the trunk, and the god's gift at the crown. 36 places for the 34
    /// marks of a tree, so a writer leaves two empty. Proved for all eleven elements at once, beside every mixed branch's
    /// pattern, by <c>UpgradeChartTests.TheSpellBranchPatternFillsAllElevenElementsAndEveryMixWithoutACrowdAndTheStickReachesAll</c>.
    ///
    /// Each place names the place its mark is meant to grow from (<see cref="Place.From"/>): the lines drawn that way miss
    /// every other mark. A writer may grow a mark from another, as long as it lies further out than its parent and its
    /// line passes clear (the base spell grows from the root; a stalk may grow from its bough's foot when the middle is
    /// left empty). Distances are in steps, X across and Y outward, as <see cref="UpgradeTree"/> places every mark.
    /// </summary>
    public static class BranchPattern
    {
        public readonly struct Place
        {
            public readonly string Name;
            public readonly float X, Y;
            /// <summary>The place its mark is meant to grow from; empty for the root and the gift.</summary>
            public readonly string From;

            public Place(string name, float x, float y, string from)
            {
                Name = name;
                X = x;
                Y = y;
                From = from ?? "";
            }
        }

        public const string Root = "root";
        public const string Gift = "gift";
        // The middle bough: its foot, its crown, and the two places at the top of the trunk.
        public const string MiddleFoot = "M.foot";
        public const string MiddleCrown = "M.crown";
        public const string TrunkLeft = "T.1";
        public const string TrunkRight = "T.2";
        // The side boughs: a foot near the root, a middle on the outer side, a crown above.
        public const string LeftFoot = "L.foot";
        public const string LeftMiddle = "L.mid";
        public const string LeftCrown = "L.crown";
        public const string RightFoot = "R.foot";
        public const string RightMiddle = "R.mid";
        public const string RightCrown = "R.crown";
        // The six stalks, each four places inmost first: Learn, Empower, Shape, Master.
        public const string MiddleA = "M.a";
        public const string MiddleB = "M.b";
        public const string LeftA = "L.a";
        public const string LeftB = "L.b";
        public const string RightA = "R.a";
        public const string RightB = "R.b";

        private static readonly Place[] places =
        {
            Slot(Root, 0f, 1.2f),
            Slot(MiddleFoot, 0f, 2.5f, Root),
            Slot("M.a.1", -0.48f, 3.45f, MiddleFoot), Slot("M.a.2", -0.48f, 4.4f, "M.a.1"), Slot("M.a.3", -0.48f, 5.35f, "M.a.2"), Slot("M.a.4", -0.48f, 6.3f, "M.a.3"),
            Slot("M.b.1", 0.48f, 3.45f, MiddleFoot), Slot("M.b.2", 0.48f, 4.4f, "M.b.1"), Slot("M.b.3", 0.48f, 5.35f, "M.b.2"), Slot("M.b.4", 0.48f, 6.3f, "M.b.3"),
            Slot(MiddleCrown, 0f, 7.35f, MiddleFoot),
            Slot(TrunkLeft, -0.62f, 8.5f, MiddleCrown), Slot(TrunkRight, 0.62f, 8.5f, MiddleCrown),
            Slot(Gift, 0f, 12f),
            Slot(LeftFoot, -1.55f, 2.55f, Root),
            Slot("L.a.1", -1.38f, 3.6f, LeftFoot), Slot("L.a.2", -1.45f, 4.53f, "L.a.1"), Slot("L.a.3", -1.52f, 5.47f, "L.a.2"), Slot("L.a.4", -1.6f, 6.4f, "L.a.3"),
            Slot(LeftMiddle, -2.45f, 6.0f, LeftFoot),
            Slot("L.b.1", -2.6f, 7.0f, LeftMiddle), Slot("L.b.2", -2.75f, 7.95f, "L.b.1"), Slot("L.b.3", -2.9f, 8.9f, "L.b.2"), Slot("L.b.4", -3.05f, 9.85f, "L.b.3"),
            Slot(LeftCrown, -1.6f, 8.6f, LeftMiddle),
            Slot(RightFoot, 1.55f, 2.55f, Root),
            Slot("R.a.1", 1.38f, 3.6f, RightFoot), Slot("R.a.2", 1.45f, 4.53f, "R.a.1"), Slot("R.a.3", 1.52f, 5.47f, "R.a.2"), Slot("R.a.4", 1.6f, 6.4f, "R.a.3"),
            Slot(RightMiddle, 2.45f, 6.0f, RightFoot),
            Slot("R.b.1", 2.6f, 7.0f, RightMiddle), Slot("R.b.2", 2.75f, 7.95f, "R.b.1"), Slot("R.b.3", 2.9f, 8.9f, "R.b.2"), Slot("R.b.4", 3.05f, 9.85f, "R.b.3"),
            Slot(RightCrown, 1.6f, 8.6f, RightMiddle),
        };

        private static Place Slot(string name, float x, float y, string from = "") => new Place(name, x, y, from);

        /// <summary>Every place of the pattern.</summary>
        public static IReadOnlyList<Place> All => places;

        /// <summary>The place by its name (<see cref="LeftFoot"/>, or a stalk's <c>L.a.2</c>).</summary>
        public static Place Of(string name)
        {
            for (int i = 0; i < places.Length; i++)
                if (places[i].Name == name) return places[i];
            throw new ArgumentException("No place in the spell-branch pattern is called " + name, nameof(name));
        }

        /// <summary>A stalk's four places, inmost first (Learn, Empower, Shape, Master).</summary>
        public static Place[] Stalk(string stalk) => new[] { Of(stalk + ".1"), Of(stalk + ".2"), Of(stalk + ".3"), Of(stalk + ".4") };

        /// <summary>Puts a passive at a place of the pattern and returns it, for a tree file's object initialisers.</summary>
        public static Upgrade Put(Upgrade mark, string place) => SpellBranch.Place(mark, Of(place));
    }
}
