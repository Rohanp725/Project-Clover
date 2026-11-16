using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The second half of Ice's gift: when something frozen is shattered, a line of breaking ice runs from it to the
    /// nearest enemy that is still Frozen and shatters that one too, which sends the line on again.
    /// Reads: Damage, Stagger, Reach.
    /// </summary>
    public class ShatterChainForm : RunningForm
    {
        public override string Id => SpellForms.ShatterChain;

        private const float RunSeconds = 0.2f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = FormKit.AimPoint(cast);
            var seen = SpellWorks.Once(run);
            seen.Clear();
            if (FormKit.Victim(cast) is IDamageable broken) seen.Add(broken);
            Component next = null;
            for (int tried = 0; tried < 8 && next == null; tried++)
            {
                var near = cast.Combat.EnemyNear(run.A, cast.Reach, seen);
                if (!(near is IDamageable one)) break;
                seen.Add(one);
                if (near is IAfflictable cold && cold.Afflictions.IsFrozen) next = near;
            }
            seen.Clear();
            if (next == null)
            {
                run.Phase = 9;
                return 0;
            }
            run.Victim = next;
            Vector3 to = SigKit.Foot(next, run.A.y);
            FormFx.Run(FormMark.Streak, run.A, to - run.A, (to - run.A).magnitude, 0.9f, FormKit.Lit(cast.Colour, 3.4f), RunSeconds + 0.15f, 0.2f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            if (run.Age < RunSeconds) return true;
            var cast = run.Cast;
            if (!FormKit.Alive(run.Victim)) return false;
            Vector3 at = SigKit.Foot(run.Victim, run.A.y);
            var blow = cast.Blow(cast.Damage, cast.Stagger, 0);
            blow.Heavy = true;
            cast.Combat.StrikeOne(run.Victim, blow, run.A);
            FormKit.Impact(cast, at, 1.4f, 0.9f);
            SigKit.Crown(at, 0.9f, 6, 1.4f, cast.Colour, 0.3f);
            return false;
        }
    }
}
