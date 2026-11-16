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
        };

        /// <summary>The names a stand or a rack may offer, in the order they are set out. The god's own sword is given, not chosen.</summary>
        public static readonly string[] Offered = { FistsName, KatanaName, MassiveSwordName, ScytheName, ArmamentName, GreatSpearName, GreatHammerName };

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
            return weapon.Named("gsword", true);
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
            return weapon.Named("scythe", false);
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
            return weapon.Named("spear", false);
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
            return weapon.Named("hammer", true);
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
            return weapon.Named("arm", false);
        }

        /// <summary>
        /// Bare hands, the first run's weapon. Short reach and light blows, but the quickest chain in the game and a
        /// great deal of stagger for its damage: fists open an enemy up for the finisher. Four lights (jab, cross, hook,
        /// kick), a lunging heavy, a rising chain heavy, and the Breaker: a charged straight that bursts the stagger meter.
        /// </summary>
        public static Weapon Fists() => new Weapon
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
