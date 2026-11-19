using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The weapon skill on its own button (Triangle, Y, Q); a held heavy stays a charged heavy (IMPLEMENTATION.md 1).</summary>
    public class WeaponSkillButtonTests
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
            pool.Gain(amount);
            return pool;
        }

        private static AttackStep Run(WeaponCombo combo, StaminaPool stamina, ManaPool mana, float seconds)
        {
            for (float t = 0f; t < seconds; t += Tick)
                if ((combo.Tick(Tick, stamina, mana) & ComboEvents.Started) != 0) return combo.Current;
            return null;
        }

        private static WeaponCombo OnButton(Weapon weapon)
        {
            var combo = weapon.Combo;
            combo.Reset();
            combo.SkillOnButton = true;
            return combo;
        }

        [Test]
        public void TheSkillButtonStartsTheSkillAtOnceAndPaysForIt()
        {
            var combo = OnButton(Weapons.MassiveSword());
            var mana = Mana(100f);
            combo.PressSkill();
            Assert.AreSame(combo.Skill, Run(combo, Stamina(), mana, 0.1f));
            Assert.AreEqual(100f - combo.SkillPrice, mana.Current, 1e-3f);
        }

        [Test]
        public void WithoutTheManaTheSkillButtonDoesNothing()
        {
            var combo = OnButton(Weapons.MassiveSword());
            var mana = Mana(5f);
            combo.PressSkill();
            Assert.IsNull(Run(combo, Stamina(), mana, 0.5f));
            Assert.IsFalse(combo.IsAttacking);
            Assert.AreEqual(5f, mana.Current, 1e-3f);
        }

        [Test]
        public void AHeldHeavyStaysAChargedHeavyAndNeverTheSkill()
        {
            var combo = OnButton(Weapons.MassiveSword());
            var mana = Mana(100f);
            combo.PressHeavy();
            Assert.IsNull(Run(combo, Stamina(), mana, 2f), "held, nothing is released");
            combo.ReleaseHeavy();
            Assert.AreSame(combo.ChargedHeavy, Run(combo, Stamina(), mana, 0.1f));
            Assert.AreEqual(100f, mana.Current, 1e-3f, "no mana spent");
        }

        [Test]
        public void AWeaponWithNoChargedHeavySwingsItsHeavyAtOnce()
        {
            var weapon = Weapons.Katana();
            Assert.IsNull(weapon.Combo.ChargedHeavy, "the Katana has one charge tier: the test needs that");
            var combo = OnButton(weapon);
            combo.PressHeavy();
            Assert.AreSame(combo.Heavy, Run(combo, Stamina(), Mana(100f), 0.05f), "no wait for a skill that is on another button");
        }

        [Test]
        public void TheSkillButtonEndsALightChainFromItsRecovery()
        {
            var combo = OnButton(Weapons.Katana());
            var stamina = Stamina();
            var mana = Mana(100f);
            combo.Press();
            Assert.AreSame(combo.Steps[0], Run(combo, stamina, mana, 0.05f));
            combo.PressSkill();
            Assert.AreSame(combo.Skill, Run(combo, stamina, mana, 1f));
        }

        [Test]
        public void ACountersStanceIsRaisedByTheSkillButton()
        {
            var combo = OnButton(Weapons.Armament(Elements.Fire));
            var stamina = Stamina();
            var mana = Mana(30f);
            combo.PressSkill();
            Run(combo, stamina, mana, 0.4f);
            Assert.IsTrue(combo.IsCharged, "braced while the button is held");
            Assert.IsTrue(combo.Counter(stamina, mana));
            Assert.AreSame(combo.Skill, combo.Current);
        }

        [Test]
        public void TheOldRuleIsKeptWhenTheSkillIsNotOnAButton()
        {
            var combo = Weapons.Katana().Combo;
            combo.Reset();
            var mana = Mana(100f);
            combo.PressHeavy();
            Assert.IsNull(Run(combo, Stamina(), mana, combo.ChargeTime + 0.1f));
            combo.ReleaseHeavy();
            Assert.AreSame(combo.Skill, Run(combo, Stamina(), mana, 0.1f));
        }
    }
}
