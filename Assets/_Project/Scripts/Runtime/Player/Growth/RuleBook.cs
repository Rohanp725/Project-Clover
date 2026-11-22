using Clover.Gameplay;

namespace Clover.Player.Growth
{
    /// <summary>
    /// The one place a standard rule is switched off or swapped (IMPLEMENTATION.md 3.10): a Devotion, a keystone or a
    /// Heresy writes here, and each system asks at the moment its rule applies, so no Devotion or Heresy check is scattered
    /// through the guard, the clocks or the afflictions. One per player, owned by <see cref="PlayerUpgrades"/> and rebuilt
    /// from nothing whenever what is in force changes (<see cref="Reset"/>, each in-force mark's <see cref="Upgrade.Rules"/>,
    /// then <see cref="Publish"/>), so it starts each run, and each change of Hand, standard. Plain fields and fixed arrays:
    /// reading it costs nothing and nothing in it allocates.
    ///
    /// Read now: <see cref="Cooldowns"/> (<see cref="CooldownMode.None"/> freezes every clock) and
    /// <see cref="BreathAfterPerfect"/> by <see cref="PlayerSpells"/>; <see cref="Guard"/>
    /// (<see cref="GuardMode.Unbreakable"/>) by <see cref="PlayerDefence"/>; <see cref="Afflictions"/> by every enemy's
    /// <see cref="AfflictionSet"/> through <see cref="Clover.Gameplay.Afflictions.Rules"/>. The other switches carry their
    /// standard values until the Heresies of phase 6 (task 6.3) write them and their systems read them; the death
    /// intercepts and The Hard Road's shares join then.
    /// </summary>
    public sealed class RuleBook
    {
        public const int StandardSlots = 4;
        public const int StandardAttunements = 2;

        // ---- The spells' clocks.
        /// <summary>How spells wait to be cast again: <see cref="CooldownMode.None"/> starts no clock at all.</summary>
        public CooldownMode Cooldowns;
        /// <summary>Seconds after a perfect guard or a perfect dodge in which spells have no cooldown (In One Breath); zero for none.</summary>
        public float BreathAfterPerfect;
        /// <summary>
        /// Spells cast in motion (On the Wind): no wind-up, cast in the middle of a dodge or a swing without ending either, and a
        /// spell cast in a dodge carries it <see cref="DodgeCastCarry"/> metres further along its way.
        /// </summary>
        public bool CastInMotion;
        public float DodgeCastCarry;
        /// <summary>How many spell slots the Hand has (One Word: 1).</summary>
        public int SlotCount = StandardSlots;
        /// <summary>How many elements are carried (Three Bloods: 3).</summary>
        public int AttunementCount = StandardAttunements;

        // ---- Mana and the charged blade.
        /// <summary>Where mana comes from. Heartwood's sealed bar is still kept by <c>ManaPool.Instead</c> until a task moves it here.</summary>
        public ManaMode Mana;
        /// <summary>How the charged blade is lit and spent.</summary>
        public BladeMode Blade;

        // ---- The body: guard, dodge, stamina, flasks, health.
        /// <summary>How the guard works: <see cref="GuardMode.Unbreakable"/> holds a blow the stamina cannot pay for.</summary>
        public GuardMode Guard;
        /// <summary>How the dodge works.</summary>
        public DodgeMode Dodge;
        /// <summary>False when a guard can never be perfect.</summary>
        public bool PerfectGuardOpen = true;
        /// <summary>False when a dodge can never be perfect.</summary>
        public bool PerfectDodgeOpen = true;
        /// <summary>False when nothing costs stamina (Unwearied).</summary>
        public bool StaminaUsed = true;
        /// <summary>False when the flasks cannot be drunk (Dry Flask).</summary>
        public bool FlasksAllowed = true;
        /// <summary>How health is kept.</summary>
        public HealthMode Health;
        /// <summary>What breaks an enemy's balance.</summary>
        public StaggerSource StaggerFrom;

        // ---- The afflictions the player lays.
        /// <summary>How afflictions behave on every enemy: kept from fading, deepening, not spent by blows, resting after fading.</summary>
        public AfflictionRules Afflictions { get; } = new AfflictionRules();

        /// <summary>Goes up at each <see cref="Publish"/>: a system that keeps something read from the book looks again then.</summary>
        public int Version { get; private set; }
        /// <summary>How many marks wrote into the book since the last <see cref="Reset"/>.</summary>
        public int Switches { get; private set; }

        public RuleBook() => Reset();

        /// <summary>Every rule standard again: the start of a rebuild, and of a run.</summary>
        public void Reset()
        {
            Cooldowns = CooldownMode.Normal;
            BreathAfterPerfect = 0f;
            CastInMotion = false;
            DodgeCastCarry = 0f;
            SlotCount = StandardSlots;
            AttunementCount = StandardAttunements;
            Mana = ManaMode.Normal;
            Blade = BladeMode.Normal;
            Guard = GuardMode.Normal;
            Dodge = DodgeMode.Normal;
            PerfectGuardOpen = true;
            PerfectDodgeOpen = true;
            StaminaUsed = true;
            FlasksAllowed = true;
            Health = HealthMode.Bar;
            StaggerFrom = StaggerSource.Blows;
            Afflictions.Clear();
            Switches = 0;
        }

        /// <summary>Lets a mark in force write its switches. True when it had any.</summary>
        public bool Apply(Upgrade mark)
        {
            if (mark == null || mark.Rules == null) return false;
            mark.Rules(this);
            Switches++;
            return true;
        }

        /// <summary>The rebuild is done: what the enemies read is set from it, and the version goes up.</summary>
        public void Publish()
        {
            Clover.Gameplay.Afflictions.Rules.CopyFrom(Afflictions);
            Version++;
        }

        /// <summary>True while every rule is the standard one.</summary>
        public bool IsStandard =>
            Cooldowns == CooldownMode.Normal && BreathAfterPerfect <= 0f && !CastInMotion && SlotCount == StandardSlots && AttunementCount == StandardAttunements
            && Mana == ManaMode.Normal && Blade == BladeMode.Normal && Guard == GuardMode.Normal && Dodge == DodgeMode.Normal
            && PerfectGuardOpen && PerfectDodgeOpen && StaminaUsed && FlasksAllowed && Health == HealthMode.Bar
            && StaggerFrom == StaggerSource.Blows && !Afflictions.Any;
    }
}
