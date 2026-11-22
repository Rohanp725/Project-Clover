using Clover.Player.Trees.Ways;

namespace Clover.Player.Way
{
    /// <summary>
    /// The technique on the chord, R1 and R2 together (steel/weapons.md 0.5; way_build_plan.md 2.3; task 5.3): one a
    /// weapon, sealed until its elite falls, then 2 Memories, working in every aspect. The armoury puts the technique in
    /// force on the weapon it builds (<see cref="Weapon.Technique"/>) and the player's chord plays it; with none in force
    /// the chord stays a heavy, as before. The Fists, taught twice, play the one set (the first bought sets itself,
    /// question 39). Plain and stateless; a fresh step each time, so nothing leaks between weapons.
    /// </summary>
    public static class Techniques
    {
        // Before the Thunder (way_build_plan.md 3.1 and question 7, the owner's answer: lands in 0.06 s, 22 damage, 15
        // stamina, no mana). (P) marks the plan's proposed numbers.
        public const float ThunderWindup = 0.06f;
        public const float ThunderActive = 0.08f;
        /// <summary>(P) The long recovery when it strikes nothing that was winding up.</summary>
        public const float ThunderRecovery = 0.6f;
        /// <summary>(P) The recovery when it broke a wind-up.</summary>
        public const float ThunderBrokeRecovery = 0.3f;
        public const float ThunderDamage = 22f;
        /// <summary>(P) Its stagger as a plain draw, as the drawing cut's.</summary>
        public const float ThunderStagger = 10f;
        /// <summary>The stagger an enemy takes when its wind-up is broken.</summary>
        public const float ThunderBreakStagger = 50f;
        public const float ThunderStamina = 15f;
        public const float ThunderReach = 2.8f, ThunderArc = 120f, ThunderLunge = 0.8f;
        /// <summary>(P) The hit stop on a broken wind-up.</summary>
        public const float ThunderBreakHitStop = 0.12f;
        /// <summary>The clip it is to have; until it is made, the drawing cut's plays.</summary>
        public const string ThunderClip = "attack_katana_before_thunder";

        /// <summary>
        /// Before the Thunder's step: no step back, the body drops and the blade is out in 0.06 s, a 0.8 m lunge, one flat
        /// cut. NEEDS: its own clip (<see cref="ThunderClip"/>; the drawing cut's is played at these timings, a stand-in)
        /// and the broken wind-up (the enemy's attack cancelled, <see cref="ThunderBreakStagger"/>, the shorter recovery).
        /// </summary>
        public static AttackStep BeforeTheThunder() => new AttackStep
        {
            Name = "attack_light_1",
            Sweep = -1f,
            Windup = ThunderWindup,
            Active = ThunderActive,
            Recovery = ThunderRecovery,
            Damage = ThunderDamage,
            Stagger = ThunderStagger,
            StaminaCost = ThunderStamina,
            ManaGain = 5f,
            Lunge = ThunderLunge,
            Reach = ThunderReach,
            ArcDegrees = ThunderArc,
            HitStopSeconds = 0.045f,
        };

        /// <summary>The technique mark of a weapon kind's Way, or null for a Way not written yet (the Great Spear's Hold the Gate waits on its Way).</summary>
        public static string MarkOf(string kind) => kind == Weapons.KatanaName ? KatanaWay.BeforeTheThunder : null;

        /// <summary>The chord's step for a weapon kind under the marks in force: its technique when bought (and so past its gate), else null.</summary>
        public static AttackStep StepFor(string kind, IMarkQuery marks)
        {
            string mark = MarkOf(kind);
            if (mark == null || marks == null || !marks.Has(mark)) return null;
            return kind == Weapons.KatanaName ? BeforeTheThunder() : null;
        }
    }
}
