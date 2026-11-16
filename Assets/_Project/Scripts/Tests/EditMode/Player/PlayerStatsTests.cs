using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The sum of what changes the player's numbers: learned upgrades, the surge, and boons that run out.</summary>
    public class PlayerStatsTests
    {
        [Test]
        public void SharesOfOneStatAreAddedTogetherAndApplyAsOneMultiplier()
        {
            var stats = new PlayerStats();
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.05f));
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.10f));
            stats.AddLearned(new StatMod(PlayerStat.SpellCost, -0.2f));
            Assert.AreEqual(1.15f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            Assert.AreEqual(0.8f, stats.Multiplier(PlayerStat.SpellCost), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.StaggerDealt), 0.0001f);
        }

        [Test]
        public void NoSumTakesANumberBelowATenthOfItself()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.DamageTaken, -5f));
            Assert.AreEqual(PlayerStats.LeastMultiplier, stats.Multiplier(PlayerStat.DamageTaken), 0.0001f);
        }

        [Test]
        public void FlatStatsArePlainAmounts()
        {
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.Health));
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.FlaskCount));
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.CritChance));
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.AfflictionStacks));
            Assert.IsFalse(PlayerStats.IsFlat(PlayerStat.WeaponDamage));
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.Health, 15f));
            stats.AddLearned(new StatMod(PlayerStat.FlaskCount, 1f));
            stats.AddLearned(new StatMod(PlayerStat.FlaskCount, 1f));
            stats.AddLearned(new StatMod(PlayerStat.CritChance, 0.05f));
            Assert.AreEqual(15f, stats.Sum(PlayerStat.Health), 0.0001f);
            Assert.AreEqual(2, stats.Whole(PlayerStat.FlaskCount));
            Assert.AreEqual(0.05f, stats.Sum(PlayerStat.CritChance), 0.0001f);
            Assert.AreEqual(0, stats.Whole(PlayerStat.AfflictionStacks));
        }

        [Test]
        public void ABoonLastsItsSecondsAndThenIsGone()
        {
            var stats = new PlayerStats();
            stats.AddBoon(PlayerStat.MoveSpeed, 0.25f, 3f);
            Assert.AreEqual(1.25f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
            Assert.AreEqual(1, stats.BoonsRunning);
            stats.Tick(2.9f);
            Assert.AreEqual(1.25f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
            stats.Tick(0.2f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
            Assert.AreEqual(0, stats.BoonsRunning);
        }

        [Test]
        public void TheSameBoonGivenAgainStartsItsTimeOverAndDoesNotStackWithItself()
        {
            var stats = new PlayerStats();
            stats.AddBoon(PlayerStat.WeaponDamage, 0.2f, 2f);
            stats.Tick(1.5f);
            stats.AddBoon(PlayerStat.WeaponDamage, 0.2f, 2f);
            Assert.AreEqual(1.2f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            Assert.AreEqual(1, stats.BoonsRunning);
            stats.Tick(1.5f);
            Assert.AreEqual(1.2f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f, "the second giving started the time over");
            // A different boon of the same stat does add.
            stats.AddBoon(PlayerStat.WeaponDamage, 0.1f, 5f);
            Assert.AreEqual(1.3f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
        }

        [Test]
        public void LearnedSurgeAndBoonsAddTogetherAndEachCanBeClearedAlone()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.DodgeDistance, 0.1f));
            stats.AddSurge(new StatMod(PlayerStat.DodgeDistance, 0.3f));
            stats.AddBoon(PlayerStat.DodgeDistance, 0.2f, 10f);
            Assert.AreEqual(1.6f, stats.Multiplier(PlayerStat.DodgeDistance), 0.0001f);
            stats.ClearSurge();
            Assert.AreEqual(1.3f, stats.Multiplier(PlayerStat.DodgeDistance), 0.0001f);
            stats.ClearLearned();
            Assert.AreEqual(1.2f, stats.Multiplier(PlayerStat.DodgeDistance), 0.0001f);
            stats.Clear();
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.DodgeDistance), 0.0001f);
        }

        [Test]
        public void TheVersionGoesUpWheneverASumChangesSoReadersKnowToLookAgain()
        {
            var stats = new PlayerStats();
            int before = stats.Version;
            stats.Tick(1f);
            Assert.AreEqual(before, stats.Version, "nothing changed");
            stats.AddBoon(PlayerStat.SwingSpeed, 0.1f, 1f);
            Assert.Greater(stats.Version, before);
            before = stats.Version;
            stats.Tick(2f);
            Assert.Greater(stats.Version, before, "the boon ran out");
        }

        [Test]
        public void MoreBoonsThanThereIsRoomForPushOutTheOneNearestItsEnd()
        {
            var stats = new PlayerStats();
            for (int i = 0; i < PlayerStats.BoonCapacity; i++) stats.AddBoon(PlayerStat.WeaponDamage, 0.01f * (i + 1), 10f + i);
            Assert.AreEqual(PlayerStats.BoonCapacity, stats.BoonsRunning);
            float before = stats.Sum(PlayerStat.WeaponDamage);
            stats.AddBoon(PlayerStat.StaggerDealt, 0.5f, 60f);
            Assert.AreEqual(PlayerStats.BoonCapacity, stats.BoonsRunning);
            Assert.AreEqual(before - 0.01f, stats.Sum(PlayerStat.WeaponDamage), 0.0001f, "the first, with the least time left, gave way");
            Assert.AreEqual(1.5f, stats.Multiplier(PlayerStat.StaggerDealt), 0.0001f);
        }
    }
}
