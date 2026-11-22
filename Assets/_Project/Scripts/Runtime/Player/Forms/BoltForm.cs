using Clover.Core;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>A bolt: the element strikes the enemy faced and leaps on to those near it. Lightning's Thunderclap is this.</summary>
    public class BoltForm : ISpellForm
    {
        public string Id => SpellForms.Bolt;

        public int Cast(in SpellCast cast)
        {
            // A spell aimed at what it seeks (SpellSpec.AimAt) starts from that enemy, not the one faced.
            var first = cast.Spec.AimAt != Clover.Gameplay.AfflictionKind.None ? cast.Target : null;
            int hits = cast.Combat.StrikeChain(cast.Reach, cast.Blow(cast.Damage, cast.Stagger), cast.Spec.Count, cast.Spec.Radius, cast.Element, first);
            ElementLook.Sparks(cast.Element, cast.Origin + Vector3.up * 1.1f + cast.Forward * 0.6f, cast.Forward, 1.2f);
            // The whole screen takes a breath of the element's colour.
            Color flash = Color.Lerp(Color.white, cast.Colour, 0.45f);
            flash.a = 0.22f;
            ScreenFlash.Flash(flash, 0.06f);
            return hits;
        }
    }
}
