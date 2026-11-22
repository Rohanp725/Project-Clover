using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The player's own rules (standin_clearance_plan.md pass 7): a light blow made heavy, the leaf on the blade, being
    /// unshaken near the garden, and Earth's charged blow that cannot be stopped.
    /// </summary>
    public class PlayerRulesTests
    {
        [Test]
        public void ALightBlowIsMadeHeavyOnlyInItsWindow()
        {
            Assert.IsTrue(PlayerCombat.MadeHeavy(false, 5f, 4.5f), "a light blow inside the window lands as a heavy");
            Assert.IsFalse(PlayerCombat.MadeHeavy(false, 5f, 5f), "the window has closed");
            Assert.IsFalse(PlayerCombat.MadeHeavy(true, 5f, 4.5f), "a heavy blow is heavy already, and spends nothing");
            Assert.IsFalse(PlayerCombat.MadeHeavy(false, 0f, 0f), "no window, no change");
            Assert.AreEqual(40f, PlayerCombat.HeavyStagger(12f, 40f), 1e-5f, "it breaks the heavy blow's balance");
            Assert.AreEqual(12f, PlayerCombat.HeavyStagger(12f, 0f), 1e-5f, "never less than its own");
        }

        [Test]
        public void GoneToAshsFirstBlowIsAHeavyBlow()
        {
            var learn = Upgrade.ById(DarkTree.GoneToAsh).Effects;
            var heavy = learn.Single(each => each.Do == UpgradeAction.NextHeavy);
            Assert.AreEqual(UpgradeTrigger.SpellHit, heavy.On);
            Assert.AreEqual(1f, heavy.Seconds, 1e-5f, "the first blow within a second");
            Assert.IsFalse(learn.Any(each => each.Do == UpgradeAction.Boon && each.Stat == PlayerStat.WeaponDamage), "not a blow twice as hard any more");
        }

        [Test]
        public void TheGardenHoldsYouKeepsYouUnshakenNearWhatYouGrew()
        {
            var mod = Upgrade.ById(LifeTree.TheGardenHoldsYou).Stats.Single();
            Assert.AreEqual(PlayerStat.Unshaken, mod.Stat);
            var stats = new PlayerStats();
            stats.AddLearned(mod);
            stats.SetWhen(StatWhen.NearGrown, false);
            Assert.IsFalse(PlayerUpgrades.IsUnshaken(stats), "away from the garden a blow can still throw you");
            stats.SetWhen(StatWhen.NearGrown, true);
            Assert.IsTrue(PlayerUpgrades.IsUnshaken(stats), "within 4 m of something grown");
            Assert.IsFalse(PlayerUpgrades.IsUnshaken(null));
        }

        [Test]
        public void EarthsChargedBlowCannotBeStopped()
        {
            Assert.IsTrue(ElementSteel.Unstoppable(Elements.Earth));
            Assert.IsFalse(ElementSteel.Unstoppable(Elements.Fire));
        }

        [Test]
        public void BloodPriceCastsASpellStillWaitingForHealth()
        {
            var mod = Upgrade.ById(DarkTree.BloodPrice).Stats.Single();
            Assert.AreEqual(PlayerStat.CooldownPrice, mod.Stat, "re-aimed at the wait, not the mana");
            Assert.AreEqual(1f, mod.Amount, 1e-5f, "1 health a second left");
            Assert.AreEqual(4f, PlayerUpgrades.CooldownHealth(4f, 1f, 50f), 1e-5f, "four seconds left, four health owed");
            Assert.AreEqual(-1f, PlayerUpgrades.CooldownHealth(4f, 1f, 4.5f), 1e-5f, "never the last point");
            Assert.AreEqual(-1f, PlayerUpgrades.CooldownHealth(4f, 0f, 50f), 1e-5f, "without the mark a waiting spell waits");
            Assert.AreEqual(0f, PlayerUpgrades.CooldownHealth(0f, 0f, 50f), 1e-5f, "a ready spell costs nothing more");
        }

        [Test]
        public void PouredOnTheEdgeIsPaidInAFlaskAndHoldsForItsTwelveSeconds()
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(WaterTree.PouredBlade)).Shapes.First(each => each.Name == "Poured on the Edge");
            Assert.AreEqual(1, shape.Spell.FlaskCost, "a flask, not mana");
            Assert.IsTrue(shape.Effects.Any(each => each.Do == UpgradeAction.Boon && each.Stat == PlayerStat.WeaponReach && System.Math.Abs(each.Amount - 0.4f) < 1e-5f), "the reach 40% longer");
            foreach (var each in shape.Effects.Where(each => each.On == UpgradeTrigger.AnyHit || each.On == UpgradeTrigger.ChainEnd))
                Assert.AreEqual(12f, each.WithinOwnCast, 1e-5f, each.Do + " only within its 12 s");
            Assert.IsTrue(shape.Effects.Any(each => each.On == UpgradeTrigger.AnyHit && each.Do == UpgradeAction.Heal && each.Amount == 2f), "every blow mends 2");
            Assert.IsTrue(PlayerUpgrades.CanPayFlasks(1, 1, false));
            Assert.IsFalse(PlayerUpgrades.CanPayFlasks(1, 0, false), "no flask, no cast");
            Assert.IsFalse(PlayerUpgrades.CanPayFlasks(1, 2, true), "not with a flask at the lips");
            Assert.IsTrue(PlayerUpgrades.CanPayFlasks(0, 0, false), "a spell paid in mana asks no flask");
            Assert.IsTrue(PlayerUpgrades.InCastWindow(10f, 21f, 12f));
            Assert.IsFalse(PlayerUpgrades.InCastWindow(10f, 22.5f, 12f), "over after 12 s");
            Assert.IsFalse(PlayerUpgrades.InCastWindow(-100f, 0f, 12f), "never cast");
        }

        [Test]
        public void SaplingsShapesSpendTheWindowsSaplings()
        {
            var learn = Upgrade.ById(LifeTree.Sapling);
            var bank = learn.Effects.Single(each => each.Do == UpgradeAction.Bank);
            Assert.AreEqual(8f, bank.Seconds, 1e-5f, "the window is 8 s");
            Assert.AreEqual(3, bank.Count);
            Assert.AreEqual(4, Upgrade.ById(SpellBranch.EmpowerId(LifeTree.Sapling)).Empower.Effects.Single().Count, "four with the Empower");
            Assert.That(learn.Tally.Cap, Is.GreaterThanOrEqualTo(4));
            var plantings = learn.Effects.Where(each => each.Do == UpgradeAction.Hazard).ToArray();
            Assert.AreEqual(2, plantings.Length);
            Assert.AreEqual(PlayerUpgrades.FamilyOf(plantings[0]), PlayerUpgrades.FamilyOf(plantings[1]), "three standing at most between the guard's and the dodge's");
            foreach (var shape in Upgrade.ById(SpellBranch.ShapeId(LifeTree.Sapling)).Shapes.Where(each => each.Name == "Strangler" || each.Name == "Dodder"))
            {
                CollectionAssert.AreEqual(new[] { 1, 2 }, shape.SetsAside, shape.Name + " in place of the sapling");
                foreach (var each in shape.Effects.Where(each => each.On == UpgradeTrigger.PerfectGuard || each.On == UpgradeTrigger.PerfectDodge))
                    Assert.IsTrue(each.Spends == 1 && each.PaysLearn, shape.Name + " pays one of the window's saplings");
            }
        }

        [Test]
        public void TheFavouredOaksLeafRidesTheBladeToTheNextDischarge()
        {
            var oak = Upgrade.ById(LightningFlowerDuo.TheFavouredOak);
            Assert.AreEqual(1, oak.Tally.Cap, "one leaf at a time");
            var hang = oak.Effects.Single(each => each.Do == UpgradeAction.Bank);
            Assert.AreEqual(UpgradeTrigger.PerfectGuard, hang.On);
            Assert.AreEqual(AfflictionKind.Seeded, hang.Needs, "a perfect guard against a Seeded enemy");
            Assert.AreEqual(8f, hang.Seconds, 1e-5f, "the leaf falls after 8 s");
            var again = oak.Effects.Single(each => each.On == UpgradeTrigger.SetOff);
            Assert.AreEqual(AfflictionKind.Static, again.Spent, "only a discharge");
            Assert.AreEqual(1, again.Spends, "the leaf is spent");
            Assert.IsTrue(again.ByPower, "the discharge strikes again at its own strength: twice in all");
            Assert.IsFalse(oak.Effects.Any(each => each.Do == UpgradeAction.Condition), "nothing hung on the enemy");
        }
    }
}
