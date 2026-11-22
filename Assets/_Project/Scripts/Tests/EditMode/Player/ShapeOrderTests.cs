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
        public void ABaseSpellsMasterAnswersOnlyItsOwnElementsKitCast()
        {
            var fireMaster = Upgrade.ById(SpellBranch.MasterId(Clover.Player.Magic.SpellSlots.BaseId(Elements.Fire)));
            Assert.IsTrue(SpellBranches.OwnBranch(fireMaster, null, Elements.Fire), "Fire's own base spell");
            Assert.IsFalse(SpellBranches.OwnBranch(fireMaster, null, Elements.Water), "not Water's, with both carried");
            Assert.IsTrue(SpellBranches.OwnBranch(fireMaster, null, null), "an element not known: as before");
            foreach (var element in new[] { Elements.Fire, Elements.Lightning, Elements.Ice, Elements.Life, Elements.Light, Elements.Dark, Elements.Water, Elements.Wind, Elements.Earth })
            {
                var empower = Upgrade.ById(SpellBranch.EmpowerId(Clover.Player.Magic.SpellSlots.BaseId(element))).Empower;
                Assert.IsFalse(empower.NotForKit, element + "'s kit takes its Empower's numbers");
                var numbers = new SpellChange { Damage = empower.Damage, Stagger = empower.Stagger, Reach = empower.Reach, Radius = empower.Radius, Seconds = empower.Seconds, Stacks = empower.Stacks };
                Assert.IsFalse(numbers.IsEmpty, element + "'s Empower gives the kit more than its wait");
            }
            Assert.IsTrue(Upgrade.ById(SpellBranch.EmpowerId(MetalTree.FlungBlade)).Empower.NotForKit, "Flung Blade's 16 is the drive's, not the throw's");
        }

        [Test]
        public void ReturnStrokeChargesTheBladeForFiveBlows()
        {
            var stroke = Worn(Elements.Lightning, LightningTree.ChainLance, 1);
            Assert.AreEqual(5, stroke.ChargeBlows, "five blows, not three");
            Assert.AreEqual(0, Worn(Elements.Lightning, LightningTree.ChainLance, 0).ChargeBlows, "Stepped Leader charges as usual");
            var blade = new Clover.Player.Magic.ChargedBlade();
            blade.Charge(Elements.Lightning, stroke.ChargeBlows);
            Assert.AreEqual(5, blade.Left);
            blade.Charge(Elements.Lightning, 0);
            Assert.AreEqual(blade.Blows, blade.Left, "nothing said: the blade's own three");
        }

        [Test]
        public void DebtsCalledRisesSmoothlyAsYourHealthFalls()
        {
            Assert.AreEqual(1f, SpellSpec.HealthScale(2f, 1f), 1e-5f, "18 at full health");
            Assert.AreEqual(1.5f, SpellSpec.HealthScale(2f, 0.5f), 1e-5f, "27 at half");
            Assert.AreEqual(2f, SpellSpec.HealthScale(2f, 0f), 1e-5f, "36 at one point");
            Assert.AreEqual(1f, SpellSpec.HealthScale(0f, 0.2f), 1e-5f, "a spell that does not scale");
            var debts = Upgrade.ById(DarkTree.DebtsCalled);
            Assert.AreEqual(2f, debts.Spell.HealthScaled, 1e-5f);
            Assert.IsEmpty(debts.Effects, "no stand-in half again below half health");
            Assert.AreEqual(2f, Upgrade.ById(SpellBranch.ShapeId(DarkTree.DebtsCalled)).Shapes.First(shape => shape.Name == "The Black Buffalo").Spell.HealthScaled, 1e-5f);
        }

        [Test]
        public void FeedTheGroundTakesAQuarterOfYourPresentHealth()
        {
            Assert.AreEqual(25f, SpellSpec.HealthPrice(0f, 0.25f, 100f), 1e-5f);
            Assert.AreEqual(10f, SpellSpec.HealthPrice(0f, 0.25f, 40f), 1e-5f, "a quarter of what is left");
            Assert.AreEqual(25f, SpellSpec.HealthPrice(25f, 0f, 40f), 1e-5f, "a fixed price as before");
            var feed = Upgrade.ById(SpellBranch.ShapeId(LifeTree.DrawnFromTheWood)).Shapes.First(shape => shape.Name == "Feed the Ground").Spell;
            Assert.AreEqual(0.25f, feed.HealthShare, 1e-5f);
            Assert.AreEqual(0f, feed.HealthCost, 1e-5f, "no fixed 25");
        }

        [Test]
        public void StandingStillIsASecondWithoutMoving()
        {
            float still = 0f;
            for (int i = 0; i < 12; i++) still = StillRules.Next(still, 0f, 0.1f);
            Assert.GreaterOrEqual(still, StillRules.Seconds, "a second unmoved");
            Assert.AreEqual(0f, StillRules.Next(still, 0.2f * 0.2f, 0.1f), 1e-5f, "a step starts it over");
            Assert.Greater(StillRules.Next(0.5f, 0.01f * 0.01f, 0.1f), 0.5f, "a turn on the spot is still standing");
            var cairn = Upgrade.ById(SpellBranch.MasterId(EarthTree.Cairn)).Effects;
            Assert.IsTrue(cairn.All(effect => effect.While == StatWhen.StoodStill), "Cairn's Master: still for a second");
            var rise = Upgrade.ById(SpellBranch.MasterId(EarthTree.StoneRise)).Effects;
            Assert.IsTrue(rise.All(effect => effect.While == StatWhen.Planted && effect.While2 == StatWhen.StoodStill), "Stone Rise's Master: still as well as rooted");
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
