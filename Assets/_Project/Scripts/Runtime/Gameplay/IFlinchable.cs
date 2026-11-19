namespace Clover.Gameplay
{
    /// <summary>Something that can be made to flinch by an effect, out of a blow: what it had begun is broken unless it is steadfast.</summary>
    public interface IFlinchable
    {
        /// <summary>Flinches for this many seconds (0: its own usual). False when it did not (dead, staggered, or mid-blow).</summary>
        bool Flinch(float seconds);
    }
}
