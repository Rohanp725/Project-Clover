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
                Assert.Greater(upgrade.Cost, 0);
                Assert.AreSame(upgrade, Upgrade.ById(upgrade.Id));
            }
            Assert.AreEqual(7, Upgrade.All.Count(upgrade => upgrade.Element == ElementKit.FireName));
            Assert.AreEqual(7, Upgrade.All.Count(upgrade => upgrade.Element == ElementKit.LightningName));
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
