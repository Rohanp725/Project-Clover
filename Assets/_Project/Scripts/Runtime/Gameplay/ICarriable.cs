using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Something a moving spell can carry before it and throw down at its end (the ledger's carried-along: Wildfire Rush's
    /// Fire Front, Debts Called's The Black Buffalo).
    /// </summary>
    public interface ICarriable
    {
        /// <summary>Moves it along the ground to this point, as far as walls allow; false when it cannot be carried (a god, the dead).</summary>
        bool CarryTo(Vector3 point);

        /// <summary>Throws it down where it is, flat for this many seconds (a god only staggers, never down); false when it was not.</summary>
        bool ThrowDown(float seconds);
    }
}
