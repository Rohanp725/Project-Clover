namespace Clover.Gameplay
{
    public enum EnemyAttackPhase
    {
        Rest,
        /// <summary>The attack is shown before it lands: this is the time the player has to answer it.</summary>
        Telegraph,
        /// <summary>The blow has landed and the attacker is following through.</summary>
        Strike,
    }
}
