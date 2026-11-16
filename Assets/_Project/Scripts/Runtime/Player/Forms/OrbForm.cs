using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A slow orb: it leaves the hand, drifts after the nearest enemy with two splinters circling it, and bursts when
    /// its time is up. A blow of the weapon that reaches it bursts it at once, and larger.
    /// </summary>
    public class OrbForm : RunningForm
    {
        public override string Id => SpellForms.Orb;

        private const float Size = 0.55f;
        private const float StruckGain = 1.5f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin + cast.Forward * 1.1f;
            var solids = SpellWorks.Solids(run);
            solids[0] = FormFx.Hold(FormMesh.Orb, cast.Colour, 0.05f);
            solids[1] = FormFx.Hold(FormMesh.Shard, cast.Colour, 0.03f);
            solids[2] = FormFx.Hold(FormMesh.Shard, cast.Colour, 0.03f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 1.6f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 2.4f));
            FormKit.Gather(cast, run.A, 1.6f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            Vector3 gap = run.A - origin;
            gap.y = 0f;
            if (gap.magnitude > step.Reach + 0.8f) return;
            if (gap.magnitude > 1f && Vector3.Angle(forward, gap) > step.ArcDegrees * 0.5f + 20f) return;
            run.Flag = true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float seconds = Mathf.Max(0.5f, cast.Spec.Seconds);
            if (run.Flag || run.Age >= seconds)
            {
                bool struck = run.Flag;
                float radius = cast.Spec.Radius * (struck ? StruckGain : 1f);
                var blow = cast.Blow(cast.Damage * (struck ? StruckGain : 1f), cast.Stagger * (struck ? StruckGain : 1f));
                blow.Heavy = struck;
                cast.Combat.StrikeArea(run.A, radius, blow);
                FormKit.Impact(cast, run.A, radius * 0.85f, struck ? 1.7f : 1.1f);
                // Splinters of it thrown out flat in every direction.
                int thrown = struck ? 12 : 8;
                for (int i = 0; i < thrown; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, thrown, 1f, run.Age * 40f);
                    Vector3 from = run.A + Vector3.up * 1.2f;
                    FormFx.Fly(FormMesh.Shard, from, from + outward * radius * 1.15f, Quaternion.LookRotation(outward), Vector3.one * (struck ? 1.6f : 1.2f), cast.Colour, 0.22f);
                }
                return false;
            }

            run.T1 -= deltaTime;
            if (run.T1 <= 0f)
            {
                run.T1 = 0.25f;
                run.Victim = cast.Combat.EnemyNear(run.A, 16f, null);
            }
            if (FormKit.Alive(run.Victim))
            {
                Vector3 gap = run.Victim.transform.position - run.A;
                gap.y = 0f;
                if (gap.magnitude > 0.9f) run.A += gap.normalized * (cast.Spec.Speed * deltaTime);
            }
            // The nearer its end, the faster it beats.
            float left = 1f - run.Age / seconds;
            float beat = Mathf.Repeat(run.Age * Mathf.Lerp(9f, 1.6f, left), 1f);
            float formed = Mathf.Clamp01(run.Age / 0.15f);
            Vector3 heart = run.A + Vector3.up * (1.25f + 0.12f * Mathf.Sin(run.Age * 3f));
            FormFx.Place(solids[0], heart, Quaternion.Euler(run.Age * 70f, run.Age * 110f, 0f), Vector3.one * (Size * formed * (1f + 0.14f * (1f - beat))));
            FormFx.Heat(solids[0], beat < 0.16f && left < 0.6f ? 1f : 0f);
            for (int i = 0; i < 2; i++)
            {
                Vector3 round = Quaternion.Euler(0f, run.Age * 300f + i * 180f, i == 0 ? 25f : -25f) * Vector3.forward;
                FormFx.Place(solids[1 + i], heart + round * 0.95f, Quaternion.LookRotation(Vector3.Cross(Vector3.up, round)), Vector3.one * (0.75f * formed));
            }
            float across = cast.Spec.Radius * 2f;
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(across, 1f, across));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 90f, 0f), new Vector3(1.9f, 1f, 1.9f));
            return true;
        }
    }
}
