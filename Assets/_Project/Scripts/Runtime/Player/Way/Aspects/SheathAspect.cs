using Clover.Gameplay;
using Clover.Player.Trees.Ways;
using Clover.World;
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
        /// <summary>A Hundred Days: every this many draws in a row with no blow taken leaves its cut hanging.</summary>
        public const int HangEvery = 4;
        /// <summary>How long the cut hangs, and the share of the draw it cuts again with, once to each enemy that walks into it.</summary>
        public const float HangSeconds = 0.6f, HangShare = 0.4f;

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

        // A Hundred Days counts its draws in a row in the run (AspectRun.Count); a blow taken sets it back to nothing.
        public override void Tick(ref AspectRun run, in AspectContext context)
        {
            if (context.Struck) run.Count = 0;
        }

        public override AspectBlow Blow(ref AspectRun run, AttackStep step, bool draw, IMarkQuery marks)
        {
            if (!draw || marks == null || !marks.Has(KatanaWay.AHundredDays)) return default;
            run.Count++;
            return new AspectBlow { HangingCut = run.Count % HangEvery == 0 };
        }

        /// <summary>
        /// The fourth draw's cut left hanging (question 18: the slash trail itself, held and fraying from its ends): for
        /// 0.6 s along the cut, it cuts once more, for 40% of the draw, each enemy that walks into it. NEEDS (way-hanging-cut):
        /// the trail held where it was cut; the patches show only as the world's quiet hazard motes in the slash colour.
        /// </summary>
        public static void LayHangingCut(AttackStep step, Vector3 origin, Vector3 forward, float damage, Color colour)
        {
            if (step == null) return;
            var print = Footprint.Of(step, origin, forward);
            var at = new Vector3[InfusionGround.MostPatches];
            int count = InfusionGround.ArcPatches(print, at);
            for (int i = 0; i < count; i++)
                Hazards.Lay(new HazardSpec
                {
                    Kind = HazardKind.Element,
                    Tint = colour,
                    Position = at[i] + Vector3.up,
                    Radius = InfusionGround.PatchRadius,
                    Seconds = HangSeconds,
                    Once = true,
                    Quiet = true,
                    Blow = new SpellBlow { Damage = damage * HangShare, Tint = colour },
                });
        }
    }
}
