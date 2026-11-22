using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Uraeus: a cobra of flame rises on the player's left shoulder and stands Seconds. Whatever begins an attack within Reach
    /// of it, it spits burning venom at as the wind-up shows: Damage, Stagger and Stacks, and the venom burns at the enemy's
    /// feet for two seconds; Count spits at most. With SpellSpec.Planted it is set down where it was cast and spits on its
    /// Interval at the nearest enemy within Radius, whoever is near (Set Down). Its body is the cobra model when the editor
    /// step has wired it (CreatureLibrary), else a tongue of flame.
    /// </summary>
    public class CobraForm : RunningForm
    {
        public override string Id => SpellForms.Cobra;

        private const int Remembered = 8;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Spec.Planted ? cast.Origin + cast.Forward * Mathf.Max(0.5f, cast.Reach) : cast.Origin;
            run.B = cast.Forward;
            run.Count = Mathf.Max(1, cast.Spec.Count);
            run.T0 = Mathf.Max(1f, cast.Spec.Seconds);
            run.T1 = 0f;
            var rig = CreatureLibrary.Make(CreatureKind.Cobra, Where(run), Quaternion.LookRotation(Flat(cast.Forward), Vector3.up));
            if (rig != null)
            {
                if (!cast.Spec.Planted) rig.transform.localScale = Vector3.one * 0.8f;
                rig.Play("cobra_rise");
                SpellWorks.Things(run)[0] = rig;
            }
            else SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Flame, cast.Colour, 0.04f);
            ElementLook.Puff(cast.Element, Where(run), Vector3.up, 1f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            if (cast.Player == null || cast.Combat == null) return false;
            var rig = SpellWorks.Things(run)[0] as CreatureRig;
            Vector3 at = Where(run);
            Vector3 facing = cast.Spec.Planted ? run.B : cast.Player.transform.forward;
            if (rig != null) rig.transform.SetPositionAndRotation(at, Quaternion.LookRotation(Flat(facing), Vector3.up));
            else FormFx.Place(SpellWorks.Solids(run)[0], at + Vector3.up * 0.3f, Quaternion.LookRotation(Flat(facing), Vector3.up), new Vector3(0.6f, 1.1f + 0.08f * Mathf.Sin(run.Age * 7f), 0.6f));
            if (run.Age >= run.T0 || run.Count <= 0) return false;
            // Who it may spit at: begun an attack within reach (on the shoulder, of the player; set down, of itself), or, set
            // down, the nearest in its circle on its beat.
            bool beat = cast.Spec.Planted && run.Age >= run.T1;
            var times = SpellWorks.Times(run);
            var things = SpellWorks.Things(run);
            var seen = SpellWorks.Once(run);
            seen.Clear();
            float reach = cast.Spec.Planted ? Mathf.Max(1f, cast.Spec.Radius) : Mathf.Max(1f, cast.Reach);
            for (int look = 0; look < 6 && run.Count > 0; look++)
            {
                var enemy = cast.Combat.EnemyNear(at, reach, seen);
                if (!(enemy is IDamageable body)) break;
                seen.Add(body);
                var afflicted = enemy as IAfflictable;
                bool winding = afflicted != null && afflicted.Afflictions.WindingUp;
                Vector3 gap = enemy.transform.position - at;
                gap.y = 0f;
                if (!CobraRules.Spits(run.Count, gap.magnitude, reach, winding, beat, run.Age - SpatAt(things, times, enemy))) continue;
                Spit(ref run, rig, at, enemy);
                Remember(things, times, enemy, run.Age);
                if (beat)
                {
                    run.T1 = run.Age + Mathf.Max(0.3f, cast.Spec.Interval);
                    break;
                }
            }
            return true;
        }

        // The spit: venom from the snout to the enemy, its blow, and the venom burning at its feet.
        private static void Spit(ref SpellRun run, CreatureRig rig, Vector3 at, Component enemy)
        {
            var cast = run.Cast;
            run.Count--;
            Vector3 mouth = rig != null ? rig.MouthAt : at + Vector3.up * 0.9f;
            Vector3 chest = enemy.transform.position + Vector3.up * 1f;
            if (rig != null) rig.Play("cobra_spit");
            FormFx.Fly(FormMesh.Fireball, mouth, chest, Quaternion.LookRotation(chest - mouth), Vector3.one * 0.45f, cast.Colour, 0.14f);
            cast.Combat.StrikeOne(enemy, cast.Blow(cast.Damage, cast.Stagger), at);
            Vector3 feet = enemy.transform.position;
            Hazards.Lay(cast.Ground(feet, 0.9f, CobraRules.VenomSeconds, 0.5f, cast.Linger(cast.Damage * 0.25f)));
            ElementLook.Sparks(cast.Element, feet + Vector3.up * 0.3f, Vector3.up, 0.6f);
        }

        /// <summary>Uraeus' Master: a spit landing on an enemy already burning makes the cobra stand a second longer.</summary>
        public override void Lengthen(ref SpellRun run, float seconds) => run.T0 += Mathf.Max(0f, seconds);

        public override void End(ref SpellRun run)
        {
            CreatureLibrary.Unmake(SpellWorks.Things(run)[0] as CreatureRig);
            SpellWorks.Things(run)[0] = null;
        }

        // On the left shoulder, or where it was set down.
        private static Vector3 Where(in SpellRun run)
        {
            if (run.Cast.Spec.Planted || run.Cast.Player == null) return run.A;
            var body = run.Cast.Player.transform;
            return body.position + Vector3.up * CobraRules.ShoulderUp - body.right * CobraRules.ShoulderLeft;
        }

        private static Vector3 Flat(Vector3 way)
        {
            way.y = 0f;
            return way.sqrMagnitude > 0.0001f ? way.normalized : Vector3.forward;
        }

        // When it last spat at this enemy (the notes' places 1 to 8 of things and times), long ago when never.
        private static float SpatAt(Component[] things, float[] times, Component enemy)
        {
            for (int i = 1; i <= Remembered; i++)
                if (ReferenceEquals(things[i], enemy)) return times[i];
            return -100f;
        }

        private static void Remember(Component[] things, float[] times, Component enemy, float now)
        {
            int slot = 1;
            for (int i = 1; i <= Remembered; i++)
            {
                if (ReferenceEquals(things[i], enemy) || things[i] == null) { slot = i; break; }
                if (times[i] < times[slot]) slot = i;
            }
            things[slot] = enemy;
            times[slot] = now;
        }
    }
}
