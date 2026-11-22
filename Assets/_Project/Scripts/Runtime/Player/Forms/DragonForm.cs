using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A dragon of the element: its head forms at the player's shoulder, jaws open, and lunges down a lane with its
    /// neck winding behind it, carrying whatever it meets along; at the end of the lane the jaws close.
    /// </summary>
    public class DragonForm : RunningForm
    {
        public override string Id => SpellForms.Dragon;

        private const float FormSeconds = 0.3f;
        private const int Joints = 7;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.B = cast.Forward;
            // The head is as wide as the lane it bites.
            run.T2 = Mathf.Clamp(cast.Spec.Radius * 1.9f, 1.4f, 3.6f);
            run.T1 = cast.Reach / Mathf.Max(4f, cast.Spec.Speed * 1.5f);
            var solids = SpellWorks.Solids(run);
            solids[0] = FormFx.Hold(FormMesh.DragonSkull, cast.Colour, 0.07f);
            solids[1] = FormFx.Hold(FormMesh.DragonJaw, cast.Colour, 0.07f);
            for (int i = 0; i < Joints; i++) solids[2 + i] = FormFx.Hold(FormMesh.DragonSpine, cast.Colour, 0.06f);
            FormFx.Run(FormMark.Lane, cast.Origin + cast.Forward * 0.8f, cast.Forward, cast.Reach - 0.8f, cast.Spec.Radius * 2f, FormKit.Lit(cast.Colour, 1.7f), FormSeconds + run.T1);
            FormKit.Gather(cast, cast.Origin, 2.6f);
            SpellWorks.Gesture(cast, FormKit.Thrust, FormSeconds + 0.25f, 0f, 0.8f, true);
            return 0;
        }

        // Where the dragon's line runs at a distance along the lane: it winds from side to side and rides up and down.
        private static Vector3 Along(in SpellRun run, float distance)
        {
            Vector3 right = Vector3.Cross(Vector3.up, run.B);
            float wind = Mathf.Sin(distance * 0.95f) * Mathf.Min(0.45f, distance * 0.2f);
            return run.A + run.B * distance + right * wind + Vector3.up * (1.25f + 0.3f * Mathf.Sin(distance * 0.7f + 1f));
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            float size = run.T2;
            float reach = cast.Reach;
            if (run.Phase == 0)
            {
                // Forming at the shoulder: it swells out of a white shape, jaws wide.
                float t = Mathf.Clamp01(run.Age / FormSeconds);
                Vector3 shoulder = run.A + Vector3.Cross(Vector3.up, run.B) * 0.2f + Vector3.up * 1.7f - run.B * 0.3f;
                Quaternion look = Quaternion.LookRotation(run.B, Vector3.up);
                float grown = size * (0.25f + 0.75f * t * (2f - t));
                FormFx.Place(solids[0], shoulder, look, Vector3.one * grown);
                FormFx.Place(solids[1], shoulder, look * Quaternion.Euler(38f, 0f, 0f), Vector3.one * grown);
                FormFx.Heat(solids[0], t < 0.6f ? 1f : 0f);
                FormFx.Heat(solids[1], t < 0.6f ? 1f : 0f);
                for (int i = 0; i < Joints; i++) FormFx.Place(solids[2 + i], shoulder, look, Vector3.zero);
                if (t < 1f) return true;
                run.Phase = 1;
                run.T0 = run.Age;
                run.T3 = 0f;
                SpellWorks.Once(run).Clear();
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.2f, 0.2f);
                return true;
            }
            if (run.Phase == 1)
            {
                float t = Mathf.Clamp01((run.Age - run.T0) / run.T1);
                // Fastest as it leaves; it is slowing as the jaws come together.
                float distance = reach * (1f - (1f - t) * (1f - t));
                Vector3 head = Along(run, distance);
                Vector3 heading = Along(run, distance + 0.3f) - head;
                Quaternion look = Quaternion.LookRotation(heading, Vector3.up);
                float jaws = t > 0.8f ? Mathf.Lerp(38f, 0f, (t - 0.8f) / 0.2f) : 38f + 5f * Mathf.Sin(run.Age * 30f);
                FormFx.Place(solids[0], head, look, Vector3.one * size);
                FormFx.Place(solids[1], head, look * Quaternion.Euler(jaws, 0f, 0f), Vector3.one * size);
                FormFx.Heat(solids[0], 0f);
                FormFx.Heat(solids[1], 0f);
                for (int i = 0; i < Joints; i++)
                {
                    float behind = distance - (i + 1) * size * 0.42f;
                    if (behind < 0.2f)
                    {
                        FormFx.Place(solids[2 + i], run.A, look, Vector3.zero);
                        continue;
                    }
                    Vector3 joint = Along(run, behind);
                    FormFx.Place(solids[2 + i], joint, Quaternion.LookRotation(Along(run, behind + 0.3f) - joint, Vector3.up), Vector3.one * (size * (0.78f - 0.07f * i)));
                }
                // What it passes over is struck once and carried along with the head.
                float passed = distance - run.T3;
                if (passed > 0.01f)
                {
                    cast.Combat.StrikeLane(run.A + run.B * run.T3, run.B, passed + size * 0.6f, cast.Spec.Radius, cast.Blow(cast.Damage * 0.5f, cast.Stagger * 0.5f), SpellWorks.Once(run));
                    cast.Combat.ShoveArea(run.A + run.B * (distance + size * 0.3f), cast.Spec.Radius + 0.8f, run.B * (passed * 0.9f));
                    run.T3 = distance;
                }
                // A charge that carries (The Black Buffalo): what it struck rides on its head down the lane.
                if (cast.Spec.Carries)
                {
                    Vector3 horns = run.A + run.B * (distance + size * 0.3f);
                    FormCarry.Carry(SpellWorks.Once(run), horns, run.B, cast.Spec.OnlyCarrying);
                }
                if (Mathf.Repeat(run.Age, 0.06f) < deltaTime) ElementLook.Puff(cast.Element, head - heading.normalized * size * 0.4f, -run.B, 0.6f);
                if (t < 1f) return true;
                // The jaws close.
                run.Phase = 2;
                run.T0 = run.Age;
                Vector3 end = run.A + run.B * reach;
                var bite = cast.Blow(cast.Damage, cast.Stagger);
                bite.Heavy = true;
                cast.Combat.StrikeArea(end, cast.Spec.Radius * 1.5f, bite);
                // Thrown down at the far end.
                if (cast.Spec.Carries) FormCarry.ThrowDown(SpellWorks.Once(run), cast.Spec.OnlyCarrying);
                FormKit.Impact(cast, end, cast.Spec.Radius * 1.4f, 1.4f);
                FormFx.Heat(solids[0], 1f);
                FormFx.Heat(solids[1], 1f);
                return true;
            }
            // Held shut for a breath, white, then it comes apart from the tail forward.
            if (run.Age < run.T0 + 0.12f) return true;
            for (int i = 0; i < 2 + Joints; i++)
            {
                FormFx.Release(solids[i], i < 2 ? 0.32f : 0.1f + 0.03f * (Joints - i));
                solids[i] = -1;
            }
            return false;
        }
    }
}
