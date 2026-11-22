using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Riptide's River Serpent: a serpent of water swims Reach out along a lane Radius wide at Speed and back. The first enemy
    /// it meets it bites (Damage, Stagger, Stacks) and carries back in its jaws, setting it down 1.5 m in front of the player;
    /// it takes no other. Its body is the serpent model when the editor step has wired it (CreatureLibrary), else a run of
    /// water drops.
    /// </summary>
    public class SerpentForm : RunningForm
    {
        public override string Id => SpellForms.Serpent;

        private const int Drops = 5;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            Vector3 forward = cast.Forward;
            forward.y = 0f;
            run.B = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            run.C = run.A;
            run.T0 = Mathf.Max(1f, cast.Reach);
            run.Victim = null;
            var rig = CreatureLibrary.Make(CreatureKind.RiverSerpent, run.A, Quaternion.LookRotation(run.B, Vector3.up));
            if (rig != null)
            {
                rig.Play("serpent_swim");
                SpellWorks.Things(run)[0] = rig;
            }
            else
                for (int i = 0; i < Drops; i++) SpellWorks.Solids(run)[i] = FormFx.Hold(FormMesh.Droplet, cast.Colour, 0.04f);
            ElementLook.Puff(cast.Element, run.A + run.B * 0.8f, run.B, 1f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            if (cast.Player == null || cast.Combat == null) return false;
            var rig = SpellWorks.Things(run)[0] as CreatureRig;
            float speed = Mathf.Max(4f, cast.Spec.Speed);
            float radius = Mathf.Max(0.5f, cast.Spec.Radius);
            Vector3 way;
            if (run.Phase == 0)
            {
                // Out along the lane: the first enemy its head meets is bitten and taken.
                Vector3 before = run.C;
                float gone = Mathf.Min(run.T0, run.Age * speed);
                run.C = run.A + run.B * gone;
                way = run.B;
                var taken = cast.Combat.EnemyNear(run.C, radius, null);
                if (taken != null && PlayerReactions.Alive(taken))
                {
                    run.Victim = taken;
                    cast.Combat.StrikeOne(taken, cast.Blow(cast.Damage, cast.Stagger), before);
                    if (rig != null) rig.Play("serpent_bite");
                    ElementLook.Sparks(cast.Element, taken.transform.position + Vector3.up, -run.B, 1f);
                    run.Phase = 1;
                }
                else if (gone >= run.T0) run.Phase = 1;
            }
            else
            {
                // Home to the player, wherever they are now, with what it took held in its jaws.
                Vector3 home = SerpentRules.DropAt(cast.Player.transform.position, cast.Player.transform.forward);
                Vector3 gap = home - run.C;
                gap.y = 0f;
                way = gap.sqrMagnitude > 0.0001f ? -gap.normalized : -run.B;
                run.C += Vector3.ClampMagnitude(gap, speed * deltaTime);
                bool arrived = gap.magnitude <= speed * deltaTime + 0.05f;
                if (run.Victim != null && PlayerReactions.Alive(run.Victim) && run.Victim is ICarriable carried)
                {
                    carried.CarryTo(run.C);
                    if (arrived) carried.CarryTo(home);
                }
                if (arrived)
                {
                    if (run.Victim != null) ElementLook.Puff(cast.Element, home + Vector3.up * 0.4f, Vector3.up, 1.2f);
                    return false;
                }
            }
            Show(run, rig, way);
            return true;
        }

        // The body faces the way it swims; the drops trail behind the head.
        private static void Show(in SpellRun run, CreatureRig rig, Vector3 way)
        {
            Quaternion turn = Quaternion.LookRotation(way.sqrMagnitude > 0.0001f ? way : Vector3.forward, Vector3.up);
            if (rig != null)
            {
                rig.transform.SetPositionAndRotation(run.C, turn);
                return;
            }
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < Drops; i++)
            {
                Vector3 trail = run.C - way * (i * 0.7f) + Vector3.Cross(Vector3.up, way) * (0.3f * Mathf.Sin(run.Age * 14f - i));
                FormFx.Place(solids[i], trail + Vector3.up * 0.4f, turn, Vector3.one * (0.9f - i * 0.12f));
            }
        }

        public override void End(ref SpellRun run)
        {
            CreatureLibrary.Unmake(SpellWorks.Things(run)[0] as CreatureRig);
            SpellWorks.Things(run)[0] = null;
        }
    }
}
