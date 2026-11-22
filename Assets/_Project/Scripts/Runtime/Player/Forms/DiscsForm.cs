using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Toothed discs that circle the player at arm's length for a while, each biting what it passes at intervals.
    /// When the time is up they fly off outward.
    /// </summary>
    public class DiscsForm : RunningForm
    {
        public override string Id => SpellForms.Discs;

        private const int Most = 8;
        private const float Size = 0.72f;
        private const float TurnDegrees = 230f;

        public override int Cast(in SpellCast cast)
        {
            // One halo at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, Most);
            run.T1 = 0.25f;
            // Which discs still turn, one bit each; the flings waiting, and on whom.
            run.Phase = (1 << run.Count) - 1;
            run.Done = 0;
            run.Victim = null;
            var solids = SpellWorks.Solids(run);
            for (int i = 0; i < run.Count; i++) solids[i] = FormFx.Hold(FormMesh.Disc, cast.Colour, 0.04f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 1.3f));
            FormKit.Gather(cast, cast.Origin, 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.28f);
            return 0;
        }

        /// <summary>The discs a fling sends: one for a blow, all for a heavy blow.</summary>
        public static int FlingsFor(bool heavy) => heavy ? Most : 1;

        /// <summary>The disc of those still turning (<paramref name="turning"/>, a bit each) nearest a point, given where each is; -1 for none.</summary>
        public static int Nearest(int turning, System.ReadOnlySpan<Vector3> at, int count, Vector3 to)
        {
            int best = -1;
            float bestApart = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if ((turning & (1 << i)) == 0) continue;
                float apart = (at[i] - to).sqrMagnitude;
                if (apart < bestApart)
                {
                    bestApart = apart;
                    best = i;
                }
            }
            return best;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // A weapon blow flings a disc into what it struck (Glass Storm); thrown in the next step.
            if (run.Cast.Spec.FlingDamage <= 0f || target == null) return;
            run.Victim = target;
            run.Done = Mathf.Min(Most, run.Done + FlingsFor(step != null && step.Heavy));
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 centre = cast.Player.transform.position;
            float radius = cast.Spec.Radius;
            float formed = Mathf.Clamp01(run.Age / 0.2f);
            // The flings waiting: the nearest turning disc into the enemy struck, each, until none is left.
            if (run.Done > 0)
            {
                var victim = run.Victim;
                System.Span<Vector3> discAt = stackalloc Vector3[Most];
                for (int i = 0; i < run.Count; i++)
                    discAt[i] = centre + FormRules.OnRing(i, run.Count, 1f, run.Age * TurnDegrees) * (radius * formed) + Vector3.up;
                while (run.Done > 0 && victim != null && victim.gameObject.activeInHierarchy)
                {
                    run.Done--;
                    int disc = Nearest(run.Phase, discAt, run.Count, victim.transform.position);
                    if (disc < 0) break;
                    run.Phase &= ~(1 << disc);
                    FormFx.Release(solids[disc]);
                    solids[disc] = -1;
                    Vector3 hit = victim.transform.position + Vector3.up;
                    FormFx.Fly(FormMesh.Disc, discAt[disc], hit, Quaternion.LookRotation(hit - discAt[disc] + Vector3.forward * 0.001f), Vector3.one * Size, cast.Colour, 0.12f, 0.04f);
                    cast.Combat.StrikeOne(victim, cast.Blow(cast.Spec.FlingDamage, cast.Stagger), discAt[disc]);
                    cast.Combat.AfflictArea(victim.transform.position, 0.6f, Clover.Gameplay.AfflictionKind.Static, 1);
                    ElementLook.Sparks(cast.Element, hit, (hit - discAt[disc]).normalized, 0.6f);
                }
                run.Done = 0;
                run.Victim = null;
                // Every disc flung: the storm is spent.
                if (run.Phase == 0) return false;
            }
            bool bite = run.Age >= run.T1;
            if (bite)
            {
                run.T1 += Mathf.Max(0.1f, cast.Spec.Interval);
                SpellWorks.Once(run).Clear();
            }
            bool over = run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds);
            for (int i = 0; i < run.Count; i++)
            {
                // A disc already flung turns no more.
                if (cast.Spec.FlingDamage > 0f && (run.Phase & (1 << i)) == 0) continue;
                Vector3 outward = FormRules.OnRing(i, run.Count, 1f, run.Age * TurnDegrees);
                Vector3 at = centre + outward * (radius * formed) + Vector3.up * (1f + 0.1f * Mathf.Sin(run.Age * 6f + i * 2f));
                if (over)
                {
                    FormFx.Release(solids[i]);
                    solids[i] = -1;
                    FormFx.Fly(FormMesh.Disc, at, at + outward * 4.5f, Quaternion.identity, Vector3.one * Size, cast.Colour, 0.28f, 0.04f);
                    continue;
                }
                // Tilted into its turn, spinning fast on its own hub.
                FormFx.Place(solids[i], at, Quaternion.LookRotation(outward) * Quaternion.Euler(0f, 0f, 14f) * Quaternion.Euler(0f, run.Age * 1100f, 0f), Vector3.one * (Size * formed));
                FormFx.Heat(solids[i], formed < 1f ? 1f : 0f);
                if (!bite) continue;
                int hits = cast.Combat.StrikeArea(at, 1f, cast.Blow(cast.Damage, cast.Stagger), SpellWorks.Once(run));
                if (hits <= 0) continue;
                FormFx.Heat(solids[i], 1f);
                FormFx.Cut(at, Vector3.Cross(Vector3.up, outward), 2.4f, 0.3f, FormKit.Lit(cast.Colour, 3.2f), 0.1f);
                ElementLook.Sparks(cast.Element, at, Vector3.Cross(Vector3.up, outward), 0.5f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.08f, 0.1f);
            }
            float across = radius * 2f * formed;
            FormFx.PlaceMark(SpellWorks.Marks(run)[0], centre, Quaternion.identity, new Vector3(across, 1f, across));
            return !over;
        }
    }
}
