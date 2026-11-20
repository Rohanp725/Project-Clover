using System;
using System.Linq;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The plain rules of the marks built to their designs on 2026-11-17: Sunspot, Lodestone, Wildfire Rush's Heat, Waiting Thunder and Scorched Earth.</summary>
    public class MarksBuiltTests
    {
        [Test]
        public void SunspotIsFedUpToItsLimitAndBurstsBiggerForEveryBlowFed()
        {
            Assert.AreEqual(1, SunspotRules.Feed(0, 6));
            Assert.AreEqual(6, SunspotRules.Feed(5, 6));
            Assert.AreEqual(6, SunspotRules.Feed(6, 6), "A seventh blow feeds nothing.");
            Assert.AreEqual(12f, SunspotRules.Damage(12f, 0), 1e-4f);
            Assert.AreEqual(66f, SunspotRules.Damage(12f, 6), 1e-4f);
            Assert.AreEqual(58f, SunspotRules.Stagger(10f, 6), 1e-4f);
            Assert.AreEqual(0.5f, SunspotRules.LeftAlone);
        }

        [Test]
        public void LodestoneDragsFurtherForEveryStackOfStaticAndNeverPastSwordReach()
        {
            Assert.AreEqual(1f, LodestoneRules.Dragged(7f, 0), 1e-4f);
            Assert.AreEqual(5f, LodestoneRules.Dragged(7f, 5), 1e-4f);
            Assert.AreEqual(0.7f, LodestoneRules.Dragged(2f, 5), 1e-4f, "It stops at sword reach.");
            Assert.AreEqual(0f, LodestoneRules.Dragged(1f, 3), 1e-4f, "One already inside sword reach is not moved.");
        }

        [Test]
        public void HeatIsRaisedAWholeLevelForEachEnemyDrivenThroughUpToItsMost()
        {
            var heat = new Heat();
            heat.Raise(2);
            Assert.AreEqual(2, heat.Level);
            heat.Raise(0);
            Assert.AreEqual(2, heat.Level);
            heat.Raise(50);
            Assert.AreEqual(heat.MaximumLevel, heat.Level);
        }

        [Test]
        public void TwinBreakSpendsBurnAndStaticTogetherAtHalfAgain()
        {
            Assert.AreEqual((10f * 3 + 7f * 2) * 1.5f, PlasmaRules.TwinDamage(3, 2), 1e-4f);
            Assert.AreEqual(25f, PlasmaRules.TwinStagger(3, 2), 1e-4f);
            Assert.AreEqual(0f, PlasmaRules.TwinDamage(-1, 0), 1e-4f);
        }

        [Test]
        public void AnEnemyMeltsDownOnlyAtFullBurnAndFullStatic()
        {
            Assert.IsTrue(PlasmaRules.Melts(5, 5, 5, 5));
            Assert.IsFalse(PlasmaRules.Melts(5, 4, 5, 5));
            Assert.IsFalse(PlasmaRules.Melts(4, 5, 5, 5));
            Assert.IsFalse(PlasmaRules.Melts(0, 0, 0, 0));
            Assert.Less(PlasmaRules.Pulse(0.2f), PlasmaRules.Pulse(2.8f), "The light quickens as the burst comes.");
            Assert.IsEmpty(Upgrade.ById(PlasmaTree.Meltdown).Effects, "Meltdown is done by hand now: no burst left in its data.");
        }

        [Test]
        public void StormFrontCarriesNoStandInConeOrPushAnyMore()
        {
            var mark = Upgrade.ById(WindTree.StormFront);
            Assert.IsEmpty(mark.Effects, "The free Gust is thrown by hand now; the cone and the push that stood in for it are gone.");
            Assert.IsTrue(mark.Stats.Any(mod => mod.Stat == PlayerStat.ManaFromBlows && mod.Amount < 0f), "Its price is less mana from blows.");
        }

        [Test]
        public void DeepReserveKeepsACountOfThreeAndNoBoon()
        {
            var mark = Upgrade.ById(LightningTree.DeepReserve);
            Assert.IsNotNull(mark.Tally, "Deep Reserve's charges are a count the HUD can show.");
            Assert.AreEqual(PlayerUpgrades.ReserveMost, mark.Tally.Cap);
            Assert.IsEmpty(mark.Effects, "The four seconds of harder blows that stood in for the charges are gone.");
        }

        [Test]
        public void TheMarksBuiltWearTheirOwnForms()
        {
            // Sunspot is retired (Phase 4): Black Pitch took its place.
            Assert.IsNull(Upgrade.ById("fire.sunspot"));
            Assert.AreEqual(SpellForms.Lodestone, Upgrade.ById("lightning.lodestone").Form);
            Assert.AreEqual(SpellForms.Waiting, Upgrade.ById("lightning.waiting").Form);
            foreach (string id in new[] { SpellForms.Sunspot, SpellForms.Lodestone, SpellForms.Waiting })
                Assert.IsNotNull(SpellForms.Get(id), $"The form '{id}' is not registered.");
            Assert.IsInstanceOf<PhalanxForm>(SpellForms.Get(SpellForms.Waiting));
            Assert.Greater(Upgrade.ById(FireTree.WildfireRush).Spell.LingerDamage, 0f, "Wildfire Rush leaves no line of fire without a linger.");
        }

        [Test]
        public void ScorchedEarthIsRetiredWithTheKeystonesNotCarriedOver()
        {
            // elements_a.md, "What to cut": its "spells cost more" means nothing once basic spells run on cooldowns.
            Assert.IsNull(Upgrade.ById(FireTree.ScorchedEarth));
            Assert.IsTrue(RetiredMarks.IsRetired(FireTree.ScorchedEarth));
            Assert.IsTrue(RetiredMarks.IsRetired(FireTree.OpenFurnace));
            Assert.AreEqual(Enum.GetValues(typeof(StatWhen)).Length, PlayerStats.WhenCount, "Every state a number may wait on has its place.");
        }
    }
}
