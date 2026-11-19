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
        /// <summary>Share. Stamina of the weapon's own blows.</summary>
        AttackStamina,
        /// <summary>Share. Stamina of a blocked blow alone (on top of StaminaCost).</summary>
        GuardStamina,
        /// <summary>Share. Stamina of the dodge alone (on top of StaminaCost).</summary>
        DodgeStamina,
        /// <summary>Share. Mana of the element's base spell alone (on top of SpellCost).</summary>
        BaseSpellCost,
        /// <summary>Share. Reach of the base spell alone (on top of SpellReach).</summary>
        BaseSpellReach,
        /// <summary>Share. Damage of the base spell alone (on top of SpellDamage).</summary>
        BaseSpellDamage,
        /// <summary>Flat, whole. How much deeper the held element's affliction can be laid than its usual greatest depth.</summary>
        AfflictionDepth,
        /// <summary>Share. Reach of the bursts afflictions make: set off, and the burst of a death.</summary>
        BurstReach,
        /// <summary>Share. Damage of those bursts.</summary>
        BurstDamage,
        /// <summary>Share. How quickly damage dealt earns a flask back.</summary>
        FlaskEarn,
        /// <summary>Flat, whole. Leaps, blades or pillars the base spell has beyond its own.</summary>
        BaseSpellCount,
        /// <summary>Share. Width of the base spell's cone.</summary>
        BaseSpellArc,
        // The numbers of the moves written by hand for Fire and Lightning, and of Heat.
        /// <summary>Share. Stagger of Backdraft's cone.</summary>
        BackdraftStagger,
        /// <summary>Share. Reach of Backdraft's cone and of the detonation a perfect guard sets off.</summary>
        BackdraftReach,
        /// <summary>Share. How long Flame Trail burns on the ground.</summary>
        TrailSeconds,
        /// <summary>Share. Each bite of Flame Trail.</summary>
        TrailDamage,
        /// <summary>Share. Damage and stagger of Blink's shock.</summary>
        BlinkPower,
        /// <summary>Share. Width of the lane Blink shocks.</summary>
        BlinkWidth,
        /// <summary>Share. Damage and stagger of the copy Afterimage leaves.</summary>
        AfterimagePower,
        /// <summary>Share. How far Static's discharge reaches from the enemy struck.</summary>
        DischargeReach,
        /// <summary>Share. Stagger of the blow a stored charge is spent on.</summary>
        ChargeStagger,
        /// <summary>Flat, whole. Blows fewer for each level of Heat (never fewer than one a level).</summary>
        HeatBlows,
        /// <summary>Share. How long Heat lasts between blows before it cools a level.</summary>
        HeatCooling,
        /// <summary>Flat, whole. Levels of Heat above the usual five.</summary>
        HeatCap,
        /// <summary>Flat, whole. Critical blows a mark of light carries beyond its three.</summary>
        MarkBlows,
        /// <summary>Share. How long what the player grows stands.</summary>
        GrownSeconds,
        /// <summary>Share. How hard what the player grows bites.</summary>
        GrownDamage,
        /// <summary>Flat, whole. How many more of each kind of grown thing may stand.</summary>
        GrownLimit,
        /// <summary>Flat, seconds. How much longer an enemy the player throws into the air hangs.</summary>
        AirSeconds,
        /// <summary>Flat, whole. How many more blows than three can each hold an enemy up in the air; past three, each adds less.</summary>
        AirBlows,
        /// <summary>Flat, seconds. How much sooner the player counts as planted after a dodge.</summary>
        PlantedSooner,
        /// <summary>Flat. Health a point of missing mana costs when a spell is cast with too little: 0.5 is half a point. Zero: a spell without its mana is not cast.</summary>
        BloodPrice,
        /// <summary>Share. Prices paid in health: a spell's health cost, the blood price, a surge's drain.</summary>
        HealthPrices,
        /// <summary>Flat. Weapon damage gained for each point of grey health held (0.0075 is three quarters of a percent), to 30% at most.</summary>
        GreyStrength,
        /// <summary>Flat. The share of weapon damage dealt that comes back as health: 0.05 is five in a hundred.</summary>
        Leech,
        /// <summary>Share: how far the player's pushes throw (the Push action and what spell forms shove).</summary>
        PushDistance,
        /// <summary>Share: how much of a blow held on the guard still comes through as grey health.</summary>
        GuardGrey,
        /// <summary>Share: how long a freeze lasts.</summary>
        FreezeSeconds,
        /// <summary>Share: how hard a shatter lands.</summary>
        ShatterDamage,
        /// <summary>Share: the stagger of bursts afflictions make (a set-off, the burst of a death).</summary>
        BurstStagger,
        /// <summary>Share: how long ground laid by an effect or left by a spell lasts.</summary>
        GroundSeconds,
        /// <summary>Share: how wide that ground is.</summary>
        GroundSize,
        /// <summary>Share: how far an Arc effect leaps from one enemy to the next.</summary>
        LeapReach,
        /// <summary>Share: how long the timed boons of the player's marks last.</summary>
        BoonSeconds,
        /// <summary>Share: what weapon play takes off the spell and strike clocks (a landed blow, a perfect guard, a finisher).</summary>
        CooldownCut,
        /// <summary>Share: how fast the spell and strike clocks run out; 0.25 makes every wait a fifth shorter (it divides the wait by 1.25).</summary>
        SpellCooldown,
    }
}
