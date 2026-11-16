using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Blizzard's crown, Diamond Dust. Each shatter leaves shards turning round the player, twelve at most; the next
    /// dodge throws them all down its path. One run keeps them: a shatter while they hang adds to it.
    /// Reads: Count (shards for each shatter), Damage (each shard), Stagger, Reach, Stacks.
    /// </summary>
    public class DustForm : RunningForm
    {
        public override string Id => SpellForms.Dust;

        public const int Most = 12;
        private const float KeepSeconds = 40f;

        /// <summary>How many shards hang after a shatter adds its own to those already there.</summary>
        public static int After(int hanging, int added) => Mathf.Min(Most, Mathf.Max(0, hanging) + Mathf.Max(0, added));

        public override int Cast(in SpellCast cast)
        {
            ref var hanging = ref SpellWorks.Find(this, out bool found);
            if (found)
            {
                hanging.Count = After(hanging.Count, cast.Spec.Count);
                hanging.T1 = hanging.Age;
                return 0;
            }
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = After(0, cast.Spec.Count);
            run.Done = cast.Upgrades != null ? cast.Upgrades.DodgeCount : 0;
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 here = cast.Player.transform.position;
            for (int i = 0; i < run.Count; i++)
            {
                if (solids[i] < 0) solids[i] = FormFx.Hold(FormMesh.Shard, cast.Colour, 0.05f);
                Vector3 outward = FormRules.OnRing(i, run.Count, 1.35f, run.Age * 150f);
                FormFx.Place(solids[i], here + outward + Vector3.up * (1.2f + 0.25f * Mathf.Sin(run.Age * 4f + i)), FormKit.Yaw(Vector3.Cross(Vector3.up, outward)) * Quaternion.Euler(-25f, 0f, 0f), new Vector3(0.9f, 0.9f, 1.3f));
            }
            if (cast.Upgrades == null || cast.Upgrades.DodgeCount == run.Done) return run.Age < run.T1 + KeepSeconds;
            // Thrown down the dodge's path, in volleys of four.
            Vector3 way = cast.Upgrades.LastDodgeDirection;
            Vector3 from = cast.Upgrades.LastDodgeFrom;
            float reach = cast.Reach;
            Color light = FormKit.Lit(cast.Colour, 3.4f);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 aside = Vector3.Cross(Vector3.up, way) * FormRules.InRow(i, run.Count, 0.22f);
                FormFx.Release(solids[i]);
                solids[i] = -1;
                FormFx.Fly(FormMesh.Shard, from + aside + Vector3.up * 1.1f, from + aside * 1.6f + way * reach + Vector3.up * 0.6f, FormKit.Yaw(way), new Vector3(1.2f, 1.2f, 2.6f), cast.Colour, 0.22f + 0.012f * i);
            }
            int volleys = Mathf.CeilToInt(run.Count / 4f);
            for (int volley = 0; volley < volleys; volley++)
            {
                int shards = Mathf.Min(4, run.Count - volley * 4);
                cast.Combat.StrikeLane(from, way, reach, 1.1f, cast.Blow(cast.Damage * shards, cast.Stagger * shards));
            }
            FormFx.Run(FormMark.Streak, from, way, reach, 2.4f, light, 0.3f, 0.3f);
            FormFx.Run(FormMark.Lane, from, way, reach, 1.8f, FormKit.Lit(cast.Colour, 2f), 0.5f);
            FormKit.Impact(cast, from + way * reach, 1.6f, 0.9f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.3f, 0.2f);
            return false;
        }
    }
}
