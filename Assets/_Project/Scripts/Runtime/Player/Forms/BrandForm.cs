using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A sign set on the enemy faced: it turns on the ground under them and hangs over their head for a while, and
    /// each of the next few blows of the weapon that land on them sets off a burst. With no enemy faced, nothing is
    /// branded and the mana is lost.
    /// </summary>
    public class BrandForm : RunningForm
    {
        public override string Id => SpellForms.Brand;

        public override int Cast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            if (victim == null)
            {
                ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 1.2f + cast.Forward, cast.Forward, 0.6f);
                if (cast.Defence != null) cast.Defence.Callout("No one to brand");
                return 0;
            }
            // One brand at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Victim = victim;
            run.Count = Mathf.Max(1, cast.Spec.Count);
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.4f));
            marks[1] = FormFx.HoldMark(FormMark.Cross, FormKit.Lit(cast.Colour, 3.2f));
            marks[2] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 2.4f));
            Vector3 chest = victim.transform.position + Vector3.up * 1.1f;
            // Burned on: a cut across them and a ring snapping shut.
            FormFx.Cut(chest, Vector3.Cross(Vector3.up, cast.Forward) + cast.Forward * 0.4f, 4.4f, 0.5f, FormKit.Lit(cast.Colour, 3.4f), 0.18f);
            FormFx.Lay(FormMark.Ring, victim.transform.position, 5f, FormKit.Lit(cast.Colour, 3f), 0.22f, 1.8f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.12f, 0.15f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // Counted here, set off in the next step: the swing is still being reckoned.
            if (target != null && ReferenceEquals(target, run.Victim)) run.Phase++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            if (!FormKit.Alive(run.Victim)) return false;
            Vector3 foot = run.Victim.transform.position;
            while (run.Phase > 0 && run.Done < run.Count)
            {
                run.Phase--;
                run.Done++;
                cast.Combat.StrikeArea(foot, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                FormKit.Impact(cast, foot, cast.Spec.Radius * 0.75f, run.Done == run.Count ? 1.2f : 0.7f);
                FormFx.Face(FormMark.Cross, foot + Vector3.up * 1.1f, 3f, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.4f, run.Done * 30f);
            }
            run.Phase = 0;
            if (run.Done >= run.Count || run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds)) return false;
            // The sign over the head beats, and is smaller for every burst spent.
            float left = 1f - run.Done / (float)run.Count;
            float beat = 1f + 0.12f * Mathf.Sin(run.Age * 9f);
            float snap = Mathf.Clamp01(run.Age / 0.1f);
            FormFx.PlaceMark(marks[0], foot, Quaternion.Euler(0f, run.Age * 80f, 0f), new Vector3(2.6f, 1f, 2.6f) * (2f - snap));
            FormFx.PlaceMarkFacing(marks[1], foot + Vector3.up * 2.9f, (0.7f + 0.7f * left) * beat, 0f);
            FormFx.PlaceMark(marks[2], foot, Quaternion.Euler(0f, -run.Age * 140f, 0f), new Vector3(1.7f, 1f, 1.7f));
            return true;
        }
    }
}
