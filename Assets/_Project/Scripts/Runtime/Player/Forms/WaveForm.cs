using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>A wave: the element thrown ahead in a cone, striking everything in it at once. Fire's Flame Wave is this.</summary>
    public class WaveForm : ISpellForm
    {
        public string Id => SpellForms.Wave;

        public int Cast(in SpellCast cast)
        {
            int hits = cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, cast.Blow(cast.Damage, cast.Stagger));
            // The wave itself: the element thrown along the cone, a spray of it part-way out, and an arc of light at its front.
            Vector3 hand = cast.Origin + Vector3.up * 1.1f;
            for (int i = 1; i <= 4; i++)
                ElementLook.Puff(cast.Element, hand + cast.Forward * (cast.Reach * i / 4.5f), cast.Forward, 0.6f);
            ElementLook.Sparks(cast.Element, hand + cast.Forward * (cast.Reach * 0.6f), cast.Forward, 1.5f);
            cast.Caster.ShowArc(cast.Reach, cast.Spec.Arc, 1f);
            cast.Caster.ShowArc(cast.Reach * 0.7f, cast.Spec.Arc, -1f);
            return hits;
        }
    }
}
