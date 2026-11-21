using Clover.Player.Trees.Ways;
using UnityEngine;

namespace Clover.Player.Way.Aspects
{
    /// <summary>
    /// The Sheath (iai; way_plan.md 1): the first cut of every chain is a draw from the sheath at 1.5 times (Sharp Draw
    /// 1.7) with 10 more stagger, and the chain is the draw and the return cut, the stepping cut gone. Light Scabbard takes
    /// stamina off the draw; A Hundred Days at the Shrine makes every light a draw, the chain one cut long, at 1.3 times.
    /// The sheathe as a beat, the held draw and its keystone, the step aside, Quick Sheath and Chiburi's flick wait for
    /// their clips and engine pieces (<see cref="KatanaWay"/>).
    /// </summary>
    public sealed class SheathAspect : WeaponAspect
    {
        public const float LightScabbardStamina = 3f;

        public override string Id => KatanaWay.Sheath;
        public override string Weapon => Weapons.KatanaName;

        public override void Patch(Weapon weapon, IMarkQuery marks)
        {
            if (weapon?.Combo == null || weapon.Combo.Steps.Count == 0 || marks == null) return;
            var steps = weapon.Combo.Steps;
            bool hundredDays = marks.Has(KatanaWay.AHundredDays);
            var draw = steps[0];
            draw.Damage *= hundredDays ? KatanaWay.HundredDaysShare : marks.Has(KatanaWay.SharpDraw) ? KatanaWay.SharpDrawShare : KatanaWay.DrawShare;
            draw.Stagger += KatanaWay.DrawStagger;
            if (marks.Has(KatanaWay.LightScabbard)) draw.StaminaCost = Mathf.Max(0f, draw.StaminaCost - LightScabbardStamina);
            // The draw and the return cut; under A Hundred Days the draw alone, again and again.
            int keep = hundredDays ? 1 : 2;
            while (steps.Count > keep) steps.RemoveAt(steps.Count - 1);
        }
    }
}
