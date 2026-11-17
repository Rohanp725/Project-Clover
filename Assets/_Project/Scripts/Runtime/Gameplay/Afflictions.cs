using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>The eleven afflictions as data, one for each element. Change a number here and every enemy follows.</summary>
    public static class Afflictions
    {
        public const int Count = 12;

        private static readonly AfflictionSpec[] specs =
        {
            new AfflictionSpec { Kind = AfflictionKind.None, Name = "", MaximumStacks = 0, Seconds = 0f, SetOffPerStack = 0f },
            // Burn keeps its own engine (BurnStatus): seconds that top up, five stacks that each add 30% to 5 a second.
            new AfflictionSpec { Kind = AfflictionKind.Burn, Name = "Burn", Colour = new Color(1f, 0.56f, 0.2f), MaximumStacks = 5, Seconds = 4f, SetOffPerStack = 10f },
            new AfflictionSpec { Kind = AfflictionKind.Static, Name = "Static", Colour = new Color(0.45f, 0.78f, 1f), MaximumStacks = 5, Seconds = 6f, SetOffPerStack = 7f },
            new AfflictionSpec { Kind = AfflictionKind.Unsteady, Name = "Unsteady", Colour = new Color(0.62f, 0.95f, 0.8f), MaximumStacks = 5, Seconds = 6f,
                StaggerTakenPerStack = 0.06f, ShovePerStack = 0.15f, TurnSlowPerStack = 0.08f },
            new AfflictionSpec { Kind = AfflictionKind.Chill, Name = "Chill", Colour = new Color(0.7f, 0.92f, 1f), MaximumStacks = 5, Seconds = 6f,
                SlowPerStack = 0.07f, FreezesAtFull = true, FrozenSeconds = 2.5f, ShatterDamage = 45f },
            new AfflictionSpec { Kind = AfflictionKind.Soaked, Name = "Soaked", Colour = new Color(0.25f, 0.5f, 1f), MaximumStacks = 1, Seconds = 8f,
                SpellDamageTakenPerStack = 0.25f, Carries = AfflictionKind.Static, AlsoCarries = AfflictionKind.Chill, CarryRadius = 4f },
            new AfflictionSpec { Kind = AfflictionKind.Seeded, Name = "Seeded", Colour = new Color(0.45f, 0.9f, 0.35f), MaximumStacks = 3, Seconds = 8f,
                HealOnBlowPerStack = 1f, DeathBurstPerStack = 12f, DeathBurstRadius = 3f },
            new AfflictionSpec { Kind = AfflictionKind.Tremor, Name = "Tremor", Colour = new Color(0.72f, 0.52f, 0.3f), MaximumStacks = 3, Seconds = 6f,
                SlowPerStack = 0.1f, StopsBalanceRecovery = true },
            new AfflictionSpec { Kind = AfflictionKind.Sundered, Name = "Sundered", Colour = new Color(0.78f, 0.8f, 0.88f), MaximumStacks = 3, Seconds = 8f,
                WeaponDamageTakenPerStack = 0.08f },
            new AfflictionSpec { Kind = AfflictionKind.Marked, Name = "Marked", Colour = new Color(1f, 0.93f, 0.6f), MaximumStacks = 1, Seconds = 8f, Blows = 3,
                MakesBlowsCritical = true, ManaPerStack = 2f },
            new AfflictionSpec { Kind = AfflictionKind.Cursed, Name = "Cursed", Colour = new Color(0.62f, 0.3f, 0.85f), MaximumStacks = 1, Seconds = 8f,
                DealsLessPerStack = 0.15f, HealOnKillPerStack = 6f },
            new AfflictionSpec { Kind = AfflictionKind.Poison, Name = "Poisoned", Colour = new Color(0.6f, 0.85f, 0.15f), MaximumStacks = 8, Seconds = 6f,
                DamagePerTick = 2f, TickSeconds = 2f, SetOffPerStack = 6f },
        };

        public static AfflictionSpec Spec(AfflictionKind kind) => specs[(int)kind];

        /// <summary>The affliction the player's own marks let lie deeper, and by how many stacks. The player's upgrades keep these.</summary>
        public static AfflictionKind DeeperKind { get; private set; }
        public static int DeeperBy { get; private set; }
        /// <summary>How many more weapon blows an affliction that is spent by blows (a mark of light) lasts for.</summary>
        public static int MoreBlows { get; set; }
        /// <summary>While true, poison on every enemy neither bites nor fades; what it would have bitten is owed (<see cref="AfflictionSet.TakeOwed"/>). Poison's gift keeps this.</summary>
        public static bool PoisonWaits { get; set; }

        // Domain reload is off: a wait left standing by the last play must not hold this one's poison.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetWaits()
        {
            PoisonWaits = false;
            PoisonBeatOff = false;
            PoisonKeeps = false;
            PoisonRipens = 0f;
            PoisonOnCursed = 0f;
            SharedBeat = false;
            SharedBeatShare = 0f;
            BeatNow = false;
            BeatStrength = 1f;
            BeatCount = 0;
            beatLeft = 0f;
            beatWasNow = false;
            FreezeSeconds = 1f;
            ShatterDamage = 1f;
            ChillSlowOff = false;
            BrittleSeconds = 0f;
            BrittleShare = 0.2f;
            BuckledShare = 0f;
            FlashFreezeAt = 0;
        }

        // ---- Poison's beat, as the player's marks change it. The player's upgrades keep all of these.

        /// <summary>While true, poison does not bite on its beat at all (it is only ever set off).</summary>
        public static bool PoisonBeatOff { get; set; }
        /// <summary>While true, poison does not fade while its bearer lives.</summary>
        public static bool PoisonKeeps { get; set; }
        /// <summary>Each bite that comes with no fresh dose since the last is this share stronger than the one before.</summary>
        public static float PoisonRipens { get; set; }
        /// <summary>Above zero: poison bites this share harder on the Cursed and does not fade from them.</summary>
        public static float PoisonOnCursed { get; set; }
        /// <summary>While true, the poison in everything bites on one beat (<see cref="TickBeat"/>), not each on its own.</summary>
        public static bool SharedBeat { get; set; }
        /// <summary>On the shared beat, each poisoned thing beyond the first that bit on the last beat makes every bite this share stronger.</summary>
        public static float SharedBeatShare { get; set; }
        /// <summary>True for the one step in which the shared beat falls.</summary>
        public static bool BeatNow { get; private set; }
        /// <summary>What every bite of the shared beat is multiplied by.</summary>
        public static float BeatStrength { get; private set; } = 1f;
        /// <summary>How many have bitten on the shared beat now falling.</summary>
        public static int BeatCount { get; set; }
        /// <summary>Seconds until the shared beat falls.</summary>
        public static float BeatLeft => beatLeft;
        private static float beatLeft;
        private static bool beatWasNow;
        /// <summary>How long a freeze lasts, and how hard a shatter lands, as shares of the usual.</summary>
        public static float FreezeSeconds { get; set; } = 1f;
        public static float ShatterDamage { get; set; } = 1f;
        /// <summary>While true, Chill slows no one (it still freezes at full depth). The player's upgrades keep it.</summary>
        public static bool ChillSlowOff { get; set; }
        /// <summary>Above zero: what thaws or is shattered stays brittle this long, taking <see cref="BrittleShare"/> more from the weapon and to its balance.</summary>
        public static float BrittleSeconds { get; set; }
        public static float BrittleShare { get; set; } = 0.2f;
        /// <summary>Above zero: what carries Tremor and Sundered together takes this share more stagger from everything.</summary>
        public static float BuckledShare { get; set; }
        /// <summary>Above zero: what is Soaked freezes at this depth of Chill, and the freeze spends the Soaked.</summary>
        public static int FlashFreezeAt { get; set; }

        /// <summary>
        /// Moves the shared beat on by one step. Called once a step by whoever keeps the beat (the player's upgrades):
        /// the beat is "now" from this call until the next, so everything that ticks once in between sees it once.
        /// </summary>
        public static void TickBeat(float deltaTime)
        {
            if (beatWasNow)
            {
                // The beat that has just fallen is counted: the next is as strong as the company it kept.
                BeatStrength = 1f + SharedBeatShare * System.Math.Max(0, BeatCount - 1);
                BeatCount = 0;
            }
            BeatNow = false;
            beatWasNow = false;
            if (!SharedBeat)
            {
                BeatStrength = 1f;
                return;
            }
            beatLeft -= deltaTime;
            if (beatLeft > 0f) return;
            beatLeft += specs[(int)AfflictionKind.Poison].TickSeconds;
            BeatNow = true;
            beatWasNow = true;
        }

        public static void SetDeeper(AfflictionKind kind, int by)
        {
            DeeperKind = by > 0 ? kind : AfflictionKind.None;
            DeeperBy = by > 0 ? by : 0;
        }

        /// <summary>The greatest depth an affliction can be laid to now.</summary>
        public static int Depth(AfflictionKind kind) => specs[(int)kind].MaximumStacks + (kind == DeeperKind && kind != AfflictionKind.None ? DeeperBy : 0);

        // Domain reload is off: a depth learned in the last play must not stay.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SetDeeper(AfflictionKind.None, 0);
            MoreBlows = 0;
        }
    }
}
