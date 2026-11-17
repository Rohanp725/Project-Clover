using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The shapes the HUD is made of, drawn by code: slanted bars with an inked frame, a gourd for each flask, a
    /// diamond for embers, a framed panel, and a dry brush stroke to lay behind words. They are white or grey so the
    /// HUD can tint them, and they are saved under stable names so a rebuild repaints them in place and nothing that
    /// points at them breaks. The game's font is found here too.
    /// </summary>
    public static class UiArt
    {
        private const string Folder = BuildKit.GeneratedRoot + "/UI";
        private const string FontPath = "Assets/_Project/Art/Fonts/spectral_semibold.ttf";

        public static readonly Color Ink = new Color(0.045f, 0.035f, 0.08f, 0.94f);
        public static readonly Color Gold = new Color(0.93f, 0.78f, 0.47f, 1f);

        private static readonly HashSet<string> painted = new HashSet<string>();

        /// <summary>The font all the game's words are set in; Unity's own if the game's is missing.</summary>
        public static Font Font()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>The back of a bar: a slanted dark slab with a thin pale line round it. Sliced, so it can be any length.</summary>
        public static Sprite BarBack() => Make("ui_bar_back", 64, 32, new Vector4(16, 0, 16, 0), (x, y) =>
        {
            float d = Slab(x, y, 64, 32, 0.5f);
            float alpha = Mathf.Clamp01(d + 0.5f);
            // A pale line just inside the edge, then ink.
            float line = Mathf.Clamp01(1.5f - Mathf.Abs(d - 2.2f));
            Color colour = Color.Lerp(Ink, new Color(0.8f, 0.72f, 0.55f, 1f), line * 0.85f);
            colour.a *= alpha;
            return colour;
        });

        /// <summary>What fills a bar: the same slab, set in from the frame, lighter along its top and darker along its foot.</summary>
        public static Sprite BarFill() => Make("ui_bar_fill", 64, 32, new Vector4(16, 0, 16, 0), (x, y) =>
        {
            float d = Slab(x, y, 64, 32, 5f);
            float alpha = Mathf.Clamp01(d + 0.5f);
            float tone = y > 32 * 0.62f ? 1f : y > 32 * 0.3f ? 0.84f : 0.66f;
            return new Color(tone, tone, tone, alpha);
        });

        /// <summary>A flask's outline and shadowed inside: a gourd with a stopper.</summary>
        public static Sprite FlaskBack() => Make("ui_flask_back", 48, 64, Vector4.zero, (x, y) =>
        {
            float d = Gourd(x, y);
            float alpha = Mathf.Clamp01(d + 0.5f);
            float line = Mathf.Clamp01(1.5f - Mathf.Abs(d - 1.8f));
            Color colour = Color.Lerp(Ink, new Color(0.8f, 0.72f, 0.55f, 1f), line * 0.85f);
            colour.a *= alpha;
            return colour;
        });

        /// <summary>What is in the flask: the gourd's inside, lighter on one side.</summary>
        public static Sprite FlaskFill() => Make("ui_flask_fill", 48, 64, Vector4.zero, (x, y) =>
        {
            float d = Gourd(x, y) - 4.5f;
            float tone = x < 20 ? 1f : 0.82f;
            return new Color(tone, tone, tone, Mathf.Clamp01(d + 0.5f));
        });

        /// <summary>An ember: a small upright diamond with a bright heart.</summary>
        public static Sprite Ember() => Make("ui_ember", 32, 32, Vector4.zero, (x, y) =>
        {
            float d = 13f - (Mathf.Abs(x - 15.5f) * 1.25f + Mathf.Abs(y - 15.5f));
            float alpha = Mathf.Clamp01(d + 0.5f);
            Color colour = d < 2.5f ? Ink : d > 7f ? new Color(1f, 0.95f, 0.75f, 1f) : Color.white;
            colour.a = alpha;
            return colour;
        });

        /// <summary>The sun mark of the Fire land's HUD: a white disc with short rays round it, to be tinted and filled radially.</summary>
        public static Sprite Sun() => Make("ui_sun", 64, 64, Vector4.zero, (x, y) =>
        {
            float dx = x - 31.5f, dy = y - 31.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float angle = Mathf.Atan2(dy, dx);
            float disc = Mathf.Clamp01(17.5f - r);
            // Twelve short rays, ink-edged so the mark reads on a white sky.
            float ray = Mathf.Clamp01((Mathf.Cos(angle * 12f) - 0.55f) * 6f) * Mathf.Clamp01(29f - r) * Mathf.Clamp01(r - 20f);
            float body = Mathf.Max(disc, ray);
            float edge = Mathf.Clamp01(Mathf.Max(Mathf.Clamp01(19.5f - r), Mathf.Clamp01((Mathf.Cos(angle * 12f) - 0.4f) * 5f) * Mathf.Clamp01(30.5f - r) * Mathf.Clamp01(r - 18.5f)));
            Color colour = body > 0.5f ? Color.white : new Color(Ink.r, Ink.g, Ink.b, 1f);
            colour.a = Mathf.Max(body, edge);
            return colour;
        });

        /// <summary>A panel for menus and spoken words: ink with a pale line set in from its edge. Sliced.</summary>
        public static Sprite Panel() => Make("ui_panel", 64, 64, new Vector4(20, 20, 20, 20), (x, y) =>
        {
            // A rectangle with its corners cut off, not rounded: harder, and drawn rather than moulded.
            float dx = Mathf.Min(x, 63 - x), dy = Mathf.Min(y, 63 - y);
            float d = Mathf.Min(Mathf.Min(dx, dy), (dx + dy - 9f) * 0.7071f);
            float alpha = Mathf.Clamp01(d + 0.5f);
            float line = Mathf.Clamp01(1.2f - Mathf.Abs(d - 5f));
            Color colour = Color.Lerp(Ink, new Color(0.8f, 0.72f, 0.55f, 1f), line * 0.75f);
            colour.a *= alpha;
            return colour;
        });

        /// <summary>A mark on the chart of upgrades: an inked square with its corners cut and a white line to be tinted. Sliced.</summary>
        public static Sprite Mark() => Make("ui_mark", 64, 64, new Vector4(22, 22, 22, 22), (x, y) =>
        {
            float dx = Mathf.Min(x, 63 - x), dy = Mathf.Min(y, 63 - y);
            float d = Mathf.Min(Mathf.Min(dx, dy), (dx + dy - 11f) * 0.7071f);
            float alpha = Mathf.Clamp01(d + 0.5f);
            // Ink outside, then the line, then ink again: the line reads against a bright world and a dark one.
            float line = Mathf.Clamp01(2.2f - Mathf.Abs(d - 5.5f));
            Color colour = Color.Lerp(new Color(Ink.r, Ink.g, Ink.b, 1f), Color.white, line);
            colour.a = alpha;
            return colour;
        });

        /// <summary>The frame that sits round the chosen mark: only a line, with nothing inside it. Sliced.</summary>
        public static Sprite MarkCursor() => Make("ui_mark_cursor", 64, 64, new Vector4(22, 22, 22, 22), (x, y) =>
        {
            float dx = Mathf.Min(x, 63 - x), dy = Mathf.Min(y, 63 - y);
            float d = Mathf.Min(Mathf.Min(dx, dy), (dx + dy - 11f) * 0.7071f);
            float line = Mathf.Clamp01(2.4f - Mathf.Abs(d - 3f));
            return new Color(1f, 1f, 1f, line);
        });

        /// <summary>An element's emblem at the foot of its tree: a diamond with a ring of ink and a bright heart, to be tinted.</summary>
        public static Sprite Emblem() => Make("ui_emblem", 64, 64, Vector4.zero, (x, y) =>
        {
            float d = 30f - (Mathf.Abs(x - 31.5f) + Mathf.Abs(y - 31.5f));
            float alpha = Mathf.Clamp01(d * 0.7071f + 0.5f);
            float depth = d * 0.7071f;
            Color colour = depth < 3f ? new Color(Ink.r, Ink.g, Ink.b, 1f) : depth < 7f ? Color.white : depth < 10f ? new Color(Ink.r, Ink.g, Ink.b, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            colour.a = alpha;
            return colour;
        });

        /// <summary>A stroke of a dry brush, to lay behind a word or two. Sliced along its length.</summary>
        public static Sprite Brush() => Make("ui_brush", 256, 64, new Vector4(80, 0, 80, 0), (x, y) =>
        {
            float along = x / 255f;
            // Full in the middle, thinning toward both ends, with a ragged edge.
            float taper = Mathf.Clamp01(Mathf.Min(along, 1f - along) * 5f);
            float half = 22f * (0.55f + 0.45f * taper) + (Noise(x * 0.09f, 3f) - 0.5f) * 9f;
            float centre = 32f + (Noise(x * 0.03f, 9f) - 0.5f) * 6f;
            float d = half - Mathf.Abs(y - centre);
            float alpha = Mathf.Clamp01(d * 0.6f + 0.5f);
            // The bristles part near the ends, leaving streaks of nothing.
            float dry = 1f - taper;
            if (dry > 0f && Noise(y * 0.9f, 20f + Mathf.Floor(along * 2f)) < dry * 0.75f) alpha *= 1f - dry;
            return new Color(1f, 1f, 1f, alpha);
        });

        /// <summary>
        /// The edge of the screen closing in: nothing in the middle, thickening to solid at the edges and most in the
        /// corners, with a ragged inner line as if brushed on. White, so the HUD can tint it; stretched over the whole screen.
        /// </summary>
        public static Sprite Vignette() => Make("ui_vignette", 256, 144, Vector4.zero, (x, y) =>
        {
            float across = Mathf.Abs(x / 255f * 2f - 1f), up = Mathf.Abs(y / 143f * 2f - 1f);
            // Squarer than a circle, so it hugs the frame of the screen and leaves the whole middle alone.
            float outward = Mathf.Pow(Mathf.Pow(across, 3.2f) + Mathf.Pow(up, 3.2f), 1f / 3.2f);
            float ragged = (Noise(Mathf.Atan2(up, across) * 9f + x * 0.05f, 4f) - 0.5f) * 0.07f;
            float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.74f, 1.14f, outward + ragged));
            return new Color(1f, 1f, 1f, alpha * 0.92f);
        });

        /// <summary>
        /// The impact frame of a comic panel: hard spikes of light driven in from the edges of the screen toward the
        /// middle, which is left almost clear so the blow and the fighters are still seen. White, so a flash can tint it.
        /// </summary>
        public static Sprite ImpactFrame() => Make("ui_impact_frame", 1024, 576, Vector4.zero, (x, y) =>
        {
            float across = x / 1023f * 2f - 1f, up = y / 575f * 2f - 1f;
            float outward = Mathf.Sqrt(across * across + up * up) / 1.2f;
            // Measured along the screen's own shape, so the spikes are as thick at the sides as at the top.
            float angle = Mathf.Atan2(up * 0.5625f, across);
            // Each spike reaches in a different way; two grains of noise make some broad and some thin.
            // They start well out: at the middle of the top and bottom edges only the longest reach into the picture.
            float reach = 0.64f + Noise(angle * 22f, 2f) * 0.34f + Noise(angle * 61f, 5f) * 0.14f;
            float spike = Mathf.Clamp01((outward - reach) * 22f);
            // Between the spikes only a breath of the colour, a little more toward the edge.
            float breath = 0.03f + 0.15f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.05f, outward));
            return new Color(1f, 1f, 1f, Mathf.Max(spike, breath));
        });

        // ---- The chart of upgrades: its night sky, the frames of its marks, and the small pictures the marks carry.

        /// <summary>The frames a mark can sit in, one for each <see cref="Clover.Player.UpgradeKind"/> in its order, and after them a plain ring for the spell that is worn.</summary>
        public const int FrameCount = 8;

        /// <summary>
        /// The night sky behind the chart: deep blue-black, a faint band of cloud across it, and stars of several
        /// sizes, a few with a cross of light. It hides everything behind it.
        /// </summary>
        public static Sprite Starfield() => Make("ui_starfield_2", 1024, 576, Vector4.zero, (x, y) =>
        {
            float across = x / 1023f, up = y / 575f;
            // A band of thin cloud from lower left to upper right, in two tones.
            float band = Mathf.Exp(-Mathf.Pow((up - 0.25f - across * 0.5f) * 3.2f, 2f));
            float cloud = Noise(x * 0.011f + Noise(y * 0.013f, 7f) * 2.5f, 3f) * Noise(y * 0.017f + Noise(x * 0.009f, 11f) * 2f, 5f);
            Color sky = Color.Lerp(new Color(0.012f, 0.012f, 0.04f, 1f), new Color(0.03f, 0.025f, 0.075f, 1f), up * 0.7f);
            sky += new Color(0.04f, 0.028f, 0.09f, 0f) * (band * cloud * 1.3f) + new Color(0.0f, 0.02f, 0.035f, 0f) * (band * (1f - cloud) * 0.5f);
            // One star to a square of sky, most of them faint, set anywhere in its square.
            float light = 0f;
            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    int cx = x / 22 + ox, cy = y / 22 + oy;
                    float seed = cx * 73.13f + cy * 191.7f;
                    float bright = Mathf.Pow(Hash(seed + 3f), 7f);
                    if (bright < 0.004f) continue;
                    float sx = (cx + 0.1f + Hash(seed) * 0.8f) * 22f, sy = (cy + 0.1f + Hash(seed + 1f) * 0.8f) * 22f;
                    float dx = Mathf.Abs(x - sx), dy = Mathf.Abs(y - sy);
                    float size = 0.35f + bright * 1.3f;
                    light = Mathf.Max(light, Mathf.Clamp01(size - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f) * (0.16f + bright * 0.8f));
                    // The brightest wear a thin cross.
                    if (bright > 0.6f)
                        light = Mathf.Max(light, Mathf.Clamp01(0.8f - Mathf.Min(dx, dy)) * Mathf.Clamp01(1f - Mathf.Max(dx, dy) / (3f + bright * 5f)) * bright * 0.6f);
                }
            }
            Color star = Color.Lerp(new Color(0.75f, 0.85f, 1f, 1f), new Color(1f, 0.92f, 0.78f, 1f), Hash(x / 22 * 5.7f + y / 22 * 13.3f));
            Color colour = sky + star * light;
            colour.a = 1f;
            return colour;
        });

        /// <summary>
        /// One sheet holding the frames of the marks and every picture named in <see cref="Clover.Player.UpgradeIcons"/>,
        /// so the whole chart is drawn from one texture. The frames come first, then the pictures in their listed order.
        /// </summary>
        public static Sprite[] ChartSheet()
        {
            int pictures = Clover.Player.UpgradeIcons.All.Count;
            var names = new string[FrameCount + pictures];
            for (int i = 0; i < FrameCount; i++) names[i] = "frame_" + i;
            for (int i = 0; i < pictures; i++) names[FrameCount + i] = "icon_" + Clover.Player.UpgradeIcons.All[i];
            return MakeSheet("ui_chart_sheet", 64, 8, names, (index, x, y) =>
            {
                // Measured from the middle of the cell, y upward.
                float px = x - 31.5f, py = y - 31.5f;
                if (index < FrameCount)
                {
                    float d = FrameShape(index, px, py);
                    float alpha = Mathf.Clamp01(d + 0.5f);
                    if (index == FrameCount - 1) return new Color(1f, 1f, 1f, Mathf.Clamp01(2.2f - Mathf.Abs(d - 2.6f)));
                    // Ink outside, then the line, then ink again; the greater marks have a second, finer line inside.
                    float line = Mathf.Clamp01(2.3f - Mathf.Abs(d - 4.6f));
                    if (index == (int)Clover.Player.UpgradeKind.Keystone || index == (int)Clover.Player.UpgradeKind.Signature || index == (int)Clover.Player.UpgradeKind.Root)
                        line = Mathf.Max(line, Mathf.Clamp01(1.3f - Mathf.Abs(d - 9.6f)) * 0.85f);
                    Color colour = Color.Lerp(new Color(Ink.r, Ink.g, Ink.b, 1f), Color.white, line);
                    colour.a = alpha;
                    return colour;
                }
                float inside = Picture(Clover.Player.UpgradeIcons.All[index - FrameCount], px, py);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(inside + 0.5f));
            });
        }

        // How far inside each frame's outline a point lies, in pixels.
        private static float FrameShape(int index, float x, float y)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            switch (index)
            {
                case (int)Clover.Player.UpgradeKind.Minor: return 28f - Mathf.Sqrt(x * x + y * y);
                case (int)Clover.Player.UpgradeKind.Root: return 30.5f - Mathf.Sqrt(x * x + y * y);
                // A square with its corners cut.
                case (int)Clover.Player.UpgradeKind.Major: return Mathf.Min(Mathf.Min(29f - ax, 29f - ay), (48f - ax - ay) * 0.7071f);
                case (int)Clover.Player.UpgradeKind.Spell: return (31f - ax - ay) * 0.7071f;
                // Eight even sides.
                case (int)Clover.Player.UpgradeKind.Keystone: return Mathf.Min(Mathf.Min(30f - ax, 30f - ay), (42.4f - ax - ay) * 0.7071f);
                // Six sides, points to left and right.
                case (int)Clover.Player.UpgradeKind.Mix: return Mathf.Min(27f - ay, (31f - ax - ay * 0.5774f) * 0.866f);
                // A sun: a disc with twelve short points.
                case (int)Clover.Player.UpgradeKind.Signature:
                {
                    float r = Mathf.Sqrt(x * x + y * y);
                    float ray = Mathf.Abs(Mathf.Repeat(Mathf.Atan2(y, x) / (Mathf.PI * 2f) * 12f, 1f) - 0.5f) * 2f;
                    return 26.5f + 4.5f * (1f - ray) - r;
                }
                default: return 29f - Mathf.Sqrt(x * x + y * y);
            }
        }

        private static float Circle(float x, float y, float cx, float cy, float r) => r - Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
        private static float Band(float x, float y, float cx, float cy, float r, float width) => width - Mathf.Abs(Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r);
        private static float Block(float x, float y, float cx, float cy, float hx, float hy) => Mathf.Min(hx - Mathf.Abs(x - cx), hy - Mathf.Abs(y - cy));

        // A stroke from one point to another, half as wide as given.
        private static float Stroke(float x, float y, float ax, float ay, float bx, float by, float half)
        {
            float vx = bx - ax, vy = by - ay, wx = x - ax, wy = y - ay;
            float t = Mathf.Clamp01((wx * vx + wy * vy) / Mathf.Max(0.0001f, vx * vx + vy * vy));
            float dx = wx - vx * t, dy = wy - vy * t;
            return half - Mathf.Sqrt(dx * dx + dy * dy);
        }

        // A shape with straight sides and no dents, its corners given going round either way.
        private static float Poly(float x, float y, params float[] corners)
        {
            float inside = float.MaxValue, area = 0f;
            int count = corners.Length / 2;
            for (int i = 0; i < count; i++)
            {
                float ax = corners[i * 2], ay = corners[i * 2 + 1];
                float bx = corners[(i + 1) % count * 2], by = corners[(i + 1) % count * 2 + 1];
                area += ax * by - bx * ay;
            }
            float way = area >= 0f ? 1f : -1f;
            for (int i = 0; i < count; i++)
            {
                float ax = corners[i * 2], ay = corners[i * 2 + 1];
                float bx = corners[(i + 1) % count * 2], by = corners[(i + 1) % count * 2 + 1];
                float ex = bx - ax, ey = by - ay;
                float length = Mathf.Sqrt(ex * ex + ey * ey);
                inside = Mathf.Min(inside, way * (ex * (y - ay) - ey * (x - ax)) / Mathf.Max(0.0001f, length));
            }
            return inside;
        }

        // A star with so many points.
        private static float StarShape(float x, float y, int points, float outer, float inner)
        {
            float best = -100f;
            for (int i = 0; i < points; i++)
            {
                float at = Mathf.PI * 0.5f + i * Mathf.PI * 2f / points, half = Mathf.PI / points;
                best = Mathf.Max(best, Poly(x, y, 0f, 0f,
                    Mathf.Cos(at - half) * inner, Mathf.Sin(at - half) * inner,
                    Mathf.Cos(at) * outer, Mathf.Sin(at) * outer,
                    Mathf.Cos(at + half) * inner, Mathf.Sin(at + half) * inner));
            }
            return best;
        }

        private static float Either(float a, float b) => Mathf.Max(a, b);
        private static float Either(float a, float b, float c) => Mathf.Max(a, Mathf.Max(b, c));
        private static float Either(float a, float b, float c, float d) => Mathf.Max(Mathf.Max(a, b), Mathf.Max(c, d));
        private static float Without(float a, float b) => Mathf.Min(a, -b);

        // Each picture as a shape: how far inside it a point lies, in pixels, measured from the middle of a 64 pixel square with y upward.
        private static float Picture(string name, float x, float y)
        {
            const float R = 0.7071f;
            // The same point turned an eighth of a turn, for the pictures that lie on a slant.
            float tx = (x + y) * R, ty = (y - x) * R;
            switch (name)
            {
                case "flame":
                    return Without(Either(Circle(x, y, 0f, -8f, 13f), Poly(x, y, -12.5f, -4f, 12.5f, -4f, 4f, 25f)), Either(Circle(x, y, 1f, -11f, 5.5f), Poly(x, y, -4f, -9f, 6f, -9f, 2f, 3f)));
                case "bolt":
                    return Either(Poly(x, y, 8f, 25f, -13f, -4f, 3f, -4f), Poly(x, y, -8f, -25f, 13f, 4f, -3f, 4f));
                case "gust":
                    return Either(
                        Either(Stroke(x, y, -23f, 11f, 9f, 11f, 2.6f), Band(x, y, 9f, 16.5f, 5.5f, 2.6f) + Mathf.Min(0f, x - 8f) * 4f),
                        Stroke(x, y, -15f, 0f, 22f, 0f, 2.6f),
                        Either(Stroke(x, y, -23f, -11f, 3f, -11f, 2.6f), Band(x, y, 3f, -16.5f, 5.5f, 2.6f) + Mathf.Min(0f, x - 2f) * 4f));
                case "snowflake":
                {
                    float best = -100f;
                    for (int i = 0; i < 6; i++)
                    {
                        float at = i * Mathf.PI / 3f, cx = Mathf.Cos(at), cy = Mathf.Sin(at);
                        best = Mathf.Max(best, Stroke(x, y, 0f, 0f, cx * 23f, cy * 23f, 2.2f));
                        // Two barbs on each arm.
                        float bx = cx * 13f, by = cy * 13f;
                        best = Mathf.Max(best, Stroke(x, y, bx, by, bx + Mathf.Cos(at + 0.9f) * 8f, by + Mathf.Sin(at + 0.9f) * 8f, 1.9f));
                        best = Mathf.Max(best, Stroke(x, y, bx, by, bx + Mathf.Cos(at - 0.9f) * 8f, by + Mathf.Sin(at - 0.9f) * 8f, 1.9f));
                    }
                    return best;
                }
                case "drop":
                    return Without(Either(Circle(x, y, 0f, -8f, 14f), Poly(x, y, -12.2f, -1.5f, 12.2f, -1.5f, 0f, 25f)), Band(x, y, 0f, -8f, 8.5f, 1.6f) + Mathf.Min(0f, -x - 2f) * 4f + Mathf.Min(0f, -y - 6f) * 4f);
                case "leaf":
                    return Either(Without(Mathf.Min(Circle(tx, ty, 0f, -13f, 24f), Circle(tx, ty, 0f, 13f, 24f)), Stroke(tx, ty, -14f, 0f, 20f, 0f, 1.5f)), Stroke(tx, ty, -27f, 0f, -17f, 0f, 2.2f));
                case "rock":
                    return Without(Poly(x, y, -21f, -15f, 19f, -15f, 24f, 1f, 9f, 19f, -11f, 17f, -24f, -1f), Either(Stroke(x, y, -3f, 17f, 3f, 3f, 1.5f), Stroke(x, y, 3f, 3f, -4f, -6f, 1.5f)));
                case "anvil":
                    return Either(Either(Block(x, y, 3f, 10f, 18f, 6.5f), Poly(x, y, -15f, 16.5f, -27f, 13f, -15f, 5f)), Block(x, y, 3f, -2f, 7.5f, 7f), Poly(x, y, -13f, -20f, 19f, -20f, 14f, -8f, -8f, -8f));
                case "sun":
                {
                    float best = Circle(x, y, 0f, 0f, 9.5f);
                    for (int i = 0; i < 8; i++)
                    {
                        float at = i * Mathf.PI / 4f, cx = Mathf.Cos(at), cy = Mathf.Sin(at);
                        best = Mathf.Max(best, Stroke(x, y, cx * 14.5f, cy * 14.5f, cx * 23f, cy * 23f, 2.5f));
                    }
                    return best;
                }
                case "moon":
                    return Without(Circle(x, y, -2f, 0f, 21f), Circle(x, y, 7f, 5f, 18f));
                case "fang":
                    return Either(Poly(x, y, -15f, 14f, -8.5f, -22f, -2f, 14f), Poly(x, y, 2f, 14f, 8.5f, -22f, 15f, 14f), Block(x, y, 0f, 18f, 19f, 4f));
                case "blade":
                    return Either(Poly(tx, ty, -8f, -3.2f, 19f, -3.2f, 27f, 0f, 19f, 3.2f, -8f, 3.2f), Block(tx, ty, -10f, 0f, 2.4f, 10f), Block(tx, ty, -18f, 0f, 6f, 2.4f), Circle(tx, ty, -25f, 0f, 3.6f));
                case "fist":
                    return Without(Either(Block(x, y, 1f, 4f, 16f, 12f) + Mathf.Min(0f, 24f - Mathf.Abs(x - 1f) - Mathf.Abs(y - 4f)), Block(x, y, -3f, -12f, 11f, 5f), Block(x, y, 14f, -8f, 4f, 9f)),
                        Either(Stroke(x, y, -7f, 6f, -7f, 17f, 1.2f), Stroke(x, y, 1f, 6f, 1f, 17f, 1.2f), Stroke(x, y, 9f, 6f, 9f, 17f, 1.2f)));
                case "boot":
                    return Without(Either(Block(x, y, -6f, 6f, 8.5f, 17f), Block(x, y, 2f, -14f, 16f, 7f), Circle(x, y, 15f, -12f, 9f)), Stroke(x, y, -14f, -19f, 24f, -19f, 1.4f));
                case "shield":
                    return Without(Either(Block(x, y, 0f, 10f, 18f, 12f), Poly(x, y, -18f, -1.5f, 0f, -25f, 18f, -1.5f)), Either(Stroke(x, y, 0f, 15f, 0f, -14f, 1.7f), Stroke(x, y, -11f, 5f, 11f, 5f, 1.7f)));
                case "flask":
                    return Without(Either(Circle(x, y, 0f, -8f, 15f), Block(x, y, 0f, 11f, 5.5f, 8f), Block(x, y, 0f, 20f, 9f, 2.6f)), Stroke(x, y, -10f, -4f, 10f, -4f, 1.5f));
                case "eye":
                    return Either(Without(Mathf.Min(Circle(x, y, 0f, -16f, 29f), Circle(x, y, 0f, 16f, 29f)), Circle(x, y, 0f, 0f, 9f)), Circle(x, y, 0f, 0f, 4.8f));
                case "heart":
                    return Either(Circle(x, y, -9f, 8f, 11f), Circle(x, y, 9f, 8f, 11f), Poly(x, y, -18.6f, 2.5f, 0f, -23f, 18.6f, 2.5f));
                case "star":
                    return StarShape(x, y, 5, 25f, 10.5f);
                case "skull":
                    return Without(Either(Circle(x, y, 0f, 6f, 17f), Block(x, y, 0f, -13f, 9.5f, 8f)),
                        Either(Either(Circle(x, y, -7f, 5f, 4.8f), Circle(x, y, 7f, 5f, 4.8f), Poly(x, y, -2.5f, -7f, 2.5f, -7f, 0f, -1f)),
                            Stroke(x, y, -4.5f, -21f, -4.5f, -13f, 1.1f), Stroke(x, y, 0f, -21f, 0f, -13f, 1.1f), Stroke(x, y, 4.5f, -21f, 4.5f, -13f, 1.1f)));
                case "hourglass":
                    return Either(Poly(x, y, -14f, 20f, 0f, -1f, 14f, 20f), Poly(x, y, -14f, -20f, 14f, -20f, 0f, 1f), Block(x, y, 0f, 22f, 17f, 2.6f), Block(x, y, 0f, -22f, 17f, 2.6f));
                case "ring":
                    return Either(Band(x, y, 0f, -4f, 16f, 3.2f), Poly(x, y, -7f, 16f, 0f, 9f, 7f, 16f, 0f, 25f));
                case "arrow":
                    return Either(Stroke(tx, ty, -24f, 0f, 14f, 0f, 2.3f), Poly(tx, ty, 10f, -9f, 28f, 0f, 10f, 9f), Poly(tx, ty, -28f, -7f, -18f, 0f, -28f, 7f, -22f, 0f));
                case "wave":
                {
                    float crest = Mathf.Sin((x + 6f) * Mathf.PI / 15f) * 5.5f;
                    return Mathf.Min(Either(3f - Mathf.Abs(y - 11f - crest), 3f - Mathf.Abs(y - crest), 3f - Mathf.Abs(y + 11f - crest)), 25f - Mathf.Abs(x));
                }
                case "orb":
                    return Either(Without(Circle(x, y, 0f, 0f, 15f), Circle(x, y, -5.5f, 5.5f, 4.5f)), Band(x, y, 0f, 0f, 22.5f, 1.7f));
                case "beam":
                    return Either(Stroke(x, y, -25f, 0f, 12f, 0f, 4.6f), StarShape(x - 15f, y, 8, 12f, 5.5f), Stroke(x, y, -25f, 11f, 2f, 11f, 1.5f), Stroke(x, y, -25f, -11f, 2f, -11f, 1.5f));
                case "spike":
                    return Either(Poly(x, y, -24f, -19f, -8f, -19f, -16f, 9f), Poly(x, y, -9.5f, -19f, 9.5f, -19f, 0f, 25f), Poly(x, y, 8f, -19f, 24f, -19f, 16f, 9f));
                case "wall":
                {
                    float mortar = Either(Stroke(x, y, -24f, 6f, 24f, 6f, 1.2f), Stroke(x, y, -24f, -6f, 24f, -6f, 1.2f));
                    mortar = Either(mortar, Stroke(x, y, -6f, 6f, -6f, 18f, 1.2f), Stroke(x, y, 10f, 6f, 10f, 18f, 1.2f));
                    mortar = Either(mortar, Stroke(x, y, -12f, -6f, -12f, 6f, 1.2f), Stroke(x, y, 4f, -6f, 4f, 6f, 1.2f));
                    mortar = Either(mortar, Stroke(x, y, -6f, -18f, -6f, -6f, 1.2f), Stroke(x, y, 10f, -18f, 10f, -6f, 1.2f));
                    return Without(Block(x, y, 0f, 0f, 23f, 18f), mortar);
                }
                case "crown":
                    return Either(Block(x, y, 0f, -13f, 20f, 5f), Poly(x, y, -20f, -9f, -7f, -9f, -19f, 15f), Poly(x, y, -9f, -9f, 9f, -9f, 0f, 20f), Poly(x, y, 7f, -9f, 20f, -9f, 19f, 15f));
                case "burst":
                    return Without(StarShape(x, y, 8, 26f, 11.5f), Circle(x, y, 0f, 0f, 4.5f));
                case "comet":
                    return Either(Circle(x, y, -10f, -10f, 10.5f), Poly(x, y, -4f, -18.5f, 25f, 25f, -18.5f, -4f));
                case "chain":
                    return Either(Band(x, y, -8.5f, 7f, 10.5f, 3.2f), Band(x, y, 8.5f, -7f, 10.5f, 3.2f));
                case "spiral":
                {
                    float r = Mathf.Sqrt(x * x + y * y);
                    float turn = Mathf.Atan2(y, x) / (Mathf.PI * 2f);
                    float t = r / 9.5f - turn;
                    return Mathf.Min(2.4f - Mathf.Abs(t - Mathf.Round(t)) * 9.5f, Mathf.Min(24f - r, r - 2f));
                }
                case "plus":
                    return Either(Block(x, y, 0f, 0f, 6.5f, 22f), Block(x, y, 0f, 0f, 22f, 6.5f));
                case "target":
                    return Either(Band(x, y, 0f, 0f, 20f, 2.6f), Band(x, y, 0f, 0f, 10.5f, 2.3f), Circle(x, y, 0f, 0f, 3.6f),
                        Either(Stroke(x, y, 0f, 17f, 0f, 27f, 1.8f), Stroke(x, y, 0f, -17f, 0f, -27f, 1.8f), Stroke(x, y, 17f, 0f, 27f, 0f, 1.8f), Stroke(x, y, -17f, 0f, -27f, 0f, 1.8f)));
                case "wing":
                    return Either(Poly(x, y, -22f, -12f, 1f, -5f, 24f, 22f), Poly(x, y, -22f, -12f, 4f, -13f, 22f, 4f), Poly(x, y, -22f, -12f, 2f, -22f, 16f, -11f));
                case "tower":
                    return Either(Either(Block(x, y, 0f, -4f, 3f, 17f), Circle(x, y, 0f, 17f, 6.5f), Block(x, y, 0f, -22f, 11f, 2.6f)),
                        Stroke(x, y, -18f, 16f, -10f, 9f, 1.8f), Stroke(x, y, 18f, 16f, 10f, 9f, 1.8f), Either(Stroke(x, y, -20f, 2f, -11f, 2f, 1.8f), Stroke(x, y, 20f, 2f, 11f, 2f, 1.8f)));
                case "gem":
                    return Without(Poly(x, y, 0f, 25f, -17f, 7f, 0f, -25f, 17f, 7f), Either(Stroke(x, y, -17f, 7f, 17f, 7f, 1.3f), Stroke(x, y, -7f, 7f, 0f, 25f, 1.1f), Stroke(x, y, 7f, 7f, 0f, 25f, 1.1f), Either(Stroke(x, y, -7f, 7f, 0f, -25f, 1.1f), Stroke(x, y, 7f, 7f, 0f, -25f, 1.1f))));
                case "claw":
                    return Either(Poly(x, y, -22f, 21f, -7f, -23f, -13f, 23f), Poly(x, y, -7f, 21f, 8f, -23f, 2f, 23f), Poly(x, y, 8f, 21f, 23f, -23f, 17f, 23f));
                case "hammer":
                    return Either(Block(tx, ty, 9f, 0f, 8f, 17f), Block(tx, ty, -12f, 0f, 16f, 3f));
                default:
                    return Circle(x, y, 0f, 0f, 12f);
            }
        }

        // One texture cut into square cells, a sprite to each, painted cell by cell. Saved under one stable name.
        private static Sprite[] MakeSheet(string name, int cell, int columns, string[] names, System.Func<int, int, int, Color> paint)
        {
            BuildKit.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            int rows = (names.Length + columns - 1) / columns;
            int width = cell * columns, height = cell * rows;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprites = new Sprite[names.Length];
            bool whole = texture != null && texture.width == width && texture.height == height;
            if (whole)
            {
                foreach (var part in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (!(part is Sprite found)) continue;
                    int at = System.Array.IndexOf(names, found.name);
                    if (at >= 0) sprites[at] = found;
                }
                foreach (var sprite in sprites) whole &= sprite != null;
            }
            if (!whole)
            {
                if (texture != null) AssetDatabase.DeleteAsset(path);
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                AssetDatabase.CreateAsset(texture, path);
                for (int i = 0; i < names.Length; i++)
                {
                    // Cells are counted from the top left, as they would be read.
                    var rect = new Rect(i % columns * cell, (rows - 1 - i / columns) * cell, cell, cell);
                    sprites[i] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                    sprites[i].name = names[i];
                    AssetDatabase.AddObjectToAsset(sprites[i], texture);
                }
                painted.Remove(name);
            }
            if (painted.Contains(name)) return sprites;
            painted.Add(name);
            var pixels = new Color[width * height];
            for (int i = 0; i < names.Length; i++)
            {
                int left = i % columns * cell, bottom = (rows - 1 - i / columns) * cell;
                for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                        pixels[(bottom + y) * width + left + x] = paint(i, x, y);
            }
            texture.SetPixels(pixels);
            texture.Apply(false);
            EditorUtility.SetDirty(texture);
            foreach (var sprite in sprites) EditorUtility.SetDirty(sprite);
            AssetDatabase.SaveAssetIfDirty(texture);
            return sprites;
        }

        // Distance inside a slab with slanted ends, in pixels: positive inside, measured to the nearest edge.
        private static float Slab(int x, int y, int width, int height, float inset)
        {
            const float Slant = 0.42f;
            float shifted = x - (y - height * 0.5f) * Slant;
            float reach = height * 0.5f * Slant;
            float dx = Mathf.Min(shifted - reach - inset, width - 1 - reach - inset - shifted) * 0.92f;
            float dy = Mathf.Min(y - inset, height - 1 - inset - y);
            return Mathf.Min(dx, dy);
        }

        // Distance inside a gourd: a round belly, a smaller shoulder, a neck and a stopper.
        private static float Gourd(int x, int y)
        {
            float belly = 17f - Mathf.Sqrt((x - 23.5f) * (x - 23.5f) + (y - 20f) * (y - 20f));
            float shoulder = 11f - Mathf.Sqrt((x - 23.5f) * (x - 23.5f) + (y - 40f) * (y - 40f));
            float neck = Mathf.Min(Mathf.Min(x - 19f, 28f - x), Mathf.Min(y - 44f, 55f - y));
            float stopper = Mathf.Min(Mathf.Min(x - 16.5f, 30.5f - x), Mathf.Min(y - 54f, 61f - y));
            return Mathf.Max(Mathf.Max(belly, shoulder), Mathf.Max(neck, stopper));
        }

        private static float Noise(float at, float seed)
        {
            float whole = Mathf.Floor(at), part = at - whole;
            part = part * part * (3f - 2f * part);
            return Mathf.Lerp(Hash(whole + seed * 57f), Hash(whole + 1f + seed * 57f), part);
        }

        private static float Hash(float n) => Mathf.Repeat(Mathf.Sin(n * 127.1f) * 43758.5453f, 1f);

        private static Sprite Make(string name, int width, int height, Vector4 border, System.Func<int, int, Color> paint)
        {
            BuildKit.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Sprite sprite = null;
            if (texture != null)
            {
                foreach (var part in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (part is Sprite found) sprite = found;
            }
            if (texture == null || sprite == null || texture.width != width || texture.height != height)
            {
                if (texture != null) AssetDatabase.DeleteAsset(path);
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = name, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                AssetDatabase.CreateAsset(texture, path);
                sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                sprite.name = name;
                AssetDatabase.AddObjectToAsset(sprite, texture);
                painted.Remove(name);
            }
            if (painted.Contains(name)) return sprite;
            painted.Add(name);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = paint(x, y);
            texture.SetPixels(pixels);
            texture.Apply(false);
            EditorUtility.SetDirty(texture);
            EditorUtility.SetDirty(sprite);
            AssetDatabase.SaveAssetIfDirty(texture);
            return sprite;
        }
    }
}
