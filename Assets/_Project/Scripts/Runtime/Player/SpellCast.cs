using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Everything a spell form is told when it is cast: who casts, in which element, at what, with which numbers.
    /// <see cref="Damage"/>, <see cref="Stagger"/> and <see cref="Reach"/> already carry what the player's upgrades
    /// add to spells; the rest of the numbers are read from <see cref="Spec"/> as written.
    /// </summary>
    public struct SpellCast
    {
        /// <summary>The spell's name, for a word on the screen.</summary>
        public string Name;
        /// <summary>The upgrade that taught it; null for the element's base spell.</summary>
        public Upgrade Source;
        public SpellSpec Spec;
        public float Damage;
        public float Stagger;
        public float Reach;

        public PlayerController Player;
        public PlayerCombat Combat;
        public PlayerDefence Defence;
        public PlayerElement Caster;
        public PlayerUpgrades Upgrades;

        /// <summary>The element it is cast in: its colour as paint, and what it lays on enemies.</summary>
        public ElementInfo Element;
        public Color Colour;
        public AfflictionKind Affliction;
        /// <summary>The number of the slot cast this is (<see cref="PlayerElement.CastTag"/>), carried by every blow it deals; 0 for a spell an effect throws.</summary>
        public int Cast;

        /// <summary>The enemy the player faces within reach, or null; where the player stands and faces; and the point aimed at (the target, or as far ahead as the spell reaches).</summary>
        public Transform Target;
        public Vector3 Origin;
        public Vector3 Forward;
        public Vector3 Aim;

        /// <summary>
        /// One blow of this spell: the damage and stagger given, the element's affliction as deep as the spell lays it
        /// (or as deep as <paramref name="stacks"/> when that is given), seen in the element's own way.
        /// </summary>
        public SpellBlow Blow(float damage, float stagger, int stacks = -1) => new SpellBlow
        {
            Damage = damage,
            Stagger = stagger,
            Affliction = Affliction,
            Stacks = stacks >= 0 ? stacks : Spec != null ? Spec.Stacks : 0,
            AfflictionSeconds = Spec != null ? Spec.AfflictionSeconds : 0f,
            Spell = true,
            Cast = Cast,
            Effect = ElementLook.HitEffect(Element),
            Tinted = ElementLook.Tints(Element),
            Tint = Colour,
            Look = Element,
        };

        /// <summary>
        /// A patch of this element's ground at a point, for <see cref="Hazards.Lay(in HazardSpec)"/>: it bites every
        /// <paramref name="interval"/> seconds for the blow given. The caller sets anything else it wants on what comes back.
        /// </summary>
        public HazardSpec Ground(Vector3 at, float radius, float seconds, float interval, in SpellBlow blow) => new HazardSpec
        {
            Kind = ElementLook.Ground(Element),
            Tint = Colour,
            Position = at,
            Radius = radius * (Upgrades != null ? Upgrades.Stats.Multiplier(PlayerStat.GroundSize) : 1f),
            Seconds = seconds * (Upgrades != null ? Upgrades.Stats.Multiplier(PlayerStat.GroundSeconds) : 1f),
            BiteEvery = interval,
            Blow = blow,
        };

        /// <summary>What a spell leaves burning, soaking or crawling on the ground: a light bite that keeps the affliction on whatever stands in it.</summary>
        public SpellBlow Linger(float damage)
        {
            var blow = Blow(damage, 0f, 1);
            // Fire's ground has always kept its burn topped up without deepening it.
            if (Affliction == AfflictionKind.Burn)
            {
                blow.Stacks = 0;
                blow.AfflictionSeconds = 2f;
            }
            return blow;
        }
    }
}
