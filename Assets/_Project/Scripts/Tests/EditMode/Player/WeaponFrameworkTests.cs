using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;

namespace Clover.Tests
{
    /// <summary>
    /// The plain rules under the newer weapons: the scythe's distance bands, the two tiers of a held heavy, the ender
    /// chosen by where a chain is stopped, the counter's stance, poise as numbers, and that every weapon brings the
    /// clips its states are named for.
    /// </summary>
    public class WeaponFrameworkTests
    {
        private const float Tick = 1f / 60f;

        private static StaminaPool Stamina()
        {
            var pool = new StaminaPool { Maximum = 100f, RegenerationPerSecond = 0f };
            pool.Fill();
            return pool;
        }

        private static ManaPool Mana(float amount)
        {
            var pool = new ManaPool { Maximum = 100f };
            pool.Empty();
            pool.Gain(amount);
            return pool;
        }

        // Runs the combo for a time and returns the first attack it starts, or null.
        private static AttackStep Run(WeaponCombo combo, StaminaPool stamina, ManaPool mana, float seconds)
        {
            for (float t = 0f; t < seconds; t += Tick)
                if ((combo.Tick(Tick, stamina, mana) & ComboEvents.Started) != 0) return combo.Current;
            return null;
        }

        private static AttackStep HoldHeavy(WeaponCombo combo, ManaPool mana, float heldFor)
        {
            var stamina = Stamina();
            combo.Reset();
            combo.PressHeavy();
            Assert.IsNull(Run(combo, stamina, mana, heldFor), "nothing is released while the button is held");
            combo.ReleaseHeavy();
            return Run(combo, stamina, mana, 0.1f);
        }

        [Test]
        public void TheScytheCutsWithItsTip()
        {
            var step = Weapons.Scythe().Combo.Steps[0];
            Assert.AreEqual(0.6f, step.BandDamage(1.0f), 0.001f, "the haft does little");
            Assert.AreEqual(1f, step.BandDamage(1.5f), 0.001f, "the middle is plain from the inner reach out");
            Assert.AreEqual(1f, step.BandDamage(2.49f), 0.001f);
            Assert.AreEqual(1.5f, step.BandDamage(2.5f), 0.001f, "the tip does half as much again");
            Assert.AreEqual(1.6f, step.BandStagger(3.2f), 0.001f);
            Assert.AreEqual(0.6f, step.BandStagger(0.4f), 0.001f);
            Assert.IsTrue(step.IsSweet(3f));
            Assert.IsFalse(step.IsSweet(2f));
            Assert.AreEqual(1f, Weapons.Katana().Combo.Steps[0].BandDamage(0.5f), 0.001f, "a weapon without bands has none");
        }

        [Test]
        public void TheScythesChainIsTheBestAtTheTipAndTheWorstOnTheHaft()
        {
            var steps = Weapons.Scythe().Combo.Steps;
            Assert.AreEqual(69f, steps.Sum(step => step.Damage * step.BandDamage(3f)), 0.01f);
            Assert.AreEqual(73.6f, steps.Sum(step => step.Stagger * step.BandStagger(3f)), 0.01f);
            Assert.AreEqual(46f, steps.Sum(step => step.Damage * step.BandDamage(2f)), 0.01f);
            Assert.AreEqual(27.6f, steps.Sum(step => step.Damage * step.BandDamage(1f)), 0.01f);
            Assert.AreEqual(2.22f, steps.Sum(step => step.Total), 0.001f);
            Assert.AreEqual(41f, steps.Sum(step => step.StaminaCost), 0.001f);
            Assert.Less(Weapons.Scythe().Combo.Heavy.Lunge, 0f, "its heavy steps backward");
            Assert.Greater(steps.Sum(step => step.Damage * step.BandDamage(3f)), Weapons.Katana().Combo.Steps.Sum(step => step.Damage));
        }

        [Test]
        public void TheMassiveSwordsChainIsAsDesigned()
        {
            var combo = Weapons.MassiveSword().Combo;
            Assert.AreEqual(84f, combo.Steps.Sum(step => step.Damage), 0.001f);
            Assert.AreEqual(94f, combo.Steps.Sum(step => step.Stagger), 0.001f);
            Assert.AreEqual(64f, combo.Steps.Sum(step => step.StaminaCost), 0.001f);
            Assert.AreEqual(2.90f, combo.Steps.Sum(step => step.Total), 0.001f);
            // Each sweep is wound up faster than the last.
            Assert.Greater(combo.Steps[0].Windup, combo.Steps[1].Windup);
            Assert.Greater(combo.Steps[1].Windup, combo.Steps[2].Windup);
        }

        [Test]
        public void AHeldHeavyHasTwoTiers()
        {
            var combo = Weapons.MassiveSword().Combo;
            Assert.AreSame(combo.Heavy, HoldHeavy(combo, Mana(100f), 0.2f), "let go early: the plain heavy");
            Assert.AreSame(combo.ChargedHeavy, HoldHeavy(combo, Mana(100f), 0.6f), "held past the first tier: the charged heavy");
            var mana = Mana(100f);
            Assert.AreSame(combo.Skill, HoldHeavy(combo, mana, 1.05f), "held to the second tier with mana: the skill");
            Assert.AreEqual(100f - combo.SkillManaCost, mana.Current, 0.001f);
            var none = Mana(0f);
            Assert.AreSame(combo.ChargedHeavy, HoldHeavy(combo, none, 1.4f), "without the mana the hold waits at the first tier");
            Assert.AreEqual(0f, none.Current, 0.001f, "the charged heavy costs no mana");
        }

        [Test]
        public void TheTiersAreAnnouncedOnceEach()
        {
            var combo = Weapons.MassiveSword().Combo;
            var stamina = Stamina();
            var mana = Mana(100f);
            combo.PressHeavy();
            int charged = 0, ready = 0;
            for (float t = 0f; t < 1.3f; t += Tick)
            {
                var events = combo.Tick(Tick, stamina, mana);
                if ((events & ComboEvents.Charged) != 0)
                {
                    charged++;
                    Assert.AreEqual(1, combo.ChargeTier);
                }
                if ((events & ComboEvents.SkillReady) != 0)
                {
                    ready++;
                    Assert.AreEqual(2, combo.ChargeTier);
                }
            }
            Assert.AreEqual(1, charged);
            Assert.AreEqual(1, ready);
            Assert.IsTrue(combo.IsCharging, "a held charge with something to release is kept");
        }

        [Test]
        public void AWeaponWithOneTierChargesAsBefore()
        {
            var combo = Weapons.Katana().Combo;
            Assert.IsNull(combo.ChargedHeavy);
            Assert.AreSame(combo.Heavy, HoldHeavy(combo, Mana(100f), 0.2f));
            Assert.AreSame(combo.Skill, HoldHeavy(combo, Mana(100f), 0.5f));
        }

        [Test]
        public void TheArmamentsEnderIsChosenByWhereTheChainStops()
        {
            string[] enders = { "attack_arm_ram", "attack_arm_rising", "attack_arm_slam", "attack_arm_burst" };
            for (int lights = 1; lights <= 4; lights++)
            {
                var combo = Weapons.Armament(Elements.Fire).Combo;
                var stamina = Stamina();
                for (int i = 0; i < lights; i++)
                {
                    combo.Press();
                    var started = Run(combo, stamina, null, 1f);
                    Assert.AreSame(combo.Steps[i], started, $"light {i + 1}");
                }
                combo.PressHeavy();
                combo.ReleaseHeavy();
                var ender = Run(combo, stamina, null, 1f);
                Assert.IsNotNull(ender, $"an ender after {lights}");
                Assert.AreEqual(enders[lights - 1], ender.Name);
                Assert.IsTrue(ender.Heavy);
            }
        }

        [Test]
        public void TheArmamentKeepsTheFistsHandsAndHitsHarder()
        {
            var fists = Weapons.Fists().Combo.Steps;
            var arm = Weapons.Armament(Elements.Ice);
            Assert.AreEqual(fists.Count, arm.Combo.Steps.Count);
            for (int i = 0; i < fists.Count; i++)
                Assert.AreEqual(fists[i].Total, arm.Combo.Steps[i].Total, 0.0001f, "the fists' timing to the hundredth");
            Assert.AreEqual(44f, arm.Combo.Steps.Sum(step => step.Damage), 0.001f);
            Assert.AreEqual(62f, arm.Combo.Steps.Sum(step => step.Stagger), 0.001f);
            Assert.AreEqual(35f, arm.Combo.Steps.Sum(step => step.StaminaCost), 0.001f);
            Assert.AreEqual("Ice Armament", arm.ShownName);
            Assert.AreEqual(Weapons.ArmamentName, arm.Name, "upgrades know it by one name whatever it wears");
            Assert.IsTrue(arm.AllSteps().All(step => step.ElementStacks >= 1), "every blow of the coat lays its element");
            Assert.AreEqual(2, arm.Combo.ChainHeavies[4].ElementStacks);
            Assert.Greater(arm.Combo.ChainHeavies[3].Radius, 0f, "the slam is a disc");
            Assert.Greater(arm.Combo.ChainHeavies[1].Travel, 0f, "the ram is carried");
            Assert.Less(arm.Combo.ChainHeavies[1].Pull, 0f, "and shoves its target away");
        }

        [Test]
        public void TheCountersStanceTakesABlowForMana()
        {
            var combo = Weapons.Armament(Elements.Fire).Combo;
            var stamina = Stamina();
            var mana = Mana(30f);
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.2f);
            Assert.IsFalse(combo.Counter(stamina, mana), "not braced yet");
            Run(combo, stamina, mana, 0.2f);
            Assert.IsTrue(combo.IsCharged, "braced");
            Assert.IsTrue(combo.Counter(stamina, mana));
            Assert.AreSame(combo.Skill, combo.Current);
            Assert.AreEqual(30f - combo.SkillManaCost, mana.Current, 0.001f);

            // Without the mana the stance is only a stance.
            combo.Reset();
            var empty = Mana(10f);
            combo.PressHeavy();
            Run(combo, stamina, empty, 0.4f);
            Assert.IsTrue(combo.IsCharged);
            Assert.IsFalse(combo.Counter(stamina, empty));
            Assert.AreEqual(10f, empty.Current, 0.001f);

            // Held too long, it lets go by itself as Meteor Fist.
            var released = Run(combo, stamina, empty, combo.CounterHoldMax + 0.2f);
            Assert.AreSame(combo.ChargedHeavy, released);

            // A weapon whose skill is released, not answered, never counters.
            var sword = Weapons.MassiveSword().Combo;
            sword.PressHeavy();
            Run(sword, stamina, Mana(100f), 0.6f);
            Assert.IsFalse(sword.Counter(stamina, Mana(100f)));
        }

        [Test]
        public void PoiseIsSizedAgainstTheCastlesBlows()
        {
            var sword = Weapons.MassiveSword().Combo;
            // The acolyte's cuts are 10 and 12, the soldier's thrust 20 and sweep 24.
            foreach (var step in sword.Steps)
            {
                Assert.GreaterOrEqual(step.Poise, 12f, "a sweep walks through an acolyte's cut");
                Assert.Less(step.Poise, 20f, "and not through a soldier's thrust");
            }
            Assert.GreaterOrEqual(sword.Heavy.Poise, 20f);
            Assert.Less(sword.Heavy.Poise, 24f);
            Assert.GreaterOrEqual(sword.ChargedHeavy.Poise, 24f);
            Assert.AreEqual(0f, Weapons.Katana().Combo.Heavy.Poise, "the Katana trades nothing");
            Assert.AreEqual(0f, Weapons.Scythe().Combo.Heavy.Poise);
        }

        [Test]
        public void AStepOfSeveralBlowsStrikesAgain()
        {
            var combo = new WeaponCombo();
            combo.Steps.Add(new AttackStep { Name = "double", Windup = 0.1f, Active = 0.3f, Recovery = 0.1f, StaminaCost = 0f, Repeats = new[] { 0.1f, 0.2f } });
            var stamina = Stamina();
            combo.Press();
            int hits = 0, repeats = 0;
            for (float t = 0f; t < 0.6f; t += Tick)
            {
                var events = combo.Tick(Tick, stamina);
                if ((events & ComboEvents.Hit) != 0) hits++;
                if ((events & ComboEvents.Repeat) != 0) repeats++;
            }
            Assert.AreEqual(1, hits);
            Assert.AreEqual(2, repeats);
        }

        [Test]
        public void EachWeaponGuardsAndDodgesItsOwnWay()
        {
            var sword = Weapons.MassiveSword();
            var scythe = Weapons.Scythe();
            var usual = new Guard();
            Assert.Greater(sword.GuardProfile.ArcDegrees, usual.ArcDegrees, "the widest guard");
            Assert.Less(sword.GuardProfile.StaminaPerDamage, usual.StaminaPerDamage, "and the cheapest");
            Assert.Less(sword.GuardProfile.PerfectWindow, usual.PerfectWindow, "with the tightest perfect guard");
            Assert.Less(sword.DodgeProfile.Distance, new DodgeRoll().Distance);
            Assert.Greater(scythe.DodgeProfile.Distance, new DodgeRoll().Distance, "distance is the scythe's whole business");
            Assert.Less(sword.MoveFactor, 1f);
            Assert.IsNull(Weapons.Katana().GuardProfile, "the Katana keeps the usual numbers");
            var late = new Guard { PerfectFrom = 0.1f, PerfectWindow = 0.3f };
            late.Press();
            late.SetRaised(true);
            late.Tick(0.05f);
            Assert.IsFalse(late.IsPerfect, "a guard slow to rise is not perfect at once");
            late.Tick(0.1f);
            Assert.IsTrue(late.IsPerfect);
        }

        [Test]
        public void EveryWeaponHasANameAStandAndItsClips()
        {
            var clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { Clover.EditorTools.Forge.PlayerAnimations.ClipFolder })
                .Select(guid => System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))).ToList();
            var seen = new HashSet<string>();
            foreach (var weapon in Weapons.All)
            {
                Assert.IsTrue(seen.Add(weapon.Name), $"two weapons are called {weapon.Name}");
                Assert.AreEqual(weapon.Name, Weapons.ByName(weapon.Name).Name);
                string own = weapon.LocomotionState == PlayerAnimator.LocomotionState ? "" : "_" + weapon.Prefix;
                foreach (string clip in new[] { "idle" + own, "run" + own, weapon.GuardState, weapon.RollState, weapon.ChargeAnimation })
                    Assert.Contains(clip, clips, $"{weapon.Name} has no '{clip}' clip");
                foreach (string clip in new[] { weapon.GuardHitState, weapon.PerfectGuardState })
                    if (!string.IsNullOrEmpty(clip)) Assert.Contains(clip, clips, $"{weapon.Name} has no '{clip}' clip");
                // What is carried is either the Katana in the character's own model or a set the weapon builder knows.
                if (!weapon.UsesSword && !string.IsNullOrEmpty(weapon.Carry))
                    Assert.IsNotNull(Clover.EditorTools.WeaponBuilder.SpecFor(weapon.Carry), $"{weapon.Name} carries '{weapon.Carry}', which no weapon model is built for");
            }
            foreach (string offered in Weapons.Offered)
                Assert.AreEqual(offered, Weapons.ByName(offered).Name);
            Assert.AreEqual(Weapons.FistsName, Weapons.ByName("no such weapon").Name);
        }
    }
}
