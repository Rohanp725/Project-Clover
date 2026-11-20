using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Magic;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The RuleBook (IMPLEMENTATION.md 3.10, task 4.4): every rule standard until a mark in force writes into it, standard
    /// again on each rebuild, and what each switch does where it is read: the affliction rules on an enemy's set (kept
    /// from fading, deepening when left alone, not spent by blows, resting after fading), the breath on the spell clocks,
    /// and the guard that cannot break. The book publishes its affliction rules to a static, which these tests leave clear.
    /// </summary>
    public class RuleBookTests
    {
        [SetUp]
        [TearDown]
        public void LeaveTheAfflictionsAsWritten() => Afflictions.Rules.Clear();

        private static void Run(AfflictionSet set, float seconds)
        {
            for (float t = 0f; t < seconds - 0.0001f; t += 0.1f) set.Tick(0.1f);
        }

        private static Upgrade Switching(System.Action<RuleBook> rules) => new Upgrade { Id = "t.switch", Kind = UpgradeKind.Devotion, Rules = rules };

        // ---- The book itself.

        [Test]
        public void ANewBookIsStandard()
        {
            var book = new RuleBook();
            Assert.IsTrue(book.IsStandard);
            Assert.AreEqual(CooldownMode.Normal, book.Cooldowns);
            Assert.AreEqual(GuardMode.Normal, book.Guard);
            Assert.AreEqual(RuleBook.StandardSlots, book.SlotCount);
            Assert.AreEqual(RuleBook.StandardAttunements, book.AttunementCount);
            Assert.IsTrue(book.FlasksAllowed && book.StaminaUsed && book.PerfectGuardOpen && book.PerfectDodgeOpen);
            Assert.IsFalse(book.Afflictions.Any);
        }

        [Test]
        public void AMarkWritesItsSwitchesAndAResetPutsEveryRuleBack()
        {
            var book = new RuleBook();
            Assert.IsFalse(book.Apply(new Upgrade { Id = "t.plain" }), "a mark with no switches writes nothing");
            Assert.IsFalse(book.Apply(null));
            Assert.IsTrue(book.Apply(Switching(rules =>
            {
                rules.Cooldowns = CooldownMode.None;
                rules.BreathAfterPerfect = 2.5f;
                rules.Guard = GuardMode.Unbreakable;
                rules.SlotCount = 1;
                rules.FlasksAllowed = false;
                rules.Afflictions.Keep(AfflictionKind.Burn);
            })));
            Assert.AreEqual(1, book.Switches);
            Assert.IsFalse(book.IsStandard);
            Assert.IsTrue(book.Afflictions.Keeps(AfflictionKind.Burn));
            // The run starts again, or the Hand changes: the book is rebuilt from nothing.
            book.Reset();
            Assert.IsTrue(book.IsStandard, "every switch back to its standard value");
            Assert.AreEqual(0, book.Switches);
            Assert.IsFalse(book.Afflictions.Keeps(AfflictionKind.Burn));
        }

        [Test]
        public void PublishingSetsWhatTheEnemiesReadAndARebuiltBookClearsIt()
        {
            var book = new RuleBook();
            book.Apply(Switching(rules => rules.Afflictions.Keep(AfflictionKind.Chill)));
            int version = book.Version;
            book.Publish();
            Assert.AreEqual(version + 1, book.Version);
            Assert.IsTrue(Afflictions.Rules.Keeps(AfflictionKind.Chill));
            book.Reset();
            book.Publish();
            Assert.IsFalse(Afflictions.Rules.Any, "a Devotion no longer in force stops keeping anything");
        }

        // ---- The affliction rules on an enemy.

        [Test]
        public void AKeptBurnNeverGoesOutByItselfButCanStillBeSetOff()
        {
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Burn, 1);
            Run(set, 12f);
            Assert.IsFalse(set.Burn.IsBurning, "as written, a burn goes out");

            Afflictions.Rules.Keep(AfflictionKind.Burn);
            set.Apply(AfflictionKind.Burn, 1);
            Run(set, 30f);
            Assert.IsTrue(set.Burn.IsBurning, "kept, it burns on");
            Assert.Greater(set.SetOff(AfflictionKind.Burn), 0f);
            Assert.IsFalse(set.Burn.IsBurning, "set off, it is gone");
        }

        [Test]
        public void ABurnLeftAloneDeepensEveryFourSecondsUpToFive()
        {
            Afflictions.Rules.Keep(AfflictionKind.Burn);
            Afflictions.Rules.Deepen(AfflictionKind.Burn, 4f, 5);
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Burn, 1);
            Run(set, 3.5f);
            Assert.AreEqual(1, set.Burn.Stacks, "not yet four seconds");
            Run(set, 1f);
            Assert.AreEqual(2, set.Burn.Stacks, "a stack after four seconds alone");
            // Something new laid on it: it is no longer left alone, and its four seconds start again.
            Run(set, 3f);
            set.Apply(AfflictionKind.Burn, 1);
            Assert.AreEqual(3, set.Burn.Stacks);
            Run(set, 3f);
            Assert.AreEqual(3, set.Burn.Stacks, "laid on three seconds ago");
            Run(set, 20f);
            Assert.AreEqual(5, set.Burn.Stacks, "five at most");
        }

        [Test]
        public void KeptChillStaysAndStillFreezesAtFullDepth()
        {
            Afflictions.Rules.Keep(AfflictionKind.Chill);
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Chill, 2);
            Run(set, 30f);
            Assert.AreEqual(2, set.Stacks(AfflictionKind.Chill), "Chill never fades");
            set.Apply(AfflictionKind.Chill, 3);
            Assert.IsTrue(set.IsFrozen, "five deep freezes as before");
        }

        [Test]
        public void AMarkNotSpentByBlowsLastsItsTimeThenRestsBeforeItCanBeLaidAgain()
        {
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Marked, 1);
            for (int i = 0; i < 3; i++) set.BlowLanded();
            Assert.IsFalse(set.Has(AfflictionKind.Marked), "as written, three blows spend a mark");

            Afflictions.Rules.NotSpentByBlows(AfflictionKind.Marked);
            Afflictions.Rules.RestAfterFading(AfflictionKind.Marked, 4f);
            set.Apply(AfflictionKind.Marked, 1);
            for (int i = 0; i < 20; i++) set.BlowLanded();
            Assert.IsTrue(set.Has(AfflictionKind.Marked), "not spent by blows");
            Run(set, Afflictions.Spec(AfflictionKind.Marked).Seconds + 0.2f);
            Assert.IsFalse(set.Has(AfflictionKind.Marked), "it fades with its time");
            Assert.Greater(set.RestLeft(AfflictionKind.Marked), 0f);
            Assert.IsFalse(set.Apply(AfflictionKind.Marked, 1).Applied, "resting: it cannot be laid again yet");
            Run(set, 4.1f);
            Assert.AreEqual(0f, set.RestLeft(AfflictionKind.Marked));
            Assert.IsTrue(set.Apply(AfflictionKind.Marked, 1).Applied, "rested: it can be laid again");
        }

        [Test]
        public void AnAfflictionSetOffOrSpentDoesNotRest()
        {
            Afflictions.Rules.RestAfterFading(AfflictionKind.Marked, 4f);
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Marked, 1);
            set.Consume(AfflictionKind.Marked);
            Assert.AreEqual(0f, set.RestLeft(AfflictionKind.Marked), "only fading by itself rests");
            Assert.IsTrue(set.Apply(AfflictionKind.Marked, 1).Applied);
        }

        [Test]
        public void ARestRunsOutAfterItsRuleHasLeftForce()
        {
            Afflictions.Rules.RestAfterFading(AfflictionKind.Marked, 4f);
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Marked, 1);
            Run(set, Afflictions.Spec(AfflictionKind.Marked).Seconds + 0.2f);
            Assert.Greater(set.RestLeft(AfflictionKind.Marked), 0f);
            Afflictions.Rules.Clear();
            Run(set, 4.1f);
            Assert.IsTrue(set.Apply(AfflictionKind.Marked, 1).Applied, "no mark is refused for ever");
        }

        [Test]
        public void ClearedRulesLeaveEveryAfflictionAsWritten()
        {
            var rules = new AfflictionRules();
            rules.Keep(AfflictionKind.Burn);
            rules.Deepen(AfflictionKind.Poison, 2f, 6);
            var copy = new AfflictionRules();
            copy.CopyFrom(rules);
            Assert.IsTrue(copy.Keeps(AfflictionKind.Burn));
            Assert.AreEqual(2f, copy.DeepensEvery(AfflictionKind.Poison));
            Assert.AreEqual(6, copy.DeepensTo(AfflictionKind.Poison));
            copy.Clear();
            Assert.IsFalse(copy.Any);
            Assert.IsFalse(copy.Keeps(AfflictionKind.Burn));
            Assert.AreEqual(0f, copy.DeepensEvery(AfflictionKind.Poison));
            rules.Keep(AfflictionKind.None);
            Assert.IsFalse(rules.Keeps(AfflictionKind.None), "nothing is kept of no affliction");
        }

        // ---- The breath on the spell clocks (In One Breath).

        [Test]
        public void InTheBreathEverySpellIsReadyAndACastWaitsItsWholeTimeAfter()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(0, 7f);
            clocks.Start(1, 5f);
            clocks.Start(SpellCooldowns.StrikeFirst, 8f);
            Assert.AreEqual(0b11, clocks.Breathe(2.5f), "the two spells waiting are freed, and say so");
            Assert.IsTrue(clocks.InBreath);
            Assert.IsTrue(clocks.Ready(0) && clocks.Ready(1) && clocks.Ready(2));
            Assert.AreEqual(0f, clocks.Fraction(0));
            Assert.AreEqual(0f, clocks.Remaining(1));
            Assert.IsFalse(clocks.Ready(SpellCooldowns.StrikeFirst), "the strikes are not spells: their clock runs");

            clocks.Start(0, 7f);
            clocks.Start(2, 9f);
            Assert.IsTrue(clocks.Ready(0) && clocks.Ready(2), "cast in the breath: no clock starts");
            Assert.AreEqual(7f, clocks.Owed(0));
            Assert.AreEqual(9f, clocks.Owed(2));

            clocks.Tick(2.6f);
            Assert.IsFalse(clocks.InBreath);
            Assert.AreEqual(6.9f, clocks.Remaining(0), 1e-3f, "after the breath its wait starts from full");
            Assert.AreEqual(8.9f, clocks.Remaining(2), 1e-3f);
            Assert.AreEqual(2.4f, clocks.Remaining(1), 1e-3f, "a spell not cast in the breath kept its clock running beneath it");
            Assert.IsTrue(clocks.Ready(3), "nothing was ever cast from South");
            Assert.AreEqual(0f, clocks.Owed(0));
        }

        [Test]
        public void ASecondBreathLastsTheLongerAndARestEndsIt()
        {
            var clocks = new SpellCooldowns();
            clocks.Breathe(2.5f);
            clocks.Tick(2f);
            Assert.AreEqual(0, clocks.Breathe(1f), "nothing was waiting");
            Assert.AreEqual(0.5f, clocks.BreathLeft, 1e-4f, "the longer of the two");
            clocks.Breathe(2.5f);
            Assert.AreEqual(2.5f, clocks.BreathLeft, 1e-4f);
            clocks.Start(0, 7f);
            clocks.ResetAll();
            Assert.IsFalse(clocks.InBreath);
            Assert.AreEqual(0f, clocks.Owed(0), "a rest forgives what the breath owed");
            Assert.IsTrue(clocks.Ready(0));
        }

        [Test]
        public void AMastersPromiseIsSpentBeforeTheBreathAndFrozenClocksTakeNoBreath()
        {
            var clocks = new SpellCooldowns();
            clocks.InstantNext(1);
            clocks.Breathe(2.5f);
            clocks.Start(1, 6f);
            Assert.AreEqual(0f, clocks.Owed(1), "the promise was spent: nothing is owed");
            Assert.IsFalse(clocks.IsInstant(1));

            var frozen = new SpellCooldowns { Frozen = true };
            Assert.AreEqual(0, frozen.Breathe(2.5f));
            Assert.IsFalse(frozen.InBreath, "with no clocks there is nothing to breathe through");
        }

        // ---- The guard that cannot break (The Guard Holds).

        private static Guard Raised()
        {
            var guard = new Guard();
            guard.SetRaised(true);
            return guard;
        }

        private static StaminaPool Pool(float left)
        {
            var pool = new StaminaPool();
            pool.Fill();
            pool.TrySpend(pool.Maximum - left);
            return pool;
        }

        [Test]
        public void AnUnbreakableGuardHoldsWhatTheStaminaCannotPayAsGreyHealth()
        {
            var guard = Raised();
            float cost = 20f * guard.StaminaPerDamage;
            Assert.AreEqual(GuardResult.Broken, guard.Resolve(20f, false, Pool(cost * 0.5f), out _), "as written, it breaks");

            var pool = Pool(cost * 0.5f);
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(20f, false, pool, out float chip, true));
            Assert.AreEqual(0f, pool.Current, "the stamina there was is spent");
            Assert.AreEqual(20f * (guard.ChipFraction + (1f - guard.ChipFraction) * 0.5f), chip, 1e-3f, "half unpaid: half the rest comes through");

            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(20f, false, Pool(0f), out chip, true));
            Assert.AreEqual(20f, chip, 1e-3f, "nothing paid: the whole blow comes through as grey health");

            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(20f, false, Pool(100f), out chip, true));
            Assert.AreEqual(20f * guard.ChipFraction, chip, 1e-3f, "paid in full: the usual chip");

            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(20f, true, Pool(100f), out _, true), "a red attack still lands");
        }
    }
}
