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
