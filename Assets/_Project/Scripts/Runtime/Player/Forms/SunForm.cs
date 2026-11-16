using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Fire's gift, Falling Sun. A sun rises over the fight when the surge is lit and hangs over whoever the player
    /// faces; every second it deepens the burns near the player. When the surge ends, or the surge button is pressed
    /// again, it falls, heavier for every stack of burn under it. A blow taken puts it out.
    /// Reads: Damage, Stagger, Radius, LingerDamage.
    /// </summary>
    public class SunForm : RunningForm
    {
        public override string Id => SpellForms.Sun;

        private const float Height = 3.8f;
        private const float FallSeconds = 0.32f;
        private const float BurnReach = 6f;
        private const float Size = 1.35f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Count = cast.Defence != null ? cast.Defence.BlowsTaken : 0;
            run.Done = cast.Upgrades != null ? cast.Upgrades.SurgeAgain : 0;
            run.A = cast.Origin + cast.Forward * 3f;
            run.T3 = 1f;
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, cast.Colour, 0.09f);
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.6f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.4f));
            SpellWorks.Marks(run)[2] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(cast.Colour, 3.2f));
            SpellWorks.Marks(run)[3] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 4f));
            FormKit.Gather(cast, cast.Origin, 2f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float radius = cast.Spec.Radius;
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                // It hangs over whoever the player faces; asked a few times a second, not every frame.
                run.T2 -= deltaTime;
                if (run.T2 <= 0f)
                {
                    run.T2 = 0.2f;
                    var faced = cast.Combat.EnemyAhead(9f);
                    run.B = faced != null ? faced.position : here + cast.Player.transform.forward * 3f;
                    run.B.y = here.y;
                }
                run.A = Vector3.Lerp(run.A, run.B, 1f - Mathf.Exp(-5f * deltaTime));
                float grown = Mathf.Clamp01(run.Age / 1.2f);
                float size = Mathf.Lerp(0.4f, Size, grown) * (1f + 0.04f * Mathf.Sin(run.Age * 9f));
                Vector3 sun = run.A + Vector3.up * Height;
                FormFx.Place(solids[0], sun, Quaternion.Euler(run.Age * 40f, run.Age * 70f, 0f), Vector3.one * size);
                FormFx.Heat(solids[0], 0.14f + 0.1f * Mathf.Sin(run.Age * 11f));
                FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
                FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, run.Age * 90f, 0f), new Vector3(radius * 1.4f, 1f, radius * 1.4f));
                FormFx.PlaceMarkFacing(marks[2], sun, size * 4.2f, run.Age * 30f);
                FormFx.PlaceMarkFacing(marks[3], sun, size * 2.7f);
                if (cast.Upgrades != null) cast.Upgrades.ListenForSurge();
                run.T0 += deltaTime;
                if (run.T0 >= 1f)
                {
                    run.T0 -= 1f;
                    cast.Combat.AfflictArea(here, BurnReach, AfflictionKind.Burn, 1);
                    FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 2.2f), 0.4f, BurnReach * 2f);
                    ElementLook.Puff(cast.Element, sun - Vector3.up * size, Vector3.down, 0.9f);
                }
                if (cast.Defence != null && cast.Defence.BlowsTaken != run.Count)
                {
                    // Put out: it gutters where it hangs.
                    cast.Defence.Callout("The sun goes out");
                    for (int i = 0; i < 5; i++) ElementLook.Puff(cast.Element, sun + FormRules.OnRing(i, 5, 1f), Vector3.down, 1f);
                    return false;
                }
                bool again = cast.Upgrades != null && cast.Upgrades.SurgeAgain != run.Done;
                if (!again && SigKit.SurgeOn(cast)) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                int near = SigKit.Gather(run, run.A, radius + 2f, 12);
                int stacks = 0;
                var things = SpellWorks.Things(run);
                for (int i = 0; i < near; i++) stacks += SigKit.Depth(things[i], AfflictionKind.Burn);
                run.T3 = 1f + Mathf.Min(1f, 0.08f * stacks);
                FormFx.Lay(FormMark.Ring, run.A, radius * 5f, FormKit.Lit(cast.Colour, 3.4f), FallSeconds, radius * 2f);
                return true;
            }
            float t = (run.Age - run.T1) / FallSeconds;
            if (t < 1f)
            {
                Vector3 falling = run.A + Vector3.up * Mathf.Lerp(Height, 1.2f, t * t);
                float size = Size * (1f + 0.35f * t);
                FormFx.Place(solids[0], falling, Quaternion.Euler(run.Age * 200f, run.Age * 140f, 0f), Vector3.one * size);
                FormFx.Heat(solids[0], t);
                FormFx.PlaceMarkFacing(marks[2], falling, size * 4.6f, run.Age * 90f);
                FormFx.PlaceMarkFacing(marks[3], falling, size * 2.9f);
                FormFx.Cut(falling + Vector3.up * 3f, Vector3.up, 6f, 1.2f, FormKit.Lit(cast.Colour, 2.6f), 0.08f, 0.2f);
                return true;
            }
            var blow = cast.Blow(cast.Damage * run.T3, cast.Stagger);
            blow.Heavy = true;
            cast.Combat.StrikeArea(run.A, radius, blow);
            FormKit.Impact(cast, run.A, radius, 2f);
            FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 4f), 0.5f, radius * 4.4f);
            FormFx.Lay(FormMark.Star, run.A, radius * 1.2f, FormKit.Lit(cast.Colour, 3.6f), 0.3f, radius * 4.5f);
            FormFx.Lay(FormMark.Crack, run.A, radius * 3.6f, FormKit.Lit(cast.Colour, 2.8f), 4f, -1f, 0f, 21f);
            SigKit.Crown(run.A, radius * 0.7f, 12, 2.2f, cast.Colour, 0.5f);
            SigKit.Crown(run.A, radius * 0.35f, 6, 3.2f, cast.Colour, 0.4f, 30f);
            SigKit.Column(run.A, FormKit.Lit(cast.Colour, 3.4f), 3.2f, 0.25f);
            Hazards.Lay(cast.Ground(run.A, radius * 0.8f, 4f, 0.5f, cast.Linger(cast.Spec.LingerDamage)));
            SigKit.Moment(cast, 0.2f, 0.9f, 0.5f, 0.35f, 0.5f);
            return false;
        }
    }
}
