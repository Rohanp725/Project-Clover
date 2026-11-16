namespace Clover.Gameplay
{
    /// <summary>What a colossus can do to the floor the player stands on.</summary>
    public enum ColossusAttack
    {
        None,
        /// <summary>One hand comes down on a circle. Can be guarded.</summary>
        Slam,
        /// <summary>One hand crosses the whole floor along a lane. Red.</summary>
        Sweep,
        /// <summary>Bolts called down one after another on where the player stands. Red.</summary>
        Bolts,
        /// <summary>The head breathes lightning down a lane from the north rim. Red.</summary>
        Breath,
        /// <summary>The head lunges down on whoever stands under it, and is stuck there for a moment. Can be guarded.</summary>
        Bite,
        /// <summary>Both hands close on the player from either side. Can be guarded.</summary>
        Clap,
    }
}
