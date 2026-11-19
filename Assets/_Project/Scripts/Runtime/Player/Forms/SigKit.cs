using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What the gods' gifts and the crowns of the mixed branches share: who stands near, what they carry, whether the
    /// surge still burns, and the strong moment (the stop, the slow, the shake, the flash) said one way.
    /// </summary>
    public static class SigKit
    {
        /// <summary>True while the caster's surge lasts.</summary>
        public static bool SurgeOn(in SpellCast cast) => cast.Caster != null && cast.Caster.SurgeActive;

        /// <summary>
        /// Writes the enemies within a radius of a point into the run's list of things, nearest first, from the place
        /// given, and says how many. It borrows the run's list of who was struck and leaves it empty.
        /// </summary>
        public static int Gather(in SpellRun run, Vector3 centre, float radius, int most, int first = 0)
        {
            var things = SpellWorks.Things(run);
            var seen = SpellWorks.Once(run);
            seen.Clear();
            int found = 0;
            while (found < most && first + found < things.Length)
            {
                var near = run.Cast.Combat.EnemyNear(centre, radius, seen);
                if (!(near is IDamageable one)) break;
                seen.Add(one);
                things[first + found++] = near;
            }
            seen.Clear();
            return found;
        }

        public static int Depth(Component who, AfflictionKind kind) => who is IAfflictable carrier ? carrier.Afflictions.Stacks(kind) : 0;

        public static bool Has(Component who, AfflictionKind kind) => who is IAfflictable carrier && carrier.Afflictions.Has(kind);

        public static AfflictionSet Set(Component who) => who is IAfflictable carrier ? carrier.Afflictions : null;

        /// <summary>Where something stands, on the ground at a height.</summary>
        public static Vector3 Foot(Component who, float y)
        {
            Vector3 at = who.transform.position;
            at.y = y;
            return at;
        }

        /// <summary>The strong moment: a stop of time, a shake, a slow of everything for a while, and a flash at the screen's edges.</summary>
        public static void Moment(in SpellCast cast, float stop, float shake, float slowSeconds, float slowScale, float flash)
        {
            if (cast.Upgrades != null)
            {
                cast.Upgrades.Jolt(stop, shake, 0.45f);
                if (slowSeconds > 0f) cast.Upgrades.Slow(slowSeconds, slowScale);
            }
            if (flash > 0f) FormKit.Flash(cast, flash, 0.09f);
        }

        /// <summary>A crown of fangs thrown up round a point.</summary>
        public static void Crown(Vector3 at, float radius, int count, float height, Color colour, float stand = 0.4f, float turn = 0f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 outward = FormRules.OnRing(i, count, radius, turn);
                FormFx.Erupt(FormMesh.Spike, at + outward, FormKit.Yaw(outward) * Quaternion.Euler(34f, 0f, 0f),
                    new Vector3(0.8f, height * (1f + 0.25f * (i % 3)), 0.8f), colour, 0.07f, stand + 0.05f * (i % 2), 0.3f);
            }
        }

        /// <summary>A column of light standing on a point for a moment: something came down from the sky here.</summary>
        public static void Column(Vector3 at, Color light, float width, float seconds)
        {
            FormFx.Cut(at + Vector3.up * 5f, Vector3.up, 11f, width, light, seconds, width * 0.15f);
        }

        /// <summary>Straight down, for a sword that hangs or falls.</summary>
        public static Quaternion Down => Quaternion.LookRotation(Vector3.down + new Vector3(0.06f, 0f, 0.04f), Vector3.forward);
    }
}
