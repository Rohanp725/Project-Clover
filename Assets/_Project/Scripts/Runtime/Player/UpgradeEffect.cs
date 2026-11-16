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

        /// <summary>Its place among all the effects in the catalogue; set when the catalogue is gathered.</summary>
        public int Slot { get; internal set; } = -1;
    }
}
