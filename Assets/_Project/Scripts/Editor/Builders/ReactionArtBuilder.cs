using System.Collections.Generic;
using System.IO;
using System.Text;
using Clover.Gameplay;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the 24 designed reactions' looks (<see cref="ReactionArtSet"/>) from the look-dev's recipe
    /// (Art/VFX/Combos/combos_recipe.json) and the kits it names (the eleven element kits in Art/VFX/&lt;Element&gt;/ and
    /// the two combo kits in Art/VFX/Combos/). Each recipe piece is taken from its kit in its own axes, placed by the
    /// recipe's position, rotation and scale about the main body's feet, and painted in its bands:
    /// - solid pieces are merged into one mesh for each place (on the body, on the ground), element and motion role
    ///   (<see cref="ReactionTimelines.RoleOf"/>: a bolt, tongues, steam, chips, a splash...; tongues split one kit
    ///   object to a part when the timeline staggers them and the budget allows), drawn in that element's inked solid
    ///   material from the element art (m_element_solid_&lt;element&gt;), with the smoothed normals the ink needs;
    /// - see-through pieces (beams, fog, a gas layer held up) into glow meshes the same way, in the element art's glow
    ///   material;
    /// - each part takes its cue from the look's timeline (<see cref="ReactionTimelines.StepFor"/>) and its mesh is
    ///   moved to the pivot that cue moves it about (<see cref="PivotOf"/>); a look over <see cref="PartBudget"/> parts
    ///   first keeps tongues together, then folds its smallest groups into the biggest of the same place and element;
    /// - flat pieces lying low (thinner than 6 cm once placed and no higher than 0.3 m: char, rosettes, fern, rings, the
    ///   drained disc, the eclipse) are rasterised from
    ///   above into the look's floor masks, up to four colours to a mask, lowest first, each drawn by its own
    ///   Clover/Decal material;
    /// - the recipe's point lamps become at most two URP lamps (<see cref="ReactionDesigns.LampIntensity"/>), the
    ///   dimmest left out.
    /// A re-coloured piece wears the recipe's material in its first slot and its kit's own in the rest. The bodies (and
    /// their recolours: bleach, pallor, frost, the after-image ghosts) are the game's own enemies, not built here.
    /// Everything lands under Generated/Vfx/Reaction/ under stable names; the set keeps its GUID across rebuilds and is
    /// handed to the element art's set, which every scene with the art already holds. Run Clover > Build > Element Art
    /// first. Writes review/checks/reaction_art.md.
    /// </summary>
    public static class ReactionArtBuilder
    {
        public const string Folder = BuildKit.GeneratedRoot + "/Vfx/Reaction";
        public const string SetPath = Folder + "/reaction_art.asset";
        public const string CombosFolder = "Assets/_Project/Art/VFX/Combos";
        public const string RecipePath = CombosFolder + "/combos_recipe.json";
        private const string KitRoot = "Assets/_Project/Art/VFX";
        private const string ReportPath = "review/checks/reaction_art.md";

        /// <summary>Thinner than this once placed, a piece is a mark on the floor (as the element art judges its flat pieces).</summary>
        public const float FlatHeight = 0.06f;
        /// <summary>A flat piece is a floor mark only when it lies no higher than this (the eclipse at 0.2 m does; Logan Stone's arcs over the head do not).</summary>
        public const float MarkHeight = 0.3f;
        /// <summary>
        /// A look over this many triangles is over its budget (the crowd shows about a dozen at once): the build warns and
        /// the EditMode suite (ReactionArtTests) fails on it. The recipe's heaviest, Steam, is about 7,400.
        /// </summary>
        public const int TriangleBudget = 8000;
        /// <summary>A mask's colours: one to a channel.</summary>
        public const int ColoursPerMask = 4;
        /// <summary>The most parts a look is built into: each is a renderer the crowd's pool must find.</summary>
        public const int PartBudget = 10;
        /// <summary>A burst or a spin centred this close to the body's axis turns about the body itself.</summary>
        private const float NearBody = 1.2f;

        /// <summary>A material's look as the builder paints it: linear colour (alpha: how see-through), Clover/Spell's flag, glow or not.</summary>
        public struct Paint
        {
            public Color Colour;
            public float Flag;
            public bool Glow;

            public Paint(float r, float g, float b, float a = 1f, float flag = 1f, bool glow = false)
            {
                Colour = new Color(r, g, b, a);
                Flag = flag;
                Glow = glow;
            }
        }

        // The materials only the combos use, first-pass pieces and combo re-colours, in linear colour from notes.md
        // ("Stage 1" bands, "Stage 2: combos" and "Combos pass 2"). The pass-2 materials come from the element art's table.
        private static readonly Dictionary<string, Paint> ComboPaints = new Dictionary<string, Paint>
        {
            // Fire: no white; char and smoke near black.
            ["m_vfx_fire_ember"] = new Paint(1f, 0.42f, 0.05f),
            ["m_vfx_fire_flame"] = new Paint(1f, 0.24f, 0.02f),
            ["m_vfx_fire_star"] = new Paint(1f, 0.35f, 0.03f),
            ["m_vfx_fire_char"] = new Paint(0.045f, 0.022f, 0.016f, 0.92f),
            ["m_vfx_fire_smoke"] = new Paint(0.09f, 0.05f, 0.04f, 0.85f),
            // The combos' own: coal seams, plasma flame (flame bands with a violet heart), slag, frost white, steam,
            // the slick's yellow-green sheen, the white flash, Fire Medicine's green heart.
            ["m_vfx_combo_coal"] = new Paint(1f, 0.3f, 0.03f),
            ["m_vfx_combo_plasmaflame"] = new Paint(0.55f, 0.14f, 0.9f),
            ["m_vfx_combo_slag"] = new Paint(0.06f, 0.04f, 0.035f),
            ["m_vfx_combo_frost"] = new Paint(0.82f, 0.93f, 1f),
            ["m_vfx_combo_steam"] = new Paint(0.86f, 0.86f, 0.84f),
            ["m_vfx_combo_sheen"] = new Paint(0.5f, 0.62f, 0.12f, 0.6f),
            ["m_vfx_combo_whiteflash"] = new Paint(1.3f, 1.3f, 1.35f),
            ["m_vfx_combo_greenheart"] = new Paint(0.25f, 0.75f, 0.25f),
            ["mc2_breath"] = new Paint(0.9f, 0.92f, 0.95f),
            // Ice: cyan, pale facet; the fog is seen through.
            ["m_vfx_ice_frost"] = new Paint(0.75f, 0.93f, 1f),
            ["m_vfx_ice_fog"] = new Paint(0.7f, 0.85f, 0.95f, 0.35f, 1f, true),
            // Water: turquoise, foam on crests (Clover/Spell's foam flag), ink.
            ["m_vfx_water"] = new Paint(0.08f, 0.6f, 0.62f),
            ["m_vfx_water_foam"] = new Paint(0.85f, 0.95f, 0.95f, 1f, 0.3f),
            ["m_vfx_water_ink"] = new Paint(0.01f, 0.05f, 0.1f),
            // Lightning's violet edge; Wind's off-white and dry leaf.
            ["m_vfx_lightning_edge"] = new Paint(0.28f, 0.08f, 0.75f),
            ["m_vfx_wind_air"] = new Paint(0.82f, 0.84f, 0.78f),
            ["m_vfx_wind_leaf"] = new Paint(0.5f, 0.42f, 0.16f),
            // Earth: ochre dust, the ochre crack's lip, near-black cracks, and pass 2's dark upturned dust.
            ["m_vfx_earth_dust"] = new Paint(0.42f, 0.36f, 0.26f),
            ["m_vfx_earth_ochrecrack"] = new Paint(0.55f, 0.36f, 0.14f),
            ["m_vfx_earth_crack"] = new Paint(0.025f, 0.018f, 0.012f, 0.95f),
            ["m2_earth_dust"] = new Paint(0.12f, 0.07f, 0.035f),
            // Metal: spark heads, steel, a pale burr.
            ["m_vfx_metal_sparkhead"] = new Paint(1f, 0.8f, 0.35f),
            ["m_vfx_metal_steel"] = new Paint(0.45f, 0.47f, 0.5f),
            ["m2_metal_burr"] = new Paint(0.62f, 0.62f, 0.64f),
            // Poison: blue-green venom and bile, never white.
            ["m_vfx_poison_venom"] = new Paint(0.08f, 0.42f, 0.33f),
            ["m_vfx_poison_bile"] = new Paint(0.7f, 0.65f, 0.12f),
            // Life: leaf green and the dark green of stems.
            ["m_vfx_life_leaf"] = new Paint(0.12f, 0.5f, 0.08f),
            ["m_vfx_life_stem"] = new Paint(0.05f, 0.2f, 0.04f),
            ["m2_life_dark"] = new Paint(0.03f, 0.14f, 0.03f),
            // Dark: ink black-violet.
            ["m_vfx_dark_body"] = new Paint(0.02f, 0.005f, 0.03f),
        };

        // The combos' own materials by the element whose ink they are drawn in.
        private static readonly Dictionary<string, string> ComboElements = new Dictionary<string, string>
        {
            ["coal"] = "fire", ["plasmaflame"] = "fire", ["slag"] = "fire", ["frost"] = "ice", ["steam"] = "water", ["sheen"] = "water",
            ["whiteflash"] = "light", ["violetedge"] = "lightning", ["blackcore"] = "lightning", ["greenheart"] = "poison", ["ghost"] = "dark",
        };

        private static readonly StringBuilder Report = new StringBuilder();
        private static readonly StringBuilder Motion = new StringBuilder();

        /// <summary>A material's paint: the combos' table, then the element art's; false (drawn grey) for one neither names.</summary>
        public static bool TryPaint(string material, out Paint paint)
        {
            if (material != null && ComboPaints.TryGetValue(material, out paint)) return true;
            bool known = ElementArtBuilder.TryBand(material, out var colour, out float flag, out bool glow);
            paint = new Paint { Colour = colour, Flag = flag, Glow = glow };
            return known;
        }

        /// <summary>
        /// The element whose solid material (and so ink) a piece of this material is drawn in: the element its name
        /// carries ("m2_bolt_cyan" is Lightning's, "m_vfx_combo_coal" Fire's, "mc2_breath" Ice's), else the pair's first.
        /// </summary>
        public static string ElementOf(string material, string pair)
        {
            string fallback = pair != null && pair.IndexOf('_') > 0 ? pair.Substring(0, pair.IndexOf('_')) : "fire";
            if (string.IsNullOrEmpty(material)) return fallback;
            if (material == "mc2_breath") return "ice";
            string rest = material.StartsWith("m_vfx_combo_") ? material.Substring(12)
                : material.StartsWith("m_vfx_") ? material.Substring(6)
                : material.StartsWith("m2_") || material.StartsWith("m3_") ? material.Substring(3) : material;
            if (material.StartsWith("m_vfx_combo_")) return ComboElements.TryGetValue(rest, out string owner) ? owner : fallback;
            int bar = rest.IndexOf('_');
            string word = bar > 0 ? rest.Substring(0, bar) : rest;
            if (word == "bolt") return "lightning";
            return ReactionDesigns.RingIndex(word) >= 0 ? word : fallback;
        }

        /// <summary>The kit file's place in the project: combo kits in Art/VFX/Combos/, element kits in their element's folder.</summary>
        public static string KitPath(string kit)
        {
            if (kit.StartsWith("vfx_combo_")) return $"{CombosFolder}/{kit}";
            int end = kit.IndexOf("_kit");
            string element = end > 4 ? kit.Substring(4, end - 4) : kit;
            return $"{KitRoot}/{char.ToUpperInvariant(element[0]) + element.Substring(1)}/{kit}";
        }

        /// <summary>The recipe as a text asset; null before it has been brought in.</summary>
        public static TextAsset RecipeAsset() => AssetDatabase.LoadAssetAtPath<TextAsset>(RecipePath);

        /// <summary>The built set; null before Clover > Build > Reaction Art has run.</summary>
        public static ReactionArtSet Set() => AssetDatabase.LoadAssetAtPath<ReactionArtSet>(SetPath);

        // ---- Building.

        // What one place, element and motion role of a look gathers before it becomes a mesh.
        private sealed class Merge
        {
            public string Element;
            public byte Kind, Place, Role;
            /// <summary>The timeline step it follows (-1: its role's default) and, for a staggered step, the kit object it holds.</summary>
            public int Step;
            public string Sub = "";
            public ReactionTimelines.Step Follows;
            public readonly List<Vector3> Vertices = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Color> Colours = new List<Color>();
            public readonly List<int> Triangles = new List<int>();
            public int Pieces;

            // Another merge's pieces join this one's (it keeps its own role and cue).
            public void Take(Merge other)
            {
                int first = Vertices.Count;
                Vertices.AddRange(other.Vertices);
                Normals.AddRange(other.Normals);
                Colours.AddRange(other.Colours);
                foreach (int index in other.Triangles) Triangles.Add(first + index);
                Pieces += other.Pieces;
            }
        }

        private struct Flat
        {
            public Vector2 A, B, C;
            public int Colour;
        }

        // Everything one combo gathers.
        private sealed class Gathered
        {
            public readonly List<Merge> Merges = new List<Merge>();
            public readonly List<Flat> Flats = new List<Flat>();
            public readonly List<Color> FlatColours = new List<Color>();
            public readonly List<float> FlatHeights = new List<float>();
            public readonly List<string> Missing = new List<string>();
            public readonly HashSet<string> Grey = new HashSet<string>();
            public int FlatPieces;
            /// <summary>Groups folded into others to keep within the part budget.</summary>
            public readonly List<string> Folded = new List<string>();

            public int MaskCount => (FlatColours.Count + ColoursPerMask - 1) / ColoursPerMask;

            public Merge MergeFor(byte place, byte kind, string element, byte role, int step, ReactionTimelines.Step follows, string sub)
            {
                foreach (var merge in Merges)
                    if (merge.Place == place && merge.Kind == kind && merge.Element == element && merge.Role == role && merge.Step == step && merge.Sub == sub) return merge;
                var made = new Merge { Place = place, Kind = kind, Element = element, Role = role, Step = step, Follows = follows, Sub = sub };
                Merges.Add(made);
                return made;
            }

            public int ColourFor(Color colour, float height)
            {
                int found = FlatColours.IndexOf(colour);
                if (found < 0)
                {
                    FlatColours.Add(colour);
                    FlatHeights.Add(height);
                    return FlatColours.Count - 1;
                }
                FlatHeights[found] = Mathf.Min(FlatHeights[found], height);
                return found;
            }
        }

        [MenuItem("Clover/Build/Reaction Art")]
        public static void Build()
        {
            Report.Clear();
            Motion.Clear();
            var recipeAsset = RecipeAsset();
            if (recipeAsset == null)
            {
                Debug.LogError($"[Clover.Build] No reaction recipe at {RecipePath}; bring the combo kits and recipe in first.");
                return;
            }
            var elementArt = ElementArtBuilder.Set();
            var glow = AssetDatabase.LoadAssetAtPath<Material>(ElementArtBuilder.Folder + "/m_element_glow.mat");
            if (elementArt == null || elementArt.Solids == null || elementArt.Solids.Length != ReactionDesigns.Ring.Length || glow == null)
            {
                Debug.LogError("[Clover.Build] Build the element art first (Clover > Build > Element Art): the reactions wear its materials.");
                return;
            }
            ReactionRecipe recipe;
            try
            {
                recipe = ReactionRecipe.Parse(recipeAsset.text);
            }
            catch (System.FormatException problem)
            {
                Debug.LogError($"[Clover.Build] The reaction recipe at {RecipePath} could not be read: {problem.Message}");
                return;
            }

            BuildKit.EnsureFolder(Folder);
            var set = Set();
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ReactionArtSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }
            // Last build's meshes go; the set itself (and its GUID) stays.
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(SetPath))
                if (old is Mesh)
                {
                    AssetDatabase.RemoveObjectFromAsset(old);
                    Object.DestroyImmediate(old, true);
                }

            var kits = new Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)>();
            var looks = new List<ReactionArtSet.Look>();
            var parts = new List<ReactionArtSet.Part>();
            var lamps = new List<ReactionArtSet.Lamp>();
            var masks = new List<(string name, Texture2D texture, int part, Color[] colours)>();
            var written = new HashSet<string>();
            int progress = UnityEditor.Progress.Start("Reaction art", "Merging the recipe's pieces and drawing their floor masks");
            try
            {
                for (int c = 0; c < recipe.Combos.Count; c++)
                {
                    var combo = recipe.Combos[c];
                    UnityEditor.Progress.Report(progress, c / (float)recipe.Combos.Count, combo.Name);
                    int pair = ReactionDesigns.PairOf(combo.Pair);
                    if (pair < 0)
                    {
                        Debug.LogError($"[Clover.Build] Reaction art: the recipe's '{combo.Pair}' ({combo.Name}) names no pair; left out.");
                        continue;
                    }
                    var gathered = Grouped(combo, kits);
                    ReactionTimelines.TryGet(combo.Pair, out var timeline);
                    var look = new ReactionArtSet.Look
                    {
                        Pair = pair, Key = combo.Pair, Name = combo.Name, Directed = ReactionDesigns.IsDirected(combo.Pair),
                        FirstPart = parts.Count, FirstLamp = lamps.Count, Pieces = combo.PieceCount, Story = timeline.Story ?? "",
                    };
                    // Body first, then ground; solids before glows; marks last, so they are drawn over in that order.
                    gathered.Merges.Sort((x, y) => x.Place != y.Place ? x.Place.CompareTo(y.Place) : x.Kind != y.Kind ? x.Kind.CompareTo(y.Kind)
                        : x.Element != y.Element ? string.CompareOrdinal(x.Element, y.Element) : x.Role != y.Role ? x.Role.CompareTo(y.Role) : string.CompareOrdinal(x.Sub, y.Sub));
                    foreach (var merge in gathered.Merges)
                    {
                        var part = MakePart(set, combo.Pair, merge, merge.Kind == ReactionArtSet.Glow ? glow : elementArt.Solids[ReactionDesigns.RingIndex(merge.Element)],
                            CueOf(merge, gathered.Merges));
                        look.Triangles += part.Triangles;
                        parts.Add(part);
                    }
                    MakeMasks(combo.Pair, gathered, parts, masks, written);
                    look.PartCount = parts.Count - look.FirstPart;
                    Timing(ref look, parts);

                    var watts = new float[combo.Lamps.Count];
                    for (int i = 0; i < watts.Length; i++) watts[i] = combo.Lamps[i].Watts;
                    var kept = new int[ReactionDesigns.MostLampsPerLook];
                    int keptCount = ReactionDesigns.KeepBrightest(watts, ReactionDesigns.MostLampsPerLook, kept);
                    for (int i = 0; i < keptCount; i++)
                    {
                        var lamp = combo.Lamps[kept[i]];
                        lamps.Add(new ReactionArtSet.Lamp
                        {
                            Position = lamp.Position,
                            // Unity takes a lamp's colour as the eye sees it; the recipe's is linear.
                            Colour = lamp.Colour.gamma,
                            Intensity = ReactionDesigns.LampIntensity(lamp.Watts),
                            Range = ReactionDesigns.LampRange(lamp.Watts, lamp.Range),
                        });
                    }
                    look.LampCount = keptCount;
                    looks.Add(look);
                    ReportLook(combo, look, parts, gathered, combo.Lamps.Count, keptCount, watts, kept);
                }
            }
            finally
            {
                UnityEditor.Progress.Remove(progress);
            }

            // The masks are written and imported first, then their materials point at them.
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var mask in masks) File.WriteAllBytes($"{Folder}/{mask.name}_mask.png", mask.texture.EncodeToPNG());
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            foreach (var mask in masks)
            {
                AssetDatabase.ImportAsset($"{Folder}/{mask.name}_mask.png", ImportAssetOptions.ForceUpdate);
                var part = parts[mask.part];
                part.Material = DecalMaterial(mask.name, AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{mask.name}_mask.png"), mask.colours);
                parts[mask.part] = part;
                Object.DestroyImmediate(mask.texture);
            }
            RemoveStale(written);

            looks.Sort((x, y) => x.Pair.CompareTo(y.Pair));
            set.Looks = looks.ToArray();
            set.Parts = parts.ToArray();
            set.Lamps = lamps.ToArray();
            EditorUtility.SetDirty(set);
            elementArt.Reactions = set;
            EditorUtility.SetDirty(elementArt);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int designed = looks.Count;
            Directory.CreateDirectory("review/checks");
            var composed = new StringBuilder();
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                if (looks.Exists(look => look.Pair == pair)) continue;
                ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out _);
                ReactionTimelines.Composed(pair, out var arrives, out var answers, out var spreads);
                composed.AppendLine($"| {AfflictionPairs.Name(pair)} | {first} ({arrives}) | {second} ({answers}) | {(mark != null ? $"{mark} ({spreads})" : "-")} |");
            }
            int mostParts = 0;
            foreach (var look in looks) mostParts = Mathf.Max(mostParts, look.PartCount);
            File.WriteAllText(ReportPath,
                $"# Reaction art\n\n{designed} designed looks from {RecipePath}, {parts.Count} parts (at most {mostParts} a look, budget {PartBudget}), {lamps.Count} lamps, {masks.Count} floor masks. " +
                $"A look over {TriangleBudget} triangles is marked OVER. Lamps: intensity = W / (4 pi^2), range sqrt(2 I) held to " +
                $"{ReactionDesigns.LeastLampRange}-{ReactionDesigns.MostLampRange} m, at most {ReactionDesigns.MostLampsPerLook} a look, lit at the impact.\n\n" +
                "| Reaction (recipe) | Game name | Pieces | Parts (solid, glow, marks) | Triangles | Lamps kept of | Impact, length | Notes |\n|---|---|---|---|---|---|---|---|\n" + Report +
                "\n## Motion\n\nEach part's verb, beat and time (ReactionTimelines; ReactionMotion).\n\n" + Motion +
                $"\n## Composed ({AfflictionPairs.Count - designed})\n\n| Reaction | First parent's piece (arrives) | Second parent's piece (answers) | Ground mark (spreads) |\n|---|---|---|---|\n" + composed);
            Debug.Log($"[Clover.Build] Reaction art: {designed} looks, {parts.Count} parts (at most {mostParts} a look), {lamps.Count} lamps, {masks.Count} masks, {AfflictionPairs.Count - designed} composed; {ReportPath}");
        }

        // A combo's pieces grouped by motion within the part budget: staggered pieces split one kit object to a part if
        // that fits, else kept together; then, while still over, the smallest group is folded into the biggest of the
        // same place, kind and element (it moves with that one).
        private static Gathered Grouped(ReactionRecipe.Combo combo, Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)> kits)
        {
            var gathered = Gather(combo, kits, true);
            if (gathered.Merges.Count + gathered.MaskCount > PartBudget) gathered = Gather(combo, kits, false);
            while (gathered.Merges.Count + gathered.MaskCount > PartBudget)
            {
                Merge smallest = null, into = null;
                foreach (var merge in gathered.Merges)
                {
                    Merge biggest = null;
                    foreach (var other in gathered.Merges)
                        if (other != merge && other.Place == merge.Place && other.Kind == merge.Kind && other.Element == merge.Element &&
                            (biggest == null || other.Triangles.Count > biggest.Triangles.Count)) biggest = other;
                    if (biggest == null || biggest.Triangles.Count < merge.Triangles.Count) continue;
                    if (smallest == null || merge.Triangles.Count < smallest.Triangles.Count)
                    {
                        smallest = merge;
                        into = biggest;
                    }
                }
                if (smallest == null)
                {
                    Debug.LogWarning($"[Clover.Build] Reaction art: {combo.Name} has {gathered.Merges.Count + gathered.MaskCount} parts, over its budget of {PartBudget}, and nothing left to fold.");
                    break;
                }
                into.Take(smallest);
                gathered.Merges.Remove(smallest);
                gathered.Folded.Add($"{ReactionTimelines.RoleNames[smallest.Role]} into {ReactionTimelines.RoleNames[into.Role]} ({smallest.Element})");
            }
            return gathered;
        }

        // A part's cue: its step's, later by the stagger for each kit object of the same step before it, flashing white
        // only where a cold hard thing lands in a solid.
        private static ReactionMotion.Cue CueOf(Merge merge, List<Merge> all)
        {
            var cue = merge.Follows.Cue;
            if (merge.Follows.Stagger > 0f && merge.Sub.Length > 0)
            {
                int before = 0;
                foreach (var other in all)
                    if (other.Step == merge.Step && other.Role == merge.Role && other.Sub.Length > 0 && string.CompareOrdinal(other.Sub, merge.Sub) < 0) before++;
                cue.Delay += before * merge.Follows.Stagger;
            }
            cue.Flash = cue.Flash && merge.Kind == ReactionArtSet.Solid && ReactionTimelines.Flashes(merge.Element);
            return cue;
        }

        // When a look's impact lands (the first impact part to land; a quarter second in for a look with none) and when its last part is gone.
        private static void Timing(ref ReactionArtSet.Look look, List<ReactionArtSet.Part> parts)
        {
            float impact = float.MaxValue, end = 0f;
            for (int i = look.FirstPart; i < look.FirstPart + look.PartCount; i++)
            {
                var cue = parts[i].Motion;
                if (cue.Beat == ReactionMotion.Impact) impact = Mathf.Min(impact, cue.LandsAt);
                end = Mathf.Max(end, cue.End);
            }
            look.ImpactAt = impact < float.MaxValue ? impact : 0.25f;
            look.Seconds = end;
        }

        /// <summary>
        /// The point a part moves about, in the recipe's space, for its verb: the base of what falls, rises, sinks, licks
        /// or spreads; the body's axis (at the part's height) for a burst or a spin round the body, else its own middle;
        /// the top of a bolt drawn down (<paramref name="axis"/> down), the end nearest the body of any other ray
        /// (<paramref name="axis"/> toward its far end); the middle of the rest. <paramref name="axis"/> is zero but for a ray.
        /// </summary>
        public static Vector3 PivotOf(byte verb, byte role, IList<Vector3> vertices, out Vector3 axis)
        {
            axis = Vector3.zero;
            if (vertices == null || vertices.Count == 0) return Vector3.zero;
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            Vector3 middle = bounds.center;
            float offAxis = new Vector2(middle.x, middle.z).magnitude;
            switch (verb)
            {
                case ReactionMotion.Fall:
                case ReactionMotion.Rise:
                case ReactionMotion.Sink:
                case ReactionMotion.Lick:
                case ReactionMotion.Spread:
                    return new Vector3(middle.x, bounds.min.y, middle.z);
                case ReactionMotion.Burst:
                    return offAxis < NearBody ? new Vector3(0f, Mathf.Clamp(middle.y, 0.3f, 1.6f), 0f) : middle;
                case ReactionMotion.Spin:
                    return offAxis < NearBody ? new Vector3(0f, middle.y, 0f) : middle;
                case ReactionMotion.Extend:
                {
                    if (role == ReactionTimelines.Strike)
                    {
                        axis = Vector3.down;
                        return new Vector3(middle.x, bounds.max.y, middle.z);
                    }
                    Vector3 root = vertices[0];
                    foreach (var v in vertices)
                        if (v.x * v.x + v.z * v.z < root.x * root.x + root.z * root.z) root = v;
                    Vector3 reach = middle - root;
                    axis = reach.sqrMagnitude > 0.0025f ? reach.normalized : Vector3.up;
                    return root;
                }
                default:
                    return middle;
            }
        }

        // Every piece of a combo, placed and sorted into merges and flat triangles; staggered steps split by kit object when asked.
        private static Gathered Gather(ReactionRecipe.Combo combo, Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)> kits, bool split)
        {
            var gathered = new Gathered();
            foreach (var group in combo.Groups)
            {
                if (!kits.TryGetValue(group.Kit, out var kit))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(KitPath(group.Kit));
                    var pieces = new Dictionary<string, MeshFilter>();
                    if (asset != null)
                        foreach (var filter in asset.GetComponentsInChildren<MeshFilter>(true)) pieces[filter.name] = filter;
                    else Debug.LogError($"[Clover.Build] Reaction art: no kit at {KitPath(group.Kit)}.");
                    kit = (asset != null ? asset.transform : null, pieces);
                    kits[group.Kit] = kit;
                }
                if (!kit.pieces.TryGetValue(group.KitObject, out var piece) || piece.sharedMesh == null)
                {
                    gathered.Missing.Add($"{group.KitObject} ({group.Kit}) x{group.Instances.Count}");
                    continue;
                }
                foreach (var instance in group.Instances) AddPiece(combo, group, instance, piece, kit.root, gathered, split);
            }
            return gathered;
        }

        private static void AddPiece(ReactionRecipe.Combo combo, ReactionRecipe.Group group, ReactionRecipe.Instance instance, MeshFilter filter, Transform kitRoot,
            Gathered gathered, bool split)
        {
            var source = filter.sharedMesh;
            // The piece in its own axes (Unity's), at its own origin wherever it lies in the kit file, then where the recipe puts it.
            var toKit = kitRoot.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            toKit.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            var place = instance.Matrix * toKit;
            var turnNormals = place.inverse.transpose;
            var vertices = source.vertices;
            var normals = source.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = place.MultiplyPoint3x4(vertices[i]);
                if (i < normals.Length) normals[i] = turnNormals.MultiplyVector(normals[i]).normalized;
            }
            var bounds = new Bounds(vertices.Length > 0 ? vertices[0] : Vector3.zero, Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);

            var renderer = filter.GetComponent<MeshRenderer>();
            var slots = renderer != null ? renderer.sharedMaterials : new Material[0];
            var paints = new Paint[source.subMeshCount];
            bool allGlow = true, anyOpaque = false;
            for (int s = 0; s < paints.Length; s++)
            {
                // The recipe's material is the one the piece wears in its first slot (a re-colour swaps that slot only).
                string name = s == 0 ? group.Material : s < slots.Length && slots[s] != null ? slots[s].name : null;
                if (!TryPaint(name, out paints[s])) gathered.Grey.Add(name ?? "(none)");
                allGlow &= paints[s].Glow;
                anyOpaque |= paints[s].Colour.a >= 0.8f;
            }

            // A flat piece lying low is a floor mark; one held up (Logan Stone's arcs over the head, a leaf in the air)
            // stays a shape of its own, drawn from both sides.
            if (bounds.size.y < FlatHeight && bounds.max.y <= MarkHeight && !allGlow)
            {
                gathered.FlatPieces++;
                for (int s = 0; s < source.subMeshCount; s++)
                {
                    int colour = gathered.ColourFor(paints[s].Colour, bounds.min.y);
                    var tris = source.GetTriangles(s);
                    for (int t = 0; t + 2 < tris.Length; t += 3)
                        gathered.Flats.Add(new Flat { A = Ground(vertices[tris[t]]), B = Ground(vertices[tris[t + 1]]), C = Ground(vertices[tris[t + 2]]), Colour = colour });
                }
                return;
            }

            // A piece with nothing opaque (a gas layer held up off the floor) is seen through, as a glow is.
            bool seeThrough = allGlow || !anyOpaque;
            // Pieces that move alike go together: by what the piece is (its role) and the timeline's step for it.
            byte role = ReactionTimelines.RoleOf(group.KitObject);
            int step = ReactionTimelines.StepFor(combo.Pair, group.KitObject, role, out var follows);
            string sub = split && follows.Stagger > 0f ? group.KitObject : "";
            var merge = gathered.MergeFor(instance.OnGround ? ReactionArtSet.OnGround : ReactionArtSet.OnBody, seeThrough ? ReactionArtSet.Glow : ReactionArtSet.Solid,
                seeThrough ? "glow" : ElementOf(group.Material, combo.Pair), role, step, follows, sub);
            int first = merge.Vertices.Count;
            merge.Vertices.AddRange(vertices);
            for (int i = 0; i < vertices.Length; i++) merge.Normals.Add(i < normals.Length ? normals[i] : Vector3.up);
            var paint = new Color[vertices.Length];
            for (int i = 0; i < paint.Length; i++) paint[i] = Color.white;
            // A mirrored placing turns the faces inside out unless their corners are taken the other way round.
            bool mirrored = place.determinant < 0f;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var band = paints[s];
                // A see-through wash inside a solid piece cannot be drawn by the solid material; it is left out, as the element art does.
                if (!seeThrough && band.Colour.a < 0.8f) continue;
                var colour = band.Colour;
                colour.a = seeThrough ? band.Colour.a : band.Flag;
                var tris = source.GetTriangles(s);
                for (int t = 0; t + 2 < tris.Length; t += 3)
                {
                    paint[tris[t]] = paint[tris[t + 1]] = paint[tris[t + 2]] = colour;
                    merge.Triangles.Add(first + tris[t]);
                    merge.Triangles.Add(first + (mirrored ? tris[t + 2] : tris[t + 1]));
                    merge.Triangles.Add(first + (mirrored ? tris[t + 1] : tris[t + 2]));
                }
            }
            merge.Colours.AddRange(paint);
            merge.Pieces++;
        }

        private static Vector2 Ground(Vector3 v) => new Vector2(v.x, v.z);

        private static ReactionArtSet.Part MakePart(ReactionArtSet set, string pair, Merge merge, Material material, ReactionMotion.Cue cue)
        {
            string place = merge.Place == ReactionArtSet.OnBody ? "body" : "ground";
            string role = ReactionTimelines.RoleNames[merge.Role];
            // A step named for a word of the pieces' names ("stone", "thorn") is told apart from the role's own step.
            string sub = merge.Sub.Length > 0 ? "_" + ReactionTimelines.Plain(merge.Sub)
                : merge.Step >= 0 && merge.Follows.Match != role ? "_" + merge.Follows.Match : "";
            // The mesh is kept about the point it moves about, so scaling and turning it move it as its verb says.
            var pivot = PivotOf(cue.Verb, merge.Role, merge.Vertices, out var axis);
            var vertices = merge.Vertices.ToArray();
            for (int i = 0; i < vertices.Length; i++) vertices[i] -= pivot;
            var mesh = new Mesh { name = $"mesh_{pair}_{place}_{merge.Element}_{role}{sub}" };
            if (vertices.Length > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(merge.Normals);
            mesh.SetColors(merge.Colours);
            mesh.SetTriangles(merge.Triangles, 0);
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, merge.Normals.ToArray()));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, set);
            return new ReactionArtSet.Part
            {
                Name = $"{pair} {place} {merge.Element} {role}{sub.Replace('_', ' ')}", Kind = merge.Kind, Place = merge.Place, Role = merge.Role, Mesh = mesh, Material = material,
                Motion = cue, Pivot = pivot, Axis = axis,
                Centre = Vector3.zero, Size = mesh.bounds.size, Triangles = merge.Triangles.Count / 3, Pieces = merge.Pieces,
            };
        }

        // The flat pieces' colours, lowest first, four to a mask; each mask one quad over its own footprint.
        private static void MakeMasks(string pair, Gathered gathered, List<ReactionArtSet.Part> parts, List<(string, Texture2D, int, Color[])> masks, HashSet<string> written)
        {
            if (gathered.Flats.Count == 0) return;
            // The ground spreads by the look's step for its marks, the upper layer a moment after the lower.
            ReactionTimelines.StepFor(pair, "", ReactionTimelines.Mark, out var follows);
            var order = new List<int>();
            for (int i = 0; i < gathered.FlatColours.Count; i++) order.Add(i);
            order.Sort((x, y) => gathered.FlatHeights[x].CompareTo(gathered.FlatHeights[y]));
            for (int layer = 0; layer * ColoursPerMask < order.Count; layer++)
            {
                var channelOf = new Dictionary<int, int>();
                var colours = new List<Color>();
                for (int k = 0; k < ColoursPerMask && layer * ColoursPerMask + k < order.Count; k++)
                {
                    int colour = order[layer * ColoursPerMask + k];
                    channelOf[colour] = k;
                    colours.Add(gathered.FlatColours[colour]);
                }
                var flats = gathered.Flats.FindAll(flat => channelOf.ContainsKey(flat.Colour));
                Vector2 low = flats[0].A, high = flats[0].A;
                foreach (var flat in flats)
                {
                    low = Vector2.Min(low, Vector2.Min(flat.A, Vector2.Min(flat.B, flat.C)));
                    high = Vector2.Max(high, Vector2.Max(flat.A, Vector2.Max(flat.B, flat.C)));
                }
                Vector2 centre = (low + high) * 0.5f;
                // A little margin round the footprint, so its outer edge is never cut by the quad's.
                float sizeX = Mathf.Max(0.1f, (high.x - low.x) * 1.04f), sizeZ = Mathf.Max(0.1f, (high.y - low.y) * 1.04f);
                string name = $"{pair}_{layer}";
                masks.Add((name, Rasterise(flats, channelOf, centre, sizeX, sizeZ), parts.Count, colours.ToArray()));
                written.Add($"{Folder}/{name}_mask.png");
                written.Add($"{Folder}/m_reaction_decal_{name}.mat");
                parts.Add(new ReactionArtSet.Part
                {
                    Name = $"{pair} mark {layer}", Kind = ReactionArtSet.Decal, Place = ReactionArtSet.OnGround, Role = ReactionTimelines.Mark,
                    Motion = Layered(follows.Cue, layer),
                    Centre = new Vector3(centre.x, gathered.FlatHeights[order[layer * ColoursPerMask]], centre.y), Size = new Vector3(sizeX, 0f, sizeZ),
                    Triangles = 2, Pieces = gathered.FlatPieces,
                });
            }
        }

        private static ReactionMotion.Cue Layered(ReactionMotion.Cue cue, int layer)
        {
            cue.Delay += layer * 0.1f;
            cue.Flash = false;
            return cue;
        }

        /// <summary>
        /// Draws flat triangles from above into a mask, each colour into its own channel (red, green, blue, alpha), nine
        /// samples a pixel so a thin line keeps its width. u runs along +X and v along +Z over the footprint given.
        /// </summary>
        private static Texture2D Rasterise(List<Flat> flats, Dictionary<int, int> channelOf, Vector2 centre, float sizeX, float sizeZ)
        {
            int longest = Mathf.Max(sizeX, sizeZ) > 4.5f ? 1024 : 512;
            int width = sizeX >= sizeZ ? longest : Mathf.Max(64, Mathf.NextPowerOfTwo(Mathf.CeilToInt(longest * sizeX / sizeZ)));
            int height = sizeZ >= sizeX ? longest : Mathf.Max(64, Mathf.NextPowerOfTwo(Mathf.CeilToInt(longest * sizeZ / sizeX)));
            var cover = new float[4, width * height];
            float left = centre.x - sizeX * 0.5f, bottom = centre.y - sizeZ * 0.5f;
            const int samples = 3;
            foreach (var flat in flats)
            {
                int channel = channelOf[flat.Colour];
                Vector2 a = Pixel(flat.A), b = Pixel(flat.B), c = Pixel(flat.C);
                float area = Edge(a, b, c);
                if (Mathf.Abs(area) < 1e-6f) continue;
                int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
                int x1 = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
                int y1 = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        int inside = 0;
                        for (int sy = 0; sy < samples; sy++)
                            for (int sx = 0; sx < samples; sx++)
                            {
                                var p = new Vector2(x + (sx + 0.5f) / samples, y + (sy + 0.5f) / samples);
                                float w0 = Edge(b, c, p) / area, w1 = Edge(c, a, p) / area, w2 = Edge(a, b, p) / area;
                                if (w0 >= 0f && w1 >= 0f && w2 >= 0f) inside++;
                            }
                        if (inside == 0) continue;
                        int index = y * width + x;
                        cover[channel, index] = Mathf.Min(1f, cover[channel, index] + inside / (float)(samples * samples));
                    }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(cover[0, i], cover[1, i], cover[2, i], cover[3, i]);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;

            Vector2 Pixel(Vector2 v) => new Vector2((v.x - left) / sizeX * width, (v.y - bottom) / sizeZ * height);
        }

        private static float Edge(Vector2 a, Vector2 b, Vector2 p) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);

        private static Material DecalMaterial(string name, Texture2D mask, Color[] colours)
        {
            var material = BuildKit.MaterialAt(Folder, "m_reaction_decal_" + name, "Clover/Decal");
            material.SetTexture("_MaskTex", mask);
            string[] slots = { "_ColorR", "_ColorG", "_ColorB", "_ColorA" };
            for (int k = 0; k < slots.Length; k++)
            {
                var linear = k < colours.Length ? colours[k] : Color.clear;
                // Material colours are given as the eye sees them; the bands are linear.
                var shown = linear.gamma;
                shown.a = linear.a;
                material.SetColor(slots[k], shown);
            }
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Opacity", 1f);
            material.SetFloat("_Reveal", 1f);
            // A reaction's ground spreads out from the feet (ReactionArt drives _Reveal).
            material.SetFloat("_RevealMode", 2f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.renderQueue = (int)RenderQueue.Transparent - 20;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        // Masks and mark materials a former build made that this one did not: a combo that lost a layer, or left the recipe.
        private static void RemoveStale(HashSet<string> written)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) return;
            foreach (string guid in AssetDatabase.FindAssets("", new[] { Folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = Path.GetFileName(path);
                bool ours = (file.StartsWith("m_reaction_decal_") && file.EndsWith(".mat")) || file.EndsWith("_mask.png");
                if (ours && !written.Contains(path)) AssetDatabase.DeleteAsset(path);
            }
        }

        private static void ReportLook(ReactionRecipe.Combo combo, ReactionArtSet.Look look, List<ReactionArtSet.Part> parts, Gathered gathered, int lampsInRecipe, int lampsKept,
            float[] watts, int[] kept)
        {
            int solids = 0, glows = 0, marks = 0;
            for (int i = look.FirstPart; i < look.FirstPart + look.PartCount; i++)
            {
                if (parts[i].Kind == ReactionArtSet.Solid) solids++;
                else if (parts[i].Kind == ReactionArtSet.Glow) glows++;
                else marks++;
            }
            var notes = new List<string>();
            if (look.Triangles > TriangleBudget)
            {
                notes.Add($"OVER {TriangleBudget}");
                Debug.LogWarning($"[Clover.Build] Reaction art: {combo.Name} is {look.Triangles} triangles, over its budget of {TriangleBudget}.");
            }
            if (lampsInRecipe > lampsKept)
            {
                var dropped = new List<string>();
                for (int i = 0; i < combo.Lamps.Count; i++)
                    if (System.Array.IndexOf(kept, i, 0, lampsKept) < 0) dropped.Add($"{combo.Lamps[i].Object} {watts[i]:0} W");
                notes.Add("lamps left out: " + string.Join(", ", dropped));
            }
            if (gathered.Missing.Count > 0)
            {
                notes.Add("MISSING " + string.Join(", ", gathered.Missing));
                Debug.LogWarning($"[Clover.Build] Reaction art: {combo.Name} is missing {string.Join(", ", gathered.Missing)}.");
            }
            if (gathered.Grey.Count > 0)
            {
                notes.Add("grey (no paint): " + string.Join(", ", gathered.Grey));
                Debug.LogWarning($"[Clover.Build] Reaction art: {combo.Name} has no paint for {string.Join(", ", gathered.Grey)}; drawn grey.");
            }
            if (look.Directed) notes.Add("laid along the blow");
            if (look.PartCount > PartBudget) notes.Add($"OVER {PartBudget} parts");
            if (gathered.Folded.Count > 0) notes.Add("folded to fit: " + string.Join(", ", gathered.Folded));
            Report.AppendLine($"| {combo.Name} ({combo.Pair}) | {AfflictionPairs.Name(look.Pair)} | {look.Pieces} ({gathered.FlatPieces} flat) | {look.PartCount} ({solids}, {glows}, {marks}) | {look.Triangles} | {lampsKept} of {lampsInRecipe} | {look.ImpactAt:0.00} s, {look.Seconds:0.00} s | {string.Join("; ", notes)} |");

            Motion.AppendLine($"### {combo.Name} ({AfflictionPairs.Name(look.Pair)})\n\n{look.Story}.\n\n| Part | Verb | Beat | Seconds | Pieces | Triangles |\n|---|---|---|---|---|---|");
            for (int i = look.FirstPart; i < look.FirstPart + look.PartCount; i++)
            {
                var cue = parts[i].Motion;
                Motion.AppendLine($"| {parts[i].Name} | {ReactionMotion.VerbNames[cue.Verb]}{(cue.Strobes > 0 ? $" ({cue.Strobes} strobes)" : "")}{(cue.Flash ? ", flash" : "")} | " +
                    $"{ReactionMotion.BeatNames[cue.Beat]} | {cue.Delay:0.00} to {cue.End:0.00} | {parts[i].Pieces} | {parts[i].Triangles} |");
            }
            Motion.AppendLine();
        }
    }
}
