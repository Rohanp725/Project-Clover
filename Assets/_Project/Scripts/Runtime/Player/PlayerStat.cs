namespace Clover.Player
{
    /// <summary>
    /// A number of the player's that an upgrade or a timed boon can change. Most are shares: an amount of 0.10 means
    /// ten percent more, -0.10 ten percent less, and all the amounts for one stat are added together before they are
    /// applied. The few marked "flat" are plain amounts added on. Which is which is in <see cref="PlayerStats.IsFlat"/>.
    /// </summary>
    public enum PlayerStat
    {
        /// <summary>Share. Damage of every blow of the weapon.</summary>
        WeaponDamage,
        /// <summary>Share. Stagger of every blow of the weapon.</summary>
        StaggerDealt,
        /// <summary>Share. Damage of blows and spells on an enemy that carries any affliction.</summary>
        DamageToAfflicted,
        /// <summary>Flat, whole stacks. Added whenever the player lays stacks of an affliction.</summary>
        AfflictionStacks,
        /// <summary>Share. How long the afflictions the player lays last.</summary>
        AfflictionSeconds,
        /// <summary>Share. How strongly the afflictions the player lays work: their damage, their slow, what they let through.</summary>
        AfflictionPower,
        /// <summary>Share. Damage of the base spell and of worn spells.</summary>
        SpellDamage,
        /// <summary>Share. Mana the base spell and worn spells cost. Negative is cheaper.</summary>
        SpellCost,
        /// <summary>Share. Reach of the base spell and of worn spells.</summary>
        SpellReach,
        /// <summary>Share. How long the surge lasts.</summary>
        SurgeSeconds,
        /// <summary>Share. Mana the surge costs. Negative is cheaper.</summary>
        SurgeCost,
        /// <summary>Share. Mana earned by blows of the weapon.</summary>
        ManaFromBlows,
        /// <summary>Share. Mana earned by a perfect guard.</summary>
        ManaFromGuards,
        /// <summary>Share. Mana earned by a perfect dodge.</summary>
        ManaFromDodges,
        /// <summary>Share. Stamina the dodge and a blocked blow cost. Negative is cheaper.</summary>
        StaminaCost,
        /// <summary>Share. How far the dodge goes.</summary>
        DodgeDistance,
        /// <summary>Share. How long blows pass through the dodge, and how long its perfect window is.</summary>
        DodgeSafeTime,
        /// <summary>Share. How long after the press a guard is perfect.</summary>
        PerfectGuardWindow,
        /// <summary>Flat, points. Added to the player's greatest health.</summary>
        Health,
        /// <summary>Share. Health a flask gives.</summary>
        FlaskHeal,
        /// <summary>Flat, whole flasks. Added to the three carried.</summary>
        FlaskCount,
        /// <summary>Flat. Chance that a blow of the weapon is critical: 0.05 is five in a hundred.</summary>
        CritChance,
        /// <summary>Share. Added to what a critical blow multiplies damage by (half again, to begin with).</summary>
        CritDamage,
        /// <summary>Share. How fast the weapon is swung and the player walks while the surge lasts.</summary>
        SurgeSpeed,
        /// <summary>Share. Damage of blows that land on the player. Negative is less.</summary>
        DamageTaken,
        /// <summary>Share. How slowly grey health fades: 1.0 is twice as long.</summary>
        GreyKept,
        /// <summary>Share. Embers earned from the fallen.</summary>
        EmbersEarned,
        /// <summary>Share. Walking speed.</summary>
        MoveSpeed,
        /// <summary>Share. How fast the weapon is swung.</summary>
        SwingSpeed,
    }
}
