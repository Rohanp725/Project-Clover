using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Water's gift, The Sea Rises. While the surge lasts the surge button pressed again spends a flask and floods the
    /// ground: for some seconds everything in the flood is Soaked and every blow of the weapon runs out from the one
    /// struck as a wave. Then the sea draws back to the player, dragging everything with it, and breaks where they face.
    /// Reads: Radius (of the flood), Seconds, Damage and Stagger (the breaking wave), Reach, Arc, Count (percent of a blow each wave carries).
    /// </summary>
    public class SeaForm : RunningForm
    {
        public override string Id => SpellForms.Sea;

        private const int Waves = 8;
        private const float DrawSeconds = 0.55f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = cast.Upgrades != null ? cast.Upgrades.SurgeAgain : 0;
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase != 1 || target == null || run.Done >= Waves) return;
            SpellWorks.Points(run)[run.Done] = target.transform.position;
            SpellWorks.Times(run)[run.Done] = damage;
            run.Done++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                if (!SigKit.SurgeOn(cast)) return false;
                if (cast.Upgrades == null) return false;
                cast.Upgrades.ListenForSurge();
                if (cast.Upgrades.SurgeAgain == run.Count) return true;
                run.Count = cast.Upgrades.SurgeAgain;
                if (cast.Defence == null || !cast.Defence.Flasks.TryThrow())
                {
                    if (cast.Defence != null) cast.Defence.Callout("No flask");
                    return true;
                }
                run.Phase = 1;
                run.T1 = run.Age;
                run.A = here;
                marks[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.5f));
                marks[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 1.2f));
                FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 3.6f), 0.6f, radius * 2f);
                cast.Defence.Callout(cast.Name);
                SigKit.Moment(cast, 0.06f, 0.4f, 0f, 1f, 0.3f);
                return true;
            }
            if (run.Phase == 1)
            {
                float since = run.Age - run.T1;
                FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
                FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, since * 14f, 0f), new Vector3(radius * 1.7f, 1f, radius * 1.7f));
                run.T0 -= deltaTime;
                if (run.T0 <= 0f)
                {
                    // The swell: a ring running out to the flood's edge, and the wet kept on all it covers.
                    run.T0 = 0.7f;
                    FormFx.Lay(FormMark.Ring, run.A, 2f, FormKit.Lit(cast.Colour, 2f), 0.9f, radius * 2f);
                    cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Soaked, 1);
                }
                var points = SpellWorks.Points(run);
                var blows = SpellWorks.Times(run);
                for (int i = 0; i < run.Done; i++)
                {
                    // A blow runs out across the water from the one it struck; that one is not struck twice.
                    cast.Combat.StrikeArea(points[i], 3.6f, cast.Blow(FormRules.Echo(blows[i], cast.Spec.Count), 8f), null, 0.7f);
                    FormFx.Lay(FormMark.Ring, points[i], 1.2f, FormKit.Lit(cast.Colour, 3.6f), 0.4f, 7.2f);
                    FormFx.Lay(FormMark.Ring, points[i], 0.6f, FormKit.Lit(cast.Colour, 2.2f), 0.55f, 6f);
                    ElementLook.Puff(cast.Element, points[i] + Vector3.up * 0.4f, Vector3.up, 0.8f);
                }
                run.Done = 0;
                if (since < cast.Spec.Seconds) return true;
                // The sea draws back.
                run.Phase = 2;
                run.T1 = run.Age;
                FormFx.ReleaseMark(marks[0], 0.3f);
                FormFx.ReleaseMark(marks[1], 0.3f);
                marks[0] = marks[1] = -1;
                cast.Combat.Displace(here, radius, 4f, true);
                for (int i = 0; i < 3; i++)
                    FormFx.Lay(FormMark.Ring, here, radius * (2f - 0.5f * i), FormKit.Lit(cast.Colour, 3f), DrawSeconds, 1.5f);
                SpellWorks.Gesture(cast, FormKit.Brace, DrawSeconds, 0f, 0.9f);
                return true;
            }
            if (run.Age < run.T1 + DrawSeconds) return true;
            // And breaks where the player faces.
            Vector3 forward = cast.Player.transform.forward;
            var blow = cast.Blow(cast.Damage, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, blow);
            for (int row = 1; row <= 4; row++)
            {
                float out_ = cast.Reach * row / 4.4f;
                int across = 2 + row;
                for (int i = 0; i < across; i++)
                {
                    Vector3 way = Quaternion.Euler(0f, FormRules.InFan(i, across, cast.Spec.Arc * 0.8f), 0f) * forward;
                    FormFx.Erupt(FormMesh.Spike, here + way * out_, FormKit.Yaw(way) * Quaternion.Euler(40f, 0f, 0f),
                        new Vector3(1.1f, 1.6f + 0.6f * row, 1.1f), cast.Colour, 0.05f + 0.03f * row, 0.25f, 0.3f);
                }
            }
            for (int i = 0; i < 5; i++)
                FormFx.Run(FormMark.Beam, here, Quaternion.Euler(0f, FormRules.InFan(i, 5, cast.Spec.Arc * 0.8f), 0f) * forward, cast.Reach, 1.6f, FormKit.Lit(cast.Colour, 3f), 0.3f, 0.3f);
            FormKit.Impact(cast, here + forward * (cast.Reach * 0.45f), 3f, 2f);
            SigKit.Moment(cast, 0.18f, 0.8f, 0.4f, 0.4f, 0.4f);
            SpellWorks.Gesture(cast, FormKit.Sweep, 0.3f, 0.3f, 1f, true);
            return false;
        }
    }
}
