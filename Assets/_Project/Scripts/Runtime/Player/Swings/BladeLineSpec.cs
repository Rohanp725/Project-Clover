using System;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// One line along a weapon that sweeps when it is swung, in the weapon's own space (grip at the origin, business end
    /// along +Z): two points from the base to the tip, or three when the edge bends (a crescent), the way the cutting
    /// edge faces, and how thick the part is there. <see cref="BladeLine"/> carries one on a weapon prefab;
    /// <see cref="BladeLines"/> keeps each weapon's as a fallback and for checks.
    /// </summary>
    [Serializable]
    public struct BladeLineSpec
    {
        /// <summary>The weapon's carry name (<see cref="Weapon.Carry"/>), or the body's for bare hands.</summary>
        public string Carry;
        /// <summary>Which part of it this is: "edge", "upper limb", "weight".</summary>
        public string Part;
        public BladeKind Kind;
        /// <summary>From the base to the tip, in the part's own space.</summary>
        public Vector3[] Points;
        /// <summary>The way the cutting edge (or the striking face) faces, in the part's own space.</summary>
        public Vector3 EdgeSide;
        /// <summary>Half the depth of the blade or head at the edge, in metres.</summary>
        public float Thickness;

        public BladeLineSpec(string carry, string part, BladeKind kind, Vector3 edgeSide, float thickness, params Vector3[] points)
        {
            Carry = carry;
            Part = part;
            Kind = kind;
            Points = points;
            EdgeSide = edgeSide;
            Thickness = thickness;
        }

        /// <summary>True when the line has a base and a tip.</summary>
        public bool Valid => Points != null && Points.Length >= 2;

        /// <summary>Its length along its points, in its own space.</summary>
        public float Length => Valid ? BladeFit.Length(Points, Points.Length) : 0f;
    }
}
