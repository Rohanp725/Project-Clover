using Clover.Gameplay;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The plain rules of the Fire land's second pass: the noon's swell, embers after a fall, the beam's light, the new kinds.</summary>
    public class FireSecondPassTests
    {
        [TearDown]
        public void ForgetTheLight() => Sunlight.Clear();

        // ---------------------------------------------------------------- the heat of noon

        [Test]
        public void TheSwellIsCalmThenWarnedThenBurnsThenCalmAgain()
        {
            var rhythm = new HeatSchedule(20f, 60f, 5f, 15f);
            Assert.AreEqual(HeatSchedule.Phase.Calm, rhythm.At(0f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Calm, rhythm.At(14.9f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Warning, rhythm.At(15f, out float warned));
            Assert.AreEqual(0f, warned, 0.001f);
            Assert.AreEqual(HeatSchedule.Phase.Warning, rhythm.At(17.5f, out warned));
            Assert.AreEqual(0.5f, warned, 0.001f, "half way through the warning, the sun mark is half full");
            Assert.AreEqual(HeatSchedule.Phase.Swell, rhythm.At(20f, out float swelled));
            Assert.AreEqual(0f, swelled, 0.001f);
            Assert.AreEqual(HeatSchedule.Phase.Swell, rhythm.At(34.9f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Calm, rhythm.At(35.1f, out _));
        }

        [Test]
        public void TheSwellComesAgainOnItsRhythmWithItsWarningBeforeIt()
        {
            var rhythm = new HeatSchedule(20f, 60f, 5f, 15f);
            Assert.AreEqual(HeatSchedule.Phase.Calm, rhythm.At(74f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Warning, rhythm.At(76f, out float warned));
            Assert.AreEqual(0.2f, warned, 0.001f);
            Assert.AreEqual(HeatSchedule.Phase.Swell, rhythm.At(81f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Swell, rhythm.At(20f + 60f * 10f + 3f, out _), "the eleventh swell");
        }

        [Test]
        public void ARhythmTooTightForItsOwnWarningAndSwellStillGivesEach()
        {
            var tight = new HeatSchedule(10f, 5f, 5f, 15f);
            Assert.AreEqual(HeatSchedule.Phase.Swell, tight.At(10f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Warning, tight.At(26f, out _), "the next is warned after the last has ended");
            Assert.AreEqual(HeatSchedule.Phase.Swell, tight.At(30f, out _));
        }

        [Test]
        public void ASkyThatNeverSwellsAndOneThatSwellsOnceStaySo()
        {
            Assert.IsFalse(HeatSchedule.Never.Swells);
            for (float t = 0f; t < 600f; t += 7f) Assert.AreEqual(HeatSchedule.Phase.Calm, HeatSchedule.Never.At(t, out _));
            var once = new HeatSchedule(10f, 0f, 5f, 15f);
            Assert.AreEqual(HeatSchedule.Phase.Swell, once.At(12f, out _));
            Assert.AreEqual(HeatSchedule.Phase.Calm, once.At(200f, out _));
        }

        [Test]
        public void TheSwellBurnsInTheSunAndNotInShadeNorInACalmZoneNorOutsideASwell()
        {
            Assert.IsTrue(HeatSchedule.Burns(HeatSchedule.Phase.Swell, true, false));
            Assert.IsFalse(HeatSchedule.Burns(HeatSchedule.Phase.Swell, false, false), "shade saves");
            Assert.IsFalse(HeatSchedule.Burns(HeatSchedule.Phase.Swell, true, true), "the elite's arena never burns");
            Assert.IsFalse(HeatSchedule.Burns(HeatSchedule.Phase.Warning, true, false), "the warning only warns");
            Assert.IsFalse(HeatSchedule.Burns(HeatSchedule.Phase.Calm, true, false));
        }

        [Test]
        public void ShadeUnderAnOpenSkyIsWhatTheSwellReads()
        {
            Sunlight.SetOpenSky(true);
            Sunlight.AddShade(0f, 4f, 0f, 4f);
            Assert.IsTrue(HeatSchedule.Burns(HeatSchedule.Phase.Swell, Sunlight.IsLit(new Vector3(10f, 0f, 10f)), false));
            Assert.IsFalse(HeatSchedule.Burns(HeatSchedule.Phase.Swell, Sunlight.IsLit(new Vector3(2f, 0f, 2f)), false));
        }

        // ---------------------------------------------------------------- the mirror's beam

        [Test]
        public void ABeamLightsItsLaneEvenInShadeAndGoesOutAfterItsSeconds()
        {
            Sunlight.SetOpenSky(true);
            Sunlight.AddShade(-20f, 20f, -20f, 20f);
            var inLane = new Vector3(0f, 0f, 5f);
            Assert.IsFalse(Sunlight.IsLit(inLane));
            Sunlight.Beam(new Vector3(0f, 1.3f, 0f), new Vector3(0f, 1.3f, 10f), 1f, 2f);
            Assert.IsTrue(Sunlight.IsLit(inLane), "the beam drives the player out of shade");
            Assert.IsTrue(Sunlight.IsLit(new Vector3(0.45f, 0f, 9f)));
            Assert.IsFalse(Sunlight.IsLit(new Vector3(1f, 0f, 5f)), "beside the lane is still shade");
            Assert.IsFalse(Sunlight.IsLit(new Vector3(0f, 0f, 11f)), "past its end is still shade");
            Sunlight.Tick(2.1f);
            Assert.IsFalse(Sunlight.IsLit(inLane));
        }

        // ---------------------------------------------------------------- embers after a fall

        [Test]
        public void AFallsEmbersGoToTheEnemiesKilledSinceTheRestAndComeBackOnTheirNextDeath()
        {
            var debt = new EmberDebt(5);
            debt.Killed(1);
            debt.Killed(3);
            Assert.AreEqual(0, debt.Fall(7), "nothing is lost for good while someone carries it");
            Assert.AreEqual(7, debt.Waiting);
            Assert.AreEqual(4, debt.Owed(1), "the first carrier takes the remainder");
            Assert.AreEqual(3, debt.Owed(3));
            Assert.IsFalse(debt.Carries(0));
            Assert.AreEqual(4, debt.Reclaim(1));
            Assert.AreEqual(0, debt.Reclaim(1), "a share is given once");
            Assert.AreEqual(3, debt.Waiting);
            Assert.AreEqual(3, debt.Reclaim(3));
            Assert.AreEqual(0, debt.Waiting);
        }

        [Test]
        public void ASecondFallLosesWhatIsStillWaitingAndSpreadsOnlyTheNewLoss()
        {
            var debt = new EmberDebt(4);
            debt.Killed(0);
            debt.Killed(1);
            debt.Fall(6);
            Assert.AreEqual(3, debt.Reclaim(0));
            // The player kills enemy 2 and falls again carrying the 3 reclaimed and 2 more.
            debt.Killed(2);
            Assert.AreEqual(3, debt.Fall(5), "enemy 1's three are gone for good");
            Assert.AreEqual(3, debt.Forfeited);
            Assert.IsFalse(debt.Carries(1));
            Assert.AreEqual(5, debt.Owed(2), "the new loss goes to whoever was killed since the last fall");
            Assert.AreEqual(5, debt.Waiting);
        }

        [Test]
        public void AFallWithNobodyKilledSinceTheRestLosesTheEmbersForGood()
        {
            var debt = new EmberDebt(3);
            debt.Killed(0);
            debt.Rested();
            Assert.AreEqual(4, debt.Fall(4));
            Assert.AreEqual(0, debt.Waiting);
            Assert.AreEqual(0, debt.Fall(0), "a fall carrying nothing loses nothing");
        }

        [Test]
        public void ARestKeepsWhatIsWaiting()
        {
            var debt = new EmberDebt(2);
            debt.Killed(0);
            debt.Fall(3);
            debt.Rested();
            Assert.AreEqual(3, debt.Waiting, "resting does not throw away embers the dead still carry");
            Assert.AreEqual(3, debt.Reclaim(0));
        }

        // ---------------------------------------------------------------- the new kinds and moves

        [Test]
        public void TheAshenSwellsBurstsHurtsItsOwnSideAndBurstsWhenKilledEarly()
        {
            var ashen = EnemyKind.ByName(EnemyKind.AshenName);
            Assert.AreEqual(EnemyKind.AshenName, ashen.Name);
            Assert.AreEqual("ashen", ashen.Prefix);
            Assert.AreEqual("enemy_wick", ashen.ModelName, "built on the wick's model");
            Assert.AreEqual("ashen", ashen.Palette);
            var swell = ashen.Brain.Attacks.Find(a => a.Name == "ashen_swell");
            Assert.IsTrue(swell.Bursts);
            Assert.IsTrue(swell.Unblockable, "a guard does nothing against the burst");
            Assert.IsTrue(swell.IsCircle);
            Assert.AreEqual(ashen.BurstRadius, swell.Radius, 0.001f, "the red ring is the burst's own reach");
            Assert.Greater(swell.Telegraph, 1f, "the swell is long enough to run from");
            Assert.Greater(ashen.BurstsOnDeath, 0f);
            Assert.Greater(ashen.BurstEnemyDamage, 0f);
            Assert.Greater(ashen.PoiseHits, 50, "it does not flinch: only killing it stops the swell");
            Assert.Greater(ashen.WalkSpeed, EnemyKind.Wick().WalkSpeed, "it comes at a run");
        }

        [Test]
        public void TheWardenPoursThePriestThrowsABeamAndNoOneElseDoes()
        {
            var pour = EnemyKind.Gilded(3).Brain.Attacks.Find(a => a.Name == "gilded_pour");
            Assert.IsNotNull(pour);
            Assert.IsTrue(pour.Pours);
            Assert.IsNull(EnemyKind.Gilded(2).Brain.Attacks.Find(a => a.Pours), "only the warden pours");
            var beam = EnemyKind.SunBlind().Brain.Attacks.Find(a => a.Name == "sunblind_beam");
            Assert.IsTrue(beam.Beams);
            Assert.IsTrue(beam.IsLine);
            Assert.Greater(beam.LightsSeconds, 0f);
            foreach (var kind in EnemyKind.All)
                foreach (var attack in kind.Brain.Attacks)
                {
                    if (attack.Bursts) Assert.AreEqual("ashen", kind.Prefix, attack.Name);
                    if (attack.Beams) Assert.AreEqual("sunblind", kind.Prefix, attack.Name);
                    if (attack.Pours) Assert.AreEqual("gilded", kind.Prefix, attack.Name);
                }
        }

        [Test]
        public void HealthHealsUpToItsMostAndNotTheDead()
        {
            var health = new Health { Maximum = 40f };
            health.Fill();
            health.Damage(30f);
            health.Heal(5f);
            Assert.AreEqual(15f, health.Current, 0.001f);
            health.Heal(100f);
            Assert.AreEqual(40f, health.Current, 0.001f);
            health.Damage(100f);
            health.Heal(10f);
            Assert.IsTrue(health.IsDead, "a drink does not raise the dead");
        }
    }
}
