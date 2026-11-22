using System.Linq;
using Clover.Player;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Infusion (steel/weapons.md 0.4; way_build_plan.md 2.2 and question 6; task 5.3): each element is in one of four
    /// families; a family mark of the carried weapon's Way bends the carrier's copy for every element of its family and
    /// never the chain; the wrong family, or no mark, does nothing; all in, the deep strike doubles the family's length,
    /// time or pull; with Fire, Lying Draw keeps the whole cut alight for twice as long.
    /// </summary>
    public class InfusionTests
    {
        private static Weapon Katana => Weapons.Katana();

        private static (AttackStep carrier, InfusionBend bend) Bent(string mark, string element, bool deep = false, bool allIn = false, Weapon weapon = null)
        {
            weapon ??= Katana;
            var carrier = weapon.CarrierStep.Clone();
            var bend = Infusion.Bend(carrier, weapon, element, deep, allIn, new MarkSet(mark));
            return (carrier, bend);
        }

        [Test]
        public void EveryElementIsInOneFamily()
        {
            string[] all = { Elements.Fire, Elements.Lightning, Elements.Wind, Elements.Ice, Elements.Water, Elements.Life, Elements.Earth, Elements.Metal, Elements.Light, Elements.Dark, Elements.Poison };
            Assert.AreEqual(Elements.Count, all.Length);
            Assert.AreEqual(3, all.Count(e => Infusion.FamilyOf(e) == InfusionFamily.Ground));
            Assert.AreEqual(3, all.Count(e => Infusion.FamilyOf(e) == InfusionFamily.Line));
            Assert.AreEqual(3, all.Count(e => Infusion.FamilyOf(e) == InfusionFamily.Weight));
            Assert.AreEqual(2, all.Count(e => Infusion.FamilyOf(e) == InfusionFamily.Draw));
            Assert.AreEqual(InfusionFamily.Ground, Infusion.FamilyOf(Elements.Poison));
            Assert.AreEqual(InfusionFamily.Line, Infusion.FamilyOf(Elements.Wind));
            Assert.AreEqual(InfusionFamily.Weight, Infusion.FamilyOf(Elements.Metal));
            Assert.AreEqual(InfusionFamily.Draw, Infusion.FamilyOf(Elements.Dark));
            Assert.AreEqual(InfusionFamily.None, Infusion.FamilyOf(""));
        }

        [Test]
        public void NoMarkOrTheWrongFamilyOrAnotherWeaponBendsNothing()
        {
            var plain = Katana.CarrierStep;
            foreach (var (mark, element, weapon) in new[] { ((string)null, Elements.Lightning, Katana), (KatanaWay.InfuseLine, Elements.Fire, Katana), (KatanaWay.InfuseLine, Elements.Lightning, Weapons.Scythe()) })
            {
                var (carrier, bend) = Bent(mark, element, weapon: weapon);
                Assert.AreEqual(InfusionFamily.None, bend.Family, mark + " " + element);
                Assert.IsNull(bend.Mark);
                if (weapon.Name != Weapons.KatanaName) continue;
                Assert.AreEqual(plain.Travel, carrier.Travel);
                Assert.AreEqual(plain.Windup, carrier.Windup);
                Assert.AreEqual(plain.Stagger, carrier.Stagger);
            }
        }

        [Test]
        public void RunningDrawIsAFiveMetreDashThroughTheLaneAndTenAllIn()
        {
            var (carrier, bend) = Bent(KatanaWay.InfuseLine, Elements.Wind);
            Assert.AreEqual(InfusionFamily.Line, bend.Family);
            Assert.AreEqual(5f, carrier.Travel, 1e-5f);
            Assert.IsTrue(carrier.PassThrough);
            Assert.AreEqual(10f, Bent(KatanaWay.InfuseLine, Elements.Wind, true, true).carrier.Travel, 1e-5f, "the deep strike all in");
            Assert.AreEqual(5f, Bent(KatanaWay.InfuseLine, Elements.Wind, true, false).carrier.Travel, 1e-5f, "deep with one element: not all in");
            Assert.AreEqual(5f, Bent(KatanaWay.InfuseLine, Elements.Wind, false, true).carrier.Travel, 1e-5f, "all in, but R1");
        }

        [Test]
        public void SettlingDrawIsSlowerUnbrokenAndStaggersSixtyPercentMore()
        {
            var plain = Katana.CarrierStep;
            var (carrier, bend) = Bent(KatanaWay.InfuseWeight, Elements.Ice);
            Assert.AreEqual(InfusionFamily.Weight, bend.Family);
            Assert.AreEqual(0.32f, carrier.Windup, 1e-5f);
            Assert.AreEqual(32f, carrier.Stagger, 1e-4f, "20 and 60% more");
            Assert.Greater(carrier.Poise, 1000f, "nothing breaks it");
            Assert.AreEqual(carrier.Windup / plain.Windup, carrier.Active / plain.Active, 1e-4f, "slowed by one share, so the blow keeps its frame");
            Assert.IsTrue(bend.StopsInFirst);
            Assert.AreEqual(plain.Damage, carrier.Damage, 1e-5f);
        }

        [Test]
        public void LyingDrawLiesThreeSecondsAndWithFireTheWholeCutTwiceAsLong()
        {
            var poison = Bent(KatanaWay.InfuseGround, Elements.Poison).bend;
            Assert.AreEqual(3f, poison.LieSeconds, 1e-5f);
            Assert.IsFalse(poison.WholeCut);
            Assert.AreEqual(6f, Bent(KatanaWay.InfuseGround, Elements.Life, true, true).bend.LieSeconds, 1e-5f, "doubled all in");
            var fire = Bent(KatanaWay.InfuseGround, Elements.Fire).bend;
            Assert.IsTrue(fire.WholeCut, "question 6: the whole cut alight");
            Assert.AreEqual(6f, fire.LieSeconds, 1e-5f, "and for twice as long");
        }

        [Test]
        public void DraggingDrawFollowsWithTheReturnCutThatDragsOneAndAHalfMetres()
        {
            var weapon = Katana;
            var (carrier, bend) = Bent(KatanaWay.InfuseDraw, Elements.Water, weapon: weapon);
            Assert.AreEqual(InfusionFamily.Draw, bend.Family);
            Assert.IsNotNull(bend.Follow);
            Assert.AreEqual(weapon.Combo.Steps[1].Name, bend.Follow.Name, "the return cut");
            Assert.AreEqual(11f, bend.Follow.Damage, 1e-5f);
            Assert.AreEqual(10f, bend.Follow.Stagger, 1e-5f);
            Assert.AreEqual(1.5f, bend.Follow.Pull, 1e-5f);
            Assert.AreEqual(0.15f, bend.FollowDelay, 1e-5f);
            Assert.AreEqual(0f, weapon.Combo.Steps[1].Pull, "the chain's own return cut is not touched");
            Assert.AreEqual(3f, Bent(KatanaWay.InfuseDraw, Elements.Dark, true, true).bend.Drag, 1e-5f);
        }

        [Test]
        public void TheBendNeverReachesTheWeapon()
        {
            var weapon = Katana;
            var before = weapon.CarrierStep.Windup;
            var carrier = weapon.CarrierStep.Clone();
            Infusion.Bend(carrier, weapon, Elements.Earth, true, true, new MarkSet(KatanaWay.InfuseWeight));
            Assert.AreEqual(before, weapon.CarrierStep.Windup);
            Assert.AreEqual(0f, weapon.CarrierStep.Poise);
            Assert.AreEqual(KatanaWay.InfuseGround, Infusion.MarkOf(WayRules.KindOf(Weapons.StormKatanaName), InfusionFamily.Ground), "the Storm Katana is a Katana");
        }
    }
}
