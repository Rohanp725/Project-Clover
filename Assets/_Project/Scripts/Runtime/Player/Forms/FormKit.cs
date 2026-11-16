using Clover.Core;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What the spell forms have in common in how they are seen: the element's colour as light, the mark that says
    /// something will land here, and the moment it does (the star, the ring, the cracks, the stain, the stop and the
    /// shake). One vocabulary, so twenty-four forms read as one game.
    /// </summary>
    public static class FormKit
    {
        public static readonly int Cast = Animator.StringToHash(PlayerAnimator.CastState);
        public static readonly int Brace = Animator.StringToHash(PlayerAnimator.ChargeState);
        public static readonly int Draw = Animator.StringToHash("attack_iaido");
        public static readonly int Chop = Animator.StringToHash("attack_heavy");
        public static readonly int Thrust = Animator.StringToHash("attack_light_3");
        public static readonly int Sweep = Animator.StringToHash("attack_light_1");
        public static readonly int Roll = Animator.StringToHash(PlayerAnimator.RollState);

        /// <summary>The element's colour as light: past white, so it blooms.</summary>
        public static Color Lit(Color colour, float gain = 2.6f, float alpha = 1f) => new Color(colour.r * gain * Brightness, colour.g * gain * Brightness, colour.b * gain * Brightness, alpha);

        /// <summary>How bright marks of light are drawn, over all: one number to turn the whole of it up or down against the bloom.</summary>
        public const float Brightness = 0.5f;

        /// <summary>The dark the element leaves on the ground.</summary>
        public static Color Stain(Color colour, float alpha = 0.6f) => new Color(colour.r * 0.1f, colour.g * 0.08f, colour.b * 0.14f, alpha);

        public static Quaternion Yaw(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction, Vector3.up) : Quaternion.identity;
        }

        /// <summary>The point a spell is aimed at, on the ground the caster stands on.</summary>
        public static Vector3 AimPoint(in SpellCast cast)
        {
            Vector3 at = cast.Target != null ? cast.Target.position : cast.Origin + cast.Forward * cast.Reach;
            at.y = cast.Origin.y;
            return at;
        }

        /// <summary>Marks a spot where something will land after some seconds: a rimmed disc of the size it will strike, a ring closing in on it, and a sign turning at its middle.</summary>
        public static void Warn(in SpellCast cast, Vector3 at, float radius, float seconds)
        {
            Color light = Lit(cast.Colour, 2.2f);
            FormFx.Lay(FormMark.Disc, at, radius * 2f, light, seconds + 0.05f);
            FormFx.Lay(FormMark.Ring, at, radius * 4.4f, Lit(cast.Colour, 3f), seconds, radius * 2.2f);
            FormFx.Lay(FormMark.Sigil, at, Mathf.Min(radius * 1.5f, 3.2f), light, seconds, -1f, 140f);
        }

        /// <summary>
        /// The moment something lands: a white star for a frame, a hard ring thrown outward, sparks of the element,
        /// a stain on the ground and, for a heavy landing, cracks that glow for a while, a flash at the screen's
        /// edges, a stop of time and a shake. <paramref name="weight"/> runs from about 0.3 (a blade) to 2 (a great sword).
        /// </summary>
        public static void Impact(in SpellCast cast, Vector3 at, float radius, float weight)
        {
            Color light = Lit(cast.Colour, 3.2f);
            Vector3 above = at + Vector3.up * 0.9f;
            float star = Mathf.Min(5.5f, radius * (1.5f + 0.5f * weight));
            FormFx.Face(FormMark.Star, above, star, Lit(cast.Colour, 4f), 0.11f, star * 1.3f);
            FormFx.Lay(FormMark.Ring, at, radius * 0.8f, light, 0.3f, radius * 2.5f);
            if (weight >= 0.9f) FormFx.Lay(FormMark.Ring, at, radius * 0.4f, Lit(cast.Colour, 2f), 0.42f, radius * 3.3f);
            FormFx.Lay(FormMark.Scorch, at, radius * 2.1f, Stain(cast.Colour), 2.5f + 2f * weight, -1f, 0f, Random.value * 360f);
            if (weight >= 0.6f) FormFx.Lay(FormMark.Crack, at, radius * (2f + 0.5f * weight), Lit(cast.Colour, 2f), 0.9f + 0.7f * weight, -1f, 0f, Random.value * 360f);
            ElementLook.Sparks(cast.Element, above, Vector3.up, 1f + weight);
            int puffs = Mathf.Clamp(Mathf.RoundToInt(3f + 3f * weight), 3, 8);
            for (int i = 0; i < puffs; i++)
                ElementLook.Puff(cast.Element, at + FormRules.OnRing(i, puffs, radius * 0.6f) + Vector3.up * 0.4f, Vector3.up, 0.5f + 0.25f * weight);
            if (weight >= 0.9f)
            {
                Vfx.Play(VfxKind.Impact, above);
                Flash(cast, 0.14f + 0.12f * weight, 0.07f);
            }
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.025f + 0.055f * weight, 0.1f + 0.22f * weight, 0.2f + 0.08f * weight);
        }

        /// <summary>The impact frame at the screen's edges, in the element's colour toward white.</summary>
        public static void Flash(in SpellCast cast, float strength, float seconds)
        {
            Color flash = Color.Lerp(Color.white, cast.Colour, 0.55f);
            flash.a = Mathf.Clamp01(strength);
            ScreenFlash.Flash(flash, seconds);
        }

        /// <summary>The light of the element gathering at a point: a sign snapping shut on it and a spray.</summary>
        public static void Gather(in SpellCast cast, Vector3 at, float size)
        {
            FormFx.Lay(FormMark.Sigil, at, size * 1.6f, Lit(cast.Colour, 2.6f), 0.35f, size, 220f);
            ElementLook.Sparks(cast.Element, at + Vector3.up * 1.1f, Vector3.up, 0.7f);
        }

        /// <summary>True when a thing a run was aimed at can still be struck.</summary>
        public static bool Alive(Component thing) => thing != null && thing.gameObject.activeInHierarchy;

        /// <summary>The enemy a cast is aimed at as a thing that can be struck; null when it faces no one. Asked once, when the spell is cast.</summary>
        public static Component Victim(in SpellCast cast) =>
            cast.Target != null ? cast.Target.GetComponentInParent<Clover.Gameplay.IDamageable>() as Component : null;
    }
}
