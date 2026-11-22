using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A patch of ground at the point aimed at that bristles for a while: small spikes of the element keep breaking
    /// out all over it, and at every interval the whole patch bites what stands on it.
    /// </summary>
    public class FieldForm : RunningForm
    {
        public override string Id => SpellForms.Field;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = FormKit.AimPoint(cast);
            run.T1 = 0.3f;
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.9f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 1.8f));
            FormFx.Lay(FormMark.Ring, run.A, 0.6f, FormKit.Lit(cast.Colour, 3f), 0.3f, cast.Spec.Radius * 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            float seconds = Mathf.Max(0.5f, cast.Spec.Seconds);
            // It spreads out from its middle in a moment, and draws in again at the end.
            float spread = Mathf.Clamp01(run.Age / 0.25f) * Mathf.Clamp01((seconds - run.Age) / 0.3f);
            float across = radius * 2f * spread;
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(across, 1f, across));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 35f, 0f), new Vector3(across * 0.82f, 1f, across * 0.82f));
            if (run.Age >= seconds) return false;

            run.T2 -= deltaTime;
            if (run.T2 <= 0f && spread >= 1f)
            {
                run.T2 = 0.07f;
                Sprout(cast, run.A, radius, 0.5f, 0.6f + Random.value * 0.6f, 0.22f);
            }
            if (run.Age < run.T1) return true;
            run.T1 += Mathf.Max(0.1f, cast.Spec.Interval);
            // The bite: every spike on it stands up at once, and a ring runs to its edge.
            cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Spec.LingerDamage, 0f));
            // A field that draws in (The Pit): what stands in it is dragged toward its middle.
            if (cast.Spec.Pull > 0f) cast.Combat.Displace(run.A, radius, Clover.World.GroundRules.PullStep(cast.Spec.Pull, cast.Spec.Interval), true);
            FormFx.Lay(FormMark.Ring, run.A, radius * 0.5f, FormKit.Lit(cast.Colour, 2.4f), 0.26f, radius * 2f);
            int stood = Mathf.Clamp(Mathf.RoundToInt(radius * 2.5f), 5, 10);
            for (int i = 0; i < stood; i++) Sprout(cast, run.A, radius, 0.65f, 1.2f + Random.value * 0.7f, 0.2f);
            ElementLook.Sparks(cast.Element, run.A + Vector3.up * 0.6f, Vector3.up, 0.6f);
            return true;
        }

        private static void Sprout(in SpellCast cast, Vector3 centre, float radius, float wide, float tall, float hold)
        {
            Vector2 spot = Random.insideUnitCircle * (radius * 0.92f);
            FormFx.Erupt(FormMesh.Spike, centre + new Vector3(spot.x, 0f, spot.y), Quaternion.Euler(Random.Range(-14f, 14f), Random.value * 360f, Random.Range(-14f, 14f)),
                new Vector3(wide, tall, wide), cast.Colour, 0.05f, hold, 0.18f, 0.03f);
        }
    }
}
