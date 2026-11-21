using Clover.Core;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A step through the enemy faced: the player is on its far side in an instant, a line of the element behind them
    /// and a cut across whatever stood on it. If it struck anything, the next heavy blow within a while is critical.
    /// Nothing can strike the player in the step. A wall, or anything too big to be shoved, stops it short.
    /// </summary>
    public class StepForm : RunningForm
    {
        public override string Id => SpellForms.Step;

        private const float StepSeconds = 0.11f;
        private const float AfterSeconds = 0.16f;

        /// <summary>
        /// Whether the step comes out beyond the enemy it went for and turns back to face it (the step), or stops in front of
        /// it still facing it (a rush, <see cref="RushForm"/>).
        /// </summary>
        protected virtual bool Through => true;

        /// <summary>How far the body goes toward an enemy that far off, within the spell's reach.</summary>
        protected virtual float RunTo(float gap, float reach) => FormRules.StepRun(gap, reach);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.A = cast.Origin;
            run.B = cast.Forward;
            float wanted = cast.Reach * 0.75f;
            if (cast.Target != null)
            {
                Vector3 gap = cast.Target.position - cast.Origin;
                gap.y = 0f;
                wanted = RunTo(gap.magnitude, cast.Reach);
            }
            run.T0 = cast.Player.ClearRun(cast.Forward, wanted);
            SpellWorks.Ghost(0.4f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            cast.Player.FormPose = FormKit.Sweep;
            cast.Player.FormSwordOut = true;
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            player.FormPose = FormKit.Sweep;
            player.FormSwordOut = true;
            if (run.Phase == 0)
            {
                float t = Mathf.Clamp01(run.Age / StepSeconds);
                player.FormPoseProgress = 0.15f + 0.4f * t;
                player.PassTo(run.A + run.B * (run.T0 * t));
                if (!run.Flag && t >= 0.5f)
                {
                    run.Flag = true;
                    SpellWorks.Ghost(0.32f, FormKit.Lit(cast.Colour, 2.6f, 0.7f));
                }
                if (t < 1f) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                // Through, and turned to face what was passed: the heavy blow that is owed goes back into it.
                if (Through && run.T0 > 0.5f) player.transform.rotation = Quaternion.LookRotation(-run.B, Vector3.up);
                // A rush that stopped short cuts on past where it stopped, into the enemy standing there.
                float lane = Through ? run.T0 : run.T0 + FormRules.RushShort + 0.4f;
                int hits = cast.Combat.StrikeLane(run.A, run.B, lane, 1.1f, cast.Blow(cast.Damage, cast.Stagger));
                Vector3 middle = run.A + run.B * (Through ? run.T0 * 0.5f : run.T0 + FormRules.RushShort * 0.6f) + Vector3.up * 1.1f;
                FormFx.Run(FormMark.Streak, run.A, run.B, lane, 1.5f, FormKit.Lit(cast.Colour, 3.2f), 0.32f, 0.1f);
                FormFx.Cut(middle, run.B, Through ? run.T0 * 1.3f : Mathf.Max(1.5f, run.T0 * 1.3f), 0.6f, FormKit.Lit(cast.Colour, 4f), 0.14f);
                ElementLook.Sparks(cast.Element, middle, run.B, 0.8f);
                if (hits > 0)
                {
                    // It landed: the next heavy blow is promised.
                    cast.Combat.PromiseHeavyCritical(Mathf.Max(0.5f, cast.Spec.Seconds));
                    FormFx.Cut(middle, Vector3.Cross(Vector3.up, run.B) + run.B * 0.5f, 4.5f, 0.55f, FormKit.Lit(cast.Colour, 4f), 0.16f);
                    Vfx.Play(VfxKind.Glint, player.transform.position + Vector3.up * 2f);
                    FormKit.Flash(cast, 0.16f, 0.05f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0.07f, 0.25f, 0.2f);
                }
                return true;
            }
            player.FormPoseProgress = Mathf.Lerp(0.55f, 0.95f, (run.Age - run.T1) / AfterSeconds);
            return run.Age < run.T1 + AfterSeconds;
        }
    }
}
