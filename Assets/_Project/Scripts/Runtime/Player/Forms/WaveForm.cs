using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>A wave: the element thrown ahead in a cone, striking everything in it at once. Fire's Flame Wave is this.</summary>
    public class WaveForm : ISpellForm
    {
        public string Id => SpellForms.Wave;

        public int Cast(in SpellCast cast)
        {
            // Thin rays fanned across the cone (Seven Horses), each drawn as a streak, or the one cone.
            int hits;
            if (cast.Spec.Rays > 1)
            {
                hits = cast.Combat.StrikeRays(cast.Origin, cast.Forward, cast.Spec.Rays, cast.Spec.Arc, cast.Reach, cast.Blow(cast.Damage, cast.Stagger));
                for (int i = 0; i < cast.Spec.Rays; i++)
                {
                    Vector3 ray = FormRules.RayWay(cast.Forward, i, cast.Spec.Rays, cast.Spec.Arc);
                    FormFx.Run(FormMark.Streak, cast.Origin + ray * 0.6f + Vector3.up * 0.9f, ray, cast.Reach - 0.6f, 0.5f, FormKit.Lit(cast.Colour, 3.4f), 0.25f, 0.1f);
                }
                return hits;
            }
            hits = cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, cast.Blow(cast.Damage, cast.Stagger));
            // The wave itself: the element thrown along the cone, a spray of it part-way out, and an arc of light at its front.
            Vector3 hand = cast.Origin + Vector3.up * 1.1f;
            for (int i = 1; i <= 4; i++)
                ElementLook.Puff(cast.Element, hand + cast.Forward * (cast.Reach * i / 4.5f), cast.Forward, 0.6f);
            ElementLook.Sparks(cast.Element, hand + cast.Forward * (cast.Reach * 0.6f), cast.Forward, 1.5f);
            cast.Caster.ShowArc(cast.Reach, cast.Spec.Arc, 1f, cast.Element != null ? cast.Element.Glow : cast.Colour);
            cast.Caster.ShowArc(cast.Reach * 0.7f, cast.Spec.Arc, -1f, cast.Element != null ? cast.Element.Glow : cast.Colour);
            return hits;
        }
    }
}
