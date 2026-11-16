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
        /// <summary>The model file's name without its extension; "enemy_" and the prefix when left empty.</summary>
        public string Model = "";
        public string ModelName => string.IsNullOrEmpty(Model) ? "enemy_" + Prefix : Model;
        /// <summary>How large the model is drawn.</summary>
        public float Scale = 1f;
        /// <summary>A boss has its name and bars across the bottom of the screen.</summary>
        public bool IsBoss;
        public string Title = "";
        /// <summary>Above zero, a finisher takes this share of the enemy's whole health, whatever the blow.</summary>
        public float FinisherHealthShare;
        /// <summary>The share of health at which the fight enters its second phase; zero for never.</summary>
        public float SecondPhaseAt;
        /// <summary>The attack it makes at once on entering the second phase.</summary>
        public string SecondPhaseAttack = "";
        /// <summary>How much faster it acts in the second phase.</summary>
        public float SecondPhasePace = 1f;
        public float StaggeredSeconds = 3.2f;

        public const string SoldierName = "Temple Soldier";
        public const string AcolyteName = "Knife Acolyte";
        public const string CasterName = "Storm Caller";
        public const string NarukamiName = "Narukami";

        public static EnemyKind ByName(string name) =>
            name == AcolyteName ? Acolyte() : name == CasterName ? Caster() : name == NarukamiName ? Narukami() : Soldier();

        public static IEnumerable<EnemyKind> All
        {
            get
            {
                yield return Soldier();
                yield return Acolyte();
                yield return Caster();
                yield return Narukami();
            }
        }

        public static EnemyKind Soldier() => new EnemyKind
        {
            Name = SoldierName,
            Prefix = "soldier",
            Scale = 1.12f,
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
            Scale = 0.95f,
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
            Scale = 1.03f,
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

        /// <summary>
        /// Narukami, the god of Lightning: once a swordsman, now the storm. A duel, not a brawl. He guards and answers,
        /// his cuts come in threes, and every so often he steps through the lightning to be somewhere else. Almost all of
        /// it can be guarded, and a perfect guard is the surest way to break him; the red ones must be stepped out of.
        /// Below half his health the storm comes down with him.
        /// </summary>
        public static EnemyKind Narukami() => new EnemyKind
        {
            Name = NarukamiName,
            Title = "God of Lightning",
            Prefix = "boss",
            Model = "boss_narukami",
            Scale = 1.22f,
            IsBoss = true,
            Health = 620f,
            StaggerMaximum = 190f,
            StaggeredSeconds = 2.8f,
            WalkSpeed = 3.4f,
            TurnDegreesPerSecond = 420f,
            PerfectGuardStagger = 42f,
            Guards = true,
            GuardedDamage = 0.2f,
            BlocksBeforeCounter = 2,
            PoiseHits = 4,
            FinisherHealthShare = 0.2f,
            SecondPhaseAt = 0.5f,
            SecondPhaseAttack = "boss_roar",
            SecondPhasePace = 1.18f,
            Brain = new EnemyBrain
            {
                AggroRange = 16f,
                LeashRange = 80f,
                PreferredRange = 3f,
                RestMin = 0.45f,
                RestMax = 1.1f,
                FlinchSeconds = 0.25f,
                Attacks = new List<EnemyAttack>
                {
                    // Three cuts: across, back, and down. Each can be guarded; guarding all three perfectly nearly breaks him.
                    new EnemyAttack { Name = "boss_slash_1", Telegraph = 0.46f, Strike = 0.12f, Recovery = 0.12f, Damage = 16f, Reach = 3.4f, ArcDegrees = 150f, Sweep = -1f, Lunge = 0.9f, MinRange = 0f, MaxRange = 3.6f, ChainInto = "boss_slash_2" },
                    new EnemyAttack { Name = "boss_slash_2", Telegraph = 0.3f, Strike = 0.12f, Recovery = 0.12f, Damage = 16f, Reach = 3.4f, ArcDegrees = 150f, Sweep = 1f, Lunge = 0.9f, MinRange = 1f, MaxRange = 0f, ChainInto = "boss_slash_3" },
                    new EnemyAttack { Name = "boss_slash_3", Telegraph = 0.5f, Strike = 0.15f, Recovery = 0.75f, Damage = 24f, Reach = 3.8f, LineWidth = 1.5f, Lunge = 1.3f, MinRange = 1f, MaxRange = 0f },
                    // A quick thrust down a lane, to catch a player hanging back at middle range.
                    new EnemyAttack { Name = "boss_thrust", Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.6f, Damage = 20f, Reach = 5f, LineWidth = 1.1f, Lunge = 1.8f, MinRange = 2.6f, MaxRange = 5.4f, Cooldown = 3f },
                    // The red draw: sheathed, sunk, then through the player along a long lane and out the other side.
                    new EnemyAttack { Name = "boss_iai", Telegraph = 1.0f, Strike = 0.15f, Recovery = 1.0f, Damage = 34f, Reach = 10.5f, LineWidth = 1.7f, Lunge = 9.5f, PassThrough = true, Unblockable = true, MinRange = 4f, MaxRange = 12f, Cooldown = 7f, AimLockAt = 0.75f },
                    // He steps through the lightning to the player's flank and cuts as he arrives.
                    new EnemyAttack { Name = "boss_blink_cut", Animation = "boss_slash_2", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.6f, Damage = 22f, Reach = 3.4f, ArcDegrees = 190f, Sweep = 1f, BlinkTo = 2.4f, MinRange = 5f, MaxRange = 16f, Cooldown = 5f },
                    // Three bolts called down one after another on where the player stands: keep moving.
                    new EnemyAttack { Name = "boss_bolt_1", Animation = "boss_call", Telegraph = 0.85f, Strike = 0.1f, Recovery = 0.05f, Damage = 20f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 6f, MaxRange = 20f, Cooldown = 9f, ChainInto = "boss_bolt_2" },
                    new EnemyAttack { Name = "boss_bolt_2", Animation = "boss_call_again", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.05f, Damage = 20f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, ChainInto = "boss_bolt_3" },
                    new EnemyAttack { Name = "boss_bolt_3", Animation = "boss_call_again", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.7f, Damage = 20f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 1f, MaxRange = 0f },
                    // Crowded, he bursts: a ring of lightning all round him.
                    new EnemyAttack { Name = "boss_nova", Telegraph = 0.7f, Strike = 0.2f, Recovery = 0.8f, Damage = 22f, Radius = 3.8f, MinRange = 0f, MaxRange = 2.6f, Cooldown = 7f },
                    // The cry that opens the second phase: wider than the burst, and it cannot be guarded.
                    new EnemyAttack { Name = "boss_roar", Telegraph = 1.1f, Strike = 0.25f, Recovery = 0.9f, Damage = 26f, Radius = 6f, Unblockable = true, MinRange = 1f, MaxRange = 0f },
                    // Second phase only: the red draw twice, the second from wherever the first left him.
                    new EnemyAttack { Name = "boss_iai_twice", Animation = "boss_iai", Telegraph = 0.85f, Strike = 0.15f, Recovery = 0.2f, Damage = 30f, Reach = 10.5f, LineWidth = 1.7f, Lunge = 9.5f, PassThrough = true, Unblockable = true, MinRange = 4f, MaxRange = 14f, Cooldown = 10f, MinPhase = 1, AimLockAt = 0.75f, ChainInto = "boss_iai_return" },
                    new EnemyAttack { Name = "boss_iai_return", Animation = "boss_iai", Telegraph = 0.7f, Strike = 0.15f, Recovery = 1.1f, Damage = 30f, Reach = 10.5f, LineWidth = 1.7f, Lunge = 9.5f, PassThrough = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.7f },
                },
            },
        };
    }
}
