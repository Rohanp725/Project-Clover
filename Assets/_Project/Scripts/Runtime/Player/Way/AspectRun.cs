namespace Clover.Player.Way
{
    /// <summary>
    /// What an aspect keeps while it is carried (IMPLEMENTATION.md 2.5): the counters and windows of its rules, the
    /// Answer's open window, the Seven Drums beaten, a Hundred Days' draws in a row. A plain value the player holds,
    /// one per carried weapon, so the aspects themselves stay stateless like the spell forms.
    /// </summary>
    public struct AspectRun
    {
        /// <summary>Seconds left on the aspect's open window (the Answer's), or zero.</summary>
        public float Window;
        /// <summary>A count the aspect keeps (drums beaten, draws in a row).</summary>
        public int Count;
        /// <summary>Seconds since the count last rose.</summary>
        public float SinceCount;
        /// <summary>A second count, for the aspects that keep two.</summary>
        public int Stage;

        /// <summary>Everything back to nothing: a rest, a change of aspect.</summary>
        public void Clear()
        {
            Window = 0f;
            Count = 0;
            SinceCount = 0f;
            Stage = 0;
        }
    }
}
