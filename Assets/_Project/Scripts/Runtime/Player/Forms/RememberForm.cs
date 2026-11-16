using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Wind's gift, The Wind Remembers. While the surge lasts every blow of the weapon leaves its cut hanging in the
    /// air where it fell, sixteen at most; when the surge ends time slows and all of them fall again, one on the heels
    /// of another, each for a share of the blow it remembers.
    /// Reads: Damage (percent of the blow), Stagger, Stacks.
    /// </summary>
    public class RememberForm : RunningForm
    {
        public override string Id => SpellForms.Remember;

        public const int Most = 16;
        private const float Between = 0.05f;
        private const float Breath = 0.3f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            FormFx.Lay(FormMark.Ring, cast.Origin, 1f, FormKit.Lit(cast.Colour, 2.6f), 0.4f, 6f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase != 0 || target == null) return;
            var points = SpellWorks.Points(run);
            var blows = SpellWorks.Times(run);
            if (run.Count < Most)
            {
                points[run.Count] = target.transform.position;
                blows[run.Count] = damage;
                run.Count++;
                // Shown the moment it is left.
                run.T0 = 0f;
            }
            // Past sixteen the newest blow is added to the lightest cut already hanging.
            else
            {
                int lightest = 0;
                for (int i = 1; i < Most; i++)
                    if (blows[i] < blows[lightest]) lightest = i;
                blows[lightest] += damage;
            }
        }

        private static Vector3 Lie(int index) => Quaternion.Euler(0f, 20f + index * 67f, 0f) * Vector3.forward;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var blows = SpellWorks.Times(run);
            if (run.Phase == 0)
            {
                run.T0 -= deltaTime;
                if (run.T0 <= 0f)
                {
                    // The cuts hang: thin, pale, drawn again before they fade.
                    run.T0 = 0.3f;
                    for (int i = 0; i < run.Count; i++)
                        FormFx.Cut(points[i] + Vector3.up * (1.1f + 0.25f * (i % 3)), Lie(i), 2.8f, 0.16f, FormKit.Lit(cast.Colour, 2.8f), 0.42f, 0.12f);
                }
                if (SigKit.SurgeOn(cast)) return true;
                if (run.Count == 0) return false;
                run.Phase = 1;
                run.T1 = run.Age;
                run.Done = 0;
                SigKit.Moment(cast, 0f, 0.1f, 0.9f, 0.3f, 0.2f);
                for (int i = 0; i < run.Count; i++)
                    FormFx.Cut(points[i] + Vector3.up * (1.1f + 0.25f * (i % 3)), Lie(i), 3.4f, 0.3f, FormKit.Lit(cast.Colour, 3.6f), Breath + i * Between + 0.05f, 0.2f);
                return true;
            }
            while (run.Done < run.Count && run.Age >= run.T1 + Breath + run.Done * Between)
            {
                int i = run.Done++;
                Vector3 chest = points[i] + Vector3.up * 1.2f;
                cast.Combat.StrikeArea(points[i], 1.4f, cast.Blow(FormRules.Echo(blows[i], cast.Spec.Damage), cast.Spec.Stagger));
                FormFx.Cut(chest, Lie(i), 6.5f, 0.9f, FormKit.Lit(cast.Colour, 4f), 0.16f, 0.05f);
                FormFx.Face(FormMark.Star, chest, 1.6f, FormKit.Lit(cast.Colour, 4f), 0.09f, 2.4f);
                ElementLook.Sparks(cast.Element, chest, Lie(i), 0.7f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.12f, 0.1f);
            }
            if (run.Done < run.Count) return true;
            Vector3 last = points[run.Count - 1];
            FormFx.Lay(FormMark.Cross, last, 6f, FormKit.Lit(cast.Colour, 2.8f), 1.2f, -1f, 0f, 30f);
            FormFx.Lay(FormMark.Ring, last, 1f, FormKit.Lit(cast.Colour, 3.4f), 0.4f, 9f);
            SigKit.Moment(cast, 0.14f, 0.6f, 0f, 1f, 0.35f);
            return false;
        }
    }
}
