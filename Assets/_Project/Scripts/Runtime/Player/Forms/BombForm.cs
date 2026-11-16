using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A great sphere: the player stands rooted and raises it overhead, growing, while the delay runs, then throws it
    /// at the point aimed at. A great burst, and the ground left burning (or soaking, or whatever the element leaves).
    /// Rolling away before the throw loses it.
    /// </summary>
    public class BombForm : RunningForm
    {
        public override string Id => SpellForms.Bomb;

        private const float FlightSeconds = 0.42f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, false, true))
            {
                run.Phase = 9;
                return 0;
            }
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            run.B = FormKit.AimPoint(cast);
            run.T2 = Mathf.Clamp(cast.Spec.Radius * 0.36f, 0.7f, 1.6f);
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, cast.Colour, 0.07f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour));
            FormKit.Warn(cast, run.B, cast.Spec.Radius, run.T0 + FlightSeconds);
            return 0;
        }

        // Rolled away, or struck, before the throw: it goes out over the player's head.
        public override bool BodyLost(ref SpellRun run)
        {
            if (run.Phase == 0) ElementLook.Puff(run.Cast.Element, run.A, Vector3.up, 1.2f);
            return run.Phase != 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float size = run.T2;
            if (run.Phase == 0)
            {
                float t = Mathf.Clamp01(run.Age / run.T0);
                player.FormPose = FormKit.Cast;
                player.FormPoseProgress = 0.15f + 0.4f * t;
                Vector3 feet = player.transform.position;
                run.A = feet + Vector3.up * (2.5f + size);
                // It grows in beats, not smoothly: each beat a little larger and white for an instant.
                float beats = Mathf.Floor(t * 5f) / 5f;
                bool onBeat = Mathf.Repeat(t * 5f, 1f) < 0.18f;
                FormFx.Place(solids[0], run.A, Quaternion.Euler(run.Age * 60f, run.Age * 95f, 0f), Vector3.one * (size * (0.25f + 0.75f * beats) * (onBeat ? 1.12f : 1f)));
                FormFx.Heat(solids[0], onBeat ? 1f : 0f);
                FormFx.PlaceMark(marks[0], feet, Quaternion.Euler(0f, run.Age * 200f, 0f), new Vector3(3f, 1f, 3f));
                run.T1 -= deltaTime;
                if (run.T1 <= 0f)
                {
                    run.T1 = 0.07f;
                    Vector3 from = FormRules.OnRing(0, 1, 2.2f, Random.value * 360f);
                    ElementLook.Puff(cast.Element, run.A + from, -from, 0.4f);
                }
                if (t < 1f) return true;
                // Thrown: the body is the player's own again, following through.
                run.Phase = 1;
                run.T0 = run.Age;
                SpellWorks.GiveBody(ref run);
                FormFx.ReleaseMark(marks[0], 0.15f);
                marks[0] = -1;
                SpellWorks.Gesture(cast, FormKit.Chop, 0.34f, 0.3f, 1f);
                return true;
            }
            float flown = Mathf.Clamp01((run.Age - run.T0) / FlightSeconds);
            Vector3 at = Vector3.Lerp(run.A, run.B, flown) + Vector3.up * (1.4f * 4f * flown * (1f - flown));
            FormFx.Place(solids[0], at, Quaternion.Euler(run.Age * 500f, run.Age * 310f, 0f), Vector3.one * size);
            FormFx.Heat(solids[0], flown > 0.85f ? 1f : 0f);
            if (flown < 1f) return true;

            FormFx.Release(solids[0]);
            solids[0] = -1;
            float radius = cast.Spec.Radius;
            var blow = cast.Blow(cast.Damage, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeArea(run.B, radius, blow);
            FormKit.Impact(cast, run.B, radius * 0.9f, 2f);
            int crown = Mathf.Clamp(Mathf.RoundToInt(radius * 3f), 8, 14);
            for (int i = 0; i < crown; i++)
            {
                Vector3 outward = FormRules.OnRing(i, crown, radius * (0.45f + 0.3f * (i % 2)), 9f);
                FormFx.Erupt(FormMesh.Spike, run.B + outward, FormKit.Yaw(outward) * Quaternion.Euler(30f + 12f * (i % 2), 0f, 0f),
                    new Vector3(0.9f, 1.8f + 0.9f * (i % 3), 0.9f), cast.Colour, 0.06f, 0.4f + 0.05f * (i % 3), 0.3f);
            }
            for (int i = 0; i < 10; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 10, 1f, 18f);
                Vector3 from = run.B + Vector3.up * 0.9f;
                FormFx.Fly(FormMesh.Shard, from, from + outward * radius * 1.3f, Quaternion.LookRotation(outward), Vector3.one * 1.5f, cast.Colour, 0.25f);
            }
            var patch = cast.Ground(run.B, radius * 0.8f, cast.Spec.Seconds, 0.5f, cast.Linger(cast.Spec.LingerDamage));
            patch.Delay = 0.2f;
            Hazards.Lay(patch);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.15f, 0.6f, 0.45f);
            return false;
        }
    }
}
