using Clover.Player;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// What the infusion families leave behind and do after the blow (way_build_plan.md 2.2 and question 6; the owner's
    /// answer on Running Draw with Lightning, 2026-11-21): the charged path, Lying Draw's whole cut on the floor, Settling
    /// Draw stopping in the first enemy, Dragging Draw's free return cut, and the finisher's held pose.
    /// </summary>
    public class InfusionGroundTests
    {
        private static InfusionBend Bend(string mark, string element, bool deep = false, bool allIn = false)
        {
            var weapon = Weapons.Katana();
            var carrier = weapon.CarrierStep.Clone();
            return Infusion.Bend(carrier, weapon, element, deep, allIn, new MarkSet(mark));
        }

        [Test]
        public void RunningDrawWithLightningLeavesAChargedPathForTwoSeconds()
        {
            Assert.AreEqual(2f, Bend(KatanaWay.InfuseLine, Elements.Lightning).ChargedPath, 1e-5f);
            Assert.AreEqual(2f, Bend(KatanaWay.InfuseLine, Elements.Lightning, true, true).ChargedPath, 1e-5f, "the dash doubles all in, not the charge");
            Assert.AreEqual(0f, Bend(KatanaWay.InfuseLine, Elements.Wind).ChargedPath, "only Lightning's path is charged");
            Assert.AreEqual(0f, Bend(KatanaWay.InfuseLine, Elements.Light).ChargedPath);
            Assert.AreEqual(0f, Bend(null, Elements.Lightning).ChargedPath, "Flash Cut alone leaves nothing");
            Assert.AreEqual(1, InfusionGround.ChargedStacks);
        }

        [Test]
        public void TheWholeCutLiesOnTheFloorPatchByPatchWithNoGaps()
        {
            var print = new Footprint { Shape = FootprintShape.Arc, Origin = Vector3.zero, Forward = Vector3.forward, Reach = 2.6f, ArcDegrees = 170f };
            var into = new Vector3[InfusionGround.MostPatches];
            int count = InfusionGround.ArcPatches(print, into);
            Assert.GreaterOrEqual(count, 4, "more than the strike's own four patches: the whole cut");
            float radius = 2.6f * 0.85f;
            for (int i = 0; i < count; i++)
                Assert.AreEqual(radius, into[i].magnitude, 1e-3f, "on the blade's sweep");
            for (int i = 1; i < count; i++)
                Assert.LessOrEqual(Vector3.Distance(into[i - 1], into[i]), InfusionGround.PatchRadius * 2f, "neighbours touch or overlap");
            Assert.AreEqual(85f, Vector3.Angle(Vector3.forward, into[0]), 0.5f, "from one end of the arc");
            Assert.AreEqual(85f, Vector3.Angle(Vector3.forward, into[count - 1]), 0.5f, "to the other");

            int lane = InfusionGround.LanePatches(Vector3.zero, new Vector3(0f, 0f, 7.4f), into);
            Assert.AreEqual(Vector3.zero, into[0]);
            Assert.Less(Vector3.Distance(new Vector3(0f, 0f, 7.4f), into[lane - 1]), 1e-4f);
            for (int i = 1; i < lane; i++)
                Assert.LessOrEqual(Vector3.Distance(into[i - 1], into[i]), InfusionGround.Spacing + 1e-4f);
            Assert.AreEqual(0, InfusionGround.LanePatches(Vector3.zero, Vector3.forward, new Vector3[0]));
        }

        [Test]
        public void LyingDrawsGroundIsTheElementsAndFireBurnsTheWholeCutForSixSeconds()
        {
            var fire = Bend(KatanaWay.InfuseGround, Elements.Fire);
            Assert.IsTrue(fire.WholeCut);
            Assert.AreEqual(6f, fire.LieSeconds, 1e-5f);
            var poison = Bend(KatanaWay.InfuseGround, Elements.Poison);
            Assert.IsFalse(poison.WholeCut);
            Assert.AreEqual(3f, poison.LieSeconds, 1e-5f);
            Assert.AreEqual(0f, Bend(KatanaWay.InfuseGround, Elements.Water).LieSeconds, "Water is not of the ground");
        }

        [Test]
        public void SettlingDrawStopsInTheFirstEnemyAndSetsTheElementInIt()
        {
            var weapon = Weapons.Katana();
            var carrier = weapon.CarrierStep.Clone();
            Infusion.Bend(carrier, weapon, Elements.Metal, false, false, new MarkSet(KatanaWay.InfuseWeight));
            Assert.IsTrue(carrier.FirstOnly);
            Assert.AreEqual(weapon.CarrierStep.ElementStacks + Infusion.SettleStacks, carrier.ElementStacks);
            Assert.IsFalse(weapon.CarrierStep.FirstOnly, "never the weapon's own");
            foreach (var step in weapon.AllSteps()) Assert.IsFalse(step.FirstOnly, step.Name);
        }

        [Test]
        public void DraggingDrawsReturnCutComesByItselfAndCostsNothingMore()
        {
            var bend = Bend(KatanaWay.InfuseDraw, Elements.Water);
            Assert.AreEqual(0f, bend.Follow.StaminaCost);
            Assert.AreEqual(Infusion.FollowDelay, bend.FollowDelay, 1e-5f);
        }

        [Test]
        public void TheFinishersPoseHoldsOnceAfterTheBlow()
        {
            var finisher = Weapons.Katana().Combo.Finisher;
            Assert.IsFalse(Temper.HoldBegins(finisher, true, finisher.HoldAt - 0.01f, false), "the blow is not in yet");
            Assert.IsTrue(Temper.HoldBegins(finisher, true, finisher.HoldAt, false));
            Assert.IsFalse(Temper.HoldBegins(finisher, true, finisher.HoldAt + 0.1f, true), "once");
            Assert.IsFalse(Temper.HoldBegins(finisher, false, finisher.HoldAt, false), "only as a finisher");
            Assert.IsFalse(Temper.HoldBegins(Weapons.Scythe().Combo.Finisher, true, 5f, false), "a finisher without a hold of its own plays as before");
            var fifth = Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.Temper5)).Combo.Finisher;
            Assert.AreEqual(0.4f, fifth.PoseHold, 1e-5f);
            Assert.AreEqual(0.2f, finisher.PoseHold, 1e-5f);
        }
    }
}
