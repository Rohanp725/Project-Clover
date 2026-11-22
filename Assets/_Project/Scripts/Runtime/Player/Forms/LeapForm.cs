using Clover.Core;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A leap and a crater: out of the fight, untouchable in the air, and down beside the enemy faced as a heavy blow
    /// that leaves the ground burning (or soaking, or whatever the element leaves). Fire's Meteor Leap is this.
    /// </summary>
    public class LeapForm : ISpellForm, ISpellLanding
    {
        public string Id => SpellForms.Leap;

        public int Cast(in SpellCast cast)
        {
            Vector3 from = cast.Origin;
            Vector3 to = from + cast.Forward * 5f;
            if (cast.Target != null)
            {
                // Down beside them, not inside them.
                Vector3 gap = cast.Target.position - from;
                gap.y = 0f;
                to = from + gap.normalized * Mathf.Max(0f, gap.magnitude - 1f);
            }
            ElementLook.Puff(cast.Element, from + Vector3.up * 0.3f, Vector3.up, 1.4f);
            ElementLook.Ring(cast.Element, from + Vector3.up * 0.2f);
            cast.Player.Leap(to, cast.Spec.Seconds, cast.Spec.Height);
            return 0;
        }

        public void Landed(in SpellCast cast)
        {
            Vector3 at = cast.Player.transform.position;
            float radius = cast.Spec.Radius;
            var blow = cast.Blow(cast.Damage, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeArea(at, radius, blow);
            var crater = cast.Ground(at, radius * 0.8f, FormRules.Own(cast.Spec.Lasts, 4f), 0.5f, cast.Linger(cast.Spec.LingerDamage));
            crater.Delay = 0.2f;
            Hazards.Lay(crater);
            Vfx.Play(VfxKind.Impact, at + Vector3.up * 0.5f);
            Vfx.Play(VfxKind.Burst, at + Vector3.up * 0.6f);
            ElementLook.Ring(cast.Element, at + Vector3.up * 0.2f);
            for (int i = 0; i < 6; i++)
                ElementLook.Puff(cast.Element, at + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (radius * 0.6f) + Vector3.up * 0.4f, Vector3.up, 0.8f);
            Color flash = Color.Lerp(Color.white, cast.Colour, 0.6f);
            flash.a = 0.32f;
            ScreenFlash.Flash(flash, 0.07f);
            cast.Upgrades.Jolt(0.12f, 0.45f, 0.3f);
        }
    }
}
