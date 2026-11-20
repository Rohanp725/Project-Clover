using Clover.Player;
using Clover.Player.Magic;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The spell clocks and what weapon play takes off them (STEEL_AND_ELEMENT.md 3.1; IMPLEMENTATION.md 3.1).</summary>
    public class SpellCooldownTests
    {
        [Test]
        public void AClockStartsRunsDownAndSaysWhenItIsReady()
        {
            var clocks = new SpellCooldowns();
            Assert.IsTrue(clocks.Ready(0), "nothing cast yet");
            clocks.Start(0, 8f);
            Assert.IsFalse(clocks.Ready(0));
            Assert.AreEqual(1f, clocks.Fraction(0), 1e-4f);
            Assert.AreEqual(0, clocks.Tick(4f));
            Assert.AreEqual(0.5f, clocks.Fraction(0), 1e-4f);
            Assert.AreEqual(4f, clocks.Remaining(0), 1e-4f);
            Assert.AreEqual(1, clocks.Tick(4.5f), "bit 0 says West came ready on this tick");
            Assert.IsTrue(clocks.Ready(0));
            Assert.AreEqual(0f, clocks.Fraction(0));
            Assert.AreEqual(0, clocks.Tick(1f), "a clock at rest is not reported again");
        }

        [Test]
        public void EachSlotKeepsItsOwnClock()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(1, 6f);
            clocks.Start(3, 12f);
            clocks.Tick(6f);
            Assert.IsTrue(clocks.Ready(1));
            Assert.IsFalse(clocks.Ready(3));
            Assert.IsTrue(clocks.Ready(0) && clocks.Ready(2));
        }

        [Test]
        public void ACutTakesSecondsOffEveryRunningClockAndNoneBelowZero()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(0, 7f);
            clocks.Start(2, 1f);
            int done = clocks.Cut(2f);
            Assert.AreEqual(5f, clocks.Remaining(0), 1e-4f);
            Assert.AreEqual(0f, clocks.Remaining(2));
            Assert.AreEqual(1 << 2, done);
            Assert.AreEqual(0f, clocks.Remaining(1), "a clock at rest stays at rest");
            Assert.AreEqual(0, clocks.Cut(0f));
        }

        [Test]
        public void ResetInstantNextAndFrozen()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(0, 9f);
            clocks.Reset(0);
            Assert.IsTrue(clocks.Ready(0));

            clocks.InstantNext(1);
            Assert.IsTrue(clocks.IsInstant(1));
            clocks.Start(1, 9f);
            Assert.IsTrue(clocks.Ready(1), "the promised cast starts no clock");
            Assert.IsFalse(clocks.IsInstant(1), "and the promise is spent");
            clocks.Start(1, 9f);
            Assert.IsFalse(clocks.Ready(1), "the cast after it does");

            clocks.Frozen = true;
            clocks.Start(2, 9f);
            Assert.IsTrue(clocks.Ready(2), "no clock starts while frozen");
            clocks.Frozen = false;

            clocks.Start(3, 9f);
            clocks.ResetAll();
            for (int i = 0; i < SpellCooldowns.Count; i++) Assert.IsTrue(clocks.Ready(i));
        }

        [Test]
        public void TheRateRunsTheClocksFasterOrSlower()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(0, 10f);
            clocks.Tick(2f, 2f);
            Assert.AreEqual(6f, clocks.Remaining(0), 1e-4f);
        }

        [Test]
        public void TheStrikeButtonsHaveTheirOwnClocks()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(SpellCooldowns.StrikeFirst, 8f);
            Assert.IsFalse(clocks.Ready(SpellCooldowns.StrikeFirst));
            Assert.IsTrue(clocks.Ready(SpellCooldowns.StrikeSecond));
            clocks.Cut(3f);
            Assert.AreEqual(5f, clocks.Remaining(SpellCooldowns.StrikeFirst), 1e-4f);
            Assert.IsTrue(clocks.Ready(-1) && clocks.Ready(SpellCooldowns.Count), "a slot that is not there is never waited on");
        }

        [TestCase(0f, false, 0.3f)]
        [TestCase(0f, true, 0.6f)]
        [TestCase(0.45f, false, 0.45f)]
        [TestCase(0.5f, true, 0.5f)]
        public void ABlowCutsByItsMove(float moveCut, bool heavy, float expected)
        {
            var cuts = new CooldownCuts();
            Assert.AreEqual(expected, cuts.ForBlow(moveCut, heavy), 1e-4f);
            Assert.AreEqual(expected, cuts.ForBlow(new AttackStep { CooldownCut = moveCut, Heavy = heavy }), 1e-4f);
        }

        [Test]
        public void GuardDodgeAndFinisherCutByTheirOwnNumbers()
        {
            var cuts = new CooldownCuts();
            Assert.AreEqual(2f, cuts.ForPerfectGuard, 1e-4f);
            Assert.AreEqual(1.5f, cuts.ForPerfectDodge, 1e-4f);
            Assert.AreEqual(3f, cuts.ForFinisher, 1e-4f);
            Assert.AreEqual(0f, cuts.ForBlow(null));
            cuts.Share = 1.5f;
            Assert.AreEqual(3f, cuts.ForPerfectGuard, 1e-4f, "the share scales every cut");
            Assert.AreEqual(0.45f, cuts.ForBlow(0f, false), 1e-4f);
        }

        [Test]
        public void ThreeBlowsAndAPerfectGuardShortenACooldownByTheirSum()
        {
            var clocks = new SpellCooldowns();
            var cuts = new CooldownCuts();
            clocks.Start(0, 10f);
            for (int i = 0; i < 3; i++) clocks.Cut(cuts.ForBlow(0f, false));
            clocks.Cut(cuts.ForPerfectGuard);
            Assert.AreEqual(10f - 0.9f - 2f, clocks.Remaining(0), 1e-4f);
        }

        [Test]
        public void TheReadyMinorsLeftMakeTheirOwnBaseSpellWaitAFifthLess()
        {
            // The owner's decision of 2026-11-19: what once cost less mana now waits less, for its own element's base spell alone.
            // Fire's Short Breath, Lightning's Quick Clap, Wind's Easy Breath, Ice's Thin Ice and Water's Low Water went with
            // their trees' reshaping (Phase 4): their Empower marks do it now.
            Assert.IsTrue(RetiredMarks.IsRetired(IceTree.ThinIce));
            Assert.IsTrue(RetiredMarks.IsRetired(WaterTree.LowWater));
            Assert.IsNull(Upgrade.ById(WaterTree.LowWater));
            var minors = new (string Id, string Element)[]
            {
                (EarthTree.NearToHand, Elements.Earth),
                (LifeTree.ReadySoil, Elements.Life), (LightTree.ReadyLight, Elements.Light),
                (MetalTree.ReadySteel, Elements.Metal), (PoisonTree.ReadyPoison, Elements.Poison),
            };
            foreach (var (id, element) in minors)
            {
                var mark = Upgrade.ById(id);
                Assert.IsNotNull(mark, id);
                float share = SpellTimings.BaseCooldownShare(mark, element);
                Assert.AreEqual(-0.2f, share, 1e-4f, id);
                Assert.AreEqual(SpellTimings.Base(element).Cooldown * 0.8f, SpellTimings.BaseCooldown(element, share), 1e-4f, id + " shortens its own base spell");
                string other = element == Elements.Fire ? Elements.Ice : Elements.Fire;
                Assert.AreEqual(0f, SpellTimings.BaseCooldownShare(mark, other), id + " leaves the other element's base spell alone");
                StringAssert.Contains("waits a fifth less between casts", mark.Description, id);
                StringAssert.DoesNotContain("mana", mark.Description, id);
            }
        }

        [Test]
        public void ABaseSpellsWaitIsItsTablesWithoutAShareAndNeverBelowATenth()
        {
            Assert.AreEqual(7f, SpellTimings.BaseCooldown(Elements.Ice, 0f), 1e-4f, "Hoarfrost waits 7 s as the table has it");
            Assert.AreEqual(5.6f, SpellTimings.BaseCooldown(Elements.Ice, -0.2f), 1e-4f);
            Assert.AreEqual(0.7f, SpellTimings.BaseCooldown(Elements.Ice, -5f), 1e-4f, "no share takes the wait below a tenth");
            Assert.AreEqual(0f, SpellTimings.BaseCooldownShare(null, Elements.Ice));
            Assert.AreEqual(0f, SpellTimings.BaseCooldownShare(Upgrade.ById(EarthTree.NearToHand), ""));
        }
    }
}
