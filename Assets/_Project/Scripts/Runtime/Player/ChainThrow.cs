namespace Clover.Player
{
    /// <summary>How a step of the Chain Sickle sends the chain out, for <see cref="ChainLine"/> to draw.</summary>
    public enum ChainThrow
    {
        None,
        /// <summary>The weight flies straight down the lane to the step's reach and is whipped back.</summary>
        Lane,
        /// <summary>The weight is swung round the bearer at the step's reach, two full turns.</summary>
        Whirl,
        /// <summary>The sickle itself goes out on the chain and is swept across the front, right to left.</summary>
        Sweep,
    }
}
