using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The Rise, given by Dark's root, and Dark's gift A Life for a Life, which buys it back. The player who would have
    /// died stands again with a share of their health, and it drains away, down to a sliver; a sigil hangs on the one that
    /// killed them. With the gift, a finisher on that one (on anyone, if it is already gone) ends the draining and gives
    /// health back: they live again, and may rise once more. <see cref="PlayerUpgrades.Rise"/> begins it.
    /// Reads: Damage (health drained each second), Count (percent of health given back by the finisher).
    /// </summary>
    public class RiseForm : RunningForm
    {
        public override string Id => SpellForms.Rise;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Victim = FormKit.Victim(cast);
            run.Done = cast.Combat.Finishers;
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 3.2f));
            marks[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2f));
            marks[2] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.6f));
            // Up out of the ground.
            FormFx.Lay(FormMark.Scorch, cast.Origin, 7f, FormKit.Stain(cast.Colour, 0.85f), 3f);
            FormFx.Lay(FormMark.Ring, cast.Origin, 9f, FormKit.Lit(cast.Colour, 3.4f), 0.5f, 1f);
            FormFx.Lay(FormMark.Crack, cast.Origin, 6f, FormKit.Lit(cast.Colour, 2.4f), 2.5f);
            SigKit.Crown(cast.Origin, 1.5f, 8, 1.8f, cast.Colour, 0.5f);
            SigKit.Column(cast.Origin, FormKit.Lit(cast.Colour, 3f), 1.6f, 0.3f);
            SigKit.Moment(cast, 0.2f, 0.7f, 0.8f, 0.3f, 0.5f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // The last thing struck was the killer, or the killer is gone and anyone will do.
            // One swing may strike several: the killer among them is enough.
            if (target != null && (target == run.Victim || !FormKit.Alive(run.Victim)))
            {
                run.Flag2 = true;
                run.T2 = run.Age;
            }
            else if (run.Age > run.T2 + 0.15f) run.Flag2 = false;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            Vector3 here = cast.Player.transform.position;
            if (cast.Upgrades == null || !cast.Upgrades.Risen) return false;
            // The root given up (another element taken, the marks forgotten): the Rise goes with it.
            if (cast.Source != null && !cast.Upgrades.Has(cast.Source.Id)) return false;
            // The draining: a point at a time, never the last.
            run.T0 += Mathf.Max(0f, cast.Spec.Damage) * cast.Upgrades.RiseDrain * deltaTime;
            while (run.T0 >= 1f)
            {
                run.T0 -= 1f;
                if (cast.Defence.Health.Current > 1.5f) cast.Defence.Health.Damage(1f);
            }
            FormFx.PlaceMark(marks[1], here, Quaternion.Euler(0f, -run.Age * 60f, 0f), new Vector3(2.4f, 1f, 2.4f));
            if (FormKit.Alive(run.Victim))
            {
                Vector3 killer = run.Victim.transform.position;
                FormFx.PlaceMarkFacing(marks[0], killer + Vector3.up * 1.2f, 2.6f + 0.2f * Mathf.Sin(run.Age * 6f), run.Age * 40f);
                FormFx.PlaceMark(marks[2], SigKit.Foot(run.Victim, here.y), Quaternion.Euler(0f, run.Age * 90f, 0f), new Vector3(2.8f, 1f, 2.8f));
            }
            else
            {
                FormFx.PlaceMark(marks[0], here + Vector3.down * 50f, Quaternion.identity, Vector3.zero);
                FormFx.PlaceMark(marks[2], here + Vector3.down * 50f, Quaternion.identity, Vector3.zero);
            }
            run.T1 -= deltaTime;
            if (run.T1 <= 0f)
            {
                run.T1 = 0.25f;
                ElementLook.Puff(cast.Element, here + Vector3.up * 1.4f, Vector3.up, 0.5f);
            }
            // A finisher is counted when it begins and lands a moment after: it is the killer's if the blow that follows is.
            int finishers = cast.Combat.Finishers;
            if (finishers != run.Done)
            {
                run.Done = finishers;
                run.Flag = true;
                run.T3 = run.Age;
            }
            if (run.Flag && run.Age > run.T3 + 3f) run.Flag = false;
            if (!run.Flag || !run.Flag2 || run.T2 < run.T3 - 0.1f) return true;
            // Bought back only with the gift; without it the finisher is a finisher, and the Risen drain on.
            if (!cast.Upgrades.Has(Trees.DarkTree.LifeForALife))
            {
                run.Flag = false;
                return true;
            }
            // A life for a life.
            cast.Upgrades.EndRise(true, cast.Spec.Count / 100f);
            FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 4f), 0.5f, 10f);
            FormFx.Lay(FormMark.Star, here, 2f, FormKit.Lit(cast.Colour, 3.6f), 0.3f, 7f);
            SigKit.Column(here, FormKit.Lit(cast.Colour, 3.6f), 2.4f, 0.35f);
            if (FormKit.Alive(run.Victim)) FormFx.Run(FormMark.Streak, run.Victim.transform.position, here - run.Victim.transform.position, (here - run.Victim.transform.position).magnitude, 1.2f, FormKit.Lit(cast.Colour, 3.4f), 0.4f, 0.2f);
            SigKit.Moment(cast, 0.2f, 0.6f, 0.6f, 0.35f, 0.5f);
            return false;
        }

        public override void End(ref SpellRun run)
        {
            if (run.Cast.Upgrades != null) run.Cast.Upgrades.EndRise(false, 0f);
        }
    }
}
