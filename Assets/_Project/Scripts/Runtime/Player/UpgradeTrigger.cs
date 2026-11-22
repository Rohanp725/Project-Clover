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
        /// <summary>A condition this effect's mark set ran out. Target: the enemy that carried it. Carries the weapon blows it took (for PerStack).</summary>
        ConditionEnded,
        /// <summary>An enemy died while carrying a condition this effect's mark set. Target: where it fell; what it carried of Needs is carried (for PerStack).</summary>
        ConditionDied,
        /// <summary>
        /// An enemy's blow aimed at the player lands, or is turned: guarded, perfectly guarded or warded (the ledger's
        /// their-blow-moment). Target: the attacker.
        /// </summary>
        TheirBlow,
        /// <summary>The enemy carrying a condition this effect's mark set struck at the player, landing or turned. Target: that enemy; the weapon blows the condition took are carried (for PerStack).</summary>
        ConditionAttacked,
        /// <summary>A ripe seed burst (In Its Season): set off, its bearer dead, or a reaction on it. Target: that enemy; carries the seeds it held.</summary>
        RipeBurst,
        /// <summary>
        /// A reaction struck an enemy: the enemy it went off on, and each its own blows reached, each once for each time it
        /// went off (the ledger's reaction-struck). An effect's <see cref="UpgradeEffect.Reaction"/> or
        /// <see cref="UpgradeEffect.ReactionWith"/> picks which reactions. Target: that enemy.
        /// </summary>
        ReactionStruck,
        /// <summary>
        /// An affliction was laid on an enemy (the ledger's laying-answer); an effect's <see cref="UpgradeEffect.Spent"/> names
        /// which kind it answers. Target: that enemy; carries the stacks laid. What the answer itself lays is not answered again.
        /// </summary>
        AfflictionLaid,
        /// <summary>A dance whose every cut was struck in time has ended its cuts (Sky Dance's five rings all met). Target: none; asked of the spell's own branch.</summary>
        DanceInTime,
        /// <summary>A grown thing of the player's burst where it stood (a call, a withering: the ledger's grown-bursts). Target: none; the point is where it stood.</summary>
        GrownBurst,
        /// <summary>A grown thing of the player's threw a bolt into an enemy (Lightning Flower). Target: none; the point is where the grown thing stands.</summary>
        GrownBolt,
        /// <summary>
        /// A spell begun and held is loosed: a gathered one (SpellSpec.Rings) by its second press or its time, an ebb's flood
        /// (SpellSpec.Ebb) by its second press. Target: none; asked of the spell's own branch, its blows not yet landed.
        /// </summary>
        SpellLoosed,
        /// <summary>A ring (a wind) was added to a gathered spell. Target: none; asked of that spell's own branch; carries the rings it holds.</summary>
        RingAdded,
        /// <summary>A later press of a spell was made (Second Shadow's change of places). Target: none; asked of that spell's own branch.</summary>
        SpellPressed,
        /// <summary>
        /// A blade of yours standing in an enemy was driven in (PlantedBlades: a weapon blow, a mark, a flight arriving at it).
        /// Target: that enemy; carries how many times that blade has been driven (The Nail's third).
        /// </summary>
        BladeDriven,
        /// <summary>A turning blade of yours was met by a perfect guard as it came home to the hand (Returning Edge). Target: none.</summary>
        BladeCaught,
        /// <summary>
        /// A standing thing a mark raised was struck: asked only of the marks of the mark that raised it. The point is where it
        /// stands; By = StruckBy.Heavy asks for your heavy blow on it. Carries 1 when the blow broke it.
        /// </summary>
        StandingStruck,
        /// <summary>A standing thing a mark raised burst by itself (Cracked Wall struck by an enemy). The point is where it stood.</summary>
        StandingBurst,
        /// <summary>
        /// You walked over a pickup a mark dropped and took it (World.Pickups). Asked of the effects written beside the one that
        /// dropped it (a reaction's too), and of any effect with AnyDrops whose Drops names its kind. The point is where it lay; carries 1 when it was drawn to you.
        /// </summary>
        PickedUp,
        /// <summary>A pickup lay untaken until its time ran out (Shining Herb's herb flowers). The point is where it lay.</summary>
        PickupLeft,
        /// <summary>
        /// An enemy stands in ground that keeps acting, at one of its bites: asked of the effects whose Condition names that
        /// ground (UpgradeEffect.Ground). Target: the enemy standing in it; the point is where it stands.
        /// </summary>
        InGround,
        /// <summary>
        /// An enemy driven hard against the stones of a ring of your own (World.OwnGrounds.StoneRing) was stopped there (The
        /// Closed Ring). Target: that enemy.
        /// </summary>
        StoppedAtStones,
        /// <summary>A perfect dodge has run its length. The point is where it ended (Sun on the Spray, Wake along its path).</summary>
        PerfectDodgeEnd,
        /// <summary>A dodge carried you to an enemy that draws it (ConditionFlags.DrawsDodge: Eagle's Sight). Target: that enemy.</summary>
        CarriedTo,
        /// <summary>A counter's stance ran out with nothing struck at it (the parry form). Answered as the counter's own cast (Blown Through).</summary>
        CounterMissed,
        /// <summary>A plate of stone took its share of a blow and broke (Stone Skin). The point is where you stand.</summary>
        PlateBroken,
    }
}
