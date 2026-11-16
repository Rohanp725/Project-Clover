using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The weapons of the slice and their numbers. Each call builds a fresh weapon, so tuning one in play never leaks
    /// into the next. Attack names are also the names of the animation states that show them.
    /// </summary>
    public static class Weapons
    {
        public const string FistsName = "Fists";
        public const string KatanaName = "Katana";
        public const string StormKatanaName = "Storm Katana";

        public static Weapon ByName(string name) => name == KatanaName ? Katana() : name == StormKatanaName ? StormKatana() : Fists();

        /// <summary>One of each, for checks.</summary>
        public static IEnumerable<Weapon> All
        {
            get
            {
                yield return Fists();
                yield return Katana();
                yield return StormKatana();
            }
        }

        /// <summary>
        /// Bare hands, the first run's weapon. Short reach and light blows, but the quickest chain in the game and a
        /// great deal of stagger for its damage: fists open an enemy up for the finisher. Four lights (jab, cross, hook,
        /// kick), a lunging heavy, a rising chain heavy, and the Breaker: a charged straight that bursts the stagger meter.
        /// </summary>
        public static Weapon Fists() => new Weapon
        {
            Name = FistsName,
            UsesSword = false,
            SkillName = "Breaker",
            ChargeAnimation = "charge_fist",
            SlashColour = new Color(1.7f, 1.5f, 1.1f, 0.8f),
            Combo = new WeaponCombo
            {
                Steps = new List<AttackStep>
                {
                    new AttackStep { Name = "attack_fist_1", Windup = 0.07f, Active = 0.06f, Recovery = 0.17f, Damage = 6f, Stagger = 9f, StaminaCost = 7f, ManaGain = 4f, Lunge = 0.5f, Reach = 1.8f, ArcDegrees = 90f, HitStopSeconds = 0.035f },
                    new AttackStep { Name = "attack_fist_2", Windup = 0.08f, Active = 0.06f, Recovery = 0.18f, Damage = 7f, Stagger = 10f, StaminaCost = 7f, ManaGain = 4f, Lunge = 0.5f, Reach = 1.8f, ArcDegrees = 90f, HitStopSeconds = 0.035f },
                    new AttackStep { Name = "attack_fist_3", Sweep = 1f, Windup = 0.11f, Active = 0.07f, Recovery = 0.22f, Damage = 9f, Stagger = 14f, StaminaCost = 9f, ManaGain = 5f, Lunge = 0.6f, Reach = 1.9f, ArcDegrees = 150f, HitStopSeconds = 0.05f },
                    new AttackStep { Name = "attack_fist_4", Sweep = -1f, Windup = 0.15f, Active = 0.09f, Recovery = 0.36f, Damage = 14f, Stagger = 24f, StaminaCost = 12f, ManaGain = 8f, Lunge = 1.1f, Reach = 2.2f, ArcDegrees = 150f, HitStopSeconds = 0.08f, Shake = 0.1f },
                },
                Heavy = new AttackStep { Name = "attack_fist_heavy", Heavy = true, Windup = 0.24f, Active = 0.09f, Recovery = 0.4f, Damage = 18f, Stagger = 34f, StaminaCost = 16f, ManaGain = 9f, Lunge = 1.4f, Reach = 2f, ArcDegrees = 80f, HitStopSeconds = 0.09f, Shake = 0.14f },
                ChainHeavy = new AttackStep { Name = "attack_fist_rising", Heavy = true, Windup = 0.15f, Active = 0.1f, Recovery = 0.44f, Damage = 20f, Stagger = 42f, StaminaCost = 16f, ManaGain = 11f, Lunge = 0.8f, Reach = 2f, ArcDegrees = 100f, HitStopSeconds = 0.1f, Shake = 0.18f },
                Skill = new AttackStep { Name = "attack_fist_skill", Heavy = true, Windup = 0.1f, Active = 0.12f, Recovery = 0.5f, Damage = 30f, Stagger = 75f, StaminaCost = 15f, ManaGain = 0f, Lunge = 2.4f, Reach = 2.3f, ArcDegrees = 90f, HitStopSeconds = 0.14f, Shake = 0.3f },
                Finisher = new AttackStep { Name = "attack_fist_finisher", Heavy = true, Windup = 0.28f, Active = 0.14f, Recovery = 0.56f, Damage = 24f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 2.6f, ArcDegrees = 120f, HitStopSeconds = 0.2f, Shake = 0.45f },
                SkillManaCost = 30f,
                ChargeTime = 0.4f,
            },
        };

        /// <summary>
        /// Narukami's own sword, given only to one who takes his element: the Katana's moves, a little more of everything,
        /// and a draw that carries further.
        /// </summary>
        public static Weapon StormKatana()
        {
            var weapon = Katana();
            weapon.Name = StormKatanaName;
            weapon.SkillName = "Thunder Draw";
            weapon.SlashColour = new Color(0.9f, 2f, 3.4f, 0.95f);
            foreach (var step in weapon.AllSteps())
            {
                step.Damage *= 1.15f;
                step.Stagger *= 1.15f;
            }
            weapon.Combo.Skill.Lunge = 3.6f;
            weapon.Combo.Skill.Reach = 3.8f;
            return weapon;
        }

        /// <summary>
        /// The Katana, found partway through the castle. The sword lives in its sheath and every chain starts with a
        /// draw. Light chain: a drawing cut, a return cut, a stepping finisher. Heavy: an overhead cut, quicker and
        /// harder when it ends a chain. Skill: the Iaido Slash, a long drawing cut released from the sheath.
        /// </summary>
        public static Weapon Katana() => new Weapon
        {
            Name = KatanaName,
            UsesSword = true,
            SkillName = "Iaido Slash",
            SlashColour = new Color(1.1f, 1.6f, 2.4f, 0.9f),
            Combo = new WeaponCombo
            {
                Steps = new List<AttackStep>
                {
                    new AttackStep { Name = "attack_light_1", Sweep = -1f, Windup = 0.10f, Active = 0.08f, Recovery = 0.24f, Damage = 10f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.7f, Reach = 2.4f, ArcDegrees = 140f, HitStopSeconds = 0.045f },
                    new AttackStep { Name = "attack_light_2", Sweep = 1f, Windup = 0.11f, Active = 0.08f, Recovery = 0.25f, Damage = 11f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.6f, Reach = 2.4f, ArcDegrees = 140f, HitStopSeconds = 0.045f },
                    new AttackStep { Name = "attack_light_3", Windup = 0.16f, Active = 0.10f, Recovery = 0.38f, Damage = 18f, Stagger = 22f, StaminaCost = 14f, ManaGain = 9f, Lunge = 1.3f, Reach = 2.7f, ArcDegrees = 100f, HitStopSeconds = 0.08f },
                },
                Heavy = new AttackStep { Name = "attack_heavy", Heavy = true, Windup = 0.30f, Active = 0.10f, Recovery = 0.46f, Damage = 26f, Stagger = 34f, StaminaCost = 20f, ManaGain = 10f, Lunge = 1.0f, Reach = 2.8f, ArcDegrees = 90f, HitStopSeconds = 0.10f, Shake = 0.16f },
                ChainHeavy = new AttackStep { Name = "attack_heavy", Heavy = true, Windup = 0.20f, Active = 0.10f, Recovery = 0.46f, Damage = 30f, Stagger = 40f, StaminaCost = 20f, ManaGain = 12f, Lunge = 1.2f, Reach = 2.8f, ArcDegrees = 90f, HitStopSeconds = 0.11f, Shake = 0.2f },
                Finisher = new AttackStep { Name = "attack_finisher", Heavy = true, Windup = 0.30f, Active = 0.14f, Recovery = 0.56f, Damage = 30f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.2f, ArcDegrees = 120f, HitStopSeconds = 0.2f, Shake = 0.45f },
                Skill = new AttackStep { Name = "attack_iaido", Sweep = -1f, Heavy = true, Windup = 0.08f, Active = 0.12f, Recovery = 0.50f, Damage = 48f, Stagger = 50f, StaminaCost = 15f, ManaGain = 0f, Lunge = 2.6f, Reach = 3.3f, ArcDegrees = 170f, HitStopSeconds = 0.14f, Shake = 0.3f },
                SkillManaCost = 30f,
                ChargeTime = 0.4f,
            },
        };
    }
}
