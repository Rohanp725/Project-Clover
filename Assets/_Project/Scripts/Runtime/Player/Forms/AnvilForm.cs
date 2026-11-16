using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Forge's crown, World Anvil. An anvil of stone comes up under the finished enemy and the ring of the blow runs
    /// out from it: every other enemy it reaches is struck, shaken and has its armour broken. A blade is forged for
    /// each one reached, six at most; they hang at the player's shoulders and fly with the next heavy swing.
    /// Reads: Radius, Damage, Stagger, Stacks, Count (most blades), LingerDamage (each blade's damage).
    /// </summary>
    public class AnvilForm : RunningForm
    {
        public override string Id => SpellForms.Anvil;

        private const float KeepSeconds = 14f;

        /// <summary>Blades forged for the enemies the ring reached.</summary>
        public static int Forged(int reached, int most) => Mathf.Clamp(reached, 0, Mathf.Max(0, most));

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            float radius = cast.Spec.Radius;
            run.A = FormKit.AimPoint(cast);
            var finished = FormKit.Victim(cast);
            var once = SpellWorks.Once(run);
            once.Clear();
            if (finished is IDamageable spared) once.Add(spared);
            int reached = cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Damage, cast.Stagger), once);
            cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Sundered, cast.Spec.Stacks, finished);
            once.Clear();
            run.Count = Forged(reached, Mathf.Min(cast.Spec.Count, 8));
            // The anvil: a foot, a waist and a face with a horn.
            FormFx.Erupt(FormMesh.Pillar, run.A, Quaternion.identity, new Vector3(2.2f, 0.7f, 1.5f), cast.Colour, 0.08f, 1.4f, 0.5f, 0.06f);
            FormFx.Erupt(FormMesh.Pillar, run.A, Quaternion.Euler(0f, 30f, 0f), new Vector3(1.1f, 1.5f, 1f), cast.Colour, 0.1f, 1.4f, 0.5f, 0.06f);
            FormFx.Erupt(FormMesh.Slab, run.A + Vector3.up * 1.2f, Quaternion.Euler(90f, 0f, 0f), new Vector3(3f, 1.5f, 0.6f), cast.Colour, 0.12f, 1.4f, 0.5f, 0.06f);
            Color light = FormKit.Lit(cast.Colour, 3.6f);
            Color steel = Elements.Colour(Elements.Metal);
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(steel, 2.2f), 0.45f, radius * 2f);
            FormFx.Lay(FormMark.Ring, run.A, 0.5f, light, 0.6f, radius * 2f);
            FormFx.Lay(FormMark.Crack, run.A, radius * 1.6f, FormKit.Lit(cast.Colour, 2.2f), 2.5f);
            FormFx.Face(FormMark.Star, run.A + Vector3.up * 1.6f, 2f, FormKit.Lit(steel, 4f), 0.14f, 5f);
            FormKit.Impact(cast, run.A, 2.4f, 2f);
            SigKit.Moment(cast, 0.18f, 0.8f, 0.3f, 0.4f, 0.4f);
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++)
            {
                solids[i] = FormFx.Hold(FormMesh.Blade, steel, 0.05f);
                FormFx.Run(FormMark.Streak, run.A, FormRules.OnRing(i, run.Count, 1f, 20f), radius, 0.5f, FormKit.Lit(steel, 3f), 0.3f, 0.1f);
            }
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            if (run.Phase == 0 && run.Age > 0.5f && step != null && step.Heavy) run.Phase = 1;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Count == 0) return run.Age < 0.2f;
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var player = cast.Player.transform;
            Vector3 here = player.position;
            if (run.Phase == 0)
            {
                // Forged at the anvil, then carried in a fan behind the shoulders.
                float carried = Mathf.Clamp01((run.Age - 0.25f) / 0.4f);
                for (int i = 0; i < run.Count; i++)
                {
                    Vector3 way = Quaternion.Euler(0f, FormRules.InFan(i, run.Count, 110f), 0f) * -player.forward;
                    Vector3 at = here + way * 0.9f + Vector3.up * (1.9f + 0.1f * Mathf.Sin(run.Age * 3f + i));
                    Vector3 forge = run.A + FormRules.OnRing(i, run.Count, 1.2f) + Vector3.up * 2.4f;
                    FormFx.Place(solids[i], Vector3.Lerp(forge, at, carried), Quaternion.LookRotation(Vector3.up + way * 0.5f), Vector3.one * 1.7f);
                    FormFx.Heat(solids[i], run.Age < 0.3f ? 1f : 0f);
                }
                return run.Age < KeepSeconds;
            }
            // They fly: each at an enemy near, round and round the ones that are there.
            int near = SigKit.Gather(run, here, 10f, run.Count);
            var things = SpellWorks.Things(run);
            Color steel = Elements.Colour(Elements.Metal);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 from = here + Vector3.up * 1.9f;
                FormFx.Release(solids[i]);
                solids[i] = -1;
                if (near == 0)
                {
                    FormFx.Fly(FormMesh.Blade, from, from + player.forward * 8f, FormKit.Yaw(player.forward), Vector3.one * 1.9f, steel, 0.25f);
                    continue;
                }
                var victim = things[i % near];
                Vector3 chest = victim.transform.position + Vector3.up * 1.1f;
                FormFx.Fly(FormMesh.Blade, from, chest, Quaternion.LookRotation(chest - from), Vector3.one * 1.9f, steel, 0.16f + 0.02f * i);
                var blow = cast.Blow(cast.Spec.LingerDamage, 12f, 0);
                blow.Affliction = AfflictionKind.Sundered;
                blow.Stacks = 1;
                cast.Combat.StrikeOne(victim, blow, here);
                FormFx.Cut(chest, chest - from, 4f, 0.45f, FormKit.Lit(steel, 4f), 0.14f, 0.05f);
            }
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.06f, 0.35f, 0.2f);
            return false;
        }
    }
}
