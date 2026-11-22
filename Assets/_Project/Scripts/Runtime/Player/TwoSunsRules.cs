using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Two Suns (trees_b.md, light.twosuns; the ledger's two-suns): when you cast a spell at an enemy, a figure of light
    /// stands on its far side, as far from it as you are (six metres at most), and a moment later casts the same spell back
    /// toward you at a share of its damage. The plain numbers and where the figure stands.
    /// </summary>
    public static class TwoSunsRules
    {
        /// <summary>The figure casts this long after you, at this share of the spell's damage and stagger.</summary>
        public const float Delay = 0.4f, Share = 0.6f;
        /// <summary>It stands no further than this from the enemy, and at least this far.</summary>
        public const float MostOff = 6f, LeastOff = 1.5f;

        /// <summary>
        /// Where the figure stands: past the enemy at <paramref name="enemy"/>, on the line from the caster at
        /// <paramref name="caster"/>, as far beyond it as the caster stands before it. <paramref name="facing"/> is the way it
        /// casts, back toward the caster.
        /// </summary>
        public static Vector3 FarSide(Vector3 caster, Vector3 enemy, out Vector3 facing)
        {
            Vector3 across = enemy - caster;
            across.y = 0f;
            float off = Mathf.Clamp(across.magnitude, LeastOff, MostOff);
            Vector3 way = across.sqrMagnitude > 0.0001f ? across.normalized : Vector3.forward;
            facing = -way;
            Vector3 at = enemy + way * off;
            at.y = caster.y;
            return at;
        }

        /// <summary>
        /// True when a spell of this form can be cast again by the figure: not one that moves your own body (a rush, a leap, a
        /// drill, a dance), sets your own self down (a heart, a copy), or is held, gathered or pressed again.
        /// </summary>
        public static bool Echoes(string form, SpellSpec spec)
        {
            if (string.IsNullOrEmpty(form)) return false;
            if (spec != null && (spec.HeldStance || spec.Rings > 0 || spec.Ebb > 0f || spec.HoldUpTo > 0f)) return false;
            switch (form)
            {
                case SpellForms.Drill:
                case SpellForms.Step:
                case SpellForms.Rush:
                case SpellForms.Leap:
                case SpellForms.Circle:
                case SpellForms.SkyDance:
                case SpellForms.Clone:
                case SpellForms.Heart:
                case SpellForms.Parry:
                case SpellForms.Eye:
                case SpellForms.FrozenWave:
                case SpellForms.Flashes:
                case SpellForms.Reverse:
                    return false;
                default:
                    return true;
            }
        }
    }
}
