using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A beam: the player braces, a lane shows where it will go, and then a thick bar of the element is held for a
    /// while, biting everything in it at intervals. The stick turns it slowly. The dodge lets it go.
    /// </summary>
    public class BeamForm : RunningForm
    {
        public override string Id => SpellForms.Beam;

        private const float BraceSeconds = 0.3f;
        private const float CloseSeconds = 0.14f;
        private const float TurnWhileFiring = 55f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, false, true))
            {
                run.Phase = 3;
                return 0;
            }
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Lane, FormKit.Lit(cast.Colour, 1.6f));
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, cast.Colour, 0.03f);
            FormKit.Gather(cast, cast.Origin, 2.2f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 3) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float reach = cast.Reach, half = cast.Spec.Radius;
            // The stick turns the beam, or the cursor while the mouse is in use.
            Vector3 steer = player.Steer;
            if (run.HoldsBody && steer.sqrMagnitude > 0.01f)
            {
                float turn = (run.Phase == 0 ? 240f : TurnWhileFiring) * deltaTime;
                player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, Quaternion.LookRotation(steer, Vector3.up), turn);
            }
            Vector3 forward = player.transform.forward;
            Vector3 foot = player.transform.position;
            Vector3 mouth = foot + forward * 0.9f + Vector3.up * 1.15f;
            Quaternion along = Quaternion.LookRotation(forward, Vector3.up);

            if (run.Phase == 0)
            {
                float t = Mathf.Clamp01(run.Age / BraceSeconds);
                player.FormPose = FormKit.Brace;
                player.FormPoseProgress = t * 0.9f;
                FormFx.PlaceMark(marks[0], foot + forward * 0.9f, along, new Vector3(half * 2f, 1f, reach - 0.9f));
                FormFx.Place(solids[0], mouth, Quaternion.Euler(run.Age * 500f, run.Age * 370f, 0f), Vector3.one * (0.55f * t));
                FormFx.Heat(solids[0], Mathf.Repeat(run.Age * 14f, 1f) < 0.5f ? 1f : 0f);
                if (t < 1f) return true;
                run.Phase = 1;
                run.T0 = run.Age;
                run.T1 = run.Age;
                marks[1] = FormFx.HoldMark(FormMark.Beam, FormKit.Lit(cast.Colour, 3f));
                FormFx.TintMark(marks[0], FormKit.Lit(cast.Colour, 0.9f));
                FormKit.Flash(cast, 0.2f, 0.06f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.3f, 0.25f);
                FormFx.Face(FormMark.Star, mouth, 3.2f, FormKit.Lit(cast.Colour, 4f), 0.1f, 4.2f);
            }

            float since = run.Age - run.T0;
            float seconds = Mathf.Max(0.2f, cast.Spec.Seconds);
            if (run.Phase == 1 && since >= seconds)
            {
                run.Phase = 2;
                run.T2 = run.Age;
            }
            // It opens too wide and snaps to size; while it holds it shivers; at the end it thins to a line and is gone.
            float open = since < 0.08f ? Mathf.Lerp(2.1f, 1f, since / 0.08f) : 1f + 0.07f * Mathf.Sin(run.Age * 46f);
            float left = run.Phase == 2 ? 1f - Mathf.Clamp01((run.Age - run.T2) / CloseSeconds) : 1f;
            player.FormPose = FormKit.Cast;
            player.FormPoseProgress = 0.62f;
            FormFx.PlaceMark(marks[0], foot + forward * 0.9f, along, new Vector3(half * 2f * left, 1f, reach - 0.9f));
            FormFx.PlaceMark(marks[1], mouth, along, new Vector3(half * 2.5f * open * left, 1f, reach - 0.9f), false);
            FormFx.Place(solids[0], mouth, Quaternion.Euler(run.Age * 700f, run.Age * 530f, 0f), Vector3.one * (0.62f * left));
            FormFx.Heat(solids[0], 1f);
            if (run.Phase == 2) return left > 0f;

            if (run.Age >= run.T1)
            {
                run.T1 += Mathf.Max(0.08f, cast.Spec.Interval);
                int hits = cast.Combat.StrikeLane(foot, forward, reach, half, cast.Blow(cast.Damage, cast.Stagger));
                Vector3 tip = mouth + forward * (reach - 0.9f);
                FormFx.Face(FormMark.Star, tip, half * 2.6f, FormKit.Lit(cast.Colour, 3.5f), 0.09f, half * 3.4f, run.Age * 200f);
                ElementLook.Sparks(cast.Element, tip, forward, 0.8f);
                ElementLook.Puff(cast.Element, mouth + forward * (reach * Random.Range(0.2f, 0.9f)), forward, 0.5f);
                FormFx.Lay(FormMark.Scorch, foot + forward * (reach * Random.Range(0.25f, 1f)), half * 2.6f, FormKit.Stain(cast.Colour, 0.45f), 2.5f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(hits > 0 ? 0.03f : 0f, hits > 0 ? 0.16f : 0.08f, 0.16f);
            }
            return true;
        }
    }
}
