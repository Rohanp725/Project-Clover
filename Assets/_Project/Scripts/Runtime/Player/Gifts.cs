using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// What a god leaves, by rule and not by list. Taking an element gives that element's Signature. Consuming one
    /// gives no duo crown (the owner's decision of 2026-11-20): it only strengthens an element already owned, and every
    /// duo branch's top marks are bought with Shards once both elements are carried. A new tree needs no line here: its
    /// Signature is found by where it sits.
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
    }
}
