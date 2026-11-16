using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;

namespace Clover.Tests
{
    public class EnemyBrainTests
    {
        private const float Frame = 1f / 60f;

        private static EnemyBrain Brain()
        {
            var brain = new EnemyBrain
            {
                AggroRange = 10f,
                LeashRange = 20f,
                PreferredRange = 2f,
                RestMin = 0.5f,
                RestMax = 0.5f,
                FlinchSeconds = 0.3f,
                Attacks = new List<EnemyAttack>
                {
                    new EnemyAttack { Name = "near", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.4f, MinRange = 0f, MaxRange = 3f },
                    new EnemyAttack { Name = "far", Telegraph = 0.8f, Strike = 0.1f, Recovery = 0.4f, MinRange = 5f, MaxRange = 9f, Cooldown = 5f },
                },
            };
            brain.Reset();
            return brain;
        }

        // Runs the brain at a fixed distance and returns how many blows landed.
        private static int Run(EnemyBrain brain, float seconds, float distance, float roll = 0f)
        {
            int blows = 0;
            for (float t = 0f; t < seconds; t += Frame)
                if (brain.Tick(Frame, distance, roll)) blows++;
            return blows;
        }

        [Test]
        public void ItIgnoresATargetOutOfSightAndClosesOnOneInSight()
        {
            var brain = Brain();
            Run(brain, 1f, 15f);
            Assert.AreEqual(EnemyState.Idle, brain.State);
            Run(brain, 0.1f, 4f);
            Assert.AreEqual(EnemyState.Stalk, brain.State);
            Assert.AreEqual(1, brain.MoveWish, "too far for either attack: it walks in");
        }

        [Test]
        public void ItPicksTheAttackThatSuitsTheDistanceAndStrikesOnce()
        {
            var brain = Brain();
            Run(brain, 0.1f, 2f);
            Assert.AreEqual("near", brain.Current.Name);
            Assert.AreEqual(1, Run(brain, 1.2f, 2f));

            brain = Brain();
            Run(brain, 0.1f, 7f);
            Assert.AreEqual("far", brain.Current.Name);
        }

        [Test]
        public void ItRestsBetweenAttacksAndHonoursACooldown()
        {
            var brain = Brain();
            Run(brain, 0.1f, 7f);
            Assert.AreEqual("far", brain.Current.Name);
            Run(brain, 1.4f, 7f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "resting, and the far attack is cooling down");
            Run(brain, 2f, 7f);
            Assert.IsNull(brain.Current, "nothing else reaches that far, so it only closes in");
            Assert.AreEqual(1, brain.MoveWish);
        }

        [Test]
        public void ALightBlowDoesNotStopAnAttackBeingShownButAHeavyOneDoes()
        {
            var brain = Brain();
            Run(brain, 0.2f, 2f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State);
            Assert.IsFalse(brain.Flinch(false));
            Assert.AreEqual(EnemyState.Telegraph, brain.State);
            Assert.IsTrue(brain.Flinch(true));
            Assert.AreEqual(EnemyState.Flinch, brain.State);
            Assert.AreEqual(0, Run(brain, 0.25f, 2f), "the blow it was showing never lands");
        }

        [Test]
        public void ItStopsTurningPartWayThroughTheTelegraph()
        {
            var brain = Brain();
            brain.Attacks[0].AimLockAt = 0.5f;
            Run(brain, 0.1f, 2f);
            Assert.IsTrue(brain.Tracks);
            Run(brain, 0.3f, 2f);
            Assert.IsFalse(brain.Tracks, "a late sidestep gets clear");
        }

        [Test]
        public void AStaggerDropsEverythingUntilItIsEnded()
        {
            var brain = Brain();
            Run(brain, 0.2f, 2f);
            brain.Stagger();
            Assert.AreEqual(EnemyState.Staggered, brain.State);
            Assert.AreEqual(0, Run(brain, 3f, 2f));
            Assert.IsFalse(brain.Flinch(true), "a staggered enemy does not flinch out of it");
            brain.EndStagger();
            Assert.AreEqual(EnemyState.Stalk, brain.State);
        }

        [Test]
        public void AChainedAttackFollowsWithNoRest()
        {
            var brain = Brain();
            brain.Attacks[0].ChainInto = "second";
            brain.Attacks.Add(new EnemyAttack { Name = "second", Telegraph = 0.2f, Strike = 0.1f, Recovery = 0.3f, MinRange = 1f, MaxRange = 0f });
            brain.Reset();
            Assert.AreEqual(1, Run(brain, 0.7f, 2f));
            Assert.AreEqual("second", brain.Current.Name, "it began the moment the first blow was through");
            Assert.AreEqual(1, Run(brain, 0.3f, 2f));
        }

        [Test]
        public void WhenItIsNotItsTurnItHoldsARingAndDoesNotAttack()
        {
            var brain = Brain();
            brain.WaitRange = 5f;
            int blows = 0;
            for (float t = 0f; t < 3f; t += Frame)
                if (brain.Tick(Frame, 2f, 0f, false)) blows++;
            Assert.AreEqual(0, blows);
            Assert.IsTrue(brain.IsWaiting);
            Assert.AreEqual(-1, brain.MoveWish, "too close for one that is waiting: it backs off to the ring");
            brain.Tick(Frame, 2f, 0f, true);
            Assert.AreEqual(EnemyState.Telegraph, brain.State, "its turn: it attacks at once");
        }

        [Test]
        public void ACounterComesAtOnceEvenOutOfTurn()
        {
            var brain = Brain();
            brain.RestMin = brain.RestMax = 10f;
            Run(brain, 1.3f, 2f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "resting after its first attack");
            brain.Counter();
            brain.Tick(Frame, 2f, 0f, false);
            Assert.AreEqual(EnemyState.Telegraph, brain.State);
        }

        [Test]
        public void AnAttackKeptForALaterPhaseWaitsForIt()
        {
            var brain = Brain();
            brain.Attacks[0].MinPhase = 1;
            brain.Reset();
            Assert.AreEqual(0, Run(brain, 2f, 2f), "nothing it may use reaches yet");
            brain.Phase = 1;
            Assert.AreEqual(1, Run(brain, 1f, 2f));
        }

        [Test]
        public void AForcedAttackBeginsAtOnceWhateverItWasDoing()
        {
            var brain = Brain();
            Run(brain, 0.2f, 2f);
            Assert.AreEqual("near", brain.Current.Name);
            Assert.IsTrue(brain.Force("far"));
            Assert.AreEqual("far", brain.Current.Name);
            Assert.AreEqual(EnemyState.Telegraph, brain.State);
            brain.Stagger();
            Assert.IsFalse(brain.Force("near"), "not while it is broken open");
        }

        [Test]
        public void TheGodHasAnAnswerAtEveryRangeAndASecondPhase()
        {
            var god = EnemyKind.Narukami();
            Assert.IsTrue(god.IsBoss);
            Assert.Greater(god.SecondPhaseAt, 0f);
            Assert.IsNotNull(god.Brain.Attacks.Find(a => a.Name == god.SecondPhaseAttack), "the attack that opens his second phase exists");
            foreach (float distance in new[] { 1f, 3f, 5f, 8f, 12f })
            {
                bool any = god.Brain.Attacks.Exists(a => a.MinPhase == 0 && distance >= a.MinRange && distance <= a.MaxRange);
                Assert.IsTrue(any, $"he has nothing to do at {distance} m");
            }
            foreach (var attack in god.Brain.Attacks)
                if (!string.IsNullOrEmpty(attack.ChainInto))
                    Assert.IsNotNull(god.Brain.Attacks.Find(a => a.Name == attack.ChainInto), $"{attack.Name} chains into an attack that does not exist");
        }

        [Test]
        public void EveryEnemyKindHasAttacksItCanReachWith()
        {
            foreach (var kind in EnemyKind.All)
            {
                Assert.Greater(kind.Health, 0f, kind.Name);
                Assert.IsNotEmpty(kind.Brain.Attacks, kind.Name);
                bool reachable = false;
                foreach (var attack in kind.Brain.Attacks)
                    if (attack.MaxRange >= attack.MinRange && attack.MaxRange >= kind.Brain.PreferredRange * 0.5f) reachable = true;
                Assert.IsTrue(reachable, $"{kind.Name} has no attack it can use from where it likes to stand");
            }
        }
    }
}
