namespace Clover.Player
{
    /// <summary>
    /// The moment an upgrade's effect answers. "The target" below is what the effect's action is aimed at when the
    /// effect says <see cref="EffectAt.Target"/>; when a trigger has none, the enemy the player faces stands in.
    /// </summary>
    public enum UpgradeTrigger
    {
        /// <summary>A light blow of the weapon lands (not blocked). Target: the enemy struck.</summary>
        LightHit,
        /// <summary>A heavy blow lands, the finisher included. Target: the enemy struck.</summary>
        HeavyHit,
        /// <summary>A finisher lands. Target: the enemy finished.</summary>
        FinisherHit,
        /// <summary>The weapon's skill lands (the Iaido Slash, the fists' skill). Target: the enemy struck.</summary>
        SkillHit,
        /// <summary>Any blow of the weapon lands. Target: the enemy struck.</summary>
        AnyHit,
        /// <summary>An enemy dies, by a blow or by what was laid on it. Target: where it fell.</summary>
        Kill,
        /// <summary>A perfect guard. Target: the attacker.</summary>
        PerfectGuard,
        /// <summary>A plain guard: a blow blocked for stamina. Target: the attacker.</summary>
        Guard,
        /// <summary>A perfect dodge. Target: none.</summary>
        PerfectDodge,
        /// <summary>Any dodge begins. Target: none.</summary>
        DodgeStart,
        /// <summary>Any dodge ends. Target: none.</summary>
        DodgeEnd,
        /// <summary>A blow lands on the player. Target: the attacker.</summary>
        Struck,
        /// <summary>The base spell or a worn spell is cast. Target: none.</summary>
        SpellCast,
        /// <summary>The base spell or a worn spell strikes an enemy; once for each enemy struck. Target: that enemy.</summary>
        SpellHit,
        /// <summary>The surge is lit. Target: none.</summary>
        SurgeStart,
        /// <summary>The surge runs out. Target: none.</summary>
        SurgeEnd,
        /// <summary>A flask is drunk (at the moment it heals). Target: none.</summary>
        FlaskDrunk,
        /// <summary>A flask is thrown. Having any effect on this trigger is what lets the flask be thrown (hold the flask button). Target: the enemy faced.</summary>
        FlaskThrown,
        /// <summary>A blow of the weapon or a spell strikes an enemy that already carries an affliction. Target: that enemy.</summary>
        AfflictedHit,
        /// <summary>An affliction on an enemy reaches full depth. Target: that enemy.</summary>
        AfflictionFull,
        /// <summary>Every so many seconds (the effect's Every) while an enemy is near. Target: none.</summary>
        EverySeconds,
        /// <summary>The player's health falls below a third. Fires once each time it falls there. Target: none.</summary>
        LowHealth,
        /// <summary>The player's mana becomes full. Fires once each time it fills. Target: none.</summary>
        ManaFull,
        /// <summary>A critical blow of the weapon lands. Target: the enemy struck.</summary>
        CritHit,
        /// <summary>The last blow of a light chain, or the heavy blow that ends one, lands. Target: the enemy struck.</summary>
        ChainEnd,
        /// <summary>An affliction on an enemy is set off or discharged by the player (the SetOff action, Lightning's discharge). Target: that enemy; carries the stacks spent.</summary>
        SetOff,
        /// <summary>A frozen enemy is shattered. Target: that enemy.</summary>
        Shatter,
        /// <summary>A ward (or a plate of stone) turns a blow and is spent. Target: none.</summary>
        WardSpent,
        /// <summary>The player throws an enemy into the air (or makes one that cannot be lifted reel). Target: that enemy.</summary>
        Lifted,
        /// <summary>An enemy thrown into the air comes down. Target: that enemy.</summary>
        Landed,
        /// <summary>Poison bites an enemy, on its beat or made to. Target: that enemy. Carries the depth it bit at (for PerStack).</summary>
        PoisonBite,
        /// <summary>A move that carried the charged blade's element landed (IMPLEMENTATION.md 3.3). Target: the first enemy it struck.</summary>
        ChargedBlow,
        /// <summary>The third and last blow of a charged blade landed. Target: that enemy.</summary>
        ChargedBlowThird,
        /// <summary>Two afflictions reacted on an enemy (IMPLEMENTATION.md 3.7); an effect with a <see cref="UpgradeEffect.Reaction"/> answers only that one. Target: that enemy.</summary>
        Reacted,
        /// <summary>A condition this effect's mark set came to its beat (<see cref="UpgradeEffect.Condition"/> names which). Target: the enemy carrying it.</summary>
        ConditionBeat,
        /// <summary>A blow landed on an enemy carrying a condition this effect's mark set (<see cref="UpgradeEffect.By"/> says which blows). Target: that enemy; the blow's damage is carried.</summary>
        ConditionStruck,
        /// <summary>A condition this effect's mark set ran out. Target: the enemy that carried it.</summary>
        ConditionEnded,
    }
}
