using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The plain rules of the sun-court: the obelisk's turning shadow, what the Sun-Bearer chooses, where a lane stops, the man's moves, and taking Fire from nothing.</summary>
    public class SunCourtTests
    {
        [TearDown]
        public void ForgetTheLight() => Sunlight.Clear();

        // ---------------------------------------------------------------- the obelisk's shadow

        [Test]
        public void TheShadowIsAStripFromTheObeliskPointingNorthAtFirst()
        {
            var hand = new ShadowHand();
            hand.Reset();
            Assert.IsTrue(hand.Covers(new Vector3(0f, 0f, 10f)));
            Assert.IsTrue(hand.Covers(new Vector3(1.7f, 0f, 17.9f)));
            Assert.IsTrue(hand.Covers(new Vector3(-1.7f, 3f, 1.3f)), "height does not matter");
            Assert.IsFalse(hand.Covers(new Vector3(0f, 0f, 1.1f)), "nobody shelters by hugging the stone");
            Assert.IsFalse(hand.Covers(new Vector3(0f, 0f, 18.2f)));
            Assert.IsFalse(hand.Covers(new Vector3(1.9f, 0f, 10f)));
            Assert.IsFalse(hand.Covers(new Vector3(0f, 0f, -10f)), "not on the sun's side");
        }

        [Test]
        public void TheShadowTurnsClockwiseAtASteadyRateAndNeverJumps()
        {
            var hand = new ShadowHand();
            hand.Reset(ShadowHand.AgainstTheMan);
            for (int i = 0; i < 60; i++) hand.Tick(1f / 60f);
            Assert.AreEqual(4f, hand.Angle, 0.01f);
            hand.Reset(ShadowHand.AgainstTheForm);
            // A quarter of its minute: it points east, which is clockwise from north seen from above.
            for (int i = 0; i < 900; i++) hand.Tick(1f / 60f);
            Assert.AreEqual(90f, hand.Angle, 0.05f);
            Assert.IsTrue(hand.Covers(new Vector3(10f, 0f, 0f)));
            Assert.IsFalse(hand.Covers(new Vector3(0f, 0f, 10f)), "where it was is burning ground again");
            Assert.AreEqual(1f, hand.Direction.x, 0.001f);
        }

        [Test]
        public void InTheFuryItStopsForAMomentAndComesBackTheOtherWay()
        {
            var hand = new ShadowHand();
            hand.Reset(ShadowHand.InFury);
            hand.Tick(2f);
            Assert.AreEqual(18f, hand.Angle, 0.001f);
            hand.StopAndTurnBack(0.5f);
            hand.Tick(0.25f);
            Assert.AreEqual(18f, hand.Angle, 0.001f, "the stop is the warning");
            hand.Tick(0.3f);
            hand.Tick(1f);
            Assert.AreEqual(9f, hand.Angle, 0.001f, "and then it goes back the way it came");
        }

        [Test]
        public void AShaftAimedIntoTheShadowIsLaidClearOfItsNearerEdge()
        {
            var hand = new ShadowHand();
            hand.Reset();
            var inside = new Vector3(0.6f, 0f, 9f);
            Vector3 laid = hand.NearestOutside(inside, 2.6f);
            Assert.IsFalse(hand.Covers(laid));
            Assert.Greater(laid.x, 0f, "to the side they stand nearer");
            hand.Measure(laid, out _, out float across);
            Assert.AreEqual(hand.HalfWidth + 2.6f, across, 0.001f, "a shaft of 2.1 m laid there does not reach into the shadow");
            var outside = new Vector3(6f, 0f, 9f);
            Assert.AreEqual(outside, hand.NearestOutside(outside, 2.6f), "someone in the sun is aimed at where they stand");
        }

        [Test]
        public void TheTurningShadowIsShadeToTheSameRuleTheSunBlindHuntBy()
        {
            Sunlight.Clear();
            Sunlight.SetOpenSky(true);
            var hand = new ShadowHand();
            hand.Reset();
            Sunlight.Hand = hand;
            Assert.IsTrue(Sunlight.InUse);
            Assert.IsFalse(Sunlight.IsLit(new Vector3(0f, 0f, 9f)));
            Assert.IsTrue(Sunlight.InHand(new Vector3(0f, 0f, 9f)));
            Assert.IsTrue(Sunlight.IsLit(new Vector3(9f, 0f, 0f)));
            hand.Tick(22.5f);
            Assert.IsTrue(Sunlight.IsLit(new Vector3(0f, 0f, 9f)), "the shade moves with the hand");
            Assert.IsFalse(Sunlight.IsLit(new Vector3(9f, 0f, 0f)));
            Sunlight.Clear();
            Assert.IsNull(Sunlight.Hand, "the last play's shadow is not the next one's");
            Assert.AreEqual(new Vector3(0f, 0f, 9f), Sunlight.OutOfHand(new Vector3(0f, 0f, 9f)), "a scene with no hand aims where it always did");
        }

        // ---------------------------------------------------------------- lanes and the obelisk

        [Test]
        public void ALaneDrawnThroughTheObeliskStopsAtIt()
        {
            var from = new Vector3(0f, 0f, 17f);
            float through = SunBearer.Run(from, Vector3.back, 18.5f, 3.4f);
            Assert.AreEqual(17f - 3.4f, through, 0.001f);
            float past = SunBearer.Run(from, new Vector3(0.6f, 0f, -0.8f), 18.5f, 3.4f);
            Assert.Greater(past, 25f, "one that clears the stone runs on to the far rim");
            float away = SunBearer.Run(new Vector3(0f, 0f, -5f), Vector3.back, 18.5f, 3.4f);
            Assert.AreEqual(13.5f, away, 0.001f, "the stone behind the lane does not shorten it");
        }

        // ---------------------------------------------------------------- what the form chooses

        [Test]
        public void TheFormOpensByShowingEverythingOnce()
        {
            var chooser = new SunBearerChooser();
            chooser.Reset();
            var seen = new List<SunBearerAttack>();
            bool falcon = false;
            for (int i = 0; i < chooser.Opening.Length; i++)
            {
                var pick = chooser.Next(0.5f, falcon, false, 0, 100f);
                seen.Add(pick);
                if (pick == SunBearerAttack.Rising) falcon = true;
                if (pick == SunBearerAttack.Setting) falcon = false;
            }
            CollectionAssert.AreEqual(chooser.Opening, seen);
            for (var attack = SunBearerAttack.Roll; attack <= SunBearerAttack.Setting; attack++) CollectionAssert.Contains(seen, attack);
            Assert.AreEqual(SunBearerAttack.Zenith, seen.Last(), "the opening ends with the blaze");
        }

        [Test]
        public void AfterTheOpeningNothingComesTwiceRunningEachShapeDoesItsOwnAndTheShapesChange()
        {
            var chooser = new SunBearerChooser();
            chooser.Reset();
            var random = new System.Random(7);
            bool falcon = false;
            float sinceZenith = 100f;
            var last = SunBearerAttack.None;
            int changes = 0, zeniths = 0, inShape = 0;
            for (int i = 0; i < 400; i++)
            {
                var pick = chooser.Next((float)random.NextDouble(), falcon, false, 0, sinceZenith);
                Assert.AreNotEqual(last, pick, $"turn {i}");
                Assert.IsTrue(falcon ? SunBearerChooser.IsFalcons(pick) : SunBearerChooser.IsScarabs(pick), $"turn {i}: {pick} as the {(falcon ? "falcon" : "scarab")}");
                if (pick == SunBearerAttack.Zenith)
                {
                    if (i >= chooser.Opening.Length) Assert.GreaterOrEqual(sinceZenith, chooser.ZenithEvery);
                    sinceZenith = 0f;
                    zeniths++;
                }
                else sinceZenith += 6f;
                if (SunBearerChooser.IsChange(pick))
                {
                    falcon = pick == SunBearerAttack.Rising;
                    changes++;
                    inShape = 0;
                }
                else Assert.LessOrEqual(++inShape, 4, "it does not stay in a shape past four attacks");
                last = pick;
            }
            Assert.Greater(changes, 60);
            Assert.Greater(zeniths, 5);
        }

        [Test]
        public void NoNewLaneIsLaidWhileThreeAreHotAndTheRamAnswersWhoeverStandsNear()
        {
            var chooser = new SunBearerChooser();
            chooser.Reset();
            for (int i = 0; i < chooser.Opening.Length; i++) chooser.Record(chooser.Opening[i]);
            var random = new System.Random(3);
            bool falcon = false;
            for (int i = 0; i < 200; i++)
            {
                var pick = chooser.Next((float)random.NextDouble(), falcon, false, 3, 100f);
                Assert.IsFalse(SunBearerChooser.LaysLane(pick), $"turn {i}: {pick} with three lanes hot");
                if (SunBearerChooser.IsChange(pick)) falcon = pick == SunBearerAttack.Rising;
            }
            chooser.Reset();
            for (int i = 0; i < chooser.Opening.Length; i++) chooser.Record(chooser.Opening[i]);
            Assert.AreEqual(SunBearerAttack.Ram, chooser.Next(0.1f, false, true, 0, 100f));
            Assert.AreNotEqual(SunBearerAttack.Ram, chooser.Next(0.1f, false, true, 0, 100f), "never twice running");
        }

        [Test]
        public void TwoHeavyAttacksEarnALull()
        {
            var chooser = new SunBearerChooser();
            chooser.Reset();
            chooser.Record(SunBearerAttack.Roll);
            Assert.IsFalse(chooser.TakeLull());
            chooser.Record(SunBearerAttack.Sparks);
            Assert.IsFalse(chooser.TakeLull(), "the sparks are not heavy");
            chooser.Record(SunBearerAttack.Furrows);
            Assert.IsTrue(chooser.TakeLull());
            Assert.IsFalse(chooser.TakeLull(), "and the count begins again");
        }

        // ---------------------------------------------------------------- the man

        [Test]
        public void TheSunKingIsAGodWhoThrowsHisSpearCallsItBackAndBringsDownTheNoon()
        {
            var king = EnemyKind.ByName(EnemyKind.SunKingName);
            Assert.AreEqual(EnemyKind.SunKingName, king.Name);
            Assert.IsTrue(king.IsBoss);
            Assert.AreEqual("God of Fire", king.Title);
            Assert.AreEqual(480f, king.Health);
            Assert.AreEqual(0, king.Embers, "the five embers are paid when the form falls");
            var attacks = king.Brain.Attacks;
            Assert.AreEqual(attacks.Count, attacks.Select(attack => attack.Name).Distinct().Count());
            Assert.IsNotNull(attacks.Find(attack => attack.Name == king.SecondPhaseAttack));
            Assert.IsNotNull(attacks.Find(attack => attack.Name == king.Brain.CounterAttack));
            foreach (var attack in attacks)
                if (!string.IsNullOrEmpty(attack.ChainInto)) Assert.IsNotNull(attacks.Find(next => next.Name == attack.ChainInto), attack.Name);
            var thrown = attacks.Find(attack => attack.Name == "sunking_throw");
            var ray = attacks.Find(attack => attack.Name == thrown.ChainInto);
            Assert.IsTrue(thrown.Unblockable && ray.Unblockable);
            Assert.AreEqual(thrown.Reach, ray.Reach, "the ray comes back down the line the spear went");
            Assert.Greater(ray.LineWidth, thrown.LineWidth);
            Assert.GreaterOrEqual(ray.Telegraph, 1.8f, "bare-handed for as long as the design gives the player");
            foreach (string name in new[] { "sunking_noon_1", "sunking_noon_2", "sunking_noon_3" })
            {
                var noon = attacks.Find(attack => attack.Name == name);
                Assert.IsTrue(noon.Ranged && noon.Unblockable && noon.IsCircle, name);
            }
            Assert.AreEqual(1, attacks.Find(attack => attack.Name == "sunking_drive").MinPhase);
        }

        // ---------------------------------------------------------------- taking Fire from nothing

        [Test]
        public void TheFirstGodsElementIsFireItsGiftExistsAndOnceCarriedTheLandNoLongerStartsEmpty()
        {
            Assert.IsNotNull(ElementKit.ByName(Elements.Fire, false));
            Assert.IsNotNull(Gifts.SignatureOf(Elements.Fire), "Falling Sun comes with the element");
            Assert.IsTrue(Elements.StartsEmpty(true, ""));
            Assert.IsFalse(Elements.StartsEmpty(true, Elements.Fire));
            var reward = new GameObject("coal").AddComponent<GodReward>();
            try
            {
                reward.Configure((SunCourtDirector)null);
                Assert.IsTrue(reward.Takes, "in the sun-court there is nothing to choose: it can only be taken");
            }
            finally
            {
                Object.DestroyImmediate(reward.gameObject);
            }
        }
    }
}
