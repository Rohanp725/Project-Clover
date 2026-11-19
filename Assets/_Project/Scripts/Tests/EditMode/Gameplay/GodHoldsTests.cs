using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// How ice and holds take on a god (the owner's decision of 2026-11-19): the man for a moment, the godly form for a
    /// hitch, neither again until a guard has passed; elites and plain enemies as before.
    /// </summary>
    public class GodHoldsTests
    {
        private const float Near = 0.0001f;

        [SetUp]
        public void PutTheNumbersBack() => GodHolds.Restore();

        [TearDown]
        public void PutTheNumbersBackAfter() => GodHolds.Restore();

        private static AfflictionSet Man() => new AfflictionSet { FreezeScale = 0.4f, IsGod = true };
        private static AfflictionSet Godly() => new AfflictionSet { CanFreeze = false, IsGod = true, InGodlyForm = true };
        private static AfflictionSet Elite() => new AfflictionSet { FreezeScale = 0.4f };

        private static float PlainFreeze => Afflictions.Spec(AfflictionKind.Chill).FrozenSeconds * Afflictions.FreezeSeconds;
        private static int FullChill => Afflictions.Depth(AfflictionKind.Chill);

        // Ticks in small steps, as frames would, so a guard or a hold is seen ending on the step it ends.
        private static void Wait(AfflictionSet set, float seconds)
        {
            for (float waited = 0f; waited < seconds; waited += 0.05f) set.Tick(0.05f);
        }

        [Test]
        public void EachPhaseHasItsShareItsCapAndItsGuard()
        {
            Assert.AreEqual(0.3f, GodHolds.Share(GodPhase.Man), Near);
            Assert.AreEqual(1f, GodHolds.Cap(GodPhase.Man), Near);
            Assert.AreEqual(3f, GodHolds.Guard(GodPhase.Man), Near);
            Assert.AreEqual(0.12f, GodHolds.Share(GodPhase.Godly), Near);
            Assert.AreEqual(0.4f, GodHolds.Cap(GodPhase.Godly), Near);
            Assert.AreEqual(5f, GodHolds.Guard(GodPhase.Godly), Near);
            Assert.Less(GodHolds.Share(GodPhase.Man), 0.4f, "the man is held for less than an elite");
            Assert.Less(GodHolds.Share(GodPhase.Godly), GodHolds.Share(GodPhase.Man), "the godly form for much less than the man");
            Assert.AreEqual(1f, GodHolds.Share(GodPhase.None));
            Assert.AreEqual(float.PositiveInfinity, GodHolds.Cap(GodPhase.None));
            Assert.AreEqual(0f, GodHolds.Guard(GodPhase.None));
        }

        [Test]
        public void SecondsTakesThePhasesShareAndNeverMoreThanItsCap()
        {
            Assert.AreEqual(0.6f, GodHolds.Seconds(2f, GodPhase.Man), Near);
            Assert.AreEqual(1f, GodHolds.Seconds(5f, GodPhase.Man), Near, "capped");
            Assert.AreEqual(0.24f, GodHolds.Seconds(2f, GodPhase.Godly), Near);
            Assert.AreEqual(0.4f, GodHolds.Seconds(5f, GodPhase.Godly), Near, "capped");
            Assert.AreEqual(0f, GodHolds.Seconds(0f, GodPhase.Man));
            Assert.AreEqual(0f, GodHolds.Seconds(-1f, GodPhase.Godly));
            // What is no god keeps its own scale, the god's numbers ignored.
            Assert.AreEqual(1f, GodHolds.Seconds(2.5f, GodPhase.None, 0.4f), Near);
            Assert.AreEqual(2.5f, GodHolds.Seconds(2.5f, GodPhase.None), Near);
        }

        [Test]
        public void ElitesAndPlainEnemiesAreFrozenAndHeldAsBefore()
        {
            var elite = Elite();
            elite.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsTrue(elite.IsFrozen);
            Assert.AreEqual(PlainFreeze * 0.4f, elite.FrozenLeft, Near);
            var plain = new AfflictionSet();
            plain.Apply(AfflictionKind.Chill, FullChill);
            Assert.AreEqual(PlainFreeze, plain.FrozenLeft, Near);

            Assert.IsTrue(plain.Hold(2f));
            Assert.AreEqual(2f, plain.HeldLeft, Near);
            Assert.IsTrue(plain.Hold(3f), "a plain enemy's hold is made longer by another");
            Assert.AreEqual(3f, plain.HeldLeft, Near);
            var held = Elite();
            Assert.IsTrue(held.Hold(2f));
            Assert.AreEqual(0.8f, held.HeldLeft, Near);
            Wait(held, 1f);
            Assert.IsFalse(held.IsHeld);
            Assert.AreEqual(0f, held.GuardLeft, "no guard on what is no god");
            Assert.IsTrue(held.Hold(2f), "an elite can be held again at once");
            Assert.AreEqual(EnemyRank.Elite, held.Rank);
        }

        [Test]
        public void TheManIsFrozenAndHeldForLessThanAnEliteAndNeverOverTheCap()
        {
            var man = Man();
            Assert.AreEqual(GodPhase.Man, man.GodPhase);
            Assert.AreEqual(EnemyRank.God, man.Rank);
            man.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsTrue(man.IsFrozen);
            Assert.AreEqual(System.Math.Min(PlainFreeze * 0.3f, 1f), man.FrozenLeft, Near);
            Assert.Less(man.FrozenLeft, PlainFreeze * 0.4f, "shorter than an elite's");

            var held = Man();
            Assert.IsTrue(held.Hold(10f));
            Assert.AreEqual(1f, held.HeldLeft, Near, "a long hold is capped");
        }

        [Test]
        public void TheGodlyFormTakesAHitchNotAStunAndStillRanksAsAGod()
        {
            var form = Godly();
            Assert.AreEqual(GodPhase.Godly, form.GodPhase);
            Assert.AreEqual(EnemyRank.God, form.Rank);
            form.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsTrue(form.IsFrozen, "a godly form can be frozen now");
            Assert.AreEqual(System.Math.Min(PlainFreeze * 0.12f, 0.4f), form.FrozenLeft, Near);
            Assert.LessOrEqual(form.FrozenLeft, 0.4f);
            Assert.AreEqual(EnemyRank.God, form.Rank, "frozen, it is still a god");

            var held = Godly();
            Assert.IsTrue(held.Hold(2f));
            Assert.AreEqual(0.24f, held.HeldLeft, Near);
            // Still too great to be lifted or rooted: those are not holds of this kind.
            Assert.IsFalse(held.Lift(1f));
            Assert.IsFalse(ConditionCaster.Takes(new ConditionSpec { Id = "test.root", Seconds = 2f, Flags = ConditionFlags.Rooted }, held));
        }

        [Test]
        public void AGodHeldOrFrozenCannotBeStoppedAgainUntilItsGuardIsOver()
        {
            var man = Man();
            Assert.IsTrue(man.Hold(2f));
            Assert.IsFalse(man.Hold(2f), "a hold is not made longer while it lasts");
            Assert.AreEqual(0.6f, man.HeldLeft, Near);
            man.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsFalse(man.IsFrozen, "nor turned into a freeze");
            Wait(man, 0.65f);
            Assert.IsFalse(man.IsHeld);
            Assert.Greater(man.GuardLeft, GodHolds.ManGuard - 0.1f, "the guard begins as the hold ends");
            Assert.IsTrue(man.Runs, "its owner keeps ticking it while the guard runs");
            Assert.IsFalse(man.Hold(2f), "guarded");
            Wait(man, GodHolds.ManGuard);
            Assert.AreEqual(0f, man.GuardLeft);
            Assert.IsTrue(man.Hold(2f), "the guard is over");

            var form = Godly();
            form.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsTrue(form.IsFrozen);
            Wait(form, 0.45f);
            Assert.IsFalse(form.IsFrozen, "thawed");
            Assert.Greater(form.GuardLeft, GodHolds.GodlyGuard - 0.5f);
            Assert.IsFalse(form.Hold(2f), "guarded after a freeze as after a hold");
        }

        [Test]
        public void ChillStillBuildsUnderTheGuardAndFreezesOnceTheGuardIsOver()
        {
            var form = Godly();
            Assert.IsTrue(form.Hold(1f));
            Wait(form, 0.2f);
            Assert.Greater(form.GuardLeft, 0f);
            int tries = 0;
            while (form.GuardLeft > 0.3f)
            {
                form.Apply(AfflictionKind.Chill, FullChill);
                Assert.AreEqual(FullChill, form.Stacks(AfflictionKind.Chill), "the Chill lies full");
                Assert.IsFalse(form.IsFrozen, "but does not freeze while guarded");
                Wait(form, 0.25f);
                tries++;
            }
            Assert.Greater(tries, 10, "the guard ran its whole time");
            Wait(form, 0.5f);
            Assert.AreEqual(0f, form.GuardLeft);
            Assert.AreEqual(FullChill, form.Stacks(AfflictionKind.Chill), "the last cold has not yet faded");
            form.Apply(AfflictionKind.Chill, 1);
            Assert.IsTrue(form.IsFrozen, "the next cold freezes the Chill that lay full");
        }

        [Test]
        public void BreakingTheIceStartsTheGuardAndClearingEndsIt()
        {
            var man = Man();
            man.Apply(AfflictionKind.Chill, FullChill);
            Assert.Greater(man.Shatter(), 0f);
            Assert.AreEqual(GodHolds.ManGuard, man.GuardLeft, Near);
            man.Clear();
            Assert.AreEqual(0f, man.GuardLeft);
            Assert.IsTrue(man.Hold(1f));
        }

        [Test]
        public void NothingTakesWhileTheBodyRefusesHolds()
        {
            var form = Godly();
            form.HoldsRefused = true;
            Assert.IsFalse(form.Hold(2f));
            form.Apply(AfflictionKind.Chill, FullChill);
            Assert.IsFalse(form.IsFrozen);
            Assert.AreEqual(FullChill, form.Stacks(AfflictionKind.Chill));
            form.HoldsRefused = false;
            form.Apply(AfflictionKind.Chill, 1);
            Assert.IsTrue(form.IsFrozen);
        }

        [Test]
        public void ANumberTriedLiveGoesBackAsBuilt()
        {
            GodHolds.GodlyCap = 2f;
            Assert.AreEqual(2f, GodHolds.Seconds(100f, GodPhase.Godly), Near);
            GodHolds.Restore();
            Assert.AreEqual(GodHolds.GodlyCapAsBuilt, GodHolds.Cap(GodPhase.Godly));
        }
    }
}
