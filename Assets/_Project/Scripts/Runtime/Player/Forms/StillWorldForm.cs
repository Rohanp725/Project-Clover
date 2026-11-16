using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Ice's gift, The World Holds Still. While the surge lasts a shell of ice stands round the player; the surge
    /// button pressed again breaks it outward as a ring that freezes everything it crosses where it stands, and the
    /// world is slowed while it goes. The shatter that runs on from one frozen enemy to the next is
    /// <see cref="ShatterChainForm"/>. Reads: Damage, Stagger, Radius.
    /// </summary>
    public class StillWorldForm : RunningForm
    {
        public override string Id => SpellForms.StillWorld;

        private const int Facets = 6;
        private const float BreakSeconds = 0.5f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = cast.Upgrades != null ? cast.Upgrades.SurgeAgain : 0;
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < Facets; i++) solids[i] = FormFx.Hold(FormMesh.Spike, cast.Colour, 0.05f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2.2f));
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float reach = cast.Spec.Radius;
            if (run.Phase == 0)
            {
                Vector3 here = cast.Player.transform.position;
                for (int i = 0; i < Facets; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, Facets, 1.05f, run.Age * 25f);
                    FormFx.Place(solids[i], here + outward, FormKit.Yaw(outward) * Quaternion.Euler(-12f, 0f, 0f), new Vector3(0.45f, 1.5f + 0.2f * (i % 2), 0.45f));
                }
                FormFx.PlaceMark(marks[0], here, Quaternion.identity, new Vector3(2.6f, 1f, 2.6f));
                if (cast.Upgrades != null) cast.Upgrades.ListenForSurge();
                bool again = cast.Upgrades != null && cast.Upgrades.SurgeAgain != run.Count;
                if (!again) return SigKit.SurgeOn(cast);
                // Broken outward.
                run.Phase = 1;
                run.T1 = run.Age;
                run.A = here;
                for (int i = 0; i < Facets; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, Facets, 1f, run.Age * 25f);
                    FormFx.Release(solids[i]);
                    solids[i] = -1;
                    FormFx.Fly(FormMesh.Shard, here + outward + Vector3.up, here + outward * reach + Vector3.up * 0.4f, FormKit.Yaw(outward), new Vector3(1.2f, 1.2f, 2.2f), cast.Colour, BreakSeconds);
                }
                FormFx.ReleaseMark(marks[0]);
                marks[0] = -1;
                SpellWorks.Once(run).Clear();
                FormFx.Lay(FormMark.Ring, here, 1.5f, FormKit.Lit(cast.Colour, 4f), BreakSeconds + 0.1f, reach * 2f);
                FormFx.Lay(FormMark.Ring, here, 0.5f, FormKit.Lit(cast.Colour, 2.4f), BreakSeconds + 0.3f, reach * 1.7f);
                FormFx.Lay(FormMark.Star, here, 2f, FormKit.Lit(cast.Colour, 3.4f), 0.25f, reach * 1.2f);
                FormFx.Lay(FormMark.Disc, here, reach * 2f, FormKit.Lit(cast.Colour, 1.3f), 1.6f);
                WorldClock.SlowEnemies(1.5f, 0.2f);
                SigKit.Moment(cast, 0.12f, 0.6f, 0f, 1f, 0.45f);
                if (cast.Defence != null) cast.Defence.Callout(cast.Name);
                return true;
            }
            float t = Mathf.Clamp01((run.Age - run.T1) / BreakSeconds);
            float radius = Mathf.Lerp(0.8f, reach, t);
            // Each thing is frozen once, as the ring reaches it: chilled as deep as chill goes.
            cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Damage, cast.Stagger, Afflictions.Depth(AfflictionKind.Chill)), SpellWorks.Once(run));
            run.T0 -= deltaTime;
            if (run.T0 <= 0f)
            {
                run.T0 = 0.04f;
                // Ice stands up where the ring has passed.
                for (int k = 0; k < 5; k++)
                {
                    Vector3 outward = FormRules.OnRing(k, 5, radius, run.Done * 29f);
                    FormFx.Erupt(FormMesh.Spike, run.A + outward, FormKit.Yaw(outward) * Quaternion.Euler(22f, 0f, 0f),
                        new Vector3(0.7f, 1.3f + 0.5f * ((k + run.Done) % 3), 0.7f), cast.Colour, 0.05f, 1.3f, 0.4f);
                }
                run.Done++;
            }
            if (t < 1f) return true;
            FormFx.Lay(FormMark.Crack, run.A, reach * 2f, FormKit.Lit(cast.Colour, 2f), 2.4f, -1f, 0f, 12f);
            return false;
        }
    }
}
