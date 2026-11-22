using Clover.Core;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>The plain rules of a turning blade (Returning Edge): its bats and its catch.</summary>
    public static class TurningBladeRules
    {
        /// <summary>Bat It Home: each bat sends it out at 1.4 times the speed and 1.5 times the damage of its first flight.</summary>
        public const float BatSpeed = 1.4f, BatDamage = 1.5f;
        /// <summary>It is at the hand, for a guard to meet, within this of it.</summary>
        public const float AtHand = 0.7f;

        /// <summary>The speed and damage share of a flight after <paramref name="bats"/> bats.</summary>
        public static float SpeedAfter(int bats) => bats > 0 ? BatSpeed : 1f;
        public static float DamageAfter(int bats) => bats > 0 ? BatDamage : 1f;

        /// <summary>True when a blade arriving at the hand may be batted out again: a perfect guard, and bats left.</summary>
        public static bool Bats(bool perfectGuard, int bats, int most) => perfectGuard && bats < most;

        /// <summary>True when a waiting blade is called home: a dodge begun since it stopped (not one already rolling as it stopped).</summary>
        public static bool CalledByDodge(bool rollingNow, bool rollingBefore) => rollingNow && !rollingBefore;
    }

    /// <summary>
    /// Returning Edge: a turning blade a metre and a half across flies out along your facing and back to your hand, cutting
    /// each enemy it passes each way, and laying the spell's notch on the way out. Its arrival at the hand is a moment a
    /// perfect guard can meet: the catch (UpgradeTrigger.BladeCaught), and with Bat It Home (SpellSpec.BatsBack) the blade
    /// sent out again faster and harder. With Through the Line (SpellSpec.WaitsOut) it stops at the end of its flight and
    /// spins there, biting what touches it, until your next dodge calls it home through everything between; uncalled, it falls.
    /// </summary>
    public class TurningBladeForm : RunningForm
    {
        public override string Id => SpellForms.TurningBlade;

        private const float Hand = 1.1f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            Begin(ref run, cast.Origin, cast.Forward);
            run.Done = 0;
            var solids = SpellWorks.Solids(run);
            solids[0] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.04f);
            solids[1] = FormFx.Hold(FormMesh.Blade, cast.Colour, 0.04f);
            SpellWorks.Gesture(cast, FormKit.Chop, 0.3f, 0.2f, 0.9f, true);
            return 0;
        }

        // Out from where the caster stands, along the way given: the run's A is the start, B the way, T0 the length, T1 when it set out.
        private static void Begin(ref SpellRun run, Vector3 from, Vector3 forward)
        {
            forward.y = 0f;
            run.A = from;
            run.B = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            run.C = from + Vector3.up * Hand;
            run.T0 = Mathf.Max(1f, run.Cast.Reach);
            run.T1 = run.Age;
            run.T3 = 0f;
            run.Phase = 0;
            SpellWorks.Once(run).Clear();
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            if (cast.Player == null) return false;
            var spec = cast.Spec;
            float speed = Mathf.Max(4f, spec.Speed) * TurningBladeRules.SpeedAfter(run.Done);
            float damage = cast.Damage * TurningBladeRules.DamageAfter(run.Done);
            float radius = Mathf.Max(0.5f, spec.Radius);
            Vector3 hand = cast.Player.transform.position + Vector3.up * Hand;
            bool rolling = cast.Player.Dodge.IsRolling;
            if (run.Phase == 0)
            {
                // Out along the line, cutting each enemy it passes once, and laying the notch.
                float gone = Mathf.Min(run.T0, (run.Age - run.T1) * speed);
                Cut(ref run, run.A + run.B * run.T3, run.B, gone - run.T3, radius, cast.Blow(damage, cast.Stagger));
                run.T3 = gone;
                run.C = run.A + run.B * gone + Vector3.up * Hand;
                Show(run, run.C, 1f);
                if (gone < run.T0) return true;
                SpellWorks.Once(run).Clear();
                if (spec.WaitsOut)
                {
                    run.Phase = 1;
                    run.T1 = run.Age;
                    run.T2 = run.Age;
                    run.Flag2 = rolling;
                    return true;
                }
                run.Phase = 2;
                return true;
            }
            if (run.Phase == 1)
            {
                // Through the Line: it stops and spins in place, biting what touches it, until a dodge calls it home.
                Show(run, run.C, 1.15f);
                if (run.Age - run.T2 >= Mathf.Max(0.1f, spec.Interval))
                {
                    run.T2 = run.Age;
                    Vector3 ground = new Vector3(run.C.x, run.A.y, run.C.z);
                    cast.Combat.StrikeArea(ground, radius, cast.Blow(spec.LingerDamage > 0f ? spec.LingerDamage : damage * 0.5f, 0f, 0));
                }
                bool called = TurningBladeRules.CalledByDodge(rolling, run.Flag2);
                run.Flag2 = rolling;
                if (called)
                {
                    // Home through everything between it and you, at once along the whole line, as the call is made.
                    Vector3 from = new Vector3(run.C.x, run.A.y, run.C.z);
                    Vector3 line = cast.Player.transform.position - from;
                    line.y = 0f;
                    float home = spec.HomeDamage > 0f ? spec.HomeDamage * (cast.Damage / Mathf.Max(0.01f, spec.Damage)) : damage;
                    if (line.sqrMagnitude > 0.04f)
                        cast.Combat.StrikeLane(from, line.normalized, line.magnitude, radius, cast.Blow(home, spec.HomeStagger > 0f ? spec.HomeStagger : cast.Stagger));
                    FormFx.Cut(Vector3.Lerp(run.C, hand, 0.5f), line, line.magnitude, 0.4f, FormKit.Lit(cast.Colour, 3.4f), 0.2f);
                    SpellWorks.Once(run).Clear();
                    run.Flag = true;
                    run.Phase = 2;
                    return true;
                }
                if (run.Age - run.T1 < Mathf.Max(0.5f, spec.Seconds)) return true;
                // Never called: it falls where it spun, and is gone.
                ElementLook.Sparks(cast.Element, run.C, Vector3.down, 0.6f);
                return false;
            }
            // Home to the hand, wherever the caster is now, cutting each enemy it passes on the way back (no notch: that was the way out).
            Vector3 toHand = hand - run.C;
            float step = speed * 1.15f * deltaTime;
            if (toHand.magnitude > TurningBladeRules.AtHand)
            {
                Vector3 next = run.C + Vector3.ClampMagnitude(toHand, step);
                // A blade called home by a dodge has already cut its line.
                if (!run.Flag)
                {
                    Vector3 way = next - run.C;
                    way.y = 0f;
                    if (way.sqrMagnitude > 0.0001f)
                        Cut(ref run, new Vector3(run.C.x, run.A.y, run.C.z), way.normalized, way.magnitude, radius, cast.Blow(damage, cast.Stagger, 0));
                }
                run.C = next;
                Show(run, run.C, 1f);
                return true;
            }
            // At the hand: a perfect guard meets it.
            bool met = cast.Defence != null && cast.Defence.Guard.IsPerfect;
            if (met)
            {
                if (cast.Upgrades != null) cast.Upgrades.OnBladeCaught(hand);
                Vfx.Play(VfxKind.Clang, hand, cast.Player.transform.forward, 1f);
            }
            if (TurningBladeRules.Bats(met, run.Done, spec.BatsBack))
            {
                // Bat It Home: out again along the way the caster faces now, faster and harder.
                run.Done++;
                run.Flag = false;
                Begin(ref run, cast.Player.transform.position, cast.Player.transform.forward);
                return true;
            }
            Vfx.Play(VfxKind.GuardSpark, hand, Vector3.up, 0.6f);
            return false;
        }

        // The lane cut from a point along a way, each enemy once a flight.
        private static void Cut(ref SpellRun run, Vector3 from, Vector3 way, float length, float radius, in SpellBlow blow)
        {
            if (length <= 0.001f) return;
            from.y = run.A.y;
            run.Cast.Combat.StrikeLane(from, way, length, radius, blow, SpellWorks.Once(run));
        }

        // Two blades crossed and whirling flat: a turning blade a metre and a half across.
        private static void Show(in SpellRun run, Vector3 at, float size)
        {
            var solids = SpellWorks.Solids(run);
            Quaternion spin = Quaternion.AngleAxis(run.Age * 1440f, Vector3.up);
            FormFx.Place(solids[0], at, spin * Quaternion.LookRotation(Vector3.forward), Vector3.one * (0.75f * size));
            FormFx.Place(solids[1], at, spin * Quaternion.LookRotation(Vector3.right), Vector3.one * (0.75f * size));
        }
    }
}
