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
        /// <summary>The slot cast it came from (<see cref="SpellCast.Cast"/>), so that cast's own landing charges the blade; 0 for none.</summary>
        public int Cast;
        /// <summary>The effect thrown on each thing struck, and the colour it is thrown in when it is one of the tintable kinds.</summary>
        public VfxKind Effect;
        public bool Tinted;
        public Color Tint;
        /// <summary>The element it is seen as when it lands, for the element art's hit (<see cref="Forms.HitArt"/>); null for none.</summary>
        public ElementInfo Look;
        /// <summary>A reaction dealt it: what it lays starts no reaction (<see cref="Hit.FromReaction"/>).</summary>
        public bool FromReaction;
        /// <summary>
        /// The tint a blow of the element on the weapon (its infused or charged strike) gives what it lands on
        /// (<see cref="Hit.Tint"/>, <see cref="Core.StruckLook"/>); clear for a spell's, a reaction's or an upgrade's.
        /// </summary>
        public Color Struck;
        /// <summary>
        /// Lands with no hit art or effect of its own: ground whose reaction draws it (Seized's film, whose bites threw a
        /// white flash sheet on every enemy in it every half second; the owner, 2026-11-20: "less flickery").
        /// </summary>
        public bool Unseen;
    }
}
