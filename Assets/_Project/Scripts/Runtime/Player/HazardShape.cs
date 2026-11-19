namespace Clover.Player
{
    /// <summary>How the patches of a ground hazard are laid out.</summary>
    public enum HazardShape
    {
        /// <summary>One circle.</summary>
        Patch,
        /// <summary>A row of circles running forward from the player.</summary>
        Line,
        /// <summary>Circles set round a middle.</summary>
        Ring,
        /// <summary>A row of circles beyond the target, carried away from the player along their line to it (downwind): Reach is how far the middle one lands, Amount the gap between them (1).</summary>
        Downwind,
    }
}
