using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Fire's gift, Falling Sun: the sun's road. When the surge is lit a small sun rises at the player's feet and a
    /// scarab of embers rolls it along the ground to the enemy faced, the ground behind it burning (dawn). A falcon of
    /// fire then stoops, takes it and carries it up to hang over that enemy, and every second the burns near the
    /// player deepen (noon). When the surge ends, or the surge button is pressed again, the falcon lets go and it sets
    /// on what is under it, heavier for every stack of burn it finds (dusk). Called while still being rolled it bursts
    /// at half weight. A blow taken sends it under with no blow of its own; if the surge has time left it rises again
    /// at the player's feet and begins from dawn. The sums are in <see cref="SunRules"/>.
    /// Reads: Damage, Stagger, Radius, LingerDamage.
    /// </summary>
    public class SunForm : RunningForm
    {
        public override string Id => SpellForms.Sun;

        private const int Dawn = 0, Noon = 1, Dusk = 2, Under = 3;
        private const float LiftSeconds = 0.9f;
        private static readonly Color Gold = new Color(1f, 0.82f, 0.36f);
        // The falcon is the red of the fire's heart, so it stands out against the gold it carries.
        private static Color Wing(Color fire) => Color.Lerp(fire, new Color(0.95f, 0.2f, 0.04f), 0.6f);
        private static readonly Color EmberDark = new Color(0.5f, 0.13f, 0.03f);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            solids[0] = FormFx.Hold(FormMesh.Orb, Color.Lerp(cast.Colour, Gold, 0.45f), 0.08f);
            solids[1] = FormFx.Hold(FormMesh.Scarab, EmberDark, 0.03f);
            solids[2] = FormFx.Hold(FormMesh.Falcon, Wing(cast.Colour), 0.045f);
            marks[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.6f));
            marks[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.4f));
            marks[2] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(Gold, 3.2f));
            marks[3] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 4f));
            marks[4] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(cast.Colour, 1.8f, 0.7f));
            Rise(ref run);
            return 0;
        }

        // A new dawn: the sun comes up out of the ground at the player's feet.
        private static void Rise(ref SpellRun run)
        {
            var cast = run.Cast;
            Vector3 here = cast.Player != null ? cast.Player.transform.position : cast.Origin;
            Vector3 forward = cast.Player != null ? cast.Player.transform.forward : cast.Forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            run.Phase = Dawn;
            run.T1 = run.Age;
            run.T0 = 0f;
            run.T2 = 0f;
            run.T3 = 1f;
            run.Flag = false;
            run.Count = cast.Defence != null ? cast.Defence.BlowsTaken : 0;
            run.Done = cast.Upgrades != null ? cast.Upgrades.SurgeAgain : 0;
            run.A = here + forward * 0.9f;
            run.B = here + forward * SunRules.DawnReach;
            run.C = forward;
            SpellWorks.Times(run)[0] = 0f;
            SpellWorks.Points(run)[0] = run.A;
            FormKit.Gather(cast, run.A, 2f);
            // First light: long low rays along the ground the way it will go.
            for (int i = -2; i <= 2; i++)
                FormFx.Run(FormMark.Streak, run.A, Quaternion.Euler(0f, i * 13f, 0f) * forward, 7f - Mathf.Abs(i), 0.5f, FormKit.Lit(Gold, 2f, 0.7f), 0.7f, 0.04f);
            FormFx.Lay(FormMark.Ring, run.A, 0.6f, FormKit.Lit(Gold, 3f), 0.45f, 4.5f);
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            switch (run.Phase)
            {
                case Dawn: return TickDawn(ref run, deltaTime);
                case Noon: return TickNoon(ref run, deltaTime);
                case Dusk: return TickDusk(ref run);
                default: return TickUnder(ref run);
            }
        }

        private static bool Struck(in SpellRun run) => run.Cast.Defence != null && run.Cast.Defence.BlowsTaken != run.Count;

        private static bool Called(in SpellRun run) => run.Cast.Upgrades != null && run.Cast.Upgrades.SurgeAgain != run.Done;

        // Where the enemy faced stands, asked a few times a second; with no one faced, a point ahead of the player.
        private static void Aim(ref SpellRun run, float deltaTime, float ahead)
        {
            run.T2 -= deltaTime;
            if (run.T2 > 0f) return;
            run.T2 = 0.2f;
            var cast = run.Cast;
            Vector3 here = cast.Player.transform.position;
            var faced = cast.Combat.EnemyAhead(SunRules.DawnReach);
            run.B = faced != null ? faced.position : here + cast.Player.transform.forward * ahead;
            run.B.y = here.y;
        }

        private static void Halo(in SpellRun run, Vector3 sun, float size, float turn)
        {
            var marks = SpellWorks.Marks(run);
            FormFx.PlaceMarkFacing(marks[2], sun, size * 4.2f, turn);
            FormFx.PlaceMarkFacing(marks[3], sun, size * 2.7f);
            FormFx.PlaceMarkFacing(marks[4], sun, size * 6.2f, -turn * 0.6f + 22f);
        }

        // ---- Dawn: rolled along the ground by the scarab, the ground behind it burning.

        private static bool TickDawn(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            var times = SpellWorks.Times(run);
            var points = SpellWorks.Points(run);
            if (cast.Upgrades != null) cast.Upgrades.ListenForSurge();
            Aim(ref run, deltaTime, SunRules.DawnReach);

            float seconds = run.Age - run.T1;
            Vector3 to = run.B - run.A;
            to.y = 0f;
            float gap = to.magnitude;
            if (gap > 0.05f) run.C = Vector3.RotateTowards(run.C, to / gap, 4f * deltaTime, 0f).normalized;
            float step = seconds > 0.25f ? Mathf.Min(SunRules.DawnSpeed * deltaTime, Mathf.Max(0f, gap - SunRules.DawnTouch * 0.8f)) : 0f;
            run.A += run.C * step;
            times[0] += step;

            float up = Mathf.Clamp01(seconds / 0.25f);
            float size = SunRules.DawnSize * (0.3f + 0.7f * up) * (1f + 0.03f * Mathf.Sin(run.Age * 12f));
            Vector3 side = Vector3.Cross(Vector3.up, run.C);
            Vector3 sun = run.A + Vector3.up * (size * 0.5f * (2f * up - 1f) + 0.05f);
            // It turns as a thing rolled turns: once round for every length of its rim.
            var rolled = Quaternion.AngleAxis(times[0] / (Mathf.PI * SunRules.DawnSize) * 360f, side);
            FormFx.Place(solids[0], sun, rolled * FormKit.Yaw(run.C), Vector3.one * size);
            FormFx.Heat(solids[0], 0.1f + 0.08f * Mathf.Sin(run.Age * 14f));
            // The scarab behind it, forelegs on the sun, rocking as it walks.
            Vector3 beetle = run.A - run.C * (size * 0.5f + 0.5f);
            FormFx.Place(solids[1], beetle, FormKit.Yaw(run.C) * Quaternion.Euler(-14f + 5f * Mathf.Sin(run.Age * 26f), 0f, 6f * Mathf.Sin(run.Age * 13f)), Vector3.one * (1.15f * up));
            FormFx.Heat(solids[1], 0.12f + 0.1f * Mathf.Sin(run.Age * 20f));
            FormFx.Place(solids[2], sun, Quaternion.identity, Vector3.zero);
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(size * 2.6f, 1f, size * 2.6f));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 120f, 0f), new Vector3(size * 1.8f, 1f, size * 1.8f));
            Halo(run, sun, size * 0.8f, run.Age * 40f);

            // The ground it crosses burns.
            if ((run.A - points[0]).sqrMagnitude >= 1f)
            {
                Vector3 patch = Vector3.Lerp(points[0], run.A, 0.5f);
                points[0] = run.A;
                var spec = cast.Ground(patch, SunRules.TrailHalfWidth, SunRules.TrailSeconds, SunRules.TrailEvery, cast.Blow(SunRules.TrailDamage, 0f, 1));
                Hazards.Lay(spec);
                FormFx.Run(FormMark.Lane, patch - run.C * 0.6f, run.C, 1.2f, SunRules.TrailHalfWidth * 2f, FormKit.Lit(cast.Colour, 2f, 0.8f), 0.5f, 0.3f);
                for (int i = -1; i <= 1; i++)
                    FormFx.Erupt(FormMesh.Flame, patch + side * (i * 0.5f) - run.C * (0.2f * (i + 1)), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.7f, 1f + 0.5f * Random.value, 0.7f), cast.Colour, 0.08f, 0.5f, 0.4f);
                ElementLook.Sparks(cast.Element, patch + Vector3.up * 0.3f, Vector3.up, 0.6f);
            }

            // The sun itself strikes the first enemy it meets.
            if (!run.Flag && up >= 1f)
            {
                var seen = SpellWorks.Once(run);
                seen.Clear();
                var met = cast.Combat.EnemyNear(run.A, size * 0.5f + 0.7f, seen);
                if (met != null)
                {
                    run.Flag = true;
                    cast.Combat.StrikeOne(met, cast.Blow(SunRules.DawnDamage, SunRules.DawnStagger, SunRules.DawnStacks), run.A - run.C);
                    FormKit.Impact(cast, run.A + run.C * 0.5f, 1.2f, 0.7f);
                }
            }

            if (Struck(run)) return GoUnder(ref run, sun, size);
            if (Called(run) || !SigKit.SurgeOn(cast))
            {
                // Called down before it was ever lifted: it bursts where it is at half weight.
                var blow = cast.Blow(cast.Damage * SunRules.CalledShare, cast.Stagger * SunRules.CalledShare);
                blow.Heavy = true;
                cast.Combat.StrikeArea(run.A, SunRules.CalledRadius, blow);
                FormKit.Impact(cast, run.A, SunRules.CalledRadius, 1.3f);
                SigKit.Crown(run.A, SunRules.CalledRadius * 0.6f, 8, 1.6f, cast.Colour, 0.4f);
                FormFx.Lay(FormMark.Star, run.A, 1.5f, FormKit.Lit(Gold, 3.4f), 0.3f, SunRules.CalledRadius * 4f);
                Hazards.Lay(cast.Ground(run.A, SunRules.CalledRadius * 0.6f, 4f, 0.5f, cast.Linger(cast.Spec.LingerDamage)));
                SigKit.Moment(cast, 0.1f, 0.5f, 0f, 1f, 0.3f);
                return false;
            }
            if (SunRules.DawnOver(seconds, times[0], gap))
            {
                run.Phase = Noon;
                run.T1 = run.Age;
                run.T0 = 0f;
                // The scarab's work is done: it burrows where it stands.
                FormFx.Release(solids[1], 0.35f);
                solids[1] = -1;
                ElementLook.Puff(cast.Element, beetle + Vector3.up * 0.2f, Vector3.up, 0.8f);
            }
            return true;
        }

        // ---- Noon: the falcon stoops, takes it, carries it up, and it hangs over the enemy faced.

        private static bool TickNoon(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            Vector3 here = cast.Player.transform.position;
            if (cast.Upgrades != null) cast.Upgrades.ListenForSurge();
            Aim(ref run, deltaTime, 3f);
            float seconds = run.Age - run.T1;
            float lifted = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((seconds - 0.28f) / (LiftSeconds - 0.28f)));
            if (lifted > 0f) run.A = Vector3.Lerp(run.A, run.B, 1f - Mathf.Exp(-5f * deltaTime));

            float size = Mathf.Lerp(SunRules.DawnSize, SunRules.NoonSize, lifted) * (1f + 0.04f * Mathf.Sin(run.Age * 9f));
            Vector3 sun = run.A + Vector3.up * Mathf.Lerp(size * 0.5f + 0.05f, SunRules.Height, lifted);
            FormFx.Place(solids[0], sun, Quaternion.Euler(run.Age * 40f, run.Age * 70f, 0f), Vector3.one * size);
            FormFx.Heat(solids[0], 0.14f + 0.1f * Mathf.Sin(run.Age * 11f) + 0.2f * lifted * (1f - lifted) * 4f);

            // The falcon: down out of the sky onto it, then beating upward with it in its claws, then holding it there.
            float stoop = Mathf.Clamp01(seconds / 0.28f);
            Vector3 grip = sun + Vector3.up * (size * 0.5f + 0.5f);
            Vector3 from = sun + Vector3.up * 6.5f - run.C * 5f;
            Vector3 bird = Vector3.Lerp(from, grip, stoop * stoop);
            float beat = Mathf.Sin(run.Age * (lifted < 1f ? 22f : 9f));
            var turned = stoop < 1f
                ? Quaternion.LookRotation((grip - from).normalized, Vector3.up)
                : FormKit.Yaw(run.C) * Quaternion.Euler(-18f * (1f - lifted) * lifted * 4f, 0f, 0f);
            FormFx.Place(solids[2], bird + Vector3.up * (0.08f * beat), turned, new Vector3(2.3f * (1f + 0.1f * beat), 2.3f * (1f + 0.5f * beat), 2.3f));
            FormFx.Heat(solids[2], 0.03f + 0.03f * beat);
            if (stoop < 1f) FormFx.Cut(bird, from - grip, 2.4f, 0.5f, FormKit.Lit(Gold, 2.6f, 0.8f), 0.07f, 0.05f);

            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f) * Mathf.Max(0.3f, lifted));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 90f, 0f), new Vector3(radius * 1.4f, 1f, radius * 1.4f) * Mathf.Max(0.3f, lifted));
            Halo(run, sun, size, run.Age * 30f);

            if (lifted >= 1f)
            {
                run.T0 += deltaTime;
                if (run.T0 >= 1f)
                {
                    run.T0 -= 1f;
                    cast.Combat.AfflictArea(here, SunRules.BurnReach, AfflictionKind.Burn, 1);
                    FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 2.2f), 0.4f, SunRules.BurnReach * 2f);
                    // Noon light: rays let down from it onto the ground under it.
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 foot = run.A + FormRules.OnRing(i, 5, radius * 0.75f, run.Age * 50f);
                        FormFx.Cut((sun + foot) * 0.5f, foot - sun, (foot - sun).magnitude, 0.34f, FormKit.Lit(Gold, 1.8f, 0.55f), 0.45f, 0.05f);
                    }
                    ElementLook.Puff(cast.Element, sun - Vector3.up * size, Vector3.down, 0.9f);
                }
            }

            if (Struck(run)) return GoUnder(ref run, sun, size);
            if (!Called(run) && SigKit.SurgeOn(cast)) return true;

            // Dusk: the falcon lets go.
            run.Phase = Dusk;
            run.T1 = run.Age;
            run.T0 = sun.y - run.A.y;
            int near = SigKit.Gather(run, run.A, radius + 2f, 12);
            int stacks = 0;
            var things = SpellWorks.Things(run);
            for (int i = 0; i < near; i++) stacks += SigKit.Depth(things[i], AfflictionKind.Burn);
            run.T3 = SunRules.Weight(stacks);
            FormFx.Release(solids[2]);
            solids[2] = -1;
            FormFx.Fly(FormMesh.Falcon, bird, bird + Vector3.up * 5f + run.C * 7f, Quaternion.LookRotation(run.C + Vector3.up * 0.6f), Vector3.one * 2.3f, Wing(cast.Colour), 0.7f);
            FormFx.Lay(FormMark.Ring, run.A, radius * 5f, FormKit.Lit(cast.Colour, 3.4f), SunRules.FallSeconds, radius * 2f);
            return true;
        }

        // ---- Dusk: it sets.

        private static bool TickDusk(ref SpellRun run)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            float radius = cast.Spec.Radius;
            float t = (run.Age - run.T1) / SunRules.FallSeconds;
            if (t < 1f)
            {
                Vector3 falling = run.A + Vector3.up * Mathf.Lerp(Mathf.Max(1.2f, run.T0), 1.2f, t * t);
                float size = SunRules.NoonSize * (1f + 0.35f * t);
                FormFx.Place(solids[0], falling, Quaternion.Euler(run.Age * 200f, run.Age * 140f, 0f), Vector3.one * size);
                FormFx.Heat(solids[0], t);
                Halo(run, falling, size * 1.1f, run.Age * 90f);
                FormFx.Cut(falling + Vector3.up * 3f, Vector3.up, 6f, 1.2f, FormKit.Lit(cast.Colour, 2.6f), 0.08f, 0.2f);
                return true;
            }
            var blow = cast.Blow(cast.Damage * run.T3, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeArea(run.A, radius, blow);
            FormKit.Impact(cast, run.A, radius, 2f);
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 4f), 0.5f, radius * 4.4f);
            FormFx.Lay(FormMark.Star, run.A, radius * 1.2f, FormKit.Lit(Gold, 3.6f), 0.3f, radius * 4.5f);
            FormFx.Lay(FormMark.Crack, run.A, radius * 3.6f, FormKit.Lit(cast.Colour, 2.8f), 4f, -1f, 0f, 21f);
            // The last light of the day: long rays thrown flat along the ground, every way.
            for (int i = 0; i < 14; i++)
                FormFx.Run(FormMark.Streak, run.A, FormRules.OnRing(i, 14, 1f, 9f).normalized, radius * (1.6f + 0.5f * (i % 3)), 0.55f, FormKit.Lit(Gold, 2.4f, 0.8f), 0.6f + 0.1f * (i % 3), 0.03f);
            SigKit.Crown(run.A, radius * 0.7f, 12, 2.2f, cast.Colour, 0.5f);
            SigKit.Crown(run.A, radius * 0.35f, 6, 3.2f, cast.Colour, 0.4f, 30f);
            for (int i = 0; i < 8; i++)
                FormFx.Erupt(FormMesh.Flame, run.A + FormRules.OnRing(i, 8, radius * 0.95f, 20f), Quaternion.Euler(0f, i * 47f, 0f), new Vector3(1.3f, 2.4f + 0.8f * (i % 2), 1.3f), cast.Colour, 0.08f, 0.7f, 0.5f);
            SigKit.Column(run.A, FormKit.Lit(cast.Colour, 3.4f), 3.2f, 0.25f);
            Hazards.Lay(cast.Ground(run.A, radius * 0.8f, 4f, 0.5f, cast.Linger(cast.Spec.LingerDamage)));
            SigKit.Moment(cast, 0.2f, 0.9f, 0.5f, 0.35f, 0.5f);
            return false;
        }

        // ---- A blow taken: it goes under, and rises again if the surge has time left.

        private static bool GoUnder(ref SpellRun run, Vector3 sun, float size)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            run.Flag2 = cast.Caster != null && cast.Caster.Kit != null && SunRules.RisesAgain(cast.Caster.Kit.SurgeLeft);
            cast.Defence.Callout("The sun goes under");
            for (int i = 0; i < 5; i++) ElementLook.Puff(cast.Element, sun + FormRules.OnRing(i, 5, 1f), Vector3.down, 1f);
            FormFx.Lay(FormMark.Ring, run.A, size * 4f, FormKit.Lit(cast.Colour, 2f), 0.4f, 0.4f);
            FormFx.Release(solids[1], 0.25f);
            FormFx.Release(solids[2], 0.25f);
            solids[1] = solids[2] = -1;
            run.Phase = Under;
            run.T1 = run.Age;
            run.C = sun;
            run.T0 = size;
            return true;
        }

        private static bool TickUnder(ref SpellRun run)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float seconds = run.Age - run.T1;
            // It sinks straight down into the ground where it was, and its light closes over it.
            float sunk = Mathf.Clamp01(seconds / 0.4f);
            Vector3 at = run.C;
            at.y = Mathf.Lerp(run.C.y, run.A.y - run.T0, sunk * sunk);
            float size = run.T0 * (1f - 0.5f * sunk);
            FormFx.Place(solids[0], at, Quaternion.Euler(run.Age * 30f, run.Age * 50f, 0f), sunk < 1f ? Vector3.one * size : Vector3.zero);
            float light = 1f - sunk;
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(2f, 1f, 2f) * light);
            FormFx.PlaceMark(marks[1], run.A, Quaternion.identity, Vector3.zero);
            FormFx.PlaceMarkFacing(marks[2], at, size * 4f * light);
            FormFx.PlaceMarkFacing(marks[3], at, size * 2.6f * light);
            FormFx.PlaceMarkFacing(marks[4], at, 0f);
            if (!run.Flag2) return sunk < 1f;
            if (!SigKit.SurgeOn(cast)) return false;
            if (seconds < SunRules.UnderSeconds) return true;
            solids[1] = FormFx.Hold(FormMesh.Scarab, EmberDark, 0.03f);
            solids[2] = FormFx.Hold(FormMesh.Falcon, Wing(cast.Colour), 0.045f);
            Rise(ref run);
            return true;
        }
    }
}
