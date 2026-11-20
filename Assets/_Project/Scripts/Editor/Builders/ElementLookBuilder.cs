using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the elements' second-pass looks (the Blender worker's batch 1, staging/blender/2026-11-20_elements) from
    /// their recipe (Art/VFX/Combos/elements2_recipe.json) into Generated/Vfx/Element/element_looks.asset, and hangs them
    /// on the element art so every scene that has the art plays them. Each piece the main body (or the player) wears
    /// becomes a part: its kit mesh baked in the colour it wears in the render (solid pieces inked in their element's
    /// material, flat ones see-through on the floor), its key pose, and its keyed path, scale, spin, orbit and window.
    /// The groups the recipe marks as shader mock-ups (Temper's steel, Chill's and Rime's ice) are not meshes: their
    /// bands' times are kept for the body's own material to draw.
    /// </summary>
    public static class ElementLookBuilder
    {
        public const string RecipePath = ReactionArtBuilder.CombosFolder + "/elements2_recipe.json";
        public const string SetPath = ElementArtBuilder.Folder + "/element_looks.asset";
        private const string Mockup = "unity_shader_mockup";
        private const string E2Kit = "vfx_elements_kit_e2.fbx";

        /// <summary>
        /// The colours of the batch's own materials (m3_*), linear: from notes.md where it gives them (the rust palette,
        /// the void purple), else matched by eye to the batch's renders. The rest are the earlier tables'.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> Paints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            ["m3_water_drop"] = new ReactionArtBuilder.Paint(0.1f, 0.62f, 0.78f),
            ["m3_steel"] = new ReactionArtBuilder.Paint(0.5f, 0.53f, 0.58f),
            ["m3_steel_dark"] = new ReactionArtBuilder.Paint(0.08f, 0.085f, 0.1f),
            ["m3_grip_leather"] = new ReactionArtBuilder.Paint(0.22f, 0.1f, 0.04f),
            ["m3_rust"] = new ReactionArtBuilder.Paint(0.58f, 0.24f, 0.07f),
            ["m3_rust_flake"] = new ReactionArtBuilder.Paint(0.76f, 0.40f, 0.13f),
            ["m3_rust_dark"] = new ReactionArtBuilder.Paint(0.24f, 0.09f, 0.035f),
            ["m3_miasma"] = new ReactionArtBuilder.Paint(0.42f, 0.6f, 0.08f),
            ["m3_fever"] = new ReactionArtBuilder.Paint(0.68f, 0.66f, 0.1f),
            ["m3_seed"] = new ReactionArtBuilder.Paint(0.45f, 0.3f, 0.12f),
            ["m3_spore"] = new ReactionArtBuilder.Paint(0.72f, 0.7f, 0.52f),
            ["m3_sprout"] = new ReactionArtBuilder.Paint(0.3f, 0.75f, 0.12f),
            ["m3_regrowth_green"] = new ReactionArtBuilder.Paint(0.18f, 0.6f, 0.12f),
            ["m3_regrowth_pink"] = new ReactionArtBuilder.Paint(0.9f, 0.3f, 0.55f),
            ["m3_dark_wisp"] = new ReactionArtBuilder.Paint(0.02f, 0.005f, 0.03f),
            ["m3_dark_voidpurple"] = new ReactionArtBuilder.Paint(0.018f, 0.004f, 0.028f),
            ["m3_rime"] = new ReactionArtBuilder.Paint(0.7f, 0.86f, 0.95f),
            ["m_vfx_combo_violetedge"] = new ReactionArtBuilder.Paint(0.28f, 0.08f, 0.75f),
            ["m_vfx_ice"] = new ReactionArtBuilder.Paint(0.55f, 0.85f, 1f),
        };

        /// <summary>
        /// Paints one look wears differently, by "slug|material", from the films of 2026-11-20: nothing white (the owner:
        /// "random white shapes ... look terrible"; the coordinator's check: no white rings or discs). Water's foam sheets
        /// and rings become see-through teal, Sundered's clang rings steel, Hunger's rim a deep violet, Fever's bile
        /// greener, Seeded's spores a dusty green.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> LookPaints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            ["water_soaked|m2_water_foam"] = new ReactionArtBuilder.Paint(0.04f, 0.4f, 0.5f, 0.45f, 1f, true),
            ["water_soaked|m_vfx_water_foam"] = new ReactionArtBuilder.Paint(0.08f, 0.42f, 0.52f, 0.8f, 1f, true),
            // The puddle under a soaked body: low and faint, so the enemy always reads clearly above it.
            ["water_soaked|m_vfx_water_ink"] = new ReactionArtBuilder.Paint(0.03f, 0.22f, 0.3f, 0.35f, 1f, true),
            // Wellspring's streams: see-through water, not solid blades.
            ["water_wellspring|m2_water_teal"] = new ReactionArtBuilder.Paint(0.06f, 0.48f, 0.58f, 0.7f, 1f, true),
            ["water_wellspring|m_vfx_water_foam"] = new ReactionArtBuilder.Paint(0.08f, 0.42f, 0.52f, 0.8f, 1f, true),
            ["water_wellspring|m2_water_foam"] = new ReactionArtBuilder.Paint(0.08f, 0.42f, 0.52f, 0.8f, 1f, true),
            ["metal_rust|m2_water_foam"] = new ReactionArtBuilder.Paint(0.04f, 0.4f, 0.5f, 0.45f, 1f, true),
            ["metal_sundered|m2_metal_arc"] = new ReactionArtBuilder.Paint(0.26f, 0.28f, 0.32f),
            ["dark_hunger|m_vfx_combo_violetedge"] = new ReactionArtBuilder.Paint(0.09f, 0.02f, 0.22f, 1f, 1f, true),
            ["poison_fever|m3_fever"] = new ReactionArtBuilder.Paint(0.4f, 0.52f, 0.06f),
            ["life_seeded|m3_spore"] = new ReactionArtBuilder.Paint(0.36f, 0.44f, 0.16f),
        };

        /// <summary>Pieces one look draws at another size, by "slug|kit object": Seeded's seeds were large for a castle soldier (notes.md).</summary>
        public static readonly Dictionary<string, float> LookScales = new Dictionary<string, float>
        {
            ["life_seeded|vfxe2_seed"] = 0.65f,
            // Soaked's drips while it lasts: small, so they read as drips off the body, not blobs beside it.
            ["water_soaked|vfx3c_vfx_water_drop"] = 0.6f,
        };

        /// <summary>
        /// Words of a piece's verb that make it repeat while its look is kept past its end: what happens again and again
        /// while an affliction lasts or a surge holds (drips, puffs, wisps, glints, mist, spores, petals, streaks).
        /// </summary>
        public static readonly string[] Repeating =
        {
            "while", "loop", "pop off", "peel off", "slide in along", "glint", "sink", "drift round", "drift outward",
            "drip", "drift up and away", "pour in", "sweep up",
        };

        /// <summary>Soaked: seconds the water takes to run down the body, and the most its puddle spans, metres.</summary>
        public const float WetRunsDown = 0.45f, SoakedPuddle = 1.15f;

        /// <summary>A piece thinner than this once placed, lying on the ground, is drawn flat and see-through on the floor.</summary>
        public const float FlatHeight = 0.06f;

        private static readonly StringBuilder Report = new StringBuilder();

        [MenuItem("Clover/Build/Element Looks")]
        public static void BuildMenu() => Build();

        public static string Build()
        {
            var recipe = AssetDatabase.LoadAssetAtPath<TextAsset>(RecipePath);
            if (recipe == null) return $"no recipe at {RecipePath}";
            var elementArt = ElementArtBuilder.Set();
            if (elementArt == null) return "no element art built";
            var glow = AssetDatabase.LoadAssetAtPath<Material>(ElementArtBuilder.Folder + "/m_element_glow.mat");
            var root = (Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text);
            var looks = (List<object>)root["looks"];

            var set = AssetDatabase.LoadAssetAtPath<ElementLookSet>(SetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ElementLookSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(SetPath))
                if (old is Mesh)
                {
                    AssetDatabase.RemoveObjectFromAsset(old);
                    Object.DestroyImmediate(old, true);
                }

            Report.Clear();
            var kits = new Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)>();
            var outLooks = new List<ElementLookSet.Look>();
            var outParts = new List<ElementLookSet.Part>();
            var outKeys = new List<ElementLookSet.Key>();
            var missing = new List<string>();
            foreach (Dictionary<string, object> look in looks)
            {
                string slug = Text(look, "slug");
                string element = Text(look, "element");
                float seconds = Number(look, "duration_s", 2f);
                var bodies = (List<object>)look["bodies"];
                var bodyAt = new List<Vector3>();
                foreach (Dictionary<string, object> body in bodies) bodyAt.Add(Vec(body["position"], Vector3.zero));
                var made = new ElementLookSet.Look { Key = slug, Seconds = seconds, FirstPart = outParts.Count, LoopFrom = seconds };
                var shellTimes = new List<(float start, float grow)>();
                foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                {
                    string kit = Text(group, "kit"), kitObject = Text(group, "kit_object"), material = Text(group, "material");
                    var instances = (List<object>)group["instances"];
                    var motions = (List<object>)group["motion"];
                    for (int n = 0; n < instances.Count; n++)
                    {
                        var instance = (List<object>)instances[n];
                        var motion = (Dictionary<string, object>)motions[n];
                        bool onPlayer = motion.TryGetValue("on_player", out var flag) && flag is bool yes && yes;
                        made.OnPlayer |= onPlayer;
                        Vector3 position = Vec(instance[1], Vector3.zero);
                        // The main body's pieces only (or the player's): the other two bodies show the same timeline again.
                        Vector3 judged = position;
                        if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbitAt && orbitAt.ContainsKey("centre"))
                            judged = Vec(orbitAt["centre"], position);
                        if (motion.TryGetValue("follow_body", out var follow) && follow is double followed && (int)followed != 0) continue;
                        // A group of one is a single piece (a rust segment, "rust1"): its number is not a body's.
                        if (!onPlayer && (instances.Count > 1 ? BodyOf(instance[0] as string, bodyAt, judged) : Nearest(bodyAt, judged)) != 0) continue;
                        var visible = (List<object>)motion["visible"];
                        float show = (float)(double)visible[0], hide = (float)(double)visible[1];
                        // Soaked (the coordinator's check of 2026-11-20: the sheets and puddles grew to 3 m and buried the
                        // group): the water running down the body is a wet sheen on the body itself, drawn by its own
                        // material from the head down; the foam ring stacks are left out.
                        if (slug == "water_soaked" && kitObject.Contains("wet_affliction"))
                        {
                            made.Shell = ElementLookSet.Wet;
                            shellTimes.Add((show, WetRunsDown));
                            continue;
                        }
                        if (slug == "water_soaked" && kitObject.Contains("water_ripple")) continue;
                        if (kit == Mockup)
                        {
                            // The shell's band: its window opens when the band snaps on; the scale keys say how long it grows.
                            float grow = 0.1f;
                            if (motion.TryGetValue("scale_keys", out var growKeys) && growKeys is List<object> keyList && keyList.Count >= 2)
                                grow = (float)((double)((List<object>)keyList[keyList.Count - 1])[0] - (double)((List<object>)keyList[0])[0]);
                            shellTimes.Add((show, grow));
                            made.Shell = slug.StartsWith("metal") ? ElementLookSet.Steel : slug.Contains("rime") ? ElementLookSet.Rime : ElementLookSet.Ice;
                            continue;
                        }
                        // Temper's climbing ring and clang are the shader's bright front on the body now (no rings round the player).
                        if (slug == "metal_temper" && (kitObject.Contains("metal_ring") || kitObject.Contains("clang"))) continue;
                        if (!TryKitPiece(kit, kitObject, kits, out var filter, out bool comboTurn))
                        {
                            missing.Add($"{slug}: {kitObject} ({kit})");
                            continue;
                        }
                        var rotation = Quaternion.Euler(Vec(instance[2], Vector3.zero));
                        var scale = Vec(instance[3], Vector3.one);
                        if (LookScales.TryGetValue(slug + "|" + kitObject, out float resize)) scale *= resize;
                        if (LookPaints.ContainsKey(slug + "|" + material)) material = slug + "|" + material;
                        string placement = instance.Count > 4 ? instance[4] as string : "b";
                        string verb = Text(motion, "verb");
                        var mesh = Bake(slug, outParts.Count - made.FirstPart, filter, comboTurn, material, element, scale, placement == "g", out byte kind, out Material wears, elementArt, glow);
                        var part = new ElementLookSet.Part
                        {
                            Mesh = mesh, Material = wears, Kind = kind,
                            Follows = placement != "g" || onPlayer,
                            Repeats = Repeating.Any(word => verb.Contains(word)) && hide < 9f,
                            Redraw = motion.ContainsKey("redraw") || verb.StartsWith("flicker"),
                            Position = position, Rotation = rotation, Scale = scale, Show = show, Hide = hide,
                        };
                        part.PathFirst = outKeys.Count;
                        part.PathCount = AddKeys(motion, "path", outKeys, false);
                        part.ScaleFirst = outKeys.Count;
                        part.ScaleCount = AddKeys(motion, "scale_keys", outKeys, false);
                        if (motion.TryGetValue("spin", out var spin) && spin is Dictionary<string, object> spinning)
                        {
                            part.SpinAxis = Vec(spinning["axis"], Vector3.up);
                            part.SpinFirst = outKeys.Count;
                            part.SpinCount = AddKeys(spinning, "keys", outKeys, true);
                        }
                        if (orbitValue is Dictionary<string, object> orbiting)
                        {
                            part.OrbitCentre = Vec(orbiting["centre"], Vector3.zero);
                            part.OrbitFirst = outKeys.Count;
                            part.OrbitCount = AddKeys(orbiting, "keys", outKeys, true);
                        }
                        if (part.Repeats) made.LoopFrom = Mathf.Min(made.LoopFrom, show);
                        // Soaked's puddle stays small and low round the feet, whatever size the render gave it.
                        if (slug == "water_soaked" && kind == ElementLookSet.Flat)
                        {
                            float grown = 1f;
                            for (int k = part.ScaleFirst; k < part.ScaleFirst + part.ScaleCount; k++)
                                grown = Mathf.Max(grown, Mathf.Max(outKeys[k].Value.x, outKeys[k].Value.z));
                            float across = Mathf.Max(mesh.bounds.size.x * part.Scale.x, mesh.bounds.size.z * part.Scale.z) * grown;
                            if (across > SoakedPuddle) part.Scale *= SoakedPuddle / across;
                        }
                        outParts.Add(part);
                        if (outParts.Count - made.FirstPart >= ElementLooks.MostParts) break;
                    }
                }
                made.PartCount = outParts.Count - made.FirstPart;
                if (shellTimes.Count > 0)
                {
                    shellTimes.Sort((a, b) => a.start.CompareTo(b.start));
                    made.ShellBands = shellTimes.Count;
                    made.ShellStart = shellTimes[0].start;
                    made.ShellStep = shellTimes.Count > 1 ? (shellTimes[shellTimes.Count - 1].start - shellTimes[0].start) / (shellTimes.Count - 1) : 0.1f;
                    made.ShellGrow = shellTimes[0].grow;
                }
                outLooks.Add(made);
                Report.AppendLine($"| {slug} | {made.Seconds:0.0} s | {(made.OnPlayer ? "player" : "body")} | {made.PartCount} | {made.LoopFrom:0.00} s | " +
                    $"{(made.Shell == ElementLookSet.NoShell ? "-" : $"{made.ShellBands} bands from {made.ShellStart:0.00} s, every {made.ShellStep:0.00} s, {made.ShellGrow:0.00} s each")} |");
            }
            set.Looks = outLooks.ToArray();
            set.Parts = outParts.ToArray();
            set.Keys = outKeys.ToArray();
            EditorUtility.SetDirty(set);
            elementArt.Looks = set;
            EditorUtility.SetDirty(elementArt);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("review/checks");
            File.WriteAllText("review/checks/element_looks.md",
                $"# Element looks (batch 1)\n\n{outLooks.Count} looks, {outParts.Count} parts, {outKeys.Count} keys, from {RecipePath}.\n\n" +
                "| Look | Length | On | Parts | Repeats from | Shell |\n|---|---|---|---|---|---|\n" + Report +
                (missing.Count > 0 ? "\n## Not found in the kits\n\n" + string.Join("\n", missing.Select(m => "- " + m)) + "\n" : ""));
            string line = $"[Clover.Build] Element looks: {outLooks.Count} looks, {outParts.Count} parts, {missing.Count} pieces missing; review/checks/element_looks.md";
            Debug.Log(line);
            return line;
        }

        // Which body a piece is on: its name says ("..._wisp0_3", "..._hole1": the number after the piece's word), else
        // the body it lies nearest.
        private static int BodyOf(string name, List<Vector3> bodies, Vector3 at)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var tokens = name.Split('_');
                for (int i = tokens.Length - 1; i >= 0; i--)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(tokens[i], "^[a-z]+([0-9]+)$");
                    if (match.Success) return int.Parse(match.Groups[1].Value);
                }
            }
            return Nearest(bodies, at);
        }

        private static int Nearest(List<Vector3> bodies, Vector3 at)
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < bodies.Count; i++)
            {
                float distance = new Vector2(bodies[i].x - at.x, bodies[i].z - at.z).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            return best;
        }

        // Where a kit object lives. The batch's own kit and the combo kits are imported with the axis turn baked; the
        // combos-3 kit is not in the project, and its pieces are copies of earlier kits' pieces, read from there.
        private static bool TryKitPiece(string kit, string kitObject, Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)> kits,
            out MeshFilter filter, out bool comboTurn)
        {
            filter = null;
            comboTurn = false;
            if (kit == "vfx_combo_kit_c3.fbx")
            {
                if (kitObject.StartsWith("vfxc3_v2_"))
                {
                    string rest = kitObject.Substring("vfxc3_v2_".Length);
                    string element = rest.Substring(0, rest.IndexOf('_'));
                    kit = $"vfx_{element}_kit_v2.fbx";
                    kitObject = "vfx2_" + rest;
                }
                else if (kitObject.StartsWith("vfxc3_vfx_"))
                {
                    kit = "vfx_combo_kit_v3.fbx";
                    kitObject = "vfx3c_vfx_" + kitObject.Substring("vfxc3_vfx_".Length);
                }
                else if (kitObject.StartsWith("vfxc3_c2_"))
                {
                    kit = "vfx_combo_kit_v2.fbx";
                    kitObject = "vfx2c_" + kitObject.Substring("vfxc3_c2_".Length);
                }
            }
            string path = kit == E2Kit ? ReactionArtBuilder.CombosFolder + "/" + E2Kit : ReactionArtBuilder.KitPath(kit);
            comboTurn = path.StartsWith(ReactionArtBuilder.CombosFolder);
            if (!kits.TryGetValue(path, out var loaded))
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var pieces = new Dictionary<string, MeshFilter>();
                if (asset != null)
                    foreach (var piece in asset.GetComponentsInChildren<MeshFilter>(true)) pieces[piece.name] = piece;
                loaded = (asset != null ? asset.transform : null, pieces);
                kits[path] = loaded;
            }
            if (loaded.root == null || !loaded.pieces.TryGetValue(kitObject, out filter) || filter.sharedMesh == null) return false;
            return true;
        }

        // The kit mesh in its own axes about its pivot, painted in what it wears: one colour for the whole piece (the
        // render re-colours shared meshes per piece), or a colour a slot for the batch's own many-slot pieces (the sword).
        private static Mesh Bake(string slug, int index, MeshFilter filter, bool comboTurn, string material, string element, Vector3 scale, bool onGround,
            out byte kind, out Material wears, ElementArtSet elementArt, Material glow)
        {
            var source = filter.sharedMesh;
            var kitRoot = filter.transform.root;
            var toKit = kitRoot.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            toKit.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            if (comboTurn) toKit = ReactionArtBuilder.ComboKitTurn * toKit;
            var normalTurn = toKit.inverse.transpose;
            var vertices = source.vertices;
            var normals = source.normals;
            // Wellspring's flow tongues ended in points that read like blades: their ends are rounded off.
            if (filter.name == "vfxe2_flow") RoundTip(vertices);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = toKit.MultiplyPoint3x4(vertices[i]);
                if (i < normals.Length) normals[i] = normalTurn.MultiplyVector(normals[i]).normalized;
            }
            var slots = filter.GetComponent<MeshRenderer>() != null ? filter.GetComponent<MeshRenderer>().sharedMaterials : new Material[0];
            var colours = new Color[vertices.Length];
            var triangles = new List<int>();
            bool perSlot = source.subMeshCount > 1 && material.Substring(material.IndexOf('|') + 1).StartsWith("m3_");
            var mainPaint = PaintOf(material);
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var paint = perSlot && s < slots.Length && slots[s] != null ? PaintOf(slots[s].name) : mainPaint;
                var subTriangles = source.GetTriangles(s);
                // A solid's alpha is Clover/Spell's flag (below a half is foam); a see-through piece's is how much shows.
                float alpha = mainPaint.Glow || (onGround && FlatAfter(source, toKit, scale)) ? paint.Colour.a : paint.Flag;
                foreach (int v in subTriangles) colours[v] = new Color(paint.Colour.r, paint.Colour.g, paint.Colour.b, alpha);
                triangles.AddRange(subTriangles);
            }
            var bounds = new Bounds(vertices.Length > 0 ? Vector3.Scale(vertices[0], scale) : Vector3.zero, Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(Vector3.Scale(v, scale));
            bool flat = (onGround && bounds.size.y < FlatHeight) || mainPaint.Glow;
            kind = flat ? ElementLookSet.Flat : ElementLookSet.Solid;
            if (flat) wears = glow;
            else
            {
                string owner = ReactionArtBuilder.ElementOf(material.Substring(material.IndexOf('|') + 1), element + "_" + element);
                int ring = ReactionDesigns.RingIndex(owner);
                wears = ring >= 0 && ring < elementArt.Solids.Length ? elementArt.Solids[ring] : elementArt.Solids[0];
            }
            var mesh = new Mesh { name = $"look_{slug}_{index:00}_{filter.name}" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, AssetDatabase.LoadAssetAtPath<ElementLookSet>(SetPath));
            return mesh;
        }

        /// <summary>
        /// Rounds a flat tongue's pointed end (in its own axes): the last two fifths of its length are drawn back into a
        /// half-ellipse as wide as the tongue is there, so it ends in a rounded lick of water, not a point.
        /// </summary>
        public static void RoundTip(Vector3[] vertices)
        {
            if (vertices.Length < 3) return;
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            var size = bounds.size;
            int along = size.x >= size.y && size.x >= size.z ? 0 : size.y >= size.z ? 1 : 2;
            int across = along == 0 ? (size.y >= size.z ? 1 : 2) : along == 1 ? (size.x >= size.z ? 0 : 2) : (size.x >= size.y ? 0 : 1);
            float low = bounds.min[along], length = Mathf.Max(0.0001f, size[along]), middle = bounds.center[across];
            // The tip is the narrower end.
            float nearLow = 0f, nearHigh = 0f;
            foreach (var v in vertices)
            {
                float s = (v[along] - low) / length;
                if (s < 0.2f) nearLow = Mathf.Max(nearLow, Mathf.Abs(v[across] - middle));
                if (s > 0.8f) nearHigh = Mathf.Max(nearHigh, Mathf.Abs(v[across] - middle));
            }
            bool tipHigh = nearHigh <= nearLow;
            const float From = 0.6f;
            float half = 0f;
            foreach (var v in vertices)
            {
                float s = (v[along] - low) / length;
                if (!tipHigh) s = 1f - s;
                if (Mathf.Abs(s - From) < 0.2f) half = Mathf.Max(half, Mathf.Abs(v[across] - middle));
            }
            if (half <= 0f) return;
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                float s = (v[along] - low) / length;
                if (!tipHigh) s = 1f - s;
                if (s <= From) continue;
                float u = (s - From) / (1f - From);
                // The end drawn back by a third, its width an ellipse's.
                float back = From + (s - From) * 0.65f;
                float width = half * Mathf.Sqrt(Mathf.Max(0f, 1f - u * u * 0.85f));
                float side = Mathf.Sign(v[across] - middle);
                if (Mathf.Abs(v[across] - middle) < 0.0001f) side = 0f;
                v[across] = middle + side * Mathf.Max(Mathf.Abs(v[across] - middle), width);
                v[along] = low + (tipHigh ? back : 1f - back) * length;
                vertices[i] = v;
            }
        }

        // True when the kit mesh, turned and scaled as placed, is flat enough to lie on the floor as a mark.
        private static bool FlatAfter(Mesh mesh, Matrix4x4 toKit, Vector3 scale)
        {
            float low = float.MaxValue, high = float.MinValue;
            foreach (var v in mesh.vertices)
            {
                float y = Vector3.Scale(toKit.MultiplyPoint3x4(v), scale).y;
                low = Mathf.Min(low, y);
                high = Mathf.Max(high, y);
            }
            return high - low < FlatHeight;
        }

        private static ReactionArtBuilder.Paint PaintOf(string material)
        {
            if (material != null && LookPaints.TryGetValue(material, out var paint)) return paint;
            if (material != null && Paints.TryGetValue(material, out paint)) return paint;
            if (ReactionArtBuilder.TryPaint(material, out paint)) return paint;
            return new ReactionArtBuilder.Paint(0.5f, 0.5f, 0.5f);
        }

        // A channel's keys: [[t, value, easing], ...]; a value is a vector, or degrees for a spin or an orbit.
        private static int AddKeys(Dictionary<string, object> from, string name, List<ElementLookSet.Key> keys, bool degrees)
        {
            if (!from.TryGetValue(name, out var value) || !(value is List<object> list)) return 0;
            int added = 0;
            foreach (var entry in list)
            {
                if (!(entry is List<object> key) || key.Count < 2) continue;
                float time = (float)(double)key[0];
                Vector3 v = degrees ? new Vector3(key[1] is double d ? (float)d : 0f, 0f, 0f) : Vec(key[1], Vector3.zero);
                keys.Add(new ElementLookSet.Key { Time = time, Value = v, Ease = EaseOf(key.Count > 2 ? key[2] as string : "linear") });
                added++;
            }
            return added;
        }

        private static byte EaseOf(string name)
        {
            switch (name)
            {
                case "out": return ElementLookSet.Out;
                case "in": return ElementLookSet.In;
                case "inout": return ElementLookSet.InOut;
                case "outback": return ElementLookSet.OutBack;
                case "step": return ElementLookSet.Step;
                default: return ElementLookSet.Linear;
            }
        }

        private static string Text(Dictionary<string, object> from, string key) => from.TryGetValue(key, out var value) && value is string text ? text : "";

        private static float Number(Dictionary<string, object> from, string key, float fallback)
        {
            if (!from.TryGetValue(key, out var value)) return fallback;
            if (value is double number) return (float)number;
            if (value is string text && float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)) return parsed;
            return fallback;
        }

        private static Vector3 Vec(object value, Vector3 fallback)
        {
            if (value is double number) return new Vector3((float)number, (float)number, (float)number);
            if (value is List<object> list && list.Count >= 3 && list[0] is double x && list[1] is double y && list[2] is double z)
                return new Vector3((float)x, (float)y, (float)z);
            return fallback;
        }

        /// <summary>The built looks; null before Clover > Build > Element Looks has run.</summary>
        public static ElementLookSet Set() => AssetDatabase.LoadAssetAtPath<ElementLookSet>(SetPath);
    }
}
