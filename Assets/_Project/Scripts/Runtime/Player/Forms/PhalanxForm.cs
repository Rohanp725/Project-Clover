using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Blades at the shoulders: they form in a fan behind the player and go where the player goes. Now and then one
    /// lets fly by itself at the nearest enemy in reach; a heavy blow of the weapon sends every one left at whoever
    /// it struck. What has not flown when the time is up goes out.
    /// </summary>
    public class PhalanxForm : RunningForm
    {
        public override string Id => SpellForms.Phalanx;

        private const float Length = 1.7f;
        private const float FireEvery = 0.55f;
        private const float Hovering = -1f;
        private const float Spent = -2f;

        public override int Cast(in SpellCast cast)
        {
            // One fan at a time: a second cast takes the first one's place.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, SpellWorks.SolidsEach);
            run.T1 = 0.6f;
            var solids = SpellWorks.Solids(run);
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            for (int i = 0; i < run.Count; i++)
            {
                solids[i] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.04f);
                times[i] = Hovering;
                points[i] = cast.Origin + Vector3.up * 1.2f;
            }
            FormKit.Gather(cast, cast.Origin, 2.4f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (!step.Heavy || target == null) return;
            run.Flag = true;
            run.Victim = target;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            var things = SpellWorks.Things(run);
            Transform body = cast.Player.transform;
            bool over = run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds);

            if (run.Flag)
            {
                // The heavy blow landed: all that are left go at once, a breath apart.
                run.Flag = false;
                int sent = 0;
                for (int i = 0; i < run.Count; i++)
                    if (times[i] == Hovering) Loose(ref run, i, run.Victim, 0.05f * sent++);
            }
            else if (!over && run.Age >= run.T1)
            {
                run.T1 = run.Age + FireEvery;
                var near = cast.Combat.EnemyNear(body.position, cast.Reach, null);
                if (near != null)
                    for (int i = 0; i < run.Count; i++)
                        if (times[i] == Hovering)
                        {
                            Loose(ref run, i, near, 0f);
                            break;
                        }
            }

            bool any = false;
            for (int i = 0; i < run.Count; i++)
            {
                if (times[i] == Spent) continue;
                if (times[i] == Hovering)
                {
                    if (over)
                    {
                        FormFx.Release(solids[i], 0.2f);
                        solids[i] = -1;
                        times[i] = Spent;
                        continue;
                    }
                    any = true;
                    // A wing of them behind the shoulders, points up and outward, each lagging a little behind the body.
                    float angle = FormRules.InFan(i, run.Count, Mathf.Min(150f, 34f * run.Count));
                    Vector3 outward = Quaternion.Euler(0f, 180f + angle, 0f) * body.forward;
                    Vector3 want = body.position + outward * 0.95f + Vector3.up * (1.55f + 0.5f * Mathf.Cos(angle * Mathf.Deg2Rad) + 0.08f * Mathf.Sin(run.Age * 4f + i));
                    points[i] = Vector3.Lerp(points[i], want, 1f - Mathf.Exp(-16f * deltaTime));
                    float formed = Mathf.Clamp01((run.Age - i * 0.05f) / 0.14f);
                    FormFx.Place(solids[i], points[i], Quaternion.LookRotation(Vector3.up + outward * 0.55f, -outward), Vector3.one * (Length * formed));
                    FormFx.Heat(solids[i], formed < 1f ? 1f : 0f);
                    continue;
                }
                any = true;
                // In flight: straight at where its enemy is now.
                Component victim = things[i];
                if (FormKit.Alive(victim)) run.B = victim.transform.position + Vector3.up * 1.1f;
                times[i] += deltaTime;
                if (times[i] < 0.0001f) continue;
                Vector3 gap = run.B - points[i];
                float step = Mathf.Max(8f, cast.Spec.Speed * 2.4f) * deltaTime;
                if (gap.magnitude > step)
                {
                    points[i] += gap.normalized * step;
                    FormFx.Place(solids[i], points[i], Quaternion.LookRotation(gap), new Vector3(Length * 0.8f, Length * 0.8f, Length * 1.3f));
                    FormFx.Heat(solids[i], 0f);
                    continue;
                }
                if (FormKit.Alive(victim))
                {
                    cast.Combat.StrikeOne(victim, cast.Blow(cast.Damage, cast.Stagger), cast.Player.transform.position);
                    FormFx.Cut(run.B, gap, 4.2f, 0.55f, FormKit.Lit(cast.Colour, 3.4f), 0.16f);
                    FormFx.Face(FormMark.Star, run.B, 1.8f, FormKit.Lit(cast.Colour, 4f), 0.09f, 2.4f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.14f, 0.15f);
                }
                FormFx.Release(solids[i]);
                solids[i] = -1;
                times[i] = Spent;
            }
            return any;
        }

        // A time below zero but above the marks is a blade waiting its turn in a volley: it counts up to zero and goes.
        private static void Loose(ref SpellRun run, int index, Component victim, float wait)
        {
            SpellWorks.Things(run)[index] = victim;
            SpellWorks.Times(run)[index] = -Mathf.Min(0.9f, wait) - 0.0001f;
            FormFx.Heat(SpellWorks.Solids(run)[index], 1f);
        }
    }
}
