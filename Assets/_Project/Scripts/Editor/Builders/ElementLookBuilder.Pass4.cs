using System.Collections.Generic;
using System.Linq;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Blender worker's answer to the owner's third effects review (docs/design/feedback/2026-11-20_effects_review_3.md):
    /// elements 4 (staging/blender/2026-11-20_elements4: thirteen afflictions and surges, the earthen-armour Bedrock, four
    /// spells and strikes, six braziers) and reactions 5 (staging/blender/2026-11-20_reactions5: the 32 reactions the review
    /// asked to change, with the owner's picks of Thermal Shock B and Chokedamp A). Each look is built under the key the
    /// game already plays (or a new one for Marked, Glaze, Bedrock, Gust and Stone Rise) in place of the earlier pass's.
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public const string E4RecipePath = ReactionArtBuilder.CombosFolder + "/elements4_recipe.json";
        public const string R5RecipePath = ReactionArtBuilder.CombosFolder + "/combos5_recipe.json";
        public const string E4Kit = "vfx_elements_kit_e4.fbx", R5Kit = "vfx_combo_kit_r5.fbx";

        /// <summary>
        /// Reactions 5's slugs that are the game's under another name (Split Rock is the game's Root Snare, Windthrow its Seed
        /// Storm, Burning Sap its Sun-Sap), and the two the owner chose between options (2026-11-20, docs/GAME.md): Thermal
        /// Shock B, the flames freezing and shattering, and Chokedamp A, the damp breathing out of the cracks.
        /// </summary>
        public static readonly Dictionary<string, string> R5GameSlugs = new Dictionary<string, string>
        {
            ["split_rock"] = "root_snare", ["windthrow"] = "seed_storm", ["burning_sap"] = "sun_sap",
            ["thermal_shock_b"] = "thermal_shock", ["chokedamp_a"] = "chokedamp",
        };

        /// <summary>The options the owner did not pick: not built.</summary>
        public static readonly HashSet<string> R5NotPicked = new HashSet<string> { "thermal_shock_a", "chokedamp_b" };

        /// <summary>The reactions built from reactions 5 (the game's slugs), in place of combos 4's or elements 3's.</summary>
        public static readonly HashSet<string> Reactions5Built = new HashSet<string>
        {
            "rime", "carried_embers", "thunder_gust", "root_snare", "anvil_stone", "quickening", "winter_seed", "iron_thorns",
            "seed_storm", "sickle_wind", "golden_feathers", "the_grudge", "quicksilver", "fire_medicine", "diamond_ring",
            "shut_in_the_dark", "smoulder", "seized", "frost_heave", "cold_short", "caught_light", "girdled",
            "the_bolt_runs_true", "mock_suns", "capsize", "thermal_shock", "chokedamp", "sun_sap", "fish_poison", "ill_wind",
            "stolen_warmth", "glint",
        };

        /// <summary>
        /// The element looks built from elements 4, by the game's key: the earlier batches' looks they replace, and three
        /// new ones the game had drawn by its own pieces (Marked's sun-wheel, Glaze's crystals alone, Bedrock's stones).
        /// </summary>
        public static readonly Dictionary<string, string> E4Looks = new Dictionary<string, string>
        {
            ["lightning_static"] = "static", ["lightning_overdrive"] = "overdrive", ["water_wellspring"] = "wellspring",
            ["metal_sundered"] = "sundered", ["life_seeded"] = "seeded", ["life_regrowth"] = "regrowth",
            ["light_clarity"] = "clarity", ["dark_cursed"] = "cursed", ["dark_hunger"] = "hunger", ["poison_fever"] = "fever",
            // Glaze's elements 4 look (crystals at the feet and shoulders, a frost bed under the player) is not built: round 4
            // asked for the previous version, the ice grown on the player's own bones (GlazeIce) alone.
            ["light_marked"] = "marked", ["earth_bedrock"] = "bedrock_armour",
        };

        /// <summary>The element looks elements 4 adds that no earlier recipe had.</summary>
        public static readonly string[] E4NewLooks = { "light_marked", "earth_bedrock" };

        /// <summary>Each element's own moments elements 4 redraws (Tailwind, Broken Ground, Briar Patch) or adds (Gust, Stone Rise).</summary>
        public static readonly HashSet<string> E4Moments = new HashSet<string> { "tailwind", "gust", "stone_rise", "broken_ground", "briar_patch" };

        /// <summary>The braziers elements 4 redraws; the other five keep their loops.</summary>
        public static readonly HashSet<string> E4Braziers = new HashSet<string>
        {
            "brazier_water", "brazier_earth", "brazier_metal", "brazier_light", "brazier_dark", "brazier_lightning",
        };

        /// <summary>
        /// Pieces of an elements 4 look the player's own body draws, by "key|kit object": Bedrock's stone plates grow on the
        /// skeleton's bones (StoneArmour) and its seams glow in the stone skin, so they never float off a body posed another
        /// way than the stand-in; Hunger's dark torn off the stand-in enemies would hang where those stood.
        /// </summary>
        public static readonly HashSet<string> E4BodyDrawn = new HashSet<string>
        {
            "earth_bedrock|vfxe4_rock_a", "earth_bedrock|vfxe4_rock_b", "earth_bedrock|vfxe3_glint", "dark_hunger|vfxe2_wisp",
        };

        /// <summary>
        /// Reactions whose body is raised by its look (the model only, never the body that moves and is struck), by slug:
        /// Anvil Stone rides up 0.5 m on the anvil (the owner, 2026-11-20: "the anvil goes through the person instead of pushing
        /// them up"), Frost Heave's block of ice shoves it 1.3 m up ("instead of the ice shoving up the person from the
        /// ground"). The height is the recipe body's own path.
        /// </summary>
        public static readonly HashSet<string> RaisedBodies = new HashSet<string> { "anvil_stone", "frost_heave", "thunder_gust" };

        /// <summary>
        /// Affliction looks kept turned to the game camera as the Blender worker's camera saw them: Sundered's dagger in the
        /// back with its point out of the chest toward the camera (the owner, 2026-11-20). Kept as the recipe stood, its
        /// hilt was toward the camera and the dagger read as stuck in the front.
        /// </summary>
        public static readonly HashSet<string> CameraFacing = new HashSet<string> { "metal_sundered" };

        private static List<Dictionary<string, object>> RecipeLooks(string path)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (recipe == null) return new List<Dictionary<string, object>>();
            return ((List<object>)((Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text))["looks"]).Cast<Dictionary<string, object>>().ToList();
        }

        /// <summary>True when the reactions 5 recipe is in the project (the reactions it redraws are built from it).</summary>
        public static bool Reactions5In() => AssetDatabase.LoadAssetAtPath<TextAsset>(R5RecipePath) != null;

        /// <summary>The reactions 5 looks by the game's slug, the options not picked left out.</summary>
        private static Dictionary<string, Dictionary<string, object>> Reactions5()
        {
            var found = new Dictionary<string, Dictionary<string, object>>();
            foreach (var look in RecipeLooks(R5RecipePath))
            {
                string slug = Text(look, "slug");
                if (R5NotPicked.Contains(slug)) continue;
                string game = R5GameSlugs.TryGetValue(slug, out var named) ? named : slug;
                if (Reactions5Built.Contains(game)) found[game] = look;
            }
            return found;
        }

        /// <summary>The elements 4 look of that slug, or null.</summary>
        private static Dictionary<string, object> Elements4Look(string slug) =>
            RecipeLooks(E4RecipePath).FirstOrDefault(l => Text(l, "slug") == slug);

        /// <summary>The element look elements 4 draws for the game's key, or null for one it does not.</summary>
        private static Dictionary<string, object> Elements4For(string key) =>
            E4Looks.TryGetValue(key, out var slug) ? Elements4Look(slug) : null;

        /// <summary>
        /// The moments the elements 3 builder cuts up (<see cref="BuildElements3"/>): elements 3's own, with elements 4's
        /// redrawn and added ones in their place. Elements 4's carry revision_of; the cut is the same.
        /// </summary>
        private static List<Dictionary<string, object>> MomentLooks()
        {
            var looks = Elements3Looks().Where(l => !E4Moments.Contains(Text(l, "slug"))).ToList();
            foreach (var look in RecipeLooks(E4RecipePath))
            {
                if (!E4Moments.Contains(Text(look, "slug"))) continue;
                var copy = new Dictionary<string, object>(look);
                copy.Remove("revision_of");
                looks.Add(Round4Placement(copy, Text(copy, "slug")));
            }
            // The infused strikes redrawn from the owner's swing comments (ElementLookBuilder.Strikes), under their own keys.
            looks.AddRange(StrikeLooks());
            return looks;
        }

        // The keys of a raised body's height: the recipe body's own path, its height alone, played from the look's start.
        private static void AddRaise(Dictionary<string, object> look, string slug, int onBody, float shift, Lists lists, ref ElementLookSet.Look made)
        {
            if (!lists.Reactions || onBody != 0 || !RaisedBodies.Contains(slug)) return;
            if (!(look.TryGetValue("bodies", out var value) && value is List<object> bodies) || bodies.Count == 0) return;
            if (!(((Dictionary<string, object>)bodies[0]).TryGetValue("motion", out var motionValue) && motionValue is Dictionary<string, object> motion)) return;
            if (!(motion.TryGetValue("path", out var pathValue) && pathValue is List<object> path) || path.Count < 2) return;
            made.RaiseFirst = lists.Keys.Count;
            foreach (var entry in path)
            {
                if (!(entry is List<object> key) || key.Count < 2) continue;
                float height = Vec(key[1], Vector3.zero).y;
                lists.Keys.Add(new ElementLookSet.Key { Time = (float)(double)key[0] - shift, Value = new Vector3(0f, height, 0f), Ease = EaseOf(key.Count > 2 ? key[2] as string : "linear") });
                made.RaiseCount++;
            }
        }

        /// <summary>
        /// Bedrock's stone plate (elements 4's chunky faceted stone), stood on its base about its middle, painted the armour's
        /// toon stone, for StoneArmour to grow on the player's bones.
        /// </summary>
        private static void BakeArmourPlate(ElementLookSet set, Lists lists, ElementArtSet elementArt)
        {
            set.ArmourPlate = null;
            set.ArmourMaterial = null;
            if (!TryKitPiece(E4Kit, "vfxe4_rock_b", lists.Kits, out var filter, out bool comboTurn)) return;
            var vertices = KitVertices(filter, comboTurn);
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            var stone = PaintOf("e4_stone_armour").Colour;
            var colours = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                // Centred across, its base at its origin, so it grows out from where it is rooted.
                vertices[i] = new Vector3(vertices[i].x - bounds.center.x, vertices[i].y - bounds.min.y, vertices[i].z - bounds.center.z);
                colours[i] = new Color(stone.r, stone.g, stone.b, 1f);
            }
            var mesh = new Mesh { name = "armour_plate" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(filter.sharedMesh.triangles, 0);
            mesh.RecalculateNormals();
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, mesh.normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, set);
            set.ArmourPlate = mesh;
            int ring = ReactionDesigns.RingIndex("earth");
            set.ArmourMaterial = ring >= 0 && ring < elementArt.Solids.Length ? ReactionSolid(elementArt.Solids[ring]) : null;
        }
    }
}
