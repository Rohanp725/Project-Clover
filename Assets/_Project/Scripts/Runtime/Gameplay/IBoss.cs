namespace Clover.Gameplay
{
    /// <summary>What the bar across the bottom of the screen needs to know of whoever it is showing.</summary>
    public interface IBoss
    {
        /// <summary>True while the fight with it is on: it has seen the player and has not fallen.</summary>
        bool InFight { get; }
        float HealthFraction { get; }
        /// <summary>How near it is to losing its balance, from 0 to 1; 1 for as long as it is broken open.</summary>
        float BalanceFraction { get; }
    }
}
