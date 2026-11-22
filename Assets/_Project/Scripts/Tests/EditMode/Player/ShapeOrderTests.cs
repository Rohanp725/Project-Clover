using System.Linq;
using Clover.Player;
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
