using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Blades from above: they form high over the enemy faced, hang there over a turning sign while the delay runs,
    /// and then fall one after another, each sticking in the ground where it strikes.
    /// </summary>
    public class BladesForm : RunningForm
    {
        public override string Id => SpellForms.Blades;

        private const float Height = 3.9f;
        private const float Length = 2.3f;
        private const float FallSeconds = 0.09f;
        private const float Between = 0.085f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, SpellWorks.SolidsEach);
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            run.A = FormKit.AimPoint(cast);
            run.Victim = FormKit.Victim(cast);
            run.T2 = run.Count > 1 ? Mathf.Max(0.9f, cast.Spec.Radius * 0.9f) : 0f;
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.045f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.8f));
            FormFx.Lay(FormMark.Ring, run.A, (run.T2 + cast.Spec.Radius) * 4f, FormKit.Lit(cast.Colour, 3f), run.T0, (run.T2 + cast.Spec.Radius) * 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.35f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            float radius = cast.Spec.Radius;
            // They hang over whoever they were called down on, wherever that one goes, until the first lets go.
            if (run.Phase == 0 && FormKit.Alive(run.Victim))
            {
                Vector3 under = run.Victim.transform.position;
                run.A = new Vector3(under.x, cast.Origin.y, under.z);
            }
            float across = (run.T2 + radius) * 2f;
            FormFx.PlaceMark(marks[0], run.A, Quaternion.Euler(0f, run.Age * 150f, 0f), new Vector3(across * 0.8f, 1f, across * 0.8f));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.identity, new Vector3(across, 1f, across));

            for (int i = run.Done; i < run.Count; i++)
            {
                // Each snaps into being a moment after the one before, point down, turning slowly on its own line.
                float formed = Mathf.Clamp01((run.Age - i * 0.04f) / 0.12f);
                Vector3 over = Over(run, i) + Vector3.up * (Height + 0.18f * Mathf.Sin(run.Age * 5f + i));
                FormFx.Place(solids[i], over, Hanging(run, i), Vector3.one * (Length * (formed < 1f ? formed * 1.25f : 1f)));
                FormFx.Heat(solids[i], formed < 1f ? 1f : run.Age > run.T0 - 0.08f ? 1f : 0f);
            }

            if (run.Phase == 0 && run.Age >= run.T0)
            {
                run.Phase = 1;
                run.T1 = run.Age;
            }
            while (run.Phase == 1 && run.Done < run.Count && run.Age >= run.T1)
            {
                int i = run.Done;
                Vector3 ground = Over(run, i);
                Quaternion turn = Hanging(run, i);
                FormFx.Release(solids[i]);
                solids[i] = -1;
                // Its point ends a quarter of its length in the ground.
                Vector3 stuck = ground - (turn * Vector3.forward) * (Length * 0.75f);
                FormFx.Drop(FormMesh.Blade, stuck + Vector3.up * Height, stuck, turn, Vector3.one * Length, cast.Colour, FallSeconds, 0.8f, 0.25f, 0.045f);
                points[i] = ground;
                times[i] = run.Age + FallSeconds;
                run.Done++;
                run.T1 += FormRules.Own(cast.Spec.Between, Between);
            }

            bool owed = run.Done < run.Count;
            for (int i = 0; i < run.Done; i++)
            {
                if (times[i] <= 0f) continue;
                if (run.Age < times[i])
                {
                    owed = true;
                    continue;
                }
                times[i] = -1f;
                cast.Combat.StrikeArea(points[i], radius, cast.Blow(cast.Damage, cast.Stagger));
                FormKit.Impact(cast, points[i], Mathf.Max(0.8f, radius * 0.7f), i == run.Count - 1 ? 0.95f : 0.4f);
            }
            return owed;
        }

        private static Vector3 Over(in SpellRun run, int index) => run.A + FormRules.Scattered(index, run.Count, run.T2);

        // Point down, leaning a little out from the middle, each turned its own way about its length.
        private static Quaternion Hanging(in SpellRun run, int index)
        {
            Vector3 lean = FormRules.Scattered(index, run.Count, 0.22f);
            return Quaternion.LookRotation(Vector3.down + lean, Vector3.forward) * Quaternion.Euler(0f, 0f, index * 47f + run.Age * 90f * (run.Phase == 0 ? 1f : 0f));
        }
    }
}
