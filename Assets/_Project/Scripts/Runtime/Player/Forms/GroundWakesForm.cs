using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Overgrowth's crown, The Ground Wakes. A ring of standing stone and root comes up round the player and holds
    /// every enemy inside it, shaken and seeded. When the hold ends the ring flowers: a burst, and the player is
    /// mended once for every enemy it held.
    /// Reads: Radius, Seconds (of the hold), Damage, Stagger, Stacks (of Tremor), LingerDamage (health for each held).
    /// </summary>
    public class GroundWakesForm : RunningForm
    {
        public override string Id => SpellForms.GroundWakes;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            float radius = cast.Spec.Radius;
            float stand = cast.Spec.Seconds;
            run.A = cast.Origin;
            run.Count = cast.Combat.HoldArea(run.A, radius, stand);
            cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Tremor, cast.Spec.Stacks);
            cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Seeded, 1);
            Color root = Elements.Colour(Elements.Life);
            const int stones = 16;
            for (int i = 0; i < stones; i++)
            {
                Vector3 outward = FormRules.OnRing(i, stones, radius);
                // Stone and root by turns, leaning inward over what they hold.
                if (i % 2 == 0)
                    FormFx.Erupt(FormMesh.Pillar, run.A + outward, FormKit.Yaw(-outward) * Quaternion.Euler(10f, 0f, 0f), new Vector3(1.3f, 3.2f + 0.6f * (i % 4), 1.3f), cast.Colour, 0.14f + 0.01f * i, stand, 0.5f, 0.05f);
                else
                    FormFx.Erupt(FormMesh.Spike, run.A + outward, FormKit.Yaw(-outward) * Quaternion.Euler(28f, 0f, 0f), new Vector3(0.9f, 3.6f, 0.9f), root, 0.2f + 0.01f * i, stand - 0.1f, 0.5f);
            }
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 3.6f), 0.4f, radius * 2f);
            FormFx.Lay(FormMark.Disc, run.A, radius * 2f, FormKit.Lit(root, 1.4f), stand);
            FormFx.Lay(FormMark.Crack, run.A, radius * 2f, FormKit.Lit(cast.Colour, 2.2f), stand + 1f);
            FormFx.Lay(FormMark.Sigil, run.A, radius * 1.2f, FormKit.Lit(root, 2f), stand, -1f, 30f);
            SigKit.Moment(cast, 0.12f, 0.7f, 0f, 1f, 0.3f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            if (run.Age < cast.Spec.Seconds) return true;
            float radius = cast.Spec.Radius;
            Color root = Elements.Colour(Elements.Life);
            cast.Combat.StrikeArea(run.A, radius, cast.Blow(cast.Damage, cast.Stagger, 0));
            if (cast.Defence != null && run.Count > 0) cast.Defence.Heal(cast.Spec.LingerDamage * run.Count, true);
            // The flowering: every stone and root opens at its head.
            const int flowers = 16;
            for (int i = 0; i < flowers; i++)
            {
                Vector3 at = run.A + FormRules.OnRing(i, flowers, radius * (i % 2 == 0 ? 0.95f : 0.55f)) + Vector3.up * (i % 2 == 0 ? 3f : 1.2f);
                FormFx.Face(FormMark.Star, at, 0.6f, FormKit.Lit(i % 2 == 0 ? root : new Color(1f, 0.55f, 0.75f), 3.4f), 0.5f, 2.2f, i * 20f);
            }
            FormFx.Lay(FormMark.Ring, run.A, radius * 2f, FormKit.Lit(root, 3.6f), 0.45f, 1f);
            FormFx.Lay(FormMark.Star, run.A, 2f, FormKit.Lit(root, 3f), 0.3f, radius * 1.6f);
            ElementLook.Sparks(cast.Element, run.A + Vector3.up, Vector3.up, 2f);
            SigKit.Moment(cast, 0.1f, 0.5f, 0f, 1f, 0.3f);
            return false;
        }
    }
}
