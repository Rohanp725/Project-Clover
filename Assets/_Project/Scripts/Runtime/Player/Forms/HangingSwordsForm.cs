using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Metal's gift, Hanging Swords. While the surge lasts every blow caught on the guard forges a sword that hangs,
    /// point down, over the one who struck, eight at most. When the surge ends they fall one after another.
    /// (That a held guard is a perfect guard while the surge lasts is a number on the mark.)
    /// Reads: Count (most swords), Damage, Stagger, Radius, Stacks.
    /// </summary>
    public class HangingSwordsForm : RunningForm
    {
        public override string Id => SpellForms.HangingSwords;

        private const float Hang = 2.7f;
        private const float Length = 2.4f;
        private const float Between = 0.11f;

        private static int Guards(in SpellCast cast) => cast.Defence != null ? cast.Defence.Blocks + cast.Defence.PerfectGuards : 0;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Done = Guards(cast);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var points = SpellWorks.Points(run);
            var over = SpellWorks.Things(run);
            int most = Mathf.Clamp(cast.Spec.Count, 1, 12);
            Quaternion down = SigKit.Down;
            Vector3 along = down * Vector3.forward;
            if (run.Phase == 0)
            {
                int guards = Guards(cast);
                if (guards != run.Done && cast.Upgrades != null)
                {
                    run.Done = guards;
                    if (run.Count < most)
                    {
                        int i = run.Count++;
                        over[i] = cast.Upgrades.LastGuardEnemy;
                        points[i] = cast.Upgrades.LastGuardFrom;
                        points[i].y = cast.Player.transform.position.y;
                        solids[i] = FormFx.Hold(FormMesh.Sword, cast.Colour, 0.06f);
                        FormFx.Face(FormMark.Star, points[i] + Vector3.up * Hang, 1.2f, FormKit.Lit(cast.Colour, 4f), 0.12f, 2.6f);
                        FormFx.Lay(FormMark.Ring, points[i], 2.6f, FormKit.Lit(cast.Colour, 3f), 0.3f, 0.6f);
                        SpellWorks.Times(run)[i] = run.Age;
                    }
                }
                for (int i = 0; i < run.Count; i++)
                {
                    // Each hangs over its own enemy, a little apart from the others over the same head.
                    if (FormKit.Alive(over[i])) points[i] = SigKit.Foot(over[i], points[i].y);
                    Vector3 aside = FormRules.OnRing(i, 5, 0.55f, 40f);
                    float formed = Mathf.Clamp01((run.Age - SpellWorks.Times(run)[i]) / 0.15f);
                    Vector3 point = points[i] + aside + Vector3.up * (Hang + 0.12f * Mathf.Sin(run.Age * 3f + i));
                    FormFx.Place(solids[i], point - along * Length, down, Vector3.one * (Length * Mathf.Lerp(0.3f, 1f, formed)));
                    FormFx.Heat(solids[i], formed < 1f ? 1f : 0f);
                }
                if (SigKit.SurgeOn(cast)) return true;
                if (run.Count == 0) return false;
                run.Phase = 1;
                run.T1 = run.Age;
                run.Done = 0;
                for (int i = 0; i < run.Count; i++) FormKit.Warn(cast, points[i], cast.Spec.Radius, 0.3f + i * Between);
                SigKit.Moment(cast, 0f, 0f, 0.5f, 0.4f, 0.15f);
                return true;
            }
            while (run.Done < run.Count && run.Age >= run.T1 + 0.3f + run.Done * Between)
            {
                int i = run.Done++;
                Vector3 aside = FormRules.OnRing(i, 5, 0.55f, 40f);
                FormFx.Release(solids[i]);
                solids[i] = -1;
                FormFx.Drop(FormMesh.Sword, points[i] + aside + Vector3.up * Hang - along * Length, points[i] + aside * 0.5f - Vector3.up * 0.5f - along * Length, down, Vector3.one * Length, cast.Colour, 0.07f, 0.8f, 0.3f, 0.06f);
                cast.Combat.StrikeArea(points[i], cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                FormKit.Impact(cast, points[i], cast.Spec.Radius, 0.9f);
                SigKit.Column(points[i], FormKit.Lit(cast.Colour, 3.2f), 0.7f, 0.12f);
            }
            if (run.Done < run.Count) return true;
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.12f, 0.5f, 0.3f);
            return false;
        }
    }
}
