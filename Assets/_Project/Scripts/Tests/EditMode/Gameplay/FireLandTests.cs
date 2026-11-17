using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The plain rules of the Fire land: no element, light and shade, the snatch, the hold, the ranks.</summary>
    public class FireLandTests
    {
        private const float Frame = 1f / 60f;

        [TearDown]
        public void ForgetTheLight() => Sunlight.Clear();

        // ---------------------------------------------------------------- no element

        [Test]
        public void ASceneStartsEmptyOnlyWhereItAsksAndOnlyUntilAnElementIsCarried()
        {
            Assert.IsTrue(Elements.StartsEmpty(true, ""));
            Assert.IsTrue(Elements.StartsEmpty(true, null));
            Assert.IsFalse(Elements.StartsEmpty(false, ""), "the demo's scenes do not ask, so they start with Fire as before");
            Assert.IsFalse(Elements.StartsEmpty(true, Elements.Fire), "once the god's element is taken it is kept");
        }

        [Test]
        public void HoldingNothingLaysNothingAndIsNoElementOfTheRing()
        {
            Assert.AreEqual(AfflictionKind.None, Elements.None.Affliction);
            Assert.IsFalse(Elements.All.Contains(Elements.None));
            foreach (var element in Elements.All) Assert.AreNotEqual(Elements.None.Name, element.Name);
            Assert.Less(Mathf.Abs(Elements.None.Colour.r - Elements.None.Colour.b), 0.1f, "no element's colour: a plain grey");
        }

        // ---------------------------------------------------------------- sun and shade

        [Test]
        public void UnderAnOpenSkyEverythingIsLitButTheShade()
        {
            Sunlight.Clear();
            Assert.IsFalse(Sunlight.InUse);
            Sunlight.SetOpenSky(true);
            Sunlight.AddShade(0f, 10f, 0f, 6f);
            Assert.IsTrue(Sunlight.InUse);
            Assert.IsTrue(Sunlight.IsLit(new Vector3(-1f, 0f, 3f)));
            Assert.IsFalse(Sunlight.IsLit(new Vector3(5f, 0f, 3f)));
            Assert.IsFalse(Sunlight.IsLit(new Vector3(5f, 2f, 3f)), "height does not matter");
            Assert.IsTrue(Sunlight.IsLit(new Vector3(5f, 0f, 6.1f)));
        }

        [Test]
        public void UnderARoofOnlyTheBarsOfLightAreLitAndShadeWinsOverLight()
        {
            Sunlight.Clear();
            Sunlight.AddLight(-22f, 22f, 408f, 412f);
            Assert.IsTrue(Sunlight.IsLit(new Vector3(0f, 4f, 410f)));
            Assert.IsFalse(Sunlight.IsLit(new Vector3(0f, 4f, 420f)));
            Sunlight.AddShade(-1f, 1f, 409f, 411f);
            Assert.IsFalse(Sunlight.IsLit(new Vector3(0f, 4f, 410f)));
            Assert.IsTrue(Sunlight.IsLit(new Vector3(5f, 4f, 410f)));
        }

        [Test]
        public void AFlareLightsItsCircleForItsSecondsAndNoLonger()
        {
            Sunlight.Clear();
            Sunlight.AddLight(100f, 101f, 100f, 101f);
            Assert.IsFalse(Sunlight.IsLit(Vector3.zero));
            Sunlight.Flare(Vector3.zero, 3f, 4f);
            Assert.IsTrue(Sunlight.IsLit(new Vector3(2.9f, 0f, 0f)));
            Assert.IsFalse(Sunlight.IsLit(new Vector3(3.2f, 0f, 0f)));
            Sunlight.Tick(3.9f);
            Assert.IsTrue(Sunlight.IsLit(Vector3.zero));
            Sunlight.Tick(0.2f);
            Assert.IsFalse(Sunlight.IsLit(Vector3.zero));
        }

        [Test]
        public void ARollThroughABarOfLightIsNotFoundButStandingInItIs()
        {
            var sense = new SunSense();
            // A roll is 0.35 s: lit for that long, then shade again.
            for (float t = 0f; t < 0.35f; t += Frame) sense.Tick(Frame, true, 10f);
            Assert.IsFalse(sense.Found);
            sense.Tick(Frame, false, 10f);
            for (float t = 0f; t < 0.3f; t += Frame) sense.Tick(Frame, true, 10f);
            Assert.IsFalse(sense.Found, "the count starts again after a break");
            for (float t = 0f; t < 0.15f; t += Frame) sense.Tick(Frame, true, 10f);
            Assert.IsTrue(sense.Found);
            Assert.IsTrue(sense.FoundLit);
            sense.Tick(Frame, false, 10f);
            Assert.IsFalse(sense.Found, "into the shade: lost at once");
        }

        [Test]
        public void TheSunBlindFindByHeatAndByAFightAndNotFromTooFar()
        {
            var sense = new SunSense();
            for (float t = 0f; t < 1f; t += Frame) sense.Tick(Frame, true, 19f);
            Assert.IsFalse(sense.Found, "lit, but beyond his reach");
            sense.Tick(Frame, false, 2.4f);
            Assert.IsTrue(sense.Found, "the heat of a body");
            Assert.IsFalse(sense.FoundLit, "found, but not in the sun: no ray");
            sense.Tick(Frame, false, 5f);
            Assert.IsFalse(sense.Found);
            sense.Fight(9f);
            sense.Tick(Frame, false, 5f);
            Assert.IsFalse(sense.Found, "a blow nine metres off is not felt");
            sense.Fight(5f);
            for (float t = 0f; t < 3.9f; t += Frame) sense.Tick(Frame, false, 5f);
            Assert.IsTrue(sense.Found, "the heat of a fight lasts four seconds");
            for (float t = 0f; t < 0.3f; t += Frame) sense.Tick(Frame, false, 5f);
            Assert.IsFalse(sense.Found);
        }

        [Test]
        public void ARayIsOnlyChosenAtALitTargetAndASnatchOnlyFromAFullBelt()
        {
            var priest = EnemyKind.SunBlind();
            priest.Brain.Reset();
            priest.Brain.Alert();
            priest.Brain.TargetLit = false;
            for (int i = 0; i < 120; i++) priest.Brain.Tick(Frame, 10f, 0.5f);
            Assert.AreEqual(EnemyState.Stalk, priest.Brain.State, "nothing he may throw at someone he has not found in the sun");
            priest.Brain.TargetLit = true;
            priest.Brain.Tick(Frame, 10f, 0.5f);
            Assert.AreEqual("sunblind_ray", priest.Brain.Current.Name);

            var thirst = EnemyKind.Thirsting();
            thirst.Brain.Reset();
            thirst.Brain.Alert();
            thirst.Brain.TargetFlasks = 1;
            // Between the claws' reach and the pounce's least distance only the snatch fits.
            for (int i = 0; i < 120; i++) thirst.Brain.Tick(Frame, 3.2f, 0.5f);
            Assert.IsNull(thirst.Brain.Current, "one flask left: it is not worth the reach");
            thirst.Brain.TargetFlasks = 2;
            thirst.Brain.Tick(Frame, 3.2f, 0.5f);
            Assert.AreEqual("thirst_snatch", thirst.Brain.Current.Name);
        }

        // ---------------------------------------------------------------- the snatch

        [Test]
        public void ASnatchedFlaskComesBackIfTheDrinkIsBrokenAndIsGoneIfItIsFinished()
        {
            var belt = new FlaskBelt();
            belt.Refill();
            var snatch = new FlaskSnatch { DrinkSeconds = 1.6f };
            Assert.IsTrue(snatch.Take(belt));
            Assert.AreEqual(2, belt.Count);
            Assert.IsFalse(snatch.Take(belt), "it holds one at a time");
            for (float t = 0f; t < 1.5f; t += Frame) Assert.IsFalse(snatch.Tick(Frame));
            Assert.IsTrue(snatch.GiveBack(belt));
            Assert.AreEqual(3, belt.Count);
            Assert.IsFalse(snatch.GiveBack(belt));

            Assert.IsTrue(snatch.Take(belt));
            bool drunk = false;
            for (float t = 0f; t < 1.7f && !drunk; t += Frame) drunk = snatch.Tick(Frame);
            Assert.IsTrue(drunk);
            Assert.IsFalse(snatch.GiveBack(belt), "drunk is drunk");
            Assert.AreEqual(2, belt.Count);
        }

        [Test]
        public void WithNoFlaskLeftTheSnatchIsOnlyAClaw()
        {
            var belt = new FlaskBelt();
            belt.Refill();
            while (belt.TryTake()) { }
            Assert.AreEqual(0, belt.Count);
            Assert.IsFalse(new FlaskSnatch().Take(belt));
            belt.PutBack();
            belt.PutBack();
            belt.PutBack();
            belt.PutBack();
            Assert.AreEqual(belt.Maximum, belt.Count, "the belt never holds more than it has room for");
        }

        // ---------------------------------------------------------------- the hold

        [Test]
        public void TheHoldLeftAloneBitesFiveTimesForThirty()
        {
            var kind = EnemyKind.Wick();
            var grip = new Grip { Seconds = kind.HoldSeconds, BiteEvery = kind.HoldBiteEvery, BiteDamage = kind.HoldBiteDamage, PressTakes = kind.HoldPressTakes };
            grip.Begin();
            int bites = 0;
            float held = 0f;
            while (grip.IsHeld && held < 5f)
            {
                bites += grip.Tick(Frame);
                held += Frame;
            }
            Assert.AreEqual(5, bites);
            Assert.AreEqual(30f, bites * grip.BiteDamage, 0.001f);
            Assert.AreEqual(2.5f, held, 0.05f);
            Assert.IsFalse(grip.BrokenByStruggle);
        }

        [Test]
        public void FivePressesBreakTheHoldAfterAboutASecondAndTwoBites()
        {
            var grip = new Grip();
            grip.Begin();
            int bites = 0;
            float held = 0f, sincePress = 0f;
            while (grip.IsHeld && held < 5f)
            {
                bites += grip.Tick(Frame);
                held += Frame;
                sincePress += Frame;
                // A press every seventh of a second or so.
                if (sincePress >= 0.15f)
                {
                    sincePress = 0f;
                    grip.Press();
                }
            }
            Assert.IsTrue(grip.BrokenByStruggle);
            Assert.AreEqual(5, grip.Presses);
            Assert.LessOrEqual(bites, 2);
            Assert.Less(held, 1.2f);
            grip.Press();
            Assert.AreEqual(5, grip.Presses, "a press with nothing holding is nothing");
        }

        [Test]
        public void TheGrabIsRedAndTakesHoldAndAHeavyBlowBreaksItWhileItIsShown()
        {
            var wick = EnemyKind.Wick();
            var grab = wick.Brain.Attacks.Find(a => a.Name == "wick_grab");
            Assert.IsTrue(grab.Unblockable);
            Assert.IsTrue(grab.Grabs);
            Assert.IsFalse(wick.Steadfast);
            wick.Brain.Reset();
            wick.Brain.Alert();
            Assert.IsTrue(wick.Brain.Force("wick_grab"));
            Assert.IsFalse(wick.Brain.Flinch(false), "a light blow does not break what it is showing");
            Assert.IsTrue(wick.Brain.Flinch(true));
            Assert.AreEqual(EnemyState.Flinch, wick.Brain.State);
        }

        [Test]
        public void BusyWithSomethingItDoesNotFlinchButABrokenBalanceEndsIt()
        {
            var thirst = EnemyKind.Thirsting();
            thirst.Brain.Reset();
            thirst.Brain.Alert();
            Assert.IsTrue(thirst.Brain.Occupy(1f));
            Assert.AreEqual(EnemyState.Busy, thirst.Brain.State);
            Assert.IsFalse(thirst.Brain.Flinch(true));
            for (float t = 0f; t < 0.9f; t += Frame) thirst.Brain.Tick(Frame, 2f, 0.5f);
            Assert.AreEqual(EnemyState.Busy, thirst.Brain.State);
            for (float t = 0f; t < 0.2f; t += Frame) thirst.Brain.Tick(Frame, 2f, 0.5f);
            Assert.AreNotEqual(EnemyState.Busy, thirst.Brain.State, "its time is up");
            thirst.Brain.Occupy();
            thirst.Brain.Stagger();
            Assert.AreEqual(EnemyState.Staggered, thirst.Brain.State);
            Assert.IsFalse(thirst.Brain.Occupy(), "nothing takes up one whose balance is broken");
        }

        // ---------------------------------------------------------------- the four kinds

        [Test]
        public void OnePerfectGuardBreaksTheThirsting()
        {
            var thirst = EnemyKind.Thirsting();
            Assert.AreEqual(40f, thirst.Health);
            Assert.GreaterOrEqual(thirst.PerfectGuardStagger, thirst.StaggerMaximum);
            Assert.AreEqual("thirst_snatch", thirst.SnatchAttack);
            Assert.IsTrue(thirst.Brain.Attacks.Find(a => a.Name == "thirst_snatch").Snatches);
            Assert.IsTrue(thirst.Brain.Attacks.Find(a => a.Name == "thirst_pounce").Unblockable);
            Assert.IsFalse(thirst.Brain.Attacks.Find(a => a.Name == "thirst_snatch").Unblockable, "the snatch can be guarded");
        }

        [Test]
        public void TheMoreGoldTheHigherTheRankAndTheHarder()
        {
            var levy = EnemyKind.Gilded(1);
            var spearman = EnemyKind.Gilded(2);
            var warden = EnemyKind.Gilded(3);
            Assert.AreEqual(new[] { 80f, 110f, 150f }, new[] { levy.Health, spearman.Health, warden.Health });
            Assert.AreEqual(new[] { 80f, 100f, 120f }, new[] { levy.StaggerMaximum, spearman.StaggerMaximum, warden.StaggerMaximum });
            Assert.AreEqual(new[] { 1, 2, 3 }, new[] { levy.Embers, spearman.Embers, warden.Embers });
            Assert.AreEqual(new[] { 3, 4, 5 }, new[] { levy.PoiseHits, spearman.PoiseHits, warden.PoiseHits });
            Assert.AreEqual(new[] { 2, 5, 6 }, new[] { levy.Brain.Attacks.Count, spearman.Brain.Attacks.Count, warden.Brain.Attacks.Count });
            Assert.IsTrue(levy.Guards && spearman.Guards && warden.Guards);
            Assert.IsFalse(levy.Steadfast || spearman.Steadfast);
            Assert.IsTrue(warden.Steadfast);
            Assert.AreEqual(1f, levy.HeavyGuardedDamage, "a heavy blow goes past the lower ranks' guard");
            Assert.AreEqual(0.5f, warden.HeavyGuardedDamage);
            float Thrust(EnemyKind kind) => kind.Brain.Attacks.Find(a => a.Name == "gilded_thrust").Damage;
            Assert.AreEqual(16f, Thrust(levy), 0.001f);
            Assert.AreEqual(16f * 1.15f, Thrust(spearman), 0.001f);
            Assert.AreEqual(16f * 1.3f, Thrust(warden), 0.001f);
            Assert.IsNull(levy.Brain.Attacks.Find(a => a.Name == "gilded_lunge"));
            Assert.IsNull(spearman.Brain.Attacks.Find(a => a.Name == "gilded_plant"));
            Assert.Greater(warden.Brain.Attacks.Find(a => a.Name == "gilded_plant").BurnSeconds, 0f);
            Assert.AreEqual(EnemyKind.GildedWardenName, EnemyKind.ByName(EnemyKind.GildedName(3)).Name);
            Assert.AreEqual("gilded3", warden.Palette);
        }

        [Test]
        public void TheFireLandsPeopleAreFoundByNameAndDrawNoStorm()
        {
            foreach (var kind in EnemyKind.FireKinds)
            {
                Assert.AreEqual(kind.Name, EnemyKind.ByName(kind.Name).Name);
                Assert.IsFalse(kind.DrawsStorm, kind.Name);
                Assert.IsTrue(kind.Ashen, kind.Name);
                foreach (var attack in kind.Brain.Attacks) Assert.IsTrue(attack.Name.StartsWith(kind.Prefix + "_"), attack.Name);
            }
            Assert.AreEqual(EnemyKind.SoldierName, EnemyKind.ByName("no such enemy").Name, "an unknown name is still the soldier");
            Assert.IsTrue(EnemyKind.SunBlind().HuntsByLight);
            Assert.IsTrue(EnemyKind.SunBlind().Brain.Attacks.Find(a => a.Name == "sunblind_ray").OnlyLit);
            Assert.Greater(EnemyKind.SunBlind().Brain.Attacks.Find(a => a.Name == "sunblind_flare").LightsSeconds, 0f);
        }

        [Test]
        public void TheYardsShadeLiesNorthOfItsRoofByTheNoonSun()
        {
            float north = Clover.EditorTools.LookBuilder.NoonShadowLength(9f);
            Assert.AreEqual(1.9f, north, 0.05f, "a 9 m roof throws its edge 1.9 m north");
            var shade = Clover.EditorTools.FireYardBuilder.PorchShade;
            Assert.Greater(shade.z, Clover.EditorTools.FireYardBuilder.PorchSouth);
            Assert.AreEqual(Clover.EditorTools.FireYardBuilder.PorchNorth - Clover.EditorTools.FireYardBuilder.PorchSouth, shade.w - shade.z, 0.001f);
        }
    }
}
