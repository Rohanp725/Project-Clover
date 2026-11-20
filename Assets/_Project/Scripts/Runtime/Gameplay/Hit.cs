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
        /// <summary>
        /// A second affliction laid by the same blow, with its stacks and seconds (zero: its usual time); it shares the
        /// first's strength. A charged blade's element rides here when it differs from what the blow lays already
        /// (IMPLEMENTATION.md 3.3); a blow carrying two roots will too (Phase 2).
        /// </summary>
        public readonly AfflictionKind Affliction2;
        public readonly int Affliction2Stacks;
        public readonly float Affliction2Seconds;
        /// <summary>
        /// True for a blow a reaction dealt (or the ground it left): what it lays starts no reaction, and no condition
        /// answers it, so a reaction never feeds on itself (IMPLEMENTATION.md 3.7).
        /// </summary>
        public readonly bool FromReaction;

        /// <summary>
        /// The colour of the element the weapon carries, which a landed blow's flash takes softened
        /// (<see cref="Clover.Core.StruckLook"/>); clear (alpha zero) for a plain weapon, a spell or an enemy's blow.
        /// </summary>
        public readonly Color Tint;

        /// <summary>
        /// True for a bite that only lays an affliction (a soaking slick, a dosing patch): no damage, no balance, not
        /// heavy, nothing set off. It is felt as the affliction, not as a blow: no shove, flinch, poise or guard.
        /// </summary>
        public bool OnlyLays => Damage <= 0f && Stagger <= 0f && !Heavy && !Detonate;

        public Hit(float damage, float stagger, Vector3 direction, bool heavy = false, bool unblockable = false, Vector3? origin = null,
            float burnSeconds = 0f, int burnStacks = 0, bool detonate = false,
            AfflictionKind affliction = AfflictionKind.None, int afflictionStacks = 0, float afflictionSeconds = 0f, float afflictionPower = 1f,
            bool spell = false, bool critical = false,
            AfflictionKind affliction2 = AfflictionKind.None, int affliction2Stacks = 0, float affliction2Seconds = 0f,
            bool fromReaction = false, Color tint = default)
        {
            FromReaction = fromReaction;
            Tint = tint;
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
            Affliction2 = affliction2;
            Affliction2Stacks = affliction2Stacks;
            Affliction2Seconds = affliction2Seconds;
        }
    }
}
