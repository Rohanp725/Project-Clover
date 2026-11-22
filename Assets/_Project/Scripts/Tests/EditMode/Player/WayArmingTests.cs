using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clover.Core;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.Player.Way.Aspects;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Way reaching play (way_build_plan.md 2.0, the editor half of task 5.2): every equip point arms through
    /// <see cref="Armoury.Arm"/>, which with nothing of the Way learned is exactly <see cref="Weapons.ByName"/> (the god's
    /// own weapon aside, which is his bough: phase5_questions.md 5.2 Q6 and Q13), and which otherwise builds what
    /// <see cref="Armoury.Resolve(string, string, IMarkQuery)"/> builds for the Katana's five states. The Way's choices are
    /// the run's (<see cref="RunState.WayChoices"/>), kept from scene to scene and read into the player's loadout.
    /// </summary>
    public class WayArmingTests
    {
        [SetUp]
        public void SetUp() => RunState.Reset();

        [TearDown]
        public void TearDown() => RunState.Reset();

        private static Loadout Carrying(string weapon, string first = "", string second = "")
        {
            var loadout = new Loadout { Weapon = weapon };
            loadout.Attune(first, second);
            return loadout;
        }

        // Every public field of every step, and the weapon's name: two weapons built alike agree on all of them.
        private static void AssertSame(Weapon expected, Weapon actual, string label)
        {
            Assert.AreEqual(expected.Name, actual.Name, label);
            Assert.AreEqual(expected.HudName, actual.HudName, label);
            var a = expected.AllSteps().ToList();
            var b = actual.AllSteps().ToList();
            Assert.AreEqual(a.Count, b.Count, label + ": the steps");
            for (int i = 0; i < a.Count; i++)
                foreach (var field in typeof(AttackStep).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.FieldType != typeof(float) && field.FieldType != typeof(int) && field.FieldType != typeof(bool) && field.FieldType != typeof(string)) continue;
                    Assert.AreEqual(field.GetValue(a[i]), field.GetValue(b[i]), label + ": step " + i + " (" + a[i].Name + ") " + field.Name);
                }
        }

        [Test]
        public void WithNothingOfTheWayLearnedEveryWeaponIsArmedExactlyAsByName()
        {
            foreach (string name in Weapons.Offered)
                AssertSame(Weapons.ByName(name), Armoury.Arm(name, Carrying(""), RunState.Unlocked, null), name);
            AssertSame(Weapons.ByName(Weapons.SunSpearName), Armoury.Arm(Weapons.SunSpearName, Carrying(""), RunState.Unlocked, null), "the Sun-Spear, whose bough is not written");
            // Marks of the element trees learned change nothing the armoury builds.
            RunState.Unlocked.Add(Upgrade.BurningCrescent);
            AssertSame(Weapons.Katana(), Armoury.Arm(Weapons.KatanaName, Carrying("", "Fire"), RunState.Unlocked, null), "an element mark learned");
        }

        [Test]
        public void TheArmamentStaysTheArmamentInTheElementItIsMadeIn()
        {
            var fire = Armoury.Arm(Weapons.ArmamentName, Carrying(""), RunState.Unlocked, "Fire");
            AssertSame(Weapons.Armament("Fire"), fire, "the Armament made in Fire");
            Assert.AreEqual(Weapons.ArmamentName, Armoury.Resolve(Weapons.ArmamentName, "", MarkSet.None).Name, "kept on the Fists' Way, but never made into the Fists");
            RunState.Element = "Lightning";
            AssertSame(Weapons.Armament("Lightning"), Armoury.Arm(Weapons.ArmamentName, Carrying(""), RunState.Unlocked, null), "no element named: the run's, as ByName makes it");
        }

        [Test]
        public void TheGodsOwnWeaponCarriedIsHisBoughEvenWithNothingBought()
        {
            var storm = Armoury.Arm(Weapons.StormKatanaName, Carrying(""), RunState.Unlocked, null);
            Assert.AreEqual(Weapons.StormKatanaName, storm.Name);
            Assert.AreEqual(StormKatanaBough.FallingDamage, storm.Combo.Steps[2].Damage, 1e-4f, "his third cut, down a lane (5.2 Q13)");
            AssertSame(Armoury.Resolve(Weapons.StormKatanaName, "", MarkSet.None), storm, "as the armoury resolves it");
        }

        [Test]
        public void EachOfTheKatanasFiveStatesIsArmedAsTheArmouryResolvesIt()
        {
            var learned = RunState.Unlocked;
            foreach (string id in new[] { KatanaWay.Sheath, KatanaWay.Stream, KatanaWay.Answer, KatanaWay.SettledHands, KatanaWay.Temper3, KatanaWay.Temper5 })
                learned.Add(id);
            var cases = new[] { ("", "plain"), (KatanaWay.Sheath, "the Sheath"), (KatanaWay.Stream, "the Stream"), (KatanaWay.Answer, "the Answer"), (KatanaWay.Storm, "the Storm Katana") };
            foreach (var (aspect, label) in cases)
            {
                var carried = Carrying(Weapons.FistsName);
                carried.SetAspect(Weapons.KatanaName, aspect);
                if (aspect == KatanaWay.Storm) learned.Add(KatanaWay.Storm);
                var armed = Armoury.Arm(Weapons.KatanaName, carried, learned, null);
                var inForce = new MarkSet(learned.Where(id => id == KatanaWay.SettledHands || id == KatanaWay.Temper3 || id == KatanaWay.Temper5 || id == aspect));
                AssertSame(Armoury.Resolve(Weapons.KatanaName, aspect, inForce), armed, label);
                Assert.AreEqual(Weapons.FistsName, carried.Weapon, "the carried loadout is not changed");
            }
            var sheath = Armoury.Arm(Weapons.KatanaName, WithAspect(KatanaWay.Sheath), learned, null);
            Assert.AreEqual(2, sheath.Combo.Steps.Count, "the Sheath's draw and return cut reach the sword");
            var plain = Armoury.Arm(Weapons.KatanaName, Carrying(""), learned, null);
            Assert.AreEqual(Weapons.Katana().Combo.Steps[0].Recovery * KatanaTrunk.SettledRecovery, plain.Combo.Steps[0].Recovery, 1e-5f, "Settled Hands, a trunk mark, works with no aspect chosen");
            Assert.AreEqual(Weapons.Katana().Combo.Steps[0].Reach + KatanaTrunk.TemperThreeReach, plain.Combo.Steps[0].Reach, 1e-5f, "Temper III's longer draw");
        }

        private static Loadout WithAspect(string aspect)
        {
            var loadout = Carrying("");
            loadout.SetAspect(Weapons.KatanaName, aspect);
            return loadout;
        }

        [Test]
        public void AnAspectLearnedButNotChosenSleepsAndAChosenOneNotLearnedIsNothing()
        {
            RunState.Unlocked.Add(KatanaWay.Sheath);
            Assert.AreEqual(3, Armoury.Arm(Weapons.KatanaName, Carrying(""), RunState.Unlocked, null).Combo.Steps.Count, "learned, not the active aspect");
            RunState.Unlocked.Clear();
            Assert.AreEqual(3, Armoury.Arm(Weapons.KatanaName, WithAspect(KatanaWay.Sheath), RunState.Unlocked, null).Combo.Steps.Count, "chosen but not learned");
        }

        [Test]
        public void MarksAreJudgedForTheWeaponAboutToBeCarriedNotTheOnePutDown()
        {
            RunState.Unlocked.Add(KatanaWay.SettledHands);
            var fromScythe = Armoury.Arm(Weapons.KatanaName, Carrying(Weapons.ScytheName), RunState.Unlocked, null);
            Assert.AreEqual(Weapons.Katana().Combo.Steps[0].Recovery * KatanaTrunk.SettledRecovery, fromScythe.Combo.Steps[0].Recovery, 1e-5f);
            var scythe = Armoury.Arm(Weapons.ScytheName, Carrying(Weapons.KatanaName), RunState.Unlocked, null);
            AssertSame(Weapons.Scythe(), scythe, "the Katana's marks never reach the Scythe");
        }

        [Test]
        public void AnElementInfusionMarkNeedsItsElementAtTheMomentOfArming()
        {
            var learned = new HashSet<string> { KatanaWay.BurningCrescent };
            var marks = new LearnedMarks(Carrying(Weapons.KatanaName, "Fire"), learned);
            Assert.IsTrue(marks.Has(KatanaWay.BurningCrescent));
            Assert.IsFalse(new LearnedMarks(Carrying(Weapons.KatanaName, "Water"), learned).Has(KatanaWay.BurningCrescent));
            Assert.IsFalse(new LearnedMarks(Carrying(Weapons.KatanaName, "Fire"), new HashSet<string>()).Has(KatanaWay.BurningCrescent), "not learned");
        }

        [Test]
        public void TheWayChoicesAreTheRunsAndEveryReadOfTheBuildTakesThem()
        {
            var bought = Upgrade.ById(KatanaWay.Sheath);
            var live = PlayerBuild.Fill(new Loadout(), null, null, null);
            Assert.AreEqual("", live.AspectOf(Weapons.KatanaName));
            // As PlayerUpgrades.TryBuy does: learned on the live loadout, then kept for the run.
            Assert.IsTrue(MarkRules.Learn(bought, RunState.Unlocked, live));
            RunState.WayChoices.CopyWayFrom(live);
            Assert.AreEqual(KatanaWay.Sheath, PlayerBuild.Fill(new Loadout(), null, null, null).AspectOf(Weapons.KatanaName), "a new scene's player reads it");
            RunState.Reset();
            Assert.AreEqual("", PlayerBuild.Fill(new Loadout(), null, null, null).AspectOf(Weapons.KatanaName), "a new run starts with none");
        }

        [Test]
        public void CopyingTheWayChoicesAgainAllocatesNothingAndLeavesTheHandAlone()
        {
            var from = new Loadout { Technique = "fists.a" };
            from.SetAspect(Weapons.KatanaName, KatanaWay.Stream);
            from.SetKeystone(KatanaWay.Stream, KatanaWay.NoLastCut);
            from.Slots[0] = "fire.base";
            var into = new Loadout();
            into.CopyWayFrom(from);
            Assert.AreEqual(KatanaWay.Stream, into.AspectOf(Weapons.KatanaName));
            Assert.AreEqual(KatanaWay.NoLastCut, into.KeystoneOf(KatanaWay.Stream));
            Assert.AreEqual("fists.a", into.Technique);
            Assert.AreEqual("", into.Slots[0], "only the Way's choices");
            var kept = into.Aspects[0];
            into.CopyWayFrom(from);
            Assert.AreSame(kept, into.Aspects[0], "already the same: nothing made again");
            from.SetAspect(Weapons.KatanaName, "");
            into.CopyWayFrom(from);
            Assert.AreEqual("", into.AspectOf(Weapons.KatanaName));
        }
    }
}
