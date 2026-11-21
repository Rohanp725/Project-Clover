using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// A count a mark keeps: one of its effects banks into it (<see cref="UpgradeAction.Bank"/>) and another spends
    /// from it (<see cref="UpgradeEffect.Spends"/>, <see cref="UpgradeEffect.SpendsAll"/>). Plates, folds, hanging
    /// swords. The HUD names it beside the weapon while the mark is in force.
    /// </summary>
    public class TallySpec
    {
        /// <summary>What the HUD calls it: "Plates".</summary>
        public string Name = "";
        /// <summary>The most it holds.</summary>
        public int Cap = 3;
        /// <summary>A stat each point held changes, by <see cref="PerPoint"/>. Left at zero amount it changes nothing.</summary>
        public PlayerStat Stat;
        public float PerPoint;
        /// <summary>A second stat each point changes.</summary>
        public PlayerStat Stat2;
        public float PerPoint2;
        /// <summary>Kept from scene to scene until the run ends (or, with <see cref="EmptiesAtRest"/>, until a rest fire).</summary>
        public bool Kept;
        /// <summary>A fight's growth: emptied at every rest fire (Folded Steel's folds).</summary>
        public bool EmptiesAtRest;
        /// <summary>
        /// A stat changed while the count is empty, by <see cref="EmptyAmount"/> (the ledger's while-count: Drawn Back to
        /// Break's weaker blows with no wave drawn). Left at zero amount it changes nothing.
        /// </summary>
        public PlayerStat EmptyStat;
        public float EmptyAmount;

        /// <summary>
        /// A rest fire: every count whose mark says <see cref="EmptiesAtRest"/> is emptied, both the one held now
        /// (<paramref name="counts"/>, by catalogue index) and the copy kept for the scenes (<paramref name="kept"/>, by
        /// id). Returns how many counts were emptied.
        /// </summary>
        public static int EmptyAtRest(IReadOnlyList<Upgrade> catalogue, int[] counts, IDictionary<string, int> kept)
        {
            if (catalogue == null) return 0;
            int emptied = 0;
            for (int i = 0; i < catalogue.Count; i++)
            {
                var mark = catalogue[i];
                if (mark?.Tally == null || !mark.Tally.EmptiesAtRest) continue;
                if (counts != null && i < counts.Length && counts[i] != 0)
                {
                    counts[i] = 0;
                    emptied++;
                }
                kept?.Remove(mark.Id);
            }
            return emptied;
        }
    }
}
