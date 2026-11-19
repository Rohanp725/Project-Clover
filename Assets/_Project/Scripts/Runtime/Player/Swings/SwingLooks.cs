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
        private static GlowStyle Bolt(Color colour) => new GlowStyle
        {
            Shape = 7f, Additive = true, Sharpness = 1.4f, Colour = colour, U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f, Swap = true,
        };

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

        public static readonly SwingLook Fire = new SwingLook
        {
            Element = 0, Name = Elements.Fire, Shed = SwingShed.Embers, ShedEvery = 0.035f,
            Line = "a flame ribbon off the swing that licks upward once left, cools through red to smoke so the arc is scorched behind it, and sheds embers; a thin lick of flame along the edge while charged",
            Layers = new[]
            {
                Moving(Sweep(0f, 1f, 0.42f, Burning(4.5f, new Vector2(-1.2f, -2.2f)), Whole, Gone()), rise: 0.9f, scatter: 0.1f, taper: 0.35f),
                Moving(Sweep(0.7f, 1f, 0.2f, Burning(7f, new Vector2(-2f, -3f)), new Color(1.15f, 1.1f, 1f, 1f), Gone()), rise: 0.5f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Burning(9f, new Vector2(0f, -3.2f))), Width = 0.08f, Inside = 0.35f, Flicker = 0.15f },
        };

        public static readonly SwingLook Lightning = new SwingLook
        {
            Element = 1, Name = Elements.Lightning, Shed = SwingShed.Crackle, ShedEvery = 0.06f,
            Line = "the liked crackle kept: a hard blue-white rim on the cut and two jagged arcs that crawl along the sweep, taking a new shape twenty times a second; a bolt running the edge while charged; a crack in the floor where a blow lands",
            Layers = new[]
            {
                Sweep(0.55f, 1f, 0.14f, Lit(new Color(0.9f, 1.3f, 2.6f), 2f, 2.2f), Seen(0.9f), Gone(0.6f, 0.7f, 1f)),
                new TrailLayer { Form = TrailForm.Strand, At = 0.97f, Width = 0.34f, Seconds = 0.22f, Style = Bolt(new Color(0.8f, 1.1f, 2.6f)), Head = Whole, Tail = Gone(), Jitter = 0.05f },
                new TrailLayer { Form = TrailForm.Strand, At = 0.6f, Width = 0.26f, Seconds = 0.18f, Style = Bolt(new Color(1f, 1.2f, 2.8f)), Head = Whole, Tail = Gone(), Jitter = 0.04f },
            },
            Sheath = new SheathStyle { Shown = true, Style = Bolt(new Color(0.8f, 1.1f, 2.6f)), Width = 0.14f, Inside = 0.5f, Flicker = 0.45f, Rebolt = 18f },
        };

        public static readonly SwingLook Wind = new SwingLook
        {
            Element = 2, Name = Elements.Wind,
            Line = "an air-blade: a pale see-through sweep with a bright leading streak that, once left, blows on the way the blade went, spreads and comes apart in gusts until nothing is left (never cut off); a faint stream of air along the edge while charged",
            Layers = new[]
            {
                Moving(Sweep(0f, 1f, 0.55f, Broken(Lit(new Color(0.85f, 0.95f, 0.9f)), 0.85f, 8f, new Vector2(-2.4f, 0.3f)), Seen(0.6f), Gone()), drift: 2.2f, scatter: 0.8f, taper: -0.6f),
                Moving(Strand(1.02f, 0.07f, 0.35f, Broken(Streak(new Color(1.2f, 1.3f, 1.25f)), 0.6f, 12f, new Vector2(-3f, 0f)), Seen(0.8f), Gone()), drift: 3f, scatter: 1.4f, taper: -0.3f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Broken(Lit(new Color(0.8f, 0.9f, 0.85f, 0.35f)), 0.6f, 10f, new Vector2(3f, 0f))), Width = 0.06f, Inside = 0.5f, Flicker = 0.1f },
        };

        public static readonly SwingLook Ice = new SwingLook
        {
            Element = 3, Name = Elements.Ice, Shed = SwingShed.FrostBreath, ShedEvery = 0.08f, End = SwingEnd.Shatter, ReplacesHitArt = true,
            Line = "the chainsaw of ice: a row of frost teeth that run along the edge while charged; the swing leaves a crystal arc that freezes in the air, hangs, then shatters into shards that fall; frost and crystals where a blow lands, no crack-web",
            Layers = new[]
            {
                new TrailLayer { Form = TrailForm.Sweep, From = 0f, To = 1f, Seconds = 0.9f, Hold = 0.55f, Head = Seen(0.9f), Tail = Seen(0.9f),
                    Style = Broken(new GlowStyle { Shape = 2f, Additive = false, Sharpness = 1f, Hard = 0.22f, Colour = new Color(0.72f, 0.92f, 1.05f, 0.9f), U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f }, 0.45f, 16f, Vector2.zero) },
                new TrailLayer { Form = TrailForm.Sweep, From = 0.8f, To = 1f, Seconds = 0.9f, Hold = 0.55f, Head = Whole, Tail = Whole,
                    Style = new GlowStyle { Shape = 2f, Additive = true, Sharpness = 1f, Hard = 0.3f, Colour = new Color(1.3f, 1.6f, 1.9f), U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f } },
            },
            Sheath = new SheathStyle
            {
                Shown = true, Teeth = true, ToothEvery = 0.055f, ToothRun = 1.4f, Width = 0.075f, Inside = 0.1f,
                Style = new GlowStyle { Shape = 3f, Additive = false, Sharpness = 1f, Colour = new Color(0.8f, 0.95f, 1.08f, 0.92f), U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f },
            },
        };

        public static readonly SwingLook Water = new SwingLook
        {
            Element = 4, Name = Elements.Water, End = SwingEnd.Spray,
            Line = "a sheath of water flowing along the edge toward the point; the swing draws a water arc with a foam rim that falls as it is left, and throws its water off the end of the swing in a spray of drops (none dotted on the blade)",
            Layers = new[]
            {
                Moving(Sweep(0f, 1f, 0.38f, CoreOf(Broken(Laid(new Color(0.18f, 0.62f, 0.9f, 0.9f)), 0.5f, 6f, new Vector2(-3f, 0f)), 0.92f), Seen(0.9f), Gone(0.6f, 0.85f, 1f)), rise: -0.5f, taper: 0.4f),
                Moving(Sweep(0.85f, 1f, 0.24f, Broken(Lit(new Color(1.1f, 1.35f, 1.5f)), 0.7f, 10f, new Vector2(-4f, 0f)), Seen(0.8f), Gone()), rise: -0.6f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(CoreOf(Broken(Laid(new Color(0.2f, 0.65f, 0.95f, 0.85f)), 0.55f, 8f, new Vector2(4.5f, 0f)), 0.9f)), Width = 0.065f, Inside = 0.5f },
        };

        public static readonly SwingLook Life = new SwingLook
        {
            Element = 5, Name = Elements.Life, Thorns = true, Shed = SwingShed.Leaves, ShedEvery = 0.07f,
            Line = "thorns grown along the real edge of each weapon while charged (the one look on the weapon, as the owner asked), a green sweep that sheds leaves, and pollen drifting off the point that rises and scatters",
            Layers = new[]
            {
                Moving(Sweep(0.2f, 1f, 0.4f, Broken(Laid(new Color(0.32f, 0.78f, 0.24f, 0.8f)), 0.7f, 12f, new Vector2(-1f, 0.5f)), Seen(0.8f), Gone(0.7f, 0.9f, 0.4f)), rise: 0.15f, drift: 0.5f, scatter: 0.2f),
                Moving(Strand(1f, 0.3f, 0.7f, Broken(Streak(new Color(1.4f, 1.3f, 0.55f)), 0.92f, 24f, new Vector2(0f, 0.4f)), Seen(0.7f), Gone()), rise: 0.35f, drift: 0.4f, scatter: 0.45f, taper: -0.8f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Broken(Laid(new Color(0.3f, 0.7f, 0.22f, 0.45f)), 0.5f, 14f, Vector2.zero)), Width = 0.04f, Inside = 0.5f },
        };

        public static readonly SwingLook Earth = new SwingLook
        {
            Element = 6, Name = Elements.Earth, Shed = SwingShed.Grit, ShedEvery = 0.08f, ReplacesHitArt = true,
            Line = "a dust trail that settles and spreads, with grit falling out of it; where a blow lands rocks are thrown up, fall, lie a moment and sink (never floating); grit along the edge while charged",
            Layers = new[]
            {
                Moving(Sweep(0f, 1f, 0.6f, Broken(Laid(new Color(0.46f, 0.34f, 0.22f, 0.8f)), 0.9f, 9f, new Vector2(0f, -0.6f)), Seen(0.85f), Gone(0.8f, 0.75f, 0.7f)), rise: -0.35f, scatter: 0.25f, taper: -0.5f),
                Moving(Strand(1f, 0.22f, 0.45f, Broken(Centred(Laid(new Color(0.24f, 0.17f, 0.11f, 0.95f))), 1f, 28f, Vector2.zero), Seen(0.9f), Gone()), rise: -1.3f, scatter: 0.3f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Broken(Laid(new Color(0.32f, 0.24f, 0.16f, 0.7f)), 0.9f, 26f, Vector2.zero)), Width = 0.05f, Inside = 0.5f },
        };

        public static readonly SwingLook Metal = new SwingLook
        {
            Element = 7, Name = Elements.Metal,
            Line = "nothing added to the weapon but a glint: a point of light runs from the guard to the tip while charged and as each swing starts, and the tip leaves a hair-thin streak for a twelfth of a second",
            Layers = new[]
            {
                Strand(1f, 0.035f, 0.08f, Streak(new Color(2.2f, 2.3f, 2.5f)), Whole, Gone()),
            },
            Sheath = new SheathStyle
            {
                Shown = true, Glint = true, GlintEvery = 1.4f, GlintSeconds = 0.28f, GlintSpan = 0.22f, Width = 0.05f, Inside = 0.5f,
                Style = new GlowStyle { Shape = 1f, Additive = true, Sharpness = 1.4f, Colour = new Color(2.6f, 2.7f, 3f), U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f },
            },
        };

        public static readonly SwingLook Light = new SwingLook
        {
            Element = 8, Name = Elements.Light,
            Line = "the edge itself glows gold with a white heart, and the swing leaves a clean, hard-edged arc of light that fades without breaking up",
            Layers = new[]
            {
                Sweep(0f, 1f, 0.3f, new GlowStyle { Shape = 2f, Additive = true, Sharpness = 1.4f, Hard = 0.3f, Core = 0.86f, Colour = new Color(1.7f, 1.3f, 0.55f), U0 = 0.45f, U1 = 0.8f, V0 = 0f, V1 = 0.5f }, Whole, Gone(1f, 0.9f, 0.7f)),
            },
            Sheath = new SheathStyle { Shown = true, Style = new GlowStyle { Shape = 2f, Additive = true, Sharpness = 1.2f, Hard = 0.35f, Core = 0.75f, Colour = new Color(2.1f, 1.65f, 0.8f), U0 = 0f, U1 = 1f, V0 = 0f, V1 = 1f }, Width = 0.055f, Inside = 0.5f, Flicker = 0.05f },
        };

        public static readonly SwingLook Dark = new SwingLook
        {
            Element = 9, Name = Elements.Dark,
            Line = "a smear of shade that lags behind the blade and drains back into it as it sinks, with a violet rim; a shadow clinging along the edge while charged (no crystals, no symbols)",
            Layers = new[]
            {
                new TrailLayer { Form = TrailForm.Sweep, From = 0f, To = 1f, Seconds = 0.5f, Lag = 0.07f, Drain = 0.85f, Rise = -0.2f, Head = Seen(0.92f), Tail = Gone(),
                    Style = Broken(Laid(new Color(0.07f, 0.03f, 0.11f, 0.92f)), 0.6f, 7f, new Vector2(1.6f, 0f)) },
                new TrailLayer { Form = TrailForm.Sweep, From = 0.82f, To = 1f, Seconds = 0.32f, Lag = 0.07f, Drain = 0.6f, Head = Seen(0.8f), Tail = Gone(),
                    Style = Lit(new Color(0.6f, 0.26f, 1f)) },
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Broken(Laid(new Color(0.06f, 0.02f, 0.1f, 0.75f)), 0.7f, 9f, new Vector2(-1.2f, 0f))), Width = 0.07f, Inside = 0.5f },
        };

        public static readonly SwingLook Poison = new SwingLook
        {
            Element = 10, Name = Elements.Poison, Shed = SwingShed.Drips, ShedEvery = 0.05f,
            Line = "venom: a green trail that sags and drips as it is left, under a heavy mist that sinks off the arc; a coat of venom along the edge while charged that now and then drips off",
            Layers = new[]
            {
                Moving(Sweep(0f, 1f, 0.5f, Broken(Laid(new Color(0.34f, 0.82f, 0.3f, 0.88f)), 0.6f, 9f, new Vector2(0f, 1f)), Seen(0.9f), Gone(0.6f, 0.8f, 0.4f)), rise: -0.7f, taper: -0.3f),
                Moving(Sweep(0f, 0.7f, 0.95f, Broken(Centred(Laid(new Color(0.28f, 0.46f, 0.22f, 0.5f))), 0.85f, 4f, new Vector2(0f, 0.5f)), Seen(0.55f), Gone()), rise: -0.55f, scatter: 0.3f, taper: -0.9f),
            },
            Sheath = new SheathStyle { Shown = true, Style = Centred(Broken(Laid(new Color(0.32f, 0.78f, 0.28f, 0.8f)), 0.6f, 10f, new Vector2(0f, -1f))), Width = 0.06f, Inside = 0.5f },
        };

        private static GlowStyle CoreOf(GlowStyle style, float core)
        {
            style.Core = core;
            return style;
        }

        /// <summary>Every element's look, by its place on the ring.</summary>
        public static readonly SwingLook[] All = { Fire, Lightning, Wind, Ice, Water, Life, Earth, Metal, Light, Dark, Poison };

        /// <summary>The most layers any look has.</summary>
        public const int MostLayers = 3;

        /// <summary>An element's look by its place on the ring; null for none.</summary>
        public static SwingLook For(int element) => element >= 0 && element < All.Length ? All[element] : null;

        /// <summary>An element's look; null for none.</summary>
        public static SwingLook For(ElementInfo element) => element != null ? For(element.Index) : null;
    }
}
