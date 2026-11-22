using Clover.Core;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// What an element's swing does besides its trail, all through the scene's pools (<see cref="FormFx"/>, <see cref="Vfx"/>):
    /// where a blow of it lands, where a slam of a heavy weapon meets the ground, what the trail's head throws off as it
    /// moves, and the ends of Water's and Ice's arcs. Thrown things fall: everything tossed comes down under gravity
    /// and lies a moment before it sinks.
    /// </summary>
    public static class SwingImpacts
    {
        private static readonly Color Soot = new Color(0.07f, 0.045f, 0.035f, 0.85f);
        private static readonly Color Thorn = new Color(0.33f, 0.4f, 0.15f);
        private static readonly Color Rockface = new Color(0.47f, 0.38f, 0.29f);
        private static readonly Color Venom = new Color(0.3f, 0.78f, 0.26f);
        private static readonly Color Gilt = new Color(1f, 0.78f, 0.35f);
        private static readonly Color FrostMist = new Color(0.36f, 0.6f, 0.86f);

        /// <summary>
        /// What a glint's and a crackle's colour is lit to: their materials glow five to eight times past white, so the
        /// colour thrown is kept low and the glow comes out the element's hue at a moderate brightness.
        /// </summary>
        public const float GlintGlow = 0.17f, ShockGlow = 0.3f;

        /// <summary>The ice that bursts from the ground and splinters off a blow: a clear blue whose red stays low, so a lit face is never white.</summary>
        public static readonly Color IceCrystal = new Color(0.42f, 0.7f, 0.92f, 1f);

        private static Color ColourOf(int element)
        {
            var all = Elements.All;
            return element >= 0 && element < all.Count ? all[element].Colour : Color.white;
        }

        private static Vector3 OnGround(Vector3 at, float ground) => new Vector3(at.x, ground + 0.05f, at.z);

        private static Vector3 Flat(Vector3 way)
        {
            way.y = 0f;
            return way.sqrMagnitude > 1e-4f ? way.normalized : Vector3.forward;
        }

        // A way out from a point on the ground: round the ring, leaning toward where the blow was going.
        private static Vector3 Outward(int i, int of, Vector3 toward, float lean)
        {
            float angle = (i + 0.5f) * 360f / Mathf.Max(1, of) + Random.Range(-18f, 18f);
            Vector3 around = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            return (around + toward * lean).normalized;
        }

        /// <summary>A blow of a charged or infused swing lands at <paramref name="at"/> (on the body), going <paramref name="way"/>; <paramref name="ground"/> is the floor's height there.</summary>
        public static void Landing(int element, Vector3 at, Vector3 way, float ground, float weight)
        {
            Color colour = ColourOf(element);
            Vector3 feet = OnGround(at, ground);
            Vector3 flat = Flat(way);
            switch (element)
            {
                case ElementSubstance.Fire:
                    Vfx.Play(VfxKind.Fire, at, Vector3.up, 0.7f * weight);
                    Vfx.Play(VfxKind.Embers, at, flat, 0.8f * weight);
                    FormFx.Lay(FormMark.Scorch, feet, 1.1f * weight, Soot, 2.2f);
                    break;
                case ElementSubstance.Lightning:
                    // The crack where it struck and its crackle in the bolt's violet-blue, not a white starburst (2026-11-20).
                    FormFx.Lay(FormMark.Crack, feet, 1.5f * weight, FormKit.Lit(colour, 1.6f), 0.4f, 1.8f * weight, 0f, Random.value * 360f);
                    Vfx.Play(VfxKind.Shock, at, flat, 0.9f * weight, StruckLook.Spark(colour, ShockGlow));
                    break;
                case ElementSubstance.Wind:
                    FormFx.Fly(FormMesh.Gust, at, at + flat * (2.4f * weight) + Vector3.up * 0.3f, Quaternion.LookRotation(flat), new Vector3(1.4f, 1.4f, 1.8f) * weight, ElementSubstance.Pale(colour), 0.35f);
                    Vfx.Play(VfxKind.Dust, feet, flat, 0.6f * weight);
                    break;
                case ElementSubstance.Ice:
                {
                    // Frost bursts out of the ground about it, thick at the foot, and splinters fly and fall.
                    // Clear ice blue, not the element's pale: lit by the sun, the pale crystals' faces burned white (2026-11-22).
                    Color pale = IceCrystal;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 spot = feet + Outward(i, 3, flat, 0.3f) * 0.45f;
                        FormFx.Erupt(FormMesh.Crystal, spot, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.45f, 0.7f + 0.2f * Random.value, 0.45f) * weight, pale, 0.06f, 0.35f, 0.25f);
                    }
                    // The splinters' glint in a deep ice cyan: the pale lit a white star at every blow (2026-11-22).
                    Vfx.Play(VfxKind.TintSpark, at, flat, 1.4f * weight, StruckLook.Spark(colour, PlayerCombat.ImpactGlow));
                    for (int i = 0; i < 4; i++) Shard(at, Outward(i, 4, flat, 0.6f), ground, pale);
                    break;
                }
                case ElementSubstance.Water:
                    Spray(at, flat, ground, 0.8f * weight, colour);
                    break;
                case ElementSubstance.Life:
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 out_ = Outward(i, 5, flat, 0.5f);
                        FormFx.Fly(FormMesh.Leaf, at, at + out_ * 1.1f + Vector3.down * 0.4f, Quaternion.LookRotation(out_) * Quaternion.Euler(0f, 0f, Random.value * 360f), Vector3.one * (0.35f * weight), colour, 0.5f);
                    }
                    break;
                case ElementSubstance.Earth:
                    Rocks(feet, flat, ground, Mathf.RoundToInt(4f + 2f * weight), weight);
                    Vfx.Play(VfxKind.Dust, feet, Vector3.up, 0.9f * weight);
                    break;
                case ElementSubstance.Metal:
                    // Only a glint of steel where it bit: the guard's white spark burst read as a white starburst (2026-11-20).
                    Vfx.Play(VfxKind.Glint, at, Vector3.up, 1f, StruckLook.Glow(colour, GlintGlow));
                    break;
                case ElementSubstance.Light:
                    // Gold, lit only so far: three times over it came out a cream-white star.
                    FormFx.Face(FormMark.Star, at, 1.1f * weight, FormKit.Lit(Gilt, 1.5f), 0.14f);
                    break;
                case ElementSubstance.Dark:
                    Vfx.Play(VfxKind.TintPuff, at, Vector3.down, 0.9f * weight, ElementSubstance.Shade(colour));
                    break;
                case ElementSubstance.Poison:
                    Spray(at, flat, ground, 0.6f * weight, Venom);
                    Vfx.Play(VfxKind.TintPuff, at, Vector3.down, 0.8f * weight, Venom);
                    break;
            }
        }

        /// <summary>A heavy weapon's blow meets the ground at <paramref name="at"/>: the element thrown out round it, <paramref name="radius"/> metres.</summary>
        public static void Ground(int element, Vector3 at, Vector3 way, float ground, float radius, float weight)
        {
            Color colour = ColourOf(element);
            Vector3 feet = OnGround(at, ground);
            Vector3 flat = Flat(way);
            int ring = Mathf.Clamp(Mathf.RoundToInt(4f + radius * 2f), 4, 9);
            switch (element)
            {
                case ElementSubstance.Fire:
                    FormFx.Lay(FormMark.Scorch, feet, radius * 1.6f, Soot, 3f);
                    for (int i = 0; i < ring; i++)
                        FormFx.Erupt(FormMesh.Flame, feet + Outward(i, ring, flat, 0f) * (radius * 0.6f), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.5f, 0.8f + 0.5f * Random.value, 0.5f) * weight, colour, 0.06f, 0.3f, 0.3f);
                    Vfx.Play(VfxKind.Embers, feet + Vector3.up * 0.3f, Vector3.up, 1.4f * weight);
                    break;
                case ElementSubstance.Lightning:
                    FormFx.Lay(FormMark.Crack, feet, radius * 1.8f, FormKit.Lit(colour, 1.6f), 0.5f, radius * 2.2f, 0f, Random.value * 360f);
                    Clover.World.LightningBolts.Strike(feet, 0.3f);
                    break;
                case ElementSubstance.Wind:
                    for (int i = 0; i < ring; i++)
                    {
                        Vector3 out_ = Outward(i, ring, flat, 0f);
                        Vector3 around = Vector3.Cross(Vector3.up, out_);
                        FormFx.Fly(FormMesh.Gust, feet + out_ * 0.4f + Vector3.up * 0.3f, feet + out_ * (radius * 1.3f) + around * 0.5f + Vector3.up * 0.5f, Quaternion.LookRotation(around), new Vector3(1.4f, 1.4f, 1.6f), ElementSubstance.Pale(colour), 0.35f);
                    }
                    break;
                case ElementSubstance.Ice:
                    for (int i = 0; i < ring; i++)
                        FormFx.Erupt(FormMesh.Crystal, feet + Outward(i, ring, flat, 0f) * (radius * 0.55f), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.6f, 0.9f + 0.4f * Random.value, 0.6f) * weight, IceCrystal, 0.07f, 0.6f, 0.3f);
                    break;
                case ElementSubstance.Water:
                    // No ring of light round the slam (the owner: no rings or circles, 2026-11-20): the water thrown out alone.
                    for (int i = 0; i < ring; i++) Spray(feet + Vector3.up * 0.2f, Outward(i, ring, flat, 0f), ground, 0.5f * weight, colour);
                    break;
                case ElementSubstance.Life:
                    for (int i = 0; i < ring; i++)
                        FormFx.Erupt(FormMesh.Vine, feet + Outward(i, ring, flat, 0f) * (radius * 0.6f), Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.5f, 0.9f, 0.5f) * weight, Thorn, 0.15f, 0.5f, 0.3f);
                    break;
                case ElementSubstance.Earth:
                    FormFx.Lay(FormMark.Crack, feet, radius * 1.6f, new Color(0.18f, 0.13f, 0.09f, 0.9f), 1.6f, radius * 1.9f, 0f, Random.value * 360f);
                    Rocks(feet, flat, ground, ring + 3, weight * 1.2f);
                    Vfx.Play(VfxKind.Dust, feet, Vector3.up, 1.5f * weight);
                    break;
                case ElementSubstance.Metal:
                    Vfx.Play(VfxKind.Glint, feet + Vector3.up * 0.2f, Vector3.up, 1f, StruckLook.Glow(colour, GlintGlow));
                    for (int i = 0; i < 4; i++) Vfx.Play(VfxKind.TintSpark, feet + Outward(i, 4, flat, 0f) * 0.4f + Vector3.up * 0.1f, Outward(i, 4, flat, 0f), 0.5f, StruckLook.Spark(colour, 0.3f));
                    break;
                case ElementSubstance.Light:
                    // Gold flecks thrown up off the slam, cooling to burnt gold: no ring and no white-hot star (2026-11-20).
                    Vfx.Play(VfxKind.TintSpark, feet + Vector3.up * 0.3f, Vector3.up, 1.2f * weight, StruckLook.Spark(Gilt, 0.3f));
                    FormFx.Lay(FormMark.Scorch, feet, radius * 0.9f, FormKit.Stain(new Color(0.42f, 0.2f, 0.02f), 0.6f), 1.2f);
                    break;
                case ElementSubstance.Dark:
                    FormFx.Lay(FormMark.Scorch, feet, radius * 1.5f, FormKit.Stain(colour, 0.85f), 1.4f, radius * 0.6f);
                    Vfx.Play(VfxKind.TintPuff, feet + Vector3.up * 0.3f, Vector3.down, 1.2f * weight, ElementSubstance.Shade(colour));
                    break;
                case ElementSubstance.Poison:
                    // A venom stain, not a lit disc (no circles, 2026-11-20).
                    FormFx.Lay(FormMark.Scorch, feet, radius * 1.2f, FormKit.Stain(new Color(0.17f, 0.28f, 0.025f), 0.7f), 1.4f);
                    for (int i = 0; i < ring; i++) Spray(feet + Vector3.up * 0.2f, Outward(i, ring, flat, 0f), ground, 0.4f * weight, Venom);
                    break;
            }
        }

        /// <summary>What the trail's head throws off as it moves, at the tip of the blade line.</summary>
        public static void Shed(SwingShed shed, int element, Vector3 tip, Vector3 way, float ground, float weight)
        {
            Color colour = ColourOf(element);
            switch (shed)
            {
                case SwingShed.Embers:
                    Vfx.Play(VfxKind.Embers, tip, Vector3.up, 0.35f * weight);
                    break;
                case SwingShed.Crackle:
                    // A cyan-violet crackle, never a plain white one (its cores burned white, 2026-11-22).
                    Vfx.Play(VfxKind.Shock, tip, way, 0.3f * weight, StruckLook.Spark(colour, ShockGlow));
                    break;
                case SwingShed.FrostBreath:
                    // Cold breath in the elements 3 frost mist's blue: the pale of ice lit to a white dot on film (2026-11-20).
                    Vfx.Play(VfxKind.TintPuff, tip, Vector3.down, 0.1f * weight, FrostMist);
                    break;
                case SwingShed.Grit:
                    Vfx.Play(VfxKind.Dust, tip, Vector3.down, 0.25f * weight);
                    break;
                case SwingShed.Leaves:
                {
                    Vector3 side = Vector3.Cross(way, Vector3.up) * (Random.value - 0.5f);
                    FormFx.Fly(FormMesh.Leaf, tip, tip + way * 0.6f + Vector3.down * 0.7f + side, Quaternion.LookRotation(way.sqrMagnitude > 1e-4f ? way : Vector3.forward) * Quaternion.Euler(0f, 0f, Random.value * 360f),
                        Vector3.one * (0.18f * weight), colour, 0.6f);
                    break;
                }
                case SwingShed.Drips:
                    Drip(tip, way * 0.8f, ground, Venom, weight);
                    if (Random.value < 0.35f) Vfx.Play(VfxKind.TintPuff, tip, Vector3.down, 0.12f * weight, Venom);
                    break;
            }
        }

        /// <summary>Water thrown on off the end of an arc: drops that fly on the way it was going and fall.</summary>
        public static void Spray(Vector3 from, Vector3 way, float ground, float weight, Color colour)
        {
            int drops = Mathf.Clamp(Mathf.RoundToInt(6f * weight), 2, 9);
            Vector3 side = Vector3.Cross(Vector3.up, way);
            for (int i = 0; i < drops; i++)
            {
                Vector3 velocity = way * Random.Range(2.5f, 4f) + Vector3.up * Random.Range(1f, 2.5f) + side * Random.Range(-0.8f, 0.8f);
                FormFx.Toss(FormMesh.Droplet, from, velocity, ground + 0.03f, Quaternion.LookRotation(velocity), Vector3.one * Random.Range(0.12f, 0.2f), colour, 0.15f, 0.2f);
            }
        }

        /// <summary>A splinter of a frozen arc thrown out and falling.</summary>
        public static void Shard(Vector3 from, Vector3 outward, float ground, Color colour)
        {
            Vector3 velocity = outward * Random.Range(1.2f, 2.4f) + Vector3.up * Random.Range(0.8f, 1.8f);
            FormFx.Toss(FormMesh.Shard, from, velocity, ground + 0.03f, Quaternion.LookRotation(velocity), new Vector3(0.12f, 0.12f, 0.3f), colour, 0.2f, 0.2f);
        }

        /// <summary>A drop falling off a trail or an edge.</summary>
        public static void Drip(Vector3 from, Vector3 velocity, float ground, Color colour, float weight)
        {
            FormFx.Toss(FormMesh.Droplet, from, velocity + Vector3.down * 0.5f, ground + 0.03f, Quaternion.LookRotation(Vector3.down), Vector3.one * (0.12f * weight), colour, 0.25f, 0.15f);
        }

        // Rocks thrown up out of the ground, leaning the way the blow went, each falling back to lie a moment and sink.
        private static void Rocks(Vector3 feet, Vector3 flat, float ground, int count, float weight)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 out_ = Outward(i, count, flat, 0.6f);
                Vector3 velocity = out_ * Random.Range(1f, 2.2f) + Vector3.up * Random.Range(3.4f, 5.4f);
                FormFx.Toss(FormMesh.Pebble, feet + Vector3.up * 0.15f + out_ * 0.2f, velocity, ground + 0.08f, Random.rotation, Vector3.one * (Random.Range(0.25f, 0.4f) * weight), Rockface, 0.35f, 0.3f);
            }
        }

        /// <summary>The colour of Life's thorns: woody, not the leaf's green, so a thorn is read as a thorn.</summary>
        public static Color ThornColour => Thorn;
    }
}
