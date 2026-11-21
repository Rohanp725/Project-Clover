namespace Clover.Player
{
    /// <summary>
    /// What an upgrade's effect does when its trigger comes. The fields of <see cref="UpgradeEffect"/> each one reads
    /// are listed with it; a field left at zero takes the value in brackets.
    /// </summary>
    public enum UpgradeAction
    {
        /// <summary>Lays stacks of an affliction on the target, or with a Radius on everything within it. Reads: Affliction, Stacks (1), Radius (0), At.</summary>
        Afflict,
        /// <summary>A burst of damage and stagger in a circle. Reads: Damage, Stagger, Radius (2.5), Delay, Affliction, Stacks, At.</summary>
        Burst,
        /// <summary>A cone thrown forward from the player, who turns to the target first. Reads: Damage, Stagger, Reach (4.5), Arc (90), Affliction, Stacks; Amount: each enemy struck thrown that many metres straight away from the player.</summary>
        Cone,
        /// <summary>Ground that bites what stands on it, for a while. Reads: Shape, Damage (each bite), Stagger, Radius (1) of each patch, Seconds (3), Interval (0.5) between bites, Count (4) patches for a line or ring, Reach (4): the line's length or the ring's radius, Delay, Affliction, Stacks, At.</summary>
        Hazard,
        /// <summary>An arc that strikes the target and leaps on to others near. Reads: Damage, Stagger, Count (2) leaps, Reach (5) of each leap, Affliction, Stacks; LeapsTo or FromTarget: it starts from the target without striking it.</summary>
        Arc,
        /// <summary>Something thrown that strikes each enemy it passes once. Reads: Flight, Damage, Stagger, Radius (0.9), Speed (10), Seconds (1.2) in the air, Count (1) thrown in a fan, Affliction, Stacks.</summary>
        Projectile,
        /// <summary>A blade or pillar that falls after a delay, on a spot marked on the ground. Reads: Damage, Stagger, Radius (2), Delay (0.6), Count (1); more than one fall round the first, Affliction, Stacks, At.</summary>
        Falling,
        /// <summary>Heals the player. Reads: Amount.</summary>
        Heal,
        /// <summary>Gives mana. Reads: Amount.</summary>
        Mana,
        /// <summary>Gives stamina. Reads: Amount.</summary>
        Stamina,
        /// <summary>A timed change to one of the player's numbers. Reads: Stat, Amount, Seconds (5).</summary>
        Boon,
        /// <summary>A ward that turns aside the next blow that would land. Reads: Count (1) wards given; no more than three are held.</summary>
        Ward,
        /// <summary>A short step through the air to the target. Reads: Reach (8), the furthest it goes; Amount: metres beyond the target to land (0 stops in front of it).</summary>
        Step,
        /// <summary>Drags enemies toward a point. Reads: Radius (5), Amount (3) metres, At. Shape Downwind: the target alone, toward the player.</summary>
        Pull,
        /// <summary>Throws enemies away from a point. Reads: Radius (5), Amount (3) metres, At. Shape Downwind: the target alone, straight away from the player (Thunder Gust).</summary>
        Push,
        /// <summary>Slows time for everything but the player. Reads: Seconds (2), Amount (0.35): the share of its speed the world keeps.</summary>
        SlowTime,
        /// <summary>A copy of the player is left where they stand and repeats the last blow a moment later. Reads: Amount (0.6): the share of that blow's damage; Delay (0.3), Radius (2.3), Stagger.</summary>
        Echo,
        /// <summary>A helper that strikes the target a few times and goes. Reads: Damage, Stagger, Count (3) strikes, Interval (0.6), Radius (2), Affliction, Stacks.</summary>
        Helper,
        /// <summary>Sets an affliction on the target off: its stacks are spent for a burst round it. Reads: Affliction (the element's own), Damage: added for each stack spent, Stagger for each stack, Radius (3).</summary>
        SetOff,
        /// <summary>Holds the target still: it cannot move, turn or begin an attack. Elites and gods are held for 0.4 of the time, the colossus not at all, as with Frozen. Reads: Seconds (1.5), Radius (0: the target alone), At.</summary>
        Hold,
        /// <summary>Adds to the count its own mark keeps (<see cref="Upgrade.Tally"/>), up to the cap. Reads: Count (1).</summary>
        Bank,
        /// <summary>Fills part of a flask; a whole one is added to the belt if there is room. Reads: Amount: the share of a flask (0.25).</summary>
        FlaskFill,
        /// <summary>Takes health from the player and leaves it grey, to be won back by blows. Never the last point. Reads: Amount.</summary>
        SpendHealth,
        /// <summary>Heals the player by a share of the damage of the last weapon blow. Reads: Amount: the share (0.1).</summary>
        Leech,
        /// <summary>Throws the target into the air: it hangs helpless and comes down, and blows hold it up a little longer. Elites and gods hang 0.6 of the time; what cannot be lifted reels instead and takes the Stagger. Reads: Seconds (1), Radius (0: the target alone), Stagger (40) for what only reels, At.</summary>
        Lift,
        /// <summary>Drives an enemy that is in the air into the ground at once. Reads: nothing.</summary>
        Drop,
        /// <summary>Bursts everything the player has grown, each where it stands, and heals for each. Reads: Damage, Stagger, Radius (3), Amount: health for each burst, Affliction, Stacks.</summary>
        BurstGrown,
        /// <summary>Casts a spell form by name (<see cref="UpgradeEffect.Form"/>) with the numbers in <see cref="UpgradeEffect.Spell"/>, at the trigger's enemy when it has one: how a god's gift or a crown is begun. Costs nothing.</summary>
        Form,
        /// <summary>Makes poison bite at once, out of turn: on the target, or with a Radius on everyone within it. Reads: Radius (0), Amount: health for each one bitten, At.</summary>
        Bite,
        /// <summary>Lays stacks on the nearest other enemy within reach of the target, and nothing on the target itself: a dose that leaps. Reads: Affliction, Stacks (1), Reach (4; longer with LeapReach).</summary>
        Pass,
        /// <summary>Sets a pair's reaction off on the target now, through its rest, when the target carries both. Reads: Reaction (the reaction's id).</summary>
        ForceReaction,
        /// <summary>Makes the target flinch: what it had begun is broken, unless it is steadfast. Reads: Seconds (0.4).</summary>
        Flinch,
        /// <summary>Sets an enemy condition (<see cref="Clover.Gameplay.EnemyConditions"/>) on the target, or with a Radius on the enemies near it (State.ToOthers: not on the target, each joined to it). Reads: State, Radius, Count (all within the radius), Affliction and Stacks laid on each that takes it.</summary>
        Condition,
        /// <summary>Takes stacks of an affliction off the target with no burst; the next effects of the same mark may count them (DamagePerSpent, PerStack). Reads: Affliction, Share (0: all; else that share, rounded up), Count (the most), CountOnly (count them and take none).</summary>
        Spend,
        /// <summary>
        /// A strike that runs down a lane from the target straight away from the player, patch by patch, as one blow: each
        /// enemy on it is struck once. Reads: Damage, Stagger, Affliction, Stacks, Reach (8) its length, Radius (0.6) half
        /// its width, Interval (0.03) seconds from one patch to the next, Delay; Reaction: the reaction whose designed
        /// look's bolt is drawn down it.
        /// </summary>
        Lane,
        /// <summary>Takes seconds off every running spell and strike clock, as a landed blow does (IMPLEMENTATION.md 3.1). Reads: Amount (seconds).</summary>
        CutCooldowns,
        /// <summary>Ends the clock of a branch's slot at once: its own branch's, or the one named in Branch. Nothing when that spell sits in no slot. Reads: Branch.</summary>
        ResetCooldown,
        /// <summary>The next cast of a branch's slot starts no clock (its own branch's, or Branch's), within Seconds (0: until it is cast). Reads: Branch, Seconds.</summary>
        InstantNext,
        /// <summary>Keeps the target frozen Seconds longer; nothing on one that is not frozen. Reads: Seconds.</summary>
        KeepFrozen,
        /// <summary>Raises the player's Heat by levels, while Heat is learned. Reads: Count (1).</summary>
        RaiseHeat,
        /// <summary>Turns the player's tide at once, high to low or low to high, while a mark keeps the tide. Reads: nothing.</summary>
        TurnTide,
        /// <summary>Makes the surge that burns last longer. Reads: Amount (seconds added), Seconds (the most it may last in all; 0 for no cap).</summary>
        LengthenSurge,
        /// <summary>Empties the count its own mark keeps (a spell's first press starting its tally again). Reads: nothing.</summary>
        ClearCount,
        /// <summary>A short step straight away from the target, through nothing solid. Reads: Reach (3) metres.</summary>
        StepBack,
        /// <summary>No blow can land on the player for a moment. Reads: Seconds (0.3).</summary>
        Untouchable,
        /// <summary>
        /// The player and the target change places: the target goes where the player stood as the spell was cast, the player
        /// where the target stood. An elite or a god is not moved: the player comes up behind it instead. Reads: nothing.
        /// </summary>
        SwapPlaces,
        /// <summary>The target keeps the affliction until it dies: it no longer fades (Stone Does Not Forgive). Reads: Affliction.</summary>
        KeepForGood,
    }
}
