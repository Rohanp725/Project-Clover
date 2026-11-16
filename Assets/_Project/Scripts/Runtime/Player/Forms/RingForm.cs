using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A ring closed round the player and the enemy faced: stakes of the element come in from outside and stand in a
    /// circle that nothing walks through, the player included, for a while. Enemies that touch it are bitten.
    /// </summary>
    public class RingForm : RunningForm
    {
        public override string Id => SpellForms.Ring;

        private const int Stakes = 14;
        private const int Sides = 12;
        private const float CloseSeconds = 0.28f;
        private const float BiteEvery = 0.4f;

        public override int Cast(in SpellCast cast)
        {
            // One ring at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.T2 = Mathf.Max(2f, cast.Spec.Radius);
            if (cast.Target != null)
            {
                // Round both of them: its middle is between them, and it is never too small to hold both.
                Vector3 gap = cast.Target.position - cast.Origin;
                gap.y = 0f;
                run.A = cast.Origin + gap * 0.5f;
                run.T2 = Mathf.Max(run.T2, gap.magnitude * 0.5f + 1.4f);
            }
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < Stakes; i++) solids[i] = FormFx.Hold(FormMesh.Spike, cast.Colour, 0.045f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2.2f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 0.9f));
            SpellWorks.Gesture(cast, FormKit.Chop, 0.34f, 0.25f, 0.95f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float radius = run.T2;
            float seconds = Mathf.Max(1f, cast.Spec.Seconds);
            float closed = Mathf.Clamp01(run.Age / CloseSeconds);
            if (run.Phase == 0 && closed >= 1f)
            {
                run.Phase = 1;
                run.T1 = run.Age + 0.1f;
                float side = 2f * radius * Mathf.Tan(Mathf.PI / Sides) * 1.06f;
                for (int i = 0; i < Sides; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, Sides, 1f);
                    SpellWorks.Barrier(run, run.A + outward * radius + Vector3.up * 1.5f, FormKit.Yaw(outward), new Vector3(side, 3f, 0.5f));
                }
                FormFx.Lay(FormMark.Ring, run.A, radius * 2.5f, FormKit.Lit(cast.Colour, 3.4f), 0.25f, radius * 2f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.3f, 0.25f);
            }
            float left = seconds - run.Age;
            for (int i = 0; i < Stakes; i++)
            {
                // They come in from outside, one after another round the circle, and drive into the ground.
                float mine = Mathf.Clamp01((run.Age - i * 0.012f) / (CloseSeconds - 0.1f));
                Vector3 outward = FormRules.OnRing(i, Stakes, 1f, 8f);
                float tall = 2.5f + 0.5f * (i % 2);
                float sink = left < 0.3f ? Mathf.Clamp01(left / 0.3f) : 1f;
                FormFx.Place(solids[i], run.A + outward * (radius * Mathf.Lerp(1.7f, 1f, mine * mine)), FormKit.Yaw(-outward) * Quaternion.Euler(Mathf.Lerp(50f, 9f, mine), 0f, 0f),
                    new Vector3(0.55f, tall * Mathf.Max(0.02f, mine * sink), 0.55f));
                FormFx.Heat(solids[i], mine < 1f || (left < 0.8f && Mathf.Repeat(run.Age * 7f, 1f) < 0.4f) ? 1f : 0f);
            }
            float across = radius * 2f * Mathf.Lerp(1.7f, 1f, closed * closed);
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(across, 1f, across));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 20f, 0f), new Vector3(across * 0.9f, 1f, across * 0.9f));
            if (run.Phase == 1 && run.Age >= run.T1)
            {
                run.T1 += BiteEvery;
                // Only the band where the stakes stand bites: the inside of the ring is safe ground.
                int bitten = cast.Combat.StrikeArea(run.A, radius + 0.9f, cast.Blow(cast.Spec.LingerDamage, 0f), null, radius - 1.1f);
                if (bitten > 0) FormFx.Lay(FormMark.Ring, run.A, radius * 2f, FormKit.Lit(cast.Colour, 3.6f), 0.18f, radius * 2.15f);
            }
            return run.Age < seconds;
        }
    }
}
