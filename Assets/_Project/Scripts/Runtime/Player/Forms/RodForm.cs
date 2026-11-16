using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A rod planted a little ahead of the player: for a while the element strikes inside a circle round it, again
    /// and again. Lightning's Lightning Rod is this.
    /// </summary>
    public class RodForm : ISpellForm
    {
        public string Id => SpellForms.Rod;

        public int Cast(in SpellCast cast)
        {
            Vector3 at = cast.Origin + cast.Forward * cast.Spec.Reach;
            ElementLook.Strike(cast.Element, at, 0.6f);
            var storm = cast.Ground(at, cast.Spec.Radius, cast.Spec.Seconds, cast.Spec.Interval, cast.Blow(cast.Damage, cast.Stagger));
            storm.Delay = cast.Spec.Delay;
            Hazards.Lay(storm);
            if (cast.Defence != null) cast.Defence.Callout(cast.Name);
            return 0;
        }
    }
}
