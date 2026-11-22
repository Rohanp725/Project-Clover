using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Forms;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// A form's lasting things timed by the spell (standin_clearance_plan.md pass 5, the ledger's form-times): a crater's burn,
    /// a column's hang, a crack's glow, spears in turn, a ring that stands its time and takes its drums with it.
    /// </summary>
    public class FormTimesTests
    {
        // The spec as cast: the Learn, with its Empower when learned, wearing the shape given (-1 for none).
        private static SpellSpec Cast(string element, string branch, bool empowered, int shape = -1)
        {
            // The Shape mark only with a shape chosen: learned, it wears its first shape by default.
            var learned = new HashSet<string> { branch };
            if (shape >= 0) learned.Add(SpellBranch.ShapeId(branch));
            if (empowered) learned.Add(SpellBranch.EmpowerId(branch));
            var loadout = new Loadout();
            loadout.Attune(element, "");
            loadout.Slots[0] = branch;
            if (shape >= 0) loadout.Shapes.Add(new IdChoice(branch, shape));
            return new SpellBranches().SpecOf(Upgrade.ById(branch), learned, loadout);
        }

        [Test]
        public void AFormTakesTheSpellsOwnTimeWhereItSetsOne()
        {
            Assert.AreEqual(6f, FormRules.Own(6f, 4f), 1e-5f);
            Assert.AreEqual(4f, FormRules.Own(0f, 4f), 1e-5f, "no time set: the form's own");
            Assert.AreEqual(0.17f, FormRules.BreakLead(8f, 20f, 0f, 0.17f), 1e-5f, "no glow: the break just behind the mark");
            Assert.AreEqual(0.4f + 0.9f, FormRules.BreakLead(8f, 20f, 0.9f, 0.17f), 1e-5f, "the whole crack drawn, then glowing 0.9 s");
        }

        [Test]
        public void MeteorLeapsCraterBurnsLongerWithItsEmpower()
        {
            Assert.AreEqual(4f, Cast(Elements.Fire, Upgrade.MeteorLeap, false).Lasts, 1e-5f);
            Assert.AreEqual(6f, Cast(Elements.Fire, Upgrade.MeteorLeap, true).Lasts, 1e-5f, "6 s with the Empower");
        }

        [Test]
        public void LiftingWindHangsItsEnemyAsItSays()
        {
            Assert.AreEqual(1f, Cast(Elements.Wind, WindTree.LiftingWind, false).Lasts, 1e-5f, "a second, not the pillars' own 0.8");
            Assert.AreEqual(1.3f, Cast(Elements.Wind, WindTree.LiftingWind, true).Lasts, 1e-5f);
        }

        [Test]
        public void FissureGlowsAndInTurnsSpearsFallAQuarterSecondApart()
        {
            Assert.AreEqual(0.9f, Cast(Elements.Earth, EarthTree.Fissure, false).Glow, 1e-5f);
            int inTurn = Upgrade.ById(SpellBranch.ShapeId(LightTree.RingOfSpears)).Shapes.ToList().FindIndex(each => each.Name == "In Turn");
            Assert.That(inTurn, Is.GreaterThanOrEqualTo(0));
            Assert.AreEqual(0.25f, Cast(Elements.Light, LightTree.RingOfSpears, false, inTurn).Between, 1e-5f);
            Assert.AreEqual(0f, Cast(Elements.Light, LightTree.RingOfSpears, false).Between, 1e-5f, "the plain ring drives in together");
        }

        [Test]
        public void RingOfDrumsTakesItsDrumsWithItWhenItsTimeIsUp()
        {
            var bank = Upgrade.ById(LightningTree.RingOfDrums).Effects.First(each => each.Do == UpgradeAction.Bank);
            Assert.AreEqual(8f, bank.Seconds, 1e-5f, "the ring stands 8 s");
            var empower = Upgrade.ById(SpellBranch.EmpowerId(LightningTree.RingOfDrums)).Empower;
            var longer = empower.Effects.Single(each => each.Index == 0);
            Assert.AreEqual(10f, longer.Seconds, 1e-5f, "10 s with the Empower");
            Assert.IsFalse(PlayerUpgrades.TallyRunsOut(0f, 100f), "a count banked for no time never empties by itself");
            Assert.IsFalse(PlayerUpgrades.TallyRunsOut(8f, 7.9f));
            Assert.IsTrue(PlayerUpgrades.TallyRunsOut(8f, 8f));
        }

        [Test]
        public void FirewalkerWalksAtHalfThePaceInTheStance()
        {
            int walker = Upgrade.ById(SpellBranch.ShapeId(FireTree.KilnStance)).Shapes.ToList().FindIndex(each => each.Name == "Firewalker");
            var spec = Cast(Elements.Fire, FireTree.KilnStance, false, walker);
            Assert.AreEqual(0.5f, spec.Walk, 1e-5f);
            Assert.AreEqual(0.6f, spec.Seconds, 1e-5f, "and holds only 0.6 s");
            Assert.AreEqual(0f, Cast(Elements.Fire, FireTree.KilnStance, false).Walk, 1e-5f, "the plain stance roots you");
            var step = FormRules.StanceStep(new UnityEngine.Vector3(1f, 0f, 0f), 6f, 0.5f, 0.1f);
            Assert.AreEqual(0.3f, step.x, 1e-5f, "half of 6 m a second for a tenth of a second");
            Assert.AreEqual(0f, FormRules.StanceStep(new UnityEngine.Vector3(1f, 0f, 0f), 6f, 0f, 0.1f).x, 1e-5f, "rooted");
            Assert.AreEqual(0.3f, FormRules.StanceStep(new UnityEngine.Vector3(3f, 0f, 4f), 6f, 0.5f, 0.1f).magnitude, 1e-4f, "never faster than the stick's full lean");
        }

        [Test]
        public void ALastingTimeGoesWithTheSpellsNumbers()
        {
            var into = new SpellSpec();
            SpellChange.Copy(new SpellSpec { Lasts = 3f, Between = 0.2f, Glow = 0.5f }, into);
            Assert.AreEqual(3f, into.Lasts, 1e-5f);
            Assert.AreEqual(0.2f, into.Between, 1e-5f);
            Assert.AreEqual(0.5f, into.Glow, 1e-5f);
            var change = new SpellChange { Lasts = 6f };
            Assert.IsFalse(change.IsEmpty);
            var spec = new SpellSpec { Lasts = 2f };
            change.AddOver(spec, new SpellSpec { Lasts = 4f });
            Assert.AreEqual(4f, spec.Lasts, 1e-5f, "two seconds more, as the Empower gives the Learn");
        }
    }
}
