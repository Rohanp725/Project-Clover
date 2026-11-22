using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Pieces of a round-4 element look the player's own body draws, by "tweak name|kit object": Bedrock's plates (StoneArmour).</summary>
        public static readonly HashSet<string> RdBodyDrawn = new HashSet<string> { "rd_earth_bedrock|vfxrd_plate" };
    }
}
