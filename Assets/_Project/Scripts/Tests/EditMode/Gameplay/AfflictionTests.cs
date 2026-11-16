using Clover.Gameplay;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The rules of the eleven afflictions: how deep they go, how long they last, and what each does to the one who carries it.</summary>
    public class AfflictionTests
    {
        [Test]
        public void EveryAfflictionHasItsNumbersAndAColourOfItsOwn()
        {
            for (int kind = 1; kind < Afflictions.Count; kind++)
            {
                var spec = Afflictions.Spec((AfflictionKind)kind);
                Assert.AreEqual((AfflictionKind)kind, spec.Kind);
                Assert.IsNotEmpty(spec.Name);
                Assert.Greater(spec.MaximumStacks, 0, spec.Name);
                Assert.Greater(spec.Seconds, 0f, spec.Name);
                for (int other = kind + 1; other < Afflictions.Count; other++)
                    Assert.AreNotEqual(spec.Colour, Afflictions.Spec((AfflictionKind)other).Colour, spec.Name);
            }
        }

        [Test]
        public void StacksStopAtTheMaximumAndLayingItAgainStartsItsTimeOver()
        {
            var set = new AfflictionSet();
            var spec = Afflictions.Spec(AfflictionKind.Static);
            Assert.IsFalse(set.Any);
            Assert.IsTrue(set.Apply(AfflictionKind.Static, 2).Applied);
            Assert.AreEqual(2, set.Stacks(AfflictionKind.Static));
            set.Tick(spec.Seconds - 1f);
            set.Apply(AfflictionKind.Static, 9);
            Assert.AreEqual(spec.MaximumStacks, set.Stacks(AfflictionKind.Static));
            set.Tick(spec.Seconds - 0.5f);
            Assert.IsTrue(set.Has(AfflictionKind.Static), "the second laying started the time over");
            set.Tick(1f);
            Assert.IsFalse(set.Has(AfflictionKind.Static));
            Assert.IsFalse(set.Any);
        }

        [Test]
        public void ReachingFullDepthIsSaidOnceOnly()
        {
            var set = new AfflictionSet();
            Assert.IsFalse(set.Apply(AfflictionKind.Static, 4).ReachedFull);
            Assert.IsTrue(set.Apply(AfflictionKind.Static, 1).ReachedFull);
            Assert.IsFalse(set.Apply(AfflictionKind.Static, 1).ReachedFull);
        }

        [Test]
        public void ChillSlowsAndAtFullDepthFreezesUntilItThaws()
        {
            var set = new AfflictionSet();
            var spec = Afflictions.Spec(AfflictionKind.Chill);
            set.Apply(AfflictionKind.Chill, 2);
            Assert.AreEqual(1f - 2 * spec.SlowPerStack, set.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(set.IsFrozen);
            Assert.IsTrue(set.Apply(AfflictionKind.Chill, 3).ReachedFull);
            Assert.IsTrue(set.IsFrozen);
            Assert.AreEqual(0f, set.SpeedMultiplier);
            Assert.IsFalse(set.Apply(AfflictionKind.Chill, 1).Applied, "what is frozen cannot be chilled further");
            set.Tick(spec.FrozenSeconds + 0.1f);
            Assert.IsFalse(set.IsFrozen);
            Assert.AreEqual(0, set.Stacks(AfflictionKind.Chill), "thawed, the cold is spent");
            Assert.AreEqual(1f, set.SpeedMultiplier, 0.0001f);
        }

        [Test]
        public void AHeavyBlowShattersTheFrozenForBigDamageAndEndsTheChill()
        {
            var set = new AfflictionSet();
            Assert.AreEqual(0f, set.Shatter(), "nothing frozen, nothing to break");
            set.Apply(AfflictionKind.Chill, 5);
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Chill).ShatterDamage, set.Shatter(), 0.0001f);
            Assert.IsFalse(set.IsFrozen);
            Assert.IsFalse(set.Has(AfflictionKind.Chill));
            Assert.AreEqual(0f, set.Shatter());
        }

        [Test]
        public void TheGreatAreFrozenForLessAndSomeThingsNotAtAll()
        {
            var boss = new AfflictionSet { FreezeScale = 0.4f };
            boss.Apply(AfflictionKind.Chill, 5);
            Assert.IsTrue(boss.IsFrozen);
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Chill).FrozenSeconds * 0.4f, boss.FrozenLeft, 0.0001f);
            var colossus = new AfflictionSet { CanFreeze = false };
            colossus.Apply(AfflictionKind.Chill, 5);
            Assert.IsFalse(colossus.IsFrozen);
            Assert.Greater(colossus.SpeedMultiplier, 0f);
        }

        [Test]
        public void SoakedTakesMoreFromSpellsAndCarriesStaticAndChillToThoseNear()
        {
            var set = new AfflictionSet();
            Assert.IsFalse(set.Apply(AfflictionKind.Static, 1).Carried, "dry, it carries nothing");
            // Soaked is not counted in stacks: laid twice, it is still one.
            set.Apply(AfflictionKind.Soaked, 2);
            Assert.AreEqual(1, set.Stacks(AfflictionKind.Soaked));
            Assert.AreEqual(1f + Afflictions.Spec(AfflictionKind.Soaked).SpellDamageTakenPerStack, set.DamageTakenMultiplier(true), 0.0001f);
            Assert.AreEqual(1.25f, set.DamageTakenMultiplier(true), 0.0001f, "a quarter more from spells, as the design has it");
            Assert.AreEqual(1f, set.DamageTakenMultiplier(false), 0.0001f);
            Assert.IsTrue(set.Apply(AfflictionKind.Static, 1).Carried);
            Assert.IsTrue(set.Apply(AfflictionKind.Chill, 1).Carried);
            Assert.IsFalse(set.Apply(AfflictionKind.Burn, 1).Carried);
            Assert.IsFalse(set.Apply(AfflictionKind.Poison, 1).Carried);
        }

        [Test]
        public void SunderedTakesMoreFromWeaponsAndNotFromSpells()
        {
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Sundered, 3);
            Assert.AreEqual(1f + 3 * Afflictions.Spec(AfflictionKind.Sundered).WeaponDamageTakenPerStack, set.DamageTakenMultiplier(false), 0.0001f);
            Assert.AreEqual(1f, set.DamageTakenMultiplier(true), 0.0001f);
        }

        [Test]
        public void TremorSlowsAndStopsBalanceFromRecovering()
        {
            var set = new AfflictionSet();
            Assert.IsTrue(set.BalanceRecovers);
            set.Apply(AfflictionKind.Tremor, 1);
            Assert.IsFalse(set.BalanceRecovers);
            Assert.Less(set.SpeedMultiplier, 1f);

            var steady = new StaggerMeter();
            var shaken = new StaggerMeter();
            steady.Add(50f);
            shaken.Add(50f);
            steady.Tick(steady.DrainDelay + 1f);
            shaken.Tick(shaken.DrainDelay + 1f, set.BalanceRecovers);
            Assert.Less(steady.Value, 50f);
            Assert.AreEqual(50f, shaken.Value, 0.0001f);
            set.Tick(Afflictions.Spec(AfflictionKind.Tremor).Seconds + 0.1f);
            Assert.IsTrue(set.BalanceRecovers);
        }

        [Test]
        public void UnsteadyStaggersSoonerIsShovedFurtherAndTurnsSlower()
        {
            var set = new AfflictionSet();
            Assert.AreEqual(1f, set.StaggerTakenMultiplier, 0.0001f);
            set.Apply(AfflictionKind.Unsteady, 2);
            Assert.Greater(set.StaggerTakenMultiplier, 1f);
            Assert.Greater(set.ShoveMultiplier, 1f);
            Assert.Less(set.TurnMultiplier, 1f);
        }

        [Test]
        public void MarkedMakesBlowsCriticalAndGivesManaSeededHealsAndBurstsCursedDealsLessAndFeedsItsKiller()
        {
            var set = new AfflictionSet();
            Assert.IsFalse(set.MakesBlowsCritical);
            // A mark is one deep until a mark of the tree deepens it, and carries three critical blows.
            set.Apply(AfflictionKind.Marked, 2);
            Assert.IsTrue(set.MakesBlowsCritical);
            Assert.AreEqual(1, set.Stacks(AfflictionKind.Marked));
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Marked).ManaPerStack, set.ManaOnBlow, 0.0001f);
            Assert.AreEqual(3, set.BlowsLeft(AfflictionKind.Marked));
            set.BlowLanded();
            set.BlowLanded();
            Assert.IsTrue(set.MakesBlowsCritical, "two blows spent, one left");
            set.Apply(AfflictionKind.Marked, 1);
            Assert.AreEqual(3, set.BlowsLeft(AfflictionKind.Marked), "marking again refills the three");
            set.BlowLanded();
            set.BlowLanded();
            set.BlowLanded();
            Assert.IsFalse(set.Has(AfflictionKind.Marked), "the third blow spends the mark");
            Assert.IsFalse(set.MakesBlowsCritical);
            set.Apply(AfflictionKind.Marked, 1);

            Assert.AreEqual(0f, set.HealOnBlow);
            Assert.AreEqual(0f, set.DeathBurst(out float none));
            set.Apply(AfflictionKind.Seeded, 3);
            Assert.AreEqual(3 * Afflictions.Spec(AfflictionKind.Seeded).HealOnBlowPerStack, set.HealOnBlow, 0.0001f);
            Assert.AreEqual(3 * Afflictions.Spec(AfflictionKind.Seeded).DeathBurstPerStack, set.DeathBurst(out float radius), 0.0001f);
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Seeded).DeathBurstRadius, radius, 0.0001f);

            Assert.AreEqual(1f, set.DamageDealtMultiplier, 0.0001f);
            // Cursed has one depth: laid twice, it is still one.
            set.Apply(AfflictionKind.Cursed, 2);
            Assert.AreEqual(1, set.Stacks(AfflictionKind.Cursed));
            Assert.AreEqual(1f - Afflictions.Spec(AfflictionKind.Cursed).DealsLessPerStack, set.DamageDealtMultiplier, 0.0001f);
            Assert.AreEqual(0.85f, set.DamageDealtMultiplier, 0.0001f);
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Cursed).HealOnKillPerStack, set.HealOnKill, 0.0001f);
        }

        [Test]
        public void PoisonDealsNothingBetweenItsBurstsAndABurstForEveryStackEachTwoSeconds()
        {
            var set = new AfflictionSet();
            var spec = Afflictions.Spec(AfflictionKind.Poison);
            Assert.AreEqual(2f, spec.TickSeconds, 0.0001f);
            set.Apply(AfflictionKind.Poison, 3);
            Assert.AreEqual(0f, set.Tick(1.9f));
            Assert.IsFalse(set.BitThisTick);
            Assert.AreEqual(3 * spec.DamagePerTick, set.Tick(0.2f), 0.0001f);
            Assert.IsTrue(set.BitThisTick);
            Assert.AreEqual(0f, set.Tick(1f));
            Assert.IsFalse(set.BitThisTick);
            // Deeper, the next burst is larger.
            set.Apply(AfflictionKind.Poison, 2);
            Assert.AreEqual(5 * spec.DamagePerTick, set.Tick(1f), 0.0001f);
        }

        [Test]
        public void ABurnLaidThroughTheSetIsTheSameBurnAsBefore()
        {
            var set = new AfflictionSet();
            var plain = new BurnStatus();
            set.Apply(AfflictionKind.Burn, 2);
            plain.Ignite(Afflictions.Spec(AfflictionKind.Burn).Seconds, 2);
            Assert.IsTrue(set.Has(AfflictionKind.Burn));
            Assert.AreEqual(2, set.Stacks(AfflictionKind.Burn));
            Assert.AreEqual(plain.Tick(1f), set.Tick(1f), 0.0001f);
            Assert.AreEqual(plain.Detonate(), set.SetOff(AfflictionKind.Burn), 0.0001f);
            Assert.IsFalse(set.Has(AfflictionKind.Burn));
            // A burn with no depth still counts as burning, one deep.
            set.Burn.Ignite(3f);
            Assert.AreEqual(1, set.Stacks(AfflictionKind.Burn));
        }

        [Test]
        public void SettingAnAfflictionOffSpendsItForDamageByItsDepth()
        {
            var set = new AfflictionSet();
            Assert.AreEqual(0f, set.SetOff(AfflictionKind.Static));
            set.Apply(AfflictionKind.Static, 4);
            Assert.AreEqual(4 * Afflictions.Spec(AfflictionKind.Static).SetOffPerStack, set.SetOff(AfflictionKind.Static), 0.0001f);
            Assert.IsFalse(set.Has(AfflictionKind.Static));
            set.Apply(AfflictionKind.Poison, 2);
            Assert.AreEqual(2, set.Consume(AfflictionKind.Poison));
            Assert.AreEqual(0, set.Consume(AfflictionKind.Poison));
            // Frozen, setting the chill off also breaks the ice.
            set.Apply(AfflictionKind.Chill, 5);
            var chill = Afflictions.Spec(AfflictionKind.Chill);
            Assert.GreaterOrEqual(set.SetOff(AfflictionKind.Chill), chill.ShatterDamage);
            Assert.IsFalse(set.IsFrozen);
        }

        [Test]
        public void AStrongerLayingDoesMoreAndClearingForgetsEverything()
        {
            var plain = new AfflictionSet();
            var strong = new AfflictionSet();
            plain.Apply(AfflictionKind.Poison, 2);
            strong.Apply(AfflictionKind.Poison, 2, 0f, 1.5f);
            Assert.AreEqual(plain.Tick(2.1f) * 1.5f, strong.Tick(2.1f), 0.0001f);
            strong.Apply(AfflictionKind.Chill, 5);
            strong.Apply(AfflictionKind.Burn, 1);
            strong.Clear();
            Assert.IsFalse(strong.Any);
            Assert.IsFalse(strong.IsFrozen);
            Assert.AreEqual(1f, strong.SpeedMultiplier, 0.0001f);
            // A longer laying lasts longer.
            var longer = new AfflictionSet();
            longer.Apply(AfflictionKind.Marked, 1, 20f);
            longer.Tick(15f);
            Assert.IsTrue(longer.Has(AfflictionKind.Marked));
        }
    }
}
