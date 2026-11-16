using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Bloom's crown, Great Lotus. A lotus of five petals opens on the ground under the player. Inside it the player
    /// mends and enemies are Soaked and Seeded. Each perfect guard made inside closes a petal on the attacker and
    /// holds it. When the last petal closes, or the lotus's time runs out, it bursts, harder for every petal closed.
    /// Reads: Radius, Seconds, Damage, Stagger, Count (petals), LingerDamage (health mended each second).
    /// </summary>
    public class LotusForm : RunningForm
    {
        public override string Id => SpellForms.Lotus;

        private const float HoldSeconds = 3f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 8);
            run.T2 = cast.Defence != null ? cast.Defence.PerfectGuards : 0;
            // The first wetting and seeding come as it opens.
            run.T0 = 0.7f;
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.06f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.6f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2f));
            FormFx.Lay(FormMark.Ring, run.A, 0.6f, FormKit.Lit(cast.Colour, 3.4f), 0.4f, cast.Spec.Radius * 2f);
            if (cast.Defence != null) cast.Defence.Callout(cast.Name);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            var closed = SpellWorks.Things(run);
            float radius = cast.Spec.Radius;
            Vector3 here = cast.Player.transform.position;
            bool inside = (here - run.A).sqrMagnitude <= radius * radius;
            float open = Mathf.Clamp01(run.Age / 0.35f);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 outward = FormRules.OnRing(i, run.Count, 1f, 36f);
                if (i < run.Done)
                {
                    // Closed: it stands over the one it holds, point down.
                    if (FormKit.Alive(closed[i])) SpellWorks.Points(run)[i] = closed[i].transform.position;
                    FormFx.Place(solids[i], SpellWorks.Points(run)[i] + Vector3.up * 3.2f, SigKit.Down, new Vector3(2.6f, 2.6f, 3f));
                }
                else
                {
                    // Open: lying out from the middle, its tip lifted a little, breathing.
                    float lift = -12f - 6f * Mathf.Sin(run.Age * 2f + i);
                    FormFx.Place(solids[i], run.A + outward * 0.3f + Vector3.up * 0.15f, FormKit.Yaw(outward) * Quaternion.Euler(lift, 0f, 0f), new Vector3(3.4f, 1.6f, radius * 0.95f * open));
                }
            }
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 20f, 0f), new Vector3(radius * 1.3f, 1f, radius * 1.3f));
            run.T0 += deltaTime;
            if (run.T0 >= 1f)
            {
                run.T0 -= 1f;
                cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Soaked, 1);
                cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Seeded, 1);
                if (inside && cast.Defence != null) cast.Defence.Heal(cast.Spec.LingerDamage, false);
                FormFx.Lay(FormMark.Ring, run.A, 0.8f, FormKit.Lit(cast.Colour, 2f), 0.7f, radius * 2f);
            }
            int guards = cast.Defence != null ? cast.Defence.PerfectGuards : 0;
            if (guards != (int)run.T2)
            {
                run.T2 = guards;
                if (inside && run.Done < run.Count && cast.Upgrades != null)
                {
                    int i = run.Done++;
                    closed[i] = cast.Upgrades.LastGuardEnemy;
                    Vector3 at = cast.Upgrades.LastGuardFrom;
                    at.y = run.A.y;
                    SpellWorks.Points(run)[i] = at;
                    var set = SigKit.Set(closed[i]);
                    if (set != null) set.Hold(HoldSeconds);
                    FormFx.Run(FormMark.Streak, run.A, at - run.A, (at - run.A).magnitude, 0.8f, FormKit.Lit(cast.Colour, 3.2f), 0.25f, 0.2f);
                    FormFx.Lay(FormMark.Ring, at, 3.4f, FormKit.Lit(cast.Colour, 3.4f), 0.3f, 0.8f);
                    FormFx.Face(FormMark.Brackets, at + Vector3.up * 1.2f, 2.6f, FormKit.Lit(cast.Colour, 3f), HoldSeconds * 0.5f, 1.8f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.25f, 0.2f);
                }
            }
            if (run.Done < run.Count && run.Age < cast.Spec.Seconds) return true;
            // It bursts.
            float burst = radius * 1.7f;
            cast.Combat.StrikeArea(run.A, burst, cast.Blow(cast.Damage * (1f + 0.3f * run.Done), cast.Stagger));
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 outward = FormRules.OnRing(i, run.Count, 1f, 36f);
                FormFx.Release(solids[i]);
                solids[i] = -1;
                FormFx.Fly(FormMesh.Blade, run.A + Vector3.up * 0.6f, run.A + outward * burst + Vector3.up * 2.4f, FormKit.Yaw(outward) * Quaternion.Euler(-30f, 0f, 0f), new Vector3(3.4f, 1.6f, 3.4f), cast.Colour, 0.4f);
            }
            FormKit.Impact(cast, run.A, burst * 0.7f, 2f);
            FormFx.Lay(FormMark.Star, run.A, 2f, FormKit.Lit(cast.Colour, 3.6f), 0.3f, burst * 2.2f);
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 4f), 0.45f, burst * 2.2f);
            SigKit.Crown(run.A, burst * 0.6f, 10, 1.4f, cast.Colour);
            SigKit.Moment(cast, 0.16f, 0.7f, 0.3f, 0.4f, 0.4f);
            return false;
        }
    }
}
