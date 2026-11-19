using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// Enemy conditions (IMPLEMENTATION.md 3.7 and 6.1): six timed states on one enemy, each with a beat, an end and an
    /// answer to blows; the flags the brain and the body read; a condition set again is refreshed, not doubled; and
    /// what the afflictions' numbers make of them.
    /// </summary>
    public class EnemyConditionTests
    {
        private const float Frame = 1f / 60f;
        private readonly List<ConditionNews> heard = new List<ConditionNews>();

        private AfflictionSet NewEnemy()
        {
            var set = new AfflictionSet();
            // Its own listener, so nothing here reaches CombatEvents.
            set.Conditions.Report = (who, news) => heard.Add(news);
            return set;
        }

        [SetUp]
        public void Begin() => heard.Clear();

        private int Count(ConditionMoment moment)
        {
            int count = 0;
            foreach (var news in heard)
                if (news.Moment == moment) count++;
            return count;
        }

        [Test]
        public void ASetConditionFillsASlotSaysItBeganAndItsFlagsAreRead()
        {
            var enemy = NewEnemy();
            Assert.IsFalse(enemy.Conditions.Any);
            Assert.IsFalse(enemy.Runs, "nothing on it: its owner need not tick it");
            int slot = enemy.Conditions.Set(new ConditionSpec { Id = "test.root", Seconds = 3f, Flags = ConditionFlags.Rooted | ConditionFlags.CannotGuard }, null, 7);
            Assert.GreaterOrEqual(slot, 0);
            Assert.IsTrue(enemy.Conditions.Any);
            Assert.IsTrue(enemy.Runs, "a condition keeps the set running so its time runs out");
            Assert.IsTrue(enemy.Conditions.Has(ConditionFlags.Rooted));
            Assert.IsTrue(enemy.Conditions.Has(ConditionFlags.CannotGuard));
            Assert.IsFalse(enemy.Conditions.Has(ConditionFlags.BeginsNoAttack));
            Assert.IsTrue(enemy.Conditions.Holds("test.root"));
            Assert.AreEqual(7, enemy.Conditions.Run(slot).Mark, "it remembers the mark that set it");
            Assert.AreEqual(1, Count(ConditionMoment.Began));
            Assert.AreEqual("test.root", heard[0].Id);
        }

        [Test]
        public void ItBeatsOnItsBeatAndEndsOnceWhenItsTimeIsUp()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.beat", Seconds = 2.5f, Beat = 1f }, null, 3, null, false, true);
            for (float t = 0f; t < 2.2f; t += Frame) enemy.Tick(Frame);
            Assert.AreEqual(2, Count(ConditionMoment.Beat), "two beats in a little over two seconds");
            Assert.AreEqual(2, heard[heard.Count - 1].Count, "each beat carries how many have come");
            Assert.IsTrue(enemy.Conditions.Holds("test.beat"));
            for (float t = 0f; t < 1f; t += Frame) enemy.Tick(Frame);
            Assert.IsFalse(enemy.Conditions.Holds("test.beat"));
            Assert.IsFalse(enemy.Conditions.Any);
            Assert.AreEqual(1, Count(ConditionMoment.Ended));
            Assert.AreEqual(2, Count(ConditionMoment.Beat), "no beat on the step it ended");
            enemy.Tick(1f);
            Assert.AreEqual(1, Count(ConditionMoment.Ended), "it ends once");
        }

        [Test]
        public void SetAgainItIsRefreshedNotDoubledAndALinkToAnotherIsItsOwnSlot()
        {
            var enemy = NewEnemy();
            var spec = new ConditionSpec { Id = "test.again", Seconds = 4f, Beat = 1f };
            int first = enemy.Conditions.Set(spec, null, 1);
            enemy.Tick(1.5f);
            Assert.AreEqual(2.5f, enemy.Conditions.Left("test.again"), 0.001f);
            int again = enemy.Conditions.Set(spec, null, 1);
            Assert.AreEqual(first, again, "the same condition from the same source keeps its slot");
            Assert.AreEqual(1, enemy.Conditions.Count);
            Assert.AreEqual(4f, enemy.Conditions.Left("test.again"), 0.001f, "its time starts over");
            Assert.AreEqual(1, enemy.Conditions.Run(first).Count, "the beats it had are kept");
            Assert.AreEqual(1, Count(ConditionMoment.Began), "a refresh is not a beginning");

            var host = new GameObject("host").transform;
            try
            {
                enemy.Conditions.Set(spec, null, 1, host);
                Assert.AreEqual(2, enemy.Conditions.Count, "joined to someone it is another run");
            }
            finally { Object.DestroyImmediate(host.gameObject); }
        }

        [Test]
        public void SixSlotsAndASeventhTakesThePlaceOfTheOneNearestItsEnd()
        {
            var enemy = NewEnemy();
            for (int i = 0; i < EnemyConditions.Slots; i++)
                enemy.Conditions.Set(new ConditionSpec { Id = "test." + i, Seconds = 10f - i }, null);
            Assert.AreEqual(EnemyConditions.Slots, enemy.Conditions.Count);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.late", Seconds = 3f }, null);
            Assert.AreEqual(EnemyConditions.Slots, enemy.Conditions.Count);
            Assert.IsTrue(enemy.Conditions.Holds("test.late"));
            Assert.IsFalse(enemy.Conditions.Holds("test.5"), "the one with the least time left went");
            Assert.IsTrue(enemy.Conditions.Holds("test.4"));
        }

        [Test]
        public void TheNumbersItCarriesChangeWhatTheAfflictionsMakeOfIt()
        {
            var enemy = NewEnemy();
            float taken = enemy.DamageTakenMultiplier(false);
            float dealt = enemy.DamageDealtMultiplier;
            float speed = enemy.SpeedMultiplier;
            enemy.Conditions.Set(new ConditionSpec { Id = "test.slow", Seconds = 4f, Pace = 0.5f }, null);
            Assert.AreEqual(speed * 0.5f, enemy.SpeedMultiplier, 0.001f);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.taken", Seconds = 4f, Flags = ConditionFlags.DamageTakenShare, Scale = 1.3f }, null);
            Assert.AreEqual(taken * 1.3f, enemy.DamageTakenMultiplier(false), 0.001f);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.dealt", Seconds = 4f, Flags = ConditionFlags.DamageDealtShare, Scale = 0.6f }, null);
            Assert.AreEqual(dealt * 0.6f, enemy.DamageDealtMultiplier, 0.001f);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.windup", Seconds = 4f, Flags = ConditionFlags.WindupSlowed, Scale = 0.5f }, null);
            Assert.AreEqual(0.5f, enemy.Conditions.WindupPace, 0.001f);
            Assert.AreEqual(1.3f, enemy.Conditions.TakenScale, 0.001f, "each run's Scale is read only by the flags it names");
            // Half a slow for what resists a hold (an elite).
            var elite = NewEnemy();
            elite.Conditions.Set(new ConditionSpec { Id = "test.slow", Seconds = 4f, Pace = 0.5f }, null, -1, null, false, false, 0.5f);
            Assert.AreEqual(0.75f, elite.Conditions.Pace, 0.001f);
        }

        [Test]
        public void ABlowIsToldOnlyToWhatAnswersItAndNeverByAReactionsOwnBlow()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.quiet", Seconds = 5f }, null);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.answers", Seconds = 5f }, null, 2, null, true);
            enemy.TakeFrom(new Hit(12f, 10f, Vector3.forward, heavy: true), null);
            Assert.AreEqual(1, Count(ConditionMoment.Struck));
            var struck = heard.Find(news => news.Moment == ConditionMoment.Struck);
            Assert.AreEqual("test.answers", struck.Id);
            Assert.AreEqual(12f, struck.Damage, 0.001f);
            Assert.IsTrue(struck.Heavy);
            Assert.IsFalse(struck.Spell);
            enemy.TakeFrom(new Hit(12f, 10f, Vector3.forward, fromReaction: true), null);
            Assert.AreEqual(1, Count(ConditionMoment.Struck), "a reaction's own blow does not answer a condition, so nothing feeds on itself");
            enemy.TakeFrom(new Hit(0f, 20f, Vector3.down), null);
            Assert.AreEqual(1, Count(ConditionMoment.Struck), "a blow that deals nothing (a landing) is not a blow struck");
        }

        [Test]
        public void ALinkIsFoundFromTheHostAndClearingSaysNothing()
        {
            var host = new GameObject("host").transform;
            var other = new GameObject("other").transform;
            try
            {
                var joined = NewEnemy();
                joined.Conditions.Set(new ConditionSpec { Id = "test.link", Seconds = 6f, Flags = ConditionFlags.Linked, Scale = 0.5f }, null, -1, host);
                Assert.IsTrue(joined.Conditions.LinkedTo(host, out float share));
                Assert.AreEqual(0.5f, share, 0.001f);
                Assert.IsFalse(joined.Conditions.LinkedTo(other, out _));
                Assert.AreSame(host, heard[0].Link);
                heard.Clear();
                joined.Clear();
                Assert.IsFalse(joined.Conditions.Any);
                Assert.AreEqual(0, heard.Count, "cleared (the enemy died or was reset) nothing ends by its time");
            }
            finally
            {
                Object.DestroyImmediate(host.gameObject);
                Object.DestroyImmediate(other.gameObject);
            }
        }

        // ---- What the brain makes of them: one read, at the top of its step.

        private static EnemyBrain Brain(EnemyConditions conditions)
        {
            var brain = new EnemyBrain
            {
                AggroRange = 10f, LeashRange = 20f, PreferredRange = 2f, RestMin = 0.5f, RestMax = 0.5f,
                Attacks = new List<EnemyAttack> { new EnemyAttack { Name = "near", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.4f, MinRange = 0f, MaxRange = 3f } },
                Conditions = conditions,
            };
            brain.Reset();
            return brain;
        }

        private static int Run(EnemyBrain brain, float seconds, float distance)
        {
            int blows = 0;
            for (float t = 0f; t < seconds; t += Frame)
                if (brain.Tick(Frame, distance, 0f)) blows++;
            return blows;
        }

        [Test]
        public void RootedItAttacksButDoesNotMove()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.rooted", Seconds = 10f, Flags = ConditionFlags.Rooted }, null);
            var brain = Brain(enemy.Conditions);
            Run(brain, 0.2f, 6f);
            Assert.AreEqual(EnemyState.Stalk, brain.State);
            Assert.AreEqual(0, brain.MoveWish, "rooted, it does not walk in");
            Assert.AreEqual(1, Run(brain, 1.2f, 2f), "rooted, it still strikes what comes into reach");
        }

        [Test]
        public void ItBeginsNoAttackButStillWalks()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.breath", Seconds = 10f, Flags = ConditionFlags.BeginsNoAttack }, null);
            var brain = Brain(enemy.Conditions);
            Assert.AreEqual(0, Run(brain, 2f, 2.5f), "no attack is begun");
            Assert.AreEqual(1, brain.MoveWish, "it still closes in");
        }

        [Test]
        public void ItsWindupIsSlowedAndItsSightCutShort()
        {
            var slowed = NewEnemy();
            slowed.Conditions.Set(new ConditionSpec { Id = "test.windup", Seconds = 10f, Flags = ConditionFlags.WindupSlowed, Scale = 0.5f }, null);
            var brain = Brain(slowed.Conditions);
            Run(brain, 0.05f, 2f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State);
            Run(brain, 0.7f, 2f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State, "a 0.5 s wind-up at half pace is still being shown after 0.7 s");
            Run(brain, 0.4f, 2f);
            Assert.AreNotEqual(EnemyState.Telegraph, brain.State);

            var blind = NewEnemy();
            blind.Conditions.Set(new ConditionSpec { Id = "test.sight", Seconds = 10f, Flags = ConditionFlags.SightShort, Sight = 2f }, null);
            brain = Brain(blind.Conditions);
            Run(brain, 0.5f, 6f);
            Assert.AreEqual(EnemyState.Idle, brain.State, "it cannot see someone 6 m off");
            Run(brain, 0.1f, 1.5f);
            Assert.AreNotEqual(EnemyState.Idle, brain.State, "inside its sight it finds them (and, in reach, goes for them)");
            // The blow it had begun when the target stepped away is finished; then it stands, and begins nothing more.
            Run(brain, 1.5f, 5f);
            Assert.AreEqual(0, brain.MoveWish, "lost from sight it stops");
            Assert.IsNull(brain.Current, "and begins no attack at what it cannot see");
        }

        // ---- Knocked flat (task 3.7): the Down flag and the brain's Down state.

        private static ConditionSpec Fall(float seconds) => new ConditionSpec { Id = "test.down", Seconds = seconds, Flags = ConditionFlags.Down };

        // The body and its brain stepped together, as the enemy does; the blows its brain strikes are counted.
        private static int Run(AfflictionSet body, EnemyBrain brain, float seconds, float distance)
        {
            int blows = 0;
            for (float t = 0f; t < seconds; t += Frame)
            {
                body.Tick(Frame);
                if (brain.Tick(Frame, distance, 0f)) blows++;
            }
            return blows;
        }

        [Test]
        public void DownHoldsForItsTimeAndEnds()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(Fall(1.2f), null, 4, null, false, true);
            Assert.IsTrue(enemy.Conditions.Has(ConditionFlags.Down));
            Assert.AreEqual(1.2f, enemy.Conditions.Left("test.down"), 0.001f);
            for (float t = 0f; t < 1.1f; t += Frame) enemy.Tick(Frame);
            Assert.IsTrue(enemy.Conditions.Has(ConditionFlags.Down), "still down just before its time is up");
            for (float t = 0f; t < 0.2f; t += Frame) enemy.Tick(Frame);
            Assert.IsFalse(enemy.Conditions.Has(ConditionFlags.Down));
            Assert.AreEqual(1, Count(ConditionMoment.Ended), "its end is told like any condition's");
        }

        [Test]
        public void AnEliteLiesHalfTheTimeAndAGodNeverGoesDown()
        {
            var elite = new AfflictionSet { FreezeScale = 0.4f };
            elite.Conditions.Report = (who, news) => heard.Add(news);
            Assert.AreEqual(0.5f, ConditionCaster.ShareOf(elite), 0.001f, "what resists ice takes half");
            Assert.AreEqual(1f, ConditionCaster.ShareOf(new AfflictionSet()), 0.001f);
            elite.Conditions.Set(Fall(1.8f), null, -1, null, false, false, ConditionCaster.ShareOf(elite));
            Assert.AreEqual(0.9f, elite.Conditions.Left("test.down"), 0.001f, "as a root, an elite lies for half the time");

            var god = new AfflictionSet { CanFreeze = false };
            Assert.IsFalse(ConditionCaster.Takes(Fall(1.2f), god), "what cannot be frozen is never knocked flat");
            Assert.IsTrue(ConditionCaster.Takes(Fall(1.2f), elite));
            Assert.IsTrue(ConditionCaster.Takes(Fall(1.2f), new AfflictionSet()));
            Assert.IsTrue(ConditionCaster.Takes(new ConditionSpec { Id = "test.open", Seconds = 2f, Flags = ConditionFlags.CannotGuard }, god),
                "a god still takes what does not hold it");
        }

        [Test]
        public void KnockedFlatItLosesItsBlowLiesBeginningNothingAndGetsUp()
        {
            var enemy = NewEnemy();
            var brain = Brain(enemy.Conditions);
            Run(enemy, brain, 0.05f, 2f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State, "in reach it begins its attack");
            enemy.Conditions.Set(Fall(1.2f), null);
            Assert.AreEqual(0, Run(enemy, brain, Frame, 2f));
            Assert.AreEqual(EnemyState.Down, brain.State, "it goes down out of the wind-up");
            Assert.IsTrue(brain.IsDown);
            Assert.IsNull(brain.Current, "the blow it had begun is lost");
            Assert.IsFalse(brain.IsGettingUp);

            Assert.AreEqual(0, Run(enemy, brain, 1f, 2f), "lying, it strikes nothing");
            Assert.AreEqual(EnemyState.Down, brain.State);
            Assert.AreEqual(0, brain.MoveWish, "it does not crawl");
            Assert.IsFalse(brain.Tracks, "nor turn");
            Assert.IsFalse(brain.Flinch(true), "a blow does not make it flinch");
            Assert.IsFalse(brain.Force("near"), "nothing from outside stands it up to attack");
            Assert.IsFalse(brain.Occupy(1f), "nor takes it up with anything else");
            Assert.AreEqual(EnemyState.Down, brain.State);

            // The condition runs out: it gets up, and only then is it back in the fight.
            Run(enemy, brain, 0.25f, 2f);
            Assert.IsFalse(enemy.Conditions.Has(ConditionFlags.Down));
            Assert.IsTrue(brain.IsGettingUp, "its time up, it is getting to its feet");
            Assert.Greater(brain.GetUpProgress, 0f);
            Run(enemy, brain, EnemyBrain.GetUpSeconds, 2f);
            Assert.IsFalse(brain.IsDown, "on its feet again");
            Assert.AreEqual(1, Run(enemy, brain, 1.5f, 2f), "and it attacks again");
        }

        [Test]
        public void ItLiesNoLessThanItsFallAndIsKnockedFlatAgainWhileGettingUp()
        {
            var enemy = NewEnemy();
            var brain = Brain(enemy.Conditions);
            enemy.Conditions.Set(Fall(0.1f), null);
            Run(enemy, brain, 0.3f, 6f);
            Assert.IsFalse(enemy.Conditions.Any);
            Assert.IsTrue(brain.IsDown && !brain.IsGettingUp, "a fall is seen through before it gets up, however short the condition");
            Run(enemy, brain, EnemyBrain.FallSeconds - 0.3f + 0.1f, 6f);
            Assert.IsTrue(brain.IsGettingUp);
            enemy.Conditions.Set(Fall(1f), null, 2);
            Run(enemy, brain, Frame, 6f);
            Assert.IsTrue(brain.IsDown && !brain.IsGettingUp, "knocked flat again as it rose, it goes down again");
            Assert.AreEqual(Frame, brain.StateSeconds, 0.001f, "from the start of a new fall");
        }

        [Test]
        public void ABrokenBalanceRunsItsCourseAndTheFallComesAfter()
        {
            var enemy = NewEnemy();
            var brain = Brain(enemy.Conditions);
            Run(enemy, brain, 0.05f, 6f);
            brain.Stagger();
            enemy.Conditions.Set(Fall(2f), null);
            Run(enemy, brain, 0.3f, 6f);
            Assert.AreEqual(EnemyState.Staggered, brain.State, "staggered, it is already helpless: the stagger is not cut short");
            brain.EndStagger();
            Run(enemy, brain, Frame, 6f);
            Assert.AreEqual(EnemyState.Down, brain.State, "the stagger over and the condition still running, it goes down");
            brain.Stagger();
            Assert.AreEqual(EnemyState.Staggered, brain.State, "a lying enemy's balance can still be broken, and opened for the finisher");
            brain.Die();
            enemy.Conditions.Set(Fall(2f), null, 3);
            Run(enemy, brain, Frame, 6f);
            Assert.AreEqual(EnemyState.Dead, brain.State, "the dead do not fall again");
        }

        [Test]
        public void CapsizeAndLoganStoneKnockFlatInsteadOfHolding()
        {
            int capsize = AfflictionPairs.Index(AfflictionKind.Soaked, AfflictionKind.Unsteady);
            int logan = AfflictionPairs.Index(AfflictionKind.Unsteady, AfflictionKind.Tremor);
            foreach (int pair in new[] { capsize, logan })
            {
                var mark = ReactionTable.MarkFor(pair);
                Assert.IsNotNull(mark, AfflictionPairs.Name(pair));
                bool down = false, held = false;
                foreach (var effect in mark.Effects)
                {
                    if (effect.Do == UpgradeAction.Hold) held = true;
                    if (effect.Do != UpgradeAction.Condition || effect.State == null || (effect.State.Flags & ConditionFlags.Down) == 0) continue;
                    down = true;
                    Assert.AreEqual(EnemyRank.Plain, effect.Ranks, AfflictionPairs.Name(pair) + ": only the plain go down; the great stagger");
                    Assert.AreEqual(pair == capsize ? 1.2f : 1.8f, effect.State.Seconds, 0.001f, AfflictionPairs.Name(pair));
                }
                Assert.IsTrue(down, AfflictionPairs.Name(pair) + " knocks flat");
                Assert.IsFalse(held, AfflictionPairs.Name(pair) + " no longer holds in its place");
            }
        }
    }
}
