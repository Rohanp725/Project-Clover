namespace Clover.Player
{
    /// <summary>What a weapon's blow sets going besides the blow itself.</summary>
    public enum StepEffect
    {
        None,
        /// <summary>A line of spikes breaks out of the ground ahead and runs away from the player.</summary>
        SpikeLine,
        /// <summary>The blow is thrown as a cone of the held element: sparks and a puff along it.</summary>
        ElementCone,
        /// <summary>A ring of the held element runs out along the ground from the blow.</summary>
        ElementRing,
        /// <summary>A line of sparks is dragged along the ground toward the target before the cut.</summary>
        GroundSparks,
        /// <summary>The ground splits along a line running on past the target a moment after the blow.</summary>
        GroundSplit,
        /// <summary>Dust and a ring thrown up where the blow came down: a hammer's head, a body driven into the ground.</summary>
        GroundSlam,
        /// <summary>Landmines: five arrows loosed upward come down in a ring round the enemy faced, and stand where they fall.</summary>
        ArrowRain,
    }
}
