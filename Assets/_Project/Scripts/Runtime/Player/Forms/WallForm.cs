using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A wall of the element: a line shows where it will stand, and slabs of it break out of the ground from the
    /// middle outward. Nothing walks through it and no blow from its far side reaches the player. When its time is
    /// up, or when the player strikes it with a heavy blow, it bursts.
    /// </summary>
    public class WallForm : RunningForm
    {
        public override string Id => SpellForms.Wall;

        private const float Tall = 2.7f;

        public override int Cast(in SpellCast cast)
        {
            // One wall at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin + cast.Forward * cast.Reach;
            run.B = cast.Forward;
            run.T0 = Mathf.Max(0.15f, cast.Spec.Delay);
            // Its width is written where a cone's angle would be.
            run.T2 = Mathf.Clamp(cast.Spec.Arc, 2f, 14f);
            run.Count = Mathf.Min(SpellWorks.SolidsEach, FormRules.Pieces(run.T2, 1f));
            Vector3 right = Vector3.Cross(Vector3.up, run.B);
            FormFx.Run(FormMark.Streak, run.A - right * (run.T2 * 0.5f), right, run.T2, 1.1f, FormKit.Lit(cast.Colour, 2.6f), run.T0 + 0.1f, 0.5f);
            FormFx.Lay(FormMark.Sigil, run.A, 2.4f, FormKit.Lit(cast.Colour), run.T0 + 0.1f, 1.6f, 200f);
            SpellWorks.Gesture(cast, FormKit.Chop, run.T0 + 0.2f, 0.2f, 0.9f);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            if (!step.Heavy || run.Phase != 1) return;
            // How far the swing's origin is from the wall's line.
            Vector3 right = Vector3.Cross(Vector3.up, run.B);
            Vector3 offset = origin - run.A;
            offset.y = 0f;
            float along = Mathf.Clamp(Vector3.Dot(offset, right), -run.T2 * 0.5f, run.T2 * 0.5f);
            if ((offset - right * along).magnitude <= step.Reach + 1f) run.Flag = true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 right = Vector3.Cross(Vector3.up, run.B);
            Quaternion facing = FormKit.Yaw(run.B);
            float each = run.T2 / run.Count;
            if (run.Phase == 0)
            {
                if (run.Age < run.T0) return true;
                run.Phase = 1;
                for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Slab, cast.Colour, 0.05f);
                SpellWorks.Barrier(run, run.A + Vector3.up * 1.5f, facing, new Vector3(run.T2, 3f, 0.7f));
                SpellWorks.Screen(run, run.A - right * (run.T2 * 0.5f), run.A + right * (run.T2 * 0.5f));
                FormFx.Lay(FormMark.Scorch, run.A, run.T2 * 0.9f, FormKit.Stain(cast.Colour, 0.5f), cast.Spec.Seconds + 1f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.04f, 0.3f, 0.25f);
            }
            float stood = run.Age - run.T0;
            float seconds = Mathf.Max(0.5f, cast.Spec.Seconds);
            bool ending = stood > seconds - 0.7f;
            for (int i = 0; i < run.Count; i++)
            {
                // From the middle outward, each a moment after its neighbour: up too far, then settling.
                float away = Mathf.Abs(FormRules.InRow(i, run.Count, 1f));
                float t = Mathf.Clamp01((stood - away * 0.035f) / 0.1f);
                float up = t <= 0f ? 0.02f : 1f - (1f - t) * (1f - t) * (1f - t) + 0.25f * Mathf.Sin(t * Mathf.PI);
                Vector3 foot = run.A + right * FormRules.InRow(i, run.Count, each);
                float tall = Tall * (1f - 0.1f * (i % 3)) * (i % 2 == 0 ? 1f : 0.9f);
                FormFx.Place(solids[i], foot, facing * Quaternion.Euler(i % 2 == 0 ? 4f : -5f, i % 2 == 0 ? 0f : 180f, (i % 3 - 1) * 5f), new Vector3(each * 1.25f, tall * up, 1.5f));
                // In its last moments it blinks: it is about to go.
                FormFx.Heat(solids[i], t < 1f || (ending && Mathf.Repeat(stood * 7f, 1f) < 0.4f) ? 1f : 0f);
                if (t > 0f && t < 1f && Mathf.Repeat(stood, 0.04f) < deltaTime) ElementLook.Puff(cast.Element, foot + Vector3.up * 0.3f, Vector3.up, 0.5f);
            }
            if (!run.Flag && stood < seconds) return true;

            // The burst.
            var blow = cast.Blow(cast.Damage, cast.Stagger);
            blow.Heavy = run.Flag;
            var once = SpellWorks.Once(run);
            once.Clear();
            cast.Combat.StrikeArea(run.A, cast.Spec.Radius, blow, once);
            cast.Combat.StrikeLane(run.A - right * (run.T2 * 0.5f), right, run.T2, cast.Spec.Radius * 0.6f, blow, once);
            FormKit.Impact(cast, run.A, cast.Spec.Radius * 0.8f, run.Flag ? 1.6f : 1.2f);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 foot = run.A + right * FormRules.InRow(i, run.Count, each) + Vector3.up * 1.2f;
                FormFx.Release(solids[i]);
                solids[i] = -1;
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 thrown = (run.B * side + right * FormRules.InRow(i, run.Count, 0.3f) + Vector3.up * 0.15f).normalized;
                    FormFx.Fly(FormMesh.Shard, foot, foot + thrown * cast.Spec.Radius, Quaternion.LookRotation(thrown), Vector3.one * 1.8f, cast.Colour, 0.26f);
                }
            }
            return false;
        }
    }
}
