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
        /// <summary>A heavy blow finishes a staggered enemy.</summary>
        public readonly bool Heavy;
        /// <summary>A red attack: holding guard does not stop it. Only a dodge or a perfect guard does.</summary>
        public readonly bool Unblockable;
        /// <summary>Where the attacker stands, when it said so; a guard uses it to tell front from behind.</summary>
        public readonly Vector3 Origin;
        public readonly bool HasOrigin;

        public Hit(float damage, float stagger, Vector3 direction, bool heavy = false, bool unblockable = false, Vector3? origin = null)
        {
            Damage = damage;
            Stagger = stagger;
            Direction = direction;
            Heavy = heavy;
            Unblockable = unblockable;
            Origin = origin ?? default;
            HasOrigin = origin.HasValue;
        }
    }
}
