using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Tempest's crown, Falling Thunder. The finisher throws the enemy skyward; the player is gone after it in a line
    /// of light; and both come down together as a bolt that shocks, unsteadies and throws back everything near.
    /// Nothing can strike the player from the throw to the landing.
    /// Reads: Damage, Stagger, Radius, Stacks.
    /// </summary>
    public class ThunderForm : RunningForm
    {
        public override string Id => SpellForms.Thunder;

        private const float UpSeconds = 0.34f;
        private const float AfterSeconds = 0.3f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Victim = FormKit.Victim(cast);
            run.A = FormKit.AimPoint(cast);
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            var set = SigKit.Set(run.Victim);
            if (set != null && FormKit.Alive(run.Victim)) set.Lift(UpSeconds + 0.1f);
            SpellWorks.Ghost(UpSeconds + 0.2f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            cast.Player.Hidden = true;
            Color light = FormKit.Lit(cast.Colour, 3.6f);
            FormFx.Cut(cast.Origin + Vector3.up * 4f, Vector3.up, 9f, 0.5f, light, UpSeconds, 0.1f);
            FormFx.Cut(run.A + Vector3.up * 4f, Vector3.up, 9f, 0.35f, light, UpSeconds, 0.1f);
            FormFx.Lay(FormMark.Ring, cast.Origin, 0.6f, light, 0.2f, 3f);
            FormKit.Warn(cast, run.A, cast.Spec.Radius, UpSeconds);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            if (run.Phase == 0)
            {
                if (run.Age < UpSeconds) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                float radius = cast.Spec.Radius;
                player.Hidden = false;
                player.StepTo(run.A - cast.Forward * 0.9f);
                var set = SigKit.Set(run.Victim);
                if (set != null) set.Drop();
                LightningBolts.Strike(run.A, 1f);
                cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Damage, cast.Stagger));
                cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Unsteady, cast.Spec.Stacks);
                cast.Combat.Displace(run.A, radius, 3f, false);
                FormKit.Impact(cast, run.A, radius, 2f);
                Color light = FormKit.Lit(cast.Colour, 4f);
                SigKit.Column(run.A, light, 2.6f, 0.2f);
                FormFx.Lay(FormMark.Ring, run.A, 1f, light, 0.4f, radius * 3f);
                FormFx.Lay(FormMark.Star, run.A, 2f, light, 0.22f, radius * 2.6f);
                for (int i = 0; i < 7; i++)
                    FormFx.Run(FormMark.Streak, run.A, FormRules.OnRing(i, 7, 1f, 13f), radius * 1.3f, 0.7f, light, 0.3f, 0.1f);
                SigKit.Crown(run.A, radius * 0.5f, 8, 1.6f, cast.Colour, 0.3f);
                SigKit.Moment(cast, 0.2f, 0.9f, 0.4f, 0.4f, 0.5f);
                return true;
            }
            player.FormPose = FormKit.Chop;
            player.FormSwordOut = true;
            player.FormPoseProgress = Mathf.Lerp(0.6f, 0.95f, (run.Age - run.T1) / AfterSeconds);
            return run.Age < run.T1 + AfterSeconds;
        }
    }
}
