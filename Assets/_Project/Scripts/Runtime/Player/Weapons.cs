using System;
using System.Collections.Generic;
using Clover.Core;
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

        public const string MassiveSwordName = "Massive Sword";
        public const string ScytheName = "Scythe";
        public const string ArmamentName = "Armament";
        public const string GreatSpearName = "Great Spear";
        public const string GreatHammerName = "Great Hammer";
        public const string TwinKnivesName = "Twin Knives";
        public const string GreatBowName = "Great Bow";
        public const string ChainSickleName = "Chain Sickle";
        public const string SunSpearName = "Sun-Spear";

        // Every weapon there is, by name. A new weapon is one line here and its own method below.
        private static readonly Dictionary<string, Func<Weapon>> builders = new Dictionary<string, Func<Weapon>>
        {
            { FistsName, Fists },
            { KatanaName, Katana },
            { StormKatanaName, StormKatana },
            { MassiveSwordName, MassiveSword },
            { ScytheName, Scythe },
            { ArmamentName, () => Armament(RunState.Element) },
            { GreatSpearName, GreatSpear },
            { GreatHammerName, GreatHammer },
            { TwinKnivesName, TwinKnives },
            { GreatBowName, GreatBow },
            { ChainSickleName, ChainSickle },
            { SunSpearName, SunSpear },
        };

        /// <summary>The names a stand or a rack may offer, in the order they are set out. The god's own sword is given, not chosen.</summary>
        public static readonly string[] Offered = { FistsName, KatanaName, MassiveSwordName, ScytheName, ArmamentName, GreatSpearName, GreatHammerName, TwinKnivesName, GreatBowName, ChainSickleName };

        /// <summary>The weapon a god gives with its element when it is taken: the storm's katana, the sun's spear. Null: none is made yet.</summary>
        public static string OfGod(string element) =>
            element == Elements.Lightning ? StormKatanaName : element == Elements.Fire ? SunSpearName : null;

        public static Weapon ByName(string name) => name != null && builders.TryGetValue(name, out var build) ? build() : Fists();

        /// <summary>One of each, for checks.</summary>
        public static IEnumerable<Weapon> All
        {
            get
            {
                foreach (var build in builders.Values) yield return build();
            }
        }

        // The scythe's bands, on every cut it makes: little on the haft, half as much again at the tip.
        private static AttackStep Banded(AttackStep step)
        {
            step.InnerReach = 1.5f;
            step.InnerDamage = 0.6f;
            step.SweetFrom = 2.5f;
            step.SweetDamage = 1.5f;
            step.SweetStagger = 1.6f;
            return step;
        }

        /// <summary>
        /// The Massive Sword: a slab of iron as long as its bearer. Three wide sweeps, each wound up faster than the
        /// last, that are not put off by a small blow; an overhead that buries itself; a leaping cut from the charge;
        /// and the Elemental Spike Line, driven into the ground from a longer hold. Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon MassiveSword()
        {
            var weapon = new Weapon
            {
                Name = MassiveSwordName,
                Carry = "gsword",
                SkillName = "Elemental Spike Line",
                SlashColour = new Color(1.9f, 1.8f, 1.6f, 0.9f),
                GuardProfile = new Guard { PerfectWindow = 0.15f, StaminaPerDamage = 0.6f, ChipFraction = 0.25f, ArcDegrees = 240f },
                DodgeProfile = new DodgeRoll { Distance = 3.9f, Duration = 0.40f, StaminaCost = 30f, InvulnerableFrom = 0.05f, InvulnerableUntil = 0.25f, Recovery = 0.16f },
                MoveFactor = 0.92f,
                GuardMoveFactor = 0.3f,
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        new AttackStep { Name = "attack_gsword_1", Sweep = -1f, Windup = 0.34f, Active = 0.14f, Recovery = 0.52f, Damage = 24f, Stagger = 26f, StaminaCost = 20f, ManaGain = 9f, Lunge = 0.8f, Reach = 3.4f, ArcDegrees = 190f, HitStopSeconds = 0.08f, Shake = 0.10f, Poise = 12f },
                        new AttackStep { Name = "attack_gsword_2", Sweep = 1f, Windup = 0.26f, Active = 0.14f, Recovery = 0.50f, Damage = 26f, Stagger = 28f, StaminaCost = 20f, ManaGain = 9f, Lunge = 0.8f, Reach = 3.4f, ArcDegrees = 190f, HitStopSeconds = 0.08f, Shake = 0.10f, Poise = 12f },
                        new AttackStep { Name = "attack_gsword_3", Sweep = -1f, Windup = 0.20f, Active = 0.18f, Recovery = 0.62f, Damage = 34f, Stagger = 40f, StaminaCost = 24f, ManaGain = 12f, Lunge = 0.6f, Reach = 3.6f, ArcDegrees = 360f, HitStopSeconds = 0.10f, Shake = 0.16f, Poise = 12f },
                    },
                    Heavy = new AttackStep { Name = "attack_gsword_heavy", Heavy = true, Windup = 0.50f, Active = 0.12f, Recovery = 0.66f, Damage = 44f, Stagger = 56f, StaminaCost = 28f, ManaGain = 14f, Lunge = 1.0f, Reach = 3.6f, ArcDegrees = 50f, HitStopSeconds = 0.13f, Shake = 0.28f, Poise = 22f },
                    ChainHeavy = new AttackStep { Name = "attack_gsword_chain", Heavy = true, Sweep = 1f, Windup = 0.28f, Active = 0.12f, Recovery = 0.60f, Damage = 46f, Stagger = 60f, StaminaCost = 26f, ManaGain = 14f, Lunge = 1.2f, Reach = 3.6f, ArcDegrees = 120f, HitStopSeconds = 0.13f, Shake = 0.30f, Poise = 22f, Effect = StepEffect.GroundSparks },
                    ChargedHeavy = new AttackStep { Name = "attack_gsword_charged", Heavy = true, Windup = 0.12f, Active = 0.14f, Recovery = 0.74f, Damage = 62f, Stagger = 80f, StaminaCost = 34f, ManaGain = 16f, Lunge = 1.4f, Reach = 3.8f, ArcDegrees = 60f, HitStopSeconds = 0.16f, Shake = 0.42f, Poise = 30f },
                    Skill = new AttackStep { Name = "attack_gsword_skill", Heavy = true, Windup = 0.22f, Active = 0.10f, Recovery = 0.64f, Damage = 20f, Stagger = 30f, StaminaCost = 20f, ManaGain = 0f, Lunge = 0.4f, Reach = 2.2f, ArcDegrees = 120f, HitStopSeconds = 0.12f, Shake = 0.30f, Effect = StepEffect.SpikeLine },
                    Finisher = new AttackStep
                    {
                        Name = "attack_gsword_finisher", Heavy = true, Windup = 0.62f, Active = 0.14f, Recovery = 0.74f, Damage = 44f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.6f, ArcDegrees = 120f, HitStopSeconds = 0.24f, Shake = 0.60f,
                        LungeSeconds = 0.14f, HoldAt = 0.44f, HoldSeconds = 0.18f, Effect = StepEffect.GroundSplit, EffectTurn = -70f,
                        // The pommel into the chest, then the point scoring the ground as the bearer walks round.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.14f, HitStopSeconds = 0.04f, Shake = 0.08f, Effect = VfxKind.HitSpark },
                            new FinisherBeat { Time = 0.30f, HitStopSeconds = 0f, Shake = 0f, Effect = VfxKind.GuardSpark },
                        },
                    },
                    SkillManaCost = 35f,
                    ChargeTime = 0.45f,
                    SkillChargeTime = 0.95f,
                    ChargeMoveFactor = 0.25f,
                },
            };
            // The carrier of its infused strike: the rising cut, dragged up off the floor (steel/weapons.md).
            return Carried(weapon, w => w.Combo.ChainHeavy).Named("gsword", true);
        }

        /// <summary>
        /// The Scythe: the longest blade on the longest haft, and it only truly cuts with its last metre. Long, short,
        /// long: out, back, and a full turn. Its heavy is made by stepping away, and the Crescent Slash leaves a blade
        /// of light hanging in the air. Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon Scythe()
        {
            var weapon = new Weapon
            {
                Name = ScytheName,
                Carry = "scythe",
                SkillName = "Crescent Slash",
                SlashColour = new Color(1.4f, 1.5f, 2.1f, 0.9f),
                GuardProfile = new Guard { PerfectWindow = 0.18f, StaminaPerDamage = 1.0f, ChipFraction = 0.35f, ArcDegrees = 220f },
                DodgeProfile = new DodgeRoll { Distance = 4.8f },
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        Banded(new AttackStep { Name = "attack_scythe_1", Sweep = 1f, Windup = 0.20f, Active = 0.14f, Recovery = 0.36f, Damage = 13f, Stagger = 12f, StaminaCost = 13f, ManaGain = 6f, Lunge = 0.3f, Reach = 3.5f, ArcDegrees = 200f, HitStopSeconds = 0.05f }),
                        Banded(new AttackStep { Name = "attack_scythe_2", Sweep = -1f, Windup = 0.14f, Active = 0.12f, Recovery = 0.30f, Damage = 13f, Stagger = 12f, StaminaCost = 12f, ManaGain = 6f, Lunge = 0.3f, Reach = 3.5f, ArcDegrees = 200f, HitStopSeconds = 0.05f }),
                        Banded(new AttackStep { Name = "attack_scythe_3", Sweep = 1f, Windup = 0.30f, Active = 0.16f, Recovery = 0.50f, Damage = 20f, Stagger = 22f, StaminaCost = 16f, ManaGain = 9f, Lunge = 0.5f, Reach = 3.7f, ArcDegrees = 360f, HitStopSeconds = 0.08f, Shake = 0.08f }),
                    },
                    Heavy = Banded(new AttackStep { Name = "attack_scythe_heavy", Heavy = true, Sweep = -1f, Windup = 0.28f, Active = 0.12f, Recovery = 0.46f, Damage = 24f, Stagger = 28f, StaminaCost = 18f, ManaGain = 10f, Lunge = -1.4f, Reach = 3.8f, ArcDegrees = 150f, HitStopSeconds = 0.09f, Shake = 0.14f }),
                    ChainHeavy = Banded(new AttackStep { Name = "attack_scythe_chain", Heavy = true, Sweep = 1f, Windup = 0.20f, Active = 0.12f, Recovery = 0.48f, Damage = 28f, Stagger = 34f, StaminaCost = 18f, ManaGain = 12f, Lunge = 0.8f, Reach = 3.6f, ArcDegrees = 120f, HitStopSeconds = 0.10f, Shake = 0.18f }),
                    Skill = Banded(new AttackStep
                    {
                        Name = "attack_scythe_skill", Heavy = true, Sweep = 1f, Windup = 0.14f, Active = 0.12f, Recovery = 0.50f, Damage = 16f, Stagger = 18f, StaminaCost = 15f, ManaGain = 0f, Lunge = 0f, Reach = 3.6f, ArcDegrees = 180f, HitStopSeconds = 0.10f, Shake = 0.20f,
                        Shot = new WeaponShot { Speed = 12f, Range = 6f, Radius = 1.6f, Damage = 20f, Stagger = 22f, Pierce = true, HangSeconds = 1.6f, HangBite = 6f, HangEvery = 0.4f, Prop = "crescent" },
                    }),
                    Finisher = Banded(new AttackStep
                    {
                        Name = "attack_scythe_finisher", Heavy = true, Sweep = 1f, Windup = 0.78f, Active = 0.14f, Recovery = 0.56f, Damage = 30f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.8f, ArcDegrees = 360f, HitStopSeconds = 0.22f, Shake = 0.50f,
                        LungeSeconds = 0.16f, HoldAt = 0.52f, HoldSeconds = 0.26f,
                        // The hook under the kneeling enemy, then the heave that throws it up to hang over the hold.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.16f, HitStopSeconds = 0.04f, Shake = 0.08f, Effect = VfxKind.HitSpark },
                            new FinisherBeat { Time = 0.46f, HitStopSeconds = 0.03f, Shake = 0.12f, Effect = VfxKind.Dust, Lift = 0.5f },
                        },
                    }),
                    SkillManaCost = 30f,
                    ChargeTime = 0.40f,
                },
            };
            // The finisher is the one blow that does not care where the enemy kneels.
            weapon.Combo.Finisher.InnerDamage = 1f;
            weapon.Combo.Finisher.SweetDamage = 1f;
            // The carrier: the full turn, a circle at the tip.
            return Carried(weapon, w => w.Combo.Steps[2]).Named("scythe", false);
        }

        // The spear's lane: too close, and it is the haft.
        private static AttackStep Lane(AttackStep step, float width)
        {
            step.LineWidth = width;
            step.InnerReach = 1.4f;
            step.InnerDamage = 0.6f;
            return step;
        }

        /// <summary>
        /// The Great Spear: the longest straight reach there is, down one narrow lane, from behind a guard that never
        /// has to come down. Thrust, thrust, and a sweep for what has come round the side; a thrust made from behind
        /// the guard; a couched charge from the hold; and the Flying Spear, thrown, stuck, and run to. Numbers from
        /// docs/design/weapons.md.
        /// </summary>
        public static Weapon GreatSpear()
        {
            var weapon = new Weapon
            {
                Name = GreatSpearName,
                Carry = "spear",
                SkillName = "Flying Spear",
                SlashColour = new Color(1.4f, 1.8f, 2.0f, 0.85f),
                GuardProfile = new Guard { PerfectWindow = 0.18f, StaminaPerDamage = 0.8f, ChipFraction = 0.3f, ArcDegrees = 160f },
                DodgeProfile = new DodgeRoll { Distance = 4.2f },
                GuardMoveFactor = 0.5f,
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        Lane(new AttackStep { Name = "attack_spear_1", Windup = 0.14f, Active = 0.07f, Recovery = 0.27f, Damage = 10f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.5f, Reach = 4.0f, HitStopSeconds = 0.045f }, 0.9f),
                        Lane(new AttackStep { Name = "attack_spear_2", Windup = 0.14f, Active = 0.07f, Recovery = 0.27f, Damage = 11f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.5f, Reach = 4.0f, HitStopSeconds = 0.045f }, 0.9f),
                        new AttackStep { Name = "attack_spear_3", Sweep = 1f, Windup = 0.20f, Active = 0.10f, Recovery = 0.36f, Damage = 15f, Stagger = 20f, StaminaCost = 14f, ManaGain = 8f, Lunge = 0.6f, Reach = 3.6f, ArcDegrees = 160f, HitStopSeconds = 0.07f, Shake = 0.06f },
                    },
                    GuardAttack = Lane(new AttackStep { Name = "attack_spear_guard", Windup = 0.16f, Active = 0.07f, Recovery = 0.30f, Damage = 8f, Stagger = 8f, StaminaCost = 12f, ManaGain = 4f, Lunge = 0.2f, Reach = 3.8f, HitStopSeconds = 0.04f }, 0.8f),
                    Heavy = Lane(new AttackStep { Name = "attack_spear_heavy", Heavy = true, Windup = 0.30f, Active = 0.10f, Recovery = 0.48f, Damage = 24f, Stagger = 32f, StaminaCost = 20f, ManaGain = 10f, Lunge = 0.8f, Reach = 4.4f, HitStopSeconds = 0.10f, Shake = 0.16f, Effect = StepEffect.GroundSlam }, 1.1f),
                    ChainHeavy = new AttackStep { Name = "attack_spear_chain", Heavy = true, Windup = 0.22f, Active = 0.12f, Recovery = 0.46f, Damage = 20f, Stagger = 44f, StaminaCost = 18f, ManaGain = 10f, Lunge = 2.4f, Reach = 2.4f, ArcDegrees = 80f, HitStopSeconds = 0.10f, Shake = 0.20f, Pull = -2.0f },
                    ChargedHeavy = Lane(new AttackStep { Name = "attack_spear_charged", Heavy = true, Windup = 0.08f, Active = 0.34f, Recovery = 0.50f, Damage = 30f, Stagger = 36f, StaminaCost = 26f, ManaGain = 12f, Lunge = 0f, Reach = 3.2f, HitStopSeconds = 0.10f, Shake = 0.24f, Travel = 5.0f, Pull = -1.5f, Poise = 16f }, 1.3f),
                    Skill = new AttackStep
                    {
                        Name = "attack_spear_skill", Heavy = true, Windup = 0.20f, Active = 0.06f, Recovery = 0.20f, Damage = 0f, Stagger = 0f, StaminaCost = 15f, ManaGain = 0f, Lunge = 0.6f, Reach = 0f, ArcDegrees = 0f, HitStopSeconds = 0.08f, Shake = 0.14f,
                        // Thrown: it stops in the first enemy it meets, or stands in the ground at the end of its flight.
                        Shot = new WeaponShot { Speed = 34f, Range = 11f, Radius = 0.5f, Damage = 26f, Stagger = 30f, Pierce = false, Sticks = true, Heavy = true, Prop = "spear", PropLength = 2.0f },
                        // Then the bearer goes to it, and arrives with the turning cut that rips it out.
                        FollowStep = new AttackStep { Name = "attack_spear_recall", Heavy = true, Windup = 0.22f, Active = 0.10f, Recovery = 0.36f, Damage = 18f, Stagger = 26f, StaminaCost = 0f, ManaGain = 0f, Lunge = 0f, Reach = 2.6f, ArcDegrees = 360f, HitStopSeconds = 0.12f, Shake = 0.30f },
                        FollowDelay = 0.15f,
                        FollowSeconds = 0.22f,
                    },
                    Finisher = new AttackStep
                    {
                        Name = "attack_spear_finisher", Heavy = true, Windup = 0.72f, Active = 0.12f, Recovery = 0.66f, Damage = 30f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.6f, ArcDegrees = 120f, HitStopSeconds = 0.24f, Shake = 0.60f,
                        LungeSeconds = 0.12f, HoldAt = 0.42f, HoldSeconds = 0.24f, Effect = StepEffect.GroundSlam, Pull = -1.8f,
                        // The low thrust under the ribs, then the lever that raises the body on the point to hang over the hold.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.14f, HitStopSeconds = 0.05f, Shake = 0.10f, Effect = VfxKind.HitSpark },
                            new FinisherBeat { Time = 0.26f, HitStopSeconds = 0.03f, Shake = 0.12f, Effect = VfxKind.Dust, Lift = 0.5f },
                        },
                    },
                    SkillManaCost = 30f,
                    ChargeTime = 0.50f,
                    SkillChargeTime = 0.90f,
                    ChargeMoveFactor = 0.3f,
                },
            };
            // The carrier: the first thrust, a lane.
            return Carried(weapon, w => w.Combo.Steps[0]).Named("spear", false);
        }

        /// <summary>
        /// The Sun-Spear, Khepharu's own, given with Fire when it is taken (fire_rework.md section 5): the Great Spear's
        /// moves, drawn in the god's gold, with one of its own in place of the Flying Spear. The Sun Throw stands in the
        /// floor where it lands, and a moment later is called back as a ray that strikes everything on the line home.
        /// </summary>
        public static Weapon SunSpear()
        {
            var weapon = GreatSpear();
            weapon.Name = SunSpearName;
            weapon.Carry = "sunspear";
            weapon.SkillName = "Sun Throw";
            weapon.SlashColour = new Color(2.6f, 2.1f, 1.1f, 0.9f);
            var skill = weapon.Combo.Skill;
            skill.Shot = new WeaponShot
            {
                Speed = 40f, Range = 12f, Radius = 0.5f, Damage = 26f, Stagger = 30f, Pierce = false, Sticks = true, StickSeconds = 0.9f, Heavy = true,
                Prop = "sunspear", PropLength = 2.0f,
                // Home as a ray: as wide as the king's own, every enemy on the line struck once, at the throw's full weight.
                RaysBack = true, RayRadius = 1.1f, RaySeconds = 0.18f, ReturnScale = 1f,
            };
            // Nobody runs to it: it comes back.
            skill.FollowStep = null;
            return weapon;
        }

        /// <summary>
        /// The Great Hammer: a block of iron on a pole that does not cut, it lands. A side swing and then a slower
        /// drop onto a spot a stride ahead; Quake; the lift that ends a chain; Earthshaker from the hold; and the
        /// Spinning Hammer Toss, which leaves the hand and comes back to it. Its guard is slow to rise and the best
        /// there is once up. Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon GreatHammer()
        {
            var weapon = new Weapon
            {
                Name = GreatHammerName,
                Carry = "hammer",
                SkillName = "Spinning Hammer Toss",
                SlashColour = new Color(2.0f, 1.6f, 1.2f, 0.9f),
                GuardProfile = new Guard { PerfectFrom = 0.08f, PerfectWindow = 0.24f, StaminaPerDamage = 0.55f, ChipFraction = 0.2f, ArcDegrees = 180f },
                DodgeProfile = new DodgeRoll { Distance = 3.6f, Duration = 0.42f, StaminaCost = 32f, InvulnerableFrom = 0.06f, InvulnerableUntil = 0.25f, Recovery = 0.18f },
                MoveFactor = 0.9f,
                GuardMoveFactor = 0.3f,
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        // The clip swings from the bearer's right across to the left, so the sweep is +1 (the design's table assumed the other side).
                        new AttackStep { Name = "attack_hammer_1", Sweep = 1f, Windup = 0.30f, Active = 0.12f, Recovery = 0.50f, Damage = 20f, Stagger = 34f, StaminaCost = 20f, ManaGain = 8f, Lunge = 0.7f, Reach = 3.0f, ArcDegrees = 150f, HitStopSeconds = 0.09f, Shake = 0.12f, Poise = 12f },
                        new AttackStep { Name = "attack_hammer_2", Windup = 0.40f, Active = 0.14f, Recovery = 0.62f, Damage = 26f, Stagger = 48f, StaminaCost = 24f, ManaGain = 10f, Lunge = 0.6f, Reach = 2.5f, Radius = 1.5f, HitStopSeconds = 0.12f, Shake = 0.22f, Poise = 16f, Effect = StepEffect.GroundSlam },
                    },
                    Heavy = new AttackStep { Name = "attack_hammer_heavy", Heavy = true, Windup = 0.46f, Active = 0.12f, Recovery = 0.70f, Damage = 36f, Stagger = 70f, StaminaCost = 30f, ManaGain = 14f, Lunge = 0.8f, Reach = 2.6f, Radius = 2.0f, HitStopSeconds = 0.14f, Shake = 0.34f, Poise = 22f, Effect = StepEffect.GroundSlam },
                    ChainHeavy = new AttackStep { Name = "attack_hammer_chain", Heavy = true, Sweep = 1f, Windup = 0.26f, Active = 0.12f, Recovery = 0.64f, Damage = 34f, Stagger = 64f, StaminaCost = 26f, ManaGain = 14f, Lunge = 0.6f, Reach = 3.0f, ArcDegrees = 110f, HitStopSeconds = 0.14f, Shake = 0.32f, Pull = -2.5f, Poise = 22f },
                    ChargedHeavy = new AttackStep { Name = "attack_hammer_charged", Heavy = true, Windup = 0.14f, Active = 0.14f, Recovery = 0.80f, Damage = 52f, Stagger = 100f, StaminaCost = 36f, ManaGain = 16f, Lunge = 0.6f, Reach = 2.6f, Radius = 3.0f, HitStopSeconds = 0.18f, Shake = 0.50f, Poise = 30f, Effect = StepEffect.GroundSlam },
                    Skill = new AttackStep
                    {
                        Name = "attack_hammer_skill", Heavy = true, Sweep = 1f, Windup = 0.30f, Active = 0.30f, Recovery = 0.50f, Damage = 14f, Stagger = 24f, StaminaCost = 20f, ManaGain = 0f, Lunge = 0f, Reach = 3.0f, ArcDegrees = 360f, HitStopSeconds = 0.08f, Shake = 0.20f,
                        // Two turns, a blow all round with each, and it is let go at the top of the second.
                        Repeats = new[] { 0.15f },
                        Shot = new WeaponShot { Speed = 16f, Range = 8f, Radius = 1.2f, Damage = 28f, Stagger = 50f, Pierce = true, Heavy = true, Returns = true, ReturnSpeed = 20f, ReturnScale = 0.5f, ReleaseAfter = 0.28f, Prop = "hammer", PropLength = 0.6f, Tumble = 900f },
                    },
                    Finisher = new AttackStep
                    {
                        Name = "attack_hammer_finisher", Heavy = true, Sweep = 1f, Windup = 0.76f, Active = 0.12f, Recovery = 0.62f, Damage = 36f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.2f, ArcDegrees = 180f, HitStopSeconds = 0.26f, Shake = 0.70f,
                        LungeSeconds = 0.12f, HoldAt = 0.42f, HoldSeconds = 0.30f, Fling = 6f,
                        // The lift under the chin that sends the enemy up; the bearer turns away and winds up while it hangs.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.16f, HitStopSeconds = 0.06f, Shake = 0.25f, Effect = VfxKind.HeavySpark, Lift = 0.6f },
                        },
                    },
                    SkillManaCost = 35f,
                    ChargeTime = 0.55f,
                    SkillChargeTime = 1.00f,
                    ChargeMoveFactor = 0.3f,
                },
            };
            // The carrier: the drop, a disc a stride ahead.
            return Carried(weapon, w => w.Combo.Steps[1]).Named("hammer", true);
        }

        /// <summary>
        /// The Twin Knives: the quickest hands and the least reach. Five cuts in a second and a half (the third is two
        /// blows), every blow worth half as much again on a back; the fifth cut and the chain's heavy both carry the
        /// bearer through the target and turn them to face its back; a cut out of the sidestep; and the Fan of Knives,
        /// a spring away with five knives of light thrown from the top of it. The guard is a parry and nothing else.
        /// Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon TwinKnives()
        {
            var weapon = new Weapon
            {
                Name = TwinKnivesName,
                Carry = "knife",
                SkillName = "Fan of Knives",
                SlashColour = new Color(1.5f, 1.9f, 1.7f, 0.85f),
                BackMultiplier = 1.5f,
                OffHandSocket = WeaponRack.HandLeft,
                RestSocket = WeaponRack.Hip,
                GuardProfile = new Guard { PerfectWindow = 0.21f, StaminaPerDamage = 1.25f, ChipFraction = 0.45f, ArcDegrees = 160f },
                DodgeProfile = new DodgeRoll { Distance = 3.4f, Duration = 0.26f, StaminaCost = 16f, InvulnerableFrom = 0.02f, InvulnerableUntil = 0.19f, Recovery = 0.06f },
                MoveFactor = 1.08f,
                GuardMoveFactor = 0.7f,
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        new AttackStep { Name = "attack_knife_1", Sweep = -1f, Windup = 0.06f, Active = 0.05f, Recovery = 0.13f, Damage = 5f, Stagger = 4f, StaminaCost = 5f, ManaGain = 3f, Lunge = 0.5f, Reach = 1.7f, ArcDegrees = 100f, HitStopSeconds = 0.025f },
                        new AttackStep { Name = "attack_knife_2", Sweep = 1f, Windup = 0.06f, Active = 0.05f, Recovery = 0.13f, Damage = 5f, Stagger = 4f, StaminaCost = 5f, ManaGain = 3f, Lunge = 0.5f, Reach = 1.7f, ArcDegrees = 100f, HitStopSeconds = 0.025f },
                        // The cross: both knives in, then both out.
                        new AttackStep { Name = "attack_knife_3", Windup = 0.08f, Active = 0.10f, Recovery = 0.16f, Damage = 5f, Stagger = 5f, StaminaCost = 7f, ManaGain = 3f, Lunge = 0.6f, Reach = 1.8f, ArcDegrees = 120f, HitStopSeconds = 0.03f, Repeats = new[] { 0.06f } },
                        new AttackStep { Name = "attack_knife_4", Sweep = -1f, Windup = 0.07f, Active = 0.05f, Recovery = 0.14f, Damage = 7f, Stagger = 5f, StaminaCost = 6f, ManaGain = 4f, Lunge = 0.5f, Reach = 1.8f, ArcDegrees = 100f, HitStopSeconds = 0.03f },
                        // The passing spin: through the target and out the far side.
                        new AttackStep { Name = "attack_knife_5", Sweep = 1f, Windup = 0.12f, Active = 0.08f, Recovery = 0.30f, Damage = 12f, Stagger = 12f, StaminaCost = 9f, ManaGain = 6f, Lunge = 0.4f, Reach = 2.0f, ArcDegrees = 200f, HitStopSeconds = 0.06f, Shake = 0.06f, Travel = 2.4f, PassThrough = true },
                    },
                    Heavy = new AttackStep { Name = "attack_knife_heavy", Heavy = true, Windup = 0.18f, Active = 0.08f, Recovery = 0.34f, Damage = 16f, Stagger = 18f, StaminaCost = 14f, ManaGain = 8f, Lunge = 2.2f, Reach = 2.0f, ArcDegrees = 60f, HitStopSeconds = 0.07f, Shake = 0.08f },
                    ChainHeavy = new AttackStep { Name = "attack_knife_chain", Heavy = true, Sweep = -1f, Windup = 0.10f, Active = 0.10f, Recovery = 0.30f, Damage = 18f, Stagger = 20f, StaminaCost = 14f, ManaGain = 9f, Lunge = 0.4f, Reach = 1.8f, ArcDegrees = 140f, HitStopSeconds = 0.07f, Shake = 0.10f, Travel = 3.0f, PassThrough = true },
                    DashAttack = new AttackStep { Name = "attack_knife_dash", Sweep = 1f, Windup = 0.04f, Active = 0.08f, Recovery = 0.20f, Damage = 9f, Stagger = 8f, StaminaCost = 6f, ManaGain = 5f, Lunge = 1.0f, Reach = 2.0f, ArcDegrees = 160f, HitStopSeconds = 0.04f },
                    Skill = new AttackStep
                    {
                        Name = "attack_knife_skill", Heavy = true, Windup = 0.10f, Active = 0.06f, Recovery = 0.36f, Damage = 0f, Stagger = 0f, StaminaCost = 12f, ManaGain = 0f, Lunge = -2.6f, Reach = 0f, ArcDegrees = 0f, HitStopSeconds = 0.05f, Shake = 0.10f,
                        // Five knives of light in a fan: from a step away all five find one enemy, from range it thins out.
                        Shot = new WeaponShot { Count = 5, SpreadDegrees = 50f, Speed = 22f, Range = 8f, Radius = 0.35f, Damage = 9f, Stagger = 8f, Pierce = false, FromStepStart = true },
                    },
                    Finisher = new AttackStep
                    {
                        Name = "attack_knife_finisher", Heavy = true, Windup = 0.70f, Active = 0.10f, Recovery = 0.50f, Damage = 22f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.0f, ArcDegrees = 360f, HitStopSeconds = 0.20f, Shake = 0.45f,
                        LungeSeconds = 0.08f, HoldAt = 0.52f, HoldSeconds = 0.16f,
                        // Three Crossings: three passes through the kneeling enemy to the corners of a triangle round it, each leaving a line.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.10f, HitStopSeconds = 0.03f, Shake = 0.05f, Effect = VfxKind.HitSpark, DashBeyond = 2.2f, DashBearing = 0f, DashSeconds = 0.10f },
                            new FinisherBeat { Time = 0.28f, HitStopSeconds = 0.03f, Shake = 0.05f, Effect = VfxKind.HitSpark, DashBeyond = 2.2f, DashBearing = 120f, DashSeconds = 0.10f },
                            new FinisherBeat { Time = 0.46f, HitStopSeconds = 0.03f, Shake = 0.06f, Effect = VfxKind.HitSpark, DashBeyond = 2.2f, DashBearing = 240f, DashSeconds = 0.10f },
                        },
                    },
                    SkillManaCost = 25f,
                    ChargeTime = 0.30f,
                },
            };
            // The carrier: the crossing, through the enemy and out behind it.
            return Carried(weapon, w => w.Combo.Steps[4]).Named("knife", true);
        }

        /// <summary>
        /// The Chain Sickle: a blade in the right hand for the enemy in front and a weight on a chain in the left for
        /// the one four metres off, and the light chain goes from one to the other: cut, weight, hook, whirl. The
        /// sickle itself thrown on the chain for the heavy; the yank, which drags what is light to the bearer's feet
        /// and drags the bearer to what is not; and Entangle, which binds the first enemy down a long lane.
        /// Numbers from docs/design/weapons.md. Its clips and its model are named "sickle": "chain" is already the
        /// word every weapon uses for the heavy that ends a chain.
        /// </summary>
        public static Weapon ChainSickle()
        {
            var weapon = new Weapon
            {
                Name = ChainSickleName,
                Carry = "sickle",
                SkillName = "Entangle",
                SlashColour = new Color(1.6f, 1.7f, 1.9f, 0.85f),
                OffHandSocket = WeaponRack.HandLeft,
                RestSocket = WeaponRack.Hip,
                AimRange = 7.5f,
                // A perfect guard whips the chain round the attacking limb: the attacker is yanked in, off balance.
                PerfectGuardPull = 1.2f,
                PerfectGuardStagger = 15f,
                GuardProfile = new Guard { PerfectWindow = 0.18f, StaminaPerDamage = 1.1f, ChipFraction = 0.4f, ArcDegrees = 200f },
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        new AttackStep { Name = "attack_sickle_1", Sweep = 1f, Windup = 0.10f, Active = 0.07f, Recovery = 0.22f, Damage = 9f, Stagger = 8f, StaminaCost = 9f, ManaGain = 5f, Lunge = 0.6f, Reach = 2.2f, ArcDegrees = 130f, HitStopSeconds = 0.04f },
                        new AttackStep { Name = "attack_sickle_2", Windup = 0.18f, Active = 0.08f, Recovery = 0.34f, Damage = 11f, Stagger = 20f, StaminaCost = 11f, ManaGain = 6f, Lunge = 0.2f, Reach = 5.2f, LineWidth = 0.9f, HitStopSeconds = 0.06f, Shake = 0.06f, Chain = ChainThrow.Lane },
                        new AttackStep { Name = "attack_sickle_3", Sweep = -1f, Windup = 0.10f, Active = 0.07f, Recovery = 0.24f, Damage = 10f, Stagger = 8f, StaminaCost = 9f, ManaGain = 5f, Lunge = 0.7f, Reach = 2.3f, ArcDegrees = 130f, HitStopSeconds = 0.04f, Pull = 0.6f },
                        // The whirl: two rings at four metres.
                        new AttackStep { Name = "attack_sickle_4", Sweep = 1f, Windup = 0.24f, Active = 0.20f, Recovery = 0.42f, Damage = 8f, Stagger = 12f, StaminaCost = 14f, ManaGain = 4f, Lunge = 0f, Reach = 4.0f, ArcDegrees = 360f, HitStopSeconds = 0.05f, Shake = 0.08f, Repeats = new[] { 0.10f }, Chain = ChainThrow.Whirl },
                    },
                    Heavy = new AttackStep { Name = "attack_sickle_heavy", Heavy = true, Sweep = 1f, Windup = 0.30f, Active = 0.14f, Recovery = 0.50f, Damage = 22f, Stagger = 26f, StaminaCost = 20f, ManaGain = 10f, Lunge = 0.4f, Reach = 4.6f, ArcDegrees = 220f, HitStopSeconds = 0.09f, Shake = 0.14f, Chain = ChainThrow.Sweep },
                    ChainHeavy = new AttackStep { Name = "attack_sickle_chain", Heavy = true, Windup = 0.16f, Active = 0.08f, Recovery = 0.44f, Damage = 14f, Stagger = 30f, StaminaCost = 16f, ManaGain = 9f, Lunge = 0f, Reach = 5.5f, LineWidth = 1.0f, HitStopSeconds = 0.08f, Shake = 0.12f, Pull = 3.0f, PullSelf = true, Chain = ChainThrow.Lane },
                    Skill = new AttackStep { Name = "attack_sickle_skill", Heavy = true, Windup = 0.16f, Active = 0.10f, Recovery = 0.40f, Damage = 12f, Stagger = 35f, StaminaCost = 14f, ManaGain = 0f, Lunge = 0f, Reach = 7.0f, LineWidth = 1.2f, HitStopSeconds = 0.10f, Shake = 0.20f, Binds = 2.5f, Chain = ChainThrow.Lane },
                    Finisher = new AttackStep
                    {
                        // The blow is made backward, on an enemy behind: it reaches all round.
                        Name = "attack_sickle_finisher", Heavy = true, Sweep = -1f, Windup = 0.66f, Active = 0.12f, Recovery = 0.60f, Damage = 28f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.4f, ArcDegrees = 360f, HitStopSeconds = 0.22f, Shake = 0.50f,
                        LungeSeconds = 0.10f, HoldAt = 0.50f, HoldSeconds = 0.16f,
                        // The weight wraps, and the bearer turns the back and walks the chain out to its length; two heaves bring the enemy in behind.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.16f, HitStopSeconds = 0.04f, Shake = 0.08f, Effect = VfxKind.HitSpark, DashBeyond = 2.6f, DashBearing = 180f, DashSeconds = 0.30f },
                            new FinisherBeat { Time = 0.30f, HitStopSeconds = 0.02f, Shake = 0.06f, Effect = VfxKind.Dust, Shove = new Vector3(0f, 0f, 0.7f) },
                            new FinisherBeat { Time = 0.42f, HitStopSeconds = 0.02f, Shake = 0.08f, Effect = VfxKind.Dust, Shove = new Vector3(0f, 0f, 0.7f) },
                        },
                    },
                    SkillManaCost = 30f,
                    ChargeTime = 0.40f,
                },
            };
            // The carrier: the weight snapped out along a lane.
            return Carried(weapon, w => w.Combo.Steps[1]).Named("sickle", false);
        }

        // An arrow off the string: it stays in what it strikes, or stands in the ground where its flight ends.
        private static WeaponShot Arrow(float speed, float range, float radius, float damage, float stagger, bool pierce, float manaGain, float releaseAfter = 0f) => new WeaponShot
        {
            Speed = speed, Range = range, Radius = radius, Damage = damage, Stagger = stagger, Pierce = pierce, Heavy = true,
            Plants = true, ManaGain = manaGain, ReleaseAfter = releaseAfter, Prop = "arrow", PropLength = 1.1f,
        };

        /// <summary>
        /// The Great Bow: a war bow taller than its bearer, shod with horn for striking. Its arrows stay in what they
        /// hit, and every blow of the stave snaps the arrows standing in its target: step off and plant, step in and
        /// cash. A swat and a stab with the arrow in the fist (which leaves it there); a snap shot; a kick that throws
        /// the bearer back with a shot loosed in the air; a full draw that goes through a line of enemies; and
        /// Landmines, five arrows dropped in a ring. Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon GreatBow()
        {
            var weapon = new Weapon
            {
                Name = GreatBowName,
                Carry = "bow",
                SkillName = "Landmines",
                SlashColour = new Color(1.8f, 1.7f, 1.3f, 0.85f),
                HandSocket = WeaponRack.HandLeft,
                OffHandSocket = WeaponRack.HandRight,
                AimRange = 12f,
                // Narrow: at twelve metres the usual wide cone would turn a shot to whoever stood nearest off to one side.
                AimConeDegrees = 60f,
                // A perfect guard drives the arrow in the right fist into the attacker.
                PerfectGuardPlants = 1,
                GuardProfile = new Guard { PerfectWindow = 0.18f, StaminaPerDamage = 1.15f, ChipFraction = 0.4f, ArcDegrees = 180f },
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        // The clip winds the stave to the right hip and sweeps it out to the left, so the sweep is +1 (the design's table says -1 and its text describes this).
                        new AttackStep { Name = "attack_bow_1", Sweep = 1f, Windup = 0.13f, Active = 0.07f, Recovery = 0.30f, Damage = 9f, Stagger = 14f, StaminaCost = 9f, ManaGain = 5f, Lunge = 0.6f, Reach = 2.6f, ArcDegrees = 130f, HitStopSeconds = 0.045f, SnapsArrows = true },
                        new AttackStep { Name = "attack_bow_2", Windup = 0.20f, Active = 0.08f, Recovery = 0.38f, Damage = 12f, Stagger = 10f, StaminaCost = 11f, ManaGain = 6f, Lunge = 0.9f, Reach = 2.2f, ArcDegrees = 50f, HitStopSeconds = 0.06f, SnapsArrows = true, PlantsArrows = 1 },
                    },
                    Heavy = new AttackStep { Name = "attack_bow_heavy", Heavy = true, Windup = 0.32f, Active = 0.06f, Recovery = 0.40f, Damage = 0f, Stagger = 0f, StaminaCost = 16f, ManaGain = 0f, Lunge = 0f, Reach = 0f, ArcDegrees = 0f, HitStopSeconds = 0.07f, Shake = 0.08f, Recoil = 0.3f, Shot = Arrow(30f, 9f, 0.4f, 18f, 22f, false, 8f) },
                    // The kick lands and snaps; the shot leaves from the far end of the spring back.
                    ChainHeavy = new AttackStep { Name = "attack_bow_chain", Heavy = true, Windup = 0.16f, Active = 0.08f, Recovery = 0.44f, Damage = 6f, Stagger = 12f, StaminaCost = 16f, ManaGain = 8f, Lunge = 0.3f, Reach = 1.8f, ArcDegrees = 80f, HitStopSeconds = 0.08f, Shake = 0.12f, Recoil = 3.0f, SnapsArrows = true, Shot = Arrow(30f, 7f, 0.4f, 20f, 24f, false, 0f, 0.2f) },
                    ChargedHeavy = new AttackStep { Name = "attack_bow_charged", Heavy = true, Windup = 0.06f, Active = 0.08f, Recovery = 0.50f, Damage = 0f, Stagger = 0f, StaminaCost = 24f, ManaGain = 0f, Lunge = 0f, Reach = 0f, ArcDegrees = 0f, HitStopSeconds = 0.11f, Shake = 0.22f, Recoil = 0.8f, Shot = Arrow(40f, 14f, 0.5f, 34f, 44f, true, 12f) },
                    Skill = new AttackStep { Name = "attack_bow_skill", Heavy = true, Windup = 0.26f, Active = 0.08f, Recovery = 0.46f, Damage = 0f, Stagger = 0f, StaminaCost = 15f, ManaGain = 0f, Lunge = 0f, Reach = 0f, ArcDegrees = 0f, HitStopSeconds = 0.08f, Shake = 0.16f, Effect = StepEffect.ArrowRain },
                    Finisher = new AttackStep
                    {
                        Name = "attack_bow_finisher", Heavy = true, Windup = 0.80f, Active = 0.10f, Recovery = 0.60f, Damage = 30f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 2.6f, ArcDegrees = 120f, HitStopSeconds = 0.24f, Shake = 0.55f,
                        LungeSeconds = 0.10f, HoldAt = 0.36f, HoldSeconds = 0.40f, Effect = StepEffect.GroundSlam, SnapsArrows = true,
                        // The lower limb takes the legs, then the upper comes over the top and knocks the enemy down.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.10f, HitStopSeconds = 0.03f, Shake = 0.06f, Effect = VfxKind.HitSpark, Shove = new Vector3(0.12f, 0f, 0f) },
                            new FinisherBeat { Time = 0.18f, HitStopSeconds = 0.05f, Shake = 0.16f, Effect = VfxKind.HeavySpark, Shove = new Vector3(0f, 0f, 0.15f) },
                        },
                    },
                    SkillManaCost = 35f,
                    ChargeTime = 0.60f,
                    SkillChargeTime = 1.00f,
                    ChargeMoveFactor = 0.35f,
                },
            };
            // The carrier: the shot, whose arrow stays.
            return Carried(weapon, w => w.Combo.Heavy).Named("bow", false);
        }

        /// <summary>
        /// The Full Body Armament: what bare hands become when the held element coats them. The fists' four blows to
        /// the hundredth, then a choice of four enders by where the chain is stopped (ram, rising blow, slam, burst),
        /// Meteor Fist from a short hold, and Energy Reflect: a stance that takes a blow and throws it back.
        /// Numbers from docs/design/weapons.md.
        /// </summary>
        public static Weapon Armament(string element)
        {
            var info = Elements.ByName(element);
            Color colour = info != null ? info.Colour : new Color(0.8f, 0.8f, 0.85f);
            var weapon = new Weapon
            {
                Name = ArmamentName,
                HudName = info != null ? info.Name + " Armament" : "Armament",
                Carry = "arm",
                SkillName = "Energy Reflect",
                SlashColour = new Color(colour.r * 2f, colour.g * 2f, colour.b * 2f, 0.9f),
                ElementColoured = true,
                GuardProfile = new Guard { PerfectWindow = 0.18f, StaminaPerDamage = 0.8f, ChipFraction = 0.2f, ArcDegrees = 200f },
                Combo = new WeaponCombo
                {
                    Steps = new List<AttackStep>
                    {
                        new AttackStep { Name = "attack_arm_1", Windup = 0.07f, Active = 0.06f, Recovery = 0.17f, Damage = 8f, Stagger = 10f, StaminaCost = 7f, ManaGain = 4f, Lunge = 0.5f, Reach = 2.1f, ArcDegrees = 90f, HitStopSeconds = 0.035f },
                        new AttackStep { Name = "attack_arm_2", Windup = 0.08f, Active = 0.06f, Recovery = 0.18f, Damage = 9f, Stagger = 11f, StaminaCost = 7f, ManaGain = 4f, Lunge = 0.5f, Reach = 2.1f, ArcDegrees = 90f, HitStopSeconds = 0.035f },
                        new AttackStep { Name = "attack_arm_3", Sweep = 1f, Windup = 0.11f, Active = 0.07f, Recovery = 0.22f, Damage = 11f, Stagger = 15f, StaminaCost = 9f, ManaGain = 5f, Lunge = 0.6f, Reach = 2.2f, ArcDegrees = 150f, HitStopSeconds = 0.05f },
                        new AttackStep { Name = "attack_arm_4", Sweep = -1f, Windup = 0.15f, Active = 0.09f, Recovery = 0.36f, Damage = 16f, Stagger = 26f, StaminaCost = 12f, ManaGain = 8f, Lunge = 1.1f, Reach = 2.5f, ArcDegrees = 150f, HitStopSeconds = 0.08f, Shake = 0.10f },
                    },
                    Heavy = new AttackStep { Name = "attack_arm_heavy", Heavy = true, Windup = 0.24f, Active = 0.09f, Recovery = 0.40f, Damage = 22f, Stagger = 36f, StaminaCost = 16f, ManaGain = 9f, Lunge = 1.8f, Reach = 2.3f, ArcDegrees = 80f, HitStopSeconds = 0.09f, Shake = 0.16f, Poise = 16f },
                    ChainHeavies = new List<AttackStep>
                    {
                        null,
                        new AttackStep { Name = "attack_arm_ram", Heavy = true, Windup = 0.12f, Active = 0.14f, Recovery = 0.36f, Damage = 14f, Stagger = 30f, StaminaCost = 14f, ManaGain = 8f, Lunge = 0f, Reach = 1.6f, ArcDegrees = 120f, HitStopSeconds = 0.08f, Shake = 0.16f, Travel = 2.2f, Pull = -2.5f, Poise = 22f },
                        new AttackStep { Name = "attack_arm_rising", Heavy = true, Windup = 0.15f, Active = 0.10f, Recovery = 0.44f, Damage = 22f, Stagger = 46f, StaminaCost = 16f, ManaGain = 11f, Lunge = 0.8f, Reach = 2.2f, ArcDegrees = 100f, HitStopSeconds = 0.10f, Shake = 0.18f },
                        new AttackStep { Name = "attack_arm_slam", Heavy = true, Windup = 0.20f, Active = 0.10f, Recovery = 0.46f, Damage = 20f, Stagger = 34f, StaminaCost = 16f, ManaGain = 10f, Lunge = 0.3f, Reach = 0.8f, Radius = 2.2f, HitStopSeconds = 0.10f, Shake = 0.24f, Effect = StepEffect.ElementRing },
                        new AttackStep { Name = "attack_arm_burst", Heavy = true, Windup = 0.18f, Active = 0.12f, Recovery = 0.52f, Damage = 28f, Stagger = 40f, StaminaCost = 18f, ManaGain = 12f, Lunge = 0.2f, Reach = 4.2f, ArcDegrees = 70f, HitStopSeconds = 0.12f, Shake = 0.28f, Effect = StepEffect.ElementCone },
                    },
                    ChargedHeavy = new AttackStep { Name = "attack_arm_charged", Heavy = true, Windup = 0.10f, Active = 0.12f, Recovery = 0.50f, Damage = 28f, Stagger = 52f, StaminaCost = 18f, ManaGain = 10f, Lunge = 2.6f, Reach = 2.5f, ArcDegrees = 90f, HitStopSeconds = 0.13f, Shake = 0.28f, Poise = 26f },
                    Skill = new AttackStep { Name = "attack_arm_skill", Heavy = true, Windup = 0.04f, Active = 0.12f, Recovery = 0.44f, Damage = 20f, Stagger = 40f, StaminaCost = 0f, ManaGain = 0f, Lunge = 0f, Reach = 5.0f, ArcDegrees = 90f, HitStopSeconds = 0.16f, Shake = 0.40f, Effect = StepEffect.ElementCone },
                    Finisher = new AttackStep
                    {
                        Name = "attack_arm_finisher", Heavy = true, Windup = 0.72f, Active = 0.12f, Recovery = 0.56f, Damage = 26f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 2.8f, ArcDegrees = 120f, HitStopSeconds = 0.22f, Shake = 0.55f,
                        LungeSeconds = 0.08f, HoldAt = 0.54f, HoldSeconds = 0.18f, Effect = StepEffect.ElementCone,
                        // Six fast blows, even as a drum roll, each driving the enemy a little back; then no blow at all.
                        Beats = new[]
                        {
                            new FinisherBeat { Time = 0.08f, HitStopSeconds = 0.03f, Shake = 0.04f, Shove = new Vector3(0f, 0f, 0.06f) },
                            new FinisherBeat { Time = 0.17f, HitStopSeconds = 0.03f, Shake = 0.04f, Shove = new Vector3(0f, 0f, 0.06f) },
                            new FinisherBeat { Time = 0.26f, HitStopSeconds = 0.03f, Shake = 0.05f, Shove = new Vector3(0f, 0f, 0.06f) },
                            new FinisherBeat { Time = 0.35f, HitStopSeconds = 0.03f, Shake = 0.05f, Shove = new Vector3(0f, 0f, 0.06f) },
                            new FinisherBeat { Time = 0.44f, HitStopSeconds = 0.03f, Shake = 0.06f, Shove = new Vector3(0f, 0f, 0.06f) },
                            new FinisherBeat { Time = 0.53f, HitStopSeconds = 0.03f, Shake = 0.07f, Shove = new Vector3(0f, 0f, 0.06f) },
                        },
                    },
                    SkillManaCost = 25f,
                    ChargeTime = 0.30f,
                    SkillIsCounter = true,
                    CounterHoldMax = 1.2f,
                },
            };
            // The coat lays one stack of its element's affliction with every blow, and the burst two.
            foreach (var step in weapon.AllSteps()) step.ElementStacks = 1;
            weapon.Combo.ChainHeavies[4].ElementStacks = 2;
            // The carrier: the fourth blow, the kick of the bare hands.
            return Carried(weapon, w => w.Combo.Steps[3]).Named("arm", false);
        }

        /// <summary>
        /// Bare hands, the first run's weapon. Short reach and light blows, but the quickest chain in the game and a
        /// great deal of stagger for its damage: fists open an enemy up for the finisher. Four lights (jab, cross, hook,
        /// kick), a lunging heavy, a rising chain heavy, and the Breaker: a charged straight that bursts the stagger meter.
        /// </summary>
        public static Weapon Fists() => Carried(new Weapon
        {
            Name = FistsName,
            LocomotionState = "locomotion_fist",
            GuardState = "guard_fist",
            GuardHitState = "guard_hit_fist",
            PerfectGuardState = "guard_perfect_fist",
            Prefix = "fist",
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
        }, w => w.Combo.Steps[3]); // the carrier: the kick, the fourth blow

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
        public static Weapon Katana() => Carried(new Weapon
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
                // The held pose behind the enemy once the blow is in (way_build_plan.md question 5): data until the engine
                // holds it: PlayerCombat stands the step's clock still for PoseHold once the blow is in.
                Finisher = new AttackStep { Name = "attack_finisher", Heavy = true, Windup = 0.30f, Active = 0.14f, Recovery = 0.56f, Damage = 30f, Stagger = 0f, StaminaCost = 0f, ManaGain = 15f, Lunge = 0f, Reach = 3.2f, ArcDegrees = 120f, HitStopSeconds = 0.2f, Shake = 0.45f, HoldAt = 0.44f, HoldSeconds = Way.Temper.FinisherHold, PoseHold = Way.Temper.FinisherHold },
                Skill = new AttackStep { Name = "attack_iaido", Sweep = -1f, Heavy = true, Windup = 0.08f, Active = 0.12f, Recovery = 0.50f, Damage = 48f, Stagger = 50f, StaminaCost = 15f, ManaGain = 0f, Lunge = 2.6f, Reach = 3.3f, ArcDegrees = 170f, HitStopSeconds = 0.14f, Shake = 0.3f },
                SkillManaCost = 30f,
                ChargeTime = 0.4f,
            },
        }, w => w.Combo.Steps[0], DrawingCut);

        // The Katana's carrier is the drawing cut made at once and wider (steel/weapons.md, the Katana's Way).
        private static void DrawingCut(AttackStep step)
        {
            step.Windup = 0.18f;
            step.Damage = 22f;
            step.Stagger = 20f;
            step.ArcDegrees = 170f;
            step.Lunge = 1.6f;
            step.Reach = 2.6f;
        }

        /// <summary>
        /// Gives a weapon the carrier of its infused strike (IMPLEMENTATION.md 3.4): a copy of one of its own moves, so
        /// what the strike changes never reaches the chain, and a god's weapon built on it scales the copy too.
        /// </summary>
        private static Weapon Carried(Weapon weapon, Func<Weapon, AttackStep> pick, Action<AttackStep> tune = null)
        {
            var from = pick(weapon);
            if (from == null) return weapon;
            weapon.Carrier = from.Clone();
            tune?.Invoke(weapon.Carrier);
            return weapon;
        }
    }
}
