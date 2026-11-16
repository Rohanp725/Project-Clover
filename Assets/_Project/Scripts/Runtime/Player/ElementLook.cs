using Clover.Core;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// How an element is seen. Fire and Lightning have effects of their own (flame, embers, sparks, bolts from the
    /// sky); every other element throws the white effects in its colour. Spell forms and upgrade effects ask here,
    /// so one form looks right in every element.
    /// </summary>
    public static class ElementLook
    {
        private static bool IsFire(ElementInfo element) => element != null && element.Name == Elements.Fire;
        private static bool IsLightning(ElementInfo element) => element != null && element.Name == Elements.Lightning;

        /// <summary>The effect thrown on something this element strikes.</summary>
        public static VfxKind HitEffect(ElementInfo element) => IsFire(element) ? VfxKind.Fire : IsLightning(element) ? VfxKind.Shock : VfxKind.TintPuff;

        /// <summary>True when that effect must be thrown in the element's colour.</summary>
        public static bool Tints(ElementInfo element) => !IsFire(element) && !IsLightning(element);

        /// <summary>What kind of ground this element leaves.</summary>
        public static HazardKind Ground(ElementInfo element) => IsFire(element) ? HazardKind.Fire : IsLightning(element) ? HazardKind.Lightning : HazardKind.Element;

        /// <summary>A puff of the element: a tongue of flame, a crackle, or a mote of its colour.</summary>
        public static void Puff(ElementInfo element, Vector3 at, Vector3 direction, float scale)
        {
            if (IsFire(element)) Vfx.Play(VfxKind.Fire, at, direction, scale);
            else if (IsLightning(element)) Vfx.Play(VfxKind.Shock, at, direction, scale);
            else Vfx.Play(VfxKind.TintPuff, at, direction, scale, element != null ? element.Colour : Color.white);
        }

        /// <summary>A spray of the element: embers, sparks, or streaks of its colour.</summary>
        public static void Sparks(ElementInfo element, Vector3 at, Vector3 direction, float scale)
        {
            if (IsFire(element)) Vfx.Play(VfxKind.Embers, at, direction, scale);
            else if (IsLightning(element)) Vfx.Play(VfxKind.Shock, at, direction, scale);
            else Vfx.Play(VfxKind.TintSpark, at, direction, scale, element != null ? element.Colour : Color.white);
        }

        /// <summary>A ring widening on the ground.</summary>
        public static void Ring(ElementInfo element, Vector3 at)
        {
            if (Tints(element)) Vfx.Play(VfxKind.TintRing, at, Vector3.up, 1f, element != null ? element.Colour : Color.white);
            else Vfx.Play(VfxKind.Ring, at, Vector3.up);
        }

        /// <summary>The element striking one point: a bolt from the sky, a gout of flame, or a column of its colour.</summary>
        public static void Strike(ElementInfo element, Vector3 at, float flash)
        {
            if (IsLightning(element))
            {
                LightningBolts.Strike(at, flash);
                return;
            }
            Puff(element, at + Vector3.up * 0.6f, Vector3.up, 1.4f);
            Sparks(element, at + Vector3.up * 1.1f, Vector3.up, 1.6f);
            Ring(element, at + Vector3.up * 0.2f);
        }
    }
}
