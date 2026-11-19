using System.Collections.Generic;

namespace Clover.Gameplay
{
    /// <summary>
    /// The ordinary enemies of the lightning castle and their numbers. Each teaches one thing: the soldier's long,
    /// plainly shown blows teach the guard; the acolyte's quick flurries and leaps teach the dodge; the caster's bolts
    /// from range teach closing the distance. Attack names are also the names of the animation states that show them.
    /// </summary>
    public partial class EnemyKind
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
        /// <summary>The share of a light blow's stagger that comes through its guard.</summary>
        public float GuardedStagger = 1f;
        /// <summary>
        /// The share of a heavy blow's damage that comes through its guard. At 1 a heavy blow goes past the guard as if
        /// it were not there; below 1 the guard meets heavy blows too, and they cost it balance and not much blood.
        /// </summary>
        public float HeavyGuardedDamage = 1f;
        /// <summary>True for an enemy whose attack, once shown, is not broken even by a heavy blow.</summary>
        public bool Steadfast;
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
        /// <summary>An elite stands between the ordinary enemies and a boss: its name is shown over its own bars while it fights.</summary>
        public bool IsElite;
        /// <summary>A god fighting in this body (Narukami; Khepharu as a man): it ranks as a god for the reactions, though ice holds it for a breath (AfflictionSet.IsGod).</summary>
        public bool IsGod;
        /// <summary>What a chain feels on the end of it: a soldier is 1. At 2 and over it is not dragged; whoever pulls goes to it.</summary>
        public float Weight => IsBoss ? 4f : IsElite ? 2f : 1f;
        /// <summary>Embers it gives up the first time it dies in a run.</summary>
        public int Embers = 1;
        /// <summary>False for an enemy the area's storm never picks to charge: its fight keeps the pace it was given.</summary>
        public bool DrawsStorm = true;
        /// <summary>Which set of colours the builder dresses its model in; the shared enemy colours when left empty.</summary>
        public string Palette = "";
        /// <summary>Above zero, a finisher takes this share of the enemy's whole health, whatever the blow.</summary>
        public float FinisherHealthShare;
        /// <summary>The share of health at which the fight enters its second phase; zero for never.</summary>
        public float SecondPhaseAt;
        /// <summary>The attack it makes at once on entering the second phase.</summary>
        public string SecondPhaseAttack = "";
        /// <summary>How much faster it acts in the second phase.</summary>
        public float SecondPhasePace = 1f;
        public float StaggeredSeconds = 3.2f;
        /// <summary>The attack it makes at once, out of turn, when the target starts to drink near it; empty for none.</summary>
        public string SnatchAttack = "";
        /// <summary>Metres within which a drink draws it, and seconds it takes to drink what it took.</summary>
        public float SnatchRange = 8f;
        public float DrinkSeconds = 1.6f;
        /// <summary>For one that grabs: the hold it takes.</summary>
        public float HoldSeconds = 2.5f;
        public float HoldBiteEvery = 0.5f;
        public float HoldBiteDamage = 6f;
        public float HoldPressTakes = 0.4f;
        /// <summary>Metres it is thrown off when the hold ends, seconds it is open after, and the balance it loses when the hold was broken by struggling.</summary>
        public float ThrownOff = 2f;
        public float ThrownOffSeconds = 1f;
        public float StruggleStagger = 20f;
        /// <summary>For one placed dormant: seconds it takes to rise, and how much harder a blow lands before it has risen.</summary>
        public float RiseSeconds = 1.2f;
        public float DormantDamage = 2f;
        /// <summary>True for the sun-blind: it finds the target by light, heat and the noise of a fight, never by sight.</summary>
        public bool HuntsByLight;
        /// <summary>For the sun-blind: its speed with a target found in the sun, the distance it then keeps and backs off under, and seconds it searches where the target was last found.</summary>
        public float LitWalkSpeed;
        public float LitPreferredRange = 7f;
        public float LitRetreatRange = 4f;
        public float SearchSeconds = 3f;
        /// <summary>How much harder a blow lands on one that hunts by light and has not found whoever struck it.</summary>
        public float UnfoundDamage = 2f;
        /// <summary>True for the people of the Fire land: they die in ash and embers, not in lightning.</summary>
        public bool Ashen;

        public const string SoldierName = "Temple Soldier";
        public const string AcolyteName = "Knife Acolyte";
        public const string CasterName = "Storm Caller";
        public const string NarukamiName = "Narukami";
        public const string CaptainName = "Gate Captain";

        public static EnemyKind ByName(string name) =>
            name == AcolyteName ? Acolyte() : name == CasterName ? Caster() : name == NarukamiName ? Narukami()
            : name == CaptainName ? Captain() : FireByName(name) ?? Soldier();

        public static IEnumerable<EnemyKind> All
        {
            get
            {
                yield return Soldier();
                yield return Acolyte();
                yield return Caster();
                yield return Narukami();
                yield return Captain();
                foreach (var kind in FireKinds) yield return kind;
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
            IsGod = true,
            Embers = 5,
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

        /// <summary>
        /// The Gate Captain: the soldier who still keeps the sealed door, a head taller than his men and far deeper in
        /// the lightning. The castle's last fight and the lesson before the god: one man, alone, who waits. His guard
        /// meets light and heavy blows alike, light blows do not make him flinch, and nothing breaks an attack he has
        /// begun, so he is beaten by breaking his balance (perfect guards above all, and heavy blows) and finishing
        /// him, two or three times. Everything he does can be guarded but the long red charge. Below half his health
        /// he is quicker and his attacks run on into one another.
        /// </summary>
        public static EnemyKind Captain() => new EnemyKind
        {
            Name = CaptainName,
            Prefix = "captain",
            Model = "enemy_soldier",
            Palette = "captain",
            // About 1.3 times the soldier, who is himself drawn at 1.12.
            Scale = 1.45f,
            IsElite = true,
            Embers = 3,
            DrawsStorm = false,
            Health = 260f,
            StaggerMaximum = 150f,
            StaggeredSeconds = 3f,
            WalkSpeed = 2.9f,
            TurnDegreesPerSecond = 260f,
            PerfectGuardStagger = 50f,
            Guards = true,
            GuardedDamage = 0.15f,
            GuardedStagger = 0.5f,
            HeavyGuardedDamage = 0.4f,
            Steadfast = true,
            BlocksBeforeCounter = 4,
            PoiseHits = 5,
            FinisherHealthShare = 0.34f,
            SecondPhaseAt = 0.5f,
            SecondPhaseAttack = "captain_slam",
            SecondPhasePace = 1.15f,
            Brain = new EnemyBrain
            {
                AggroRange = 8.5f,
                LeashRange = 22f,
                PreferredRange = 3f,
                RestMin = 0.7f,
                RestMax = 1.4f,
                FlinchSeconds = 0.2f,
                CounterAttack = "captain_jab",
                ImpatientBeyond = 4.5f,
                Attacks = new List<EnemyAttack>
                {
                    // Two thrusts down a lane. The second waits a beat longer than a frightened roll lasts and turns to follow it.
                    new EnemyAttack { Name = "captain_thrust_1", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.1f, Damage = 16f, Reach = 4.4f, LineWidth = 1.1f, Lunge = 0.8f, MinRange = 1.9f, MaxRange = 4.2f, ChainInto = "captain_thrust_2" },
                    new EnemyAttack { Name = "captain_thrust_2", Telegraph = 0.55f, Strike = 0.12f, Recovery = 0.9f, Damage = 22f, Reach = 4.8f, LineWidth = 1.2f, Lunge = 1.4f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f },
                    // A wide level cut that takes in both sides: it punishes circling him.
                    new EnemyAttack { Name = "captain_sweep", Telegraph = 0.8f, Strike = 0.18f, Recovery = 0.8f, Damage = 26f, Reach = 3.8f, ArcDegrees = 200f, Sweep = -1f, MinRange = 0f, MaxRange = 3.4f, Cooldown = 3f },
                    // Too close for too long: a short stab with the spear choked up, quick and light. It is also his answer when he has turned enough blows.
                    new EnemyAttack { Name = "captain_jab", Telegraph = 0.34f, Strike = 0.1f, Recovery = 0.5f, Damage = 12f, Reach = 2.4f, ArcDegrees = 70f, Lunge = 0.4f, MinRange = 0f, MaxRange = 2.6f, Cooldown = 2.5f },
                    // He leaps to where the player stands and brings the spear down: a circle to walk out of, or to guard at a price.
                    new EnemyAttack { Name = "captain_slam", Telegraph = 1.0f, Strike = 0.2f, Recovery = 1.0f, Damage = 30f, Radius = 2.6f, Leap = true, MinRange = 3.6f, MaxRange = 8f, Cooldown = 6f, AimLockAt = 0.5f },
                    // The red one: couched, then the whole length of a long lane and out the far end of it.
                    new EnemyAttack { Name = "captain_charge", Telegraph = 1.1f, Strike = 0.2f, Recovery = 1.1f, Damage = 34f, Reach = 9f, LineWidth = 1.5f, Lunge = 7.5f, PassThrough = true, Unblockable = true, MinRange = 5f, MaxRange = 11f, Cooldown = 7f, AimLockAt = 0.7f },
                    // Second phase only: the cut runs on into a thrust with no rest between.
                    new EnemyAttack { Name = "captain_reap", Animation = "captain_sweep", Telegraph = 0.7f, Strike = 0.18f, Recovery = 0.1f, Damage = 26f, Reach = 3.8f, ArcDegrees = 200f, Sweep = -1f, MinRange = 0f, MaxRange = 3.4f, Cooldown = 5f, MinPhase = 1, ChainInto = "captain_reap_thrust" },
                    new EnemyAttack { Name = "captain_reap_thrust", Animation = "captain_follow", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.95f, Damage = 22f, Reach = 4.8f, LineWidth = 1.2f, Lunge = 1.4f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f },
                    // Second phase only: the red charge twice, the second back along whatever line the first left him on.
                    new EnemyAttack { Name = "captain_charge_twice", Animation = "captain_charge", Telegraph = 0.95f, Strike = 0.2f, Recovery = 0.2f, Damage = 30f, Reach = 9f, LineWidth = 1.5f, Lunge = 7.5f, PassThrough = true, Unblockable = true, MinRange = 5f, MaxRange = 11f, Cooldown = 10f, MinPhase = 1, AimLockAt = 0.7f, ChainInto = "captain_charge_back" },
                    new EnemyAttack { Name = "captain_charge_back", Animation = "captain_charge", Telegraph = 0.85f, Strike = 0.2f, Recovery = 1.2f, Damage = 30f, Reach = 9f, LineWidth = 1.5f, Lunge = 7.5f, PassThrough = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f },
                },
            },
        };
    }
}
