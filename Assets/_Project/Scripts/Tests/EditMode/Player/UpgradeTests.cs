using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The rules under the element trees: Heat, the depth of a burn and setting it off, and the catalogue itself.</summary>
    public class UpgradeTests
    {
        [Test]
        public void HeatRisesALevelForEveryFewBlowsAndStopsAtItsMaximum()
        {
            var heat = new Heat { MaximumLevel = 2, BlowsPerLevel = 3 };
            heat.Reset();
            Assert.IsFalse(heat.Landed());
            Assert.IsFalse(heat.Landed());
            Assert.IsTrue(heat.Landed());
            Assert.AreEqual(1, heat.Level);
            for (int i = 0; i < 10; i++) heat.Landed();
            Assert.AreEqual(2, heat.Level);
            Assert.AreEqual(1f + 2 * heat.DamagePerLevel, heat.DamageMultiplier, 0.0001f);
        }

        [Test]
        public void BeingStruckPutsHeatOutAndStandingOffCoolsItALevelAtATime()
        {
            var heat = new Heat { CoolAfter = 5f };
            heat.Fill();
            Assert.AreEqual(heat.MaximumLevel, heat.Level);
            heat.Tick(4.9f);
            Assert.AreEqual(heat.MaximumLevel, heat.Level);
            heat.Tick(0.2f);
            Assert.AreEqual(heat.MaximumLevel - 1, heat.Level);
            heat.Struck();
            Assert.AreEqual(0, heat.Level);
            Assert.AreEqual(1f, heat.DamageMultiplier, 0.0001f);
        }

        [Test]
        public void ADeeperBurnHurtsMoreAndItsDepthIsCapped()
        {
            var plain = new BurnStatus();
            plain.Ignite(4f);
            var deep = new BurnStatus();
            for (int i = 0; i < 9; i++) deep.Ignite(4f, 1);
            Assert.AreEqual(deep.MaximumStacks, deep.Stacks);
            Assert.Greater(deep.Tick(1f), plain.Tick(1f));
        }

        [Test]
        public void SettingABurnOffEndsItAndDealsMoreTheDeeperItWas()
        {
            var shallow = new BurnStatus();
            shallow.Ignite(4f, 1);
            var deep = new BurnStatus();
            for (int i = 0; i < 4; i++) deep.Ignite(4f, 1);
            float small = shallow.Detonate();
            float large = deep.Detonate();
            Assert.Greater(small, 0f);
            Assert.Greater(large, small);
            Assert.IsFalse(deep.IsBurning);
            Assert.AreEqual(0, deep.Stacks);
            Assert.AreEqual(0f, deep.Detonate());
        }

        [Test]
        public void ABurnThatRunsOutLosesItsDepth()
        {
            var burn = new BurnStatus();
            burn.Ignite(1f, 3);
            burn.Tick(2f);
            Assert.IsFalse(burn.IsBurning);
            Assert.AreEqual(0, burn.Stacks);
        }

        [Test]
        public void EveryUpgradeHasItsOwnIdAndBelongsToAnElementWithSevenInEachTree()
        {
            var ids = new HashSet<string>();
            foreach (var upgrade in Upgrade.All)
            {
                Assert.IsTrue(ids.Add(upgrade.Id), upgrade.Id);
                Assert.IsNotEmpty(upgrade.Name);
                Assert.IsNotEmpty(upgrade.Description);
                // What a god leaves is never for sale; everything else has a price.
                if (upgrade.Signature) Assert.AreEqual(0, upgrade.Cost);
                else Assert.Greater(upgrade.Cost, 0);
                Assert.AreSame(upgrade, Upgrade.ById(upgrade.Id));
            }
            Assert.AreEqual(7, Upgrade.All.Count(upgrade => upgrade.Element == ElementKit.FireName && !upgrade.Signature));
            Assert.AreEqual(7, Upgrade.All.Count(upgrade => upgrade.Element == ElementKit.LightningName && !upgrade.Signature));
        }

        [Test]
        public void EveryTreeIsARootWithBranchesThatAreLearnedFromTheFootOutward()
        {
            foreach (string element in UpgradeTree.Elements)
            {
                var tree = Upgrade.All.Where(upgrade => upgrade.Element == element && !upgrade.Signature).ToList();
                Assert.AreEqual(1, tree.Count(upgrade => upgrade.Requires == ""), element + " has one root");
                foreach (var upgrade in tree)
                {
                    if (upgrade.Requires == "") continue;
                    var parent = Upgrade.ById(upgrade.Requires);
                    Assert.IsNotNull(parent, upgrade.Id);
                    Assert.AreEqual(element, parent.Element, upgrade.Id);
                    // Outward costs more, and lies further along the trunk.
                    Assert.GreaterOrEqual(upgrade.Cost, parent.Cost, upgrade.Id);
                    Assert.Greater(upgrade.Y, parent.Y, upgrade.Id);
                }
                // No two marks on the same spot.
                foreach (var one in tree)
                    foreach (var other in tree)
                        if (one != other) Assert.Greater((UpgradeTree.OnChart(one) - UpgradeTree.OnChart(other)).magnitude, 0.9f, one.Id + " " + other.Id);
                // A path must be chosen: the castle and its god give nineteen embers, and the whole tree costs more.
                Assert.Greater(tree.Sum(upgrade => upgrade.Cost), 19, element);
            }
        }

        [Test]
        public void AnUpgradeCannotBeReachedBeforeWhatItGrowsFrom()
        {
            var learned = new HashSet<string>();
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Heat), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            learned.Add(Upgrade.Heat);
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(Upgrade.BurnAndDetonate), learned));
        }

        [Test]
        public void TheStickMovesTheChoiceToTheNearestMarkThatWayAndStaysPutWhenThereIsNone()
        {
            var places = new List<UnityEngine.Vector2>
            {
                new UnityEngine.Vector2(0f, 0f), new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(0f, 2f),
                new UnityEngine.Vector2(1.2f, 1f), new UnityEngine.Vector2(-1.2f, 0.9f),
            };
            Assert.AreEqual(1, UpgradeTree.Step(places, 0, UnityEngine.Vector2.up));
            Assert.AreEqual(2, UpgradeTree.Step(places, 1, UnityEngine.Vector2.up));
            Assert.AreEqual(3, UpgradeTree.Step(places, 1, UnityEngine.Vector2.right));
            Assert.AreEqual(4, UpgradeTree.Step(places, 1, UnityEngine.Vector2.left));
            Assert.AreEqual(2, UpgradeTree.Step(places, 2, UnityEngine.Vector2.up), "nothing lies beyond the top");
            Assert.AreEqual(0, UpgradeTree.Step(places, 0, UnityEngine.Vector2.down));
            Assert.AreEqual(0, UpgradeTree.Step(places, 0, UnityEngine.Vector2.zero));
        }

        [Test]
        public void EveryMarkOnATreeCanBeWalkedToFromItsRootWithTheStick()
        {
            var pushes = new[]
            {
                UnityEngine.Vector2.up, UnityEngine.Vector2.down, UnityEngine.Vector2.left, UnityEngine.Vector2.right,
                new UnityEngine.Vector2(1f, 1f), new UnityEngine.Vector2(-1f, 1f), new UnityEngine.Vector2(1f, -1f), new UnityEngine.Vector2(-1f, -1f),
            };
            foreach (string element in UpgradeTree.Elements)
            {
                var places = Upgrade.All.Where(upgrade => upgrade.Element == element).Select(UpgradeTree.OnChart).ToList();
                var reached = new HashSet<int> { 0 };
                var toVisit = new Queue<int>();
                toVisit.Enqueue(0);
                while (toVisit.Count > 0)
                {
                    int at = toVisit.Dequeue();
                    foreach (var push in pushes)
                    {
                        int next = UpgradeTree.Step(places, at, push);
                        if (reached.Add(next)) toVisit.Enqueue(next);
                    }
                }
                Assert.AreEqual(places.Count, reached.Count, element);
            }
        }

        [Test]
        public void AThrownFlaskIsUsedUpWithoutADrink()
        {
            var belt = new FlaskBelt { Maximum = 1 };
            belt.Refill();
            Assert.IsTrue(belt.TryThrow());
            Assert.AreEqual(0, belt.Count);
            Assert.IsFalse(belt.IsDrinking);
            Assert.IsFalse(belt.TryThrow());
        }
    }
}
