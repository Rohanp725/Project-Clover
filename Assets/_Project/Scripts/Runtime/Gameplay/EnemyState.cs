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
        /// <summary>Taken up with something that is not an attack (drinking, holding, rising, searching) until it is over or it is broken out of it.</summary>
        Busy,
        Dead,
        /// <summary>Knocked flat by a Down condition: it falls on its back, lies while the condition lasts, then gets up. It begins nothing, cannot guard and does not flinch.</summary>
        Down,
    }
}
