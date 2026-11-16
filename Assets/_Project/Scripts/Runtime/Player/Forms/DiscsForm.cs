using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Toothed discs that circle the player at arm's length for a while, each biting what it passes at intervals.
    /// When the time is up they fly off outward.
    /// </summary>
    public class DiscsForm : RunningForm
    {
        public override string Id => SpellForms.Discs;

        private const int Most = 8;
        private const float Size = 0.72f;
        private const float TurnDegrees = 230f;

        public override int Cast(in SpellCast cast)
        {
            // One halo at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, Most);
            run.T1 = 0.25f;
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Disc, cast.Colour, 0.04f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 1.3f));
            FormKit.Gather(cast, cast.Origin, 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.28f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 centre = cast.Player.transform.position;
            float radius = cast.Spec.Radius;
            float formed = Mathf.Clamp01(run.Age / 0.2f);
            bool bite = run.Age >= run.T1;
            if (bite)
            {
                run.T1 += Mathf.Max(0.1f, cast.Spec.Interval);
                SpellWorks.Once(run).Clear();
            }
            bool over = run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 outward = FormRules.OnRing(i, run.Count, 1f, run.Age * TurnDegrees);
                Vector3 at = centre + outward * (radius * formed) + Vector3.up * (1f + 0.1f * Mathf.Sin(run.Age * 6f + i * 2f));
                if (over)
                {
                    FormFx.Release(solids[i]);
                    solids[i] = -1;
                    FormFx.Fly(FormMesh.Disc, at, at + outward * 4.5f, Quaternion.identity, Vector3.one * Size, cast.Colour, 0.28f, 0.04f);
                    continue;
                }
                // Tilted into its turn, spinning fast on its own hub.
                FormFx.Place(solids[i], at, Quaternion.LookRotation(outward) * Quaternion.Euler(0f, 0f, 14f) * Quaternion.Euler(0f, run.Age * 1100f, 0f), Vector3.one * (Size * formed));
                FormFx.Heat(solids[i], formed < 1f ? 1f : 0f);
                if (!bite) continue;
                int hits = cast.Combat.StrikeArea(at, 1f, cast.Blow(cast.Damage, cast.Stagger), SpellWorks.Once(run));
                if (hits <= 0) continue;
                FormFx.Heat(solids[i], 1f);
                FormFx.Cut(at, Vector3.Cross(Vector3.up, outward), 2.4f, 0.3f, FormKit.Lit(cast.Colour, 3.2f), 0.1f);
                ElementLook.Sparks(cast.Element, at, Vector3.Cross(Vector3.up, outward), 0.5f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.08f, 0.1f);
            }
            float across = radius * 2f * formed;
            FormFx.PlaceMark(SpellWorks.Marks(run)[0], centre, Quaternion.identity, new Vector3(across, 1f, across));
            return !over;
        }
    }
}
