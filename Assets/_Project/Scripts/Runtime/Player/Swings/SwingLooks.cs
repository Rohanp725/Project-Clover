using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The eleven elements on a swung weapon, designed from the owner's comments on the effects of 2026-11-19
    /// (docs/design/feedback/2026-11-19_effects_review.md): each reads as its element moving with the swing, laid on the
    /// real path of the weapon's blade line, and nothing is stuck along the blade but Life's thorns. Lightning keeps the
    /// spirit of its liked set. Trails are drawn with the game's `Clover/Glow` shader in each element's own band.
    /// </summary>
    public static class SwingLooks
    {
        // Sweeps are laid on the shape's window so the outer edge is the bright one (V 0 to 0.5) and the head the whole (U to 0.8).
        private static GlowStyle Lit(Color colour, float shape = 2f, float sharpness = 1.6f) => new GlowStyle
        {
            Shape = shape, Additive = true, Sharpness = sharpness, Colour = colour, U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f,
        };

        private static GlowStyle Laid(Color colour, float shape = 2f, float sharpness = 1.2f) => new GlowStyle
        {
            Shape = shape, Additive = false, Sharpness = sharpness, Colour = colour, U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f,
        };

        private static GlowStyle Burning(float noiseScale, Vector2 noiseSpeed) => new GlowStyle
        {
            Shape = 2f, Fire = true, Additive = false, Sharpness = 1f, Colour = Color.white, NoiseScale = noiseScale, NoiseSpeed = noiseSpeed,
            U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f,
        };

        // Flame across the whole strip: the churn and the licking tongues make its edges, not the slash's outline.
        private static GlowStyle Ablaze(float noiseScale, Vector2 noiseSpeed)
        {
            var style = Burning(noiseScale, noiseSpeed);
            // The arc is several times longer than it is wide: the churn is spread along it (U whole, V a quarter) so
            // its blobs stand across the arc as tongues, not lie along it in stripes.
            style.Shape = 3f;
            style.U0 = 0f;
            style.U1 = 1f;
            style.V0 = 0f;
            style.V1 = 0.25f;
            return style;
        }

        private static GlowStyle Broken(GlowStyle style, float noise, float scale, Vector2 speed)
        {
            style.Noise = noise;
            style.NoiseScale = scale;
            style.NoiseSpeed = speed;
            return style;
        }

        private static GlowStyle Centred(GlowStyle style)
        {
            style.V0 = 0f;
            style.V1 = 1f;
            return style;
        }

        // A streak pinched at its tail and whole at its head.
        private static GlowStyle Streak(Color colour) => new GlowStyle
        {
            Shape = 1f, Additive = true, Sharpness = 1.4f, Colour = colour, U0 = 0f, U1 = 0.5f, V0 = 0f, V1 = 1f,
        };

        // A bolt drawn along the strip (the shader draws it along V).
        private static GlowStyle Bolt(Color colour, float sharpness = 1.4f) => new GlowStyle
        {
            Shape = 7f, Additive = true, Sharpness = sharpness, Colour = colour, U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f, Swap = true,
        };

        private static TrailLayer Licking(TrailLayer layer, float lick, float every)
        {
            layer.Lick = lick;
            layer.LickEvery = every;
            return layer;
        }

        private static readonly Color Whole = Color.white;
        private static Color Gone(float r = 1f, float g = 1f, float b = 1f) => new Color(r, g, b, 0f);
        private static Color Seen(float alpha) => new Color(1f, 1f, 1f, alpha);

        private static TrailLayer Sweep(float from, float to, float seconds, GlowStyle style, Color head, Color tail) => new TrailLayer
        {
            Form = TrailForm.Sweep, From = from, To = to, Seconds = seconds, Style = style, Head = head, Tail = tail,
        };

        private static TrailLayer Strand(float at, float width, float seconds, GlowStyle style, Color head, Color tail) => new TrailLayer
        {
            Form = TrailForm.Strand, At = at, Width = width, Seconds = seconds, Style = style, Head = head, Tail = tail,
        };

        private static TrailLayer Moving(TrailLayer layer, float rise = 0f, float drift = 0f, float scatter = 0f, float taper = 0f)
        {
            layer.Rise = rise;
            layer.Drift = drift;
            layer.Scatter = scatter;
            layer.Taper = taper;
            return layer;
        }

        /// <summary>
        /// One step of an elements 3 trail (the Blender worker's blades): a flat band of one colour across a share of the
        /// sweep, drawn on with the stroke and wiped off from its tail within a third of a second, never faded to white.
        /// </summary>
        private static TrailLayer Band(float from, float to, float r, float g, float b, float seconds = 0.36f) => new TrailLayer
        {
            Form = TrailForm.Sweep, From = from, To = to, Seconds = seconds, Head = Whole, Tail = Whole, Style = Flat(r, g, b),
        };

        private static GlowStyle Flat(float r, float g, float b) => new GlowStyle
        {
            Shape = 3f, Additive = false, Sharpness = 1f, Colour = new Color(r, g, b, 1f), U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f,
        };

        // The crescent of toon steps from the sweep's inner edge to its outer, as the recipe's five tubes (darkest, middle,
        // lightest, middle; its fifth, darkest again, is the inked rim).
        private static TrailLayer[] Bands(float r0, float g0, float b0, float r1, float g1, float b1, float r2, float g2, float b2, float r3, float g3, float b3) => new[]
        {
            Band(0f, 0.3f, r0, g0, b0), Band(0.3f, 0.55f, r1, g1, b1), Band(0.55f, 0.82f, r2, g2, b2), Band(0.82f, 1f, r3, g3, b3),
        };

        // Wind's steps blow on the way the blade went and come apart in curls past the end of the stroke, never cut off.
        private static TrailLayer[] Blowing(TrailLayer[] bands)
        {
            for (int i = 0; i < bands.Length; i++)
            {
                bands[i].Seconds = 0.5f;
                bands[i].Drift = 2.2f;
                bands[i].Scatter = 0.6f;
                bands[i].Taper = -0.5f;
            }
            return bands;
        }

        public static readonly SwingLook Fire = new SwingLook
        {
            Element = 0, Name = Elements.Fire,
            // The struck tint: the flame's orange band in the Glow shader, which draws the fire trail.
            Tint = new Color(2f, 0.52f, 0.06f), Shed = SwingShed.Embers, ShedEvery = 0.035f,
            Line = "a flame ribbon off the swing that licks upward once left, cools through red to smoke so the arc is scorched behind it, and sheds embers",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.82f, 0.14f, 0.02f, 1f, 0.4f, 0.04f, 1f, 0.68f, 0.14f, 1f, 0.4f, 0.04f),
        };

        public static readonly SwingLook Lightning = new SwingLook
        {
            Element = 1, Name = Elements.Lightning,
            // The struck tint: the rim's.
            Tint = new Color(1f, 1.4f, 2.9f), Shed = SwingShed.Crackle, ShedEvery = 0.04f,
            Line = "the liked crackle kept: a hard blue-white rim on the cut and two jagged arcs that crawl along the sweep, taking a new shape twenty times a second; a crack in the floor where a blow lands",
            Layers = new[]
            {
                // Filmed 2026-11-20 the arcs were gone before they were seen from the game camera: wider, brighter and
                // a little longer, still jumping twenty times a second.
                Sweep(0.5f, 1f, 0.2f, Lit(new Color(1f, 1.4f, 2.9f), 2f, 2f), Whole, Gone(0.6f, 0.7f, 1f)),
                new TrailLayer { Form = TrailForm.Strand, At = 0.97f, Width = 0.55f, Seconds = 0.32f, Style = Bolt(new Color(1.8f, 2.2f, 4.2f), 0.6f), Head = Whole, Tail = Gone(), Jitter = 0.08f },
                new TrailLayer { Form = TrailForm.Strand, At = 0.6f, Width = 0.42f, Seconds = 0.26f, Style = Bolt(new Color(2f, 2.3f, 4.4f), 0.6f), Head = Whole, Tail = Gone(), Jitter = 0.06f },
            },
        };

        public static readonly SwingLook Wind = new SwingLook
        {
            Element = 2, Name = Elements.Wind,
            // The struck tint: the teal of the elements 3 trail's middle step. The old air-blade's near-white lit the
            // struck bodies white (the swing films of 2026-11-20). No cut streak through the body: wind goes round it.
            Tint = new Color(0.22f, 0.7f, 0.5f), CutsRound = true,
            Line = "an air-blade: a pale see-through sweep with a bright leading streak that, once left, blows on the way the blade went, spreads and comes apart in gusts until nothing is left (never cut off)",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Blowing(Bands(0.06f, 0.34f, 0.3f, 0.22f, 0.7f, 0.5f, 0.5f, 0.86f, 0.66f, 0.22f, 0.7f, 0.5f)),
        };

        public static readonly SwingLook Ice = new SwingLook
        {
            Element = 3, Name = Elements.Ice,
            // The struck tint: the crystal arc's.
            Tint = new Color(0.72f, 0.92f, 1.05f), Shed = SwingShed.FrostBreath, ShedEvery = 0.08f, ReplacesHitArt = true,
            Line = "the swing leaves a crystal arc that freezes in the air, hangs, then shatters into shards that fall; frost and crystals where a blow lands, no crack-web",
            // Glacier ice, paler than Water's blue (the owner, 2026-11-20: Ice and Water swung the same blue): a steel-blue
            // edge, a pale sky middle and a frost tip still tinted blue, so it is never pure white and never Metal's grey.
            // Filmed at the owner's first values (0.30/0.62/0.86 red) it read silver-grey beside Metal: a touch bluer.
            Layers = Bands(0.22f, 0.46f, 0.68f, 0.5f, 0.8f, 0.96f, 0.74f, 0.93f, 1.0f, 0.5f, 0.8f, 0.96f),
        };

        public static readonly SwingLook Water = new SwingLook
        {
            Element = 4, Name = Elements.Water,
            // The struck tint: the water arc's.
            Tint = new Color(0.18f, 0.62f, 0.9f), End = SwingEnd.Spray,
            Line = "the swing draws a water arc with a foam rim that falls as it is left, and throws its water off the end of the swing in a spray of drops (none dotted on the blade)",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.02f, 0.13f, 0.42f, 0.04f, 0.34f, 0.78f, 0.22f, 0.68f, 0.94f, 0.04f, 0.34f, 0.78f),
        };

        public static readonly SwingLook Life = new SwingLook
        {
            Element = 5, Name = Elements.Life,
            // The struck tint: the green sweep's.
            Tint = new Color(0.4f, 0.92f, 0.28f), Shed = SwingShed.Leaves, ShedEvery = 0.07f,
            Line = "a green sweep that sheds leaves, and pollen drifting off the point that rises and scatters",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.05f, 0.25f, 0.04f, 0.15f, 0.5f, 0.08f, 0.38f, 0.75f, 0.16f, 0.15f, 0.5f, 0.08f),
        };

        public static readonly SwingLook Earth = new SwingLook
        {
            Element = 6, Name = Elements.Earth,
            // The struck tint: the dust's.
            Tint = new Color(0.66f, 0.5f, 0.32f), Shed = SwingShed.Grit, ShedEvery = 0.08f, ReplacesHitArt = true,
            Line = "a dust trail that settles and spreads, with grit falling out of it; where a blow lands rocks are thrown up, fall, lie a moment and sink (never floating)",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.2f, 0.13f, 0.07f, 0.4f, 0.29f, 0.16f, 0.6f, 0.47f, 0.29f, 0.4f, 0.29f, 0.16f),
        };

        public static readonly SwingLook Metal = new SwingLook
        {
            Element = 7, Name = Elements.Metal,
            // The struck tint: the glint's.
            // A cold steel blue: the glint's own near-white lit the struck body white (metal's film of 2026-11-20).
            Tint = new Color(1f, 1.3f, 2f),
            Line = "nothing added to the weapon: the tip leaves a hair-thin streak for a twelfth of a second",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = new[] { Strand(1f, 0.07f, 0.2f, Flat(0.34f, 0.39f, 0.46f), Whole, Whole) },
        };

        public static readonly SwingLook Light = new SwingLook
        {
            Element = 8, Name = Elements.Light,
            // The struck tint: the arc of light's.
            Tint = new Color(1.7f, 1.3f, 0.55f),
            Line = "the swing leaves a clean, hard-edged arc of light that fades without breaking up",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.62f, 0.25f, 0.02f, 1f, 0.5f, 0.06f, 1f, 0.74f, 0.24f, 1f, 0.5f, 0.06f),
        };

        public static readonly SwingLook Dark = new SwingLook
        {
            Element = 9, Name = Elements.Dark,
            // The struck tint: the violet rim's: the shade under it is too dark to light a flash.
            Tint = new Color(1f, 0.42f, 1.7f),
            Line = "a smear of shade that lags behind the blade and drains back into it as it sinks, with a violet rim (no crystals, no symbols)",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = new[] { Band(0f, 0.85f, 0.02f, 0.004f, 0.035f), Band(0.85f, 1f, 0.36f, 0.09f, 0.72f) },
        };

        public static readonly SwingLook Poison = new SwingLook
        {
            Element = 10, Name = Elements.Poison,
            // The struck tint: the venom trail's.
            Tint = new Color(0.38f, 0.88f, 0.3f), Shed = SwingShed.Drips, ShedEvery = 0.05f,
            Line = "venom: a green trail that sags and drips as it is left, under a heavy mist that sinks off the arc",
            // The elements 3 trail (staging/blender/2026-11-20_elements3): flat toon steps, never lit to white.
            Layers = Bands(0.1f, 0.25f, 0.02f, 0.32f, 0.6f, 0.05f, 0.6f, 0.82f, 0.12f, 0.32f, 0.6f, 0.05f),
        };

        private static GlowStyle CoreOf(GlowStyle style, float core)
        {
            style.Core = core;
            return style;
        }

        /// <summary>Every element's look, by its place on the ring.</summary>
        public static readonly SwingLook[] All = { Fire, Lightning, Wind, Ice, Water, Life, Earth, Metal, Light, Dark, Poison };

        /// <summary>The most layers any look has.</summary>
        public const int MostLayers = 4;

        /// <summary>An element's look by its place on the ring; null for none.</summary>
        public static SwingLook For(int element) => element >= 0 && element < All.Length ? All[element] : null;

        /// <summary>An element's look; null for none.</summary>
        public static SwingLook For(ElementInfo element) => element != null ? For(element.Index) : null;
    }
}
