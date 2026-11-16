using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Light's gift, The Scales. While the surge lasts two pans of light hang over the player: one takes the weight
    /// of every wound the weapon deals, the other of every wound the player takes. When the surge ends they are read.
    /// If the dealt is the heavier, a sword of light falls on every Marked enemy near for a share of the difference;
    /// if not, the player's mana is gone.
    /// Reads: Damage (percent of the difference), Stagger, Radius, Reach, Count (the most a sword can deal).
    /// </summary>
    public class ScalesForm : RunningForm
    {
        public override string Id => SpellForms.Scales;

        private const float Beam = 3.3f;
        private const float Arm = 1.5f;
        private const float Length = 9f;

        /// <summary>What each sword deals: a share of how much heavier the dealt was than the taken, never more than the most.</summary>
        public static float Verdict(float dealt, float taken, float percent, float most) =>
            dealt > taken ? Mathf.Min(most, (dealt - taken) * percent / 100f) : 0f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.T2 = cast.Defence != null ? cast.Defence.Health.Current : 0f;
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 2.6f));
            marks[1] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(new Color(1f, 0.25f, 0.2f), 2.2f));
            marks[2] = FormFx.HoldMark(FormMark.Streak, FormKit.Lit(cast.Colour, 3f));
            marks[3] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 1.6f));
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase == 0) run.T0 += damage;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                float health = cast.Defence != null ? cast.Defence.Health.Current : 0f;
                if (health < run.T2) run.T1 += run.T2 - health;
                run.T2 = health;
                // The beam tips toward the heavier pan; the dealt hangs on the left, the taken on the right.
                float lean = Mathf.Clamp((run.T0 - run.T1) / 60f, -1f, 1f);
                run.T3 = Mathf.Lerp(run.T3, lean, 1f - Mathf.Exp(-6f * deltaTime));
                Vector3 pivot = here + Vector3.up * Beam;
                Vector3 left = pivot + Vector3.left * Arm - Vector3.up * (run.T3 * 0.7f);
                Vector3 right = pivot + Vector3.right * Arm + Vector3.up * (run.T3 * 0.7f);
                FormFx.PlaceMarkFacing(marks[0], left, 0.9f + Mathf.Min(0.7f, run.T0 / 150f));
                FormFx.PlaceMarkFacing(marks[1], right, 0.9f + Mathf.Min(0.7f, run.T1 / 150f));
                Vector3 across = right - left;
                FormFx.PlaceMark(marks[2], left, Quaternion.LookRotation(across.normalized, Vector3.up), new Vector3(0.22f, 1f, across.magnitude), false);
                FormFx.PlaceMark(marks[3], here, Quaternion.Euler(0f, run.Age * 30f, 0f), new Vector3(3f, 1f, 3f));
                if (SigKit.SurgeOn(cast)) return true;
                run.Phase = 1;
                run.T2 = run.Age;
                float each = Verdict(run.T0, run.T1, cast.Spec.Damage, cast.Spec.Count);
                run.Done = 0;
                run.Count = 0;
                if (each <= 0f)
                {
                    // Found wanting.
                    cast.Combat.Mana.Empty();
                    if (cast.Defence != null) cast.Defence.Callout("The light leaves you");
                    Clover.Core.ScreenFlash.Flash(new Color(0f, 0f, 0f, 0.55f), 0.3f);
                    FormFx.Lay(FormMark.Ring, here, 6f, FormKit.Lit(cast.Colour, 2f), 0.5f, 0.4f);
                    return false;
                }
                var things = SpellWorks.Things(run);
                int near = SigKit.Gather(run, here, cast.Reach, SpellWorks.NotesEach);
                for (int i = 0; i < near; i++)
                    if (SigKit.Has(things[i], AfflictionKind.Marked)) things[run.Count++] = things[i];
                for (int i = run.Count; i < near; i++) things[i] = null;
                run.T0 = each;
                if (cast.Defence != null) cast.Defence.Callout(run.Count > 0 ? "Judged" : "No one is Marked");
                for (int i = 0; i < run.Count; i++) FormKit.Warn(cast, SigKit.Foot(things[i], here.y), cast.Spec.Radius, 0.45f + i * 0.08f);
                SigKit.Moment(cast, 0f, 0.1f, 0.6f, 0.35f, 0.25f);
                for (int i = 0; i < 4; i++) FormFx.ReleaseMark(marks[i], 0.25f);
                marks[0] = marks[1] = marks[2] = marks[3] = -1;
                return run.Count > 0;
            }
            var judged = SpellWorks.Things(run);
            Quaternion down = SigKit.Down;
            Vector3 along = down * Vector3.forward;
            while (run.Done < run.Count && run.Age >= run.T2 + 0.45f + run.Done * 0.08f)
            {
                int i = run.Done++;
                if (!FormKit.Alive(judged[i])) continue;
                Vector3 at = SigKit.Foot(judged[i], here.y);
                FormFx.Drop(FormMesh.Sword, at + Vector3.up * 12f - along * Length, at - Vector3.up * (Length * 0.15f) - along * Length, down, Vector3.one * Length, cast.Colour, 0.08f, 0.9f, 0.3f, 0.09f);
                var blow = cast.Blow(run.T0, cast.Stagger);
                blow.Heavy = true;
                // One sword, one enemy: a crowd of the Marked is not struck once for each of them.
                cast.Combat.StrikeOne(judged[i], blow, at + Vector3.up * 6f);
                FormKit.Impact(cast, at, cast.Spec.Radius, 1.6f);
                SigKit.Column(at, FormKit.Lit(cast.Colour, 3.6f), 2.2f, 0.22f);
            }
            if (run.Done < run.Count) return true;
            SigKit.Moment(cast, 0.16f, 0.7f, 0f, 1f, 0.4f);
            return false;
        }
    }
}
