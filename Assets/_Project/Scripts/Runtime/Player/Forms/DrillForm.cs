using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The player becomes a boring point of the element and goes through everything on a line, coming out the far
    /// side facing back the way they came; then the cuts they are owed land on everyone on that line, one after
    /// another. Nothing can strike the player while it lasts. A wall, or anything too big to be shoved, stops it.
    /// </summary>
    public class DrillForm : RunningForm
    {
        public override string Id => SpellForms.Drill;

        private const float WindSeconds = 0.14f;
        private const float BetweenCuts = 0.07f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.A = cast.Origin;
            run.B = cast.Forward;
            run.T0 = cast.Player.ClearRun(cast.Forward, cast.Reach);
            run.T1 = Mathf.Max(0.08f, run.T0 / Mathf.Max(6f, cast.Spec.Speed * 1.8f));
            run.Count = Mathf.Max(1, cast.Spec.Count);
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Drill, cast.Colour, 0.05f);
            FormFx.Run(FormMark.Lane, run.A + run.B * 0.6f, run.B, Mathf.Max(0.5f, run.T0 - 0.6f), cast.Spec.Radius * 2f, FormKit.Lit(cast.Colour, 1.7f), WindSeconds + run.T1 + 0.1f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var solids = SpellWorks.Solids(run);
            Quaternion along = Quaternion.LookRotation(run.B, Vector3.up);
            if (run.Phase == 0)
            {
                // Coiled: the point forms in front of the chest, spinning up.
                float t = Mathf.Clamp01(run.Age / WindSeconds);
                player.FormPose = FormKit.Brace;
                player.FormPoseProgress = 0.4f + 0.5f * t;
                FormFx.Place(solids[0], run.A + Vector3.up * 1.05f + run.B * 0.3f, along * Quaternion.Euler(0f, 0f, run.Age * 1400f), Vector3.one * (1.25f * t));
                FormFx.Heat(solids[0], 1f);
                if (t < 1f) return true;
                run.Phase = 1;
                run.T2 = run.Age;
                run.T3 = 0f;
                player.Hidden = true;
                SpellWorks.Ghost(0.4f, FormKit.Lit(cast.Colour, 2.4f, 0.8f));
                SpellWorks.Once(run).Clear();
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.2f, 0.2f);
                return true;
            }
            if (run.Phase == 1)
            {
                float t = Mathf.Clamp01((run.Age - run.T2) / run.T1);
                float distance = run.T0 * t;
                Vector3 at = run.A + run.B * distance;
                player.PassTo(at);
                Vector3 body = player.transform.position + Vector3.up * 1.05f;
                FormFx.Place(solids[0], body - run.B * 0.4f, along * Quaternion.Euler(0f, 0f, run.Age * 2600f), new Vector3(1.25f, 1.25f, 1.6f));
                FormFx.Heat(solids[0], 0f);
                float passed = distance - run.T3;
                if (passed > 0.01f)
                {
                    int met = cast.Combat.StrikeLane(run.A + run.B * run.T3, run.B, passed + 1f, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger), SpellWorks.Once(run));
                    // Fire's drill raises Heat a level for every enemy it goes through.
                    if (met > 0 && cast.Affliction == Clover.Gameplay.AfflictionKind.Burn && cast.Upgrades != null) cast.Upgrades.Heat.Raise(met);
                    // The rings it throws off behind it.
                    if (Mathf.FloorToInt(distance / 1.1f) != Mathf.FloorToInt(run.T3 / 1.1f))
                    {
                        FormFx.Face(FormMark.Ring, body - run.B * 0.6f, 1.2f, FormKit.Lit(cast.Colour, 2.8f), 0.22f, 3f);
                        // The line it leaves burning behind it, for a spell that leaves one.
                        if (cast.Spec.LingerDamage > 0f && cast.Spec.Seconds > 0f)
                        {
                            Vector3 patch = at - run.B * 0.55f;
                            Clover.World.Hazards.Lay(cast.Ground(patch, Mathf.Max(0.8f, cast.Spec.Radius), cast.Spec.Seconds, 0.5f, cast.Linger(cast.Spec.LingerDamage)));
                            Vector3 across = Vector3.Cross(Vector3.up, run.B);
                            for (int flame = -1; flame <= 1; flame++)
                                FormFx.Erupt(FormMesh.Spike, patch + across * (flame * 0.45f) + run.B * (0.25f * flame), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.8f, 1.2f + 0.5f * Random.value, 0.8f), cast.Colour, 0.06f, 0.55f, 0.45f);
                            FormFx.Run(FormMark.Lane, patch - run.B * 0.6f, run.B, 1.2f, cast.Spec.Radius * 1.8f, FormKit.Lit(cast.Colour, 2.2f, 0.8f), 0.6f, 0.4f);
                        }
                        ElementLook.Sparks(cast.Element, body, -run.B, 0.5f);
                    }
                    run.T3 = distance;
                }
                // A drive that carries (Fire Front): what it met is kept at its front, a stride ahead of the player.
                if (cast.Spec.Carries) FormCarry.Carry(SpellWorks.Once(run), at + run.B * (cast.Spec.Radius * 0.5f + 0.9f), run.B, cast.Spec.OnlyCarrying);
                if (t < 1f) return true;
                // And thrown down at the end.
                if (cast.Spec.Carries) FormCarry.ThrowDown(SpellWorks.Once(run), cast.Spec.OnlyCarrying);
                // Out the far side: back in their own shape, turned to look at what they went through.
                run.Phase = 2;
                run.T2 = run.Age;
                run.Done = 1;
                player.Hidden = false;
                player.transform.rotation = Quaternion.LookRotation(-run.B, Vector3.up);
                FormFx.Release(solids[0], 0.12f);
                solids[0] = -1;
                FormFx.Run(FormMark.Streak, run.A, run.B, run.T0, 1.1f, FormKit.Lit(cast.Colour, 3f), 0.3f, 0f);
                return true;
            }
            player.FormPose = FormKit.Draw;
            player.FormSwordOut = true;
            player.FormPoseProgress = Mathf.Lerp(0.55f, 0.98f, (run.Age - run.T2) / (BetweenCuts * run.Count + 0.15f));
            // The cuts that are owed, one after another, on everyone still on the line.
            while (run.Done < run.Count && run.Age >= run.T2 + BetweenCuts * run.Done)
            {
                bool last = run.Done == run.Count - 1;
                int hits = cast.Combat.StrikeLane(run.A, run.B, run.T0, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                Vector3 middle = run.A + run.B * (run.T0 * 0.5f) + Vector3.up * 1.1f;
                Vector3 slant = Quaternion.Euler(0f, (run.Done % 2 == 0 ? 24f : -24f) + run.Done * 9f, 0f) * run.B;
                FormFx.Cut(middle, slant, run.T0 * 1.05f, 0.5f, FormKit.Lit(cast.Colour, 3.4f), 0.14f);
                if (hits > 0 && cast.Upgrades != null) cast.Upgrades.Jolt(last ? 0.09f : 0.02f, last ? 0.35f : 0.12f, 0.2f);
                if (last && hits > 0) FormKit.Flash(cast, 0.22f, 0.06f);
                run.Done++;
            }
            return run.Age < run.T2 + BetweenCuts * run.Count + 0.15f;
        }
    }
}
