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
            Duo.ShiningHerbDuo.Add(to);
            Duo.TurnedToStoneDuo.Add(to);
            Duo.QuicksilverDuo.Add(to);
            Duo.KindlingPointDuo.Add(to);
            Duo.TheGrudgeDuo.Add(to);
            Duo.IllWindDuo.Add(to);
            Duo.SteamDuo.Add(to);
            Duo.LightningFlowerDuo.Add(to);
            Duo.LoganStoneDuo.Add(to);
            Duo.ColdShortDuo.Add(to);
            Duo.CaughtLightDuo.Add(to);
            Duo.GirdledDuo.Add(to);
            Duo.ChokedampDuo.Add(to);
            Duo.RedHotDuo.Add(to);
            Duo.TheBoltRunsTrueDuo.Add(to);
            Duo.BreathTakenDuo.Add(to);
            Duo.VenomRimeDuo.Add(to);
            Duo.FirebloomDuo.Add(to);
            Duo.EarthedDuo.Add(to);
            Duo.SickleWindDuo.Add(to);
            Duo.MockSunsDuo.Add(to);
            Duo.DrownedWeightDuo.Add(to);
            Duo.RotRootsDuo.Add(to);
            Duo.BurntMoundDuo.Add(to);
            Duo.LiveIronDuo.Add(to);
            Duo.GoldenFeathersDuo.Add(to);
            Duo.StolenWarmthDuo.Add(to);
            Duo.FishPoisonDuo.Add(to);
            // The weapons' Ways (phase 5), one file per weapon under Trees/Ways/, after the element charts.
            Ways.KatanaWay.Add(to);
            // The spare spells kept hidden for Phase 6's Evolutions, and the plain reactions, one hidden mark per pair, after
            // every tree (they are on none).
            SpareSpells.Add(to);
            Reactions.Add(to);
        }
    }
}
