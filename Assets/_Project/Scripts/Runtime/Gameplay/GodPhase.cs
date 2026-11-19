namespace Clover.Gameplay
{
    /// <summary>
    /// Which of a god's two bodies is being fought, for how ice and holds take on it (<see cref="GodHolds"/>): none for
    /// anything that is not a god, the man for its first phase (Narukami, Khepharu as a man), the godly form for its
    /// second (the storm dragon, the Sun-Bearer).
    /// </summary>
    public enum GodPhase
    {
        None,
        Man,
        Godly,
    }
}
