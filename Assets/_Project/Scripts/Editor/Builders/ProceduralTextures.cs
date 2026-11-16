using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Surface detail drawn by code: stone flags, wall blocks, planks. They are near-white patterns that the toon
    /// material's own colour is multiplied by, so one texture serves many colours. They tile, and they are saved as
    /// assets under stable names so a rebuild repaints them in place.
    /// </summary>
    public static class ProceduralTextures
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Textures";
        private const int Size = 512;

        /// <summary>Square flagstones, four across, each a slightly different tone, with dark joints and a few cracks.</summary>
        public static Texture2D Flagstones() => Make("stone_flags", (x, y) =>
        {
            const int Cells = 4;
            float cell = Size / (float)Cells;
            int cx = (int)(x / cell), cy = (int)(y / cell);
            // Alternate rows are shifted by half a stone, as laid paving is.
            float shifted = x + (cy % 2 == 0 ? 0f : cell * 0.5f);
            cx = (int)(Mathf.Repeat(shifted, Size) / cell);
            float fx = Mathf.Repeat(shifted, cell) / cell, fy = Mathf.Repeat(y, cell) / cell;
            float edge = Mathf.Min(Mathf.Min(fx, 1f - fx), Mathf.Min(fy, 1f - fy));
            // Flat tones, one per stone, with a broad brushed drift and no grain: the shapes do the work.
            float tone = 0.8f + 0.2f * Mathf.Round(Hash(cx, cy) * 3f) / 3f;
            tone *= 0.96f + 0.04f * Noise(x, y, 128f);
            float joint = Mathf.SmoothStep(0.38f, 1f, Mathf.Clamp01(edge / 0.035f));
            return Mathf.Lerp(0.38f, tone, joint);
        });

        /// <summary>Coursed wall blocks: long stones in rows, each row offset from the one below.</summary>
        public static Texture2D WallBlocks() => Make("stone_wall", (x, y) =>
        {
            const int Rows = 6, Columns = 3;
            float rowHeight = Size / (float)Rows, width = Size / (float)Columns;
            int row = (int)(y / rowHeight);
            float shifted = Mathf.Repeat(x + (row % 2) * width * 0.5f + Hash(row, 11) * width * 0.2f, Size);
            int column = (int)(shifted / width);
            float fx = Mathf.Repeat(shifted, width) / width, fy = Mathf.Repeat(y, rowHeight) / rowHeight;
            float edge = Mathf.Min(Mathf.Min(fx, 1f - fx) * (width / rowHeight), Mathf.Min(fy, 1f - fy));
            float tone = 0.78f + 0.22f * Mathf.Round(Hash(column, row) * 3f) / 3f;
            tone *= 0.96f + 0.04f * Noise(x, y, 128f);
            float joint = Mathf.SmoothStep(0.34f, 1f, Mathf.Clamp01(edge / 0.07f));
            return Mathf.Lerp(0.34f, tone, joint);
        });

        /// <summary>Planks running along the texture's height, with grain and a gap between boards.</summary>
        public static Texture2D Planks() => Make("wood_planks", (x, y) =>
        {
            const int Boards = 5;
            float width = Size / (float)Boards;
            int board = (int)(x / width);
            float fx = Mathf.Repeat(x, width) / width;
            float tone = 0.76f + 0.22f * Hash(board, 5);
            tone *= 0.95f + 0.05f * Noise(x, y, 128f);
            float gap = Mathf.SmoothStep(0.3f, 1f, Mathf.Clamp01(Mathf.Min(fx, 1f - fx) / 0.05f));
            return Mathf.Lerp(0.3f, tone, gap);
        });

        /// <summary>Packed earth and gravel: no pattern, only tone drifting at three scales.</summary>
        public static Texture2D Ground() => Make("ground_earth", (x, y) =>
            0.9f + 0.1f * Noise(x, y, 256f));

        private static Texture2D Make(string name, System.Func<float, float, float> tone)
        {
            BuildKit.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width != Size)
            {
                if (texture != null) AssetDatabase.DeleteAsset(path);
                texture = new Texture2D(Size, Size, TextureFormat.RGBA32, true) { name = name };
                AssetDatabase.CreateAsset(texture, path);
            }
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    byte value = (byte)Mathf.Clamp(Mathf.RoundToInt(tone(x, y) * 255f), 0, 255);
                    pixels[y * Size + x] = new Color32(value, value, value, 255);
                }
            }
            texture.SetPixels32(pixels);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 4;
            texture.Apply(true);
            EditorUtility.SetDirty(texture);
            return texture;
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return ((h ^ (h >> 16)) & 0xFFFFu) / 65535f;
            }
        }

        // Smooth noise that repeats exactly across the texture, so the tiles have no seam.
        private static float Noise(float x, float y, float period)
        {
            int cells = Mathf.Max(1, Mathf.RoundToInt(Size / period));
            float gx = Mathf.Repeat(x, Size) / Size * cells, gy = Mathf.Repeat(y, Size) / Size * cells;
            int x0 = Mathf.FloorToInt(gx), y0 = Mathf.FloorToInt(gy);
            float fx = gx - x0, fy = gy - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            int x1 = (x0 + 1) % cells, y1 = (y0 + 1) % cells;
            x0 %= cells;
            y0 %= cells;
            int salt = cells * 31;
            return Mathf.Lerp(Mathf.Lerp(Hash(x0 + salt, y0), Hash(x1 + salt, y0), fx),
                              Mathf.Lerp(Hash(x0 + salt, y1), Hash(x1 + salt, y1), fx), fy);
        }
    }
}
