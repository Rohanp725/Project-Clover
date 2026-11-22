using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Shape and Empower order (standin_clearance_plan.md pass 5): a worn shape that sets aside an effect of its Learn.
    /// </summary>
    public class ShapeOrderTests
    {
        private static void SetsAside(string branch, string shapeName, UpgradeAction what)
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(branch)).Shapes.First(each => each.Name == shapeName);
            var learn = Upgrade.ById(branch).Effects;
            Assert.IsNotEmpty(shape.SetsAside, shapeName + " sets something aside");
            foreach (int index in shape.SetsAside)
            {
                Assert.That(index, Is.InRange(0, learn.Length - 1), shapeName + " names a Learn effect that is there");
                Assert.AreEqual(what, learn[index].Do, shapeName + " sets aside what it says");
                Assert.IsTrue(ShapeDef.SetAside(shape, index));
            }
        }

        [Test]
        public void AWornShapeSetsAsideWhatItSays()
        {
            SetsAside(MetalTree.PouredBronze, "Red-Hot", UpgradeAction.Hold);
            SetsAside(PoisonTree.OneByOne, "Eaten in Turn", UpgradeAction.SetOff);
            SetsAside(LifeTree.ReapWhatIsSown, "Golden Sickle", UpgradeAction.BurstGrown);
            SetsAside(LifeTree.DrawnFromTheWood, "Feed the Ground", UpgradeAction.BurstGrown);
            Assert.IsFalse(ShapeDef.SetAside(null, 0), "no shape worn: everything runs");
            Assert.IsFalse(ShapeDef.SetAside(new ShapeDef(), 0));
        }

        private static SpellSpec Worn(string element, string branch, int shape)
        {
            var learned = new HashSet<string> { branch, SpellBranch.EmpowerId(branch), SpellBranch.ShapeId(branch) };
            var loadout = new Loadout();
            loadout.Attune(element, "");
            loadout.Slots[0] = branch;
            loadout.Shapes.Add(new IdChoice(branch, shape));
            return new SpellBranches().SpecOf(Upgrade.ById(branch), learned, loadout);
        }

        [Test]
        public void AnEmpowerMeetsAShapesNumbersAsTheShapeSays()
        {
            var fog = Worn(Elements.Ice, IceTree.RimeGuard, 0);
            Assert.AreEqual(2.5f, fog.Radius, 1e-4f, "Freezing Fog keeps its own 2.5 m under the Empower's 4");
            Assert.AreEqual(7f, fog.Cooldown, 1e-4f, "and still takes the Empower's shorter wait");
            var tide = Worn(Elements.Water, WaterTree.TheTideTurns, 0);
            Assert.AreEqual(5f, tide.Reach, 1e-4f, "King Tide stops where you began");
            var circle = Worn(Elements.Wind, WindTree.ClosingCircle, 1);
            Assert.AreEqual(0.6f + (1.5f - 1.2f), circle.Seconds, 1e-4f, "Wide Circle's short hang made longer by the Empower's rise");
            var change = new SpellChange { Radius = 5f, Cooldown = 3f };
            var spec = new SpellSpec { Radius = 2f };
            change.AddOver(spec, new SpellSpec { Radius = 4f });
            Assert.AreEqual(3f, spec.Radius, 1e-5f, "a metre more, as the Empower gives the Learn");
            Assert.AreEqual(3f, spec.Cooldown, 1e-5f, "its wait set as written");
        }

        [Test]
        public void EveryShapesSetAsideNamesARealEffect()
        {
            foreach (var mark in Upgrade.All.Where(mark => mark.Part == BranchPart.Shape && mark.Shapes != null))
            {
                var learn = Upgrade.ById(mark.Branch);
                if (learn == null) continue;
                foreach (var shape in mark.Shapes.Where(shape => shape != null && shape.SetsAside != null))
                    foreach (int index in shape.SetsAside)
                        Assert.That(index, Is.InRange(0, learn.Effects.Length - 1), mark.Id + "'s " + shape.Name);
            }
        }
    }
}
