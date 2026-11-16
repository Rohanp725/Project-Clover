using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Blight's crown, The Reaping. A blade as long as the field sweeps once round the finished enemy. Where it has
    /// passed, all the poison in every enemy near lands at once, and half of what it dealt mends the player.
    /// Reads: Radius, Damage (added for each stack of poison), Stagger.
    /// </summary>
    public class ReapingForm : RunningForm
    {
        public override string Id => SpellForms.Reaping;

        private const float SweepSeconds = 0.42f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = FormKit.AimPoint(cast);
            run.B = cast.Forward;
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.1f);
            SpellWorks.Solids(run)[1] = FormFx.Hold(FormMesh.Pillar, new Color(0.05f, 0.05f, 0.06f), 0.05f);
            float radius = cast.Spec.Radius;
            FormFx.Lay(FormMark.Disc, run.A, radius * 2f, FormKit.Lit(cast.Colour, 1.5f), SweepSeconds + 0.1f);
            FormFx.Lay(FormMark.Ring, run.A, radius * 2.6f, FormKit.Lit(cast.Colour, 3f), SweepSeconds, radius * 2f);
            Clover.Core.ScreenFlash.Flash(new Color(0f, 0f, 0f, 0.5f), 0.2f);
            if (cast.Upgrades != null) cast.Upgrades.Slow(SweepSeconds + 0.4f, 0.35f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            float radius = cast.Spec.Radius;
            float t = Mathf.Clamp01(run.Age / SweepSeconds);
            // Once round, fast in the middle of the sweep: a scythe's haft from the middle and its blade across the end.
            float turned = Mathf.SmoothStep(0f, 1f, t) * 380f - 100f;
            Vector3 arm = Quaternion.Euler(0f, turned, 0f) * run.B;
            Vector3 tip = run.A + arm * radius + Vector3.up * 0.9f;
            FormFx.Place(solids[1], run.A + Vector3.up * 0.9f, FormKit.Yaw(arm) * Quaternion.Euler(90f, 0f, 0f), new Vector3(0.3f, radius, 0.3f));
            FormFx.Place(solids[0], tip, FormKit.Yaw(Vector3.Cross(Vector3.up, arm) + arm * 0.35f), new Vector3(6f, 3f, radius * 0.75f));
            FormFx.Heat(solids[0], t > 0.85f ? 1f : 0.15f);
            run.T0 -= deltaTime;
            if (run.T0 <= 0f)
            {
                run.T0 = 0.02f;
                FormFx.Run(FormMark.Streak, run.A, arm, radius * 1.05f, 1.6f, FormKit.Lit(cast.Colour, 2.6f), 0.22f, 0.2f);
            }
            if (t < 1f) return true;
            int near = SigKit.Gather(run, run.A, radius, SpellWorks.NotesEach);
            var things = SpellWorks.Things(run);
            float total = 0f;
            for (int i = 0; i < near; i++)
            {
                var set = SigKit.Set(things[i]);
                if (set == null || !set.Has(AfflictionKind.Poison)) continue;
                int depth = set.Stacks(AfflictionKind.Poison);
                float reaped = set.SetOff(AfflictionKind.Poison) + cast.Damage * depth;
                Vector3 chest = things[i].transform.position + Vector3.up * 1.1f;
                cast.Combat.StrikeOne(things[i], cast.Blow(reaped, cast.Stagger * depth, 0), run.A);
                FormFx.Cut(chest, Vector3.Cross(Vector3.up, chest - run.A), 5f, 0.8f, FormKit.Lit(cast.Colour, 4f), 0.18f, 0.05f);
                FormFx.Run(FormMark.Streak, SigKit.Foot(things[i], run.A.y), cast.Player.transform.position - things[i].transform.position, (cast.Player.transform.position - things[i].transform.position).magnitude, 0.5f, FormKit.Lit(cast.Colour, 2.4f), 0.45f, 0.1f);
                total += reaped;
            }
            LastReaped = total;
            if (cast.Defence != null && total > 0f) cast.Defence.Heal(total * 0.5f, true);
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 4f), 0.4f, radius * 2.2f);
            FormFx.Lay(FormMark.Scorch, run.A, radius * 2f, FormKit.Stain(cast.Colour, 0.7f), 4f);
            FormFx.Lay(FormMark.Cross, run.A, radius, FormKit.Lit(cast.Colour, 2.6f), 1f, -1f, 0f, 20f);
            SigKit.Moment(cast, 0.18f, 0.7f, 0f, 1f, 0.4f);
            return false;
        }

        /// <summary>What the last sweep dealt in all, for probes.</summary>
        public static float LastReaped { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LastReaped = 0f;
    }
}
