using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Life's gift, First Spring. While the surge lasts the surge button pressed again gives half the player's present
    /// health to the ground (it is left grey). A stand of trees comes up through every Seeded enemy near and holds it;
    /// blows on what the forest holds give the health back, up to what was given.
    /// Reads: Reach, Seconds (of the hold), Damage, Stagger, Count (percent of a blow that returns as health).
    /// </summary>
    public class SpringForm : RunningForm
    {
        public override string Id => SpellForms.Spring;

        private const int Most = 8;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = cast.Upgrades != null ? cast.Upgrades.SurgeAgain : 0;
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            if (run.Phase != 1 || target == null) return;
            var held = SpellWorks.Things(run);
            for (int i = 0; i < run.Done; i++)
                if (held[i] == target) run.T2 += FormRules.Echo(damage, run.Cast.Spec.Count);
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                if (!SigKit.SurgeOn(cast) || cast.Upgrades == null || cast.Defence == null) return false;
                cast.Upgrades.ListenForSurge();
                if (cast.Upgrades.SurgeAgain == run.Count) return true;
                run.Count = cast.Upgrades.SurgeAgain;
                var things = SpellWorks.Things(run);
                int near = SigKit.Gather(run, here, cast.Reach, NotesKept);
                int seeded = 0;
                for (int i = 0; i < near; i++)
                    if (SigKit.Has(things[i], AfflictionKind.Seeded) && seeded < Most) things[seeded++] = things[i];
                for (int i = seeded; i < near; i++) things[i] = null;
                if (seeded == 0)
                {
                    cast.Defence.Callout("Nothing is Seeded");
                    return true;
                }
                run.Phase = 1;
                run.T1 = run.Age;
                run.Done = seeded;
                float stand = cast.Spec.Seconds;
                for (int i = 0; i < seeded; i++)
                {
                    var set = SigKit.Set(things[i]);
                    if (set != null) set.Hold(stand);
                    cast.Combat.StrikeOne(things[i], cast.Blow(cast.Damage, cast.Stagger), here);
                }
                // The price, taken once the forest has struck so that its own blow wins none of it back: half of what the player has now, left grey.
                run.T3 = cast.Defence.Health.Current * 0.5f;
                cast.Defence.Health.Chip(run.T3);
                for (int i = 0; i < seeded; i++)
                {
                    Vector3 at = SigKit.Foot(things[i], here.y);
                    // Trunks round it, leaning in, and thorns between them.
                    for (int k = 0; k < 4; k++)
                    {
                        Vector3 outward = FormRules.OnRing(k, 4, 1.05f, 45f + i * 31f);
                        FormFx.Erupt(FormMesh.Pillar, at + outward, FormKit.Yaw(-outward) * Quaternion.Euler(14f, 0f, 0f),
                            new Vector3(0.55f, 3.4f + 0.7f * (k % 2), 0.55f), cast.Colour, 0.18f + 0.04f * k, stand - 0.4f, 0.5f);
                    }
                    SigKit.Crown(at, 1.6f, 7, 1.1f, cast.Colour, stand - 0.5f, i * 17f);
                    FormFx.Lay(FormMark.Sigil, at, 3.6f, FormKit.Lit(cast.Colour, 2.4f), stand, -1f, 40f);
                    FormFx.Lay(FormMark.Ring, at, 0.6f, FormKit.Lit(cast.Colour, 3.4f), 0.4f, 4.6f);
                    FormFx.Run(FormMark.Streak, here, at - here, (at - here).magnitude, 0.7f, FormKit.Lit(cast.Colour, 2.6f), 0.35f, 0.15f);
                    ElementLook.Puff(cast.Element, at + Vector3.up * 0.5f, Vector3.up, 1.2f);
                }
                FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 3.6f), 0.5f, cast.Reach * 2f);
                FormFx.Lay(FormMark.Crack, here, 5f, FormKit.Lit(cast.Colour, 2f), 1.6f);
                cast.Defence.Callout(cast.Name);
                SpellWorks.Gesture(cast, FormKit.Brace, 0.35f, 0f, 0.9f);
                SigKit.Moment(cast, 0.12f, 0.6f, 0.45f, 0.4f, 0.35f);
                return true;
            }
            if (run.T2 >= 1f && run.T0 < run.T3)
            {
                // What was given comes back as the held are struck, never more than was given.
                float back = Mathf.Min(run.T2, run.T3 - run.T0);
                run.T0 += back;
                run.T2 = 0f;
                cast.Defence.Heal(back, true);
                ElementLook.Sparks(cast.Element, here + Vector3.up * 1.2f, Vector3.up, 0.6f);
            }
            return run.Age < run.T1 + cast.Spec.Seconds;
        }

        private const int NotesKept = SpellWorks.NotesEach;
    }
}
