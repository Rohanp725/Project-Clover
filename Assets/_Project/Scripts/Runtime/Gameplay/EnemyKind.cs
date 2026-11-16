using System.Collections.Generic;

namespace Clover.Gameplay
{
    /// <summary>
    /// The ordinary enemies of the lightning castle and their numbers. Each teaches one thing: the soldier's long,
    /// plainly shown blows teach the guard; the acolyte's quick flurries and leaps teach the dodge; the caster's bolts
    /// from range teach closing the distance. Attack names are also the names of the animation states that show them.
    /// </summary>
    public class EnemyKind
    {
        public string Name;
        /// <summary>The start of this kind's animation names, as in "soldier_idle_loop".</summary>
        public string Prefix;
        public float Health;
        public float StaggerMaximum;
        public float WalkSpeed;
        public float TurnDegreesPerSecond;
        /// <summary>Stagger the enemy takes when its blow is perfectly guarded.</summary>
        public float PerfectGuardStagger;
        /// <summary>
        /// True for an enemy that guards: a light blow from the front, while it is not attacking, is turned on its
        /// weapon. It takes a fraction of the damage but all of the stagger, so its guard is beaten by breaking its
        /// balance, by a heavy blow, by fire, or by getting behind it.
        /// </summary>
        public bool Guards;
        public float GuardedDamage = 0.25f;
        /// <summary>After turning this many blows it strikes back at once.</summary>
        public int BlocksBeforeCounter = 3;
        /// <summary>Light blows it shrugs off before one makes it flinch. 1 means every blow does.</summary>
        public int PoiseHits = 1;
        public EnemyBrain Brain;

        public const string SoldierName = "Temple Soldier";
        public const string AcolyteName = "Knife Acolyte";
        public const string CasterName = "Storm Caller";

        public static EnemyKind ByName(string name) =>
            name == AcolyteName ? Acolyte() : name == CasterName ? Caster() : Soldier();

        public static IEnumerable<EnemyKind> All
        {
            get
            {
                yield return Soldier();
                yield return Acolyte();
                yield return Caster();
            }
        }

        public static EnemyKind Soldier() => new EnemyKind
        {
            Name = SoldierName,
            Prefix = "soldier",
            Health = 95f,
            StaggerMaximum = 90f,
            WalkSpeed = 2.7f,
            TurnDegreesPerSecond = 220f,
            PerfectGuardStagger = 55f,
            Guards = true,
            BlocksBeforeCounter = 3,
            PoiseHits = 3,
            Brain = new EnemyBrain
            {
                AggroRange = 11f,
                PreferredRange = 3.2f,
                WaitRange = 6f,
                RestMin = 0.5f,
                RestMax = 1.1f,
                Attacks = new List<EnemyAttack>
                {
                    // A straight thrust down a narrow lane: long reach, easy to sidestep, easy to guard.
                    new EnemyAttack { Name = "soldier_thrust", Telegraph = 0.65f, Strike = 0.15f, Recovery = 0.6f, Damage = 20f, Reach = 4.2f, ArcDegrees = 24f, Lunge = 0.9f, MinRange = 0f, MaxRange = 4.4f },
                    // A wide sweep that punishes circling close.
                    new EnemyAttack { Name = "soldier_sweep", Telegraph = 0.85f, Strike = 0.2f, Recovery = 0.75f, Damage = 24f, Reach = 3.4f, ArcDegrees = 170f, Sweep = -1f, MinRange = 0f, MaxRange = 3.2f, Cooldown = 2.5f },
                    // The red one: a long charging lunge from distance that a held guard will not stop.
                    new EnemyAttack { Name = "soldier_lunge", Telegraph = 1.0f, Strike = 0.2f, Recovery = 0.9f, Damage = 30f, Reach = 4.6f, ArcDegrees = 26f, Lunge = 3.2f, MinRange = 3.5f, MaxRange = 8f, Unblockable = true, Cooldown = 6f },
                },
            },
        };

        public static EnemyKind Acolyte() => new EnemyKind
        {
            Name = AcolyteName,
            Prefix = "acolyte",
            Health = 55f,
            StaggerMaximum = 55f,
            WalkSpeed = 4.4f,
            TurnDegreesPerSecond = 420f,
            PerfectGuardStagger = 55f,
            PoiseHits = 2,
            Brain = new EnemyBrain
            {
                WaitRange = 5.5f,
                AggroRange = 12f,
                PreferredRange = 1.9f,
                RestMin = 0.35f,
                RestMax = 0.9f,
                FlinchSeconds = 0.25f,
                Attacks = new List<EnemyAttack>
                {
                    // Two cuts with barely a breath between them: guarding the first leaves no stamina to spare for the second.
                    new EnemyAttack { Name = "acolyte_slash_1", Telegraph = 0.32f, Strike = 0.1f, Recovery = 0.2f, Damage = 10f, Reach = 2.2f, ArcDegrees = 130f, Sweep = 1f, Lunge = 0.5f, MinRange = 0f, MaxRange = 2.4f, ChainInto = "acolyte_slash_2" },
                    new EnemyAttack { Name = "acolyte_slash_2", Telegraph = 0.22f, Strike = 0.1f, Recovery = 0.5f, Damage = 12f, Reach = 2.2f, ArcDegrees = 130f, Sweep = -1f, Lunge = 0.5f, MinRange = 1f, MaxRange = 0f },
                    // The red one: a leap from range that lands with both knives.
                    new EnemyAttack { Name = "acolyte_leap", Telegraph = 0.6f, Strike = 0.15f, Recovery = 0.7f, Damage = 22f, Reach = 2.4f, ArcDegrees = 70f, Lunge = 4.2f, MinRange = 3.5f, MaxRange = 7.5f, Unblockable = true, Cooldown = 4f },
                },
            },
        };

        public static EnemyKind Caster() => new EnemyKind
        {
            Name = CasterName,
            Prefix = "caster",
            Health = 45f,
            StaggerMaximum = 40f,
            WalkSpeed = 2.4f,
            TurnDegreesPerSecond = 300f,
            PerfectGuardStagger = 40f,
            Brain = new EnemyBrain
            {
                AggroRange = 15f,
                PreferredRange = 9f,
                RetreatRange = 5f,
                RestMin = 0.8f,
                RestMax = 1.6f,
                Attacks = new List<EnemyAttack>
                {
                    // A bolt called down on where the player stands: keep moving, or better, get close.
                    new EnemyAttack { Name = "caster_bolt", Telegraph = 1.1f, Strike = 0.2f, Recovery = 0.6f, Damage = 22f, Radius = 1.9f, Ranged = true, MinRange = 3.5f, MaxRange = 15f, Unblockable = true },
                    // Cornered, it bursts: a ring of lightning around itself.
                    new EnemyAttack { Name = "caster_nova", Telegraph = 0.8f, Strike = 0.2f, Recovery = 0.8f, Damage = 18f, Radius = 3f, MinRange = 0f, MaxRange = 3.2f, Cooldown = 3f },
                },
            },
        };
    }
}
