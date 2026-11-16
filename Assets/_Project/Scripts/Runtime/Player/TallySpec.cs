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
        /// <summary>Kept from scene to scene until the run ends.</summary>
        public bool Kept;
    }
}
