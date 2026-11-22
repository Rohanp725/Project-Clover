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
