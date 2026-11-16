using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Glacier's crown, Weight of Ice. Each blow guarded lays a plate of ice on the player, five at most (a perfect
    /// guard lays two); each plate turns a little of every wound. The next heavy swing spends them all in one edge of
    /// ice down the line of the swing, longer and harder for every plate.
    /// Reads: Count (plates this guard lays), Damage, Stagger and Reach (for each plate spent), Stacks, Radius (half width of the edge).
    /// </summary>
    public class PlatesForm : RunningForm
    {
        public override string Id => SpellForms.Plates;

        public const int Most = 5;
        /// <summary>The share of every wound one plate turns.</summary>
        public const float TurnedEach = 0.06f;
        private const float KeepSeconds = 30f;

        public static int After(int worn, int laid) => Mathf.Min(Most, Mathf.Max(0, worn) + Mathf.Max(0, laid));

        public override int Cast(in SpellCast cast)
        {
            ref var worn = ref SpellWorks.Find(this, out bool found);
            if (found)
            {
                if (worn.Phase == 0)
                {
                    worn.Count = After(worn.Count, cast.Spec.Count);
                    worn.T1 = worn.Age;
                }
                return 0;
            }
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = After(0, cast.Spec.Count);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            if (run.Phase != 0 || step == null || !step.Heavy) return;
            run.Phase = 1;
            run.A = origin;
            run.B = forward;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                for (int i = 0; i < run.Count; i++)
                {
                    if (solids[i] < 0)
                    {
                        solids[i] = FormFx.Hold(FormMesh.Slab, cast.Colour, 0.05f);
                        FormFx.Lay(FormMark.Ring, here, 2.4f, FormKit.Lit(cast.Colour, 3f), 0.25f, 1.2f);
                    }
                    Vector3 outward = FormRules.OnRing(i, Most, 0.95f, run.Age * 40f);
                    FormFx.Place(solids[i], here + outward + Vector3.up * 0.55f, FormKit.Yaw(outward), new Vector3(0.75f, 0.9f, 0.6f));
                }
                if (cast.Upgrades != null) cast.Upgrades.Stats.AddBoon(PlayerStat.DamageTaken, -TurnedEach * run.Count, 0.25f);
                return run.Age < run.T1 + KeepSeconds;
            }
            // Spent: one great edge of ice down the line of the swing.
            int plates = run.Count;
            float reach = 2.5f + cast.Spec.Reach * plates;
            Vector3 way = run.B;
            for (int i = 0; i < plates; i++)
            {
                FormFx.Release(solids[i]);
                solids[i] = -1;
            }
            var blow = cast.Blow(cast.Damage * plates, cast.Stagger * plates, Mathf.Max(1, cast.Spec.Stacks) * plates);
            blow.Heavy = true;
            cast.Combat.StrikeLane(run.A, way, reach, cast.Spec.Radius, blow);
            int fangs = FormRules.Pieces(reach, 0.9f);
            for (int i = 0; i < fangs; i++)
            {
                float along = (i + 1f) / fangs;
                FormFx.Erupt(FormMesh.Spike, run.A + way * (reach * along) + Vector3.Cross(Vector3.up, way) * (0.35f * (i % 2 == 0 ? 1f : -1f)), FormKit.Yaw(way) * Quaternion.Euler(30f, 0f, 0f),
                    new Vector3(1f + 0.5f * along, 1.6f + 2.2f * along, 1f + 0.5f * along), cast.Colour, 0.04f + 0.1f * along, 0.7f, 0.35f);
            }
            Color light = FormKit.Lit(cast.Colour, 3.6f);
            FormFx.Run(FormMark.Streak, run.A, way, reach * 1.1f, 2.6f, light, 0.3f, 0.3f);
            FormFx.Run(FormMark.Beam, run.A, way, reach, 1.2f, light, 0.18f, 0.2f);
            FormKit.Impact(cast, run.A + way * reach, 1.8f, 0.8f + 0.25f * plates);
            SigKit.Moment(cast, 0.03f * plates, 0.15f * plates, 0f, 1f, 0.08f * plates);
            return false;
        }
    }
}
