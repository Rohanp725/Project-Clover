using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The plain rules of the gods' gifts and the crowns of the mixed branches: which forms the marks begin, and the sums the forms do.</summary>
    public class SignatureMoveTests
    {
        [TearDown]
        public void LetPoisonBite() => Afflictions.PoisonWaits = false;

        [Test]
        public void EveryFormAMarkBeginsIsRegisteredAndHasItsNumbers()
        {
            int begun = 0;
            foreach (var mark in Upgrade.All)
                foreach (var effect in mark.Effects)
                {
                    if (effect.Do != UpgradeAction.Form) continue;
                    begun++;
                    Assert.IsNotNull(SpellForms.Get(effect.Form), $"{mark.Id} begins a form '{effect.Form}' that is not registered.");
                    Assert.IsNotNull(effect.Spell, $"{mark.Id} begins '{effect.Form}' with no numbers.");
                    Assert.AreEqual(0f, effect.Spell.Cost, $"{mark.Id}: a form begun by a mark costs no mana.");
                }
            Assert.GreaterOrEqual(begun, 17);
        }

        [Test]
        public void TheSunsRoadEndsItsDawnAtTheEnemyOrAtTheEndOfItsReachOrItsTime()
        {
            Assert.IsFalse(SunRules.DawnOver(1f, 4f, 3f), "Still on its way.");
            Assert.IsTrue(SunRules.DawnOver(1f, 4f, SunRules.DawnTouch), "It has reached the enemy faced.");
            Assert.IsTrue(SunRules.DawnOver(1f, SunRules.DawnReach, 5f), "It has been rolled as far as it goes.");
            Assert.IsTrue(SunRules.DawnOver(SunRules.DawnSeconds, 2f, 5f), "Its time is up.");
            Assert.AreEqual(6f, SunRules.DawnSpeed);
            Assert.AreEqual(9f, SunRules.DawnReach);
        }

        [Test]
        public void TheSettingSunIsHeavierForEveryBurnUpToDouble()
        {
            Assert.AreEqual(1f, SunRules.Weight(0), 1e-4f);
            Assert.AreEqual(1.24f, SunRules.Weight(3), 1e-4f);
            Assert.AreEqual(2f, SunRules.Weight(40), 1e-4f);
            Assert.AreEqual(1f, SunRules.Weight(-2), 1e-4f);
        }

        [Test]
        public void ASunSentUnderRisesAgainOnlyWithThreeSecondsOfSurgeLeft()
        {
            Assert.IsTrue(SunRules.RisesAgain(3f));
            Assert.IsTrue(SunRules.RisesAgain(7.5f));
            Assert.IsFalse(SunRules.RisesAgain(2.9f));
            Assert.IsFalse(SunRules.RisesAgain(0f));
        }

        [Test]
        public void EveryGiftAndCrownIsBespokeNow()
        {
            // No Signature is still carried by one of the generic forms or by plain bursts alone.
            string[] generic = { "flash", "pillars", "wave", "bolt" };
            foreach (var mark in Upgrade.All.Where(mark => mark.Signature))
            {
                if (mark.IsSpell) Assert.IsFalse(generic.Contains(mark.Form), $"{mark.Id} is still carried by the generic form '{mark.Form}'.");
                if (mark.IsSpell) Assert.IsNotNull(SpellForms.Get(mark.Form), $"{mark.Id} wears a form that is not registered.");
            }
            Assert.AreEqual(SpellForms.Mirrors, Upgrade.All.First(mark => mark.Name == "Six Mirrors").Form);
            Assert.AreEqual(SpellForms.Pyres, Upgrade.All.First(mark => mark.Name == "Light the Pyres").Form);
        }

        [Test]
        public void HeldPoisonNeitherBitesNorFadesAndOwesEveryBeat()
        {
            var spec = Afflictions.Spec(AfflictionKind.Poison);
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Poison, 4);
            Afflictions.PoisonWaits = true;
            float bitten = 0f;
            // Far longer than the poison would have lasted.
            float waited = spec.Seconds * 2f;
            for (float t = 0f; t < waited; t += 0.1f) bitten += set.Tick(0.1f);
            Assert.AreEqual(0f, bitten, 0.001f, "Poison bit while it was made to wait.");
            Assert.AreEqual(4, set.Stacks(AfflictionKind.Poison), "Poison faded while it was made to wait.");
            float owed = set.TakeOwed();
            float beats = (float)System.Math.Floor(set.OwedSeconds / spec.TickSeconds);
            Assert.Greater(owed, 0f);
            Assert.AreEqual(0f, beats, "What was owed is forgotten once it is taken.");
            Assert.AreEqual(0f, set.TakeOwed(), 0.001f, "The same bites were paid twice.");
            // One bite for each whole beat, at four stacks.
            float expected = (float)System.Math.Floor((waited + 0.05f) / spec.TickSeconds) * spec.DamagePerTick * 4f;
            Assert.AreEqual(expected, owed, spec.DamagePerTick * 4f + 0.01f);
            Afflictions.PoisonWaits = false;
            float after = 0f;
            for (float t = 0f; t < spec.TickSeconds + 0.2f; t += 0.1f) after += set.Tick(0.1f);
            Assert.Greater(after, 0f, "Poison did not bite again once it was let go.");
        }

        [Test]
        public void NothingIsOwedByWhatCarriesNoPoison()
        {
            var set = new AfflictionSet();
            Afflictions.PoisonWaits = true;
            for (int i = 0; i < 30; i++) set.Tick(0.1f);
            Assert.AreEqual(0f, set.TakeOwed());
        }

        [Test]
        public void TheScalesPayAShareOfTheDifferenceAndNothingWhenFoundWanting()
        {
            Assert.AreEqual(60f, ScalesForm.Verdict(150f, 50f, 60f, 120f), 0.001f);
            Assert.AreEqual(120f, ScalesForm.Verdict(1000f, 0f, 60f, 120f), 0.001f, "A sword of the Scales dealt more than its most.");
            Assert.AreEqual(0f, ScalesForm.Verdict(40f, 40f, 60f, 120f), "Even pans are not the heavier.");
            Assert.AreEqual(0f, ScalesForm.Verdict(10f, 90f, 60f, 120f));
        }

        [Test]
        public void TheReturningLightGivesBackAShareOfWhatWasLostInTheDark()
        {
            Assert.AreEqual(25f, TotalityForm.Returned(50f, 50f, 80f), 0.001f);
            Assert.AreEqual(80f, TotalityForm.Returned(1000f, 50f, 80f), 0.001f);
            Assert.AreEqual(0f, TotalityForm.Returned(-5f, 50f, 80f));
        }

        [Test]
        public void ShardsAndPlatesStopAtTheirMost()
        {
            Assert.AreEqual(4, DustForm.After(0, 4));
            Assert.AreEqual(12, DustForm.After(8, 4));
            Assert.AreEqual(DustForm.Most, DustForm.After(11, 4));
            Assert.AreEqual(2, PlatesForm.After(0, 2));
            Assert.AreEqual(PlatesForm.Most, PlatesForm.After(4, 2));
            // Five plates never turn a whole wound.
            Assert.Less(PlatesForm.Most * PlatesForm.TurnedEach, 0.5f);
        }

        [Test]
        public void TheAnvilForgesOneBladeForEachEnemyReachedUpToItsMost()
        {
            Assert.AreEqual(0, AnvilForm.Forged(0, 6));
            Assert.AreEqual(3, AnvilForm.Forged(3, 6));
            Assert.AreEqual(6, AnvilForm.Forged(9, 6));
        }

        [Test]
        public void TheWaveRollsFromAPaceAheadAtItsSpeedAndNoFurtherThanItsReach()
        {
            Assert.AreEqual(1f, FrozenWaveForm.Rolled(0f, 6f, 9f), 0.001f);
            Assert.AreEqual(4f, FrozenWaveForm.Rolled(0.5f, 6f, 9f), 0.001f);
            Assert.AreEqual(9f, FrozenWaveForm.Rolled(5f, 6f, 9f), 0.001f);
            Assert.AreEqual(SpellForms.FrozenWave, Upgrade.All.First(mark => mark.Name == "The Wave Stands Still").Form);
            Assert.AreEqual(SpellForms.Flashes, Upgrade.All.First(mark => mark.Name == "Seven Flashes").Form);
        }

        [Test]
        public void TheBallGrowsWithEachStrikeUpToThreeAndTheHeartReturnsWhatWasPawnedAndAShareOfTheBlows()
        {
            Assert.AreEqual(16f, BallForm.Burst(16f, 12f, 0), 0.001f);
            Assert.AreEqual(40f, BallForm.Burst(16f, 12f, 2), 0.001f);
            Assert.AreEqual(52f, BallForm.Burst(16f, 12f, 9), 0.001f, "A ball struck more than three times grew past its most.");
            Assert.AreEqual(99f, HeartForm.Redeemed(99f, 0f, 30f), 0.001f);
            Assert.AreEqual(129f, HeartForm.Redeemed(99f, 100f, 30f), 0.001f);
            Assert.AreEqual(SpellForms.Ball, Upgrade.All.First(mark => mark.Name == "Ball Lightning").Form);
            var heart = Upgrade.All.First(mark => mark.Name == "Pawned Heart");
            Assert.AreEqual(SpellForms.Heart, heart.Form);
            Assert.AreEqual(0f, heart.Spell.Cost, "Pawned Heart costs mana; its words say it does not.");
        }

        [Test]
        public void TheWindRemembersNoMoreCutsThanARunCanKeep()
        {
            Assert.LessOrEqual(RememberForm.Most, SpellWorks.NotesEach);
        }
    }
}
