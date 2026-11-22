using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The surges' open parts (standin_clearance_plan.md pass 13, the ledger's surges row): Kindle's Fed Flame, Overdrive's
    /// critical chain-ender and Wellspring's The Flask Breaks (steel/elements_a.md).
    /// </summary>
    public class SurgeRulesTests
    {
        private static ManaPool Full()
        {
            var pool = new ManaPool { Maximum = 100f };
            pool.Empty();
            pool.Gain(100f);
            return pool;
        }

        [Test]
        public void KindleTradesItsBurnForFedFlame()
        {
            var kindle = ElementKit.Fire();
            Assert.AreEqual(0f, kindle.SurgeBurnSeconds, 1e-5f, "no burn on every blow");
            Assert.AreEqual(0.4f, kindle.SurgeFeeds, 1e-5f);
            Assert.AreEqual(2f, kindle.SurgeSmothers, 1e-5f);
            Assert.IsTrue(kindle.TrySurge(Full()));
            Assert.AreEqual(10f, kindle.SurgeLeft, 1e-4f);
            kindle.Feed(kindle.SurgeFeeds);
            Assert.AreEqual(10.4f, kindle.SurgeLeft, 1e-4f, "a blow on a burning enemy feeds it 0.4 s");
            for (int i = 0; i < 100; i++) kindle.Feed(kindle.SurgeFeeds);
            Assert.AreEqual(20f, kindle.SurgeLength, 1e-4f, "never past twice its length");
            kindle.Smother(kindle.SurgeSmothers);
            Assert.AreEqual(18f, kindle.SurgeLeft, 1e-3f, "a blow taken smothers 2 s");
            kindle.Smother(100f);
            Assert.IsTrue(kindle.SurgeActive, "smothered out, it ends on its own next tick");
            Assert.IsTrue(kindle.TickSurge(0.01f), "and that tick ends it, starting its wait as any end does");
            Assert.AreEqual(20f, ElementKit.FedMost(10f), 1e-5f);
        }

        [Test]
        public void AFedSurgeLitAgainStartsFromItsOwnLength()
        {
            var kindle = ElementKit.Fire();
            kindle.TrySurge(Full());
            for (int i = 0; i < 100; i++) kindle.Feed(0.4f);
            kindle.TrySurge(Full());
            Assert.AreEqual(10f, kindle.SurgeLength, 1e-4f, "lit again: its own ten seconds");
        }

        [Test]
        public void SkyDancesMasterLightsTailwindForFreeWhenEveryCutIsInTime()
        {
            var master = Upgrade.ById(SpellBranch.MasterId(Clover.Player.Trees.WindTree.SkyDance)).Effects;
            var gift = System.Linq.Enumerable.Single(master);
            Assert.AreEqual(UpgradeTrigger.DanceInTime, gift.On, "only when every cut was in time");
            Assert.AreEqual(UpgradeAction.FreeSurge, gift.Do);
            Assert.AreEqual(Elements.Wind, gift.SurgeOf, "Tailwind itself");
            Assert.AreEqual(4f, gift.Seconds, 1e-5f);
            Assert.IsTrue(PlayerUpgrades.AllInTime(5, 5));
            Assert.IsFalse(PlayerUpgrades.AllInTime(4, 5), "one late cut and nothing is lit");
            Assert.IsTrue(PlayerUpgrades.AllInTime(6, 6), "six rings with the Empower, all six");
            Assert.IsFalse(PlayerUpgrades.AllInTime(0, 0));
            var tailwind = ElementKit.Wind();
            Assert.IsTrue(tailwind.LightFree(4f), "lit at no cost");
            Assert.AreEqual(4f, tailwind.SurgeLeft, 1e-4f);
            Assert.IsFalse(tailwind.LightFree(2f), "already lit: kept, not lit again");
            Assert.AreEqual(4f, tailwind.SurgeLeft, 1e-4f, "never shortened");
        }

        [Test]
        public void OverdriveAndWellspringCarryTheirNewParts()
        {
            Assert.IsTrue(ElementKit.Lightning().SurgeCritsChainEnd, "the last blow of every chain is critical");
            Assert.IsFalse(ElementKit.Fire().SurgeCritsChainEnd);
            Assert.IsTrue(ElementKit.Water().SurgeFlaskBreaks, "each flask drunk bursts out as a ring");
            Assert.IsFalse(ElementKit.Lightning().SurgeFlaskBreaks);
            Assert.AreEqual(2f, PlayerUpgrades.FlaskRingRadius, 1e-5f, "4 m across");
            Assert.AreEqual(20f, PlayerUpgrades.FlaskRingDamage, 1e-5f);
            Assert.AreEqual(30f, PlayerUpgrades.FlaskRingStagger, 1e-5f);
            Assert.AreEqual(2.5f, PlayerUpgrades.FlaskRingThrow, 1e-5f);
            Assert.AreEqual(4f, PlayerUpgrades.FlaskRingCut, 1e-5f, "4 s off every running wait");
        }
    }
}
