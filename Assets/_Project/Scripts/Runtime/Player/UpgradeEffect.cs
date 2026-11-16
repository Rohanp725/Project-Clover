using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// One thing an upgrade does: when (<see cref="On"/>), under what conditions, and what (<see cref="Do"/>) with
    /// which numbers. Which numbers an action reads is written on each <see cref="UpgradeAction"/>. Distances are in
    /// metres, times in seconds, damage and stagger in the same points weapons use (a light blow of the fists is 8
    /// damage and 10 stagger; a soldier has 60 health and 100 balance).
    /// </summary>
    public class UpgradeEffect
    {
        public UpgradeTrigger On;
        public UpgradeAction Do;
        public EffectAt At = EffectAt.Target;

        // Conditions. All that are set must hold.
        /// <summary>The chance it happens each time the trigger comes: 1 always, 0.25 one time in four.</summary>
        public float Chance = 1f;
        /// <summary>Seconds after it happens before it can happen again.</summary>
        public float Cooldown;
        /// <summary>For <see cref="UpgradeTrigger.EverySeconds"/>: the seconds between.</summary>
        public float Every = 5f;
        /// <summary>The target must already carry this affliction. None asks for nothing.</summary>
        public AfflictionKind Needs = AfflictionKind.None;
        /// <summary>True when it happens only while the surge lasts.</summary>
        public bool WhileSurge;
        /// <summary>The target must carry this one as well as <see cref="Needs"/>.</summary>
        public AfflictionKind Needs2 = AfflictionKind.None;
        /// <summary>The affliction named in <see cref="Needs"/> must be at its full depth.</summary>
        public bool NeedsFull;
        /// <summary>The target must be in the air (or reeling).</summary>
        public bool NeedsAirborne;
        /// <summary>Only while the player's health is below this share of the whole. Zero: at any health.</summary>
        public float HealthBelow;
        /// <summary>Damage, stagger and stacks are multiplied by the stacks the trigger carried: what was spent by a set-off, or what the dead enemy carried of <see cref="Needs"/>.</summary>
        public bool PerStack;
        /// <summary>The stacks it lays are as many as the trigger carried: what the dead enemy bore of <see cref="Needs"/>, or what a set-off spent.</summary>
        public bool CarriesDepth;
        /// <summary>Only while this state of the player holds.</summary>
        public StatWhen While = StatWhen.Always;
        /// <summary>For the SetOff trigger: only when this was the affliction spent. None: any.</summary>
        public AfflictionKind Spent = AfflictionKind.None;
        /// <summary>For the SetOff trigger: only when at least this many stacks were spent.</summary>
        public int SpentAtLeast;
        /// <summary>For a Hazard: it is something grown (counted, stood near, burst with the rest), and at most <see cref="Limit"/> of this effect's stand at once (0: no limit).</summary>
        public bool Grown;
        public int Limit;
        /// <summary>For the SpellCast trigger: only when the spell cast is this mark's own.</summary>
        public bool OwnSpell;
        /// <summary>It happens only if its mark's count holds this many, and spends them.</summary>
        public int Spends;
        /// <summary>It happens only if its mark's count holds any, spends them all, and carries how many were spent (for <see cref="PerStack"/>).</summary>
        public bool SpendsAll;

        // Numbers.
        /// <summary>The affliction laid or set off. None means the element's own.</summary>
        public AfflictionKind Affliction = AfflictionKind.None;
        /// <summary>Stacks of the affliction laid. For actions that deal damage, zero lays none.</summary>
        public int Stacks;
        public float Damage;
        public float Stagger;
        public float Radius;
        public float Reach;
        /// <summary>Full width of a cone, in degrees.</summary>
        public float Arc;
        public float Seconds;
        public float Delay;
        public float Interval;
        public float Speed;
        /// <summary>A plain amount: health, mana, stamina, metres, or a share, as the action says.</summary>
        public float Amount;
        public int Count;
        /// <summary>For <see cref="UpgradeAction.Boon"/>: the stat changed.</summary>
        public PlayerStat Stat;
        public HazardShape Shape = HazardShape.Patch;
        public Flight Flight = Flight.Straight;
        /// <summary>For <see cref="UpgradeAction.Form"/>: the form cast, by its name in <see cref="SpellForms"/>, and the numbers it reads.</summary>
        public string Form = "";
        public SpellSpec Spell;

        /// <summary>Its place among all the effects in the catalogue; set when the catalogue is gathered.</summary>
        public int Slot { get; internal set; } = -1;
        /// <summary>The mark this effect belongs to. Set when the catalogue is gathered.</summary>
        public Upgrade Owner { get; internal set; }
    }
}
