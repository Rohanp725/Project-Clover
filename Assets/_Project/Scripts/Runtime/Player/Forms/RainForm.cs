using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A rain of the element on a circle of ground: for a while, at every interval, a ring closes on some spot inside
    /// it and a shard comes down there. Half of them fall where an enemy stands; the rest fall where they will.
    /// </summary>
    public class RainForm : RunningForm
    {
        public override string Id => SpellForms.Rain;

        private const float WarnSeconds = 0.34f;
        private const float StrikeRadius = 1.2f;
        private const float Long = 2.6f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = FormKit.AimPoint(cast);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.2f));
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 3f), 0.3f, cast.Spec.Radius * 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.35f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            float radius = cast.Spec.Radius;
            float seconds = Mathf.Max(0.3f, cast.Spec.Seconds);
            bool falling = run.Age < seconds;
            float across = radius * 2f * Mathf.Clamp01(run.Age / 0.2f) * Mathf.Clamp01((seconds + WarnSeconds - run.Age) / 0.25f);
            FormFx.PlaceMark(SpellWorks.Marks(run)[0], run.A, Quaternion.identity, new Vector3(across, 1f, across));

            if (falling && run.Age >= run.T1)
            {
                run.T1 += Mathf.Max(0.06f, cast.Spec.Interval);
                int free = -1;
                for (int k = 0; k < times.Length && free < 0; k++)
                    if (times[k] <= 0f) free = k;
                if (free >= 0)
                {
                    Vector2 scatter = Random.insideUnitCircle * radius;
                    Vector3 spot = run.A + new Vector3(scatter.x, 0f, scatter.y);
                    if (run.Count++ % 2 == 0)
                    {
                        var near = cast.Combat.EnemyNear(spot, radius, null);
                        if (near != null && (near.transform.position - run.A).sqrMagnitude <= radius * radius)
                            spot = new Vector3(near.transform.position.x, run.A.y, near.transform.position.z);
                    }
                    points[free] = spot;
                    times[free] = run.Age + WarnSeconds;
                    FormFx.Lay(FormMark.Ring, spot, StrikeRadius * 4f, FormKit.Lit(cast.Colour, 3f), WarnSeconds, StrikeRadius * 1.6f);
                    FormFx.Lay(FormMark.Disc, spot, StrikeRadius * 2f, FormKit.Lit(cast.Colour, 1.8f), WarnSeconds);
                    // It is already on its way, from far above the screen, and arrives as the ring closes.
                    Quaternion slant = Quaternion.LookRotation(new Vector3(0.22f, -1f, 0.12f), Vector3.forward);
                    Vector3 stuck = spot - (slant * Vector3.forward) * (Long * 0.7f);
                    FormFx.Drop(FormMesh.Shard, stuck - (slant * Vector3.forward) * 18f, stuck, slant, Vector3.one * Long, cast.Colour, WarnSeconds, 0.4f, 0.2f, 0.045f);
                }
            }

            bool owed = false;
            for (int k = 0; k < times.Length; k++)
            {
                if (times[k] <= 0f) continue;
                if (run.Age < times[k])
                {
                    owed = true;
                    continue;
                }
                times[k] = 0f;
                cast.Combat.StrikeArea(points[k], StrikeRadius, cast.Blow(cast.Damage, cast.Stagger));
                FormKit.Impact(cast, points[k], StrikeRadius * 0.8f, 0.4f);
            }
            return falling || owed;
        }
    }
}
