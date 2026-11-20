using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The pieces of enemy conditions the 49 reactions of task 3.4 share (docs/design/steel/content_open.tsv): an
    /// affliction kept from fading and from blows, a condition that lasts only while afflictions are carried, a beat on
    /// the poison's bite, a condition spent by one blow or ended after so many, the rank read from holding, a root halved
    /// on an elite, and the new shares and the critical the afflictions read. Plain rules, on sets with their own engine
    /// and listener, so nothing here reaches CombatEvents.
    /// </summary>
    public class ConditionPiecesTests
    {
        private const float Frame = 0.1f;
        private readonly List<ConditionNews> heard = new List<ConditionNews>();
        private ReactionEngine engine;

        [SetUp]
        public void Begin()
        {
            heard.Clear();
            engine = new ReactionEngine((who, pair, laid) => { }, () => 0);
        }

        private AfflictionSet NewEnemy()
        {
            var set = new AfflictionSet { Reactions = engine };
            set.Conditions.Report = (who, news) => heard.Add(news);
            return set;
        }

        private static void Run(AfflictionSet set, float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / Frame);
            for (int i = 0; i < steps; i++) set.Tick(Frame);
        }

        private static void Lay(AfflictionSet set, AfflictionKind kind, int stacks = 1) => set.Lay(kind, stacks, 0f, 1f, LaySource.Spell);

        private int Count(ConditionMoment moment)
        {
            int count = 0;
            foreach (var news in heard)
                if (news.Moment == moment) count++;
            return count;
        }

        private ConditionNews Last(ConditionMoment moment)
        {
            for (int i = heard.Count - 1; i >= 0; i--)
                if (heard[i].Moment == moment) return heard[i];
            return default;
        }

        [Test]
        public void AKeptAfflictionNeitherFadesNorIsSpentByBlowsAndFadesOnceTheKeepingEnds()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Sundered);
            Lay(enemy, AfflictionKind.Marked);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.notches", Seconds = 10f, Keeps = AfflictionKind.Sundered }, null, 1);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.mark", Seconds = 2f, Keeps = AfflictionKind.Marked }, null, 1);
            Assert.IsTrue(enemy.Conditions.KeepsKind(AfflictionKind.Sundered) && enemy.Conditions.KeepsKind(AfflictionKind.Marked));
            for (int i = 0; i < 3; i++) enemy.BlowLanded();
            Assert.IsTrue(enemy.Has(AfflictionKind.Marked), "kept, the Mark is not spent by its three blows");

            Run(enemy, 9f);
            Assert.IsTrue(enemy.Has(AfflictionKind.Sundered), "kept, notches that last 8 s are there after 9");
            Assert.IsFalse(enemy.Conditions.KeepsKind(AfflictionKind.Marked), "the Mark's keeping ended at 2 s");
            Assert.IsTrue(enemy.Has(AfflictionKind.Marked), "it fades again from then, and 8 s have not passed since");
            for (int i = 0; i < 3; i++) enemy.BlowLanded();
            Assert.IsFalse(enemy.Has(AfflictionKind.Marked), "no longer kept, three blows spend it");

            Run(enemy, 9.5f);
            Assert.IsFalse(enemy.Conditions.Any);
            Assert.IsFalse(enemy.Has(AfflictionKind.Sundered), "the keeping over at 10 s, the notches fade in their 8");
        }

        [Test]
        public void AKeptBurnBurnsOnWithoutRunningDown()
        {
            var enemy = NewEnemy();
            enemy.Ignite(3f, 2, LaySource.Spell);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.sunk", Seconds = 6f, Keeps = AfflictionKind.Burn }, null, 1);
            float damage = 0f;
            for (int i = 0; i < 50; i++) damage += enemy.Tick(Frame);
            Assert.IsTrue(enemy.Burn.IsBurning, "a 3 s burn still burns after 5 s");
            Assert.AreEqual(3f, enemy.Burn.SecondsLeft, 0.05f, "its time does not run down");
            Assert.Greater(damage, 0f, "it bites on");
            Run(enemy, 4.5f);
            Assert.IsFalse(enemy.Burn.IsBurning, "the keeping over at 6 s, its 3 s run out");
        }

        [Test]
        public void AConditionThatLastsWhileAfflictionsAreCarriedEndsAndIsToldOnceOneIsGone()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Poison, 2);
            Lay(enemy, AfflictionKind.Static, 3);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.both", Seconds = 30f, While = AfflictionKind.Poison, While2 = AfflictionKind.Static }, null, 2, null, false, true);
            Run(enemy, 1f);
            Assert.IsTrue(enemy.Conditions.Holds("test.both"), "both carried, it runs");
            Assert.AreEqual(0, Count(ConditionMoment.Ended));
            enemy.Spend(AfflictionKind.Static, 3);
            enemy.Tick(Frame);
            Assert.IsFalse(enemy.Conditions.Holds("test.both"), "the Static gone, it ends");
            Assert.AreEqual(1, Count(ConditionMoment.Ended), "and its end is told");
            Assert.AreEqual("test.both", Last(ConditionMoment.Ended).Id);
            Run(enemy, 1f);
            Assert.AreEqual(1, Count(ConditionMoment.Ended), "once");
        }

        [Test]
        public void AConditionOnTheBiteBeatsWhenThePoisonBitesAndOnNoClock()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Poison, 2);
            // Its Beat is ignored: the bite is its beat.
            enemy.Conditions.Set(new ConditionSpec { Id = "test.bite", Seconds = 30f, Beat = 0.5f, OnBite = true }, null, 3);
            Run(enemy, 1.5f);
            Assert.AreEqual(0, Count(ConditionMoment.Beat), "no bite yet, no beat, whatever the clock");
            Run(enemy, 3f);
            Assert.AreEqual(2, Count(ConditionMoment.Beat), "poison bites every 2 s: two bites in 4.5 s, two beats");
            Assert.AreEqual(2, Last(ConditionMoment.Beat).Count, "each carries how many have come");
        }

        [Test]
        public void TheBlowThatSpendsAConditionIsAnsweredAndThenItEndsUntold()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.heavy", Seconds = 5f, EndsWith = ConditionBlow.Heavy }, null, 1, null, true, true);
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward), null);
            Assert.AreEqual(1, Count(ConditionMoment.Struck), "a light blow is answered");
            Assert.IsTrue(enemy.Conditions.Holds("test.heavy"), "and does not spend it");
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true, spell: true), null);
            Assert.IsTrue(enemy.Conditions.Holds("test.heavy"), "a spell is no heavy blow of the weapon");
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true), null);
            Assert.AreEqual(3, Count(ConditionMoment.Struck), "the heavy blow is answered");
            Assert.IsTrue(Last(ConditionMoment.Struck).Heavy);
            Assert.IsFalse(enemy.Conditions.Holds("test.heavy"), "then it is spent");
            Assert.AreEqual(0, Count(ConditionMoment.Ended), "spent, not run out: no end is told");
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true), null);
            Assert.AreEqual(3, Count(ConditionMoment.Struck), "the next heavy blow finds nothing");

            var other = NewEnemy();
            other.Conditions.Set(new ConditionSpec { Id = "test.weapon", Seconds = 5f, EndsWith = ConditionBlow.Weapon }, null);
            other.TakeFrom(new Hit(5f, 0f, Vector3.forward, spell: true), null);
            Assert.IsTrue(other.Conditions.Holds("test.weapon"), "a spell does not spend what a weapon blow spends");
            other.TakeFrom(new Hit(5f, 0f, Vector3.forward), null);
            Assert.IsFalse(other.Conditions.Holds("test.weapon"));
        }

        [Test]
        public void WeaponBlowsAreCountedAndTheLastItAllowsEndsIt()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.three", Seconds = 10f, Blows = 3 }, null, 1, null, true);
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, spell: true), null);
            Assert.AreEqual(0, Last(ConditionMoment.Struck).Blows, "a spell is answered but not counted");
            for (int i = 1; i <= 3; i++)
            {
                enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward), null);
                Assert.AreEqual(i, Last(ConditionMoment.Struck).Blows, "each weapon blow carries its number");
            }
            Assert.AreEqual(4, Count(ConditionMoment.Struck));
            Assert.IsFalse(enemy.Conditions.Holds("test.three"), "the third was its last");
            Assert.AreEqual(0, Count(ConditionMoment.Ended));
        }

        [Test]
        public void TheRankIsReadFromHoldingAndAnEliteIsRootedForHalfTheTime()
        {
            Assert.AreEqual(EnemyRank.Plain, new AfflictionSet().Rank);
            Assert.AreEqual(EnemyRank.Elite, new AfflictionSet { FreezeScale = 0.4f }.Rank);
            Assert.AreEqual(EnemyRank.God, new AfflictionSet { CanFreeze = false }.Rank);

            var elite = NewEnemy();
            elite.Conditions.Set(new ConditionSpec { Id = "test.root", Seconds = 2f, Flags = ConditionFlags.Rooted }, null, -1, null, false, false, 0.5f);
            Assert.AreEqual(1f, elite.Conditions.Left("test.root"), 0.001f, "half the root");
            elite.Conditions.Set(new ConditionSpec { Id = "test.free", Seconds = 2f }, null, -1, null, false, false, 0.5f);
            Assert.AreEqual(2f, elite.Conditions.Left("test.free"), 0.001f, "what does not hold keeps its time");
            var plain = NewEnemy();
            plain.Conditions.Set(new ConditionSpec { Id = "test.root", Seconds = 2f, Flags = ConditionFlags.Rooted }, null);
            Assert.AreEqual(2f, plain.Conditions.Left("test.root"), 0.001f);
        }

        [Test]
        public void TheNewSharesAndTheCriticalAreReadByTheAfflictions()
        {
            var enemy = NewEnemy();
            float weapon = enemy.DamageTakenMultiplier(false);
            float spell = enemy.DamageTakenMultiplier(true);
            float stagger = enemy.StaggerTakenMultiplier;
            Assert.IsFalse(enemy.MakesBlowsCritical);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.weapon", Seconds = 4f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.25f }, null);
            Assert.AreEqual(weapon * 1.25f, enemy.DamageTakenMultiplier(false), 0.001f);
            Assert.AreEqual(spell, enemy.DamageTakenMultiplier(true), 0.001f, "weapon blows only");
            enemy.Conditions.Set(new ConditionSpec { Id = "test.spell", Seconds = 4f, Flags = ConditionFlags.SpellTakenShare, Scale = 0.5f }, null);
            Assert.AreEqual(spell * 0.5f, enemy.DamageTakenMultiplier(true), 0.001f);
            Assert.AreEqual(weapon * 1.25f, enemy.DamageTakenMultiplier(false), 0.001f, "spells only");
            enemy.Conditions.Set(new ConditionSpec { Id = "test.stagger", Seconds = 4f, Flags = ConditionFlags.StaggerTakenShare, Scale = 2f }, null);
            Assert.AreEqual(stagger * 2f, enemy.StaggerTakenMultiplier, 0.001f);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.critical", Seconds = 4f, Flags = ConditionFlags.TakesCritical }, null);
            Assert.IsTrue(enemy.MakesBlowsCritical);
            enemy.Conditions.Clear();
            Assert.IsFalse(enemy.MakesBlowsCritical);
            Assert.AreEqual(weapon, enemy.DamageTakenMultiplier(false), 0.001f);
            Assert.AreEqual(stagger, enemy.StaggerTakenMultiplier, 0.001f);
        }

        [Test]
        public void TheBlowThatShattersIsToldSoItsAnswerNeedNotAskForTheChillItEnded()
        {
            // Cold Damage (Venom Rime): its doses go off on the blow that shatters, which has ended the Chill by then.
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.bone", Seconds = 10f }, null, 1, null, true);
            enemy.Apply(AfflictionKind.Chill, 5);
            Assert.IsTrue(enemy.IsFrozen);
            // As Enemy and TrainingDummy do: the ice breaks first, then the blow is told.
            bool shattered = enemy.Shatter() > 0f;
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true), null, shattered);
            Assert.IsTrue(Last(ConditionMoment.Struck).Shattered, "the blow that broke the ice says so");
            Assert.IsFalse(enemy.Has(AfflictionKind.Chill), "and the Chill is gone before anyone answers it");
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true), null);
            Assert.IsFalse(Last(ConditionMoment.Struck).Shattered, "a heavy blow on nothing frozen breaks nothing");
        }

        [Test]
        public void TheCarriersBlowAtThePlayerIsToldAndSpendsWhatItSpends()
        {
            // The their-blow moment: The Grudge's cloud answers the enemy's next blow at you, and that blow spends it.
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.cloud", Seconds = 8f, EndsWith = ConditionBlow.TheirBlow }, null, 1, null, false, false, 1f, false, true);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.quiet", Seconds = 8f }, null, 2);
            enemy.TakeFrom(new Hit(5f, 0f, Vector3.forward, heavy: true), null);
            Assert.IsTrue(enemy.Conditions.Holds("test.cloud"), "no blow of the player's spends it");
            Assert.AreEqual(0, Count(ConditionMoment.Attacked));
            enemy.Conditions.Attacked();
            Assert.AreEqual(1, Count(ConditionMoment.Attacked), "told once, and only for the one that answers it");
            Assert.AreEqual("test.cloud", Last(ConditionMoment.Attacked).Id);
            Assert.IsFalse(enemy.Conditions.Holds("test.cloud"), "its blow spends the cloud");
            Assert.AreEqual(0, Count(ConditionMoment.Ended), "spent, not run out: no end is told");
            Assert.IsTrue(enemy.Conditions.Holds("test.quiet"), "one that is not spent by it stays");
            enemy.Conditions.Attacked();
            Assert.AreEqual(1, Count(ConditionMoment.Attacked), "the next blow finds nothing to answer");
        }

        [Test]
        public void AFrozenEnemyIsKeptFrozenLongerAndOneNotFrozenIsNot()
        {
            // Six Levels Down: a Chill laid on a frozen enemy does nothing, so the bite keeps the freeze itself.
            var enemy = NewEnemy();
            Assert.IsFalse(enemy.KeepFrozen(0.3f), "nothing frozen, nothing kept");
            enemy.Apply(AfflictionKind.Chill, 5);
            float left = enemy.FrozenLeft;
            Assert.IsTrue(enemy.KeepFrozen(0.3f));
            Assert.AreEqual(left + 0.3f, enemy.FrozenLeft, 0.0001f);
            Run(enemy, left + 0.1f);
            Assert.IsTrue(enemy.IsFrozen, "still frozen past its own time");
            Run(enemy, 0.3f);
            Assert.IsFalse(enemy.IsFrozen, "and thawed once the kept time is done");
            Assert.AreEqual(0, enemy.Stacks(AfflictionKind.Chill));
        }
    }
}
