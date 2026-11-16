using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    public class TipRulesTests
    {
        // A scene that has been open long enough for tips to be allowed.
        private static TipRules Settled()
        {
            var rules = new TipRules();
            rules.BeginScene(false);
            Assert.IsNull(rules.Tick(rules.QuietSeconds + 0.1f));
            return rules;
        }

        [Test]
        public void TheGuardTipComesOnTheSecondUnguardedBlowAndNeverAgain()
        {
            var rules = Settled();
            rules.UnguardedBlow();
            Assert.IsNull(rules.Tick(0.1f));
            rules.UnguardedBlow();
            Assert.AreEqual(TipRules.GuardText, rules.Tick(0.1f));
            Assert.IsTrue(rules.WasShown(Tip.Guard));

            rules.UnguardedBlow();
            rules.UnguardedBlow();
            Assert.IsNull(rules.Tick(rules.SpacingSeconds + 1f));
            Assert.IsFalse(rules.IsWaiting(Tip.Guard));
        }

        [Test]
        public void TwoBlowsTurnedMustBeCloseTogether()
        {
            var rules = Settled();
            rules.BlowTurned();
            Assert.IsNull(rules.Tick(rules.TurnedWindowSeconds + 0.5f));
            rules.BlowTurned();
            Assert.IsNull(rules.Tick(1f));
            rules.BlowTurned();
            Assert.AreEqual(TipRules.BreakGuardText, rules.Tick(0.1f));
        }

        [Test]
        public void OnlyAStaggerNearThePlayerEarnsTheFinishTip()
        {
            var rules = Settled();
            rules.EnemyStaggered(rules.StaggerRange + 1f);
            Assert.IsNull(rules.Tick(0.1f));
            rules.EnemyStaggered(rules.StaggerRange - 1f);
            Assert.AreEqual(TipRules.FinishText, rules.Tick(0.1f));
        }

        [Test]
        public void NothingIsSaidWhileASceneIsNew()
        {
            var rules = new TipRules();
            rules.BeginScene(false);
            Assert.IsNull(rules.Tick(rules.QuietSeconds - 1.5f));
            rules.EnemyStaggered(1f);
            Assert.IsNull(rules.Tick(1f));
            Assert.AreEqual(TipRules.FinishText, rules.Tick(1f));
        }

        [Test]
        public void TipsKeepTheirDistanceFromEachOther()
        {
            var rules = Settled();
            rules.EnemyStaggered(1f);
            Assert.AreEqual(TipRules.FinishText, rules.Tick(0.1f));

            float waitedFor = 0f;
            string said = null;
            // The blows keep being turned, so the tip keeps being asked for until there is room for it.
            while (said == null && waitedFor < 20f)
            {
                rules.BlowTurned();
                said = rules.Tick(0.5f);
                waitedFor += 0.5f;
            }
            Assert.AreEqual(TipRules.BreakGuardText, said);
            Assert.GreaterOrEqual(waitedFor, rules.SpacingSeconds);
            Assert.Less(waitedFor, rules.SpacingSeconds + 1f);
        }

        [Test]
        public void ATipWaitsWhileTheLineIsBusy()
        {
            var rules = Settled();
            rules.EnemyStaggered(1f);
            rules.LineUsed();
            Assert.IsNull(rules.Tick(rules.BusySeconds * 0.5f));
            Assert.IsTrue(rules.IsWaiting(Tip.Finish));
            Assert.IsFalse(rules.WasShown(Tip.Finish));
            Assert.AreEqual(TipRules.FinishText, rules.Tick(rules.BusySeconds * 0.6f));
        }

        [Test]
        public void ATipHeldTooLongIsLetGoAndAskedForAgainNextTime()
        {
            var rules = Settled();
            rules.EnemyStaggered(1f);
            // The line is kept busy for longer than a tip will wait.
            for (float held = 0f; held <= rules.HoldSeconds; held += 0.5f)
            {
                rules.LineUsed();
                Assert.IsNull(rules.Tick(0.5f));
            }
            Assert.IsFalse(rules.IsWaiting(Tip.Finish));
            Assert.IsNull(rules.Tick(rules.BusySeconds + 1f));

            rules.EnemyStaggered(1f);
            Assert.AreEqual(TipRules.FinishText, rules.Tick(0.1f));
        }

        [Test]
        public void TheHubIsSilentAndCountsNothing()
        {
            var rules = new TipRules();
            rules.BeginScene(true);
            rules.UnguardedBlow();
            rules.UnguardedBlow();
            rules.EnemyStaggered(1f);
            rules.BlowTurned();
            rules.BlowTurned();
            Assert.IsNull(rules.Tick(rules.QuietSeconds + 1f));

            // Leaving for the castle: the blows taken in the hub were not counted.
            rules.BeginScene(false);
            Assert.IsNull(rules.Tick(rules.QuietSeconds + 1f));
            rules.UnguardedBlow();
            Assert.IsNull(rules.Tick(0.1f));
            rules.UnguardedBlow();
            Assert.AreEqual(TipRules.GuardText, rules.Tick(0.1f));
        }

        [Test]
        public void ANewSceneDropsWhatWasWaitingButRemembersWhatWasSaid()
        {
            var rules = Settled();
            rules.EnemyStaggered(1f);
            Assert.AreEqual(TipRules.FinishText, rules.Tick(0.1f));
            rules.UnguardedBlow();
            rules.UnguardedBlow();
            Assert.IsTrue(rules.IsWaiting(Tip.Guard));

            rules.BeginScene(false);
            Assert.IsFalse(rules.IsWaiting(Tip.Guard));
            Assert.IsNull(rules.Tick(rules.QuietSeconds + rules.SpacingSeconds));
            rules.EnemyStaggered(1f);
            Assert.IsNull(rules.Tick(0.1f));
            Assert.IsTrue(rules.WasShown(Tip.Finish));
        }

        [Test]
        public void AFinisherLandedUnpromptedLeavesNothingToTell()
        {
            var rules = Settled();
            rules.Learned(Tip.Finish);
            rules.EnemyStaggered(1f);
            Assert.IsNull(rules.Tick(0.1f));
        }

        [Test]
        public void TheFreshestOfTwoWaitingTipsIsSaidFirst()
        {
            var rules = Settled();
            rules.LineUsed();
            rules.UnguardedBlow();
            rules.UnguardedBlow();
            Assert.IsNull(rules.Tick(0.4f));
            rules.EnemyStaggered(1f);
            Assert.AreEqual(TipRules.FinishText, rules.Tick(rules.BusySeconds));
        }
    }
}
