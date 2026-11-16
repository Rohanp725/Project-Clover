using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// What a god leaves, by rule and not by list. Taking an element gives that element's Signature; consuming one
    /// gives the crown of the branch it shares with the element held, when the two stand side by side on the ring.
    /// A new tree or branch needs no line here: its Signature is found by where it sits.
    /// </summary>
    public static class Gifts
    {
        /// <summary>The Signature at the crown of an element's own tree, or null if its tree has none.</summary>
        public static Upgrade SignatureOf(string element) => SignatureOf(element, Upgrade.All);

        public static Upgrade SignatureOf(string element, IReadOnlyList<Upgrade> marks)
        {
            for (int i = 0; i < marks.Count; i++)
                if (marks[i].Signature && !marks[i].IsMix && marks[i].Element == element) return marks[i];
            return null;
        }

        /// <summary>The Signature of the branch two elements share, in either order, or null when they are not neighbours.</summary>
        public static Upgrade CrownOf(string held, string consumed) => CrownOf(held, consumed, Upgrade.All);

        public static Upgrade CrownOf(string held, string consumed, IReadOnlyList<Upgrade> marks)
        {
            if (held == consumed || !Elements.AreNeighbours(held, consumed)) return null;
            for (int i = 0; i < marks.Count; i++)
                if (marks[i].Signature && marks[i].IsMix && marks[i].BelongsTo(held) && marks[i].BelongsTo(consumed)) return marks[i];
            return null;
        }
    }
}
