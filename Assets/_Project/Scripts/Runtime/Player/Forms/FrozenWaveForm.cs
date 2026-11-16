using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Glacier's The Wave Stands Still. While the spell button stays held a wave rolls out ahead of the player,
    /// pushing and soaking what it meets. When the button is let go, or the wave has gone as far as it can, it turns
    /// to ice where it stands: a wall nothing walks through, and everything in it Frozen. A heavy swing beside the
    /// wall shatters all of it at once; left alone it stands its time and sinks.
    /// Reads: Reach (how far it can roll), Speed, Arc (width in metres), LingerDamage (the wave's own), Seconds (the wall stands), Damage and Stagger (the shattering).
    /// </summary>
    public class FrozenWaveForm : RunningForm
    {
        public override string Id => SpellForms.FrozenWave;

        private const float Least = 2.5f;
        private const float Depth = 3.2f;
        private const int Crests = 5;

        /// <summary>How far the wave has rolled after some seconds: from a pace ahead, at its speed, never past its reach.</summary>
        public static float Rolled(float seconds, float speed, float reach) => Mathf.Min(reach, 1f + Mathf.Max(0f, seconds) * speed);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.B = cast.Forward;
            var solids = SpellWorks.Solids(run);
            Color water = Elements.Colour(Elements.Water);
            for (int i = 0; i < Crests; i++) solids[i] = FormFx.Hold(FormMesh.Slab, water, 0.05f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Lane, FormKit.Lit(water, 2.2f));
            SpellWorks.Gesture(cast, FormKit.Sweep, 0.35f, 0f, 0.8f, true);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            // A heavy swing from beside the wall, on either side of it.
            if (run.Phase != 1 || step == null || !step.Heavy) return;
            Vector3 from = origin - run.C;
            from.y = 0f;
            float half = run.Cast.Spec.Arc * 0.5f + 1.5f;
            if (Mathf.Abs(Vector3.Dot(from, run.B)) < Depth + 2.5f && Mathf.Abs(Vector3.Dot(from, Vector3.Cross(Vector3.up, run.B))) < half) run.Flag = true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float width = Mathf.Max(2f, cast.Spec.Arc);
            Vector3 across = Vector3.Cross(Vector3.up, run.B);
            Color water = Elements.Colour(Elements.Water);
            Color ice = Elements.Colour(Elements.Ice);
            if (run.Phase == 0)
            {
                float far = Rolled(run.Age, cast.Spec.Speed, cast.Reach);
                Vector3 front = run.A + run.B * far;
                for (int i = 0; i < Crests; i++)
                {
                    // The crest: slabs leaning forward, each rising and falling a little out of step.
                    float swell = 1.5f + 0.35f * Mathf.Sin(run.Age * 9f + i * 1.3f);
                    FormFx.Place(solids[i], front + across * FormRules.InRow(i, Crests, width / Crests), FormKit.Yaw(run.B) * Quaternion.Euler(24f, 0f, 0f), new Vector3(width / Crests * 1.15f, swell, 1.1f));
                }
                FormFx.PlaceMark(marks[0], run.A, Quaternion.LookRotation(run.B, Vector3.up), new Vector3(width, 1f, far));
                // What it meets is soaked once and carried along.
                var wet = cast.Blow(cast.Spec.LingerDamage, 0f, 0);
                wet.Affliction = AfflictionKind.Soaked;
                wet.Stacks = 1;
                cast.Combat.StrikeLane(run.A, run.B, far, width * 0.5f, wet, SpellWorks.Once(run));
                cast.Combat.Displace(front - run.B * 2.2f, 2.6f, cast.Spec.Speed * deltaTime, false);
                run.T0 -= deltaTime;
                if (run.T0 <= 0f)
                {
                    run.T0 = 0.07f;
                    ElementLook.Puff(cast.Element, front + across * Random.Range(-width * 0.5f, width * 0.5f) + Vector3.up * 0.8f, run.B, 0.7f);
                }
                bool held = cast.Caster != null && cast.Caster.SpellHeld;
                if ((held || far < Least) && far < cast.Reach) return true;
                // It stands still.
                run.Phase = 1;
                run.T1 = run.Age;
                run.C = front;
                for (int i = 0; i < Crests; i++)
                {
                    FormFx.Release(solids[i]);
                    solids[i] = -1;
                }
                FormFx.ReleaseMark(marks[0], 0.2f);
                marks[0] = -1;
                float stand = Mathf.Max(1f, cast.Spec.Seconds);
                int slabs = FormRules.Pieces(width, 0.9f);
                for (int i = 0; i < slabs; i++)
                {
                    Vector3 at = front + across * FormRules.InRow(i, slabs, width / slabs);
                    FormFx.Erupt(FormMesh.Slab, at, FormKit.Yaw(run.B) * Quaternion.Euler(14f, 0f, 0f), new Vector3(width / slabs * 1.2f, 2.3f + 0.4f * (i % 2), 0.8f), ice, 0.05f, stand, 0.4f, 0.06f);
                    // The curl of the crest, caught.
                    FormFx.Erupt(FormMesh.Spike, at + Vector3.up * 1.9f + run.B * 0.3f, FormKit.Yaw(run.B) * Quaternion.Euler(70f, 0f, 0f), new Vector3(0.8f, 1.5f, 0.8f), ice, 0.06f, stand, 0.4f);
                    FormFx.Erupt(FormMesh.Spike, at - run.B * (0.9f + 0.5f * (i % 3)), FormKit.Yaw(-run.B) * Quaternion.Euler(30f, 0f, 0f), new Vector3(0.7f, 1.2f + 0.3f * (i % 2), 0.7f), ice, 0.07f, stand, 0.4f);
                }
                SpellWorks.Barrier(run, front + Vector3.up * 1.2f, FormKit.Yaw(run.B), new Vector3(width, 2.4f, 0.9f));
                SpellWorks.Screen(run, front - across * (width * 0.5f), front + across * (width * 0.5f));
                // Everything in it or touching it is chilled as deep as chill goes.
                var once = SpellWorks.Once(run);
                once.Clear();
                var cold = cast.Blow(0f, 0f, 0);
                cold.Affliction = AfflictionKind.Chill;
                cold.Stacks = Afflictions.Depth(AfflictionKind.Chill);
                run.Count = cast.Combat.StrikeLane(front - run.B * Depth, run.B, Depth + 1.4f, width * 0.5f + 0.4f, cold, once);
                once.Clear();
                FormFx.Run(FormMark.Beam, front - across * (width * 0.5f), across, width, 1.4f, FormKit.Lit(ice, 3.6f), 0.3f, 0.3f);
                FormFx.Lay(FormMark.Disc, front - run.B * (Depth * 0.5f), Depth * 1.6f, FormKit.Lit(ice, 1.4f), stand);
                FormFx.Lay(FormMark.Crack, front, width * 1.2f, FormKit.Lit(ice, 2.2f), stand);
                SigKit.Moment(cast, 0.12f, 0.5f, 0f, 1f, 0.3f);
                return true;
            }
            if (!run.Flag) return run.Age < run.T1 + Mathf.Max(1f, cast.Spec.Seconds);
            // Shattered: the wall and everything in it, each as its own shatter.
            var blow = cast.Blow(cast.Damage, cast.Stagger, 0);
            blow.Heavy = true;
            Shattered = cast.Combat.StrikeLane(run.C - run.B * Depth, run.B, Depth + 1.4f, width * 0.5f + 0.4f, blow);
            int shards = FormRules.Pieces(width, 0.6f);
            for (int i = 0; i < shards; i++)
            {
                Vector3 at = run.C + across * FormRules.InRow(i, shards, width / shards) + Vector3.up * 1.2f;
                Vector3 way = (i % 2 == 0 ? run.B : -run.B) + across * FormRules.InRow(i, shards, 0.25f);
                FormFx.Fly(FormMesh.Shard, at, at + way.normalized * 4.5f + Vector3.down * 0.8f, FormKit.Yaw(way), new Vector3(1.4f, 1.4f, 2.4f), ice, 0.3f + 0.02f * (i % 4));
            }
            FormKit.Impact(cast, run.C, width * 0.5f, 2f);
            FormFx.Run(FormMark.Streak, run.C - across * (width * 0.6f), across, width * 1.2f, 2.2f, FormKit.Lit(ice, 4f), 0.25f, 0.3f);
            SigKit.Moment(cast, 0.2f, 0.9f, 0.35f, 0.4f, 0.45f);
            return false;
        }

        /// <summary>How many the last shattered wall struck, for probes.</summary>
        public static int Shattered { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Shattered = 0;
    }
}
