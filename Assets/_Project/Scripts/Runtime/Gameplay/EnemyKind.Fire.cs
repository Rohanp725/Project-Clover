using System.Collections.Generic;

namespace Clover.Gameplay
{
    /// <summary>
    /// The people of the sun-king's city (docs/design/fire_land.md, section 4), each corrupted by fire in its own way:
    /// the thirsting, who come for the flask; the wick, who grabs and holds to share the burning; the gilded soldier
    /// in three ranks, told apart by how much gold is on him; and the sun-blind priest, who hunts by light and shade.
    /// </summary>
    public partial class EnemyKind
    {
        public const string ThirstName = "The Thirsting";
        public const string WickName = "The Wick";
        public const string GildedLevyName = "Gilded Levy";
        public const string GildedSpearmanName = "Gilded Spearman";
        public const string GildedWardenName = "Gilded Warden";
        public const string SunBlindName = "Sun-Blind Priest";
        public const string AshenName = "The Ashen";

        /// <summary>Above zero: killed before its own burst, it bursts where it fell after this many seconds (the ashen).</summary>
        public float BurstsOnDeath;
        /// <summary>The ashen's burst: metres it reaches, what it does to the player and what it does to any other enemy in it.</summary>
        public float BurstRadius = 3.2f;
        public float BurstDamage = 30f;
        public float BurstEnemyDamage = 45f;

        public static IEnumerable<EnemyKind> FireKinds
        {
            get
            {
                yield return Thirsting();
                yield return Wick();
                yield return Gilded(1);
                yield return Gilded(2);
                yield return Gilded(3);
                yield return SunBlind();
                yield return TheAshen();
                yield return Champion();
                yield return SunKing();
            }
        }

        public const string ChampionName = "Champion of the Gate";

        public const string SunKingName = "Khepharu";

        /// <summary>
        /// Khepharu, the sun-king as a man (fire_land.md, section 6.2): a tall thin old man with the sun-spear, who
        /// fights at the far end of it. Three thrusts down a lane, a long reach for someone hanging back, the haft
        /// swept low for someone inside the lane, the butt as his answer, the spear thrown down a red lane and called
        /// back along the same line as a ray, the spear planted so the ground round him burns (which opens his second
        /// half), and three shafts of noon brought down one after another, which cannot be aimed into the shadow of the obelisk.
        /// </summary>
        public static EnemyKind SunKing() => new EnemyKind
        {
            Name = SunKingName,
            Title = "God of Fire",
            Prefix = "sunking",
            Model = "boss_sunking",
            Scale = 1.25f,
            IsBoss = true,
            Embers = 0,
            DrawsStorm = false,
            Ashen = true,
            Health = 480f,
            StaggerMaximum = 170f,
            StaggeredSeconds = 2.8f,
            WalkSpeed = 3.2f,
            TurnDegreesPerSecond = 400f,
            PerfectGuardStagger = 42f,
            Guards = true,
            GuardedDamage = 0.2f,
            BlocksBeforeCounter = 2,
            PoiseHits = 4,
            FinisherHealthShare = 0.2f,
            SecondPhaseAt = 0.5f,
            SecondPhaseAttack = "sunking_plant",
            SecondPhasePace = 1.15f,
            Brain = new EnemyBrain
            {
                AggroRange = 16f,
                LeashRange = 80f,
                PreferredRange = 4.2f,
                RestMin = 0.5f,
                RestMax = 1.1f,
                FlinchSeconds = 0.25f,
                CounterAttack = "sunking_butt",
                Attacks = new List<EnemyAttack>
                {
                    // Three long thrusts. Each can be guarded; the third waits and turns to follow, and leaves him open.
                    new EnemyAttack { Name = "sunking_thrust_1", Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.1f, Damage = 15f, Reach = 5.5f, LineWidth = 1.1f, Lunge = 1f, MinRange = 2f, MaxRange = 5.2f, ChainInto = "sunking_thrust_2", Fiery = true },
                    new EnemyAttack { Name = "sunking_thrust_2", Telegraph = 0.34f, Strike = 0.12f, Recovery = 0.1f, Damage = 15f, Reach = 5.5f, LineWidth = 1.1f, Lunge = 1f, MinRange = 1f, MaxRange = 0f, ChainInto = "sunking_thrust_3", Fiery = true },
                    new EnemyAttack { Name = "sunking_thrust_3", Telegraph = 0.55f, Strike = 0.12f, Recovery = 0.8f, Damage = 22f, Reach = 5.5f, LineWidth = 1.1f, Lunge = 1.6f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f, Fiery = true },
                    // For a player hanging back just past the thrusts: one long lane and a long step behind it.
                    new EnemyAttack { Name = "sunking_reach", Telegraph = 0.55f, Strike = 0.12f, Recovery = 0.6f, Damage = 20f, Reach = 7f, LineWidth = 1f, Lunge = 1.8f, MinRange = 3.5f, MaxRange = 7f, Cooldown = 3f, Fiery = true },
                    // The haft swept low: what he does to someone who has got inside the lane.
                    new EnemyAttack { Name = "sunking_sweep", Telegraph = 0.7f, Strike = 0.18f, Recovery = 0.7f, Damage = 20f, Reach = 3.6f, ArcDegrees = 180f, Sweep = -1f, MinRange = 0f, MaxRange = 3f, Cooldown = 3f, Fiery = true },
                    // Quick and light: it ends a chain on his guard.
                    new EnemyAttack { Name = "sunking_butt", Telegraph = 0.3f, Strike = 0.1f, Recovery = 0.45f, Damage = 10f, Reach = 2.2f, ArcDegrees = 70f, Lunge = 0.3f, MinRange = 0f, MaxRange = 2.4f, Cooldown = 2.5f },
                    // The spear thrown down a long red lane; it stands where the lane ends. Then he is bare-handed, and
                    // calls it back down the same line as a ray of light, wider than the throw was.
                    new EnemyAttack { Name = "sunking_throw", Telegraph = 0.9f, Strike = 0.12f, Recovery = 0.05f, Damage = 26f, Reach = 14f, LineWidth = 1.2f, Unblockable = true, MinRange = 6f, MaxRange = 16f, Cooldown = 8f, AimLockAt = 0.7f, ChainInto = "sunking_ray", Fiery = true },
                    new EnemyAttack { Name = "sunking_ray", Telegraph = 1.8f, Strike = 0.15f, Recovery = 0.6f, Damage = 30f, Reach = 14f, LineWidth = 2.2f, Unblockable = true, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.01f, Fiery = true },
                    // The planted spear: fire comes up round him and the ground goes on burning. It opens his second half.
                    new EnemyAttack { Name = "sunking_plant", Telegraph = 1.1f, Strike = 0.2f, Recovery = 0.9f, Damage = 22f, Radius = 5f, Unblockable = true, MinRange = 0f, MaxRange = 4f, Cooldown = 18f, BurnSeconds = 8f, BurnDamage = 10f, BurnEvery = 0.5f, Fiery = true },
                    // Three shafts of noon, one after another, on where the player stands: keep walking, or stand in the shadow.
                    new EnemyAttack { Name = "sunking_noon_1", Telegraph = 0.85f, Strike = 0.1f, Recovery = 0.05f, Damage = 18f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 6f, MaxRange = 20f, Cooldown = 9f, ChainInto = "sunking_noon_2", Fiery = true },
                    new EnemyAttack { Name = "sunking_noon_2", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.05f, Damage = 18f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, ChainInto = "sunking_noon_3", Fiery = true },
                    new EnemyAttack { Name = "sunking_noon_3", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.7f, Damage = 18f, Radius = 2.1f, Ranged = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, Fiery = true },
                    // Below half: the long third thrust no longer ends in an opening. The sweep follows it with no rest.
                    new EnemyAttack { Name = "sunking_drive", Telegraph = 0.55f, Strike = 0.12f, Recovery = 0.1f, Damage = 22f, Reach = 5.5f, LineWidth = 1.1f, Lunge = 1.6f, MinRange = 2f, MaxRange = 5.2f, Cooldown = 5f, MinPhase = 1, AimLockAt = 0.8f, ChainInto = "sunking_drive_sweep", Fiery = true },
                    new EnemyAttack { Name = "sunking_drive_sweep", Telegraph = 0.7f, Strike = 0.18f, Recovery = 0.75f, Damage = 20f, Reach = 3.6f, ArcDegrees = 180f, Sweep = -1f, MinRange = 1f, MaxRange = 0f, Fiery = true },
                },
            },
        };

        /// <summary>
        /// The Champion of the Gate (fire_land.md, section 5): one of the wholly gilded, cast in gold from mask to soles,
        /// who holds the last gate alone with a spear and shows the god's patterns before the god does. He is fought
        /// as the Gate Captain is: guard, guard perfectly, strike in the opening, break the balance, finish, three
        /// times. Three thrusts down a lane, a wide sweep, the butt as his answer, a long red cast of himself and the
        /// spear down a lane (standing in for the thrown spear and its return), and the planted spear whose ground
        /// burns, which opens his second phase.
        /// </summary>
        public static EnemyKind Champion() => new EnemyKind
        {
            Name = ChampionName,
            Prefix = "champion",
            Model = "elite_gilded",
            Palette = "cast",
            Scale = 1.5f,
            IsElite = true,
            Embers = 3,
            DrawsStorm = false,
            Ashen = true,
            Health = 240f,
            StaggerMaximum = 140f,
            StaggeredSeconds = 3f,
            WalkSpeed = 3.0f,
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
            SecondPhaseAttack = "champion_plant",
            SecondPhasePace = 1.12f,
            Brain = new EnemyBrain
            {
                AggroRange = 8.5f,
                LeashRange = 22f,
                PreferredRange = 3.4f,
                RestMin = 0.7f,
                RestMax = 1.4f,
                FlinchSeconds = 0.2f,
                CounterAttack = "champion_butt",
                ImpatientBeyond = 5f,
                Attacks = new List<EnemyAttack>
                {
                    // The god's three long thrusts, a little slower. The third waits and turns to follow.
                    new EnemyAttack { Name = "champion_thrust_1", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.1f, Damage = 14f, Reach = 5f, LineWidth = 1.1f, Lunge = 0.8f, MinRange = 2f, MaxRange = 4.8f, ChainInto = "champion_thrust_2", Fiery = true },
                    new EnemyAttack { Name = "champion_thrust_2", Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.1f, Damage = 14f, Reach = 5f, LineWidth = 1.1f, Lunge = 0.8f, MinRange = 1f, MaxRange = 0f, ChainInto = "champion_thrust_3", Fiery = true },
                    new EnemyAttack { Name = "champion_thrust_3", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.9f, Damage = 20f, Reach = 5.4f, LineWidth = 1.1f, Lunge = 1.4f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f, Fiery = true },
                    // Punishes circling.
                    new EnemyAttack { Name = "champion_sweep", Telegraph = 0.8f, Strike = 0.18f, Recovery = 0.8f, Damage = 24f, Reach = 3.8f, ArcDegrees = 200f, Sweep = -1f, MinRange = 0f, MaxRange = 3.4f, Cooldown = 3f, Fiery = true },
                    // Close in only, and his answer after four blows turned.
                    new EnemyAttack { Name = "champion_butt", Telegraph = 0.34f, Strike = 0.1f, Recovery = 0.5f, Damage = 12f, Reach = 2.4f, ArcDegrees = 70f, Lunge = 0.4f, MinRange = 0f, MaxRange = 2.6f, Cooldown = 2.5f },
                    // The red one, for someone who hangs back: the spear cast down a long lane and he behind it.
                    new EnemyAttack { Name = "champion_throw", Telegraph = 1.0f, Strike = 0.2f, Recovery = 1.2f, Damage = 26f, Reach = 12f, LineWidth = 1.2f, Lunge = 8f, PassThrough = true, Unblockable = true, MinRange = 6f, MaxRange = 12f, Cooldown = 8f, AimLockAt = 0.7f, Fiery = true },
                    // The planted spear: the ground round him burns. It opens his second phase.
                    new EnemyAttack { Name = "champion_plant", Telegraph = 1.1f, Strike = 0.2f, Recovery = 1.2f, Damage = 18f, Radius = 4.2f, MinRange = 0f, MaxRange = 3.4f, Unblockable = true, Cooldown = 16f, BurnSeconds = 6f, BurnDamage = 8f, BurnEvery = 0.5f, Fiery = true },
                    // Second phase only: the sweep runs on into the long thrust with no rest between.
                    new EnemyAttack { Name = "champion_reap", Telegraph = 0.7f, Strike = 0.18f, Recovery = 0.1f, Damage = 24f, Reach = 3.8f, ArcDegrees = 200f, Sweep = -1f, MinRange = 0f, MaxRange = 3.4f, Cooldown = 5f, MinPhase = 1, ChainInto = "champion_reap_thrust", Fiery = true },
                    new EnemyAttack { Name = "champion_reap_thrust", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.95f, Damage = 20f, Reach = 5.4f, LineWidth = 1.1f, Lunge = 1.4f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f, Fiery = true },
                    // Second phase only: the red cast twice, the second from where the first left him.
                    new EnemyAttack { Name = "champion_throw_twice", Telegraph = 0.9f, Strike = 0.2f, Recovery = 0.2f, Damage = 24f, Reach = 12f, LineWidth = 1.2f, Lunge = 8f, PassThrough = true, Unblockable = true, MinRange = 6f, MaxRange = 12f, Cooldown = 12f, MinPhase = 1, AimLockAt = 0.7f, ChainInto = "champion_throw_again", Fiery = true },
                    new EnemyAttack { Name = "champion_throw_again", Telegraph = 0.8f, Strike = 0.2f, Recovery = 1.2f, Damage = 24f, Reach = 12f, LineWidth = 1.2f, Lunge = 8f, PassThrough = true, Unblockable = true, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f, Fiery = true },
                },
            },
        };

        private static EnemyKind FireByName(string name) =>
            name == ThirstName ? Thirsting() : name == WickName ? Wick() : name == GildedLevyName ? Gilded(1)
            : name == GildedSpearmanName ? Gilded(2) : name == GildedWardenName ? Gilded(3) : name == SunBlindName ? SunBlind() : name == AshenName ? TheAshen() : name == ChampionName ? Champion() : name == SunKingName ? SunKing() : null;

        /// <summary>The gilded soldier's name at a rank from 1 to 3.</summary>
        public static string GildedName(int rank) => rank >= 3 ? GildedWardenName : rank == 2 ? GildedSpearmanName : GildedLevyName;

        /// <summary>
        /// One of the river people, dried out standing up. Weak, quick and plain: it teaches the chain, the perfect
        /// guard (one breaks it) and the finisher, and to watch where one drinks, because a drink near it brings it
        /// lunging for the flask.
        /// </summary>
        public static EnemyKind Thirsting() => new EnemyKind
        {
            Name = ThirstName,
            Prefix = "thirst",
            Model = "enemy_thirst",
            Scale = 0.95f,
            Health = 40f,
            StaggerMaximum = 45f,
            PerfectGuardStagger = 45f,
            PoiseHits = 1,
            WalkSpeed = 3.8f,
            TurnDegreesPerSecond = 380f,
            Embers = 1,
            DrawsStorm = false,
            Ashen = true,
            SnatchAttack = "thirst_snatch",
            SnatchRange = 8f,
            DrinkSeconds = 1.6f,
            // Kneeling at a dry pool it is up at once, and is struck no harder for it.
            RiseSeconds = 0.4f,
            DormantDamage = 1f,
            Brain = new EnemyBrain
            {
                AggroRange = 9f,
                PreferredRange = 1.9f,
                WaitRange = 5.5f,
                RestMin = 0.5f,
                RestMax = 1.0f,
                FlinchSeconds = 0.25f,
                Attacks = new List<EnemyAttack>
                {
                    // Two slow plain claws: hold guard, then try the perfect guard.
                    new EnemyAttack { Name = "thirst_claw_1", Telegraph = 0.55f, Strike = 0.12f, Recovery = 0.15f, Damage = 8f, Reach = 2.2f, ArcDegrees = 100f, Sweep = 1f, Lunge = 0.5f, MinRange = 0f, MaxRange = 2.4f, ChainInto = "thirst_claw_2", Fiery = true },
                    new EnemyAttack { Name = "thirst_claw_2", Telegraph = 0.4f, Strike = 0.12f, Recovery = 0.6f, Damage = 10f, Reach = 2.2f, ArcDegrees = 100f, Sweep = -1f, Lunge = 0.5f, MinRange = 1f, MaxRange = 0f, Fiery = true },
                    // The red pounce down a lane: step out of it.
                    new EnemyAttack { Name = "thirst_pounce", Telegraph = 0.9f, Strike = 0.15f, Recovery = 0.8f, Damage = 16f, Reach = 5f, LineWidth = 1.2f, Lunge = 4f, MinRange = 3.5f, MaxRange = 7f, Unblockable = true, Cooldown = 5f, AimLockAt = 0.65f, Fiery = true },
                    // A hand reaching for the belt, palm up. Guard it or roll; if it lands a flask goes with it.
                    new EnemyAttack { Name = "thirst_snatch", Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.3f, Damage = 6f, Reach = 6.5f, LineWidth = 1.4f, Lunge = 5.5f, MinRange = 3f, MaxRange = 6.5f, Cooldown = 14f, AimLockAt = 0.7f, NeedsFlasks = 2, Snatches = true, Fiery = true },
                },
            },
        };

        /// <summary>
        /// The wrapped dead, smouldering like a candle wick. Slow and upright; the alley is what makes it dangerous.
        /// Its red grab cannot be guarded: caught, the player is held and burnt until they struggle free.
        /// </summary>
        public static EnemyKind Wick() => new EnemyKind
        {
            Name = WickName,
            Prefix = "wick",
            Model = "enemy_wick",
            Scale = 1.05f,
            Health = 70f,
            StaggerMaximum = 70f,
            PerfectGuardStagger = 55f,
            PoiseHits = 3,
            WalkSpeed = 2.2f,
            TurnDegreesPerSecond = 200f,
            Embers = 1,
            DrawsStorm = false,
            Ashen = true,
            HoldSeconds = 2.5f,
            HoldBiteEvery = 0.5f,
            HoldBiteDamage = 6f,
            HoldPressTakes = 0.4f,
            ThrownOff = 2f,
            ThrownOffSeconds = 1f,
            StruggleStagger = 20f,
            RiseSeconds = 1.2f,
            DormantDamage = 2f,
            Brain = new EnemyBrain
            {
                AggroRange = 10f,
                PreferredRange = 2.2f,
                WaitRange = 4.5f,
                RestMin = 0.7f,
                RestMax = 1.4f,
                Attacks = new List<EnemyAttack>
                {
                    new EnemyAttack { Name = "wick_swipe", Telegraph = 0.7f, Strike = 0.15f, Recovery = 0.6f, Damage = 12f, Reach = 2.4f, ArcDegrees = 120f, Sweep = 1f, Lunge = 0.4f, MinRange = 0f, MaxRange = 2.6f, Fiery = true },
                    // Both arms brought down.
                    new EnemyAttack { Name = "wick_slam", Telegraph = 0.9f, Strike = 0.18f, Recovery = 0.8f, Damage = 18f, Reach = 2.6f, ArcDegrees = 70f, MinRange = 0f, MaxRange = 2.6f, Cooldown = 3f, Fiery = true },
                    // The red grab: suddenly fast. Missed, it stumbles and is open for a second.
                    new EnemyAttack { Name = "wick_grab", Telegraph = 0.9f, Strike = 0.15f, Recovery = 1.0f, Damage = 6f, Reach = 3.4f, LineWidth = 1.6f, Lunge = 2.2f, MinRange = 1.5f, MaxRange = 4f, Unblockable = true, Cooldown = 6f, AimLockAt = 0.6f, Grabs = true, Fiery = true },
                },
            },
        };

        /// <summary>
        /// A soldier of the golden way. Gold was poured on him and took; the more of it, the higher he stands. The
        /// gold is why he guards: a blow on it rings and does little, and what is not gold is hurt as flesh is, so from
        /// behind every blow lands in full. Each rank adds a word of the spear's grammar and of the god's.
        /// </summary>
        public static EnemyKind Gilded(int rank)
        {
            rank = rank < 1 ? 1 : rank > 3 ? 3 : rank;
            float harder = rank == 3 ? 1.3f : rank == 2 ? 1.15f : 1f;
            var attacks = new List<EnemyAttack>
            {
                new EnemyAttack { Name = "gilded_thrust", Telegraph = 0.65f, Strike = 0.15f, Recovery = 0.6f, Damage = 16f * harder, Reach = 4.4f, LineWidth = 1.0f, Lunge = 0.9f, MinRange = 0f, MaxRange = 4.6f, Fiery = true },
                new EnemyAttack { Name = "gilded_sweep", Telegraph = 0.85f, Strike = 0.2f, Recovery = 0.75f, Damage = 20f * harder, Reach = 3.4f, ArcDegrees = 170f, Sweep = -1f, MinRange = 0f, MaxRange = 3.2f, Cooldown = 2.5f, Fiery = true },
            };
            if (rank >= 2)
            {
                // Two thrusts; the second waits longer than a frightened roll lasts and turns to follow.
                attacks.Add(new EnemyAttack { Name = "gilded_thrust_1", Telegraph = 0.6f, Strike = 0.12f, Recovery = 0.1f, Damage = 14f * harder, Reach = 4.4f, LineWidth = 1.1f, Lunge = 0.8f, MinRange = 1.9f, MaxRange = 4.4f, Cooldown = 3f, ChainInto = "gilded_thrust_2", Fiery = true });
                attacks.Add(new EnemyAttack { Name = "gilded_thrust_2", Telegraph = 0.5f, Strike = 0.12f, Recovery = 0.85f, Damage = 18f * harder, Reach = 4.8f, LineWidth = 1.1f, Lunge = 1.2f, MinRange = 1f, MaxRange = 0f, AimLockAt = 0.8f, Fiery = true });
                // The red lunge from distance.
                attacks.Add(new EnemyAttack { Name = "gilded_lunge", Telegraph = 1.0f, Strike = 0.2f, Recovery = 0.9f, Damage = 26f * harder, Reach = 5f, LineWidth = 1.3f, Lunge = 3.4f, MinRange = 3.5f, MaxRange = 8f, Unblockable = true, Cooldown = 6f, Fiery = true });
            }
            if (rank == 2)
            {
                // The throw: the spear cast down a long red lane. It stands in the floor where it lands, and until he
                // has walked to it and pulled it out he has no guard.
                attacks.Add(new EnemyAttack { Name = "gilded_cast", Telegraph = 0.9f, Strike = 0.15f, Recovery = 0.5f, Damage = 20f * harder, Reach = 10f, LineWidth = 1.0f, MinRange = 5f, MaxRange = 10f, Unblockable = true, Cooldown = 10f, AimLockAt = 0.7f, Casts = true, Fiery = true });
            }
            if (rank >= 3)
            {
                // The pour: he tears gold from his own side and throws it in an arc before him, a low wall of molten
                // gold that stands a few seconds. It keeps the player off him while the lower ranks press.
                attacks.Add(new EnemyAttack { Name = "gilded_pour", Telegraph = 0.95f, Strike = 0.2f, Recovery = 0.9f, Damage = 12f * harder, Reach = 3.6f, ArcDegrees = 110f, Sweep = 1f, MinRange = 2.2f, MaxRange = 5.5f, Unblockable = true, Cooldown = 9f, Pours = true, Fiery = true });
                // The planted spear: a ring round himself that goes on burning. His spear is stuck: he is open to anyone with reach.
                attacks.Add(new EnemyAttack { Name = "gilded_plant", Telegraph = 1.0f, Strike = 0.2f, Recovery = 1.2f, Damage = 20f * harder, Radius = 3.2f, MinRange = 0f, MaxRange = 3f, Unblockable = true, Cooldown = 8f, BurnSeconds = 3f, BurnDamage = 4f, BurnEvery = 0.5f, Fiery = true });
            }
            return new EnemyKind
            {
                Name = GildedName(rank),
                Prefix = "gilded",
                Model = "enemy_gilded",
                Palette = "gilded" + rank,
                Scale = rank == 3 ? 1.2f : rank == 2 ? 1.12f : 1.05f,
                Health = rank == 3 ? 150f : rank == 2 ? 110f : 80f,
                StaggerMaximum = rank == 3 ? 120f : rank == 2 ? 100f : 80f,
                PerfectGuardStagger = 55f,
                PoiseHits = rank + 2,
                Guards = true,
                GuardedDamage = rank == 3 ? 0.2f : 0.25f,
                GuardedStagger = 1f,
                HeavyGuardedDamage = rank == 3 ? 0.5f : 1f,
                Steadfast = rank == 3,
                // The levy only turns blows; from the second rank up, three blows turned are answered with the thrust.
                BlocksBeforeCounter = rank == 1 ? 9999 : 3,
                WalkSpeed = 2.7f,
                TurnDegreesPerSecond = 220f,
                Embers = rank,
                DrawsStorm = false,
                Ashen = true,
                Brain = new EnemyBrain
                {
                    AggroRange = 11f,
                    PreferredRange = 3.4f,
                    // The warden lets the lower ranks go first.
                    WaitRange = rank == 3 ? 8f : 6f,
                    RestMin = 0.5f,
                    RestMax = 1.1f,
                    CounterAttack = "gilded_thrust",
                    Attacks = attacks,
                },
            };
        }

        /// <summary>
        /// A priest who looked at his god until his eyes burned out. He cannot see: he finds whoever stands in the sun,
        /// keeps his distance and brings the sun down on them; in shade he loses them, walks to where they were, and
        /// searches. Struck by someone he has not found, he takes the blow twice over.
        /// </summary>
        public static EnemyKind SunBlind() => new EnemyKind
        {
            Name = SunBlindName,
            Prefix = "sunblind",
            Model = "enemy_sunblind",
            Scale = 1.0f,
            Health = 60f,
            StaggerMaximum = 50f,
            PerfectGuardStagger = 40f,
            PoiseHits = 2,
            WalkSpeed = 2.0f,
            LitWalkSpeed = 3.4f,
            LitPreferredRange = 7f,
            LitRetreatRange = 4f,
            SearchSeconds = 3f,
            UnfoundDamage = 2f,
            TurnDegreesPerSecond = 300f,
            Embers = 1,
            DrawsStorm = false,
            Ashen = true,
            HuntsByLight = true,
            Brain = new EnemyBrain
            {
                AggroRange = 18f,
                PreferredRange = 2.4f,
                RestMin = 0.8f,
                RestMax = 1.5f,
                Attacks = new List<EnemyAttack>
                {
                    // He lifts the sun on his staff and it comes down where the target stood. Only at someone standing in the light.
                    new EnemyAttack { Name = "sunblind_ray", Telegraph = 1.1f, Strike = 0.2f, Recovery = 0.6f, Damage = 20f, Radius = 2.0f, Ranged = true, MinRange = 3.5f, MaxRange = 18f, Unblockable = true, OnlyLit = true, Fiery = true },
                    // The staff swung low.
                    new EnemyAttack { Name = "sunblind_brand", Telegraph = 0.7f, Strike = 0.15f, Recovery = 0.7f, Damage = 16f, Reach = 2.8f, ArcDegrees = 140f, Sweep = 1f, MinRange = 0f, MaxRange = 3f, Fiery = true },
                    // A burst of light round himself; the ground it fell on counts as sunlit for a while after.
                    // The mirror: the disc on his breast tipped to the sky, and a beam of the sun thrown down a lane. Where it
                    // falls the ground counts as sunlit for a while, so it can drive someone out of shade.
                    new EnemyAttack { Name = "sunblind_beam", Telegraph = 1.0f, Strike = 0.25f, Recovery = 0.7f, Damage = 14f, Reach = 12f, LineWidth = 1.1f, MinRange = 3f, MaxRange = 12f, Unblockable = true, Cooldown = 6f, AimLockAt = 0.75f, Beams = true, LightsSeconds = 3.5f, Fiery = true },
                    new EnemyAttack { Name = "sunblind_flare", Telegraph = 0.8f, Strike = 0.2f, Recovery = 0.9f, Damage = 16f, Radius = 3f, MinRange = 0f, MaxRange = 3.2f, Cooldown = 5f, LightsSeconds = 4f, Fiery = true },
                },
            },
        };

        /// <summary>
        /// A wick burned down to its last: a shell of grey ash round a white-hot core. It comes at a stumbling run,
        /// and close in it stops and swells (the core brightens, the cracks open, a red ring on the ground) and then
        /// bursts, hurting everything in the ring, its own side too, and lighting any wick it reaches. Killed before
        /// that, it bursts where it fell a moment later. It does not flinch: only killing it stops the swell.
        /// </summary>
        public static EnemyKind TheAshen() => new EnemyKind
        {
            Name = AshenName,
            Prefix = "ashen",
            Model = "enemy_wick",
            Palette = "ashen",
            Scale = 0.98f,
            Health = 32f,
            StaggerMaximum = 999f,
            PerfectGuardStagger = 0f,
            PoiseHits = 999,
            WalkSpeed = 4.4f,
            TurnDegreesPerSecond = 260f,
            Embers = 1,
            DrawsStorm = false,
            Ashen = true,
            RiseSeconds = 0.5f,
            DormantDamage = 1f,
            BurstsOnDeath = 0.7f,
            BurstRadius = 3.2f,
            BurstDamage = 30f,
            BurstEnemyDamage = 45f,
            Brain = new EnemyBrain
            {
                AggroRange = 11f,
                LeashRange = 30f,
                PreferredRange = 1.4f,
                WaitRange = 0f,
                RestMin = 0.2f,
                RestMax = 0.4f,
                FlinchSeconds = 0.1f,
                Attacks = new List<EnemyAttack>
                {
                    // The swell: it stops, the core brightens and the shell cracks open, and the ring on the ground is
                    // what it will burn. Get out of the ring; a guard does nothing.
                    new EnemyAttack { Name = "ashen_swell", Telegraph = 1.35f, Strike = 0.1f, Recovery = 0.3f, Damage = 30f, Radius = 3.2f, MinRange = 0f, MaxRange = 2.2f, Unblockable = true, Bursts = true, AimLockAt = 0f, Fiery = true },
                },
            },
        };
    }
}
