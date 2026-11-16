using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A burst round the caster: the element is drawn in to a sign under their feet while the delay runs, and then
    /// goes out all at once as a crown of shards and a ring that sweeps to the edge.
    /// </summary>
    public class NovaForm : RunningForm
    {
        public override string Id => SpellForms.Nova;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            float radius = cast.Spec.Radius;
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            FormFx.Lay(FormMark.Disc, cast.Origin, radius * 2f, FormKit.Lit(cast.Colour, 1.8f), run.T0 + 0.05f);
            FormFx.Lay(FormMark.Ring, cast.Origin, radius * 2.2f, FormKit.Lit(cast.Colour, 3f), run.T0, 1.2f);
            FormFx.Lay(FormMark.Sigil, cast.Origin, 3.4f, FormKit.Lit(cast.Colour), run.T0, 2.2f, 260f);
            SpellWorks.Gesture(cast, FormKit.Brace, run.T0, 0f, 0.9f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            float radius = cast.Spec.Radius;
            Vector3 at = cast.Player.transform.position;
            if (run.Age < run.T0)
            {
                // Motes of it are pulled in from the edge.
                run.T1 -= deltaTime;
                if (run.T1 <= 0f)
                {
                    run.T1 = 0.05f;
                    Vector3 from = FormRules.OnRing(0, 1, radius * (1f - run.Age / run.T0), Random.value * 360f);
                    ElementLook.Puff(cast.Element, at + from + Vector3.up * 0.8f, -from, 0.35f);
                }
                return true;
            }
            int hits = cast.Combat.StrikeArea(at, radius, cast.Blow(cast.Damage, cast.Stagger));
            FormKit.Impact(cast, at, radius * 0.8f, 1.3f);
            FormFx.Lay(FormMark.Ring, at, 1f, FormKit.Lit(cast.Colour, 3.6f), 0.36f, radius * 2.1f);
            // The crown: a ring of shards thrown outward, a shorter ring inside it.
            int outer = Mathf.Clamp(Mathf.RoundToInt(radius * 3.2f), 8, 14);
            for (int i = 0; i < outer; i++)
            {
                Vector3 outward = FormRules.OnRing(i, outer, radius * 0.78f, 12f);
                FormFx.Erupt(FormMesh.Spike, at + outward, FormKit.Yaw(outward) * Quaternion.Euler(40f, 0f, 0f),
                    new Vector3(0.75f, 1.9f + 0.6f * (i % 2), 0.75f), cast.Colour, 0.06f, 0.3f + 0.04f * (i % 3), 0.28f);
            }
            for (int i = 0; i < 6; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 6, radius * 0.4f, 40f);
                FormFx.Erupt(FormMesh.Spike, at + outward, FormKit.Yaw(outward) * Quaternion.Euler(22f, 0f, 0f), new Vector3(0.6f, 1.3f, 0.6f), cast.Colour, 0.05f, 0.22f, 0.25f);
            }
            if (hits > 0 && cast.Upgrades != null) cast.Upgrades.Jolt(0.09f, 0.4f, 0.3f);
            SpellWorks.Gesture(cast, FormKit.Draw, 0.3f, 0.35f, 1f, true);
            return false;
        }
    }
}
