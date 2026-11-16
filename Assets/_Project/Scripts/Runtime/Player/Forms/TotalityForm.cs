using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Eclipse's crown, Totality. When the surge is lit a black sun with a burning rim hangs over the player and the
    /// ground goes dark. While it lasts every enemy near is kept Marked and Cursed, and what each one loses to the
    /// weapon is remembered. When the surge ends the light returns: a column of it strikes each enemy wounded in the
    /// dark for a share of what it lost. (That blows are critical and feed the player in the dark are numbers on the mark.)
    /// Reads: Reach, Stacks, Damage (percent of what each lost), Count (the most one column deals), Stagger.
    /// </summary>
    public class TotalityForm : RunningForm
    {
        public override string Id => SpellForms.Totality;

        private const float Height = 4.6f;

        /// <summary>What the returning light deals to one enemy: a share of what it lost in the dark, never more than the most.</summary>
        public static float Returned(float lost, float percent, float most) => Mathf.Min(most, Mathf.Max(0f, lost) * percent / 100f);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, new Color(0.01f, 0.01f, 0.02f), 0.02f);
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Scorch, new Color(0f, 0f, 0.02f, 0.82f));
            marks[1] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 3f));
            marks[2] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(cast.Colour, 1.1f));
            marks[3] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2f));
            Clover.Core.ScreenFlash.Flash(new Color(0f, 0f, 0f, 0.7f), 0.35f);
            if (cast.Defence != null) cast.Defence.Callout(cast.Name);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.1f, 0.3f, 0.3f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase != 0 || target == null) return;
            var wounded = SpellWorks.Things(run);
            var lost = SpellWorks.Times(run);
            for (int i = 0; i < run.Count; i++)
                if (wounded[i] == target)
                {
                    lost[i] += damage;
                    return;
                }
            if (run.Count >= SpellWorks.NotesEach) return;
            wounded[run.Count] = target;
            lost[run.Count] = damage;
            run.Count++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                float covered = Mathf.Clamp01(run.Age / 0.5f);
                Vector3 sun = here + Vector3.up * 4.1f + Vector3.forward * 0.5f;
                FormFx.Place(solids[0], sun, Quaternion.Euler(0f, run.Age * 10f, 0f), Vector3.one * (1.25f * covered));
                FormFx.PlaceMark(marks[0], here, Quaternion.identity, new Vector3(70f * covered, 1f, 70f * covered));
                FormFx.PlaceMarkFacing(marks[1], sun, 3.3f * covered * (1f + 0.03f * Mathf.Sin(run.Age * 13f)));
                FormFx.PlaceMarkFacing(marks[2], sun, 4.4f * covered, run.Age * 14f);
                FormFx.PlaceMark(marks[3], here, Quaternion.identity, new Vector3(cast.Reach * 2f, 1f, cast.Reach * 2f));
                run.T0 -= deltaTime;
                if (run.T0 <= 0f)
                {
                    run.T0 = 1f;
                    cast.Combat.AfflictArea(here, cast.Reach, AfflictionKind.Marked, cast.Spec.Stacks);
                    cast.Combat.AfflictArea(here, cast.Reach, AfflictionKind.Cursed, cast.Spec.Stacks);
                }
                if (SigKit.SurgeOn(cast)) return true;
                // The light returns.
                run.Phase = 1;
                run.T1 = run.Age;
                run.Done = 0;
                FormFx.Release(solids[0], 0.25f);
                solids[0] = -1;
                for (int i = 0; i < 4; i++)
                {
                    FormFx.ReleaseMark(marks[i], i == 0 ? 0.2f : 0.1f);
                    marks[i] = -1;
                }
                Clover.Core.ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.6f), 0.14f);
                FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 4f), 0.5f, cast.Reach * 2f);
                SigKit.Moment(cast, 0.08f, 0.3f, 0.6f, 0.35f, 0f);
                return run.Count > 0;
            }
            var things = SpellWorks.Things(run);
            var took = SpellWorks.Times(run);
            while (run.Done < run.Count && run.Age >= run.T1 + 0.2f + run.Done * 0.07f)
            {
                int i = run.Done++;
                if (!FormKit.Alive(things[i])) continue;
                Vector3 at = SigKit.Foot(things[i], here.y);
                cast.Combat.StrikeOne(things[i], cast.Blow(Returned(took[i], cast.Spec.Damage, cast.Spec.Count), cast.Stagger, 0), at + Vector3.up * 6f);
                SigKit.Column(at, FormKit.Lit(cast.Colour, 4f), 2.4f, 0.3f);
                FormKit.Impact(cast, at, 1.8f, 1.2f);
            }
            if (run.Done < run.Count) return true;
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.14f, 0.6f, 0.35f);
            return false;
        }
    }
}
