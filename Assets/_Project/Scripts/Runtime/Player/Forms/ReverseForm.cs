using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A burst that runs backwards: rings and streaks of the element fall inward to a point ahead, dragging every
    /// enemy near toward it while the delay runs, and then the point lets go of all it took in.
    /// </summary>
    public class ReverseForm : RunningForm
    {
        public override string Id => SpellForms.Reverse;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = FormKit.AimPoint(cast);
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, cast.Colour * 0.55f, 0.05f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.6f));
            SpellWorks.Gesture(cast, FormKit.Cast, 0.4f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            Vector3 heart = run.A + Vector3.up * 1.2f;
            if (run.Age < run.T0)
            {
                float t = run.Age / run.T0;
                // Everything that can be moved is drawn most of the way to the middle by the time it goes.
                cast.Combat.Displace(run.A, radius, radius * 0.85f / run.T0 * deltaTime, true);
                float across = radius * 2f;
                FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(across, 1f, across));
                bool beat = Mathf.Repeat(run.Age * Mathf.Lerp(4f, 12f, t), 1f) < 0.25f;
                FormFx.Place(solids[0], heart, Quaternion.Euler(run.Age * -220f, run.Age * -340f, 0f), Vector3.one * (0.3f + 0.75f * t) * (beat ? 1.15f : 1f));
                FormFx.Heat(solids[0], beat && t > 0.5f ? 1f : 0f);
                run.T1 -= deltaTime;
                if (run.T1 <= 0f)
                {
                    run.T1 = Mathf.Lerp(0.16f, 0.07f, t);
                    FormFx.Lay(FormMark.Ring, run.A, across, FormKit.Lit(cast.Colour, 2.6f), 0.3f, 0.5f);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 outward = FormRules.OnRing(0, 1, 1f, Random.value * 360f);
                        FormFx.Run(FormMark.Streak, run.A + outward * radius + Vector3.up * Random.Range(0.2f, 1.6f), -outward, radius * 0.75f, 0.3f, FormKit.Lit(cast.Colour, 3f), 0.16f, 0.05f);
                    }
                    ElementLook.Puff(cast.Element, heart + FormRules.OnRing(0, 1, radius * 0.7f, Random.value * 360f), Vector3.up, 0.4f);
                }
                return true;
            }
            FormFx.Release(solids[0]);
            solids[0] = -1;
            var blow = cast.Blow(cast.Damage, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeArea(run.A, radius, blow);
            FormKit.Impact(cast, run.A, radius * 0.7f, 1.5f);
            for (int i = 0; i < 12; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 12, 1f, 15f);
                FormFx.Fly(FormMesh.Shard, heart, heart + outward * radius * 1.1f, Quaternion.LookRotation(outward), Vector3.one * 1.5f, cast.Colour, 0.24f);
            }
            for (int i = 0; i < 7; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 7, radius * 0.3f, 40f);
                FormFx.Erupt(FormMesh.Spike, run.A + outward, FormKit.Yaw(outward) * Quaternion.Euler(28f, 0f, 0f), new Vector3(0.8f, 2f + 0.5f * (i % 2), 0.8f), cast.Colour, 0.06f, 0.35f, 0.28f);
            }
            return false;
        }
    }
}
