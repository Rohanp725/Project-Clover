using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The Body branch (way_plan.md 0.4; way_build_plan.md 2.4; task 5.4): twelve marks, 28 Memories, in the decided shape
    /// (a stem of four, then the enduring body and the light body, each ending in a bearing); always in force whatever is
    /// carried; the bearings exclusive, the first bought set; the numbers summed to health 135, stamina 125, four flasks
    /// healing 55; Steady's poise; the Light Body's flow, never a third; and the found pieces stacking.
    /// </summary>
    public class BodyWayTests
    {
        private static List<Upgrade> Body => Upgrade.All.Where(mark => mark.Tree == MarkTree.Body).ToList();

        private static PlayerStats Summed(IEnumerable<string> ids)
        {
            var stats = new PlayerStats();
            foreach (string id in ids)
                foreach (var mod in Upgrade.ById(id).Stats)
                    stats.AddLearned(mod);
            return stats;
        }

        [Test]
        public void TwelveMarksTwentyEightMemoriesInTheDecidedShape()
        {
            var body = Body;
            Assert.AreEqual(12, body.Count);
            Assert.AreEqual(28, body.Sum(mark => mark.Cost));
            foreach (var mark in body)
            {
                Assert.AreEqual(Currency.Memories, mark.Currency, mark.Id);
                Assert.AreEqual("", mark.Weapon, mark.Id + ": on every weapon's chart");
                Assert.AreEqual(mark.Kind == UpgradeKind.Keystone ? 4 : 2, mark.Cost, mark.Id);
            }
            string[][] lines =
            {
                new[] { BodyWay.Health1, BodyWay.Stamina1, BodyWay.FourthFlask, BodyWay.Health2 },
                new[] { BodyWay.Health2, BodyWay.FullFlask, BodyWay.Steady, BodyWay.HardFrame, BodyWay.EnduringBody },
                new[] { BodyWay.Health2, BodyWay.DeepLungs, BodyWay.QuickBreath, BodyWay.LongStep, BodyWay.LightBody },
            };
            CollectionAssert.IsEmpty(Upgrade.ById(BodyWay.Health1).Requires);
            foreach (var line in lines)
                for (int i = 1; i < line.Length; i++)
                    CollectionAssert.AreEqual(new[] { line[i - 1] }, Upgrade.ById(line[i]).Requires, line[i]);
            Assert.AreEqual(2, body.Count(mark => mark.Kind == UpgradeKind.Keystone), "two bearings");
            Assert.IsTrue(WayRules.IsKeystone(Upgrade.ById(BodyWay.EnduringBody)));
            Assert.AreEqual(WayRules.BodyBough, WayRules.BoughOf(Upgrade.ById(BodyWay.LightBody)));
        }

        [Test]
        public void TheBodyIsInForceWhateverIsCarriedAndOnlyTheSetBearing()
        {
            var learned = new HashSet<string>(Body.Select(mark => mark.Id));
            foreach (string weapon in new[] { "", Weapons.KatanaName, Weapons.ScytheName, Weapons.StormKatanaName })
                Assert.IsTrue(WayRules.InForce(Upgrade.ById(BodyWay.Steady), new Loadout { Weapon = weapon }), "carrying " + weapon);
            var loadout = new Loadout { Weapon = Weapons.KatanaName };
            Assert.IsFalse(WayRules.InForce(Upgrade.ById(BodyWay.LightBody), loadout), "no bearing set");
            Assert.IsTrue(WayRules.SettleBought(Upgrade.ById(BodyWay.LightBody), loadout), "the first bought sets itself");
            Assert.IsFalse(WayRules.SettleBought(Upgrade.ById(BodyWay.EnduringBody), loadout), "the second waits for a rest fire");
            Assert.IsTrue(WayRules.InForce(Upgrade.ById(BodyWay.LightBody), loadout));
            Assert.IsFalse(WayRules.InForce(Upgrade.ById(BodyWay.EnduringBody), loadout));
            Assert.IsTrue(WayRules.ChooseKeystone(loadout, Upgrade.ById(BodyWay.EnduringBody), learned));
            Assert.IsTrue(WayRules.InForce(Upgrade.ById(BodyWay.EnduringBody), loadout));
            Assert.IsFalse(WayRules.InForce(Upgrade.ById(BodyWay.LightBody), loadout));
            Assert.IsTrue(WayRules.InForce(Upgrade.ById(BodyWay.LongStep), loadout), "the light side's numbers count whichever bearing is set");
            Assert.IsTrue(WayRules.Open(Upgrade.ById(BodyWay.Health1), new List<string>(), false), "the Body is open with no weapon owned");
        }

        [Test]
        public void TheNumbersSumToHealthOneThirtyFiveStaminaOneTwentyFiveAndFourFlasksHealingFiftyFive()
        {
            var numbers = Body.Where(mark => mark.Kind != UpgradeKind.Keystone).Select(mark => mark.Id);
            var stats = Summed(numbers);
            Assert.AreEqual(135f, 100f + stats.Sum(PlayerStat.Health), 1e-4f);
            Assert.AreEqual(125f, 100f + stats.Sum(PlayerStat.StaminaMost), 1e-4f);
            Assert.AreEqual(4, 3 + stats.Whole(PlayerStat.FlaskCount));
            Assert.AreEqual(55f, 45f * stats.Multiplier(PlayerStat.FlaskHeal), 1e-3f);
            Assert.AreEqual(6f, stats.Sum(PlayerStat.Poise), 1e-5f);
            Assert.AreEqual(0.6f, 0.8f + stats.Sum(PlayerStat.StaminaDelay), 1e-5f, "Quick Breath");
            Assert.AreEqual(0.6f, stats.Sum(PlayerStat.DodgeMetres), 1e-5f, "Long Step, at the same cost");
            Assert.AreEqual(0f, stats.Sum(PlayerStat.DodgeStamina) + stats.Sum(PlayerStat.StaminaCost));
            var enduring = Summed(new[] { BodyWay.EnduringBody });
            Assert.AreEqual(0.2f, 0.35f + enduring.Sum(PlayerStat.FlinchSeconds), 1e-5f);
            Assert.AreEqual(0.5f, enduring.Multiplier(PlayerStat.KnockbackTaken), 1e-5f);
            Assert.AreEqual(4f, 2.5f + enduring.Sum(PlayerStat.GreyDelay), 1e-5f);
            var light = Summed(new[] { BodyWay.LightBody });
            Assert.AreEqual(1, light.Whole(PlayerStat.DodgeFlow));
            Assert.AreEqual(-0.1f, light.Sum(PlayerStat.DodgeRecovery), 1e-5f);
            Assert.AreEqual(System.Enum.GetValues(typeof(PlayerStat)).Length, PlayerStats.Count, "the stat table holds every stat, the Body's new ones last");
            foreach (var stat in new[] { PlayerStat.StaminaMost, PlayerStat.Poise, PlayerStat.DodgeMetres, PlayerStat.GreyDelay })
                Assert.IsTrue(PlayerStats.IsFlat(stat), stat + " is a plain amount");
            Assert.IsFalse(PlayerStats.IsFlat(PlayerStat.KnockbackTaken));
        }

        [Test]
        public void SteadyHoldsSmallBlowsOutsideASwingAndAddsPoiseInOne()
        {
            Assert.IsTrue(BodyRules.ShrugsOff(false, BodyWay.SteadyPoise, 6f));
            Assert.IsFalse(BodyRules.ShrugsOff(false, BodyWay.SteadyPoise, 6.5f));
            Assert.IsFalse(BodyRules.ShrugsOff(false, 0f, 1f), "without Steady every landed blow flinches, as before");
            Assert.IsFalse(BodyRules.ShrugsOff(true, BodyWay.SteadyPoise, 3f), "in a swing the swing's poise decides");
            Assert.IsTrue(BodyRules.Poised(0f + BodyWay.SteadyPoise, 6f), "a swing with no poise of its own gets Steady's 6");
            Assert.IsFalse(BodyRules.Poised(0f, 1f), "and without it, none, as before");
        }

        [Test]
        public void TheLightBodyFlowsFromADodgesLastMomentAtHalfCostNeverAThird()
        {
            Assert.IsTrue(BodyRules.MayFlow(1, true, false, 0.35f, 0.30f, 0f));
            Assert.IsFalse(BodyRules.MayFlow(1, true, false, 0.35f, 0.20f, 0f), "too early");
            Assert.IsFalse(BodyRules.MayFlow(1, true, true, 0.35f, 0.30f, 0f), "never a third");
            Assert.IsFalse(BodyRules.MayFlow(0, true, false, 0.35f, 0.30f, 0f), "no Light Body");
            Assert.IsFalse(BodyRules.MayFlow(1, true, false, 0.35f, 0.30f, 0.1f), "pressed before the last moment, held in the buffer");

            var stamina = new StaminaPool();
            stamina.Fill();
            var roll = new DodgeRoll { Flows = 1, Recovery = 0f };
            Assert.IsTrue(roll.TryStart(Vector3.forward, stamina));
            float afterFirst = stamina.Current;
            roll.Tick(roll.Duration - 0.05f);
            Assert.IsTrue(roll.TryStart(Vector3.right, stamina, DodgeTerms.Plain, 0f), "flows on");
            Assert.AreEqual(roll.StaminaCost * BodyRules.FlowCostShare, afterFirst - stamina.Current, 1e-4f, "for half the stamina");
            Assert.IsTrue(roll.Flowed);
            Assert.AreEqual(1, roll.FlowsTaken);
            roll.Tick(roll.Duration - 0.05f);
            Assert.IsFalse(roll.TryStart(Vector3.forward, stamina, DodgeTerms.Plain, 0f), "never a third");

            var plain = new DodgeRoll();
            Assert.IsTrue(plain.TryStart(Vector3.forward, stamina));
            plain.Tick(plain.Duration - 0.05f);
            Assert.IsFalse(plain.TryStart(Vector3.forward, stamina, DodgeTerms.Plain, 0f), "without the Light Body a dodge waits, as before");
        }

        [Test]
        public void FoundPiecesStackOnTopAndThreeShardsMakeAFlask()
        {
            var found = new List<string> { "memory.fire.1", "evolution.x" };
            Assert.AreEqual("body.vessel.fire", BodyPieces.KeyOf(BodyPiece.Vessel, "fire"));
            Assert.AreEqual("", BodyPieces.KeyOf(BodyPiece.None, "fire"));
            Assert.IsTrue(BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.Vessel, "fire"), found));
            Assert.IsFalse(BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.Vessel, "fire"), found), "once");
            Assert.IsFalse(BodyPieces.Take("memory.fire.2", found), "not a piece");
            BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.Vessel, "fire.god"), found);
            BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.BreathStone, "storm"), found);
            BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.FlaskShard, "fire.god"), found);
            BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.FlaskShard, "storm"), found);
            var stats = new PlayerStats();
            foreach (var mod in BodyPieces.StatsOf(found)) stats.AddLearned(mod);
            Assert.AreEqual(10f, stats.Sum(PlayerStat.Health), 1e-5f, "two vessels");
            Assert.AreEqual(5f, stats.Sum(PlayerStat.StaminaMost), 1e-5f);
            Assert.AreEqual(0, stats.Whole(PlayerStat.FlaskCount), "two shards are not a flask yet");
            BodyPieces.Take(BodyPieces.KeyOf(BodyPiece.FlaskShard, "sea"), found);
            Assert.AreEqual(1, BodyPieces.StatsOf(found).Where(mod => mod.Stat == PlayerStat.FlaskCount).Sum(mod => (int)mod.Amount));
            Assert.AreEqual(3, BodyPieces.Count(BodyPiece.FlaskShard, found));
            CollectionAssert.IsEmpty(BodyPieces.StatsOf(new List<string>()));
            Assert.AreEqual("A vessel. More health.", BodyPieces.WordsOf(BodyPiece.Vessel));
        }

        [Test]
        public void TheBodySaysWhatItDoesInThePlansWords()
        {
            Assert.AreEqual("More health.", Upgrade.ById(BodyWay.Health1).Description);
            Assert.AreEqual("Carry one more flask.", Upgrade.ById(BodyWay.FourthFlask).Description);
            Assert.AreEqual("Small hits don't make you flinch, and your swings are harder to stop.", Upgrade.ById(BodyWay.Steady).Description);
            Assert.AreEqual("Dodge again just as a dodge ends to flow into a second one for half the stamina.", Upgrade.ById(BodyWay.LightBody).Description);
            foreach (var mark in Body)
                foreach (string sentence in mark.Description.Split('.'))
                    Assert.LessOrEqual(sentence.Split(' ').Length, 18, mark.Id + ": short sentences: " + sentence);
        }
    }
}
