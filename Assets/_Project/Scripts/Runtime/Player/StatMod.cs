namespace Clover.Player
{
    /// <summary>One change to one of the player's numbers: which, and by how much. See <see cref="PlayerStat"/> for what an amount means.</summary>
    public readonly struct StatMod
    {
        public readonly PlayerStat Stat;
        public readonly float Amount;

        public StatMod(PlayerStat stat, float amount)
        {
            Stat = stat;
            Amount = amount;
        }
    }
}
