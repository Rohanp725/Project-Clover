using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A copy of the player in the element's light: it stands at their shoulder doing what they did a beat ago, and
    /// every blow of the weapon that lands is landed again by the copy, a beat late, for a share of its damage.
    /// </summary>
    public class CloneForm : RunningForm
    {
        public override string Id => SpellForms.Clone;

        private const float Beat = 0.2f;
        /// <summary>How far a copy left standing reaches for an enemy to repeat a blow on: a sword's reach.</summary>
        public const float ShadowReach = 2.5f;

        public override int Cast(in SpellCast cast)
        {
            // One copy at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            var clone = SpellWorks.Clone;
            if (clone != null)
            {
                clone.Show(FormKit.Lit(cast.Colour, 4.4f, 1f));
                // Left where you stood (Second Shadow), not at your shoulder.
                if (cast.Spec.StaysPut) clone.AnchorAt(cast.Origin);
                SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 3f));
                SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 1.6f));
                FormKit.Gather(cast, clone.Position, 2f);
                FormFx.Lay(FormMark.Ring, clone.Position, 0.5f, FormKit.Lit(cast.Colour, 3f), 0.3f, 2.6f);
            }
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (target == null) return;
            var things = SpellWorks.Things(run);
            for (int k = 0; k < things.Length; k++)
            {
                if (things[k] != null) continue;
                things[k] = target;
                SpellWorks.Times(run)[k] = Beat;
                // The first number of the point is what the echo deals; the second says whether the blow was a heavy one.
                SpellWorks.Points(run)[k] = new Vector3(FormRules.Echo(damage, run.Cast.Spec.Damage), step.Heavy ? 1f : 0f, 0f);
                return;
            }
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var clone = SpellWorks.Clone;
            var things = SpellWorks.Things(run);
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            Vector3 from = clone != null && clone.Showing ? clone.Position : cast.Player.transform.position;
            FormFx.PlaceMark(SpellWorks.Marks(run)[0], from, Quaternion.Euler(0f, run.Age * 100f, 0f), new Vector3(1.9f, 1f, 1.9f));
            FormFx.PlaceMark(SpellWorks.Marks(run)[1], from, Quaternion.identity, new Vector3(1.3f, 1f, 1.3f));
            for (int k = 0; k < things.Length; k++)
            {
                if (things[k] == null) continue;
                times[k] -= deltaTime;
                if (times[k] > 0f) continue;
                Component victim = things[k];
                things[k] = null;
                // A copy left standing repeats the blow on whoever is nearest it, within its reach.
                if (clone != null && clone.Anchored) victim = cast.Combat.EnemyNear(from, ShadowReach, null);
                if (!FormKit.Alive(victim)) continue;
                bool heavy = points[k].y > 0.5f;
                if (!cast.Combat.StrikeOne(victim, cast.Blow(points[k].x, 0f), from)) continue;
                Vector3 chest = victim.transform.position + Vector3.up * 1.1f;
                Vector3 toward = chest - from;
                FormFx.Cut(chest, Vector3.Cross(Vector3.up, toward) + toward * 0.3f, heavy ? 4.8f : 3.4f, heavy ? 0.6f : 0.4f, FormKit.Lit(cast.Colour, 3.4f), 0.15f);
                FormFx.Run(FormMark.Streak, from, toward, new Vector3(toward.x, 0f, toward.z).magnitude, 0.5f, FormKit.Lit(cast.Colour, 2.4f), 0.14f, 0.05f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(heavy ? 0.04f : 0f, heavy ? 0.2f : 0.08f, 0.15f);
            }
            return run.Age < Mathf.Max(1f, cast.Spec.Seconds);
        }

        public override void End(ref SpellRun run)
        {
            var clone = SpellWorks.Clone;
            if (clone == null || !clone.Showing) return;
            ElementLook.Puff(run.Cast.Element, clone.Position + Vector3.up, Vector3.up, 0.9f);
            clone.Hide();
        }
    }
}
