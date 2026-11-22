using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// What one cast holds for itself (the owner's answers of 2026-11-22): The Yellow Gale's store split among those it
    /// struck, and One by One's Master making only One by One's own bursts stronger.
    /// </summary>
    public class CastScopedTests
    {
        [Test]
        public void TheGalesStoreIsSplitEvenlyAndRoundedUp()
        {
            Assert.AreEqual(7, DepthMoves.ShareEach(7, 1), "one lone enemy takes it all");
            Assert.AreEqual(4, DepthMoves.ShareEach(7, 2), "seven between two is four each, rounded up");
            Assert.AreEqual(3, DepthMoves.ShareEach(7, 3));
            Assert.AreEqual(1, DepthMoves.ShareEach(2, 5), "a thin store still gives each one a dose");
            Assert.AreEqual(0, DepthMoves.ShareEach(0, 3), "nothing stored, nothing shared");
            Assert.AreEqual(0, DepthMoves.ShareEach(6, 0), "no one struck, nothing laid");
            Assert.AreEqual(2, DepthMoves.Half(5), "half of five doses, rounded down");
            Assert.AreEqual(0, DepthMoves.Half(1), "one dose is left where it is");
            Assert.AreEqual(0, DepthMoves.Half(-3));
        }

        [Test]
        public void TheYellowGaleDrinksIntoAStoreAsItIsCast()
        {
            var gale = Upgrade.ById(IllWindDuo.TheYellowGale);
            var store = gale.Effects.Single(each => each.Do == UpgradeAction.ShareStore);
            Assert.AreEqual(UpgradeTrigger.SpellCast, store.On, "the store is shared once the gale has struck");
            Assert.IsTrue(store.OwnSpell, "only the gale's own cast");
            Assert.AreEqual(AfflictionKind.Poison, store.Affliction);
            Assert.AreEqual(6f, store.Radius, 1e-5f, "everything within 6 m is drunk");
        }

        [Test]
        public void ACastsBoonCountsOnlyForThatCast()
        {
            var boons = new CastBoons();
            boons.Add(7, PlayerStat.BurstDamage, 0.5f, 10f);
            Assert.AreEqual(1.5f, boons.Multiplier(7, PlayerStat.BurstDamage, 9f), 1e-5f, "the cast that earned it bursts half again as hard");
            Assert.AreEqual(1f, boons.Multiplier(8, PlayerStat.BurstDamage, 9f), 1e-5f, "another cast's bursts are as they were");
            Assert.AreEqual(1f, boons.Multiplier(0, PlayerStat.BurstDamage, 9f), 1e-5f, "a burst of no cast (a reaction's, Volatile's) is as it was");
            Assert.AreEqual(1f, boons.Multiplier(7, PlayerStat.SpellDamage, 9f), 1e-5f, "only the stat it names");
            Assert.AreEqual(1f, boons.Multiplier(7, PlayerStat.BurstDamage, 10f), 1e-5f, "over once its time is up");
            boons.Add(0, PlayerStat.BurstDamage, 0.5f, 10f);
            Assert.AreEqual(1f, boons.Multiplier(0, PlayerStat.BurstDamage, 9f), 1e-5f, "no cast holds nothing");
            for (int i = 0; i < CastBoons.Most; i++) boons.Add(20 + i, PlayerStat.BurstDamage, 1f, 10f);
            Assert.AreEqual(1f, boons.Multiplier(7, PlayerStat.BurstDamage, 9f), 1e-5f, "a new one takes the oldest one's place");
            Assert.AreEqual(2f, boons.Multiplier(20 + CastBoons.Most - 1, PlayerStat.BurstDamage, 9f), 1e-5f);
            boons.Clear();
            Assert.AreEqual(1f, boons.Multiplier(20, PlayerStat.BurstDamage, 9f), 1e-5f);
        }

        [Test]
        public void OneByOnesMasterIsHeldByItsOwnCast()
        {
            var master = Upgrade.ById(SpellBranch.MasterId(PoisonTree.OneByOne));
            var boon = master.Effects.Single(each => each.Do == UpgradeAction.Boon);
            Assert.IsTrue(boon.ThisCast, "only One by One's own bursts, not every burst in that moment");
            Assert.IsTrue(boon.OnCastBeat, "still only on the poison's beat");
            Assert.AreEqual(PlayerStat.BurstDamage, boon.Stat);
            Assert.AreEqual(0.5f, boon.Amount, 1e-5f);
        }
    }
}
