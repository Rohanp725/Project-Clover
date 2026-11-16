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
        /// <summary>Seconds the blow sets its target burning; zero for none.</summary>
        public readonly float BurnSeconds;
        /// <summary>How much deeper the blow makes the burn; zero leaves its depth alone.</summary>
        public readonly int BurnStacks;
        /// <summary>Sets off whatever burn is on the target, all at once. Without a burn and without damage of its own, such a blow misses.</summary>
        public readonly bool Detonate;
        /// <summary>The affliction the blow lays, how many stacks of it, for how long (zero: its usual time) and how strongly (1: as written).</summary>
        public readonly AfflictionKind Affliction;
        public readonly int AfflictionStacks;
        public readonly float AfflictionSeconds;
        public readonly float AfflictionPower;
        /// <summary>True for a spell, false for a weapon: some afflictions let more of one through than of the other.</summary>
        public readonly bool Spell;
        /// <summary>True for a critical blow; its damage already includes what that adds.</summary>
        public readonly bool Critical;

        public Hit(float damage, float stagger, Vector3 direction, bool heavy = false, bool unblockable = false, Vector3? origin = null,
            float burnSeconds = 0f, int burnStacks = 0, bool detonate = false,
            AfflictionKind affliction = AfflictionKind.None, int afflictionStacks = 0, float afflictionSeconds = 0f, float afflictionPower = 1f,
            bool spell = false, bool critical = false)
        {
            Damage = damage;
            Stagger = stagger;
            Direction = direction;
            Heavy = heavy;
            Unblockable = unblockable;
            Origin = origin ?? default;
            HasOrigin = origin.HasValue;
            BurnSeconds = burnSeconds;
            BurnStacks = burnStacks;
            Detonate = detonate;
            Affliction = affliction;
            AfflictionStacks = afflictionStacks;
            AfflictionSeconds = afflictionSeconds;
            AfflictionPower = afflictionPower;
            Spell = spell;
            Critical = critical;
        }
    }
}
