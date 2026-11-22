using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.Player.Way.Aspects;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Temper (steel/weapons.md 0.3; way_build_plan.md 2.1 and questions 4 and 5; task 5.3): five ranks bought in order for
    /// 1, 1, 2, 2 and 3 Memories, III, IV and V behind the third, fifth and seventh god outside the playtest, +6% damage and
    /// +5% stagger a rank (+30% and +25% at V); Temper III's longer drawing cut; Temper V's harder finisher and its held
    /// pose, 0.2 s on the plain Katana and 0.4 s at V. Nothing is drawn on the weapon: Temper is numbers only.
    /// </summary>
    public class TemperTests
    {
        private static readonly string[] Ranks = { KatanaWay.Temper1, KatanaWay.Temper2, KatanaWay.Temper3, KatanaWay.Temper4, KatanaWay.Temper5 };
        private static IReadOnlyList<Upgrade> All => Upgrade.All;

        [Test]
        public void TheRankIsHowManyAreLearnedAndThePricesAndGatesAreTheDesigns()
        {
            var learned = new HashSet<string>();
            Assert.AreEqual(0, WayRules.TemperRank(Weapons.KatanaName, learned, All));
            learned.Add(KatanaWay.Temper1);
            learned.Add(KatanaWay.Temper2);
            learned.Add(KatanaWay.Temper3);
            Assert.AreEqual(3, WayRules.TemperRank(Weapons.KatanaName, learned, All));
            Assert.AreEqual(0, WayRules.TemperRank(Weapons.ScytheName, learned, All), "a rank is its own weapon's");
            int[] prices = { 1, 1, 2, 2, 3 };
            string[] gates = { "", "", "gods.3", "gods.5", "gods.7" };
            for (int i = 0; i < Ranks.Length; i++)
            {
                var mark = Upgrade.ById(Ranks[i]);
                Assert.AreEqual(UpgradeKind.Temper, mark.Kind);
                Assert.AreEqual(Currency.Memories, mark.Currency);
                Assert.AreEqual(prices[i], mark.Cost, Ranks[i]);
                Assert.AreEqual(gates[i], mark.Gate ?? "", Ranks[i]);
                Assert.AreEqual(prices[i], Temper.PriceOf(i + 1));
            }
            Assert.AreEqual(9, prices.Sum(), "nine Memories for the five");
        }

        [Test]
        public void TheRanksAreBoughtInOrderAndTheirGatesHoldOutsideThePlaytest()
        {
            var wallet = new Wallet { Memories = 99 };
            var learned = new HashSet<string>();
            var second = Upgrade.ById(KatanaWay.Temper2);
            var third = Upgrade.ById(KatanaWay.Temper3);
            Assert.IsFalse(MarkRules.CanBuy(second, learned, wallet, true, false, null, 0), "II before I");
            learned.Add(KatanaWay.Temper1);
            Assert.IsTrue(MarkRules.CanBuy(second, learned, wallet, true, false, null, 0));
            learned.Add(KatanaWay.Temper2);
            Assert.IsFalse(MarkRules.CanBuy(third, learned, wallet, true, false, null, 2), "III waits for the third god");
            Assert.IsTrue(MarkRules.CanBuy(third, learned, wallet, true, false, null, 3));
            Assert.IsTrue(MarkRules.CanBuy(third, learned, wallet, true, true, null, 0), "the playtest opens every gate (5.1 Q2)");
            Assert.IsFalse(MarkRules.CanBuy(third, learned, new Wallet { Memories = 1 }, true, true, null, 0), "two Memories for III");
        }

        [Test]
        public void TheFiveSumToThirtyPercentDamageAndTwentyFivePercentStagger()
        {
            float damage = 0f, stagger = 0f;
            foreach (string id in Ranks)
                foreach (var mod in Upgrade.ById(id).Stats)
                {
                    if (mod.Stat == PlayerStat.WeaponDamage) damage += mod.Amount;
                    if (mod.Stat == PlayerStat.StaggerDealt) stagger += mod.Amount;
                }
            Assert.AreEqual(0.30f, damage, 1e-5f);
            Assert.AreEqual(0.25f, stagger, 1e-5f);
            // The drawing cut at ranks 0, 3 and 5: 10, 11.8 and 13 (the probe's numbers).
            float draw = Weapons.Katana().Combo.Steps[0].Damage;
            Assert.AreEqual(10f, draw * Temper.DamageShare(0), 1e-4f);
            Assert.AreEqual(11.8f, draw * Temper.DamageShare(3), 1e-4f);
            Assert.AreEqual(13f, draw * Temper.DamageShare(5), 1e-4f);
            Assert.AreEqual(1.25f, Temper.StaggerShare(5), 1e-5f);
            Assert.AreEqual(1.30f, Temper.DamageShare(9), 1e-5f, "no rank past V");
        }

        [Test]
        public void TheKatanasFinisherRunsThroughAndHoldsBehindTheEnemy()
        {
            // Question 5, the owner's answer: you stop behind the enemy with the blade out. From the finisher's stand-off
            // the run-through carries the player past the kneeling body, clear of it on the far side.
            var finisher = Weapons.Katana().Combo.Finisher;
            var combat = new UnityEngine.GameObject("Combat").AddComponent<PlayerCombat>();
            float standOff = combat.FinisherStandOff;
            UnityEngine.Object.DestroyImmediate(combat.gameObject);
            Assert.IsTrue(finisher.PassThrough, "it passes through the body");
            Assert.Greater(finisher.Travel - standOff, 2f, "and stands more than 2 m past its middle, where the camera sees it");
            Assert.AreEqual(Weapons.StormKatana().Combo.Finisher.Travel, finisher.Travel, 1e-5f, "the Storm Katana's the same");
        }

        [Test]
        public void TemperThreeReachesFurtherAndTemperFiveHitsHarderAndHoldsThePoseLonger()
        {
            var plain = Weapons.Katana();
            Assert.AreEqual(Temper.FinisherHold, plain.Combo.Finisher.HoldSeconds, 1e-5f, "the plain Katana's finisher holds its pose 0.2 s (question 5)");
            Assert.AreEqual(plain.Combo.Finisher.Windup + plain.Combo.Finisher.Active, plain.Combo.Finisher.HoldAt, 1e-5f, "the hold comes once the blow is in");
            var third = Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.Temper3));
            Assert.AreEqual(2.6f, third.Combo.Steps[0].Reach, 1e-5f, "the drawing cut at 2.6 m");
            Assert.AreEqual(plain.Combo.Steps[1].Reach, third.Combo.Steps[1].Reach, 1e-5f, "only the drawing cut");
            Assert.AreEqual(plain.Carrier.Reach + KatanaTrunk.TemperThreeReach, third.Carrier.Reach, 1e-5f, "and the carrier made of it");
            var fifth = Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.Temper5));
            Assert.AreEqual(plain.Combo.Finisher.Damage * 1.15f, fifth.Combo.Finisher.Damage, 1e-4f);
            Assert.AreEqual(Temper.FinisherHoldAtFive, fifth.Combo.Finisher.HoldSeconds, 1e-5f);
            Assert.AreEqual(0.4f, Temper.FinisherHoldAt(5), 1e-5f);
            Assert.AreEqual(0.2f, Temper.FinisherHoldAt(4), 1e-5f);
            Assert.AreEqual(plain.Combo.Finisher.Total, fifth.Combo.Finisher.Total, 1e-5f, "the finisher's own timings are its clip's: the hold is added by the engine");
        }

        [Test]
        public void TemperSaysWhatItDoesInShortPlainWords()
        {
            foreach (string id in Ranks)
            {
                string words = Upgrade.ById(id).Description;
                StringAssert.StartsWith("Every Katana hit does a little more damage", words, id);
                foreach (string sentence in words.Split('.'))
                    Assert.LessOrEqual(sentence.Split(' ').Length, 16, id + ": " + sentence);
            }
            StringAssert.Contains("reaches a little further", Upgrade.ById(KatanaWay.Temper3).Description);
            StringAssert.Contains("hold your last pose", Upgrade.ById(KatanaWay.Temper5).Description);
        }
    }
}
