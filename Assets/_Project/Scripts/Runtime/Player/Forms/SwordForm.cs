using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// One sword as tall as the screen: it hangs over the point aimed at while the delay runs, comes down in a
    /// single frame's worth of fall, and stands buried in the ground it has cracked. The crack bites for a while.
    /// </summary>
    public class SwordForm : RunningForm
    {
        public override string Id => SpellForms.Sword;

        private const float Length = 13f;
        private const float SlamSeconds = 0.1f;
        private const float StandSeconds = 1.1f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            run.A = FormKit.AimPoint(cast);
            run.Victim = FormKit.Victim(cast);
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Sword, cast.Colour, 0.11f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.8f));
            FormFx.Lay(FormMark.Ring, run.A, cast.Spec.Radius * 5f, FormKit.Lit(cast.Colour, 3f), run.T0, cast.Spec.Radius * 2f);
            // The arm goes up with the sword and is held there until it falls.
            SpellWorks.Gesture(cast, FormKit.Chop, run.T0 + SlamSeconds + 0.2f, 0f, 0.7f, true);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            // It follows whoever it hangs over until just before it falls: then the spot is fixed.
            if (run.Phase == 0 && run.Age < run.T0 - 0.2f && FormKit.Alive(run.Victim))
            {
                Vector3 under = run.Victim.transform.position;
                run.A = new Vector3(under.x, cast.Origin.y, under.z);
            }
            Quaternion down = Quaternion.LookRotation(Vector3.down + new Vector3(0.08f, 0f, 0.05f), Vector3.forward) * Quaternion.Euler(0f, 0f, 20f);
            Vector3 along = down * Vector3.forward;
            if (run.Phase == 0)
            {
                float t = Mathf.Clamp01(run.Age / run.T0);
                // Its point sinks slowly toward the mark as the time runs out.
                float pointHeight = Mathf.Lerp(9f, 5.5f, t) + 0.15f * Mathf.Sin(run.Age * 6f);
                float formed = Mathf.Clamp01(run.Age / 0.16f);
                FormFx.Place(solids[0], run.A + Vector3.up * pointHeight - along * Length, down, Vector3.one * (Length * Mathf.Lerp(0.3f, 1f, formed)));
                FormFx.Heat(solids[0], formed < 1f || run.Age > run.T0 - 0.07f ? 1f : 0f);
                FormFx.PlaceMark(marks[0], run.A, Quaternion.Euler(0f, run.Age * 120f, 0f), new Vector3(radius * 1.5f, 1f, radius * 1.5f));
                FormFx.PlaceMark(marks[1], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
                if (run.Age >= run.T0) run.Phase = 1;
                return true;
            }
            if (run.Phase == 1)
            {
                float t = Mathf.Clamp01((run.Age - run.T0) / SlamSeconds);
                float pointHeight = Mathf.Lerp(5.5f, -Length * 0.2f, t * t);
                FormFx.Place(solids[0], run.A + Vector3.up * pointHeight - along * Length, down, new Vector3(Length * 0.9f, Length * 0.9f, Length * 1.1f));
                FormFx.Heat(solids[0], 0f);
                if (t < 1f) return true;
                run.Phase = 2;
                FormFx.ReleaseMark(marks[0]);
                FormFx.ReleaseMark(marks[1]);
                marks[0] = marks[1] = -1;
                FormFx.Place(solids[0], run.A - Vector3.up * (Length * 0.2f) - along * Length, down, Vector3.one * Length);
                FormFx.Heat(solids[0], 1f);
                var blow = cast.Blow(cast.Damage, cast.Stagger);
                blow.Heavy = true;
                cast.Combat.StrikeArea(run.A, radius, blow);
                FormKit.Impact(cast, run.A, radius, 2f);
                // The ground breaks under it: a crown of shards thrown up round the blade, and a crack that stays.
                for (int i = 0; i < 9; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, 9, radius * 0.62f, 20f);
                    FormFx.Erupt(FormMesh.Spike, run.A + outward, FormKit.Yaw(outward) * Quaternion.Euler(38f, 0f, 0f),
                        new Vector3(0.8f, 1.5f + 0.5f * (i % 3), 0.8f), cast.Colour, 0.07f, 0.45f + 0.05f * (i % 2), 0.3f);
                }
                FormFx.Lay(FormMark.Crack, run.A, radius * 3.4f, FormKit.Lit(cast.Colour, 2.8f), Mathf.Max(1f, cast.Spec.Seconds), -1f, 0f, 33f);
                var crack = cast.Ground(run.A, radius * 0.75f, cast.Spec.Seconds, 0.5f, cast.Linger(cast.Spec.LingerDamage));
                crack.Delay = 0.25f;
                Hazards.Lay(crack);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.17f, 0.65f, 0.45f);
                return true;
            }
            FormFx.Heat(solids[0], run.Age < run.T0 + SlamSeconds + 0.07f ? 1f : 0f);
            if (run.Age < run.T0 + SlamSeconds + StandSeconds) return true;
            FormFx.Release(solids[0], 0.3f);
            solids[0] = -1;
            return false;
        }
    }
}
