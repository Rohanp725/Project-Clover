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
