using System.Collections.Generic;

namespace Clover.Player.Trees
{
    /// <summary>Every tree file, gathered in the order the elements stand round the ring. A new tree is one line here.</summary>
    public static class UpgradeTrees
    {
        public static void AddAll(List<Upgrade> to)
        {
            // Round the ring from straight up; each mix after the first of its two elements.
            FireTree.Add(to);
            PlasmaTree.Add(to);
            LightningTree.Add(to);
            TempestTree.Add(to);
            WindTree.Add(to);
            BlizzardTree.Add(to);
            IceTree.Add(to);
            GlacierTree.Add(to);
            WaterTree.Add(to);
            BloomTree.Add(to);
            LifeTree.Add(to);
            OvergrowthTree.Add(to);
            EarthTree.Add(to);
            ForgeTree.Add(to);
            MetalTree.Add(to);
            HaloTree.Add(to);
            LightTree.Add(to);
            EclipseTree.Add(to);
            DarkTree.Add(to);
            BlightTree.Add(to);
            PoisonTree.Add(to);
            PyreTree.Add(to);
            // The duo branches of pairs that are not neighbours, one file per pair under Trees/Duo/ (task 4.5 on), in pair order.
            Duo.CarriedEmbersDuo.Add(to);
            Duo.SleetChargeDuo.Add(to);
            Duo.CapsizeDuo.Add(to);
            Duo.WinterSeedDuo.Add(to);
            Duo.TheGroundGivesDuo.Add(to);
            Duo.IronThornsDuo.Add(to);
            Duo.EarthlightDuo.Add(to);
            Duo.BlackChainDuo.Add(to);
            Duo.BurningSapDuo.Add(to);
            Duo.SmoulderDuo.Add(to);
            Duo.NerveFireDuo.Add(to);
            Duo.ThermalShockDuo.Add(to);
            Duo.SeizedDuo.Add(to);
            Duo.WindthrowDuo.Add(to);
            Duo.FrostHeaveDuo.Add(to);
            Duo.RustDuo.Add(to);
            // The plain reactions, one hidden mark per pair, after every tree (they are on none).
            Reactions.Add(to);
        }
    }
}
