using System.Collections.Generic;
using Clover.Player.Trees.Ways;
using UnityEngine;

namespace Clover.Player.Way.Aspects
{
    /// <summary>
    /// Narukami's Storm Katana, the god's bough (way_plan.md 1): the god's sword itself (<see cref="Weapons.StormKatana"/>),
    /// with his chain across, back and down, the third a downward cut (22) in a 3 m lane, on the stepping cut's clip until
    /// its own is made. Long Blade lengthens every cut; The Red Draw is the held heavy, a long lane passed through, on the
    /// Thunder Draw's clip; The Falling Cut lengthens the third cut's lane. The drums, the flank step, Raikiri and the drum
    /// ring wait for their clips and engine pieces (<see cref="KatanaWay"/>).
    /// </summary>
    public sealed class StormKatanaBough : WeaponAspect
    {
        public const float FallingDamage = 22f, FallingReach = 3f, FallingWidth = 1.2f, FallingCutReach = 1f;
        public const float LongBladeReach = 0.2f;
        public const float RedDrawDamage = 40f, RedDrawStagger = 50f, RedDrawTravel = 10f, RedDrawWidth = 1.2f, RedDrawHold = 0.6f;

        public override string Id => KatanaWay.Storm;
        public override string Weapon => Weapons.KatanaName;
        public override bool GodBough => true;

        public override Weapon Build() => Weapons.StormKatana();

        // The drums (Seven Drums, Drumbeat, Rolling Drums): Way/Drums.cs. NEEDS (way-drum-looks): the seven drum marks on the
        // scabbard lighting one by one (way_build_plan.md question 15).
        public override void Tick(ref AspectRun run, in AspectContext context)
        {
            var marks = context.Marks;
            if (marks == null || !marks.Has(KatanaWay.SevenDrums)) return;
            Drums.Tick(ref run, context, marks.Has(KatanaWay.Drumbeat), marks.Has(KatanaWay.RollingDrums));
        }

        public override AspectBlow Blow(ref AspectRun run, AttackStep step, bool draw, IMarkQuery marks)
        {
            if (marks == null || !marks.Has(KatanaWay.SevenDrums)) return default;
            return new AspectBlow { Stagger = Drums.SpendOnDraw(ref run, draw) };
        }

        public override void Patch(Weapon weapon, IMarkQuery marks)
        {
            if (weapon?.Combo == null || marks == null) return;
            var third = StepAt(weapon, 2);
            if (third != null)
            {
                // NEEDS: its own clip, a downward cut; the stepping cut's is played.
                third.Damage = FallingDamage;
                third.Reach = FallingReach + (marks.Has(KatanaWay.TheFallingCut) ? FallingCutReach : 0f);
                third.LineWidth = FallingWidth;
            }
            if (marks.Has(KatanaWay.TheRedDraw) && weapon.Combo.Skill != null)
            {
                // NEEDS: its mana price (40), untouchable on the lane, and a clip of its own; the Thunder Draw's is played.
                var red = weapon.Combo.Skill.Clone();
                red.Damage = RedDrawDamage;
                red.Stagger = RedDrawStagger;
                red.Travel = RedDrawTravel;
                red.PassThrough = true;
                red.LineWidth = RedDrawWidth;
                red.Lunge = 0f;
                weapon.Combo.ChargedHeavy = red;
                weapon.Combo.ChargeTime = RedDrawHold;
                // On the old rule (the skill from a held heavy) the skill is held for longer than the red draw.
                weapon.Combo.SkillChargeTime = Mathf.Max(weapon.Combo.SkillChargeTime, RedDrawHold * 2f);
            }
            if (marks.Has(KatanaWay.LongBlade))
            {
                // Each step once, however many places it fills.
                var seen = new List<AttackStep>();
                foreach (var step in weapon.AllSteps())
                {
                    if (step == null || seen.Contains(step)) continue;
                    seen.Add(step);
                    step.Reach += LongBladeReach;
                }
            }
        }
    }
}
