using System.Collections.Generic;
using System.Linq;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Blender worker's round-4 redesigns (staging/blender/2026-11-22_round4, the answer to the owner's fourth effects
    /// review, docs/design/feedback/2026-11-22_round4_plan.md section 2, redone after the owner's verdicts on round 1): twelve
    /// reactions in the reactions-5 schema (batches 1 and 2) and fourteen element looks in the elements-4 schema (batches 3
    /// and 4). Each look names the earlier one it replaces (revision_of) and is built under the key the game already plays,
    /// in place of that one. A redesign is built under its own tweak name ("rd_" and the game's key) so the earlier passes'
    /// fixes keyed by slug (scales, lifts, ribbon widths, shell timings) do not land on a look drawn anew; what the game
    /// does with a reaction (how long it holds, a body flame it keeps) is still read by the game's slug (<see cref="GameSlug"/>).
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public static readonly string[] RdRecipePaths =
        {
            ReactionArtBuilder.CombosFolder + "/round4_batch1_recipe.json", ReactionArtBuilder.CombosFolder + "/round4_batch2_recipe.json",
            ReactionArtBuilder.CombosFolder + "/round4_batch3_recipe.json", ReactionArtBuilder.CombosFolder + "/round4_batch4_recipe.json",
        };
        public const string RdKit = "vfx_round4_kit.fbx";

        /// <summary>The tweak name a redesign is built under: its game key behind this.</summary>
        public const string RdPrefix = "rd_";

        /// <summary>The game's slug or key behind a redesign's tweak name ("rd_rime" is "rime"); any other slug as it is.</summary>
        public static string GameSlug(string slug) => slug != null && slug.StartsWith(RdPrefix) ? slug.Substring(RdPrefix.Length) : slug;

        /// <summary>The twelve reactions built from round 4 (the game's slugs), in place of reactions 5's, elements 3's or combos 4's.</summary>
        public static readonly HashSet<string> RdReactions = new HashSet<string>
        {
            "rime", "quickening", "diamond_ring", "shut_in_the_dark", "carried_embers", "winter_seed",
            "smoulder", "thermal_shock", "logan_stone", "chokedamp", "breath_taken", "glint",
        };

        /// <summary>The element looks built from round 4, by the game's key: the round-4 slug drawn there.</summary>
        public static readonly Dictionary<string, string> RdLooks = new Dictionary<string, string>
        {
            ["lightning_static"] = "rd_static", ["lightning_overdrive"] = "rd_overdrive", ["water_wellspring"] = "rd_wellspring",
            ["life_regrowth"] = "rd_regrowth", ["light_clarity"] = "rd_clarity", ["poison_fever"] = "rd_fever", ["dark_hunger"] = "rd_hunger",
            ["light_marked"] = "rd_marked", ["earth_bedrock"] = "rd_bedrock", ["fire_kindle"] = "rd_kindle",
        };

        /// <summary>The element looks round 4 adds that no earlier recipe built: Kindle (the player catching fire).</summary>
        public static readonly string[] RdNewLooks = { "fire_kindle" };

        /// <summary>Each element's own moments round 4 redraws, by the moment's slug (built as "e3_" and the slug): the round-4 slug.</summary>
        public static readonly Dictionary<string, string> RdMoments = new Dictionary<string, string>
        {
            ["gust"] = "rd_gust", ["tailwind"] = "rd_tailwind", ["glare"] = "rd_glare", ["flung_blade"] = "rd_flung_blade",
        };

        /// <summary>True when the round-4 recipes are in the project.</summary>
        public static bool RedesignsIn() => AssetDatabase.LoadAssetAtPath<TextAsset>(RdRecipePaths[0]) != null;

        private static List<Dictionary<string, object>> RedesignLooks()
        {
            var looks = new List<Dictionary<string, object>>();
            foreach (string path in RdRecipePaths) looks.AddRange(RecipeLooks(path));
            return looks;
        }

        /// <summary>The round-4 reactions by the game's slug.</summary>
        private static Dictionary<string, Dictionary<string, object>> RedesignedReactions()
        {
            var found = new Dictionary<string, Dictionary<string, object>>();
            foreach (var look in RedesignLooks())
            {
                if (Text(look, "moment") != "reaction") continue;
                string game = GameSlug(Text(look, "slug"));
                if (RdReactions.Contains(game)) found[game] = look;
            }
            return found;
        }

        /// <summary>The round-4 element look drawn for the game's key, or null.</summary>
        private static Dictionary<string, object> RedesignFor(string key) =>
            RdLooks.TryGetValue(key, out var slug) ? RedesignLooks().FirstOrDefault(l => Text(l, "slug") == slug) : null;

        /// <summary>The round-4 moments, each renamed to the moment's slug, revision_of dropped (cut as elements 3's are).</summary>
        private static List<Dictionary<string, object>> RedesignedMoments()
        {
            var moments = new List<Dictionary<string, object>>();
            var looks = RedesignLooks();
            foreach (var pair in RdMoments)
            {
                var look = looks.FirstOrDefault(l => Text(l, "slug") == pair.Value);
                if (look == null) continue;
                var copy = new Dictionary<string, object>(look) { ["slug"] = pair.Key };
                copy.Remove("revision_of");
                moments.Add(copy);
            }
            return moments;
        }

        /// <summary>
        /// The round-4 materials, linear, as the specs give them (tools/b1.py to b4.py and rdcommon.py; a ramp's middle stop
        /// for the emission ramps). The see-through ones built by hand in the blend are drawn by their own materials
        /// (<see cref="RdSoftMaterials"/>); their paint here is the colour those carry.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> RdPaints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            // Batch 1.
            ["rd_seed_gold"] = new ReactionArtBuilder.Paint(0.55f, 0.33f, 0.09f),
            ["rd_lightchain"] = new ReactionArtBuilder.Paint(0.95f, 0.7f, 0.22f),
            ["rd_sleet"] = new ReactionArtBuilder.Paint(0.62f, 0.8f, 0.9f),
            ["rd_frostbead"] = new ReactionArtBuilder.Paint(0.7f, 0.84f, 0.94f),
            ["rd_petal_blue"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.86f),
            ["rd_petal_deep"] = new ReactionArtBuilder.Paint(0.05f, 0.16f, 0.55f),
            ["rd_flower_heart"] = new ReactionArtBuilder.Paint(0.9f, 0.72f, 0.25f),
            ["rd_stem"] = new ReactionArtBuilder.Paint(0.16f, 0.5f, 0.16f),
            ["rd_heal"] = new ReactionArtBuilder.Paint(0.3f, 0.8f, 0.35f),
            ["rd_poison_cloud"] = new ReactionArtBuilder.Paint(0.33f, 0.48f, 0.1f),
            ["rd_coldgust"] = new ReactionArtBuilder.Paint(0.3f, 0.6f, 0.9f),
            ["rd_coldgust_edge"] = new ReactionArtBuilder.Paint(0.6f, 0.82f, 0.95f),
            ["rd_heal_bright"] = new ReactionArtBuilder.Paint(0.3f, 0.92f, 0.36f),
            ["rd_tear"] = new ReactionArtBuilder.Paint(0.035f, 0.012f, 0.06f),
            ["rd_tear_gold"] = new ReactionArtBuilder.Paint(0.95f, 0.66f, 0.2f),
            ["rd_tear_amber"] = new ReactionArtBuilder.Paint(0.5f, 0.26f, 0.06f),
            ["rd_sickcloud_dark"] = new ReactionArtBuilder.Paint(0.1f, 0.2f, 0.05f),
            ["rd_sickcloud"] = new ReactionArtBuilder.Paint(0.2f, 0.36f, 0.08f),
            // Batch 2.
            ["rd_damp"] = new ReactionArtBuilder.Paint(0.07f, 0.1f, 0.06f, 0.6f, 1f, true),
            ["rd_cough"] = new ReactionArtBuilder.Paint(0.1f, 0.17f, 0.07f),
            // The breath: blue-grey, a little see-through so it reads as a wisp and never as a white stroke.
            ["rd_breath"] = new ReactionArtBuilder.Paint(0.62f, 0.68f, 0.78f, 0.8f, 1f, true),
            ["rd_goldsteel"] = new ReactionArtBuilder.Paint(0.98f, 0.7f, 0.28f),
            ["rd_flashgold"] = new ReactionArtBuilder.Paint(1f, 0.6f, 0.12f),
            ["rd_crackfire"] = new ReactionArtBuilder.Paint(0.98f, 0.45f, 0.08f),
            ["rd_violet_rim"] = new ReactionArtBuilder.Paint(0.42f, 0.16f, 0.72f),
            ["rd_blackwind"] = new ReactionArtBuilder.Paint(0.03f, 0.02f, 0.05f),
            ["rd_flame_black"] = new ReactionArtBuilder.Paint(0.03f, 0.012f, 0.045f),
            ["rd_flame_violet"] = new ReactionArtBuilder.Paint(0.4f, 0.1f, 0.75f),
            ["rd_flame_violet_deep"] = new ReactionArtBuilder.Paint(0.2f, 0.04f, 0.4f),
            // Batch 3.
            ["rd_bolt_cyan"] = new ReactionArtBuilder.Paint(0.3f, 0.86f, 1f),
            ["rd_bolt_violet"] = new ReactionArtBuilder.Paint(0.66f, 0.32f, 1f),
            ["rd_bolt_core"] = new ReactionArtBuilder.Paint(0.6f, 0.9f, 1f),
            ["rd_wind"] = new ReactionArtBuilder.Paint(0.42f, 0.78f, 0.72f),
            ["rd_wind_pale"] = new ReactionArtBuilder.Paint(0.68f, 0.86f, 0.8f),
            ["rd_dust"] = new ReactionArtBuilder.Paint(0.62f, 0.55f, 0.42f),
            ["rd_water"] = new ReactionArtBuilder.Paint(0.1f, 0.45f, 0.85f),
            ["rd_water_light"] = new ReactionArtBuilder.Paint(0.35f, 0.7f, 0.95f),
            ["rd_wet"] = new ReactionArtBuilder.Paint(0.05f, 0.09f, 0.14f, 0.6f, 1f, true),
            // Batch 4.
            ["rd_stone"] = new ReactionArtBuilder.Paint(0.52f, 0.42f, 0.31f),
            ["rd_stone_dark"] = new ReactionArtBuilder.Paint(0.3f, 0.23f, 0.17f),
            ["rd_stone_deep"] = new ReactionArtBuilder.Paint(0.19f, 0.14f, 0.1f),
            ["rd_grit"] = new ReactionArtBuilder.Paint(0.45f, 0.36f, 0.26f),
            ["rd_steelglint"] = new ReactionArtBuilder.Paint(0.62f, 0.74f, 0.9f),
            ["rd_steeltrail"] = new ReactionArtBuilder.Paint(0.48f, 0.58f, 0.72f),
            ["rd_spark"] = new ReactionArtBuilder.Paint(0.98f, 0.62f, 0.25f),
            ["rd_blossom"] = new ReactionArtBuilder.Paint(0.96f, 0.58f, 0.72f),
            ["rd_blossom_deep"] = new ReactionArtBuilder.Paint(0.82f, 0.36f, 0.52f),
            ["rd_leaf"] = new ReactionArtBuilder.Paint(0.24f, 0.62f, 0.22f),
            ["rd_leaf_deep"] = new ReactionArtBuilder.Paint(0.13f, 0.4f, 0.12f),
            ["rd_sigil_gold"] = new ReactionArtBuilder.Paint(0.98f, 0.72f, 0.22f),
            ["rd_brand_hot"] = new ReactionArtBuilder.Paint(0.95f, 0.35f, 0.05f),
            ["rd_lightrise"] = new ReactionArtBuilder.Paint(0.98f, 0.76f, 0.3f),
            ["rd_lightrise_amber"] = new ReactionArtBuilder.Paint(0.92f, 0.5f, 0.12f),
            ["rd_fever_sweat"] = new ReactionArtBuilder.Paint(0.05f, 0.09f, 0.04f),
            ["rd_fever_vein"] = new ReactionArtBuilder.Paint(0.04f, 0.12f, 0.04f),
            ["rd_fever_sheen"] = new ReactionArtBuilder.Paint(0.42f, 0.55f, 0.16f),
            ["rd_shimmer"] = new ReactionArtBuilder.Paint(0.5f, 0.55f, 0.36f),
            // Hunger's shadow: a near-black stain on the floor, mostly opaque.
            ["rd_shadow"] = new ReactionArtBuilder.Paint(0.02f, 0.01f, 0.03f, 0.85f, 1f, true),
            ["rd_fang"] = new ReactionArtBuilder.Paint(0.03f, 0.02f, 0.05f),
            // Built by hand in the blend: the fang's near-black (its violet rim is the solid's ink, RdInks), the fever
            // flush (the player's own material draws it, FeverFlush), and the soft cards' and the shimmer's tints.
            ["rd_fang_rim"] = new ReactionArtBuilder.Paint(0.02f, 0.012f, 0.03f),
            ["rd_fever_skin"] = new ReactionArtBuilder.Paint(0.3f, 0.42f, 0.15f, 0.76f, 1f, true),
            ["rd_fogcard"] = new ReactionArtBuilder.Paint(0.07f, 0.13f, 0.05f, 0.7f, 1f, true),
            ["rd_glowgold"] = new ReactionArtBuilder.Paint(1f, 0.62f, 0.16f, 0.55f, 1f, true),
            ["rd_heatshimmer"] = new ReactionArtBuilder.Paint(0.62f, 0.6f, 0.36f, 0.5f, 1f, true),
        };

        /// <summary>
        /// Solid round-4 pieces inked in their own colour, by material: Hunger's fangs are near black read by a violet rim
        /// where they turn away from the eye (the recipe's unity_note: "a fresnel rim on an unlit near-black material"); the
        /// toon solid's ink round the silhouette is that rim.
        /// </summary>
        public static readonly Dictionary<string, Color> RdInks = new Dictionary<string, Color>
        {
            ["rd_fang_rim"] = new Color(0.5f, 0.2f, 0.92f).gamma,
        };

        /// <summary>
        /// The see-through materials the Blender worker built by hand (their node trees do not ride the recipe), drawn by
        /// their own shaders: the soft fog card (Chokedamp's damp) and its gold copy (Clarity's glow on the motes) in
        /// Clover/SoftCard, a soft-edged disc fading with depth where it meets the floor or a body, the damp laid over what
        /// is behind it and the gold added; Fever's heat shimmer in Clover/HeatHaze, a wobbling refraction of the scene.
        /// </summary>
        public static readonly Dictionary<string, string> RdSoftMaterials = new Dictionary<string, string>
        {
            ["rd_fogcard"] = "m_element_fogcard", ["rd_damp"] = "m_element_fogcard", ["rd_glowgold"] = "m_element_glowcard", ["rd_heatshimmer"] = "m_element_heathaze",
        };

        /// <summary>
        /// Pieces of a round-4 element look not drawn about the player, by "tweak name|kit object": Bedrock's plates, which
        /// the player's own body grows (StoneArmour), and Hunger's shadow and jaws, drawn round each near enemy's real feet
        /// (<see cref="HungerJawsKey"/>) rather than where the stand-ins stood.
        /// </summary>
        public static readonly HashSet<string> RdBodyDrawn = new HashSet<string>
        {
            "rd_earth_bedrock|vfxrd_plate", "rd_dark_hunger|vfxrd_shadow", "rd_dark_hunger|vfxrd_fang",
        };

        /// <summary>
        /// The material a round-4 see-through piece is drawn in (<see cref="RdSoftMaterials"/>), made once a build; null for
        /// any other. The damp is laid over what is behind it, a little denser than the Blender worker's (the honest read:
        /// "faint at the game camera"); the gold glow is added; the shimmer bends the scene.
        /// </summary>
        private static Material RdSoftMaterial(string material)
        {
            if (material == null || !RdSoftMaterials.TryGetValue(material, out var name)) return null;
            bool haze = material == "rd_heatshimmer", added = material == "rd_glowgold";
            var made = BuildKit.MaterialAt(ElementArtBuilder.Folder, name, haze ? "Clover/HeatHaze" : "Clover/SoftCard");
            if (!haze)
            {
                made.SetFloat("_SrcBlend", added ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                made.SetFloat("_DstBlend", added ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                made.SetFloat("_Density", added ? 1f : 2.6f);
                made.SetFloat("_Softness", added ? 0.25f : 0.15f);
                made.SetFloat("_Breathe", added ? 0.2f : 0.45f);
            }
            made.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(made);
            return made;
        }

        /// <summary>A card's UVs, -1 to 1 across its two widest ways about its middle: what the soft card and the haze fade by.</summary>
        private static List<Vector2> CardUVs(Vector3[] vertices)
        {
            var uvs = new List<Vector2>(vertices.Length);
            if (vertices.Length == 0) return uvs;
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            var size = bounds.size;
            int thin = size.x <= size.y && size.x <= size.z ? 0 : size.y <= size.z ? 1 : 2;
            int a = thin == 0 ? 1 : 0, b = thin == 2 ? 1 : 2;
            // The haze sheet stands up: its rows climb along y.
            if (thin == 2) (a, b) = (0, 1);
            foreach (var v in vertices)
                uvs.Add(new Vector2((v[a] - bounds.center[a]) / Mathf.Max(0.0001f, size[a] * 0.5f), (v[b] - bounds.center[b]) / Mathf.Max(0.0001f, size[b] * 0.5f)));
            return uvs;
        }

        /// <summary>
        /// Round-4 parts on a body drawn this far toward the game camera, by "tweak name|material" (<see cref="RibbonFacing"/>):
        /// Static's crawling arcs and Marked's brand, its smoke and motes sat inside a castle soldier's armour at the slim
        /// stand-in's girth (the day stills of 2026-11-22: a cyan speck at the feet, no brand at all).
        /// </summary>
        public static readonly Dictionary<string, float> RdFacing = new Dictionary<string, float>
        {
            ["rd_lightning_static|rd_bolt_cyan"] = 0.35f, ["rd_lightning_static|rd_bolt_violet"] = 0.35f,
            ["rd_light_marked|rd_sigil_gold"] = 0.4f, ["rd_light_marked|rd_brand_hot"] = 0.4f,
            ["rd_light_marked|r4_smoke_soft"] = 0.35f, ["rd_light_marked|e3_gold_mote"] = 0.35f,
        };

        /// <summary>Flat round-4 pieces turned to face the recipe's camera, by "tweak name|kit object": Marked's brand on the chest.</summary>
        public static readonly HashSet<string> CameraFacedPieces = new HashSet<string> { "rd_light_marked|vfxrd_sigil" };

        /// <summary>
        /// The turn that lays a flat kit piece (its face up its own y once baked) square to the recipe's game camera: from the main body
        /// toward the recipe's player (the camera's side), 55 degrees up.
        /// </summary>
        private static Quaternion FacingTheCamera(Dictionary<string, object> look, List<Vector3> bodyAt)
        {
            Vector3 player = look.TryGetValue("player", out var value) && value is Dictionary<string, object> p ? Vec(p["position"], Vector3.zero) : new Vector3(1.93f, 0f, 2.3f);
            Vector3 toward = player - (bodyAt.Count > 0 ? bodyAt[0] : Vector3.zero);
            toward.y = 0f;
            toward = toward.sqrMagnitude > 0.0001f ? toward.normalized : Vector3.back;
            Vector3 normal = toward * Mathf.Cos(55f * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(55f * Mathf.Deg2Rad);
            // The baked piece lies flat in its own x and z, its face up its y.
            return Quaternion.FromToRotation(Vector3.up, normal);
        }

        /// <summary>Hunger's jaws about one near enemy: the shadow stretched to it and the two rows of fangs round its feet.</summary>
        public const string HungerJawsKey = "dark_hunger@b";
        private static readonly HashSet<string> JawPieces = new HashSet<string> { "vfxrd_shadow", "vfxrd_fang" };

        /// <summary>
        /// Hunger's jaws cut out of its look: the pieces about the recipe's main near enemy (body 0), moved into its frame.
        /// The recipe draws them about the player, the fangs round each near stand-in's feet and the shadow stretched from
        /// the player toward it; a fang is the main one's by where it stands, a shadow by the way it stretches. Their names
        /// carry "@b" so no check takes them for the player's look's pieces.
        /// </summary>
        private static void BuildHungerJaws(Lists lists, ElementArtSet elementArt, Material glow)
        {
            var look = RedesignFor("dark_hunger");
            if (look == null) return;
            var bodies = new List<Vector3>();
            foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodies.Add(Vec(body["position"], Vector3.zero));
            if (bodies.Count == 0) return;
            Vector3 player = Vec(((Dictionary<string, object>)look["player"])["position"], Vector3.zero);
            Vector3 main = bodies[0];
            var groups = new List<object>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (!JawPieces.Contains(Text(group, "kit_object"))) continue;
                bool shadow = Text(group, "kit_object") == "vfxrd_shadow";
                var instances = new List<object>();
                var motions = new List<object>();
                var groupInstances = (List<object>)group["instances"];
                var groupMotions = (List<object>)group["motion"];
                for (int n = 0; n < groupInstances.Count; n++)
                {
                    var instance = (List<object>)groupInstances[n];
                    Vector3 world = player + Vec(instance[1], Vector3.zero);
                    int nearest = 0;
                    float best = float.MinValue;
                    for (int b = 0; b < bodies.Count; b++)
                    {
                        // A shadow lies along its own +x from its back edge at the player: it is the body it points at.
                        Vector3 to = bodies[b] - world;
                        to.y = 0f;
                        float score = shadow ? Vector3.Dot(to.normalized, Quaternion.Euler(Vec(instance[2], Vector3.zero)) * Vector3.right) : -to.magnitude;
                        if (score > best)
                        {
                            best = score;
                            nearest = b;
                        }
                    }
                    if (nearest != 0) continue;
                    var copy = new List<object>(instance)
                    {
                        [0] = (instance[0] as string ?? "") + "@b",
                        [1] = ToList(world - main),
                    };
                    if (copy.Count > 4) copy[4] = shadow ? "g" : "b";
                    var motion = new Dictionary<string, object>((Dictionary<string, object>)groupMotions[n]);
                    motion.Remove("on_player");
                    motion["body"] = 0.0;
                    instances.Add(copy);
                    motions.Add(motion);
                }
                if (instances.Count > 0) groups.Add(new Dictionary<string, object>(group) { ["instances"] = instances, ["motion"] = motions });
            }
            var jaws = new Dictionary<string, object>(look)
            {
                ["groups"] = groups,
                ["ribbons"] = new List<object>(),
                ["bodies"] = new List<object> { new Dictionary<string, object> { ["position"] = ToList(Vector3.zero) } },
                ["player"] = new Dictionary<string, object> { ["position"] = ToList(player - main) },
            };
            lists.Flat = true;
            BuildLook(jaws, RdPrefix + HungerJawsKey, HungerJawsKey, 0, new List<Vector3> { Vector3.zero }, 0f, lists, elementArt, glow);
        }

        /// <summary>
        /// Bedrock's plate from round 4 (rdk_plate: an irregular seven-sided slab 15 cm thick, chamfered and domed, a dark
        /// stone top and deep-brown sides), laid with its thickness up and its base at its origin, each face painted by its
        /// slot, for StoneArmour to grow on the player's bones. False when the kit has no plate (elements 4's is used).
        /// </summary>
        private static bool BakeRdArmourPlate(ElementLookSet set, Lists lists, ElementArtSet elementArt)
        {
            if (!TryKitPiece(RdKit, "vfxrd_plate", lists.Kits, out var filter, out bool comboTurn)) return false;
            var vertices = KitVertices(filter, comboTurn);
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            // The slab's thinnest way is its thickness: turned to stand up along y.
            var size = bounds.size;
            int thin = size.x <= size.y && size.x <= size.z ? 0 : size.y <= size.z ? 1 : 2;
            var turn = thin == 0 ? Quaternion.Euler(0f, 0f, 90f) : thin == 2 ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
            for (int i = 0; i < vertices.Length; i++) vertices[i] = turn * (vertices[i] - bounds.center);
            var turned = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) turned.Encapsulate(v);
            var slots = filter.GetComponent<MeshRenderer>() != null ? filter.GetComponent<MeshRenderer>().sharedMaterials : new Material[0];
            var colours = new Color[vertices.Length];
            var triangles = new List<int>();
            var source = filter.sharedMesh;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var paint = PaintOf(s < slots.Length && slots[s] != null ? slots[s].name : "rd_stone_deep");
                foreach (int v in source.GetTriangles(s)) colours[v] = new Color(paint.Colour.r, paint.Colour.g, paint.Colour.b, 1f);
                triangles.AddRange(source.GetTriangles(s));
            }
            // Its base at its origin, so it grows out from where it is rooted.
            for (int i = 0; i < vertices.Length; i++) vertices[i].y -= turned.min.y;
            var mesh = new Mesh { name = "armour_plate_rd" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, mesh.normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, set);
            set.ArmourPlate = mesh;
            int ring = ReactionDesigns.RingIndex("earth");
            set.ArmourMaterial = ring >= 0 && ring < elementArt.Solids.Length ? ReactionSolid(elementArt.Solids[ring]) : null;
            return true;
        }
    }
}
