using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>Where Root Walk comes up: the plain rule, apart from the scene.</summary>
    public static class BurrowRules
    {
        /// <summary>Under the ground this long before coming up.</summary>
        public const float Under = 0.5f;
        /// <summary>It comes up this far short of the grown thing or the enemy, on the side it went down, not inside it.</summary>
        public const float Beside = 0.8f;
        /// <summary>With nothing to go to, it comes up this far ahead.</summary>
        public const float Ahead = 6f;

        /// <summary>
        /// The place to come up: beside <paramref name="to"/> on the way from <paramref name="from"/> when there is a place to
        /// go (a grown thing, a Seeded enemy), else <see cref="Ahead"/> metres along <paramref name="forward"/>.
        /// </summary>
        public static Vector3 ComeUp(Vector3 from, Vector3 forward, bool hasPlace, Vector3 to)
        {
            forward.y = 0f;
            if (!hasPlace) return from + (forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward) * Ahead;
            Vector3 gap = to - from;
            gap.y = 0f;
            if (gap.magnitude <= Beside) return from;
            Vector3 up = to - gap.normalized * Beside;
            up.y = from.y;
            return up;
        }
    }

    /// <summary>
    /// Root Walk: the player sinks into the ground in a swirl of leaves, untouchable, and comes up half a second later at a
    /// grown thing of theirs within Reach, the one nearest where they aim (with SpellSpec.AimAt, under the enemy carrying it
    /// instead: Up Through Them), or six metres ahead with none. Roots burst round them as they come up: Damage, Stagger and
    /// Stacks within Radius. With SpellSpec.SwapsGrown the grown thing goes to where they went down (Changeling).
    /// </summary>
    public class BurrowForm : RunningForm
    {
        public override string Id => SpellForms.Burrow;

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
            bool hasPlace = false;
            Vector3 to = run.A;
            // Up Through Them: under the enemy that carries what the spell seeks (NewCast aimed it there).
            if (cast.Spec.AimAt != Clover.Gameplay.AfflictionKind.None && cast.Target != null)
            {
                to = cast.Target.position;
                hasPlace = true;
                run.Victim = FormKit.Victim(cast);
            }
            else
            {
                Vector3 aim = cast.Aim.sqrMagnitude > 0.0001f ? cast.Aim : run.A + run.B * BurrowRules.Ahead;
                hasPlace = Hazards.NearestGrownTo(aim, run.A, Mathf.Max(1f, cast.Reach), out to);
                run.Flag2 = hasPlace;
            }
            run.C = BurrowRules.ComeUp(run.A, run.B, hasPlace, to);
            SpellWorks.Points(run)[0] = to;
            cast.Player.Hidden = true;
            // A swirl of leaves where they go down.
            ElementLook.Puff(cast.Element, run.A + Vector3.up * 0.5f, Vector3.up, 1.2f);
            ElementLook.Sparks(cast.Element, run.A + Vector3.up * 0.3f, Vector3.up, 1f);
            FormFx.Lay(FormMark.Crack, run.A, 1.8f, FormKit.Stain(cast.Colour, 0.6f), 0.15f, 1.2f, 0f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            if (run.Phase == 0)
            {
                // Under the ground: a line of churned earth runs to where they will come up.
                if (run.Age < BurrowRules.Under)
                {
                    float t = run.Age / BurrowRules.Under;
                    Vector3 under = Vector3.Lerp(run.A, run.C, t);
                    if (Mathf.FloorToInt(run.Age / 0.1f) != Mathf.FloorToInt((run.Age - deltaTime) / 0.1f))
                        ElementLook.Puff(cast.Element, under + Vector3.up * 0.1f, Vector3.up, 0.5f);
                    return true;
                }
                run.Phase = 1;
                run.T1 = run.Age;
                player.PassTo(run.C);
                player.Hidden = false;
                Vector3 facing = SpellWorks.Points(run)[0] - run.C;
                facing.y = 0f;
                if (facing.sqrMagnitude > 0.04f) player.transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                // Roots burst round them as they come up.
                cast.Combat.StrikeArea(run.C, Mathf.Max(0.5f, cast.Spec.Radius), cast.Blow(cast.Damage, cast.Stagger));
                for (int i = 0; i < 6; i++)
                {
                    Vector3 root = run.C + Quaternion.Euler(0f, i * 60f + 15f, 0f) * Vector3.forward * (cast.Spec.Radius * 0.6f);
                    FormFx.Erupt(FormMesh.Spike, root, Quaternion.Euler(Random.Range(-18f, 18f), i * 60f, Random.Range(-18f, 18f)), new Vector3(0.6f, 1.3f + 0.4f * Random.value, 0.6f), cast.Colour, 0.08f, 0.35f, 0.35f);
                }
                ElementLook.Puff(cast.Element, run.C + Vector3.up * 0.4f, Vector3.up, 1.3f);
                // Changeling: the grown thing stands where they went down, in the way of the blow aimed there.
                if (cast.Spec.SwapsGrown && run.Flag2) Hazards.MoveGrown(SpellWorks.Points(run)[0], run.A);
                return true;
            }
            return run.Age < run.T1 + 0.25f;
        }

        public override void End(ref SpellRun run)
        {
            if (run.Cast.Player != null) run.Cast.Player.Hidden = false;
        }
    }
}
