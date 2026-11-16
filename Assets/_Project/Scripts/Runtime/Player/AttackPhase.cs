namespace Clover.Player
{
    public enum AttackPhase
    {
        None,
        /// <summary>The heavy button is held: the player stands ready and has not yet attacked.</summary>
        Charge,
        Windup,
        Active,
        Recovery,
    }
}
