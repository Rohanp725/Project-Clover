using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Spikes along the ground: marks run out from the player in rows, and a moment behind each mark a spike erupts,
    /// taller the further out it is. Each enemy on the line is struck once.
    /// </summary>
    public class SpikesForm : RunningForm
    {
        public override string Id => SpellForms.Spikes;

        private const float MarkLead = 0.17f;
        private const float First = 1.3f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.B = cast.Forward;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 5);
            run.T2 = Mathf.Max(0.6f, cast.Spec.Radius * 1.5f);
            // How long each mark waits for its break: just behind it, or the whole crack glowing first (Fissure).
            run.T0 = FormRules.BreakLead(cast.Reach, cast.Spec.Speed, cast.Spec.Glow, MarkLead);
            float width = cast.Spec.Radius * 1.7f * (run.Count - 1) + cast.Spec.Radius * 2f;
            FormFx.Run(FormMark.Lane, run.A + run.B * 0.8f, run.B, cast.Reach - 0.6f, width, FormKit.Lit(cast.Colour, 1.5f), cast.Reach / Mathf.Max(2f, cast.Spec.Speed) + run.T0);
            SpellWorks.Gesture(cast, FormKit.Chop, 0.36f, 0.25f, 0.95f, true);
            return 0;
        }

        // Where the spike of a row stands at a step along the line; every other row is set half a step forward.
        private static Vector3 Spot(in SpellRun run, int step, int row, out float along)
        {
            along = First + step * run.T2 + (row % 2 == 1 ? run.T2 * 0.5f : 0f);
            Vector3 right = Vector3.Cross(Vector3.up, run.B);
            return run.A + run.B * along + right * FormRules.InRow(row, run.Count, run.Cast.Spec.Radius * 1.7f);
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            float speed = Mathf.Max(2f, cast.Spec.Speed);
            float radius = cast.Spec.Radius;
            float reach = cast.Reach;
            float marked = speed * run.Age, erupted = speed * (run.Age - run.T0);
            while (First + run.Phase * run.T2 <= Mathf.Min(marked, reach))
            {
                for (int row = 0; row < run.Count; row++)
                {
                    Vector3 spot = Spot(run, run.Phase, row, out float along);
                    if (along <= reach + 0.3f) FormFx.Lay(FormMark.Ring, spot, radius * 3f, FormKit.Lit(cast.Colour, 2.6f), run.T0 + 0.03f, radius * 1.5f);
                }
                run.Phase++;
            }
            while (First + run.Done * run.T2 <= Mathf.Min(erupted, reach))
            {
                bool last = First + (run.Done + 1) * run.T2 > reach;
                for (int row = 0; row < run.Count; row++)
                {
                    Vector3 spot = Spot(run, run.Done, row, out float along);
                    if (along > reach + 0.3f) continue;
                    // They lean the way they run and stand taller the further out: the line ends on its biggest.
                    float tall = 1.3f + 1.5f * (along / reach) + 0.3f * ((run.Done + row) % 2) + (last ? 0.7f : 0f);
                    FormFx.Erupt(FormMesh.Spike, spot, FormKit.Yaw(run.B) * Quaternion.Euler(16f, (run.Done * 67 + row * 131) % 360, 0f),
                        new Vector3(radius * 1.35f, tall, radius * 1.35f), cast.Colour, 0.05f, 0.45f, 0.25f, 0.04f);
                    cast.Combat.StrikeArea(spot, radius, cast.Blow(cast.Damage, cast.Stagger), SpellWorks.Once(run));
                    ElementLook.Puff(cast.Element, spot + Vector3.up * 0.3f, Vector3.up, 0.45f);
                }
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, last ? 0.3f : 0.1f, last ? 0.25f : 0.1f);
                if (last) FormKit.Impact(cast, run.A + run.B * Mathf.Min(reach, First + run.Done * run.T2), radius * 1.4f, 0.9f);
                run.Done++;
            }
            return First + run.Done * run.T2 <= reach;
        }
    }
}
