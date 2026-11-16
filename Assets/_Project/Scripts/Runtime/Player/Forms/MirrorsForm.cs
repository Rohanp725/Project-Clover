using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Halo's crown, Six Mirrors. Six mirrors of steel stand in a ring round the enemy faced. The player is gone, and
    /// a line of light crosses the ring from each mirror in turn; then the player stands where they began, and every
    /// cut lands at once: on the enemy in full, with a mark and a notch for each, and at half on anything else a line
    /// crossed. Nothing can strike the player from the vanishing to the landing.
    /// Reads: Count (mirrors), Interval, Delay, Damage (each cut), Stagger, Radius (of the ring), Reach.
    /// </summary>
    public class MirrorsForm : RunningForm
    {
        public override string Id => SpellForms.Mirrors;

        private const float AfterSeconds = 0.3f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.Victim = FormKit.Victim(cast);
            run.A = FormKit.AimPoint(cast);
            run.B = cast.Forward;
            run.C = cast.Origin;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 8);
            run.T0 = Mathf.Max(0.05f, cast.Spec.Interval);
            run.T1 = Mathf.Max(0.15f, cast.Spec.Delay);
            float ring = Mathf.Max(2f, cast.Spec.Radius);
            float stand = run.Count * run.T0 + run.T1 + 0.3f;
            Color steel = Elements.Colour(Elements.Metal);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 outward = Mirror(run, i, ring);
                FormFx.Erupt(FormMesh.Slab, run.A + outward, FormKit.Yaw(-outward), new Vector3(1.5f, 2.6f, 0.5f), steel, 0.08f + 0.015f * i, stand, 0.25f, 0.07f);
                FormFx.Lay(FormMark.Ring, run.A + outward, 0.4f, FormKit.Lit(cast.Colour, 3f), 0.25f, 1.8f);
            }
            FormFx.Lay(FormMark.Ring, run.A, ring * 2f, FormKit.Lit(cast.Colour, 2.4f), stand);
            FormFx.Lay(FormMark.Sigil, run.A, ring * 1.3f, FormKit.Lit(cast.Colour, 2f), stand, -1f, 90f);
            SpellWorks.Ghost(stand, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            cast.Player.Hidden = true;
            FormKit.Flash(cast, 0.14f, 0.05f);
            return 0;
        }

        private static Vector3 Mirror(in SpellRun run, int index, float ring) => Quaternion.Euler(0f, 180f + index * 360f / run.Count, 0f) * run.B * ring;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            float ring = Mathf.Max(2f, cast.Spec.Radius);
            if (FormKit.Alive(run.Victim) && run.Phase == 0)
            {
                Vector3 under = run.Victim.transform.position;
                run.A = new Vector3(under.x, cast.Origin.y, under.z);
            }
            Vector3 chest = run.A + Vector3.up * 1.1f;
            if (run.Phase == 0)
            {
                while (run.Done < run.Count && run.Age >= 0.12f + run.Done * run.T0)
                {
                    // From this mirror, through the enemy, to the far side of the ring.
                    Vector3 from = Mirror(run, run.Done, ring);
                    float lasts = (run.Count - run.Done) * run.T0 + run.T1 + 0.04f;
                    FormFx.Cut(chest, -from, ring * 2.2f, 0.2f, FormKit.Lit(cast.Colour, 3.2f), lasts, 0.14f);
                    FormFx.Cut(chest, -from, ring * 2.6f, 0.8f, FormKit.Lit(cast.Colour, 4f), 0.07f, 0.1f);
                    FormFx.Face(FormMark.Star, run.A + from + Vector3.up * 1.4f, 1.4f, FormKit.Lit(cast.Colour, 4f), 0.1f, 2.4f);
                    ElementLook.Sparks(cast.Element, chest, -from, 0.4f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.07f, 0.08f);
                    run.Done++;
                }
                if (run.Age < 0.12f + run.Count * run.T0) return true;
                // Back where they began, the blade going home.
                run.Phase = 1;
                run.T2 = run.Age;
                player.Hidden = false;
                player.StepTo(run.C);
                player.transform.rotation = Quaternion.LookRotation(run.B, Vector3.up);
                FormFx.Lay(FormMark.Ring, run.C, 2.6f, FormKit.Lit(cast.Colour, 3f), 0.2f, 0.8f);
                return true;
            }
            player.FormPose = FormKit.Draw;
            player.FormSwordOut = true;
            if (run.Phase == 1)
            {
                player.FormPoseProgress = Mathf.Lerp(0.62f, 0.9f, (run.Age - run.T2) / run.T1);
                if (run.Age < run.T2 + run.T1) return true;
                run.Phase = 2;
                run.T2 = run.Age;
                int landed = 0;
                var once = SpellWorks.Once(run);
                for (int i = 0; i < run.Count; i++)
                {
                    Vector3 from = Mirror(run, i, ring);
                    var blow = cast.Blow(cast.Damage, cast.Stagger, 0);
                    once.Clear();
                    if (FormKit.Alive(run.Victim))
                    {
                        landed += cast.Combat.StrikeOne(run.Victim, blow, run.A + from) ? 1 : 0;
                        if (run.Victim is IDamageable faced) once.Add(faced);
                    }
                    // Anything else the line crossed takes half.
                    landed += cast.Combat.StrikeLane(run.A + from, -from, ring * 2f, 0.7f, cast.Blow(cast.Damage * 0.5f, cast.Stagger * 0.5f, 0), once);
                    FormFx.Cut(chest, -from, ring * 2.8f, 0.85f, FormKit.Lit(cast.Colour, 4f), 0.16f, 0.05f);
                }
                once.Clear();
                cast.Combat.AfflictArea(run.A, 1.2f, AfflictionKind.Marked, run.Count);
                cast.Combat.AfflictArea(run.A, 1.2f, AfflictionKind.Sundered, run.Count);
                FormFx.Face(FormMark.Star, chest, 4.2f, FormKit.Lit(cast.Colour, 4f), 0.11f, 5.4f);
                FormFx.Lay(FormMark.Scorch, run.A, 4f, FormKit.Stain(cast.Colour), 3f);
                FormFx.Lay(FormMark.Cross, run.A, 5f, FormKit.Lit(cast.Colour, 2.6f), 1.2f, -1f, 0f, 30f);
                ElementLook.Sparks(cast.Element, chest, Vector3.up, 2f);
                if (landed > 0) SigKit.Moment(cast, 0.18f, 0.6f, 0.3f, 0.4f, 0.4f);
                return true;
            }
            player.FormPoseProgress = Mathf.Lerp(0.9f, 0.99f, (run.Age - run.T2) / AfterSeconds);
            return run.Age < run.T2 + AfterSeconds;
        }
    }
}
