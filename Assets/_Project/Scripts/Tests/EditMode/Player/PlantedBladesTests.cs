using System.Linq;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>Blades of yours standing in enemies and the ground (standin_clearance_plan.md pass 10: planted-blades, blades-home).</summary>
    public class PlantedBladesTests
    {
        [SetUp]
        public void Begin() => PlantedBlades.Clear();

        [TearDown]
        public void End() => PlantedBlades.Clear();

        private static BladeOrders Orders(PlantedKind kind = PlantedKind.Flung, bool empowered = false, bool neverFades = false)
        {
            var orders = PlayerUpgrades.OrdersOf(null, empowered, neverFades);
            orders.Kind = kind;
            return orders;
        }

        [Test]
        public void ALightBlowDrivesTheOldestAndTheEmpowersHeavyBlowDrivesThemAll()
        {
            var enemy = new object();
            int first = PlantedBlades.Plant(Orders(), enemy, Vector3.zero, 0f);
            int second = PlantedBlades.Plant(Orders(), enemy, Vector3.zero, 1f);
            Assert.IsFalse(PlantedBlades.Standing(first), "one blade in one enemy: the new one sends the old one home");
            Assert.IsTrue(PlantedBlades.Standing(second));
            PlantedBlades.Clear();

            var strong = Orders(empowered: true);
            Assert.AreEqual(3, strong.MostInOne, "the Empower: three in one enemy");
            Assert.AreEqual(16f, strong.Drive, 1e-5f);
            int a = PlantedBlades.Plant(strong, enemy, Vector3.zero, 0f);
            int b = PlantedBlades.Plant(strong, enemy, Vector3.zero, 1f);
            int c = PlantedBlades.Plant(strong, enemy, Vector3.zero, 2f);
            Assert.AreEqual(3, PlantedBlades.CountIn(enemy));
            Assert.AreEqual(1, PlantedBlades.WeaponBlow(enemy, false), "a light blow drives one");
            Assert.IsTrue(PlantedBlades.TakeDrive(a, out var driven), "the oldest");
            Assert.IsFalse(PlantedBlades.TakeDrive(a, out _), "a drive is taken once");
            Assert.AreEqual(16f, BladeRules.DriveDamage(driven, 12f, 0.5f), 1e-4f, "the Empower's 16 against the kit's 12");
            // Its form takes a driven blade home, out of the list.
            PlantedBlades.Remove(a);
            Assert.AreEqual(2, PlantedBlades.WeaponBlow(enemy, true), "a heavy blow drives the rest");
            Assert.IsTrue(PlantedBlades.TakeDrive(b, out _) && PlantedBlades.TakeDrive(c, out _));
            Assert.AreEqual(0, PlantedBlades.WeaponBlow(new object(), true), "a blow on another enemy drives nothing");
        }

        [Test]
        public void TheNailAnswersHeavyBlowsOnlyAndGoesDeeperEachTime()
        {
            var enemy = new object();
            int nail = PlantedBlades.Plant(Orders(PlantedKind.Nail), enemy, Vector3.zero, 0f);
            Assert.AreEqual(0, PlantedBlades.WeaponBlow(enemy, false), "a light blow does not drive the nail");
            float[] expected = { 12f, 16f, 24f };
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(1, PlantedBlades.WeaponBlow(enemy, true));
                Assert.IsTrue(PlantedBlades.TakeDrive(nail, out var blade));
                Assert.AreEqual(i + 1, blade.Drives);
                Assert.AreEqual(expected[i], BladeRules.DriveDamage(blade, 12f, 1f), 1e-4f);
                Assert.AreEqual(i == 2, BladeRules.SpentByDrive(blade), "it stays until its third");
            }
            Assert.AreEqual(3, BladeRules.NailKneelsAt);
        }

        [Test]
        public void ACherryRedBladeStrikesDoubleWhileStillRed()
        {
            var enemy = new object();
            int red = PlantedBlades.Plant(Orders(PlantedKind.CherryRed), enemy, Vector3.zero, 10f);
            PlantedBlades.Drive(red);
            Assert.IsTrue(PlantedBlades.TakeDrive(red, out var blade));
            Assert.AreEqual(24f, BladeRules.DriveDamage(blade, 12f, 11.5f), 1e-4f, "within two seconds: double");
            Assert.AreEqual(12f, BladeRules.DriveDamage(blade, 12f, 12.5f), 1e-4f, "after: plain");
            Assert.AreEqual(1f, BladeRules.Glow(10f, 10f), 1e-5f);
            Assert.AreEqual(0.5f, BladeRules.Glow(10f, 13f), 1e-5f);
            Assert.AreEqual(0f, BladeRules.Glow(10f, 17f), 1e-5f, "cooled to black by six seconds");
        }

        [Test]
        public void ToTheBladeFindsTheNearestBladeAheadFirst()
        {
            int behind = PlantedBlades.Plant(Orders(), null, new Vector3(0f, 0f, -3f), 0f);
            int ahead = PlantedBlades.Plant(Orders(), null, new Vector3(0f, 0f, 8f), 0f);
            PlantedBlades.Plant(Orders(), null, new Vector3(0f, 0f, 20f), 0f);
            Assert.AreEqual(ahead, PlantedBlades.Nearest(Vector3.zero, Vector3.forward, 12f), "ahead, though farther than the one behind");
            Assert.AreEqual(behind, PlantedBlades.Nearest(Vector3.zero, Vector3.back, 12f));
            PlantedBlades.Remove(ahead);
            PlantedBlades.Remove(behind);
            Assert.AreEqual(-1, PlantedBlades.Nearest(Vector3.zero, Vector3.forward, 12f), "twenty metres is out of reach");
            Assert.IsFalse(PlantedBlades.Standing(ahead), "a removed blade's handle names nothing");
            var learn = Upgrade.ById(MetalTree.ToTheBlade);
            Assert.AreEqual(12f, learn.Spell.BladeReach, 1e-5f);
            var spec = new SpellSpec();
            SpellChange.Copy(learn.Spell, spec);
            Upgrade.ById(SpellBranch.EmpowerId(MetalTree.ToTheBlade)).Empower.ApplyTo(spec);
            Assert.AreEqual(14f, spec.BladeReach, 1e-5f, "the Empower's fourteen");
        }

        [Test]
        public void AllBladesHomeKeepsSixteenAndCallsThemHome()
        {
            var orders = Orders(neverFades: true);
            Assert.IsTrue(orders.NeverFades);
            int first = PlantedBlades.Plant(orders, null, Vector3.zero, 0f);
            for (int i = 1; i < BladeRules.MostStanding; i++) PlantedBlades.Plant(orders, null, Vector3.right * i, i);
            Assert.AreEqual(16, PlantedBlades.Count());
            PlantedBlades.Plant(orders, null, Vector3.forward, 100f);
            Assert.IsFalse(PlantedBlades.Standing(first), "the seventeenth takes the oldest's place, and its form flies it home");
            var places = new Vector3[PlantedBlades.Most];
            Assert.AreEqual(16, PlantedBlades.RecallAll(places), "every standing blade");
            Assert.IsTrue(places.Contains(Vector3.forward), "each called from where it stood");
            Assert.AreEqual(0, PlantedBlades.Count());
            var call = Upgrade.ById(MetalTree.AllBladesHome).Effects.Single();
            Assert.AreEqual(UpgradeAction.CallBladesHome, call.Do);
            Assert.AreEqual(UpgradeTrigger.SurgeStart, call.On);
            Assert.AreEqual(12f, call.Damage, 1e-5f);
            Assert.AreEqual(8f, call.Stagger, 1e-5f);
            Assert.AreEqual(0.4f, call.Seconds, 1e-5f, "each blade home adds 0.4 s to the surge");
        }

        [Test]
        public void FlungBladesShapesAndMasterUseThePlantedBlades()
        {
            var shapes = Upgrade.ById(SpellBranch.ShapeId(MetalTree.FlungBlade)).Shapes;
            var red = shapes.Single(each => each.Name == MetalTree.CherryRedName);
            Assert.AreEqual(PlantedKind.CherryRed, PlayerUpgrades.OrdersOf(red, false, false).Kind);
            var fan = red.Effects.Single();
            Assert.AreEqual(UpgradeAction.PlantBlades, fan.Do);
            Assert.AreEqual(2, fan.Count, "two more, three in all");
            Assert.AreEqual(9f, fan.Reach, 1e-5f, "within the kit's nine metres");
            Assert.AreEqual(6f, fan.Spell.Seconds, 1e-5f, "standing six seconds as they cool");
            var nail = shapes.Single(each => each.Name == MetalTree.NailName);
            Assert.AreEqual(PlantedKind.Nail, PlayerUpgrades.OrdersOf(nail, false, false).Kind);
            Assert.IsTrue(nail.Effects.All(each => each.On == UpgradeTrigger.BladeDriven && each.SpentAtLeast == 3), "its third drive, heavy blows only");
            Assert.IsTrue(nail.Effects.Any(each => each.Do == UpgradeAction.Condition && each.State.Id == MetalTree.Kneeling));
            var master = Upgrade.ById(SpellBranch.MasterId(MetalTree.FlungBlade)).Effects;
            Assert.IsTrue(master.Any(each => each.On == UpgradeTrigger.PerfectGuard && each.Do == UpgradeAction.DriveBlades), "only the blades in the attacker");
            Assert.IsFalse(master.Any(each => each.Do == UpgradeAction.Burst), "no stand-in blade's worth");
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.BladeDriven));
        }
    }
}
