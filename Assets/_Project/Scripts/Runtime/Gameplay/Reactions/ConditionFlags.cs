using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// The states an enemy condition can put an enemy in (IMPLEMENTATION.md 3.7, README "Enemy states"). The brain, the
    /// enemy's body and the gods' choosers each read them in one place; the afflictions' numbers read the shares.
    /// </summary>
    [Flags]
    public enum ConditionFlags
    {
        None = 0,
        /// <summary>It cannot walk or be shoved, but it can still attack what comes into reach.</summary>
        Rooted = 1 << 0,
        /// <summary>It begins no attack; it still moves, and a blow already begun is finished.</summary>
        BeginsNoAttack = 1 << 1,
        /// <summary>It cannot raise its guard.</summary>
        CannotGuard = 1 << 2,
        /// <summary>Its wind-ups are shown slower, by the run's Scale (0.5: twice as long).</summary>
        WindupSlowed = 1 << 3,
        /// <summary>The damage it takes is multiplied by the run's Scale.</summary>
        DamageTakenShare = 1 << 4,
        /// <summary>The damage it deals is multiplied by the run's Scale.</summary>
        DamageDealtShare = 1 << 5,
        /// <summary>It sees only as far as the run's Sight: one farther off is lost to it.</summary>
        SightShort = 1 << 6,
        /// <summary>It is joined to another enemy (the run's Link); what passes along the link is the setting mark's to say.</summary>
        Linked = 1 << 7,
        /// <summary>The stagger it takes is multiplied by the run's Scale (Breath Taken, Logan Stone, Turned to Stone).</summary>
        StaggerTakenShare = 1 << 8,
        /// <summary>The damage it takes from weapon blows only is multiplied by the run's Scale (Cold Short, Drowned Weight, Girdled).</summary>
        WeaponTakenShare = 1 << 9,
        /// <summary>The damage it takes from spells only is multiplied by the run's Scale (Turned to Stone).</summary>
        SpellTakenShare = 1 << 10,
        /// <summary>Every blow on it is critical, as a mark of light makes it (Fish Poison's float).</summary>
        TakesCritical = 1 << 11,
        /// <summary>
        /// Knocked flat (task 3.7): it falls on its back and lies there for the run's time, beginning nothing, unable to
        /// guard or be made to flinch, and then gets up (<see cref="EnemyState.Down"/>). What resists a root resists this
        /// the same way: an elite lies half the time, and a god never goes down (what cannot be frozen, or a god in an
        /// ordinary body: AfflictionSet.Rank).
        /// </summary>
        Down = 1 << 12,
    }
}
