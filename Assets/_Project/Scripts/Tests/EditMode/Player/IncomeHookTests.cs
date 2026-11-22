using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.UI;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// Memories paid in play (task 5.6; way_build_plan.md 2.6 and questions 10 and 11): an area's deaths, a god's fall and
    /// a cache found each pay once ever, into the wallet, under their key; a Proving Ground pen's enemies pay nothing; the
    /// Gate Captain pays as a new kind; and what the player sees (the light's flight and the "+N Memories" line) follows
    /// the owner's answer.
    /// </summary>
    public class IncomeHookTests
    {
        [Test]
        public void AnAreasDeathPaysTheFirstOfItsKindOnceIntoTheWallet()
        {
            var income = new Income();
            var paid = new List<string>();
            var wallet = new Wallet();
            Assert.AreEqual(1, income.PayKill(EnemyKind.Soldier(), true, paid, wallet));
            Assert.AreEqual(0, income.PayKill(EnemyKind.Soldier(), true, paid, wallet), "a second soldier");
            Assert.AreEqual(1, income.PayKill(EnemyKind.Acolyte(), true, paid, wallet));
            Assert.AreEqual(2, wallet.Memories);
            Assert.AreEqual(0, wallet.Shards, "Memories only: the area's Shards are paid as before");
        }

        [Test]
        public void APensEnemyPaysNothingAndLeavesItsKindUnpaid()
        {
            var income = new Income();
            var paid = new List<string>();
            var wallet = new Wallet();
            Assert.AreEqual(0, income.PayKill(EnemyKind.Champion(), false, paid, wallet), "let out of a Proving Ground pen");
            Assert.AreEqual(0, wallet.Memories);
            CollectionAssert.IsEmpty(paid, "the real one still pays when met in its land");
            Assert.AreEqual(4, income.PayKill(EnemyKind.Champion(), true, paid, wallet));
        }

        [Test]
        public void TheChampionPaysFourAndTheGateCaptainOneAsANewKind()
        {
            var income = new Income();
            var paid = new List<string>();
            var wallet = new Wallet();
            Assert.IsTrue(EnemyKind.Captain().IsElite, "still an elite in the fight");
            Assert.IsTrue(EnemyKind.Captain().PaysAsCommon);
            Assert.IsFalse(EnemyKind.Champion().PaysAsCommon);
            Assert.AreEqual(1, income.PayKill(EnemyKind.Captain(), true, paid, wallet));
            Assert.AreEqual(4, income.PayKill(EnemyKind.Champion(), true, paid, wallet));
            Assert.AreEqual(5, wallet.Memories);
        }

        [Test]
        public void AGodsBodiesPayNothingAndHisFallPaysEightOnce()
        {
            var income = new Income();
            var paid = new List<string>();
            var wallet = new Wallet();
            Assert.AreEqual(0, income.PayKill(EnemyKind.SunKing(), true, paid, wallet), "Khepharu as a man");
            Assert.AreEqual(0, income.PayKill(EnemyKind.Narukami(), true, paid, wallet));
            Assert.AreEqual(8, income.PayGodFall(Elements.Fire, paid, wallet), "the Sun-Bearer falls");
            Assert.AreEqual(0, income.PayGodFall(Elements.Fire, paid, wallet), "his fight again");
            Assert.AreEqual(8, income.PayGodFall(Elements.Lightning, paid, wallet));
            Assert.AreEqual(16, wallet.Memories);
        }

        [Test]
        public void ACacheFoundPaysOnceUnderItsKey()
        {
            var income = new Income();
            var found = new List<string>();
            var wallet = new Wallet();
            Assert.AreEqual(3, income.PayFound("memory.fire.1", false, found, wallet));
            Assert.AreEqual(0, income.PayFound("memory.fire.1", false, found, wallet));
            Assert.AreEqual(5, income.PayFound("memory.storm.1", true, found, wallet));
            Assert.AreEqual(8, wallet.Memories);
            Assert.AreEqual(0, income.PayFound("memory.fire.2", false, found, null), "no wallet, nothing noted as paid");
            CollectionAssert.DoesNotContain(found, "memory.fire.2");
        }

        [Test]
        public void TheLineSaysWhatWasPaidForTwoSecondsAndAddsWhatLandsMeanwhile()
        {
            Assert.AreEqual("+1 Memory", MemoryLine.Words(1));
            Assert.AreEqual("+4 Memories", MemoryLine.Words(4));
            var line = new MemoryLine();
            Assert.IsFalse(line.Visible);
            line.Add(0);
            Assert.IsFalse(line.Visible, "nothing paid, nothing shown");
            line.Add(1);
            Assert.IsTrue(line.Visible);
            Assert.AreEqual(1f, line.Alpha);
            line.Tick(1.5f);
            line.Add(4);
            Assert.AreEqual(5, line.Amount, "landed while it showed: added");
            line.Tick(1.9f);
            Assert.IsTrue(line.Visible, "two seconds from the last payment");
            Assert.Less(line.Alpha, 1f, "fading at the end");
            line.Tick(0.2f);
            Assert.IsFalse(line.Visible);
            line.Add(4);
            Assert.AreEqual(4, line.Amount, "a new line starts from nothing");
        }

        [Test]
        public void TheLightRisesOffTheBodyThenIsDrawnIntoTheChestInPointEightSeconds()
        {
            Assert.AreEqual(0.8f, MemoryFlight.Seconds, 1e-6f);
            var body = new Vector3(0f, 1f, 0f);
            var chest = new Vector3(6f, 1.2f, 0f);
            Assert.Less(Vector3.Distance(body, MemoryFlight.At(body, chest, 0f)), 1e-4f);
            Assert.Less(Vector3.Distance(chest, MemoryFlight.At(body, chest, 1f)), 1e-4f);
            var early = MemoryFlight.At(body, chest, 0.3f);
            Assert.Greater(early.y, body.y + 0.1f, "it rises first");
            Assert.Less(early.x, 1.5f, "still over the body");
            Assert.Greater(MemoryFlight.At(body, chest, 0.6f).y, chest.y, "it comes down into the chest from above");
            Assert.Less(MemoryFlight.Share(1f, 2.5f), 0f, "a god's light waits for the fall's moment");
            Assert.AreEqual(1f, MemoryFlight.Share(3.3f, 2.5f), 1e-5f);
            Assert.AreEqual(0.5f, MemoryFlight.Share(0.4f, 0f), 1e-5f);
        }

        [Test]
        public void TheLineUnderTheShardsIsDrawnWhateverItsBoxHeight()
        {
            // Filmed 2026-11-21: the line's box is the Shards count's (40 high) and the font's line at 28 is taller, so
            // a line cut to its box drew nothing at all.
            var canvas = new UnityEngine.GameObject("Canvas", typeof(UnityEngine.Canvas));
            var shardsObject = new UnityEngine.GameObject("Shards", typeof(UnityEngine.RectTransform));
            shardsObject.transform.SetParent(canvas.transform, false);
            var shards = shardsObject.AddComponent<UnityEngine.UI.Text>();
            shards.fontSize = 28;
            shards.rectTransform.sizeDelta = new UnityEngine.Vector2(400f, 40f);
            var gain = MemoryGain.Make(shards);
            var line = gain.GetComponent<UnityEngine.UI.Text>();
            Assert.AreEqual(UnityEngine.VerticalWrapMode.Overflow, line.verticalOverflow);
            Assert.AreEqual(UnityEngine.HorizontalWrapMode.Overflow, line.horizontalOverflow);
            Assert.Less(line.rectTransform.anchoredPosition.y, shards.rectTransform.anchoredPosition.y, "under the Shards count");
            UnityEngine.Object.DestroyImmediate(canvas);
        }

        [Test]
        public void TheLightIsAmberAndNeverWhite()
        {
            foreach (var colour in new[] { MemoryGain.Amber, MemoryGain.LineColour })
            {
                Assert.Greater(colour.r, colour.g);
                Assert.Greater(colour.g, colour.b);
                Assert.Less(colour.b, 0.6f, "warm, not white");
            }
        }
    }
}
