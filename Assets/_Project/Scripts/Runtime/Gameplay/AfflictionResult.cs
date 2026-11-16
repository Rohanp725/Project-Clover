namespace Clover.Gameplay
{
    /// <summary>What came of laying an affliction on something.</summary>
    public readonly struct AfflictionResult
    {
        /// <summary>True when anything was laid at all.</summary>
        public readonly bool Applied;
        /// <summary>True when this laying is the one that brought it to full depth.</summary>
        public readonly bool ReachedFull;
        /// <summary>True when what it was laid on carries it on to those standing near.</summary>
        public readonly bool Carried;

        public AfflictionResult(bool applied, bool reachedFull, bool carried)
        {
            Applied = applied;
            ReachedFull = reachedFull;
            Carried = carried;
        }
    }
}
