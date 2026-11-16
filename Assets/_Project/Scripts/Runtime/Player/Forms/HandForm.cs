using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Earth's gift, Hand of the Mountain. While the surge lasts a hand of stone hangs over the player's shoulder and
    /// follows them. Each heavy blow that lands, the hand goes over that enemy and, a moment after, comes down on it:
    /// a share of the blow again, in a circle, with a tremor.
    /// Reads: Damage (percent of the blow), Stagger, Radius, Delay, Stacks.
    /// </summary>
    public class HandForm : RunningForm
    {
        public override string Id => SpellForms.Hand;

        private const float Hover = 3.3f;
        private const float DropSeconds = 0.09f;
        private const float RestSeconds = 0.35f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            var solids = SpellWorks.Solids(run);
            // A palm, four fingers and a thumb.
            for (int i = 0; i < 6; i++) solids[i] = FormFx.Hold(FormMesh.Pillar, cast.Colour, 0.06f);
            run.A = cast.Origin + Vector3.up * Hover;
            run.B = cast.Forward;
            FormKit.Gather(cast, cast.Origin, 2.4f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase != 0 || target == null || step == null || !(step.Heavy || run.Cast.Combat.IsFinishing)) return;
            run.Phase = 1;
            run.T1 = run.Age;
            run.T2 = damage;
            run.C = SigKit.Foot(target, run.Cast.Player.transform.position.y);
            run.Flag = true;
        }

        // The hand, built flat: its palm at a point, its fingers toward a direction, as wide as its size.
        private static void Shape(int[] solids, Vector3 palm, Vector3 toward, float size, float spread, float hot)
        {
            Quaternion yaw = FormKit.Yaw(toward);
            Quaternion lying = yaw * Quaternion.Euler(90f, 0f, 0f);
            FormFx.Place(solids[0], palm - yaw * Vector3.forward * (size * 0.5f), lying, new Vector3(size * 1.15f, size * 1.1f, size * 0.5f));
            for (int i = 0; i < 4; i++)
            {
                float side = FormRules.InRow(i, 4, size * 0.3f);
                Quaternion finger = yaw * Quaternion.Euler(90f, 0f, 0f) * Quaternion.Euler(0f, 0f, -FormRules.InRow(i, 4, spread));
                float length = size * (i == 1 || i == 2 ? 1.05f : 0.85f);
                FormFx.Place(solids[1 + i], palm + yaw * new Vector3(side, 0f, size * 0.45f), finger, new Vector3(size * 0.27f, length, size * 0.3f));
                FormFx.Heat(solids[1 + i], hot);
            }
            FormFx.Place(solids[5], palm + yaw * new Vector3(-size * 0.6f, 0f, -size * 0.1f), yaw * Quaternion.Euler(90f, 0f, 0f) * Quaternion.Euler(0f, 0f, 55f), new Vector3(size * 0.3f, size * 0.75f, size * 0.32f));
            FormFx.Heat(solids[0], hot);
            FormFx.Heat(solids[5], hot);
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var player = cast.Player.transform;
            float radius = cast.Spec.Radius;
            float wait = Mathf.Max(0.3f, cast.Spec.Delay);
            if (run.Phase == 0)
            {
                if (!SigKit.SurgeOn(cast)) return false;
                // Over the sword arm, a little behind, drifting after the player.
                Vector3 want = player.position + player.right * 1.6f - player.forward * 1.2f + Vector3.up * (Hover + 0.25f * Mathf.Sin(run.Age * 2.2f));
                run.A = Vector3.Lerp(run.A, want, 1f - Mathf.Exp(-7f * deltaTime));
                run.B = Vector3.Slerp(run.B, player.forward, 1f - Mathf.Exp(-9f * deltaTime));
                Shape(solids, run.A, run.B, 1.5f, 9f, 0f);
                return true;
            }
            float since = run.Age - run.T1;
            Vector3 over = run.C + Vector3.up * (Hover + 0.5f);
            if (run.Phase == 1)
            {
                if (run.Flag)
                {
                    run.Flag = false;
                    FormKit.Warn(cast, run.C, radius, wait);
                }
                // It goes over them, opening as it rises.
                float t = Mathf.Clamp01(since / (wait - DropSeconds));
                run.A = Vector3.Lerp(run.A, over, 1f - Mathf.Exp(-14f * deltaTime));
                Shape(solids, run.A, run.B, Mathf.Lerp(1.5f, 2.3f, t), Mathf.Lerp(9f, 16f, t), since > wait - DropSeconds - 0.06f ? 1f : 0f);
                if (since < wait - DropSeconds) return true;
                run.Phase = 2;
                return true;
            }
            if (run.Phase == 2)
            {
                float t = Mathf.Clamp01((since - (wait - DropSeconds)) / DropSeconds);
                Shape(solids, Vector3.Lerp(over, run.C + Vector3.up * 0.5f, t * t), run.B, 2.3f, 16f, 0f);
                if (t < 1f) return true;
                run.Phase = 3;
                run.T3 = run.Age;
                var blow = cast.Blow(FormRules.Echo(run.T2, cast.Spec.Damage), cast.Stagger);
                blow.Heavy = true;
                cast.Combat.StrikeArea(run.C, radius, blow);
                FormKit.Impact(cast, run.C, radius, 2f);
                SigKit.Crown(run.C, radius * 0.9f, 10, 1.5f, cast.Colour, 0.4f);
                FormFx.Lay(FormMark.Crack, run.C, radius * 3.2f, FormKit.Lit(cast.Colour, 2.4f), 2.5f, -1f, 0f, Random.value * 360f);
                SigKit.Moment(cast, 0.16f, 0.8f, 0f, 1f, 0.3f);
                run.A = run.C + Vector3.up * 0.5f;
                return true;
            }
            // Flat on the ground for a breath, then back to the shoulder.
            Shape(solids, run.C + Vector3.up * 0.5f, run.B, 2.3f, 16f, run.Age < run.T3 + 0.06f ? 1f : 0f);
            if (run.Age < run.T3 + RestSeconds) return SigKit.SurgeOn(cast) || run.Age < run.T3 + 0.15f;
            run.Phase = 0;
            return true;
        }
    }
}
