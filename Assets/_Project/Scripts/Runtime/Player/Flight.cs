namespace Clover.Player
{
    /// <summary>How a thrown thing travels.</summary>
    public enum Flight
    {
        /// <summary>Straight ahead.</summary>
        Straight,
        /// <summary>Turning toward the enemy it was thrown at.</summary>
        Homing,
        /// <summary>Out, and then back to the player, striking again on the way home.</summary>
        Returning,
    }
}
