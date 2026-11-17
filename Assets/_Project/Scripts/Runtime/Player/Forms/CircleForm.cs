using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Wind's Closing Circle. The player runs a full ring round the enemy faced, drawing a line on the ground behind
    /// them; when the line meets its own start it snaps inward, a column of wind stands in it, and everything inside
    /// is struck and thrown into the air. The player ends where they began, facing in, and cannot be touched while
    /// they run.
    /// Reads: Reach (to the enemy), Radius (the widest the ring may be), Seconds (in the air), Damage, Stagger, Stacks.
    /// </summary>
    public class CircleForm : RunningForm, ISpellGate
    {
        public override string Id => SpellForms.Circle;

        private const float RunSeconds = 0.7f;
        private const float SnapSeconds = 0.2f;
        private const float AfterSeconds = 0.25f;
        private const int Pieces = 24;      // pieces of the line laid round the ring

        /// <summary>How many the last circle took off the ground, for probes.</summary>
        public static int Taken { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Taken = 0;

        /// <summary>The circle needs an enemy near enough to run round, and a body free to run. Refused here, the cast costs nothing.</summary>
        public bool CanCast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            if (victim == null || !FormKit.Alive(victim)) return false;
            Vector3 apart = victim.transform.position - cast.Origin;
            apart.y = 0f;
            if (apart.magnitude > cast.Reach + 0.5f) return false;
            if (cast.Player == null || cast.Player.IsLeaping) return false;
            if (cast.Defence != null && cast.Defence.IsHelpless) return false;
            return SpellWorks.RunningOf(Id) == 0;
        }

        public override int Cast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            if (victim == null || !FormKit.Alive(victim)) return 0;
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.Victim = victim;
            run.C = cast.Origin;
            run.A = SigKit.Foot(victim, cast.Origin.y);
            Vector3 outward = cast.Origin - run.A;
            outward.y = 0f;
            float radius = Mathf.Clamp(outward.magnitude, 2f, Mathf.Max(2f, cast.Spec.Radius));
            run.B = outward.sqrMagnitude > 0.01f ? outward.normalized : -cast.Forward;
            run.T0 = radius;
            run.Done = 0;
            // The body is carried by hand round the ring: it must not catch on the one it runs round. Land() gives it back.
            var body = cast.Player.GetComponent<CharacterController>();
            if (body != null) body.enabled = false;
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 1.2f, 0.6f));
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
            FormFx.Lay(FormMark.Disc, run.A, radius * 2f, FormKit.Stain(cast.Colour, 0.45f), RunSeconds + SnapSeconds);
            SpellWorks.Ghost(0.3f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 0.3f, Vector3.up, 1f);
            return 0;
        }

        public override void End(ref SpellRun run)
        {
            if (run.Phase == 9 || run.Cast.Player == null) return;
            Land(run.Cast.Player, run.C);
        }

        private static void Land(PlayerController player, Vector3 at)
        {
            var body = player.GetComponent<CharacterController>();
            if (body == null || body.enabled) return;
            player.transform.position = at;
            body.enabled = true;
        }

        private static Vector3 OnRing(in SpellRun run, float degrees) => run.A + Quaternion.Euler(0f, degrees, 0f) * run.B * run.T0;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var marks = SpellWorks.Marks(run);
            float radius = run.T0;
            Color light = FormKit.Lit(cast.Colour, 3.4f);

            if (run.Phase == 0)
            {
                float through = Mathf.Clamp01(run.Age / RunSeconds);
                float eased = through * through * (3f - 2f * through);
                Vector3 on = OnRing(run, eased * 360f);
                // Out to the ring in the first steps, and home from it in the last.
                float join = Mathf.Min(Mathf.Clamp01(through * 8f), Mathf.Clamp01((1f - through) * 8f));
                Vector3 want = Vector3.Lerp(run.C, on, join);
                Vector3 going = want - player.transform.position;
                going.y = 0f;
                player.transform.position = want;
                if (going.sqrMagnitude > 0.0001f) player.transform.rotation = FormKit.Yaw(going);
                player.FormPose = FormKit.Draw;
                player.FormPoseProgress = Mathf.Lerp(0.15f, 0.45f, through);
                player.FormSwordOut = true;
                // The line drawn behind: each piece lies until the ring has closed and snapped.
                while (run.Done < Pieces && run.Done < eased * Pieces)
                {
                    Vector3 from = OnRing(run, run.Done * 360f / Pieces);
                    Vector3 to = OnRing(run, (run.Done + 1) * 360f / Pieces);
                    FormFx.Run(FormMark.Streak, from, to - from, (to - from).magnitude * 1.25f, 0.55f, light, RunSeconds - run.Age + SnapSeconds + 0.05f, 0.3f);
                    if (run.Done % 3 == 0) ElementLook.Puff(cast.Element, from + Vector3.up * 0.3f, Vector3.up, 0.6f);
                    run.Done++;
                }
                if (through < 1f) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                Land(player, run.C);
                player.transform.rotation = FormKit.Yaw(run.A - run.C);
                FormFx.TintMark(marks[0], FormKit.Lit(cast.Colour, 4f));
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.12f, 0.1f);
                return true;
            }
            player.FormPose = FormKit.Chop;
            player.FormSwordOut = true;
            if (run.Phase == 1)
            {
                float through = Mathf.Clamp01((run.Age - run.T1) / SnapSeconds);
                float wide = Mathf.Lerp(radius * 2f, radius * 1.1f, through * through);
                FormFx.PlaceMark(marks[0], run.A, Quaternion.Euler(0f, through * 120f, 0f), new Vector3(wide, 1f, wide));
                player.FormPoseProgress = Mathf.Lerp(0.2f, 0.6f, through);
                if (through < 1f) return true;
                run.Phase = 2;
                run.T1 = run.Age;
                FormFx.ReleaseMark(marks[0], 0.1f);
                marks[0] = -1;
                // The wind inside rises: struck first, so what is laid is laid before they leave the ground.
                cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Damage, cast.Stagger));
                Taken = cast.Combat.LiftArea(run.A, radius, Mathf.Max(0.4f, cast.Spec.Seconds));
                SigKit.Column(run.A, FormKit.Lit(cast.Colour, 2.2f, 0.7f), radius * 0.9f, 0.45f);
                SigKit.Column(run.A, FormKit.Lit(cast.Colour, 5f), radius * 0.35f, 0.3f);
                FormFx.Lay(FormMark.Ring, run.A, radius * 1.1f, light, 0.4f, radius * 3.2f, 90f);
                FormFx.Lay(FormMark.Sigil, run.A, radius * 2f, FormKit.Lit(cast.Colour, 2.2f), 0.5f, radius * 2.3f, 200f);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 foot = OnRing(run, i * 45f + 20f);
                    FormFx.Cut(foot + Vector3.up * 1.6f, Vector3.up + (run.A - foot).normalized * 0.25f, 3.6f, 0.3f, light, 0.3f + 0.03f * i, 0.05f);
                    ElementLook.Puff(cast.Element, Vector3.Lerp(foot, run.A, 0.4f) + Vector3.up * 0.4f, Vector3.up, 1.2f);
                }
                FormKit.Impact(cast, run.A, radius, 1.2f);
                SigKit.Moment(cast, 0.08f, 0.5f, Taken > 0 ? 0.3f : 0f, 0.5f, 0.3f);
                return true;
            }
            player.FormPoseProgress = Mathf.Lerp(0.6f, 0.95f, (run.Age - run.T1) / AfterSeconds);
            return run.Age < run.T1 + AfterSeconds;
        }
    }
}
