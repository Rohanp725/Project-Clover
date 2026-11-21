using Clover.Player.Trees.Ways;

namespace Clover.Player.Way.Aspects
{
    /// <summary>
    /// The Answer (counter; way_plan.md 1): light blows a tenth weaker, and a perfect guard opening the answer, whose two
    /// moves are built here from the weapon's own clips (<see cref="Riposte"/>, <see cref="ThroughTheGap"/>) for the
    /// answer window to play once it exists. Turned Edge adds to a perfect guard's stagger; Guard Cut is the guard's light
    /// attack; Kiri-otoshi makes the light chain two cuts (its wider window is a stat on its mark). The window itself,
    /// the answer by itself and the red blow's keystone wait for their engine pieces (<see cref="KatanaWay"/>).
    /// </summary>
    public sealed class AnswerAspect : WeaponAspect
    {
        public const float LightShare = 0.9f;
        public const float TurnedEdgeStagger = 8f;
        public const float GuardCutDamage = 8f, GuardCutStagger = 6f;
        public const float RiposteDamage = 24f, RiposteStagger = 20f;
        public const float GapDamage = 30f, GapStagger = 40f, GapTravel = 3f;
        /// <summary>Seconds the answer stays open after a perfect guard, and with Open Window (for the window to come).</summary>
        public const float Window = 0.6f, OpenWindowSeconds = 0.8f;

        public override string Id => KatanaWay.Answer;
        public override string Weapon => Weapons.KatanaName;

        public override void Patch(Weapon weapon, IMarkQuery marks)
        {
            if (weapon?.Combo == null || weapon.Combo.Steps.Count == 0 || marks == null) return;
            var steps = weapon.Combo.Steps;
            for (int i = 0; i < steps.Count; i++) steps[i].Damage *= LightShare;
            if (marks.Has(KatanaWay.TurnedEdge)) weapon.PerfectGuardStagger += TurnedEdgeStagger;
            if (marks.Has(KatanaWay.GuardCut))
            {
                // NEEDS: its own clip (a short cut with the guard raised); the drawing cut's is played.
                var cut = CopyOf(weapon, 0);
                cut.Damage = GuardCutDamage;
                cut.Stagger = GuardCutStagger;
                cut.Lunge = 0f;
                weapon.Combo.GuardAttack = cut;
            }
            if (marks.Has(KatanaWay.KiriOtoshi))
                while (steps.Count > 2) steps.RemoveAt(steps.Count - 1);
        }

        /// <summary>The answer's light attack: a straight cut, critical (the answer window gives the critical), on the stepping cut's clip.</summary>
        public static AttackStep Riposte(Weapon weapon)
        {
            var step = CopyOf(weapon, 2) ?? CopyOf(weapon, 0);
            if (step == null) return null;
            step.Damage = RiposteDamage;
            step.Stagger = RiposteStagger;
            step.Sweep = 0f;
            return step;
        }

        /// <summary>The answer's heavy attack: a step through to the attacker's back, on the Iaido Slash's clip.</summary>
        public static AttackStep ThroughTheGap(Weapon weapon)
        {
            var from = weapon?.Combo?.Skill;
            if (from == null) return null;
            var step = from.Clone();
            step.Damage = GapDamage;
            step.Stagger = GapStagger;
            step.Travel = GapTravel;
            step.PassThrough = true;
            return step;
        }
    }
}
