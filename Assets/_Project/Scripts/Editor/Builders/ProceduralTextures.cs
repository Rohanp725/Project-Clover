using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Surface detail painted by code, in the manner of a hand-painted game floor: flat stones of differing tone and
    /// tint, each with a lit edge and a shaded edge, chipped corners, a crack here and there, moss in the joints.
    /// They are near-white patterns that the toon material's own colour is multiplied by, so one texture serves many
    /// colours; the tints in them only lean a stone a little toward blue, violet, green or warm. They tile, and they
    /// are saved as assets under stable names so a rebuild repaints them in place.
    /// </summary>
    public static class ProceduralTextures
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Textures";
        private const int Size = 1024;

        // What a stone may lean toward. Most stay plain.
        private static readonly Color[] Tints =
        {
            new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), new Color(0.9f, 0.96f, 1.1f), new Color(1.07f, 0.95f, 1.06f),
            new Color(0.9f, 1.04f, 0.94f), new Color(1.08f, 1f, 0.9f), new Color(0.95f, 0.95f, 1.02f),
        };
        private static readonly Color Moss = new Color(0.62f, 0.92f, 0.6f);

        /// <summary>Paving: rows of flagstones of uneven width, each its own tone, with painted edges, cracks and moss.</summary>
        public static Texture2D Flagstones() => Make("stone_flags", (x, y) => Stones(x, y, 6, 6, 0.6f, 0.03f, 0.32f, true, 3));

        /// <summary>Coursed wall blocks: long stones in rows, each row offset from the one below.</summary>
        public static Texture2D WallBlocks() => Make("stone_wall", (x, y) => Stones(x, y, 8, 6, 0.45f, 0.045f, 0.3f, false, 17));

        // Stones laid in rows. Each row is cut into stones one or two units wide; where the cuts fall is the row's own.
        private static Color Stones(float x, float y, int rows, int units, float splitChance, float jointWidth, float jointTone, bool moss, int salt)
        {
            float rowHeight = Size / (float)rows, unit = Size / (float)units;
            // The joints wander a little, so no line is ruled.
            float wy = Mathf.Repeat(y + (Noise(x, y, 256f) - 0.5f) * rowHeight * 0.08f, Size);
            int row = (int)(wy / rowHeight);
            float offset = Hash(row, salt) * unit;
            float wx = Mathf.Repeat(x + offset + (Noise(x + 300f, y, 256f) - 0.5f) * unit * 0.06f, Size);
            int column = (int)(wx / unit);
            // Walk the row from its start, a stone at a time, to the stone this point is in.
            int left = 0, right = 1;
            while (true)
            {
                right = left + 1 < units && !Cut(left + 1, row, units, splitChance, salt) ? left + 2 : left + 1;
                if (column < right) break;
                left = right;
            }
            float x0 = left * unit, x1 = right * unit, y0 = row * rowHeight, y1 = y0 + rowHeight;
            int id = left * 31 + row * 977 + salt;

            float fromLeft = wx - x0, fromRight = x1 - wx, fromBottom = wy - y0, fromTop = y1 - wy;
            float joint = jointWidth * rowHeight;
            // Rounded corners: the distance to the stone's edge, measured as to a box with its corners worn off.
            float corner = joint * (2.2f + 2.5f * Hash(id, 3));
            float dx = Mathf.Min(fromLeft, fromRight), dy = Mathf.Min(fromBottom, fromTop);
            float edge = dx > corner || dy > corner
                ? Mathf.Min(dx, dy)
                : corner - Mathf.Sqrt((corner - dx) * (corner - dx) + (corner - dy) * (corner - dy));
            float inside = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((edge - joint * 0.5f) / (joint * 0.5f)));

            // The stone's own flat tone and lean, with a broad brushed drift in three steps.
            float tone = 0.78f + 0.22f * Mathf.Round(Hash(id, 1) * 4f) / 4f;
            tone *= 0.97f + 0.06f * Mathf.Round(Noise(x, y, 64f) * 2f) / 2f;
            tone *= 0.92f + 0.1f * Noise(x + 91f, y + 17f, 512f);
            Color colour = Tints[(int)(Hash(id, 2) * Tints.Length) % Tints.Length] * tone;

            // Painted edges: light catches the top and left of each stone, and the bottom and right fall away.
            float band = joint * 2.4f;
            float lit = Mathf.Max(Band(fromTop, joint, band), Band(fromLeft, joint, band));
            float shaded = Mathf.Max(Band(fromBottom, joint, band), Band(fromRight, joint, band));
            colour *= 1f + 0.16f * lit - 0.2f * shaded * (1f - lit);

            // A crack across some stones, and chips of lighter stone on others.
            float chance = Hash(id, 5);
            if (chance > 0.72f)
            {
                float angle = Hash(id, 6) * Mathf.PI;
                float cx = (x0 + x1) * 0.5f + (Hash(id, 7) - 0.5f) * (x1 - x0) * 0.4f, cy = (y0 + y1) * 0.5f;
                float across = (wx - cx) * Mathf.Sin(angle) - (wy - cy) * Mathf.Cos(angle);
                float along = (wx - cx) * Mathf.Cos(angle) + (wy - cy) * Mathf.Sin(angle);
                across += (Noise(along * 3f, id, 64f) - 0.5f) * rowHeight * 0.1f;
                float reach = rowHeight * (0.25f + 0.3f * Hash(id, 8));
                if (Mathf.Abs(along) < reach)
                    colour *= Mathf.Lerp(0.5f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.Abs(across) / (Size / 512f * 1.6f)));
            }
            else if (chance < 0.2f)
            {
                float speck = Noise(x * 1.7f, y * 1.7f, 16f);
                if (speck > 0.8f) colour *= 1.08f;
            }

            Color jointColour = new Color(jointTone * 0.92f, jointTone * 0.95f, jointTone * 1.1f);
            if (moss)
            {
                // Moss gathers in the joints and creeps on to the stones beside them, in patches.
                float patch = Noise(x + 40f, y + 77f, 256f) * 0.7f + Noise(x, y, 64f) * 0.3f;
                float near = 1f - Mathf.Clamp01(edge / (joint * 5f));
                float grown = Mathf.SmoothStep(0.5f, 0.58f, patch) * near;
                colour = Color.Lerp(colour, Moss * (0.62f + 0.14f * Mathf.Round(Noise(x, y, 32f) * 2f) / 2f), grown * 0.85f);
                jointColour = Color.Lerp(jointColour, Moss * 0.42f, Mathf.SmoothStep(0.44f, 0.54f, patch));
            }
            return Color.Lerp(jointColour, colour, inside);
        }

        private static bool Cut(int boundary, int row, int units, float chance, int salt) =>
            boundary % units == 0 || Hash(boundary * 7 + salt, row * 13) < chance;

        // 1 at the stone's very edge (just inside the joint), falling to 0 a band's width in.
        private static float Band(float distance, float joint, float width) =>
            1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distance - joint * 0.5f) / width));

        /// <summary>Planks running along the texture's height: boards of differing tone, with grain, knots and a gap between.</summary>
        public static Texture2D Planks() => Make("wood_planks", (x, y) =>
        {
            const int Boards = 5;
            float width = Size / (float)Boards;
            int board = (int)(x / width);
            float fx = Mathf.Repeat(x, width) / width;
            float tone = 0.74f + 0.24f * Mathf.Round(Hash(board, 5) * 3f) / 3f;
            // Grain: long streaks that run with the board and bend round its knots.
            float knotY = Hash(board, 9) * Size, knotX = (board + 0.3f + 0.4f * Hash(board, 10)) * width;
            float toKnot = Mathf.Sqrt((x - knotX) * (x - knotX) + Mathf.Pow(Mathf.DeltaAngle(0f, (y - knotY) / Size * 360f) / 360f * Size, 2f) * 0.3f);
            float bend = Mathf.Exp(-toKnot / (width * 0.25f));
            float grain = Noise(x * 6f + bend * 60f, y * 0.35f, 32f);
            tone *= 0.93f + 0.1f * Mathf.Round(grain * 3f) / 3f;
            if (toKnot < width * 0.07f) tone *= 0.62f;
            float lit = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((fx - 0.04f) / 0.08f));
            float shaded = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((0.96f - fx) / 0.08f));
            tone *= 1f + 0.12f * lit - 0.16f * shaded;
            float gap = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Min(fx, 1f - fx) / 0.035f));
            float value = Mathf.Lerp(0.26f, tone, gap);
            return new Color(value * 1.03f, value, value * 0.95f);
        });

        /// <summary>Packed earth: blotches of three tones, pebbles, and tufts of dead grass.</summary>
        public static Texture2D Ground() => Make("ground_earth", (x, y) =>
        {
            float broad = Noise(x, y, 256f) * 0.6f + Noise(x + 50f, y + 90f, 128f) * 0.4f;
            float tone = 0.8f + 0.2f * Mathf.Round(broad * 3f) / 3f;
            tone *= 0.96f + 0.05f * Noise(x, y, 32f);
            Color colour = new Color(tone * 1.02f, tone, tone * 0.96f);
            // Pebbles: small rounds, lighter than the earth, each with a dark side.
            const float Cell = 64f;
            int cx = (int)(x / Cell), cy = (int)(y / Cell);
            if (Hash(cx + 400, cy) > 0.8f)
            {
                float px = (cx + 0.2f + 0.6f * Hash(cx, cy + 9)) * Cell, py = (cy + 0.2f + 0.6f * Hash(cx + 5, cy)) * Cell;
                float radius = Cell * (0.06f + 0.1f * Hash(cx + 1, cy + 1));
                float distance = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
                if (distance < radius) colour *= (y - py) > -radius * 0.2f ? 1.1f : 0.86f;
            }
            // Dead grass: patches leaning green-grey.
            float grass = Mathf.SmoothStep(0.6f, 0.7f, Noise(x + 11f, y + 300f, 128f));
            return Color.Lerp(colour, new Color(0.78f, 0.9f, 0.66f) * tone, grass * 0.6f);
        });

        /// <summary>
        /// Dressed stone, for caps, steps and plinths: large square-cut blocks of one flat tone, each with a line chiselled
        /// a little way in from its edge, a narrow joint, and a chip knocked out here and there. Nothing blotchy: worked
        /// stone is even, and its only drawing is the mason's line.
        /// </summary>
        public static Texture2D DressedStone() => Make("stone_dressed", (x, y) =>
        {
            const int Blocks = 2;
            float block = Size / (float)Blocks;
            int column = (int)(x / block), row = (int)(y / block);
            int id = column * 53 + row * 911 + 7;
            float fx = Mathf.Repeat(x, block), fy = Mathf.Repeat(y, block);
            float edge = Mathf.Min(Mathf.Min(fx, block - fx), Mathf.Min(fy, block - fy));
            // One flat tone a block, barely apart from its neighbours, with the faintest brushed drift across it.
            float tone = 0.9f + 0.06f * Mathf.Round(Hash(id, 1) * 2f) / 2f;
            tone *= 0.985f + 0.03f * Noise(x + 13f, y + 71f, 512f);
            // The chiselled line: dark, with a thin light lip on its inner side where the cut catches the light.
            float lineAt = block * 0.085f, lineWide = block * 0.012f;
            float line = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Abs(edge - lineAt) / lineWide);
            float lip = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Abs(edge - lineAt - lineWide * 2.2f) / lineWide);
            tone *= 1f - 0.3f * line + 0.07f * lip * (1f - line);
            // A few chips, mostly along the edges: flat darker flakes with a hard outline.
            const float Cell = 128f;
            int cx = (int)(x / Cell), cy = (int)(y / Cell);
            if (Hash(cx + 77, cy + 5) > 0.84f)
            {
                float px = (cx + 0.25f + 0.5f * Hash(cx, cy + 31)) * Cell, py = (cy + 0.25f + 0.5f * Hash(cx + 17, cy)) * Cell;
                float radius = Cell * (0.1f + 0.12f * Hash(cx + 3, cy + 3));
                float reach = Mathf.Abs(x - px) * (0.7f + 0.6f * Hash(cx, cy)) + Mathf.Abs(y - py);
                if (reach < radius) tone *= reach > radius * 0.72f ? 0.74f : 0.86f;
            }
            float joint = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edge / (block * 0.014f)));
            float value = Mathf.Lerp(0.42f, tone, joint);
            return new Color(value * 0.99f, value, value * 1.02f);
        });

        /// <summary>Roof tiles: overlapping rows of rounded tiles, each row shading the one below.</summary>
        public static Texture2D RoofTiles() => Make("roof_tiles", (x, y) =>
        {
            const int Rows = 8, Columns = 8;
            float rowHeight = Size / (float)Rows, width = Size / (float)Columns;
            int row = (int)(y / rowHeight);
            float shifted = Mathf.Repeat(x + (row % 2) * width * 0.5f, Size);
            int column = (int)(shifted / width);
            float fx = Mathf.Repeat(shifted, width) / width, fy = Mathf.Repeat(y, rowHeight) / rowHeight;
            // Each tile's lower edge is a curve, and the tile above casts a band of shade across its top.
            float lower = 0.18f + 0.16f * (1f - Mathf.Abs(fx - 0.5f) * 2f) * (1f - Mathf.Abs(fx - 0.5f) * 2f);
            float tone = 0.76f + 0.24f * Mathf.Round(Hash(column, row + 60) * 3f) / 3f;
            if (fy < lower * 0.5f) tone *= 0.55f;
            else if (fy < lower) tone *= 1.14f;
            if (fy > 0.86f) tone *= 0.7f;
            float side = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Min(fx, 1f - fx) / 0.05f));
            float value = Mathf.Lerp(0.4f, tone, side);
            return new Color(value, value, value);
        });

        private static Texture2D Make(string name, System.Func<float, float, Color> paint)
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
            // Painted once per editor session: every builder asks for the same few textures.
            if (painted.Contains(name)) return texture;
            painted.Add(name);
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    Color colour = paint(x, y);
                    pixels[y * Size + x] = new Color32(Byte(colour.r), Byte(colour.g), Byte(colour.b), 255);
                }
            }
            texture.SetPixels32(pixels);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 8;
            texture.Apply(true);
            EditorUtility.SetDirty(texture);
            return texture;
        }

        private static readonly System.Collections.Generic.HashSet<string> painted = new System.Collections.Generic.HashSet<string>();

        /// <summary>Makes the next request for each texture paint it afresh; for use after changing how one is painted.</summary>
        [MenuItem("Clover/Build/Repaint Textures")]
        public static void Repaint()
        {
            painted.Clear();
            Flagstones();
            WallBlocks();
            Planks();
            Ground();
            DressedStone();
            RoofTiles();
            AssetDatabase.SaveAssets();
            Debug.Log("[Clover.Build] Repainted the generated textures.");
        }

        private static byte Byte(float value) => (byte)Mathf.Clamp(Mathf.RoundToInt(value * 255f), 0, 255);

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
