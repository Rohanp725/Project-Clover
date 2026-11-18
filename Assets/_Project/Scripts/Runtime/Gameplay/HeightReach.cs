using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// How far up or down a blow struck by hand reaches. A body on a roof, a deck or a ledge is out of reach of a blade
    /// swung on the floor below it, however near it stands on the map, and the same holds the other way. Measured from
    /// the striker's feet to the target's feet: a step, a stair or a low plinth is still in reach.
    /// </summary>
    public static class HeightReach
    {
        /// <summary>The most, in metres, a target's feet may stand above or below the striker's for a blow by hand to land.</summary>
        public const float Melee = 1.2f;

        public static bool Within(float strikerFeet, float targetFeet, float reach = Melee) => Mathf.Abs(targetFeet - strikerFeet) <= reach;
    }
}
