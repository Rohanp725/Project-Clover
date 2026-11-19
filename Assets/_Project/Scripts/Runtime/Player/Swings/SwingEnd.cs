namespace Clover.Player.Swings
{
    /// <summary>What an element's trail does once the swing that drew it is over.</summary>
    public enum SwingEnd
    {
        /// <summary>It fades as it was going to.</summary>
        None,
        /// <summary>Water: the arc throws its water on off the end of the swing, in drops that fall.</summary>
        Spray,
        /// <summary>Ice: the frozen arc hangs, then shatters into shards that fall.</summary>
        Shatter,
    }
}
