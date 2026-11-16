namespace Clover.Gameplay
{
    public enum EnemyState
    {
        /// <summary>Has not noticed the target.</summary>
        Idle,
        /// <summary>Aware of the target and between attacks: closing in, backing off or holding its ground.</summary>
        Stalk,
        /// <summary>Showing an attack that has not landed yet.</summary>
        Telegraph,
        /// <summary>The blow has landed; following through.</summary>
        Strike,
        /// <summary>Open after an attack.</summary>
        Recover,
        /// <summary>Knocked off balance by a blow for a moment.</summary>
        Flinch,
        /// <summary>Stagger meter full: helpless until it passes or the finisher comes.</summary>
        Staggered,
        Dead,
    }
}
