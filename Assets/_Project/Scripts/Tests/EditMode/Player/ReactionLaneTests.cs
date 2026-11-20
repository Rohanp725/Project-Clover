using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The owner's words of 2026-11-20 on two reactions: The Bolt Runs True runs down a lane from the struck enemy away from
    /// the player, striking everyone on it with today's numbers; Flashover's flame is all violet, so the plain Burn is not
    /// drawn while its flame is live.
    /// </summary>
    public class ReactionLaneTests
    {
        private static Upgrade MarkOf(AfflictionKind one, AfflictionKind other) =>
            ReactionTable.MarkFor(AfflictionPairs.Index(one, other));

        [Test]
        public void TheBoltRunsTrueStrikesALaneAwayFromThePlayerWithTheStrikesNumbers()
        {
            var mark = MarkOf(AfflictionKind.Marked, AfflictionKind.Static);
            Assert.IsNotNull(mark);
            var lane = mark.Effects.Single(effect => effect.Do == UpgradeAction.Lane);
            Assert.AreEqual(UpgradeTrigger.ConditionEnded, lane.On, "the lane comes as the thread ends");
            Assert.AreEqual(Reactions.Thread, lane.Condition);
            Assert.AreEqual(8f, lane.Reach, "8 m long");
            Assert.AreEqual(0.6f, lane.Radius, "1.2 m wide");
            Assert.AreEqual(20f, lane.Damage, "the strike's damage to each on it");
            Assert.AreEqual(35f, lane.Stagger, "and its stagger");
            Assert.AreEqual(AfflictionKind.Static, lane.Affliction, "and its Static");
            Assert.AreEqual(AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Static), ReactionTable.PairOf(lane.Reaction), "its own look's bolt runs down it");
            Assert.IsFalse(mark.Effects.Any(effect => effect.Do == UpgradeAction.Burst), "no strike on the one enemy alone is left beside it");
            Assert.IsTrue(mark.Effects.Any(effect => effect.Do == UpgradeAction.SetOff && effect.Affliction == AfflictionKind.Static), "its own Static still discharges");
        }

        [Test]
        public void ALaneRunsFromTheTargetAwayWithNoGapABodyCouldStandIn()
        {
            Vector3 target = new Vector3(2f, 0f, 1f), away = Vector3.right;
            int patches = PlayerUpgrades.LanePatches(8f, 0.6f);
            Assert.AreEqual(target, PlayerUpgrades.LanePatch(target, away, 8f, 0, patches), "it starts on the one struck");
            Assert.AreEqual(target + away * 8f, PlayerUpgrades.LanePatch(target, away, 8f, patches - 1, patches), "and ends 8 m beyond it");
            for (int i = 1; i < patches; i++)
            {
                float gap = (PlayerUpgrades.LanePatch(target, away, 8f, i, patches) - PlayerUpgrades.LanePatch(target, away, 8f, i - 1, patches)).magnitude;
                // Where two circles of 0.6 m meet they still cover at least 0.4 m to either side of the line.
                Assert.LessOrEqual(Mathf.Sqrt(0.36f - gap * gap * 0.25f), 0.6f);
                Assert.GreaterOrEqual(Mathf.Sqrt(Mathf.Max(0f, 0.36f - gap * gap * 0.25f)), 0.4f, $"patches {gap:0.00} m apart leave the lane thin between them");
            }
        }

        [Test]
        public void FlashoversLiveFlameHidesThePlainBurn()
        {
            var mark = MarkOf(AfflictionKind.Burn, AfflictionKind.Static);
            var live = mark.Effects.Single(effect => effect.Do == UpgradeAction.Condition && effect.State != null && effect.State.Id == Reactions.LiveFlame);
            Assert.IsTrue((live.State.Flags & ConditionFlags.FlameLive) != 0, "while its flame is live the Burn is drawn only as the violet flame");
        }
    }
}
