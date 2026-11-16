using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>One blow as the target receives it.</summary>
    public readonly struct Hit
    {
        public readonly float Damage;
        public readonly float Stagger;
        /// <summary>The direction the blow travels along the ground, from attacker to target.</summary>
        public readonly Vector3 Direction;

        public Hit(float damage, float stagger, Vector3 direction)
        {
            Damage = damage;
            Stagger = stagger;
            Direction = direction;
        }
    }
}
