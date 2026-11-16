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
        /// <summary>A cone thrown forward from the player, who turns to the target first. Reads: Damage, Stagger, Reach (4.5), Arc (90), Affliction, Stacks.</summary>
        Cone,
        /// <summary>Ground that bites what stands on it, for a while. Reads: Shape, Damage (each bite), Stagger, Radius (1) of each patch, Seconds (3), Interval (0.5) between bites, Count (4) patches for a line or ring, Reach (4): the line's length or the ring's radius, Delay, Affliction, Stacks, At.</summary>
        Hazard,
        /// <summary>An arc that strikes the target and leaps on to others near. Reads: Damage, Stagger, Count (2) leaps, Reach (5) of each leap, Affliction, Stacks.</summary>
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
        /// <summary>Drags enemies toward a point. Reads: Radius (5), Amount (3) metres, At.</summary>
        Pull,
        /// <summary>Throws enemies away from a point. Reads: Radius (5), Amount (3) metres, At.</summary>
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
    }
}
