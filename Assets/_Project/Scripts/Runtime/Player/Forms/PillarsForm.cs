using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Pillars from below: a mark closes under each of the nearest enemies, following it while the delay runs, and
    /// then a column bursts up under each in turn. With fewer enemies than pillars, the rest stand round the first.
    /// </summary>
    public class PillarsForm : RunningForm
    {
        public override string Id => SpellForms.Pillars;

        private const int Most = 8;
        private const float Between = 0.07f;
        /// <summary>Seconds what a column strikes hangs in the air.</summary>
        private const float LiftSeconds = 0.8f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, Most);
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            var points = SpellWorks.Points(run);
            var things = SpellWorks.Things(run);
            var marks = SpellWorks.Marks(run);
            var taken = SpellWorks.Once(run);
            float radius = cast.Spec.Radius;
            for (int i = 0; i < run.Count; i++)
            {
                // The first rises under what the spell seeks (SpellSpec.AimAt), when it found one; the rest under the nearest.
                var enemy = i == 0 && cast.Spec.AimAt != Clover.Gameplay.AfflictionKind.None && cast.Target != null && cast.Target.GetComponentInParent<Clover.Gameplay.IDamageable>() is Component sought
                    ? sought : cast.Combat.EnemyNear(cast.Origin, cast.Reach, taken);
                if (enemy != null)
                {
                    taken.Add((Clover.Gameplay.IDamageable)enemy);
                    things[i] = enemy;
                    points[i] = new Vector3(enemy.transform.position.x, cast.Origin.y, enemy.transform.position.z);
                }
                else if (i == 0) points[i] = FormKit.AimPoint(cast);
                else points[i] = points[0] + FormRules.OnRing(i, run.Count, radius * 2.1f, 35f);
                FormFx.Lay(FormMark.Ring, points[i], radius * 4.4f, FormKit.Lit(cast.Colour, 3f), run.T0 + i * Between, radius * 2.1f);
                if (i < marks.Length) marks[i] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 2f));
            }
            taken.Clear();
            SpellWorks.Gesture(cast, FormKit.Chop, run.T0 + 0.15f, 0.2f, 0.95f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var things = SpellWorks.Things(run);
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            for (int i = run.Done; i < run.Count; i++)
            {
                // Each mark keeps under its enemy until just before its pillar comes.
                float due = run.T0 + i * Between;
                if (run.Age < due - 0.12f && FormKit.Alive(things[i]))
                    points[i] = new Vector3(things[i].transform.position.x, cast.Origin.y, things[i].transform.position.z);
                if (i < marks.Length) FormFx.PlaceMark(marks[i], points[i], Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
            }
            while (run.Done < run.Count && run.Age >= run.T0 + run.Done * Between)
            {
                int i = run.Done++;
                if (i < marks.Length)
                {
                    FormFx.ReleaseMark(marks[i]);
                    marks[i] = -1;
                }
                float wide = Mathf.Clamp(radius * 0.95f, 0.8f, 2.2f);
                FormFx.Erupt(FormMesh.Pillar, points[i], Quaternion.Euler(i % 2 == 0 ? 3f : -4f, i * 50f, 0f), new Vector3(wide, 2.7f + 0.4f * (i % 2), wide), cast.Colour, 0.07f, 0.5f, 0.3f, 0.06f);
                for (int k = 0; k < 4; k++)
                {
                    Vector3 outward = FormRules.OnRing(k, 4, wide * 0.62f, i * 30f + 20f);
                    FormFx.Erupt(FormMesh.Spike, points[i] + outward, FormKit.Yaw(outward) * Quaternion.Euler(34f, 0f, 0f), new Vector3(0.6f, 1.2f, 0.6f), cast.Colour, 0.06f, 0.5f, 0.3f);
                }
                var blow = cast.Blow(cast.Damage, cast.Stagger);
                blow.Heavy = true;
                cast.Combat.StrikeArea(points[i], radius, blow);
                // What the column comes up under leaves the ground with it for a moment.
                cast.Combat.LiftArea(points[i], radius, LiftSeconds);
                FormKit.Impact(cast, points[i], radius, i == run.Count - 1 ? 1.2f : 0.8f);
            }
            return run.Done < run.Count;
        }
    }
}
