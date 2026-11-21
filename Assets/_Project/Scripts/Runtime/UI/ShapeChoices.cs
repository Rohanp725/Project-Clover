using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;

namespace Clover.UI
{
    /// <summary>
    /// What the shape screen at a rest fire lists (the owner's decision of 2026-11-20): every learned spell's Shape mark,
    /// then every learned reaction's, each with the shape worn. Plain state with no scene behind it, like
    /// <see cref="RestChoices"/>, so the rules can be tested; the choice itself is made through
    /// <see cref="PlayerBuild.ChooseShape"/>, which only a rest fire allows.
    /// </summary>
    public class ShapeChoices
    {
        private readonly List<ShapeRow> rows = new List<ShapeRow>();

        public int Count => rows.Count;
        public int Selected { get; private set; }
        public ShapeRow Current => rows[Selected];

        public ShapeRow At(int index) => rows[index];

        /// <summary>Lists the learned Shape marks of <paramref name="marks"/>, spells first and then reactions, each in the chart's order.</summary>
        public void Fill(IReadOnlyList<Upgrade> marks, ICollection<string> learned, Loadout loadout)
        {
            string keep = rows.Count > 0 && Selected < rows.Count ? rows[Selected].Key : null;
            rows.Clear();
            if (marks != null && learned != null)
            {
                for (int pass = 0; pass < 2; pass++)
                    for (int i = 0; i < marks.Count; i++)
                    {
                        var mark = marks[i];
                        if (mark == null || mark.IsHidden || mark.Part != BranchPart.Shape || mark.Shapes == null || mark.Shapes.Length == 0) continue;
                        if (!learned.Contains(mark.Id)) continue;
                        bool reaction = ReactionTable.IsReactionShape(mark);
                        if (reaction != (pass == 1)) continue;
                        rows.Add(RowFor(mark, reaction, learned, loadout));
                    }
            }
            Selected = 0;
            for (int i = 0; i < rows.Count; i++)
                if (rows[i].Key == keep) Selected = i;
        }

        private static ShapeRow RowFor(Upgrade mark, bool reaction, ICollection<string> learned, Loadout loadout)
        {
            int pair = reaction ? HeartLayout.PairOf(mark) : -1;
            string key = reaction ? ReactionTable.ShapeKey(pair) : mark.Branch;
            int worn = loadout != null ? loadout.ShapeOf(key) : 0;
            if (worn < 0 || worn >= mark.Shapes.Length) worn = 0;
            return new ShapeRow
            {
                Mark = mark,
                Key = key,
                Title = reaction ? AfflictionPairs.Name(pair) : SpellName(mark.Branch),
                Reaction = reaction,
                Worn = worn,
                InForce = loadout != null && MarkRules.InForce(mark, loadout, learned),
            };
        }

        /// <summary>The name of a spell branch's spell: an element's own (its base spell) or the Learn mark's.</summary>
        public static string SpellName(string branch)
        {
            if (string.IsNullOrEmpty(branch)) return "";
            string element = HandRules.ElementOfBase(branch);
            if (element.Length > 0)
            {
                var kit = ElementKit.ByName(element, false);
                if (kit != null) return kit.SpellName;
            }
            var learn = Upgrade.ById(branch);
            return learn != null ? learn.Name : branch;
        }

        /// <summary>Moves the choice of row up or down, round the ends.</summary>
        public void Move(int by)
        {
            if (rows.Count == 0) return;
            Selected = ((Selected + by) % rows.Count + rows.Count) % rows.Count;
        }

        /// <summary>Chooses a row by its place (a click on it). False when there is none there.</summary>
        public bool Choose(int index)
        {
            if (index < 0 || index >= rows.Count) return false;
            Selected = index;
            return true;
        }

        /// <summary>The shape the chosen row would wear after stepping <paramref name="by"/> from the one worn, round the ends.</summary>
        public int Step(int by)
        {
            if (rows.Count == 0) return 0;
            var row = rows[Selected];
            int count = row.ShapeCount;
            return count == 0 ? 0 : ((row.Worn + by) % count + count) % count;
        }
    }
}
