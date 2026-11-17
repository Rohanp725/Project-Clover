using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Fire's brand, Sunspot. A spot of fire is set on the enemy faced: a small sun over its head and a ring under
    /// it. Every light blow of the weapon that lands on it feeds the spot (the sun swells, the ring grows a step, one
    /// more flame stands round it), up to the spell's count. One heavy blow sets it off: a single burst, bigger for
    /// every blow fed, that also lays the branded enemy's Burn on everything it reaches. If its time runs out first
    /// it bursts at half. With no enemy faced nothing is branded and the mana is lost. Sums in <see cref="SunspotRules"/>.
    /// Reads: Count, Seconds, Radius, Damage, Stagger.
    /// </summary>
    public class SunspotForm : RunningForm
    {
        public override string Id => SpellForms.Sunspot;

        private static readonly Color Gold = new Color(1f, 0.82f, 0.36f);

        public override int Cast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            if (victim == null)
            {
                ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 1.2f + cast.Forward, cast.Forward, 0.6f);
                if (cast.Defence != null) cast.Defence.Callout("No one to brand");
                return 0;
            }
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Victim = victim;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 8);
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            solids[0] = FormFx.Hold(FormMesh.Orb, Color.Lerp(cast.Colour, Gold, 0.4f), 0.05f);
            for (int i = 0; i < run.Count; i++) solids[1 + i] = -1;
            marks[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2.6f));
            marks[1] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(Gold, 2.8f));
            marks[2] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.2f, 0.6f));
            Vector3 chest = victim.transform.position + Vector3.up * 1.1f;
            FormFx.Cut(chest, Vector3.Cross(Vector3.up, cast.Forward) + cast.Forward * 0.4f, 4.4f, 0.5f, FormKit.Lit(cast.Colour, 3.4f), 0.18f);
            FormFx.Lay(FormMark.Ring, victim.transform.position, 5f, FormKit.Lit(cast.Colour, 3f), 0.22f, 1.4f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.12f, 0.15f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // Noted here, acted on in the next step: the swing is still being reckoned.
            if (target == null || !ReferenceEquals(target, run.Victim)) return;
            if (step.Heavy) run.Flag = true;
            else run.Phase++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            if (!FormKit.Alive(run.Victim)) return false;
            Vector3 foot = run.Victim.transform.position;
            Vector3 over = foot + Vector3.up * 2.7f;

            while (run.Phase > 0)
            {
                run.Phase--;
                int fed = SunspotRules.Feed(run.Done, run.Count);
                if (fed == run.Done) continue;
                run.Done = fed;
                run.T0 = run.Age;
                // One more flame stands round it, and the ring jumps a step.
                solids[fed] = FormFx.Hold(FormMesh.Flame, cast.Colour, 0.03f);
                FormFx.Lay(FormMark.Ring, foot, Ring(fed) * 0.6f, FormKit.Lit(Gold, 3.2f), 0.2f, Ring(fed) * 1.15f);
                FormFx.Face(FormMark.Star, over, 1.2f, FormKit.Lit(Gold, 3.6f), 0.1f, 2.2f + 0.3f * fed, fed * 30f);
                ElementLook.Sparks(cast.Element, over, Vector3.up, 0.5f + 0.1f * fed);
            }

            bool timeUp = run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds);
            if (run.Flag || timeUp)
            {
                Burst(ref run, foot, run.Flag ? 1f : SunspotRules.LeftAlone);
                return false;
            }

            float full = run.Done / (float)run.Count;
            float kick = 1f + 0.35f * Mathf.Clamp01(1f - (run.Age - run.T0) / 0.18f) * (run.Done > 0 ? 1f : 0f);
            float beat = 1f + 0.06f * Mathf.Sin(run.Age * (8f + 10f * full));
            float size = (0.38f + 0.75f * full) * kick * beat;
            FormFx.Place(solids[0], over, Quaternion.Euler(run.Age * 60f, run.Age * 95f, 0f), Vector3.one * size);
            FormFx.Heat(solids[0], 0.1f + 0.35f * full + 0.1f * Mathf.Sin(run.Age * 13f));
            float ring = Ring(run.Done) * (run.Age < 0.1f ? 2f - run.Age / 0.1f : 1f);
            FormFx.PlaceMark(marks[0], foot, Quaternion.Euler(0f, run.Age * 50f, 0f), new Vector3(ring, 1f, ring));
            FormFx.PlaceMark(marks[2], foot, Quaternion.identity, new Vector3(ring * 0.9f, 1f, ring * 0.9f));
            FormFx.PlaceMarkFacing(marks[1], over, size * 3.6f, run.Age * 45f);
            // The flames fed to it stand in a ring round the enemy, turning with the ring.
            for (int i = 1; i <= run.Done; i++)
            {
                Vector3 outward = FormRules.OnRing(i - 1, run.Count, ring * 0.5f, run.Age * 50f);
                FormFx.Place(solids[i], foot + outward, FormKit.Yaw(outward), new Vector3(0.55f, 0.8f + 0.25f * Mathf.Sin(run.Age * 9f + i), 0.55f));
            }
            return true;
        }

        // Metres across the ring under the branded enemy with this many blows fed.
        private static float Ring(int fed) => 1.5f + 0.38f * fed;

        private static void Burst(ref SpellRun run, Vector3 foot, float share)
        {
            var cast = run.Cast;
            float radius = Mathf.Max(1f, cast.Spec.Radius);
            int fed = run.Done;
            float scale = cast.Spec.Damage > 0f ? cast.Damage / cast.Spec.Damage : 1f;
            // The branded enemy's own Burn, read before the burst deepens it, is laid on all the burst reaches.
            int carried = SigKit.Depth(run.Victim, AfflictionKind.Burn);
            var blow = cast.Blow(SunspotRules.Damage(cast.Damage, fed, scale) * share, SunspotRules.Stagger(cast.Stagger, fed) * share);
            blow.Heavy = true;
            cast.Combat.StrikeArea(foot, radius, blow);
            if (carried > 0) cast.Combat.AfflictArea(foot, radius, AfflictionKind.Burn, carried, run.Victim);
            float weight = (0.8f + 0.2f * fed) * Mathf.Lerp(0.6f, 1f, share);
            FormKit.Impact(cast, foot, radius, weight);
            FormFx.Lay(FormMark.Star, foot, radius, FormKit.Lit(Gold, 3.6f), 0.28f, radius * (2.6f + 0.3f * fed));
            FormFx.Lay(FormMark.Ring, foot, 1f, FormKit.Lit(cast.Colour, 4f), 0.4f, radius * 2.6f);
            // Every flame fed to it leaps outward as a tongue of fire, and a wall of flame stands on the burst's rim.
            int tongues = 6 + 2 * fed;
            for (int i = 0; i < tongues; i++)
            {
                Vector3 outward = FormRules.OnRing(i, tongues, 1f, 13f);
                Vector3 from = foot + Vector3.up * 0.9f;
                FormFx.Fly(FormMesh.Fireball, from, from + outward * radius * 1.05f, Quaternion.LookRotation(outward), Vector3.one * (0.8f + 0.08f * fed), cast.Colour, 0.24f);
                if (i % 2 == 0)
                    FormFx.Erupt(FormMesh.Flame, foot + outward * radius * 0.85f, FormKit.Yaw(outward), new Vector3(1f, 1.5f + 0.25f * fed, 1f), cast.Colour, 0.07f, 0.4f, 0.4f);
            }
            SigKit.Column(foot, FormKit.Lit(Gold, 3f), 1.2f + 0.3f * fed, 0.2f);
            if (fed >= 3) SigKit.Moment(cast, 0.05f + 0.02f * fed, 0.3f + 0.08f * fed, 0f, 1f, 0.2f + 0.04f * fed);
        }
    }
}
