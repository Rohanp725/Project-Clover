using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The eleven tap spells, each doing what its substance would do. Fire spreads along the ground and leaves it
    /// burning; lightning is there and gone, jagged, from one body to the next; wind is a rush of streaks and leaves
    /// that throws and lifts; ice grows as crystals that stand until a blow shatters them; water rises as a wave out
    /// ahead and rolls back to the caster, carrying what it reaches to their feet; life grows a bed of thorned vines
    /// that flowers and bites; earth breaks out under an enemy and leaves a wall of rock behind it; metal is a
    /// thrown blade that sticks, is driven in by the weapon and flies back to the hand; light is a ray with a flare
    /// that marks what it crosses; dark is a slow heavy ball that draws everything in, devours and drags; poison is
    /// a cloud that hangs, drifts after the nearest enemy and seeps. The run's <see cref="SpellRun.Count"/> is the
    /// element's place on the ring; A is where the caster stood, B the way they faced, C the point the spell is about.
    /// </summary>
    public class TapForm : RunningForm
    {
        public override string Id => SpellForms.Tap;

        private static readonly Color Petal = new Color(1f, 0.72f, 0.85f);
        private static readonly Color Ember = new Color(1f, 0.86f, 0.4f);
        // The element art's fire has no pale gold in it: its second tongue is the deep red band.
        private static readonly Color FireRed = new Color(0.85f, 0.16f, 0.04f);
        private static readonly Color BoltViolet = new Color(0.45f, 0.2f, 1f);
        // Wind is pale dust, never white, whether the art is loaded or not (the research brief of 2026-11-20).
        private static readonly Color WindPale = ElementSubstance.WindDust;
        private static readonly Color DryLeaf = new Color(0.5f, 0.42f, 0.16f);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            // With nothing to run it in, the spell is still thrown: a plain cone of the element.
            if (!begun) return cast.Combat != null ? cast.Combat.StrikeCone(cast.Reach, Mathf.Max(60f, cast.Spec.Arc), cast.Blow(cast.Damage, cast.Stagger)) : 0;
            run.Count = cast.Element != null ? cast.Element.Index : 0;
            Vector3 forward = cast.Forward;
            forward.y = 0f;
            run.A = cast.Origin;
            run.B = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            run.C = FormKit.AimPoint(cast);
            switch (run.Count)
            {
                case ElementSubstance.Lightning: return BeginLightning(ref run);
                case ElementSubstance.Wind: return BeginWind(ref run);
                case ElementSubstance.Ice: return BeginIce(ref run);
                case ElementSubstance.Water: return BeginWater(ref run);
                case ElementSubstance.Life: return BeginLife(ref run);
                case ElementSubstance.Earth: return BeginEarth(ref run);
                case ElementSubstance.Metal: return BeginMetal(ref run);
                case ElementSubstance.Light: return BeginLight(ref run);
                case ElementSubstance.Dark: return BeginDark(ref run);
                case ElementSubstance.Poison: return BeginPoison(ref run);
                default: return BeginFire(ref run);
            }
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            switch (run.Count)
            {
                case ElementSubstance.Lightning: return TickLightning(ref run);
                case ElementSubstance.Wind: return TickWind(ref run, deltaTime);
                case ElementSubstance.Ice: return TickIce(ref run);
                case ElementSubstance.Water: return TickWater(ref run, deltaTime);
                case ElementSubstance.Life: return TickLife(ref run);
                case ElementSubstance.Earth: return TickEarth(ref run);
                case ElementSubstance.Metal: return TickMetal(ref run);
                case ElementSubstance.Light: return TickLight(ref run);
                case ElementSubstance.Dark: return TickDark(ref run, deltaTime);
                case ElementSubstance.Poison: return TickPoison(ref run, deltaTime);
                default: return TickFire(ref run, deltaTime);
            }
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // Noted here, acted on in the next step: the swing is still being reckoned.
            if (run.Count == ElementSubstance.Ice && run.Age > 0.4f && target != null)
            {
                Vector3 off = target.transform.position - (run.A + run.B * (run.Cast.Reach * 0.55f));
                off.y = 0f;
                if (off.magnitude <= run.Cast.Reach * 0.6f + 1f) run.Flag = true;
            }
            if (run.Count == ElementSubstance.Metal && target != null && ReferenceEquals(target, run.Victim)) run.Flag = true;
        }

        private static Vector3 Hand(in SpellRun run) => run.A + Vector3.up * 1.1f + run.B * 0.5f;

        private static Vector3 Side(in SpellRun run) => Vector3.Cross(Vector3.up, run.B);

        private static float Hash(int n) => ElementSubstance.Hash(n);

        // ---- Fire: Flame Wave. It strikes the cone at once, then flame runs out along the ground row by row and three patches of it stay burning.

        // Where Flame Wave's three burning patches lie in its cone: how far out (share of its reach) and how far round
        // (share of its arc, either side of the middle), and how wide each is.
        private static readonly float[] PatchOut = { 0.42f, 0.78f, 0.78f };
        private static readonly float[] PatchTurn = { 0f, -0.28f, 0.28f };
        private const float PatchRadius = 1.25f;

        private static int BeginFire(ref SpellRun run)
        {
            var cast = run.Cast;
            int hits = cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, cast.Blow(cast.Damage, cast.Stagger));
            // The elements 3 Flame Wave: one rolling front of real flame out over the cone, every one in it catching, in
            // place of the rows of tongues, the hoods and the smoke below (the owner: "the wave hits a cone shape, but the
            // burn is only a straight line"; the coordinator: no rows of identical cones).
            if (ElementArt.Ready && ElementMoves.PlaySpell(ElementSubstance.Fire, run.A, run.B, Mathf.Clamp(cast.Reach / 6f, 0.8f, 1.2f)) >= 0)
            {
                run.Flag2 = true;
                int count = cast.Combat.CopyLastStruck(out var struck);
                for (int i = 0; i < count; i++) ElementMoves.SpellReached(ElementSubstance.Fire, struck[i].transform, run.B);
            }
            // The element art has no arc of light on the ground: fire is flame, never a glow.
            if (cast.Caster != null && !ElementArt.Ready) cast.Caster.ShowArc(cast.Reach, cast.Spec.Arc, 1f, cast.Element != null ? cast.Element.Glow : cast.Colour);
            run.T1 = 0.8f;
            return hits;
        }

        private static bool TickFire(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            const float spread = 0.3f, burns = 3f;
            var patches = SpellWorks.Points(run);
            if (run.Phase == 0)
            {
                float front = cast.Reach * Mathf.Clamp01(run.Age / spread);
                while (!run.Flag2 && run.T1 <= front)
                {
                    // One row of tongues across the cone at this distance, wider the farther out.
                    float across = 2f * run.T1 * Mathf.Tan(cast.Spec.Arc * 0.5f * Mathf.Deg2Rad);
                    int count = Mathf.Clamp(Mathf.RoundToInt(across / 0.75f), 1, 7);
                    for (int k = 0; k < count; k++)
                    {
                        Vector3 way = Quaternion.Euler(0f, FormRules.InFan(k, count, cast.Spec.Arc * 0.85f), 0f) * run.B;
                        Vector3 at = run.A + way * (run.T1 + Random.Range(-0.2f, 0.2f));
                        FormFx.Erupt(FormMesh.Flame, at, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.75f, 1.1f + Random.value * 1f, 0.75f),
                            k % 3 == 1 ? (ElementArt.Ready ? FireRed : Ember) : cast.Colour, 0.06f, 0.3f + Random.value * 0.35f, 0.25f);
                    }
                    run.T1 += 0.7f;
                }
                if (run.Age < spread) return true;
                run.Phase = 1;
                // The burning ground lies over the cone the wave struck, near its middle and out to both sides at its
                // far end, not in a line down the middle (the owner's review, 2026-11-19).
                for (int i = 0; i < 3; i++)
                {
                    patches[i] = run.A + Quaternion.Euler(0f, PatchTurn[i] * cast.Spec.Arc, 0f) * run.B * (cast.Reach * PatchOut[i]);
                    Clover.World.Hazards.Lay(cast.Ground(patches[i], PatchRadius, burns, 0.5f, cast.Linger(2f)));
                }
                if (ElementArt.Ready && !run.Flag2)
                {
                    // The front rears up as hooded tongues, cobra-flared, leaning in; rolling smoke over the carpet.
                    float yaw = ElementArt.Turn(run.B, 180f);
                    // At the one it reaches (or the cone's end): the front rears where it lands.
                    Vector3 toAim = run.C - run.A;
                    toAim.y = 0f;
                    float far = Mathf.Clamp(Vector3.Dot(toAim, run.B), 1.5f, cast.Reach);
                    ElementArt.Erupt(ElementArt.Piece("fire_hoods_spell"), run.A + run.B * far, Quaternion.Euler(0f, yaw, 0f), Vector3.one * 1.35f, 0.08f, 0.7f, 0.3f);
                    for (int i = 0; i < 3; i++) ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), patches[i] + Vector3.up * (2.2f + 0.4f * i), 2.4f, 1.4f);
                }
            }
            // What is left keeps licking upward until it has burned out, anywhere over the patches.
            run.T2 -= deltaTime;
            if (run.T2 <= 0f && !run.Flag2)
            {
                run.T2 = 0.05f;
                Vector2 off = Random.insideUnitCircle * PatchRadius;
                float left = 1f - (run.Age - spread) / burns;
                FormFx.Erupt(FormMesh.Flame, patches[Random.Range(0, 3)] + new Vector3(off.x, 0f, off.y), Quaternion.Euler(0f, Random.value * 360f, 0f),
                    new Vector3(0.5f, (0.5f + Random.value * 0.8f) * (0.4f + 0.6f * left), 0.5f), Random.value < 0.3f ? (ElementArt.Ready ? FireRed : Ember) : cast.Colour, 0.08f, 0.18f, 0.2f, 0.03f);
                // Smoke keeps rolling off what burns.
                if (ElementArt.Ready && Random.value < 0.06f) ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), patches[Random.Range(0, 3)] + Vector3.up * 2f, 2f, 1.3f);
            }
            return run.Age < spread + burns;
        }

        // ---- Lightning: Thunderclap. Instant: jagged lengths from the hand to the enemy faced and on to those near, redrawn several times in a fifth of a second. Nothing is left.

        private static int BeginLightning(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var solids = SpellWorks.Solids(run);
            var victim = FormKit.Victim(cast);
            int links = 0;
            points[0] = Hand(run);
            if (victim != null)
            {
                // The chain as it will be seen: the enemy faced, then the nearest to it.
                int near = SigKit.Gather(run, victim.transform.position, cast.Spec.Radius, Mathf.Min(6, cast.Spec.Count + 1));
                var things = SpellWorks.Things(run);
                points[++links] = victim.transform.position + Vector3.up * 1.1f;
                for (int i = 0; i < near; i++)
                    if (!ReferenceEquals(things[i], victim)) points[++links] = things[i].transform.position + Vector3.up * 1.1f;
                links = Mathf.Min(links, cast.Spec.Count + 1);
            }
            else points[++links] = run.A + run.B * Mathf.Min(cast.Reach, 4f) + Vector3.up * 0.1f;
            run.Done = links;
            int hits = cast.Combat.StrikeChain(cast.Reach, cast.Blow(cast.Damage, cast.Stagger), cast.Spec.Count, cast.Spec.Radius, cast.Element);
            Color white = ElementSubstance.Pale(cast.Colour);
            bool art = ElementArt.Ready;
            for (int i = 0; i < links * 2 && i < solids.Length; i++)
            {
                // The element art's bolts: a white-cyan core beside a violet edge, inked indigo.
                solids[i] = FormFx.Hold(FormMesh.Bolt, i % 2 == 0 ? white : art ? BoltViolet : cast.Colour, 0.02f);
                FormFx.Heat(solids[i], 0.55f);
            }
            for (int i = 1; i <= links; i++)
            {
                if (art)
                {
                    // The strike sheet where each link lands, a Lichtenberg fern burnt under it, the stepped leader over the first.
                    Vector3 foot = points[i] - Vector3.up * 1.1f;
                    ElementArt.Sheet(ElementArt.SheetOf("lightning_bolt_strike"), points[i] + Vector3.up * 0.5f, 3.2f, 0.55f);
                    ElementArt.Floor(ElementArt.Piece("lightning_fern_spell"), foot, i * 70f, 0.8f, 1f, 2f, 0.1f, true);
                    if (i == 1) ElementArt.Stand(ElementArt.Piece("lightning_leader_spell"), foot, 180f, 1f, 0.16f);
                    continue;
                }
                FormFx.Face(FormMark.Star, points[i], 2.2f, FormKit.Lit(cast.Colour, 4f), 0.09f, 3f, i * 40f);
                Vfx.Play(VfxKind.Shock, points[i], Vector3.up, 1f);
            }
            Color flash = Color.Lerp(Color.white, cast.Colour, 0.45f);
            flash.a = 0.22f;
            ScreenFlash.Flash(flash, 0.06f);
            TickLightning(ref run);
            return hits;
        }

        private static bool TickLightning(ref SpellRun run)
        {
            var points = SpellWorks.Points(run);
            var solids = SpellWorks.Solids(run);
            int beat = Mathf.FloorToInt(run.Age * 22f);
            for (int link = 0; link < run.Done; link++)
            {
                Vector3 from = points[link], to = points[link + 1];
                // The elbow of this link is somewhere new on every beat.
                Vector3 elbow = Vector3.Lerp(from, to, 0.35f + 0.3f * Hash(beat * 7 + link)) +
                    new Vector3(Hash(beat * 3 + link * 11) - 0.5f, (Hash(beat * 5 + link * 17) - 0.3f) * 0.9f, Hash(beat * 13 + link * 5) - 0.5f) * 1.5f;
                Lay(solids[link * 2], from, elbow);
                if (link * 2 + 1 < solids.Length) Lay(solids[link * 2 + 1], elbow, to);
            }
            return run.Age < 0.2f;
        }

        // A length of something along +Z laid from one point to another.
        private static void Lay(int handle, Vector3 from, Vector3 to, float thick = 2.2f)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            if (length < 0.01f) return;
            FormFx.Place(handle, from, Quaternion.LookRotation(along / length), new Vector3(thick, thick, length));
        }

        // ---- Wind: Gust. A rush of air seen as streaks and leaves: it strikes the cone and throws what it reaches straight down the wind. (It does not lift: what is lifted comes down where it left the ground, which would undo the throw.)

        private static int BeginWind(ref SpellRun run)
        {
            var cast = run.Cast;
            int hits = cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, cast.Blow(cast.Damage, cast.Stagger));
            Vector3 middle = run.A + run.B * (cast.Reach * 0.55f);
            // The wind keeps blowing for as long as it is seen: what it reached is pushed all that while, so a body
            // that reels from the blow first is still thrown the whole way.
            run.Phase = SigKit.Gather(run, middle, cast.Reach * 0.6f, 12);
            run.T3 = 0f;
            // Elements 4's Gust (the owner, 2026-11-20: "static lines through the enemy"): fronts of crescent curls sweep out
            // and part round each one reached, hooking in behind it, instead of streaks flown straight through the bodies.
            run.Flag2 = ElementArt.Ready && ElementMoves.PlaySpell(ElementSubstance.Wind, run.A, run.B) >= 0;
            if (run.Flag2)
            {
                int count = cast.Combat.CopyLastStruck(out var struck);
                for (int i = 0; i < count; i++) ElementMoves.SpellReached(ElementSubstance.Wind, struck[i].transform, run.B);
                TickWind(ref run, 0.1f);
                return hits;
            }
            Vfx.Play(VfxKind.Dust, run.A + run.B * 0.6f, run.B, 2f);
            Vfx.Play(VfxKind.Dust, middle, run.B, 3f);
            if (ElementArt.Ready)
            {
                // Grass flattened in a band ahead of it, the gust's streaks and curl sweeping across.
                ElementArt.Stand(ElementArt.Piece("wind_grass_spell"), middle, ElementArt.Turn(run.B), 1f, 1.1f);
                ElementArt.Sheet(ElementArt.SheetOf("wind_gust"), middle + Vector3.up * 0.8f, 4.5f, 0.6f, ElementSubstance.WindDust);
            }
            TickWind(ref run, 0.1f);
            return hits;
        }

        private static bool TickWind(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            const float blows = 0.45f;
            float front = (cast.Reach + 1.5f) * Mathf.Clamp01(run.Age / blows);
            float share = Mathf.Min(deltaTime / blows, 1f - run.T3);
            if (share > 0f)
            {
                run.T3 += share;
                var blown = SpellWorks.Things(run);
                Vector3 push = run.B * (GustRules.Impulse * share * cast.Combat.StatPush);
                for (int i = 0; i < run.Phase; i++)
                    if (FormKit.Alive(blown[i]) && blown[i] is IShovable body) body.Push(push);
            }
            run.T2 -= deltaTime;
            // The look draws the wind; only the shove is the code's.
            while (!run.Flag2 && run.T2 <= 0f)
            {
                run.T2 += 0.012f;
                run.Done++;
                float wide = 0.4f + front * Mathf.Tan(cast.Spec.Arc * 0.5f * Mathf.Deg2Rad);
                Vector3 from = run.A + run.B * Mathf.Max(0.3f, front - 1.2f) + Side(run) * Random.Range(-wide, wide) + Vector3.up * Random.Range(0.25f, 1.9f);
                if (run.Done % 4 == 0)
                {
                    // A leaf torn along with it, tumbling upward.
                    FormFx.Fly(FormMesh.Leaf, from, from + run.B * 4.5f + Vector3.up * Random.Range(0.5f, 2f), Random.rotation, Vector3.one * 0.5f, ElementArt.Ready ? DryLeaf : new Color(0.5f, 0.78f, 0.3f), 0.4f, 0.02f);
                    continue;
                }
                FormFx.Fly(FormMesh.Gust, from, from + run.B * 3.6f, Quaternion.LookRotation(run.B) * Quaternion.Euler(0f, 0f, Random.value * 360f),
                    new Vector3(4f, 4f, 2.2f + Random.value * 2.2f), WindPale, 0.26f, 0.03f);
            }
            return run.Age < blows || run.T3 < 1f;
        }

        // ---- Ice: Hoarfrost. Crystals grow out along the ground in a fan and the cold strikes as they come; they stand four seconds, and a blow of the weapon among them shatters the lot.

        private const int Crystals = 13;
        private static readonly int[] IceRows = { 2, 3, 4, 4 };

        private static int BeginIce(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var times = SpellWorks.Times(run);
            var solids = SpellWorks.Solids(run);
            var rows = IceRows;
            int k = 0;
            for (int row = 0; row < rows.Length; row++)
            {
                float out_ = cast.Reach * (0.2f + 0.23f * row);
                for (int i = 0; i < rows[row]; i++, k++)
                {
                    Vector3 way = Quaternion.Euler(0f, FormRules.InFan(i, rows[row], cast.Spec.Arc * 0.8f) + (Hash(k) - 0.5f) * 10f, 0f) * run.B;
                    points[k] = run.A + way * (out_ + (Hash(k + 40) - 0.5f) * 0.5f);
                    times[k] = 0.3f * (row / (float)rows.Length) + 0.02f * i;
                    solids[k] = FormFx.Hold(FormMesh.Crystal, k % 3 == 0 && !ElementArt.Ready ? Color.white : cast.Colour);
                    FormFx.Place(solids[k], points[k], Quaternion.identity, Vector3.one * 0.001f);
                }
            }
            if (ElementArt.Ready)
            {
                // Runic straight cuts etched round it, and the frost creeping out in hard steps.
                Vector3 middle = run.A + run.B * (cast.Reach * 0.5f);
                ElementArt.Floor(ElementArt.Piece("ice_runes_spell"), middle, ElementArt.Turn(run.B), 1f, 1f, Mathf.Max(1f, cast.Spec.Seconds), 0.3f, true);
                ElementArt.Sheet(ElementArt.SheetOf("ice_frost_creep"), middle + Vector3.up * 0.6f, 3.4f, 0.9f);
            }
            else FormFx.Lay(FormMark.Crack, run.A + run.B * (cast.Reach * 0.5f), cast.Reach * 1.3f, FormKit.Lit(cast.Colour, 1.6f), 1.2f, -1f, 0f, Quaternion.LookRotation(run.B).eulerAngles.y);
            return 0;
        }

        private static bool TickIce(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var times = SpellWorks.Times(run);
            var solids = SpellWorks.Solids(run);
            float stands = Mathf.Max(1f, cast.Spec.Seconds);
            if (run.Phase == 0 && run.Age >= 0.15f)
            {
                run.Phase = 1;
                cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, cast.Blow(cast.Damage, cast.Stagger));
            }
            if (run.Flag)
            {
                // Shattered: every crystal goes to splinters, and the splinters cut.
                for (int k = 0; k < Crystals; k++)
                {
                    FormFx.Release(solids[k]);
                    solids[k] = -1;
                    for (int piece = 0; piece < 2; piece++)
                    {
                        Vector3 way = Random.onUnitSphere;
                        way.y = Mathf.Abs(way.y) + 0.3f;
                        FormFx.Fly(FormMesh.Shard, points[k] + Vector3.up * 0.5f, points[k] + way * 2.2f, Quaternion.LookRotation(way), Vector3.one * 0.7f, piece == 0 ? Color.white : cast.Colour, 0.3f);
                    }
                }
                Vector3 middle = run.A + run.B * (cast.Reach * 0.55f);
                cast.Combat.StrikeArea(middle, cast.Reach * 0.6f, cast.Blow(5f, 8f, 1));
                if (ElementArt.Ready)
                {
                    // Broken all at once: the shatter's chips and the frost. The white crack-web covered everything and
                    // is gone (the owner's review, 2026-11-19).
                    ElementArt.Sheet(ElementArt.SheetOf("ice_frost_creep"), middle + Vector3.up * 0.8f, 4f, 0.6f);
                }
                else
                {
                    FormFx.Face(FormMark.Star, middle + Vector3.up, 4f, FormKit.Lit(cast.Colour, 3.6f), 0.1f, 5.5f);
                    FormFx.Lay(FormMark.Ring, middle, 1f, FormKit.Lit(cast.Colour, 3f), 0.3f, cast.Reach * 1.4f);
                }
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.04f, 0.16f, 0.2f);
                return false;
            }
            if (run.Age > 0.6f && run.Age < stands - 0.35f) return true;
            for (int k = 0; k < Crystals; k++)
            {
                float since = run.Age - times[k];
                if (since < 0f) continue;
                // Out fast and a little too far, then set; melting down at the end.
                float t = Mathf.Clamp01(since / 0.1f);
                float grown = 1f - (1f - t) * (1f - t) + 0.3f * Mathf.Sin(t * Mathf.PI);
                grown *= Mathf.Clamp01((stands - run.Age) / 0.3f);
                float size = 0.8f + 0.7f * Hash(k + 9);
                FormFx.Place(solids[k], points[k], Quaternion.Euler((Hash(k + 3) - 0.5f) * 24f, Hash(k + 5) * 360f, (Hash(k + 7) - 0.5f) * 24f),
                    new Vector3(size, size * (0.9f + 0.6f * Hash(k + 21)) * Mathf.Max(0.02f, grown), size));
            }
            return run.Age < stands;
        }

        // ---- Water: Riptide. A wave stands up out ahead and rolls back to the caster, carrying what it reaches with it, and breaks at their feet.

        private static int BeginWater(ref SpellRun run)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            run.Done = SigKit.Gather(run, run.A + run.B * (cast.Reach * 0.5f), cast.Reach * 0.5f + TideRules.HalfWidth, 12);
            var left = SpellWorks.Times(run);
            for (int i = 0; i < run.Done && i < left.Length; i++) left[i] = -1f;
            run.T1 = Mathf.Max(cast.Reach, TideRules.BreaksAt);
            // The elements 3 Riptide: one clean curling breaker with no triangles on its crest, pitching forward into a
            // splash crown at the break, no bubble rings and no ring round the caster. The recipe's wave rises 1.3 m
            // ahead of its caster and rolls away; here it rises where the tide does and rolls back in to the caster.
            run.Flag2 = ElementArt.Ready && ElementMoves.PlaySpell(ElementSubstance.Water, run.A + run.B * (run.T1 + 1.3f), -run.B) >= 0;
            if (!run.Flag2)
            {
                solids[0] = FormFx.Hold(FormMesh.Crest, Deep(cast.Colour, 0.86f), 0.03f);
                solids[1] = FormFx.Hold(FormMesh.Crest, Deep(cast.Colour, 0.6f), 0.02f);
                solids[2] = FormFx.Hold(FormMesh.Foam, Color.white, 0.012f);
                solids[3] = FormFx.Hold(FormMesh.Foam, Color.white, 0.012f);
            }
            Vector3 rises = run.A + run.B * run.T1;
            // The element art: the wave alone, with no lane or ring of light. Its shark-tooth crest read as random
            // triangles on top of the wave and is left off (the owner's review, 2026-11-19).
            solids[4] = -1;
            if (!ElementArt.Ready)
            {
                // The way it will come, drawn on the ground from where it rises to where it breaks.
                FormFx.Run(FormMark.Lane, rises, -run.B, run.T1 - TideRules.BreaksAt, TideRules.HalfWidth * 1.7f, FormKit.Lit(cast.Colour, 1.5f, 0.7f), 0.75f);
                FormFx.Lay(FormMark.Ring, rises, 1f, FormKit.Lit(cast.Colour, 2.4f), 0.35f, TideRules.HalfWidth * 2f);
            }
            TickWater(ref run, 0f);
            return 0;
        }

        private static bool TickWater(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var things = SpellWorks.Things(run);
            var touched = SpellWorks.Once(run);
            var left = SpellWorks.Times(run);
            float front = TideRules.Front(cast.Reach, run.Age);
            float step = run.T1 - front;
            run.T1 = front;
            Vector3 side = Side(run);
            Vector3 at = run.A + run.B * front;

            for (int i = 0; i < run.Done; i++)
            {
                var thing = things[i];
                if (!FormKit.Alive(thing)) continue;
                Vector3 off = thing.transform.position - run.A;
                float along = Vector3.Dot(off, run.B), aside = Vector3.Dot(off, side);
                if (run.Age > TideRules.RiseSeconds && thing is IDamageable one && !touched.Contains(one) && TideRules.Reached(along, aside, front))
                {
                    // Struck from its far side, once, as the face of the wave takes it.
                    touched.Add(one);
                    cast.Combat.StrikeOne(thing, cast.Blow(cast.Damage, cast.Stagger), thing.transform.position + run.B * 2f);
                    if (run.Flag2) ElementMoves.SpellReached(ElementSubstance.Water, thing.transform, -run.B);
                    else FormFx.Erupt(FormMesh.Spout, thing.transform.position, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.7f, 1.8f, 0.7f), ElementSubstance.Pale(cast.Colour), 0.07f, 0.05f, 0.25f);
                }
                if (i >= left.Length) continue;
                // Taken once, it is owed the way from where it stood to where the wave breaks, and is paid that as
                // the wave comes in: a body trails the water that carries it, so where it is now is no guide.
                if (left[i] < 0f && TideRules.Reached(along, aside, front)) left[i] = TideRules.Owed(along);
                float carry = left[i] > 0f ? Mathf.Min(step, left[i]) : 0f;
                if (carry <= 0f) continue;
                left[i] -= carry;
                // A shove is spent over a moment and moves two thirds of its length: half as much again keeps a body on the wave.
                if (thing is IShovable body) body.Push(-run.B * (carry * 1.5f));
            }

            float broken = run.Phase == 1 ? Mathf.Clamp01((run.Age - run.T3) / 0.28f) : 0f;
            float t = Mathf.Clamp01(run.Age / TideRules.RiseSeconds);
            float risen = (1f - (1f - t) * (1f - t)) * (1f - broken);
            // It faces the way it travels: toward the caster. Breaking, it falls flat and runs out along the ground.
            var facing = Quaternion.LookRotation(-run.B);
            // The swell breathes a little as it comes; breaking, it throws itself forward and lies down.
            float tall = 2.05f * risen * (1f + 0.04f * Mathf.Sin(run.Age * 22f));
            float long_ = 2.3f + 0.9f * broken;
            FormFx.Place(solids[0], at, facing, new Vector3(TideRules.HalfWidth * 2.1f, Mathf.Max(0.02f, tall), long_));
            // A second, lower swell following the first.
            FormFx.Place(solids[1], at + run.B * 2.1f, facing, new Vector3(TideRules.HalfWidth * 1.7f, Mathf.Max(0.02f, tall * 0.36f), long_ * 0.7f));
            // The lace of foam it leaves on the ground behind it, and the churn running out ahead of its foot.
            float trail = Mathf.Clamp(Mathf.Max(cast.Reach, TideRules.BreaksAt) - front + 0.6f, 0.6f, 3.2f);
            FormFx.Place(solids[2], at + run.B * (1.2f + trail * 0.5f) + Vector3.up * 0.02f, facing, new Vector3(TideRules.HalfWidth * 1.9f, 0.5f * Mathf.Max(0.05f, risen + broken), trail));
            FormFx.Place(solids[3], at - run.B * (0.42f * long_) + Vector3.up * 0.03f, facing, new Vector3(TideRules.HalfWidth * 1.9f, 2.4f * Mathf.Max(0.05f, risen + broken * 0.5f), 0.7f + 1.6f * broken));
            // The crest's teeth hang off the lip toward the camera, the longest in the middle.
            float teeth = TideRules.HalfWidth * 2.1f / 4.6f * Mathf.Max(0.02f, risen);
            FormFx.Place(solids[4], at - run.B * 0.9f + Vector3.up * (tall * 0.72f), Quaternion.Euler(0f, ElementArt.Turn(-run.B, 0f), 0f), Vector3.one * teeth);

            run.T2 -= deltaTime;
            if (run.T2 <= 0f && run.Phase == 0 && !run.Flag2)
            {
                // Spray torn off the lip: some thrown ahead of it, some left hanging behind.
                run.T2 = 0.03f;
                Vector3 top = at + side * Random.Range(-TideRules.HalfWidth, TideRules.HalfWidth) * 0.8f - run.B * 0.9f + Vector3.up * (tall * 0.8f);
                bool ahead = run.Done % 2 == 0 || Random.value < 0.5f;
                Vector3 to = top + (ahead ? -run.B * Random.Range(0.6f, 1.3f) - Vector3.up * 0.3f : run.B * 1.2f + Vector3.up * Random.Range(0.3f, 0.8f));
                FormFx.Fly(FormMesh.Droplet, top, to, Quaternion.LookRotation(to - top), Vector3.one * 0.3f, Color.white, 0.28f, 0.012f);
                // The wet it leaves: a dark lane where it has passed, with a line of light on it.
                if (step > 0f)
                {
                    FormFx.Run(FormMark.Lane, at + run.B * 0.6f, run.B, 1.2f, TideRules.HalfWidth * 1.8f, FormKit.Stain(cast.Colour, 0.4f), 1.8f);
                    FormFx.Run(FormMark.Streak, at + run.B * 0.8f + side * Random.Range(-TideRules.HalfWidth, TideRules.HalfWidth) * 0.8f, run.B, 1.4f, 0.12f, FormKit.Lit(Color.white, 0.9f, 0.5f), 0.9f, 0.02f);
                }
            }

            if (run.Phase == 0 && TideRules.Broken(cast.Reach, run.Age))
            {
                // It breaks: a wall of spray thrown up and forward over the caster's feet, and rings running out.
                run.Phase = 1;
                run.T3 = run.Age;
                Vector3 lip = at - run.B * 1.1f;
                for (int i = 0; i < 18 && !run.Flag2; i++)
                {
                    Vector3 from = lip + side * FormRules.InRow(i, 18, TideRules.HalfWidth * 1.8f / 18f) + Vector3.up * Random.Range(0.5f, 1.4f);
                    Vector3 to = from - run.B * Random.Range(0.8f, 2.4f) + Vector3.up * Random.Range(-0.3f, 1.2f);
                    FormFx.Fly(FormMesh.Droplet, from, to, Quaternion.LookRotation(to - from), Vector3.one * Random.Range(0.4f, 0.75f), i % 3 == 0 ? ElementSubstance.Pale(cast.Colour) : Color.white, 0.42f, 0.012f);
                }
                // The white of it going up where it lands, as mist, and the water thrown out of it.
                for (int i = 0; i < 5 && !run.Flag2; i++)
                    FormFx.Erupt(FormMesh.Cloud, lip + side * FormRules.InRow(i, 5, 0.95f) + Vector3.up * 0.3f, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(1.5f, 0.9f + 0.5f * Random.value, 1.2f), Color.white, 0.08f, 0.1f, 0.35f, 0.01f);
                for (int i = 0; i < 4 && !run.Flag2; i++)
                    FormFx.Erupt(FormMesh.Spout, lip + side * FormRules.InRow(i, 4, 1.2f), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.7f, 1.3f + Random.value, 0.7f), ElementSubstance.Pale(cast.Colour), 0.07f, 0.06f, 0.3f, 0.015f);
                if (run.Flag2) { }
                else if (ElementArt.Ready)
                {
                    // It slaps down: crowns along the lip, and the floor left wet with foam lace draining in rings.
                    for (int i = 0; i < 3; i++) ElementArt.Sheet(ElementArt.SheetOf("water_splash"), lip + side * FormRules.InRow(i, 3, 1.3f) + Vector3.up * 0.8f, 3f, 0.75f);
                    ElementArt.Floor(ElementArt.Piece("water_wet_affliction"), lip, 0f, 1.3f, 1.5f, 1.6f);
                }
                else
                {
                    FormFx.Lay(FormMark.Ring, lip, 1.2f, FormKit.Lit(Color.white, 1.6f, 0.8f), 0.5f, TideRules.HalfWidth * 2.4f);
                    FormFx.Lay(FormMark.Disc, lip, TideRules.HalfWidth * 1.7f, FormKit.Lit(ElementSubstance.Pale(cast.Colour), 0.8f, 0.3f), 1.3f, TideRules.HalfWidth * 2.1f);
                }
                // The stone is left wet: dark, with a sheen that dries off it.
                if (!run.Flag2) FormFx.Lay(FormMark.Scorch, lip, TideRules.HalfWidth * 2.4f, FormKit.Stain(cast.Colour, 0.5f), 2.6f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.14f, 0.18f);
            }
            return run.Phase == 0 || run.Age - run.T3 < 0.3f;
        }

        // The element's colour as the body of a wave: its blue kept, its green brought up so the paint on the wave can turn it to sea-green.
        private static Color Deep(Color colour, float share) => new Color(colour.r * share * 0.55f, Mathf.Max(colour.g, colour.b * 0.62f) * share * 0.9f, colour.b * share, 1f);

        // ---- Life: Briar Patch. Thorned vines grow out of the ground, flowers open among them, and the bed lashes whatever stands in it twice a second.

        private const int Vines = 9, Flowers = 4;

        private static int BeginLife(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var solids = SpellWorks.Solids(run);
            // The elements 3 Briar Patch: seeds thrown and briars bursting up from the middle outward, winding up the legs
            // of those inside, kept while the bed stands; no curved green thing on the ground and no white flash.
            run.Flag2 = ElementArt.Ready && ElementMoves.PlaySpell(ElementSubstance.Life, run.C - run.B * ElementMoves.SpellAhead, run.B, 1f, Mathf.Max(0.5f, cast.Spec.Seconds)) >= 0;
            for (int k = 0; k < Vines + Flowers && !run.Flag2; k++)
            {
                bool vine = k < Vines;
                points[k] = run.C + (vine ? FormRules.Scattered(k, Vines, cast.Spec.Radius * 0.9f) : FormRules.OnRing(k, Flowers, cast.Spec.Radius * 0.55f, 40f));
                solids[k] = FormFx.Hold(vine ? FormMesh.Vine : FormMesh.Flower, vine ? (k % 2 == 0 ? cast.Colour : new Color(0.25f, 0.6f, 0.22f)) : Petal);
                FormFx.Place(solids[k], points[k], Quaternion.identity, Vector3.one * 0.001f);
            }
            if (run.Flag2) { }
            else if (ElementArt.Ready)
            {
                // The bed blooms. Its runner vines curving over the floor read as a mystery shape and are left off
                // (the owner's review, 2026-11-19).
                ElementArt.Sheet(ElementArt.SheetOf("life_bloom"), run.C + Vector3.up * 0.8f, 3.2f, 1.1f);
            }
            else FormFx.Lay(FormMark.Ring, run.C, 0.5f, FormKit.Lit(cast.Colour, 2.4f), 0.35f, cast.Spec.Radius * 2f);
            run.T1 = 0.3f;
            run.T3 = -1f;
            return 0;
        }

        private static bool TickLife(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var solids = SpellWorks.Solids(run);
            float stands = Mathf.Max(0.5f, cast.Spec.Seconds);
            float leaving = Mathf.Clamp01((stands - run.Age) / 0.35f);
            float lash = 1f + 0.4f * Mathf.Clamp01(1f - (run.Age - run.T3) / 0.22f);
            for (int k = 0; k < Vines; k++)
            {
                float t = Mathf.Clamp01((run.Age - k * 0.045f) / 0.5f);
                float grown = (1f - (1f - t) * (1f - t)) * leaving;
                float tall = 1.3f + 0.9f * Hash(k + 2);
                var lean = Quaternion.Euler(Mathf.Sin(run.Age * 2.1f + k) * 9f, Hash(k) * 360f + run.Age * 8f, Mathf.Cos(run.Age * 1.7f + k * 2f) * 9f);
                FormFx.Place(solids[k], points[k], lean, new Vector3(1.2f * Mathf.Max(0.02f, grown), tall * Mathf.Max(0.02f, grown) * lash, 1.2f * Mathf.Max(0.02f, grown)));
            }
            for (int k = Vines; k < Vines + Flowers; k++)
            {
                // Each opens with a pop a little after the vines are up.
                float t = Mathf.Clamp01((run.Age - 0.5f - (k - Vines) * 0.12f) / 0.18f);
                float open = (t + 0.35f * Mathf.Sin(t * Mathf.PI)) * leaving;
                FormFx.Place(solids[k], points[k] + Vector3.up * (0.25f + 0.5f * Hash(k)), Quaternion.Euler(0f, run.Age * 25f + k * 50f, 0f), Vector3.one * (1.5f * Mathf.Max(0.02f, open)));
            }
            if (run.Age >= stands) return false;
            if (run.Age < run.T1) return true;
            run.T1 += Mathf.Max(0.1f, cast.Spec.Interval);
            run.T3 = run.Age;
            cast.Combat.StrikeArea(run.C, cast.Spec.Radius, cast.Blow(cast.Spec.LingerDamage, 0f));
            if (run.Flag2)
            {
                int count = cast.Combat.CopyLastStruck(out var struck);
                for (int i = 0; i < count; i++) ElementMoves.SpellReached(ElementSubstance.Life, struck[i].transform, run.B);
                return true;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 from = run.C + FormRules.OnRing(i, 3, cast.Spec.Radius * 0.5f, Random.value * 120f) + Vector3.up * 0.8f;
                FormFx.Fly(FormMesh.Leaf, from, from + Vector3.up * 1.4f + Random.insideUnitSphere * 0.8f, Random.rotation, Vector3.one * 0.45f, cast.Colour, 0.4f, 0.02f);
            }
            return true;
        }

        // ---- Earth: Stone Rise. The ground breaks under the enemy faced and a wall of rock is left standing behind it, which nothing walks through.

        /// <summary>Seconds elements 4's Stone Rise look plays after the stones burst up (its 1.6 s timeline).</summary>
        public const float StoneRiseShown = 1.6f;

        private static int BeginEarth(ref SpellRun run)
        {
            var cast = run.Cast;
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            FormKit.Warn(cast, run.C, cast.Spec.Radius, run.T0);
            return 0;
        }

        private static bool TickEarth(ref SpellRun run)
        {
            var cast = run.Cast;
            var points = SpellWorks.Points(run);
            var solids = SpellWorks.Solids(run);
            float stands = Mathf.Max(1f, cast.Spec.Seconds);
            if (run.Phase == 0)
            {
                if (run.Age < run.T0) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                cast.Combat.StrikeArea(run.C, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                // Up under them and down again; the wall behind them stays.
                cast.Combat.LiftArea(run.C, cast.Spec.Radius, 0.35f);
                // Elements 4's Stone Rise (the owner, 2026-11-20: "i still dont want the ring of 3 stones around the enemy, just
                // the massive stones rising and knocking up the enemy"): massive stones burst up under each one struck and
                // sink back crumbling. No ring, no wall of three stones behind it: that wall was those stones.
                if (ElementArt.Ready && ElementMoves.HasSpellOnBody(ElementSubstance.Earth))
                {
                    run.Flag2 = true;
                    int count = cast.Combat.CopyLastStruck(out var struck);
                    for (int i = 0; i < count; i++) ElementMoves.SpellReached(ElementSubstance.Earth, struck[i].transform, run.B);
                    if (count == 0) ElementMoves.SpellAt(ElementSubstance.Earth, run.C, run.B);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.24f, 0.25f);
                    return true;
                }
                FormFx.Erupt(FormMesh.Rock, run.C, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(1.5f, 1.2f, 1.5f), cast.Colour, 0.07f, 0.25f, 0.5f);
                Vector3 side = Side(run);
                for (int j = 0; j < 3; j++)
                {
                    points[j] = run.C + run.B * (1.6f - 0.3f * Mathf.Abs(j - 1)) + side * ((j - 1) * 1.15f);
                    solids[j] = FormFx.Hold(FormMesh.Rock, j == 1 ? cast.Colour : new Color(cast.Colour.r * 0.75f, cast.Colour.g * 0.75f, cast.Colour.b * 0.8f), 0.045f);
                    FormFx.Place(solids[j], points[j], Quaternion.identity, Vector3.one * 0.001f);
                }
                SpellWorks.Barrier(run, run.C + run.B * 1.55f + Vector3.up, FormKit.Yaw(run.B), new Vector3(3.5f, 2f, 0.9f));
                for (int i = 0; i < 8; i++)
                {
                    Vector3 way = FormRules.OnRing(i, 8, 1f, Random.value * 40f);
                    Vector3 from = run.C + Vector3.up * 0.4f;
                    Vector3 to = run.C + way * Random.Range(1.6f, 3f) + Vector3.up * Random.Range(0.1f, 1.6f);
                    FormFx.Fly(FormMesh.Pebble, from, to, Random.rotation, Vector3.one * Random.Range(0.4f, 0.8f), cast.Colour, 0.45f);
                }
                if (ElementArt.Ready)
                {
                    // Standing stones thrust up round it with overshoot, the ground broken, a heavy ring of dust.
                    ElementArt.Erupt(ElementArt.Piece("earth_stones_spell"), run.C, Quaternion.Euler(0f, 180f, 0f), Vector3.one * 0.85f, 0.07f, stands - 0.5f, 0.4f);
                    ElementArt.Sheet(ElementArt.SheetOf("earth_dust_burst"), run.C + Vector3.up * 0.9f, 4.4f, 0.9f);
                }
                else FormFx.Lay(FormMark.Crack, run.C, 4.6f, FormKit.Lit(cast.Colour, 2f), 2.2f, -1f, 0f, Random.value * 360f);
                FormFx.Lay(FormMark.Scorch, run.C, 3f, FormKit.Stain(cast.Colour), 4f);
                Vfx.Play(VfxKind.Dust, run.C + Vector3.up * 0.2f, Vector3.up, 3f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.24f, 0.25f);
            }
            float since = run.Age - run.T1;
            // The look plays the stones on its own; the spell ends with it.
            if (run.Flag2) return since < StoneRiseShown;
            if (since > 0.3f && since < stands - 0.45f) return true;
            for (int j = 0; j < 3; j++)
            {
                float t = Mathf.Clamp01((since - j * 0.04f) / 0.12f);
                float up = (1f - (1f - t) * (1f - t) + 0.25f * Mathf.Sin(t * Mathf.PI)) * Mathf.Clamp01((stands - since) / 0.4f);
                FormFx.Place(solids[j], points[j], Quaternion.Euler(0f, 30f + j * 115f, 0f) * Quaternion.Euler(j == 1 ? 0f : 8f, 0f, 0f), new Vector3(1.5f, (j == 1 ? 2.3f : 1.8f) * Mathf.Max(0.02f, up), 1.3f));
            }
            return since < stands;
        }

        // ---- Metal: Flung Blade. A blade thrown into the enemy faced stands in it; the next blow of the weapon drives it in, and it tears free and flies back to the hand, cutting what is in its way.

        private static int BeginMetal(ref SpellRun run)
        {
            var cast = run.Cast;
            run.Victim = FormKit.Victim(cast);
            // The element art: a xiphos with a chrome stripe, and no sign of light over the one it stands in.
            int xiphos = ElementArt.Ready ? ElementArt.Hold(ElementArt.Piece("metal_xiphos_spell")) : -1;
            run.Flag2 = xiphos >= 0;
            SpellWorks.Solids(run)[0] = xiphos >= 0 ? xiphos : FormFx.Hold(FormMesh.Blade, cast.Colour, 0.04f);
            SpellWorks.Marks(run)[0] = xiphos >= 0 ? -1 : FormFx.HoldMark(FormMark.Cross, FormKit.Lit(cast.Colour, 3f));
            run.C = run.Victim != null ? run.Victim.transform.position + Vector3.up * 1.1f : run.A + run.B * Mathf.Min(cast.Reach, 5f) + Vector3.up * 0.3f;
            run.T0 = Mathf.Clamp((run.C - Hand(run)).magnitude / 32f, 0.07f, 0.2f);
            TickMetal(ref run);
            return 0;
        }

        private static bool TickMetal(ref SpellRun run)
        {
            var cast = run.Cast;
            int blade = SpellWorks.Solids(run)[0];
            int sign = SpellWorks.Marks(run)[0];
            Vector3 hand = Hand(run);
            bool alive = FormKit.Alive(run.Victim);
            if (run.Phase == 0)
            {
                if (alive) run.C = run.Victim.transform.position + Vector3.up * 1.1f;
                float t = Mathf.Clamp01(run.Age / run.T0);
                // Thrown to hang over the head of the one faced (the owner's review, 2026-11-19: it looked goofy stood
                // in the body), turning to point down at it as it arrives.
                Vector3 over = run.Victim != null ? run.C + Vector3.up * BladeHover : run.C;
                Vector3 way = (over - hand).normalized;
                Quaternion flying = Quaternion.Slerp(Quaternion.LookRotation(way), Quaternion.LookRotation(Vector3.down, way), t * t);
                FormFx.Place(blade, Vector3.Lerp(hand, over, t), Blade(run, flying), Vector3.one * BladeSize(run, 1.5f));
                FormFx.PlaceMarkFacing(sign, run.C, 0.01f);
                if (t < 1f) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                // It bites as it lands, lightly: the weight of it comes when it is driven in.
                if (alive) cast.Combat.StrikeOne(run.Victim, cast.Blow(cast.Damage * 0.25f, 0f, 0), run.A);
                if (run.Flag2)
                {
                    // It rings: the clang, and sparks off it in a forward fan that fork as they go.
                    ElementArt.Sheet(ElementArt.SheetOf("metal_clang_v2"), run.C, 2.8f, 0.5f);
                    ElementArt.Stand(ElementArt.Piece("metal_sparks_spell"), new Vector3(run.C.x, run.A.y, run.C.z) - way * 1.2f, ElementArt.Turn(way), 1f, 0.2f);
                    return true;
                }
                Vfx.Play(VfxKind.Clang, run.C, -way, 1f);
                FormFx.Cut(Vector3.Lerp(hand, run.C, 0.5f), way, (run.C - hand).magnitude, 0.3f, FormKit.Lit(cast.Colour, 3f), 0.14f);
                return true;
            }
            if (run.Phase == 1)
            {
                float stuck = run.Age - run.T1;
                Vector3 way = run.C - hand;
                way.y = 0f;
                way = way.sqrMagnitude > 0.01f ? way.normalized : run.B;
                if (alive)
                {
                    run.C = run.Victim.transform.position + Vector3.up * 1.1f;
                    // It hangs over them point down, bobbing and turning slowly, shivering a moment as it stops.
                    float shiver = Mathf.Sin(stuck * 60f) * 9f * Mathf.Exp(-stuck * 6f);
                    Vector3 over = run.C + Vector3.up * (BladeHover + 0.06f * Mathf.Sin(stuck * 3.2f));
                    FormFx.Place(blade, over, Blade(run, Quaternion.LookRotation(Vector3.down, way) * Quaternion.Euler(shiver, 0f, stuck * 50f)), Vector3.one * BladeSize(run, 1.9f));
                    FormFx.PlaceMarkFacing(sign, run.C + Vector3.up * 1.7f, 0.8f * (1f + 0.12f * Mathf.Sin(run.Age * 9f)));
                    if (run.Flag)
                    {
                        Vector3 foot = run.Victim.transform.position;
                        cast.Combat.StrikeArea(foot, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                        FormKit.Impact(cast, foot, cast.Spec.Radius * 0.75f, 1.1f);
                        if (run.Flag2) ElementArt.Sheet(ElementArt.SheetOf("metal_clang_v2"), run.C, 3.6f, 0.55f);
                        else FormFx.Face(FormMark.Cross, run.C, 3f, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.4f, 30f);
                    }
                    else if (stuck < Mathf.Max(0.5f, cast.Spec.Seconds)) return true;
                }
                else if (run.Victim == null && stuck < 0.45f)
                {
                    // No one was faced: it stands in the ground a moment before it comes back.
                    FormFx.Place(blade, run.C + Vector3.up * 0.35f - way * 0.3f, Blade(run, Quaternion.LookRotation(way + Vector3.down * 1.2f)), Vector3.one * BladeSize(run, 1.5f));
                    return true;
                }
                run.Phase = 2;
                run.T1 = run.Age;
                FormFx.ReleaseMark(sign, 0.1f);
                SpellWorks.Marks(run)[0] = -1;
                // Called back: it cuts everything on the line home.
                Vector3 home = cast.Player.transform.position - run.C;
                home.y = 0f;
                Vector3 from = run.C;
                from.y = run.A.y;
                cast.Combat.StrikeLane(from, home, home.magnitude, 0.7f, cast.Blow(cast.Damage * 0.35f, 0f, 0), SpellWorks.Once(run));
                if (!run.Flag2) FormFx.Cut(Vector3.Lerp(run.C, cast.Player.transform.position + Vector3.up * 1.1f, 0.5f), home, home.magnitude, 0.35f, FormKit.Lit(cast.Colour, 3f), 0.2f);
                return true;
            }
            float sinceCalled = run.Age - run.T1;
            if (run.Flag && sinceCalled < BladeDrop)
            {
                // Driven in: it falls point first from over the head into the body before it tears free.
                float drop = sinceCalled / BladeDrop;
                Vector3 downWay = run.C - cast.Player.transform.position;
                downWay.y = 0f;
                FormFx.Place(blade, Vector3.Lerp(run.C + Vector3.up * BladeHover, run.C - Vector3.up * 0.2f, drop * drop),
                    Blade(run, Quaternion.LookRotation(Vector3.down, downWay.sqrMagnitude > 0.01f ? downWay.normalized : run.B)), Vector3.one * BladeSize(run, 1.9f));
                return true;
            }
            float back = Mathf.Clamp01((sinceCalled - (run.Flag ? BladeDrop : 0f)) / 0.2f);
            Vector3 to = cast.Player.transform.position + Vector3.up * 1.1f;
            Vector3 flight = to - run.C;
            // End over end all the way home, from the body it was driven into or from over the head it hung over.
            Vector3 leaves = !run.Flag && run.Victim != null ? run.C + Vector3.up * BladeHover : run.C;
            FormFx.Place(blade, Vector3.Lerp(leaves, to, back * back), Blade(run, Quaternion.LookRotation(flight.sqrMagnitude > 0.01f ? flight.normalized : run.B) * Quaternion.Euler(run.Age * 1800f, 0f, 0f)), Vector3.one * BladeSize(run, 1.5f));
            if (back < 1f) return true;
            Vfx.Play(VfxKind.GuardSpark, to, Vector3.up, 0.6f);
            return false;
        }

        // The kit's xiphos was drawn pointing about 236 degrees round from +Z, at its own length: turned to lie along +Z
        // as the built blade does, and not scaled up as that one is.
        private static Quaternion Blade(in SpellRun run, Quaternion along) => run.Flag2 ? along * Quaternion.Euler(0f, -236f, 0f) : along;

        private static float BladeSize(in SpellRun run, float built) => run.Flag2 ? built * 0.62f : built;

        // Flung Blade hangs this far over the body's middle (about half a metre over the head), and is driven down into it over this long.
        private const float BladeHover = 1.45f, BladeDrop = 0.1f;

        // ---- Light: Glare. A ray from the hand, narrow and straight, with a flare where it starts and on everything it crosses; what it crosses is marked, and the mark is seen.

        private static int BeginLight(ref SpellRun run)
        {
            var cast = run.Cast;
            var things = SpellWorks.Things(run);
            var marks = SpellWorks.Marks(run);
            var struck = SpellWorks.Once(run);
            float length = cast.Reach + 1.5f;
            struck.Clear();
            int hits = cast.Combat.StrikeLane(run.A, run.B, length, 0.7f, cast.Blow(cast.Damage, cast.Stagger), struck);
            int n = 0;
            foreach (var one in struck)
                if (n < 5 && one is Component body) things[n++] = body;
            struck.Clear();
            run.Done = n;
            Vector3 hand = Hand(run);
            Color light = FormKit.Lit(cast.Colour, 3.4f);
            if (ElementArt.Ready)
            {
                // A beam from a glint at the hand, a jali star lattice printed along its footprint, and the shadow of
                // everything it crosses thrown four metres back, away from the light.
                FormFx.Run(FormMark.Beam, hand - Vector3.up * 0.1f, run.B, length, 1.3f, FormKit.Lit(new Color(1f, 0.82f, 0.4f), 1.15f), 0.4f, 0.4f);
                float yaw = ElementArt.Turn(run.B, 233f);
                ElementArt.Floor(ElementArt.Piece("light_jali_spell"), ElementArt.Anchor(run.A + run.B * 0.6f, yaw, new Vector3(2.3f, 0f, 1.7f)), yaw, 1f, 1f, 1.2f, 0.12f, true);
                ElementArt.Sheet(ElementArt.SheetOf("light_flash_v2"), hand, 2.2f, 0.45f);
                for (int i = 0; i < n; i++)
                {
                    Vector3 feet = SigKit.Foot(things[i], run.A.y);
                    ElementArt.Shadow(feet, feet - run.A, 4f, 1f, 1.2f, 0.1f);
                }
                Color glare = Color.Lerp(Color.white, cast.Colour, 0.4f);
                glare.a = 0.14f;
                ScreenFlash.Flash(glare, 0.07f);
                return hits;
            }
            FormFx.Run(FormMark.Beam, hand - Vector3.up * 0.1f, run.B, length, 1.2f, light, 0.3f, 0.05f);
            FormFx.Run(FormMark.Streak, run.A + run.B * 0.5f, run.B, length, 0.6f, FormKit.Lit(cast.Colour, 1.6f, 0.7f), 1.1f, 0.05f);
            FormFx.Fly(FormMesh.Ray, hand, hand, Quaternion.LookRotation(run.B), new Vector3(3.2f, 3.2f, length), Color.white, 0.32f, 0.01f);
            // The flare: a star on the hand and a long thin streak across it, as a lens draws a bright light.
            FormFx.Face(FormMark.Star, hand, 2.8f, FormKit.Lit(cast.Colour, 4f), 0.14f, 3.8f);
            FormFx.Cut(hand, Side(run), 8f, 0.3f, light, 0.26f);
            for (int i = 0; i < n; i++)
            {
                Vector3 chest = things[i].transform.position + Vector3.up * 1.1f;
                FormFx.Face(FormMark.Star, chest, 2.2f, FormKit.Lit(cast.Colour, 4f), 0.16f, 3f, 22f);
                FormFx.Cut(chest, Side(run), 4.5f, 0.22f, light, 0.3f);
                marks[i] = FormFx.HoldMark(FormMark.Brackets, light);
                // Rays standing out of the one it marked.
                for (int ray = 0; ray < 4; ray++)
                {
                    Vector3 way = (FormRules.OnRing(ray, 4, 1f, 45f) + Vector3.up * 0.8f).normalized;
                    FormFx.Fly(FormMesh.Ray, chest, chest, Quaternion.LookRotation(way), new Vector3(1.6f, 1.6f, 1.8f), Color.white, 0.5f, 0.01f);
                }
            }
            Color flash = Color.Lerp(Color.white, cast.Colour, 0.4f);
            flash.a = 0.16f;
            ScreenFlash.Flash(flash, 0.07f);
            TickLight(ref run);
            return hits;
        }

        private static bool TickLight(ref SpellRun run)
        {
            var things = SpellWorks.Things(run);
            var marks = SpellWorks.Marks(run);
            for (int i = 0; i < run.Done; i++)
            {
                if (!FormKit.Alive(things[i])) continue;
                // The mark snaps shut on them and turns.
                float size = 1.7f * (1f + 1.2f * Mathf.Clamp01(1f - run.Age / 0.16f));
                FormFx.PlaceMark(marks[i], SigKit.Foot(things[i], run.A.y), Quaternion.Euler(0f, run.Age * 90f, 0f), new Vector3(size, 1f, size));
            }
            return run.Age < 1.6f;
        }

        // ---- Dark: Grasp. A black ball crawls to the enemy faced, drawing in everything it passes; it closes on them, and what it has hold of is dragged back to the caster's feet.

        private static readonly Color Black = new Color(0.03f, 0.01f, 0.05f);

        private static int BeginDark(ref SpellRun run)
        {
            var cast = run.Cast;
            run.Victim = FormKit.Victim(cast);
            if (run.Victim == null) run.C = run.A + run.B * Mathf.Min(cast.Reach, 4f);
            // The element art: a travelling hole in the floor with a pale horizon, not a ball.
            run.Flag2 = ElementArt.Ready;
            if (run.Flag2)
            {
                SpellWorks.Marks(run)[0] = ElementArt.HoldFloor(ElementArt.Piece("dark_drain_spell"));
                SpellWorks.Marks(run)[1] = ElementArt.HoldFloor(ElementArt.Piece("dark_streaks_spell"));
                SpellWorks.Marks(run)[2] = ElementArt.HoldFloor(ElementArt.Piece("dark_hole_spell"));
                SpellWorks.Marks(run)[3] = ElementArt.HoldFloor(ElementArt.Piece("dark_horizon_spell"));
            }
            else
            {
                SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, Black, 0.06f);
                SpellWorks.Solids(run)[1] = FormFx.Hold(FormMesh.Disc, cast.Colour, 0.02f);
            }
            Vector3 to = run.Victim != null ? run.Victim.transform.position : run.C;
            // Slow: it can be watched all the way there.
            run.T0 = Mathf.Max(0.3f, (to - run.A).magnitude / 6.5f);
            TickDark(ref run, 0f);
            return 0;
        }

        private static bool TickDark(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Vector3 hand = Hand(run);
            if (run.Phase == 0)
            {
                if (FormKit.Alive(run.Victim)) run.C = run.Victim.transform.position;
                float t = Mathf.Clamp01(run.Age / run.T0);
                Vector3 ground = Vector3.Lerp(run.A + run.B * 0.5f, run.C, t);
                Vector3 at = ground + Vector3.up * 1.1f;
                // It grows as it feeds, and beats slowly.
                float size = (0.3f + 0.3f * t) * (1f + 0.1f * Mathf.Sin(run.Age * 7f));
                FormFx.Place(solids[0], at, Quaternion.Euler(run.Age * 40f, run.Age * 70f, 0f), Vector3.one * size);
                FormFx.Place(solids[1], at, Quaternion.Euler(20f, run.Age * -200f, 0f), new Vector3(size * 2.1f, size * 0.5f, size * 2.1f));
                if (run.Flag2) PlaceHole(run, ground, 0.7f + 0.5f * t, run.Age * -40f);
                cast.Combat.Displace(ground, 2.6f, 3.2f * deltaTime, true);
                run.T2 -= deltaTime;
                if (run.T2 <= 0f)
                {
                    run.T2 = 0.1f;
                    if (run.Flag2) ElementArt.Stand(ElementArt.Piece("dark_streaksair_spell"), ground, Random.value * 360f, 0.9f, 0.12f);
                    else
                    {
                        // Rings close on it along the ground and smoke falls into it from all round.
                        FormFx.Lay(FormMark.Ring, ground, 3.6f, FormKit.Lit(cast.Colour, 1.8f), 0.24f, 0.4f);
                        Vector3 from = at + Random.onUnitSphere * 1.7f;
                        FormFx.Fly(FormMesh.Cloud, from, at, Random.rotation, Vector3.one * 0.6f, ElementSubstance.Shade(cast.Colour), 0.24f, 0.02f);
                    }
                }
                if (t < 1f) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                run.C = ground;
                cast.Combat.StrikeArea(ground, 1.7f, cast.Blow(cast.Damage, cast.Stagger));
                if (run.Flag2)
                {
                    // The snap: one inverted frame, then the hole closes like an iris.
                    ElementArt.Sheet(ElementArt.SheetOf("dark_hole_close_v2"), at, 3f, 0.6f);
                }
                else
                {
                    FormFx.Lay(FormMark.Ring, ground, 5f, FormKit.Lit(cast.Colour, 2.6f), 0.2f, 0.3f);
                    FormFx.Lay(FormMark.Scorch, ground, 3.4f, FormKit.Stain(cast.Colour, 0.8f), 3f);
                    FormFx.Face(FormMark.Star, at, 2.4f, FormKit.Lit(cast.Colour, 2.4f), 0.1f, 0.6f);
                }
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.14f, 0.2f);
                // What it has hold of comes back with it: dragged to a sword's length from the caster.
                int held = SigKit.Gather(run, ground, 2f, 5);
                var things = SpellWorks.Things(run);
                for (int i = 0; i < held; i++)
                {
                    Vector3 home = cast.Player.transform.position - things[i].transform.position;
                    home.y = 0f;
                    float far = home.magnitude;
                    if (far > 1.5f && things[i] is IShovable body) body.Push(home / far * (Mathf.Min(5f, far - 1.4f) * 1.5f));
                }
                return true;
            }
            float back = Mathf.Clamp01((run.Age - run.T1) / 0.35f);
            Vector3 feet = cast.Player.transform.position + (run.C - cast.Player.transform.position).normalized * 1.2f;
            Vector3 now = Vector3.Lerp(run.C + Vector3.up * 1.1f, feet, back * back);
            float left = 0.6f * (1f - back * 0.7f);
            FormFx.Place(solids[0], now, Quaternion.identity, Vector3.one * left);
            FormFx.Place(solids[1], now, Quaternion.Euler(20f, run.Age * -200f, 0f), new Vector3(left * 2.1f, left * 0.5f, left * 2.1f));
            // The hole drags what it holds home, closing as it comes.
            if (run.Flag2) PlaceHole(run, new Vector3(now.x, run.A.y, now.z), 1.2f * (1f - back * 0.85f), run.Age * -40f);
            run.T2 -= deltaTime;
            if (run.T2 <= 0f && !run.Flag2)
            {
                run.T2 = 0.04f;
                FormFx.Fly(FormMesh.Cloud, now, new Vector3(now.x, run.A.y, now.z), Random.rotation, new Vector3(0.7f, 0.45f, 0.7f), ElementSubstance.Shade(cast.Colour), 0.5f, 0.02f);
            }
            if (back < 1f) return true;
            if (!run.Flag2) FormFx.Lay(FormMark.Scorch, feet, 2f, FormKit.Stain(cast.Colour, 0.7f), 2f);
            return false;
        }

        // The element art's Grasp: the drained floor and the streaks pulled in about it, the hole and its pale horizon.
        private static void PlaceHole(in SpellRun run, Vector3 ground, float size, float swirl)
        {
            var marks = SpellWorks.Marks(run);
            ElementArt.PlaceFloor(marks[0], ElementArt.Piece("dark_drain_spell"), ground, 0f, size);
            ElementArt.PlaceFloor(marks[1], ElementArt.Piece("dark_streaks_spell"), ground, swirl, size * (0.9f + 0.1f * Mathf.Sin(run.Age * 9f)));
            ElementArt.PlaceFloor(marks[2], ElementArt.Piece("dark_hole_spell"), ground, 0f, size);
            ElementArt.PlaceFloor(marks[3], ElementArt.Piece("dark_horizon_spell"), ground, swirl * 2f, size);
        }

        // ---- Poison: Miasma. A low cloud that hangs, creeps after the nearest enemy and doses whatever stands in it once a second; bubbles rise through it and it leaves a stain.

        private const int Clouds = 8;

        private static string Lip(int k) => k == 0 ? "poison_lip_spell" : k == 1 ? "poison_lip_spell_1" : "poison_lip_spell_2";
        private static string Gas(int k) => k == 0 ? "poison_gas_spell" : k == 1 ? "poison_gas_spell_1" : "poison_gas_spell_2";
        private static string Ruyi(int k) => k % 3 == 0 ? "poison_ruyi_spell" : k % 3 == 1 ? "poison_ruyi_spell_1" : "poison_ruyi_spell_2";
        private static string Pits(int k) => k % 3 == 0 ? "poison_pits_spell" : k % 3 == 1 ? "poison_pits_spell_1" : "poison_pits_spell_2";

        // The element art's Miasma: three terraces crawling the way it creeps, each a little behind and above the one
        // before, bobbing out of step, over their see-through gas.
        private static void PlaceGas(ref SpellRun run, float spread, float deltaTime)
        {
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float yaw = ElementArt.Turn(run.B);
            var turn = Quaternion.Euler(0f, yaw, 0f);
            float grown = Mathf.Max(0.02f, spread);
            for (int k = 0; k < 3; k++)
            {
                Vector3 at = run.C - run.B * (0.55f * k) + Vector3.up * (0.04f * k + 0.03f * Mathf.Sin(run.Age * 1.7f + k * 2.1f));
                FormFx.Place(solids[k], at, turn, new Vector3(grown, grown * (1f + 0.08f * Mathf.Sin(run.Age * 2.3f + k)), grown));
                ElementArt.PlaceFloor(marks[k], ElementArt.Piece(Gas(k)), at, yaw, grown);
            }
        }

        private static int BeginPoison(ref SpellRun run)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            Color murk = Color.Lerp(cast.Colour, new Color(0.2f, 0.3f, 0.1f), 0.6f), fume = Color.Lerp(cast.Colour, new Color(0.2f, 0.3f, 0.1f), 0.25f);
            run.Flag2 = ElementArt.Ready;
            // The elements 3 Miasma: a glob of venom bursts and a low bank of lobed yellow-green clouds billows out and
            // churns as it creeps (the owner: "it needs to be like a cloud"), in place of the terraces and the pits.
            run.Look = run.Flag2 ? ElementMoves.PlaySpell(ElementSubstance.Poison, run.C - run.B * ElementMoves.SpellAhead, run.B, 1f, Mathf.Max(0.5f, cast.Spec.Seconds)) : -1;
            run.Flag = run.Look >= 0;
            if (run.Flag) { }
            else if (run.Flag2)
            {
                // The element art: a heavy gas front in three terraces of inked lobes over see-through layers.
                for (int k = 0; k < 3; k++)
                {
                    solids[k] = ElementArt.Hold(ElementArt.Piece(Lip(k)));
                    SpellWorks.Marks(run)[k] = ElementArt.HoldFloor(ElementArt.Piece(Gas(k)));
                }
            }
            else
            {
                for (int k = 0; k < Clouds; k++) solids[k] = FormFx.Hold(FormMesh.Cloud, k % 2 == 0 ? fume : murk, 0.015f);
                SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.2f, 0.7f));
            }
            run.Victim = FormKit.Victim(cast);
            run.T1 = 0.4f;
            TickPoison(ref run, 0f);
            return 0;
        }

        private static bool TickPoison(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            float hangs = Mathf.Max(0.5f, cast.Spec.Seconds);
            float radius = cast.Spec.Radius;
            if (FormKit.Alive(run.Victim))
            {
                // It creeps: slower than anything walks, but it does not stop.
                Vector3 toward = run.Victim.transform.position - run.C;
                toward.y = 0f;
                if (toward.magnitude > 0.2f) run.C += toward.normalized * (0.8f * deltaTime);
            }
            float spread = Mathf.Clamp01(run.Age / 0.45f) * Mathf.Clamp01((hangs - run.Age) / 0.5f);
            if (run.Flag) ElementLooks.MoveHeld(run.Look, run.C - run.B * ElementMoves.SpellAhead);
            else if (run.Flag2) PlaceGas(ref run, spread, deltaTime);
            else for (int k = 0; k < Clouds; k++)
            {
                float angle = (k * 47f + run.Age * (10f + k * 3f)) * Mathf.Deg2Rad;
                float out_ = radius * (0.2f + 0.65f * Hash(k + 1)) * spread;
                Vector3 at = run.C + new Vector3(Mathf.Sin(angle) * out_, 0.45f + 0.6f * Hash(k + 8) + 0.1f * Mathf.Sin(run.Age * 1.4f + k), Mathf.Cos(angle) * out_);
                float size = (1.5f + 0.9f * Hash(k + 4)) * Mathf.Max(0.02f, spread);
                FormFx.Place(solids[k], at, Quaternion.Euler(0f, k * 70f + run.Age * 14f, 0f), new Vector3(size, size * 0.5f, size));
            }
            float across = radius * 2f * spread;
            if (!run.Flag2) FormFx.PlaceMark(SpellWorks.Marks(run)[0], run.C, Quaternion.identity, new Vector3(across, 1f, across));
            if (run.Age >= hangs)
            {
                // What has seeped into the ground stays a while.
                if (run.Flag) { }
                else if (run.Flag2)
                {
                    float yaw = ElementArt.Turn(run.B);
                    ElementArt.Floor(ElementArt.Piece("poison_stain_spell"), run.C, yaw, 0.8f, 0.9f, 3f, 0.4f, true);
                    ElementArt.Floor(ElementArt.Piece("poison_stainrim_spell"), run.C, yaw, 0.8f, 0.9f, 3f, 0.4f, true);
                }
                else FormFx.Lay(FormMark.Scorch, run.C, radius * 2.4f, FormKit.Stain(cast.Colour, 0.7f), 3f);
                return false;
            }
            run.T2 -= deltaTime;
            if (run.T2 <= 0f && !run.Flag)
            {
                run.T2 = 0.08f;
                Vector2 off = Random.insideUnitCircle * radius * 0.9f;
                Vector3 from = run.C + new Vector3(off.x, 0.1f, off.y);
                FormFx.Fly(FormMesh.Bubble, from, from + Vector3.up * Random.Range(1.2f, 2f), Quaternion.LookRotation(Vector3.up), Vector3.one * Random.Range(0.3f, 0.55f), ElementSubstance.Pale(cast.Colour), 0.7f, 0.02f);
            }
            if (run.Age < run.T1) return true;
            run.T1 += Mathf.Max(0.1f, cast.Spec.Interval);
            cast.Combat.StrikeArea(run.C, radius, cast.Blow(cast.Spec.LingerDamage, 0f));
            if (run.Flag)
            {
                // Those inside double over, small puffs of poison popping off them.
                int count = cast.Combat.CopyLastStruck(out var struck);
                for (int i = 0; i < count; i++) ElementMoves.SpellReached(ElementSubstance.Poison, struck[i].transform, run.B);
            }
            else if (run.Flag2)
            {
                // Hooked fumes rise off the front and bend back; pits open just ahead of it.
                float yaw = ElementArt.Turn(run.B);
                Vector3 top = run.C + Vector3.up * 0.3f + FormRules.OnRing(0, 1, radius * 0.6f, run.Age * 97f);
                ElementArt.Fly(ElementArt.Piece(Ruyi(Mathf.FloorToInt(run.Age))), top, top + Vector3.up * 0.8f, Quaternion.Euler(0f, yaw, 0f), 1f, 1.1f);
                ElementArt.Floor(ElementArt.Piece(Pits(Mathf.FloorToInt(run.Age))), run.C + run.B * (radius * 0.9f), yaw, 1.3f, 1.3f, 2.5f);
            }
            else FormFx.Lay(FormMark.Ring, run.C, radius * 0.4f, FormKit.Lit(cast.Colour, 1.8f), 0.4f, radius * 2f);
            // It follows whoever is nearest now.
            if (SigKit.Gather(run, run.C, 5f, 1) > 0) run.Victim = SpellWorks.Things(run)[0];
            return true;
        }
    }
}
