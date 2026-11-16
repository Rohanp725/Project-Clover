using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Small helpers of the element: they rise beside the player, each runs at the enemy nearest it and jabs it at
    /// intervals, and when their time is up they go out. With no enemy near they keep by the player.
    /// </summary>
    public class MinionsForm : RunningForm
    {
        public override string Id => SpellForms.Minions;

        private const int Most = 6;
        private const float RunSpeed = 7.5f;
        private const float Height = 1.15f;
        private const float JabSeconds = 0.14f;

        public override int Cast(in SpellCast cast)
        {
            // One band of them at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, Most);
            var solids = SpellWorks.Solids(run);
            var points = SpellWorks.Points(run);
            var times = SpellWorks.Times(run);
            Vector3 right = Vector3.Cross(Vector3.up, cast.Forward);
            for (int i = 0; i < run.Count; i++)
            {
                solids[i] = FormFx.Hold(FormMesh.Minion, cast.Colour, 0.04f);
                points[i] = cast.Origin + right * FormRules.InRow(i, run.Count, 1.3f) + cast.Forward * (0.9f - 0.3f * Mathf.Abs(FormRules.InRow(i, run.Count, 1f)));
                times[i] = 0.35f + 0.12f * i;
                times[Most + i] = 1f;
                FormFx.Lay(FormMark.Sigil, points[i], 1.7f, FormKit.Lit(cast.Colour), 0.4f, 1.1f, 240f);
            }
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var points = SpellWorks.Points(run);
            var times = SpellWorks.Times(run);
            var things = SpellWorks.Things(run);
            Transform body = cast.Player.transform;
            if (run.Age >= Mathf.Max(1f, cast.Spec.Seconds))
            {
                for (int i = 0; i < run.Count; i++)
                {
                    ElementLook.Puff(cast.Element, points[i] + Vector3.up * 0.6f, Vector3.up, 0.6f);
                    FormFx.Release(solids[i], 0.2f);
                    solids[i] = -1;
                }
                return false;
            }
            // Whom each goes for is looked at a few times a second, not every frame.
            run.T1 -= deltaTime;
            bool look = run.T1 <= 0f;
            if (look) run.T1 = 0.3f;
            // Each takes an enemy no other has, while there are enough to go round.
            var taken = SpellWorks.Once(run);
            if (look) taken.Clear();
            float rise = Mathf.Clamp01(run.Age / 0.25f);
            for (int i = 0; i < run.Count; i++)
            {
                if (look)
                {
                    things[i] = cast.Combat.EnemyNear(points[i], 16f, taken) ?? cast.Combat.EnemyNear(points[i], 16f, null);
                    if (things[i] is Clover.Gameplay.IDamageable mine) taken.Add(mine);
                }
                Component victim = things[i];
                Vector3 facing = body.forward;
                float hop = 0f, jab = 0f;
                // The second half of each minion's notes holds how long ago it jabbed.
                times[Most + i] += deltaTime;
                if (rise >= 1f && FormKit.Alive(victim))
                {
                    Vector3 gap = victim.transform.position - points[i];
                    gap.y = 0f;
                    float distance = gap.magnitude;
                    if (distance > 0.05f) facing = gap / distance;
                    if (distance > 1.35f)
                    {
                        points[i] += facing * Mathf.Min(RunSpeed * deltaTime, distance - 1.3f);
                        hop = Mathf.Abs(Mathf.Sin(run.Age * 15f + i * 1.7f)) * 0.3f;
                    }
                    else
                    {
                        times[i] -= deltaTime;
                        if (times[i] <= 0f)
                        {
                            times[i] = Mathf.Max(0.15f, cast.Spec.Interval);
                            times[Most + i] = 0f;
                            cast.Combat.StrikeOne(victim, cast.Blow(cast.Damage, cast.Stagger), points[i]);
                            Vector3 chest = victim.transform.position + Vector3.up * 0.9f;
                            FormFx.Cut(chest, Quaternion.Euler(0f, 60f + i * 40f, 0f) * facing, 2.2f, 0.32f, FormKit.Lit(cast.Colour, 3.2f), 0.12f);
                        }
                    }
                    if (times[Most + i] < JabSeconds) jab = Mathf.Sin(times[Most + i] / JabSeconds * Mathf.PI);
                }
                else if (rise >= 1f)
                {
                    // No one to go for: back to the player's side.
                    Vector3 home = body.position + body.right * FormRules.InRow(i, run.Count, 1.3f) - body.forward * 0.8f;
                    Vector3 gap = home - points[i];
                    gap.y = 0f;
                    if (gap.magnitude > 0.3f)
                    {
                        points[i] += gap.normalized * Mathf.Min(RunSpeed * deltaTime, gap.magnitude);
                        facing = gap.normalized;
                        hop = Mathf.Abs(Mathf.Sin(run.Age * 15f + i * 1.7f)) * 0.3f;
                    }
                }
                points[i].y = body.position.y;
                Vector3 shown = points[i] + Vector3.up * hop + facing * (jab * 0.4f);
                FormFx.Place(solids[i], shown, Quaternion.LookRotation(facing, Vector3.up) * Quaternion.Euler(jab * 22f, 0f, 0f),
                    new Vector3(Height, Height * rise * (1f + 0.12f * Mathf.Sin(run.Age * 15f + i)), Height * (1f + jab * 0.35f)));
                FormFx.Heat(solids[i], rise < 1f || jab > 0.6f ? 1f : 0f);
            }
            return true;
        }
    }
}
