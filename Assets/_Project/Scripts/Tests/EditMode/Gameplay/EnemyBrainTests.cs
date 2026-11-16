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

        // Runs the brain with the target standing off the ground the enemy keeps.
        private static void RunOffGround(EnemyBrain brain, float seconds, float distance)
        {
            for (float t = 0f; t < seconds; t += Frame) brain.Tick(Frame, distance, 0f, true, false);
        }

        [Test]
        public void ItDoesNotNoticeATargetStandingOffItsGround()
        {
            var brain = Brain();
            RunOffGround(brain, 2f, 2f);
            Assert.AreEqual(EnemyState.Idle, brain.State, "close enough to see, but not on its ground");
            Run(brain, 0.1f, 2f);
            Assert.AreNotEqual(EnemyState.Idle, brain.State, "the same distance, on its ground");
        }

        [Test]
        public void ItLetsATargetGoThatLeavesItsGroundButNotAtOnce()
        {
            var brain = Brain();
            Run(brain, 0.1f, 15f);
            brain.Alert();
            RunOffGround(brain, brain.GiveUpSeconds - 0.5f, 15f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "it watches from the edge first");
            RunOffGround(brain, 1f, 15f);
            Assert.AreEqual(EnemyState.Idle, brain.State);

            // Stepping back onto the ground before it has given up starts the count again.
            brain = Brain();
            brain.Alert();
            RunOffGround(brain, brain.GiveUpSeconds - 0.5f, 15f);
            Run(brain, 0.1f, 15f);
            RunOffGround(brain, brain.GiveUpSeconds - 0.5f, 15f);
            Assert.AreEqual(EnemyState.Stalk, brain.State);
        }

        [Test]
        public void ABlowFromOffItsGroundBringsItOutForAWhile()
        {
            var brain = Brain();
            brain.Provoke();
            Assert.IsTrue(brain.IsProvoked);
            RunOffGround(brain, brain.GiveUpSeconds + 1f, 15f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "the ground does not hold it while it is provoked");
            RunOffGround(brain, brain.ProvokedSeconds + brain.GiveUpSeconds, 15f);
            Assert.IsFalse(brain.IsProvoked);
            Assert.AreEqual(EnemyState.Idle, brain.State, "and then it does again");
        }

        [Test]
        public void ItsAnswerToBlowsTurnedIsTheAttackItKeepsForThat()
        {
            var brain = Brain();
            brain.CounterAttack = "far";
            brain.Attacks[1].MinRange = 0f;
            brain.RestMin = brain.RestMax = 5f;
            Run(brain, 0.1f, 2f);
            Run(brain, 1.2f, 2f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "resting after its first attack");
            Run(brain, 6f, 12f);
            brain.Counter();
            Run(brain, 0.05f, 2f);
            Assert.AreEqual("far", brain.Current.Name, "the counter, though the roll alone would have picked the other");

            // Out of the counter's reach it answers with whatever suits the distance.
            brain = Brain();
            brain.CounterAttack = "near";
            brain.Alert();
            brain.Counter();
            Run(brain, 0.05f, 7f);
            Assert.AreEqual("far", brain.Current.Name);
        }

        [Test]
        public void TheCaptainIsAnEliteWithTheAttacksNamedForHim()
        {
            var captain = EnemyKind.Captain();
            Assert.AreEqual(EnemyKind.CaptainName, EnemyKind.ByName(EnemyKind.CaptainName).Name);
            Assert.IsTrue(captain.IsElite);
            Assert.IsFalse(captain.IsBoss, "the bar across the bottom of the screen is the god's");
            Assert.AreEqual(3, captain.Embers);
            Assert.AreEqual(1, EnemyKind.Soldier().Embers);
            Assert.AreEqual(5, EnemyKind.Narukami().Embers);
            Assert.IsFalse(captain.DrawsStorm);
            foreach (string name in new[] { "captain_thrust_1", "captain_thrust_2", "captain_sweep", "captain_jab", "captain_slam", "captain_charge" })
            {
                var attack = captain.Brain.Attacks.Find(a => a.Name == name);
                Assert.IsNotNull(attack, name);
                Assert.AreEqual(0, attack.MinPhase, $"{name} is his from the start");
            }
            foreach (var attack in captain.Brain.Attacks)
            {
                Assert.IsTrue(attack.Name.StartsWith(captain.Prefix + "_"), attack.Name);
                if (!string.IsNullOrEmpty(attack.ChainInto))
                    Assert.IsNotNull(captain.Brain.Attacks.Find(a => a.Name == attack.ChainInto), $"{attack.Name} chains into an attack that does not exist");
            }
            Assert.IsNotNull(captain.Brain.Attacks.Find(a => a.Name == captain.Brain.CounterAttack), "his counter exists");
        }

        [Test]
        public void OnlyTheCaptainsChargesAreRed()
        {
            foreach (var attack in EnemyKind.Captain().Brain.Attacks)
                Assert.AreEqual(attack.Name.Contains("charge"), attack.Unblockable, attack.Name);
            var charge = EnemyKind.Captain().Brain.Attacks.Find(a => a.Name == "captain_charge");
            Assert.IsTrue(charge.IsLine && charge.PassThrough, "a lane he goes the length of");
            var slam = EnemyKind.Captain().Brain.Attacks.Find(a => a.Name == "captain_slam");
            Assert.IsTrue(slam.IsCircle && slam.Leap && !slam.Ranged, "a circle he lands on, struck by hand");
        }

        [Test]
        public void TheCaptainsTwoThrustsLeaveABeatLongerThanARoll()
        {
            var captain = EnemyKind.Captain();
            var first = captain.Brain.Attacks.Find(a => a.Name == "captain_thrust_1");
            var second = captain.Brain.Attacks.Find(a => a.Name == first.ChainInto);
            Assert.IsFalse(first.Unblockable || second.Unblockable, "both can be guarded");
            float between = first.Strike + second.Telegraph;
            Assert.Greater(between, new Clover.Player.DodgeRoll().Duration + 0.2f, "a roll begun at the first blow is over, and spent, before the second");
            Assert.Greater(second.AimLockAt, first.AimLockAt, "the second turns to follow for longer");
        }

        [Test]
        public void TheCaptainIsBrokenNotChippedAndChangesAtHalf()
        {
            var captain = EnemyKind.Captain();
            var soldier = EnemyKind.Soldier();
            Assert.IsTrue(captain.Guards);
            Assert.Greater(captain.BlocksBeforeCounter, soldier.BlocksBeforeCounter, "he turns more blows before he answers");
            Assert.Greater(captain.PoiseHits, soldier.PoiseHits);
            Assert.Less(captain.GuardedDamage, soldier.GuardedDamage);
            Assert.Less(captain.GuardedStagger, 1f);
            Assert.Less(captain.HeavyGuardedDamage, 1f, "his guard meets heavy blows too");
            Assert.IsTrue(captain.Steadfast);
            Assert.AreEqual(0.5f, captain.SecondPhaseAt, 0.0001f);
            Assert.Greater(captain.SecondPhasePace, 1f);
            Assert.IsNotNull(captain.Brain.Attacks.Find(a => a.Name == captain.SecondPhaseAttack));
            // Finishers alone: three of them fell him, two do not.
            Assert.Less(captain.FinisherHealthShare * 2f, 1f);
            Assert.GreaterOrEqual(captain.FinisherHealthShare * 3f, 1f);
            // Perfect guards alone break his balance in three.
            Assert.GreaterOrEqual(captain.PerfectGuardStagger * 3f, captain.StaggerMaximum);
            Assert.Less(captain.PerfectGuardStagger * 2f, captain.StaggerMaximum);

            int chains = 0;
            foreach (var attack in captain.Brain.Attacks)
                if (attack.MinPhase == 1 && !string.IsNullOrEmpty(attack.ChainInto)) chains++;
            Assert.GreaterOrEqual(chains, 2, "in his second phase his attacks run on into one another");
        }

        [Test]
        public void TheCaptainHasAnAnswerAtEveryRangeHeFightsAt()
        {
            var captain = EnemyKind.Captain();
            foreach (float distance in new[] { 0.8f, 1.5f, 2.5f, 3.5f, 5f, 7f, 8.4f })
            {
                bool any = captain.Brain.Attacks.Exists(a => a.MinPhase == 0 && distance >= a.MinRange && distance <= a.MaxRange);
                Assert.IsTrue(any, $"he has nothing to do at {distance} m");
            }
            var jab = captain.Brain.Attacks.Find(a => a.Name == "captain_jab");
            foreach (var attack in captain.Brain.Attacks)
                if (attack != jab && !attack.Unblockable) Assert.Less(jab.Telegraph, attack.Telegraph, "the jab is the quickest thing he does");
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
