using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// One blow of a spell or of an upgrade's effect, as the caster describes it: what it deals, what it lays, and
    /// what is seen where it lands. The player's weapon code turns it into a <see cref="Hit"/> for each thing struck.
    /// </summary>
    public struct SpellBlow
    {
        public float Damage;
        public float Stagger;
        /// <summary>The affliction it lays and how deep. A burn may be laid with no depth at all: only its seconds.</summary>
        public AfflictionKind Affliction;
        public int Stacks;
        /// <summary>Seconds the affliction lasts; zero for its usual time.</summary>
        public float AfflictionSeconds;
        /// <summary>A heavy blow finishes a staggered enemy and shatters a frozen one.</summary>
        public bool Heavy;
        /// <summary>Sets off whatever burn is on the target.</summary>
        public bool Detonate;
        /// <summary>True for the base spell and worn spells: these count as spells for Soaked, for spell damage, and for the spell-hit trigger.</summary>
        public bool Spell;
        /// <summary>The effect thrown on each thing struck, and the colour it is thrown in when it is one of the tintable kinds.</summary>
        public VfxKind Effect;
        public bool Tinted;
        public Color Tint;
    }
}
