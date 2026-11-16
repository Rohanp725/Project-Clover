using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A ring of bullets: they snap into being one after another in a circle round the enemy faced, points inward,
    /// turn slowly with it while the delay runs, and then all go in together.
    /// </summary>
    public class BulletsForm : RunningForm
    {
        public override string Id => SpellForms.Bullets;

        private const float FlySeconds = 0.1f;
        private const float Length = 1.25f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, SpellWorks.SolidsEach);
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            run.A = FormKit.AimPoint(cast);
            run.Victim = FormKit.Victim(cast);
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Shard, cast.Colour, 0.04f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 3f));
            SpellWorks.Gesture(cast, FormKit.Cast, 0.35f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            var times = SpellWorks.Times(run);
            float radius = cast.Spec.Radius;
            if (run.Phase == 0 && FormKit.Alive(run.Victim))
            {
                Vector3 under = run.Victim.transform.position;
                run.A = new Vector3(under.x, cast.Origin.y, under.z);
            }
            if (run.Phase == 0 && run.Age >= run.T0)
            {
                run.Phase = 1;
                run.T1 = run.Age;
                // The turn of the ring is fixed as they go; each arrives a hair after the one before, so the blows rattle.
                run.T2 = run.T0 * 40f;
                for (int i = 0; i < run.Count; i++) times[i] = run.Age + FlySeconds + 0.012f * i;
                FormFx.ReleaseMark(marks[0], 0.1f);
                marks[0] = -1;
            }
            float turn = run.Phase == 0 ? run.Age * 40f : run.T2;
            // The ring on the ground draws in as the moment comes.
            float closing = Mathf.Lerp(1.25f, 1f, Mathf.Clamp01(run.Age / run.T0));
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f * closing, 1f, radius * 2f * closing));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, -run.Age * 120f, 0f), new Vector3(2.2f, 1f, 2.2f));
            Vector3 heart = run.A + Vector3.up * 1.1f;
            bool owed = false;
            for (int i = 0; i < run.Count; i++)
            {
                if (solids[i] < 0) continue;
                owed = true;
                Vector3 outward = FormRules.OnRing(i, run.Count, 1f, turn);
                if (run.Phase == 0)
                {
                    float formed = Mathf.Clamp01((run.Age - i * 0.025f) / 0.08f);
                    FormFx.Place(solids[i], heart + outward * (radius + Length * 0.5f), Quaternion.LookRotation(-outward), Vector3.one * (Length * formed));
                    FormFx.Heat(solids[i], formed < 1f || run.Age > run.T0 - 0.07f ? 1f : 0f);
                    continue;
                }
                float flown = 1f - Mathf.Clamp01((times[i] - run.Age) / (FlySeconds + 0.012f * i));
                if (flown < 1f)
                {
                    FormFx.Place(solids[i], heart + outward * ((radius + Length * 0.5f) * (1f - flown * flown)), Quaternion.LookRotation(-outward), new Vector3(Length * 0.8f, Length * 0.8f, Length * 1.5f));
                    FormFx.Heat(solids[i], 0f);
                    continue;
                }
                FormFx.Release(solids[i]);
                solids[i] = -1;
                var blow = cast.Blow(cast.Damage, cast.Stagger);
                if (FormKit.Alive(run.Victim)) cast.Combat.StrikeOne(run.Victim, blow, heart + outward * radius);
                else cast.Combat.StrikeArea(run.A, 1.2f, blow);
                FormFx.Cut(heart, outward, radius * 1.7f, 0.34f, FormKit.Lit(cast.Colour, 3.2f), 0.16f);
                if (i == run.Count - 1)
                {
                    FormKit.Impact(cast, run.A, 1.5f, 0.95f);
                    FormFx.Face(FormMark.Cross, heart, 3.4f, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.6f);
                }
                else if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.1f, 0.1f);
            }
            return owed;
        }
    }
}
