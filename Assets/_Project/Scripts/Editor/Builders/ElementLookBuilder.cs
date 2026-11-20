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
    /// Builds the elements' second-pass looks (the Blender worker's batches 1 and 2, staging/blender/2026-11-20_elements)
    /// from their recipe (Art/VFX/Combos/elements2_recipe.json) into Generated/Vfx/Element/element_looks.asset, and hangs
    /// them on the element art so every scene that has the art plays them. Each piece the main body (or the player) wears
    /// becomes a part: its kit mesh baked in the colour it wears in the render (solid pieces inked in their element's
    /// material, flat ones see-through on the floor), its key pose, and its keyed path, scale, spin, orbit and window.
    /// Each ribbon (wind's curls, lightning's arcs, the floor's combed lanes) becomes a tube along its points, its u
    /// running tail to head, drawn on and wiped off by its keys through Clover/Spell's _DrawHead and _DrawTail.
    /// The groups the recipe marks as shader mock-ups (Temper's steel, Chill's and Rime's ice) are not meshes: their
    /// bands' times are kept for the body's own material to draw.
    /// </summary>
    public static class ElementLookBuilder
    {
        public const string RecipePath = ReactionArtBuilder.CombosFolder + "/elements2_recipe.json";
        public const string SetPath = ElementArtBuilder.Folder + "/element_looks.asset";
        private const string Mockup = "unity_shader_mockup";
        private const string E2Kit = "vfx_elements_kit_e2.fbx";
        /// <summary>The Blender worker's eleven brazier loops (staging/blender/2026-11-20_braziers) and where they are built.</summary>
        public const string BrazierRecipePath = ReactionArtBuilder.CombosFolder + "/braziers_recipe.json";
        /// <summary>
        /// The Blender worker's bespoke reaction looks (staging/blender/2026-11-20_reactions4, combos 4), built into the same
        /// set as the element looks when the recipe is there: each reaction a look on the body it lands on (its slug), one on
        /// each other body it reaches ("slug@1", "slug@2"...) and one on the player ("slug@p").
        /// </summary>
        public const string ReactionRecipePath = ReactionArtBuilder.CombosFolder + "/combos4_recipe.json";

        /// <summary>The reactions built from combos 4: all 55, each in its own bespoke look (groups A to F of the recipe).</summary>
        public static readonly string[] ReactionsBuilt =
        {
            // Group A.
            "thunder_gust", "carried_embers", "supercooled", "quickening", "drowned_weight", "winter_seed", "lightning_flower", "seized", "rust", "smoulder",
            // Group B.
            "seed_storm", "red_hot", "breath_taken", "live_iron", "golden_feathers", "rime", "flashover", "kindling_point", "quicksilver",
            // Group C.
            "chokedamp", "rot_roots", "fire_medicine", "nerve_fire", "shut_in_the_dark", "sleet_charge", "capsize", "the_ground_gives", "steam",
            // Group D (the ten the owner praised; Root Snare is the game's Split Rock and Sun-Sap its Burning Sap).
            "turned_to_stone", "shining_herb", "ill_wind", "frost_heave", "earthlight", "black_chain", "venom_rime", "mock_suns", "root_snare", "sun_sap",
            // Group E.
            "cold_short", "sickle_wind", "anvil_stone", "glint", "diamond_ring", "iron_thorns", "thermal_shock", "the_grudge", "logan_stone",
            // Group F.
            "caught_light", "girdled", "the_bolt_runs_true", "firebloom", "earthed", "burnt_mound", "stolen_warmth", "fish_poison",
        };
        public const string BrazierSetPath = ElementArtBuilder.Folder + "/brazier_looks.asset";
        private const string BrazierKit = "vfx_brazier_kit.fbx";

        /// <summary>
        /// The colours of the batches' own materials (m3_*), linear: from notes.md where it gives them (the rust palette,
        /// the void purple, the water rings, the fire bands), else matched by eye to the batches' renders. The rest are
        /// the earlier tables'.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> Paints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            ["m3_water_drop"] = new ReactionArtBuilder.Paint(0.1f, 0.62f, 0.78f),
            ["m3_steel"] = new ReactionArtBuilder.Paint(0.5f, 0.53f, 0.58f),
            ["m3_steel_dark"] = new ReactionArtBuilder.Paint(0.08f, 0.085f, 0.1f),
            // Sundered's dagger (a brighter steel than the sword's, notes.md) and the dark wound it stands in; both were
            // drawn grey until 2026-11-20's rebuild listed them.
            ["m3_steel_dagger"] = new ReactionArtBuilder.Paint(0.6f, 0.63f, 0.68f),
            ["m3_wound"] = new ReactionArtBuilder.Paint(0.16f, 0.025f, 0.03f),
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
            // Batch 1's water rings, as notes.md gives them: deep blue and cyan, see-through on the floor, never white.
            ["m3_water_ring"] = new ReactionArtBuilder.Paint(0.03f, 0.32f, 0.55f, 0.7f, 1f, true),
            ["m3_water_rimlight"] = new ReactionArtBuilder.Paint(0.18f, 0.62f, 0.78f, 0.7f, 1f, true),
            // Batch 2. Lightning: cyan arcs (the ink is the violet fringe), a dark scorch where one lands.
            ["m3_bolt"] = new ReactionArtBuilder.Paint(0.3f, 0.8f, 1f),
            ["m3_scorch"] = new ReactionArtBuilder.Paint(0.035f, 0.03f, 0.05f, 0.55f, 1f, true),
            // Fire: the flame's own foot-to-tip bands are baked by height (FlameBands); this is its middle.
            ["m3_flame"] = new ReactionArtBuilder.Paint(1f, 0.2f, 0.013f),
            ["m3_flame_core"] = new ReactionArtBuilder.Paint(1f, 0.88f, 0.43f),
            ["m3_earth_dust"] = new ReactionArtBuilder.Paint(0.46f, 0.38f, 0.27f),
            // Light: the lotus of light, gold and a paler gold, lying see-through on the floor.
            ["m3_light_gold"] = new ReactionArtBuilder.Paint(1f, 0.6f, 0.1f, 0.8f, 1f, true),
            ["m3_light_pale"] = new ReactionArtBuilder.Paint(1f, 0.82f, 0.42f, 0.65f, 1f, true),
            // Wind: pale dust (its mint rim is the ink), never white: the wind solid's grey takes these to about 0.85 at most.
            ["m3_wind_ribbon"] = new ReactionArtBuilder.Paint(0.86f, 0.9f, 0.8f),
            ["m3_wind_dust"] = new ReactionArtBuilder.Paint(0.78f, 0.74f, 0.62f),
            ["m3_wind_comb"] = new ReactionArtBuilder.Paint(0.74f, 0.74f, 0.64f),
            ["m3_grass_gold"] = new ReactionArtBuilder.Paint(0.85f, 0.68f, 0.16f),
            ["m3_leaf_ochre"] = new ReactionArtBuilder.Paint(0.86f, 0.45f, 0.08f),
            // The braziers' own materials (r4_*), matched by eye to the Blender worker's renders (all_braziers.png); no white.
            ["r4_ember_bed"] = new ReactionArtBuilder.Paint(0.8f, 0.2f, 0.03f),
            // The ball a see-through violet-blue cage round a bright cyan heart, so the arcs inside it show.
            ["r4_bolt_ball"] = new ReactionArtBuilder.Paint(0.24f, 0.36f, 1f, 0.45f, 1f, true),
            ["r4_bolt_heart"] = new ReactionArtBuilder.Paint(0.12f, 0.85f, 1f),
            ["r4_wind_inner"] = new ReactionArtBuilder.Paint(0.36f, 0.62f, 0.56f),
            ["r4_frost_bed"] = new ReactionArtBuilder.Paint(0.45f, 0.68f, 0.95f),
            ["r4_ice_glint"] = new ReactionArtBuilder.Paint(0.55f, 0.82f, 1f),
            ["r4_cold_mist"] = new ReactionArtBuilder.Paint(0.45f, 0.65f, 0.9f, 0.4f, 1f, true),
            ["r4_water_surface"] = new ReactionArtBuilder.Paint(0.04f, 0.34f, 0.6f),
            ["r4_moss"] = new ReactionArtBuilder.Paint(0.18f, 0.42f, 0.1f),
            // Metal's melt read dark orange (notes.md): a hot liquid orange with the ingot hotter still.
            ["r4_molten"] = new ReactionArtBuilder.Paint(1f, 0.42f, 0.06f),
            ["r4_ingot"] = new ReactionArtBuilder.Paint(1f, 0.72f, 0.24f),
            ["r4_ingot_shine"] = new ReactionArtBuilder.Paint(0.95f, 0.85f, 0.62f),
            ["r4_light_orb"] = new ReactionArtBuilder.Paint(1f, 0.72f, 0.26f),
            // Light's rays read as a spiky sun (notes.md): soft see-through beams instead of solid blades.
            ["r4_light_ray"] = new ReactionArtBuilder.Paint(1f, 0.82f, 0.45f, 0.16f, 1f, true),
            ["r4_violet_mote"] = new ReactionArtBuilder.Paint(0.5f, 0.26f, 0.95f),
            ["r4_dark_rim"] = new ReactionArtBuilder.Paint(0.42f, 0.14f, 0.85f),
            ["r4_brew"] = new ReactionArtBuilder.Paint(0.28f, 0.52f, 0.1f),
            ["m_vfx_lightning_scorch"] = new ReactionArtBuilder.Paint(0.05f, 0.05f, 0.09f),
            // Combos 4, group A (Supercooled, Quickening, Drowned Weight, Winter Seed, Lightning Flower, Seized, Rust,
            // Smoulder), matched by eye to the Blender worker's strips (staging/blender/2026-11-20_reactions4/r4_*.png): water
            // a clear teal and blue, ice a clear blue (never white), the curse's water and flame near-black with a violet heart.
            ["r4_megadrop"] = new ReactionArtBuilder.Paint(0.1f, 0.62f, 0.78f),
            ["r4_splash"] = new ReactionArtBuilder.Paint(0.12f, 0.55f, 0.78f),
            ["r4_waterplant"] = new ReactionArtBuilder.Paint(0.12f, 0.52f, 0.95f),
            ["r4_waterpetal"] = new ReactionArtBuilder.Paint(0.18f, 0.5f, 1f),
            ["r4_heal_green"] = new ReactionArtBuilder.Paint(0.3f, 0.85f, 0.35f),
            ["r4_ice"] = new ReactionArtBuilder.Paint(0.25f, 0.55f, 1f),
            // Lying face up, the spell shader lights it a quarter brighter plus its shine: a deep blue so it lands a clear blue.
            ["r4_iceflower"] = new ReactionArtBuilder.Paint(0.1f, 0.3f, 0.8f),
            ["r4_frozen_pool"] = new ReactionArtBuilder.Paint(0.22f, 0.5f, 0.88f, 0.6f, 1f, true),
            ["r4_blackwater"] = new ReactionArtBuilder.Paint(0.02f, 0.03f, 0.07f),
            ["r4_blackwater_flat"] = new ReactionArtBuilder.Paint(0.01f, 0.02f, 0.05f, 0.75f, 1f, true),
            ["r4_boltpetal"] = new ReactionArtBuilder.Paint(0.38f, 0.38f, 1f),
            ["r4_boltstem"] = new ReactionArtBuilder.Paint(0.1f, 0.14f, 0.42f),
            // Seized's pool: a dark, see-through water (it read as a bright blue slab on the floor at 0.7).
            ["r4_live_water"] = new ReactionArtBuilder.Paint(0.015f, 0.08f, 0.15f, 0.42f, 1f, true),
            ["r4_rustwater"] = new ReactionArtBuilder.Paint(0.55f, 0.25f, 0.08f),
            ["r4_blackflame"] = new ReactionArtBuilder.Paint(0.035f, 0.01f, 0.05f),
            ["r4_blackflame_core"] = new ReactionArtBuilder.Paint(0.3f, 0.08f, 0.55f),
            ["r4_blackflake"] = new ReactionArtBuilder.Paint(0.06f, 0.02f, 0.08f),
            ["r4_coalglow"] = new ReactionArtBuilder.Paint(0.55f, 0.08f, 0.04f, 0.7f, 1f, true),
            // Combos 4, groups B and C and the redone Thunder Gust and Carried Embers, matched to the Blender worker's strips:
            // nothing white (steam and breath a pale grey-blue, white-hot a hot yellow), smoke and soot near black.
            ["r4_blackwind"] = new ReactionArtBuilder.Paint(0.07f, 0.05f, 0.1f),
            ["r4_bolt_bright"] = new ReactionArtBuilder.Paint(0.42f, 0.85f, 1f),
            ["r4_breath"] = new ReactionArtBuilder.Paint(0.58f, 0.63f, 0.7f, 0.6f, 1f, true),
            ["r4_char_lane"] = new ReactionArtBuilder.Paint(0.12f, 0.05f, 0.03f),
            ["r4_char_glow"] = new ReactionArtBuilder.Paint(0.85f, 0.26f, 0.05f),
            ["r4_cloud_burning"] = new ReactionArtBuilder.Paint(0.92f, 0.36f, 0.06f),
            ["r4_crack"] = new ReactionArtBuilder.Paint(0.08f, 0.06f, 0.05f, 0.85f, 1f, true),
            ["r4_damp_stain"] = new ReactionArtBuilder.Paint(0.14f, 0.24f, 0.06f, 0.6f, 1f, true),
            ["r4_dirt"] = new ReactionArtBuilder.Paint(0.36f, 0.24f, 0.13f),
            ["r4_feather_gold"] = new ReactionArtBuilder.Paint(0.95f, 0.66f, 0.16f),
            ["r4_feather_pale"] = new ReactionArtBuilder.Paint(0.86f, 0.76f, 0.5f),
            ["r4_fire_ribbon"] = new ReactionArtBuilder.Paint(1f, 0.42f, 0.05f),
            ["r4_fire_ribbon_red"] = new ReactionArtBuilder.Paint(0.85f, 0.12f, 0.03f),
            ["r4_frost_streak"] = new ReactionArtBuilder.Paint(0.3f, 0.6f, 1f),
            ["r4_jar"] = new ReactionArtBuilder.Paint(0.55f, 0.32f, 0.18f),
            ["r4_livesteel"] = new ReactionArtBuilder.Paint(0.45f, 0.6f, 0.75f),
            ["r4_mercury"] = new ReactionArtBuilder.Paint(0.62f, 0.65f, 0.7f),
            ["r4_mudwater"] = new ReactionArtBuilder.Paint(0.3f, 0.24f, 0.14f),
            ["r4_plasmacore"] = new ReactionArtBuilder.Paint(0.72f, 0.48f, 1f),
            ["r4_plasmaflame"] = new ReactionArtBuilder.Paint(0.55f, 0.2f, 1f),
            ["r4_redhot"] = new ReactionArtBuilder.Paint(1f, 0.32f, 0.05f),
            ["r4_root"] = new ReactionArtBuilder.Paint(0.32f, 0.2f, 0.1f),
            ["r4_rotroot"] = new ReactionArtBuilder.Paint(0.2f, 0.18f, 0.08f),
            ["r4_scorch_violet"] = new ReactionArtBuilder.Paint(0.12f, 0.04f, 0.18f, 0.6f, 1f, true),
            ["r4_smoke"] = new ReactionArtBuilder.Paint(0.16f, 0.14f, 0.14f),
            ["r4_smoke_soft"] = new ReactionArtBuilder.Paint(0.28f, 0.25f, 0.24f, 0.5f, 1f, true),
            ["r4_soot"] = new ReactionArtBuilder.Paint(0.05f, 0.04f, 0.04f),
            ["r4_spark_cyan"] = new ReactionArtBuilder.Paint(0.3f, 0.9f, 1f),
            ["r4_steam"] = new ReactionArtBuilder.Paint(0.58f, 0.64f, 0.72f, 0.5f, 1f, true),
            ["r4_storm_lit"] = new ReactionArtBuilder.Paint(0.4f, 0.45f, 1f),
            ["r4_stormband"] = new ReactionArtBuilder.Paint(0.45f, 0.5f, 0.64f),
            ["r4_stormband_dark"] = new ReactionArtBuilder.Paint(0.18f, 0.2f, 0.32f),
            ["r4_stormcloud"] = new ReactionArtBuilder.Paint(0.22f, 0.22f, 0.3f),
            ["r4_sunbead"] = new ReactionArtBuilder.Paint(1f, 0.74f, 0.24f),
            ["r4_venom_blade"] = new ReactionArtBuilder.Paint(0.4f, 0.75f, 0.15f),
            ["r4_wetsand"] = new ReactionArtBuilder.Paint(0.42f, 0.34f, 0.2f),
            ["r4_whirl_core"] = new ReactionArtBuilder.Paint(1f, 0.64f, 0.15f),
            ["r4_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.82f, 0.42f),
            // Combos 4, groups D, E and F and the polish pass's new materials, from the middle of each of the Blender
            // worker's ramps (tools/specs_a, b, d, e, f): light is gold, never white; the curse's chain, bolt, roots and
            // seams near-black with violet; frosted steel a pale steel blue; the floor's sap, slick and glow see-through.
            ["r4_blackbolt"] = new ReactionArtBuilder.Paint(0.09f, 0.035f, 0.16f),
            ["r4_chain"] = new ReactionArtBuilder.Paint(0.05f, 0.035f, 0.08f),
            ["r4_crackglow"] = new ReactionArtBuilder.Paint(1f, 0.45f, 0.1f, 0.8f, 1f, true),
            ["r4_dustband"] = new ReactionArtBuilder.Paint(0.55f, 0.55f, 0.5f),
            ["r4_feather_shaft"] = new ReactionArtBuilder.Paint(0.5f, 0.26f, 0.07f),
            ["r4_figroot"] = new ReactionArtBuilder.Paint(0.07f, 0.035f, 0.1f),
            ["r4_foulwater"] = new ReactionArtBuilder.Paint(0.15f, 0.35f, 0.12f),
            ["r4_frost_steel"] = new ReactionArtBuilder.Paint(0.36f, 0.5f, 0.68f),
            ["r4_grudgecloud"] = new ReactionArtBuilder.Paint(0.06f, 0.05f, 0.1f),
            ["r4_grudgelit"] = new ReactionArtBuilder.Paint(0.42f, 0.22f, 0.85f),
            ["r4_halo"] = new ReactionArtBuilder.Paint(1f, 0.72f, 0.3f, 0.6f, 1f, true),
            ["r4_hotstone"] = new ReactionArtBuilder.Paint(0.6f, 0.12f, 0.03f),
            ["r4_lightherb"] = new ReactionArtBuilder.Paint(1f, 0.76f, 0.34f),
            ["r4_lightpetal"] = new ReactionArtBuilder.Paint(1f, 0.74f, 0.34f),
            ["r4_lilypetal"] = new ReactionArtBuilder.Paint(1f, 0.5f, 0.1f),
            ["r4_lilystem"] = new ReactionArtBuilder.Paint(0.16f, 0.28f, 0.05f),
            ["r4_mound"] = new ReactionArtBuilder.Paint(0.22f, 0.15f, 0.1f),
            ["r4_net"] = new ReactionArtBuilder.Paint(1f, 0.74f, 0.32f),
            ["r4_net_cyan"] = new ReactionArtBuilder.Paint(0.3f, 0.75f, 0.92f),
            ["r4_poisonwind"] = new ReactionArtBuilder.Paint(0.4f, 0.66f, 0.18f),
            ["r4_sap"] = new ReactionArtBuilder.Paint(0.85f, 0.66f, 0.08f),
            ["r4_sap_edge"] = new ReactionArtBuilder.Paint(0.9f, 0.68f, 0.12f, 0.7f, 1f, true),
            ["r4_sap_pool"] = new ReactionArtBuilder.Paint(0.55f, 0.45f, 0.04f, 0.6f, 1f, true),
            ["r4_seam"] = new ReactionArtBuilder.Paint(0.06f, 0.02f, 0.1f),
            ["r4_slick"] = new ReactionArtBuilder.Paint(0.06f, 0.13f, 0.05f, 0.6f, 1f, true),
            ["r4_spark"] = new ReactionArtBuilder.Paint(1f, 0.6f, 0.18f),
            ["r4_stone"] = new ReactionArtBuilder.Paint(0.36f, 0.33f, 0.3f),
            ["r4_venom_frost"] = new ReactionArtBuilder.Paint(0.4f, 0.55f, 0.18f, 0.55f, 1f, true),
            ["r4_venom_ice"] = new ReactionArtBuilder.Paint(0.42f, 0.7f, 0.16f),
            ["r4_warmth"] = new ReactionArtBuilder.Paint(1f, 0.42f, 0.16f),
            ["r4_wetfilm"] = new ReactionArtBuilder.Paint(0.03f, 0.12f, 0.2f, 0.5f, 1f, true),
        };

        /// <summary>
        /// Paints one look wears differently, by "slug|material", from the films of 2026-11-20: nothing white (the owner:
        /// "random white shapes ... look terrible"; the coordinator's check: no white rings or discs). Water's foam sheets
        /// and rings become see-through teal, Sundered's clang rings steel, Hunger's rim a deep violet, Fever's bile
        /// greener, Seeded's spores a dusty green, Clarity's white motes pale gold.
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
            // Light's rays read as a spiky sun (notes.md): faint see-through beams (the gilded bed keeps its own gold).
            ["brazier_light|m3_light_gold"] = new ReactionArtBuilder.Paint(1f, 0.7f, 0.24f, 0.2f, 1f, true),
            // Life's spores a dusty green, not pale cream; Metal's melt a deep molten red-orange under a hotter ingot.
            ["brazier_life|m3_spore"] = new ReactionArtBuilder.Paint(0.5f, 0.6f, 0.22f),
            ["brazier_metal|r4_molten"] = new ReactionArtBuilder.Paint(0.72f, 0.1f, 0.02f),
            // Chill's crystals a deeper blue in their facets, so the ice is not near white (the coordinator, 2026-11-20).
            ["ice_chill|m_vfx_ice"] = new ReactionArtBuilder.Paint(0.15f, 0.45f, 1f),
            // Chill's frost rosettes on the floor, its glints and its mist: pale blue, not white (the snowflakes read white).
            ["ice_chill|m_vfx_ice_frost"] = new ReactionArtBuilder.Paint(0.22f, 0.5f, 0.9f, 0.6f, 1f, true),
            ["ice_chill|m2_light_white"] = new ReactionArtBuilder.Paint(0.45f, 0.7f, 1f),
            ["ice_chill|m_vfx_ice_fog"] = new ReactionArtBuilder.Paint(0.35f, 0.55f, 0.85f, 0.3f, 1f, true),
            // Winter Seed's and Supercooled's frost rosette on the floor: a see-through mid blue (it lay as a pale 5 m star).
            ["winter_seed|r4_frost_bed"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.8f, 0.45f, 1f, true),
            ["supercooled|r4_frost_bed"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.8f, 0.45f, 1f, true),
            ["brazier_ice|m_vfx_ice_frost"] = new ReactionArtBuilder.Paint(0.22f, 0.5f, 0.9f, 0.6f, 1f, true),
            ["brazier_ice|m_vfx_ice"] = new ReactionArtBuilder.Paint(0.15f, 0.45f, 1f),
            // Smoulder (the coordinator's check of its film, 2026-11-20): its flare at the end burned in plain fire colours
            // over the black; every flame of it is the curse's black flame, the flare too.
            ["smoulder|m3_flame"] = new ReactionArtBuilder.Paint(0.035f, 0.01f, 0.05f),
            ["smoulder|m3_flame_core"] = new ReactionArtBuilder.Paint(0.3f, 0.08f, 0.55f),
            ["smoulder|m_vfx_fire_ember"] = new ReactionArtBuilder.Paint(0.3f, 0.08f, 0.55f),
            ["light_clarity|m2_light_white"] = new ReactionArtBuilder.Paint(1f, 0.74f, 0.28f),
            // The films of the rebuild (2026-11-20): Wellspring's pool read pale grey (water, not foam: a deep see-through
            // teal), Hunger's burning crescent and bead white (a hot violet), Temper's glints white (a pale steel).
            ["water_wellspring|m_vfx_water_ink"] = new ReactionArtBuilder.Paint(0.03f, 0.3f, 0.42f, 0.6f, 1f, true),
            ["dark_hunger|m2_dark_rim"] = new ReactionArtBuilder.Paint(0.55f, 0.18f, 0.95f),
            ["metal_temper|m2_light_white"] = new ReactionArtBuilder.Paint(0.62f, 0.68f, 0.78f),
            ["light_clarity|m2_light_beam"] = new ReactionArtBuilder.Paint(1f, 0.78f, 0.36f, 0.55f, 1f, true),
            // Tremor on the game's darker floors: the cracks a deeper ochre and see-through, the dust a thin see-through puff
            // (the render's pale floor let them read lighter; on the Proving Ground they were bright tan starbursts).
            ["earth_tremor|m_vfx_earth_ochrecrack"] = new ReactionArtBuilder.Paint(0.3f, 0.16f, 0.05f, 0.8f, 1f, true),
            ["earth_tremor|m3_earth_dust"] = new ReactionArtBuilder.Paint(0.4f, 0.33f, 0.24f, 0.4f, 1f, true),
            // The films of groups D, E and F on the castle soldiers (2026-11-20), against "nothing pure white": Anvil Stone's
            // anvil read as a pale grey-white block (an iron-faced stone: a dark iron grey); Diamond Ring's corona and bead
            // as a white ring and star (gold, as light is); Mock Suns' two suns as white balls (a deep gold); Iron Thorns'
            // roots and thorns as white wires (dark iron); the white-hot sparks of Sickle Wind, Iron Thorns and Cold Short
            // as white specks (a hot orange); Sickle Wind's tornado as bright white crescents (a dusty grey, the mint rim
            // still on its curls); Earthlight's slabs as pale grey (earth); The Grudge's cloud, once lifted clear of the
            // health bar, as a pale lilac puff (near black, carried by the dark solid's violet ink, its inner light a
            // brighter violet); Fish Poison's and Caught Light's crowns as a
            // white ring at the feet (a deeper blue).
            ["anvil_stone|m_vfx_earth_rock"] = new ReactionArtBuilder.Paint(0.11f, 0.105f, 0.115f),
            ["diamond_ring|m2_light_pale"] = new ReactionArtBuilder.Paint(1f, 0.62f, 0.18f),
            ["diamond_ring|m2_light_white"] = new ReactionArtBuilder.Paint(1f, 0.7f, 0.26f),
            ["mock_suns|r4_sunbead"] = new ReactionArtBuilder.Paint(0.95f, 0.5f, 0.1f),
            ["iron_thorns|m2_metal_burr"] = new ReactionArtBuilder.Paint(0.17f, 0.16f, 0.17f),
            ["iron_thorns|m_vfx_metal_steel"] = new ReactionArtBuilder.Paint(0.26f, 0.27f, 0.3f),
            ["iron_thorns|m3_steel"] = new ReactionArtBuilder.Paint(0.3f, 0.32f, 0.36f),
            ["iron_thorns|m2_metal_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.5f, 0.12f),
            ["sickle_wind|m2_metal_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.5f, 0.12f),
            ["cold_short|m2_metal_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.5f, 0.12f),
            ["sickle_wind|m3_wind_ribbon"] = new ReactionArtBuilder.Paint(0.36f, 0.4f, 0.37f),
            ["sickle_wind|r4_wind_inner"] = new ReactionArtBuilder.Paint(0.27f, 0.3f, 0.28f),
            ["sickle_wind|m3_wind_dust"] = new ReactionArtBuilder.Paint(0.42f, 0.38f, 0.3f),
            ["earthlight|m_vfx_earth_rock"] = new ReactionArtBuilder.Paint(0.3f, 0.23f, 0.16f),
            ["the_grudge|r4_grudgecloud"] = new ReactionArtBuilder.Paint(0.07f, 0.05f, 0.12f),
            ["the_grudge|r4_grudgelit"] = new ReactionArtBuilder.Paint(0.55f, 0.3f, 1f),
            ["fish_poison|r4_splash"] = new ReactionArtBuilder.Paint(0.05f, 0.34f, 0.5f),
            ["caught_light|r4_splash"] = new ReactionArtBuilder.Paint(0.05f, 0.34f, 0.5f),
            // The Bolt Runs True's straight bolt drew as a pure white slab across the frame: a pale violet-blue bolt.
            ["the_bolt_runs_true|m3_bolt"] = new ReactionArtBuilder.Paint(0.42f, 0.5f, 1f),
        };

        /// <summary>
        /// Pieces one look lifts, by "slug|kit object", metres: The Grudge's cloud hung at 2.3 m, right behind the enemy's
        /// health bar, and was not seen at all from the game camera; it gathers higher, clear of the bar, and its bolts
        /// are stretched up to reach it (<see cref="RibbonLifts"/>).
        /// </summary>
        public static readonly Dictionary<string, float> LookLifts = new Dictionary<string, float>
        {
            ["the_grudge|vfxe2_cloud_a"] = 0.75f,
            ["the_grudge|vfxe2_cloud_b"] = 0.75f,
        };

        /// <summary>
        /// Ribbons one look stretches upward, by slug: a ribbon reaching above the first height has its points lifted by up
        /// to the second, nothing at its foot and all of it at its top, so a bolt still leaves the lifted cloud and still
        /// lands on the body.
        /// </summary>
        public static readonly Dictionary<string, Vector2> RibbonLifts = new Dictionary<string, Vector2>
        {
            ["the_grudge"] = new Vector2(1.9f, 0.75f),
        };

        /// <summary>Pieces one look draws at another size, by "slug|kit object": Seeded's seeds were large for a castle soldier (notes.md).</summary>
        public static readonly Dictionary<string, float> LookScales = new Dictionary<string, float>
        {
            ["life_seeded|vfxe2_seed"] = 0.65f,
            // Soaked's drips while it lasts: small, so they read as drips off the body, not blobs beside it.
            ["water_soaked|vfx3c_vfx_water_drop"] = 0.6f,
            // Unsteady's caught leaves were small at the game camera (notes.md): big enough to see as leaves.
            ["wind_unsteady|vfx3c_vfx_wind_leaf"] = 1.6f,
            ["wind_strike|vfx3c_vfx_earth_chip"] = 0.6f,
            // Light's rays shorter, so they are beams round the orb, not the points of a star.
            ["brazier_light|vfxr4_softray"] = 0.8f,
            // Quickening's water plants were faint beside the Seeded pods: larger, so the blue plants are what is seen.
            ["quickening|vfx3c_vfx_life_vine_a"] = 1.6f,
            ["winter_seed|vfx3c_vfx_ice_rosette"] = 0.5f,
            ["quickening|vfxr4_vfx_life_sapling"] = 1.5f,
            ["quickening|vfxr4_vfx_life_flower"] = 1.5f,
            // Tremor's cracks and dust were 2.3 m and 1.7 m across under one body: kept to about the body's own ground.
            ["earth_tremor|vfx3c_vfx_earth_crack2"] = 0.5f,
            ["earth_tremor|vfx3c_vfx_earth_dust"] = 0.5f,
            // The Grudge's cloud, lifted clear of the health bar, is made broader so it reads as a cloud over the enemy.
            ["the_grudge|vfxe2_cloud_a"] = 1.35f,
            ["the_grudge|vfxe2_cloud_b"] = 1.35f,
        };

        /// <summary>
        /// Ribbons one look draws wider, by "slug|material": Unsteady's curls were 13 cm at 12 m and read thin (notes.md),
        /// the wind strike's crescent and lanes likewise, and Overdrive's arcs were "thin at 12 m".
        /// </summary>
        public static readonly Dictionary<string, float> RibbonWidths = new Dictionary<string, float>
        {
            ["wind_unsteady|m3_wind_ribbon"] = 2f,
            ["wind_unsteady|m3_wind_dust"] = 1.8f,
            ["wind_strike|m3_wind_ribbon"] = 1.2f,
            ["wind_strike|m3_wind_comb"] = 2f,
            ["lightning_static|m3_bolt"] = 1.3f,
            ["lightning_overdrive|m3_bolt"] = 1.3f,
            // The brazier's crackling ball: its arcs thicker, so the storm reads at the game camera (notes.md: "the arcs ...
            // are thin at 12 m").
            ["brazier_lightning|m3_bolt"] = 1.5f,
        };

        /// <summary>A ribbon's ink by its material: wind's mint rim, lightning's violet fringe; the rest the element's own.</summary>
        public static readonly Dictionary<string, Color> RibbonInks = new Dictionary<string, Color>
        {
            ["m3_wind_ribbon"] = new Color(0.27f, 0.62f, 0.5f),
            ["m3_wind_dust"] = new Color(0.4f, 0.5f, 0.42f),
            ["m3_wind_comb"] = new Color(0.4f, 0.5f, 0.42f),
            ["m3_bolt"] = new Color(0.42f, 0.2f, 0.95f),
            // The curse's black chain, black bolt and fig roots vanished on the dark floors with a dark ink (Black Chain:
            // "I dont see the black chain at all", 2026-11-19): a violet rim carries them, as in the Blender worker's films.
            ["r4_chain"] = new Color(0.55f, 0.26f, 0.95f),
            ["r4_blackbolt"] = new Color(0.55f, 0.26f, 0.95f),
            ["r4_figroot"] = new Color(0.5f, 0.22f, 0.85f),
        };

        /// <summary>Ribbons of dust and combed floor are stippled thin (air, not a solid band).</summary>
        public static readonly Dictionary<string, float> RibbonThin = new Dictionary<string, float>
        {
            ["m3_wind_dust"] = 0.35f,
            ["m3_wind_comb"] = 0.3f,
        };

        /// <summary>
        /// Looks built once per body, as stacks: Unsteady's three bodies carry one, three and five stacks (one curl and
        /// one leaf; two curls and three; three curls faster and five), so each becomes a look of its own, moved onto its
        /// body and back to its own start. The first stays under the recipe's slug.
        /// </summary>
        public static readonly Dictionary<string, string[]> PerBody = new Dictionary<string, string[]>
        {
            ["wind_unsteady"] = new[] { "wind_unsteady", "wind_unsteady_1", "wind_unsteady_5" },
        };

        /// <summary>
        /// Words of a piece's verb that make it repeat while its look is kept past its end: what happens again and again
        /// while an affliction lasts or a surge holds (drips, puffs, wisps, glints, mist, spores, petals, streaks, dust
        /// and pebbles at each jolt, licks and embers off a flame, scorches where an arc lands).
        /// </summary>
        public static readonly string[] Repeating =
        {
            "while", "loop", "pop off", "peel off", "slide in along", "glint", "sink", "drift round", "drift outward",
            "drip", "drift up and away", "pour in", "sweep up", "kick up", "hop up", "tear off", "rise (embers)",
            "rise off the lotus", "mark the floor where", "scorch the floor", "buzz",
        };

        /// <summary>Words of a held piece's verb that keep its scale and path going round (a flame, a churning cloud, a bobbing spore, an orbiting leaf).</summary>
        public static readonly string[] CyclingWords = { "flicker", "churn", "bobbing", "fly round", "orbit and tumble", "crawl" };

        /// <summary>Words of a held piece's verb that keep its spin and orbit turning on (the above, a lotus, Hunger's pouring lines).</summary>
        public static readonly string[] TurningWords =
        {
            "flicker", "churn", "bobbing", "fly round", "orbit and tumble", "turn the other way", "then glow", "open inside it", "pour in, turning", "fall in (black motes)",
        };

        /// <summary>
        /// Wind's caught leaves read as gold diamonds (the coordinator, 2026-11-20): the kit's leaf is drawn as a leaf, a
        /// pointed oval with a darker midrib and a fold along it, in a muted ochre or olive by the paint it wore.
        /// </summary>
        public const string LeafPiece = "vfx3c_vfx_wind_leaf";
        public static readonly Dictionary<string, Color> LeafColours = new Dictionary<string, Color>
        {
            ["m3_grass_gold"] = new Color(0.56f, 0.6f, 0.2f),
            ["m3_leaf_ochre"] = new Color(0.78f, 0.5f, 0.15f),
        };

        /// <summary>Glaze's crystal (the elements2 kit's hexagonal crystal), kept in the built looks for the player's ice.</summary>
        public const string CrystalPiece = "vfxe2_crystal";

        /// <summary>The brazier kit's bed disc (water, frost, moss, melt, brew...), which every brazier's look lays on its coals.</summary>
        public const string BedPiece = "vfxr4_disc";
        public static readonly ReactionArtBuilder.Paint GlazeCrystalPaint = new ReactionArtBuilder.Paint(0.15f, 0.45f, 1f);

        /// <summary>
        /// Materials drawn in another element's solid than their name says: Winter Seed's ice flower lies flat, and the ice
        /// solid lights faces turned up white (its glint on the tops), so it is drawn in the water solid's flatter shine.
        /// </summary>
        public static readonly Dictionary<string, string> SolidOf = new Dictionary<string, string>
        {
            ["r4_iceflower"] = "water",
            // Winter Seed's thorns and Supercooled's ice: the ice solid prints every face turned straight up pure white
            // (its glint), which made the thorns a white star on the floor.
            ["r4_ice"] = "water",
            // The curse's black pieces (Black Chain's chain, The Grudge's cloud, Girdled's fig roots) are drawn in the dark
            // solid for its violet ink: in the metal or lightning solid's near-black ink they were lost on the dark floors.
            ["r4_chain"] = "dark",
            ["r4_grudgecloud"] = "dark",
            ["r4_grudgelit"] = "dark",
            ["r4_figroot"] = "dark",
            // Venom Rime's crystals in the water solid's flatter shine too, as r4_ice is, so no face of them lights white.
            ["r4_venom_ice"] = "water",
            // Mock Suns' two suns and its light lanes: in the ice solid (the look's first element) the suns' faces lit
            // pure white; gold light is drawn in fire's solid, which keeps its colour.
            ["r4_sunbead"] = "fire",
            ["r4_halo"] = "fire",
        };

        /// <summary>Soaked: seconds the water takes to run down the body, and the most its puddle spans, metres.</summary>
        public const float WetRunsDown = 0.45f, SoakedPuddle = 1.15f;

        /// <summary>A piece thinner than this once placed, lying on the ground, is drawn flat and see-through on the floor.</summary>
        public const float FlatHeight = 0.06f;

        /// <summary>Sides round a ribbon's tube, and the pieces each span of a smooth ribbon is cut into.</summary>
        public const int RibbonSides = 6, RibbonSteps = 4;

        /// <summary>
        /// A flame's bands from its foot to its tip, hottest at the foot (notes.md, linear from FFF2B0, FFC93C, FF7A1E and
        /// D8401A), and the share of its height where each next band begins.
        /// </summary>
        public static readonly Color[] FlameBands =
        {
            new Color(1f, 0.888f, 0.434f), new Color(1f, 0.584f, 0.045f), new Color(1f, 0.195f, 0.013f), new Color(0.687f, 0.051f, 0.01f),
        };
        public static readonly float[] FlameBandFrom = { 0f, 0.24f, 0.5f, 0.78f };

        private static readonly StringBuilder Report = new StringBuilder();
        private static readonly HashSet<string> Unknown = new HashSet<string>();

        [MenuItem("Clover/Build/Element Looks")]
        public static void BuildMenu() => Build();

        [MenuItem("Clover/Build/Brazier Looks")]
        public static void BuildBraziersMenu() => BuildBraziers();

        public static string Build() => BuildSet(RecipePath, SetPath, "review/checks/element_looks.md", "Element looks (batches 1 and 2)", true);

        /// <summary>The key of a reaction's look on another body (n of the recipe's bodies), or on the player (n below nought).</summary>
        public static string ReactionKey(string slug, int body) => body == 0 ? slug : body < 0 ? slug + "@p" : slug + "@" + body;

        /// <summary>The eleven brazier loops (the recipe's lineup, all eleven together, is only a picture and is left out).</summary>
        public static string BuildBraziers() => BuildSet(BrazierRecipePath, BrazierSetPath, "review/checks/brazier_looks.md", "Brazier looks", false);

        private class Lists
        {
            public readonly List<ElementLookSet.Look> Looks = new List<ElementLookSet.Look>();
            public readonly List<ElementLookSet.Part> Parts = new List<ElementLookSet.Part>();
            public readonly List<ElementLookSet.Key> Keys = new List<ElementLookSet.Key>();
            public readonly List<string> Missing = new List<string>();
            public readonly Dictionary<string, (Transform root, Dictionary<string, MeshFilter> pieces)> Kits = new Dictionary<string, (Transform, Dictionary<string, MeshFilter>)>();
            public int Ribbons;
            public ElementLookSet Target;
            /// <summary>Building reactions: pieces split by the body they are on, the player's apart.</summary>
            public bool Reactions;
        }

        private static string BuildSet(string recipePath, string setPath, string reportPath, string title, bool elementLooks)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<TextAsset>(recipePath);
            if (recipe == null) return $"no recipe at {recipePath}";
            var elementArt = ElementArtBuilder.Set();
            if (elementArt == null) return "no element art built";
            var glow = AssetDatabase.LoadAssetAtPath<Material>(ElementArtBuilder.Folder + "/m_element_glow.mat");
            var root = (Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text);
            var looks = (List<object>)root["looks"];

            var set = AssetDatabase.LoadAssetAtPath<ElementLookSet>(setPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ElementLookSet>();
                AssetDatabase.CreateAsset(set, setPath);
            }
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(setPath))
                if (old is Mesh)
                {
                    AssetDatabase.RemoveObjectFromAsset(old);
                    Object.DestroyImmediate(old, true);
                }

            Report.Clear();
            Unknown.Clear();
            var lists = new Lists { Target = set };
            // The reactions' looks join the element looks, played by the same pool.
            if (elementLooks)
            {
                var reactions = AssetDatabase.LoadAssetAtPath<TextAsset>(ReactionRecipePath);
                if (reactions != null)
                {
                    lists.Reactions = true;
                    foreach (Dictionary<string, object> look in (List<object>)((Dictionary<string, object>)ReactionRecipe.ReadJson(reactions.text))["looks"])
                    {
                        string slug = Text(look, "slug");
                        if (!ReactionsBuilt.Contains(slug)) continue;
                        var bodyAt = new List<Vector3>();
                        foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodyAt.Add(Vec(body["position"], Vector3.zero));
                        for (int b = -1; b < Mathf.Max(1, bodyAt.Count); b++)
                        {
                            int before = lists.Looks.Count;
                            BuildLook(look, slug, ReactionKey(slug, b), b, bodyAt, 0f, lists, elementArt, glow);
                            // A body (or the player) with nothing of the look on it has no look of its own.
                            if (b != 0 && lists.Looks.Count > before && lists.Looks[lists.Looks.Count - 1].PartCount == 0) lists.Looks.RemoveAt(lists.Looks.Count - 1);
                        }
                    }
                    lists.Reactions = false;
                }
            }
            foreach (Dictionary<string, object> look in looks)
            {
                string slug = Text(look, "slug");
                if (slug == "brazier_lineup") continue;
                var bodies = (List<object>)look["bodies"];
                var bodyAt = new List<Vector3>();
                foreach (Dictionary<string, object> body in bodies) bodyAt.Add(Vec(body["position"], Vector3.zero));
                if (PerBody.TryGetValue(slug, out var variants))
                {
                    float firstOn = Onset(look, 0);
                    for (int b = 0; b < variants.Length && b < bodyAt.Count; b++)
                        BuildLook(look, slug, variants[b], b, bodyAt, Mathf.Max(0f, Onset(look, b) - firstOn), lists, elementArt, glow);
                }
                else BuildLook(look, slug, slug, 0, bodyAt, 0f, lists, elementArt, glow);
            }
            set.Looks = lists.Looks.ToArray();
            set.Parts = lists.Parts.ToArray();
            set.Keys = lists.Keys.ToArray();
            if (elementLooks) BakeCrystal(set, lists, elementArt);
            EditorUtility.SetDirty(set);
            if (elementLooks)
            {
                elementArt.Looks = set;
                EditorUtility.SetDirty(elementArt);
            }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("review/checks");
            File.WriteAllText(reportPath,
                $"# {title}\n\n{lists.Looks.Count} looks, {lists.Parts.Count} parts ({lists.Ribbons} ribbons), {lists.Keys.Count} keys, from {recipePath}.\n\n" +
                "| Look | Length | On | Parts | Ribbons | Repeats from | Shell |\n|---|---|---|---|---|---|---|\n" + Report +
                (lists.Missing.Count > 0 ? "\n## Not found in the kits\n\n" + string.Join("\n", lists.Missing.Select(m => "- " + m)) + "\n" : "") +
                (Unknown.Count > 0 ? "\n## Materials with no paint (drawn grey)\n\n" + string.Join("\n", Unknown.OrderBy(m => m).Select(m => "- " + m)) + "\n" : ""));
            string line = $"[Clover.Build] {title}: {lists.Looks.Count} looks, {lists.Parts.Count} parts, {lists.Ribbons} ribbons, {lists.Missing.Count} pieces missing, {Unknown.Count} unpainted; {reportPath}";
            Debug.Log(line);
            return line;
        }

        // When a body's part of a look begins: its first ribbon or piece seen.
        private static float Onset(Dictionary<string, object> look, int body)
        {
            float first = float.MaxValue;
            if (look.TryGetValue("ribbons", out var value) && value is List<object> ribbons)
                foreach (Dictionary<string, object> ribbon in ribbons)
                    if (CentreBody(Text(ribbon, "centre")) == body)
                        first = Mathf.Min(first, Window((Dictionary<string, object>)ribbon["motion"]).x);
            return first == float.MaxValue ? 0f : first;
        }

        private static int CentreBody(string centre) =>
            centre == "station" ? 0 : centre.StartsWith("body ") && int.TryParse(centre.Substring(5), out int n) ? n : -1;

        private static Vector2 Window(Dictionary<string, object> motion)
        {
            var visible = (List<object>)motion["visible"];
            return new Vector2((float)(double)visible[0], (float)(double)visible[1]);
        }

        private static void BuildLook(Dictionary<string, object> look, string slug, string key, int onBody, List<Vector3> bodyAt, float shift,
            Lists lists, ElementArtSet elementArt, Material glow)
        {
            string element = Text(look, "element");
            if (string.IsNullOrEmpty(element) && look.TryGetValue("pair", out var pairValue) && pairValue is List<object> pair && pair.Count > 0) element = pair[0] as string ?? "";
            float seconds = Number(look, "duration_s", 2f) - shift;
            bool playerVariant = onBody < 0;
            Vector3 playerAt = look.TryGetValue("player", out var playerAtValue) && playerAtValue is Dictionary<string, object> playerAtDict ? Vec(playerAtDict["position"], Vector3.zero) : Vector3.zero;
            Vector3 bodyFrom = playerVariant ? playerAt : onBody < bodyAt.Count ? bodyAt[onBody] : Vector3.zero;
            var made = new ElementLookSet.Look { Key = key, Seconds = seconds, FirstPart = lists.Parts.Count, LoopFrom = seconds, OnPlayer = playerVariant };
            // Where this look's body stood from the main body's (or the player's), for the game to find the real one there.
            if (lists.Reactions && bodyAt.Count > 0) made.FromMain = bodyFrom - bodyAt[0];
            // The way the recipe's player faced: toward the main body.
            if (look.TryGetValue("player", out var playerValue) && playerValue is Dictionary<string, object> player && bodyAt.Count > 0)
            {
                Vector3 toward = bodyAt[0] - Vec(player["position"], Vector3.zero);
                made.FacingYaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
            }
            var shellTimes = new List<(float start, float grow)>();
            int ribbonsMade = 0;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                string kit = Text(group, "kit"), kitObject = Text(group, "kit_object"), material = Text(group, "material");
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                for (int n = 0; n < instances.Count; n++)
                {
                    if (lists.Parts.Count - made.FirstPart >= ElementLooks.MostParts) break;
                    var instance = (List<object>)instances[n];
                    var motion = (Dictionary<string, object>)motions[n];
                    bool onPlayer = motion.TryGetValue("on_player", out var flag) && flag is bool yes && yes;
                    string placementHere = instance.Count > 4 ? instance[4] as string : "b";
                    Vector3 position = Vec(instance[1], Vector3.zero);
                    if (lists.Reactions)
                    {
                        // A reaction's piece belongs to the player's look or a body's, never to both.
                        bool onThePlayer = onPlayer || placementHere == "p";
                        if (onThePlayer != playerVariant) continue;
                        if (playerVariant) onPlayer = true;
                    }
                    made.OnPlayer |= onPlayer;
                    // The main body's pieces only (or the player's): the other two bodies show the same timeline again.
                    Vector3 judged = position;
                    if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbitAt && orbitAt.ContainsKey("centre"))
                        judged = Vec(orbitAt["centre"], position);
                    if (motion.TryGetValue("follow_body", out var follow) && follow is double followed && (int)followed != onBody) continue;
                    // A group of one is a single piece (a rust segment, "rust1"): its number is not a body's.
                    // A look with no bodies (a brazier's) keeps every piece.
                    if (!onPlayer && bodyAt.Count > 0 && (instances.Count > 1 ? BodyOf(instance[0] as string, bodyAt, judged) : Nearest(bodyAt, judged)) != onBody) continue;
                    var window = Window(motion);
                    float show = window.x - shift, hide = window.y >= 9f ? 9f : window.y - shift;
                    // The piece about its own anchor: the player for the player's look, else its body.
                    if (playerVariant) position -= playerAt;
                    else if (onBody != 0) position -= bodyFrom;
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
                    // Turned to Stone's second skin of stone over the whole body is its stone shell at full height, which
                    // the body's own material draws (Clover/Toon's _IceStone); nothing more to build.
                    if (kit == "enemy_model")
                    {
                        if (material != "r4_stone") lists.Missing.Add($"{slug}: {kitObject} (a second skin of the enemy model, drawn by the body's own material: not built yet)");
                        continue;
                    }
                    if (kit == Mockup)
                    {
                        // The shell's band: its window opens when the band snaps on; the scale keys say how long it grows.
                        float grow = 0.1f;
                        if (motion.TryGetValue("scale_keys", out var growKeys) && growKeys is List<object> keyList && keyList.Count >= 2)
                            grow = (float)((double)((List<object>)keyList[keyList.Count - 1])[0] - (double)((List<object>)keyList[0])[0]);
                        shellTimes.Add((show, grow));
                        // Turned to Stone's bands are stone and Venom Rime's poisoned ice, by the material they wore.
                        made.Shell = slug.StartsWith("metal") ? ElementLookSet.Steel
                            : material == "r4_stone" ? ElementLookSet.Stone
                            : material == "r4_venom_ice" ? ElementLookSet.Venom
                            : slug.Contains("rime") ? ElementLookSet.Rime : ElementLookSet.Ice;
                        continue;
                    }
                    // Temper's climbing ring and clang are the shader's bright front on the body now (no rings round the player).
                    if (slug == "metal_temper" && (kitObject.Contains("metal_ring") || kitObject.Contains("clang"))) continue;
                    if (!TryKitPiece(kit, kitObject, lists.Kits, out var filter, out bool comboTurn))
                    {
                        lists.Missing.Add($"{slug}: {kitObject} ({kit})");
                        continue;
                    }
                    var rotation = Quaternion.Euler(Vec(instance[2], Vector3.zero));
                    var scale = Vec(instance[3], Vector3.one);
                    if (LookScales.TryGetValue(slug + "|" + kitObject, out float resize)) scale *= resize;
                    if (LookLifts.TryGetValue(slug + "|" + kitObject, out float lift)) position.y += lift;
                    if (LookPaints.ContainsKey(slug + "|" + material)) material = slug + "|" + material;
                    string placement = instance.Count > 4 ? instance[4] as string : "b";
                    string verb = Text(motion, "verb");
                    var mesh = Bake(key, lists.Parts.Count - made.FirstPart, filter, comboTurn, material, element, scale, placement == "g", out byte kind, out Material wears, elementArt, glow, lists.Target);
                    bool held = hide >= 9f;
                    var part = new ElementLookSet.Part
                    {
                        Mesh = mesh, Material = wears, Kind = kind,
                        Follows = placement != "g" || onPlayer,
                        Repeats = Repeating.Any(word => verb.Contains(word)) && !held,
                        Cycles = held && CyclingWords.Any(word => verb.Contains(word)),
                        RunsOn = held && TurningWords.Any(word => verb.Contains(word)),
                        Redraw = motion.ContainsKey("redraw") || verb.StartsWith("flicker ("),
                        Position = position, Rotation = rotation, Scale = scale, Show = show, Hide = hide,
                    };
                    part.PathFirst = lists.Keys.Count;
                    part.PathCount = AddKeys(motion, "path", lists.Keys, false, shift);
                    part.ScaleFirst = lists.Keys.Count;
                    part.ScaleCount = AddKeys(motion, "scale_keys", lists.Keys, false, shift);
                    if (motion.TryGetValue("spin", out var spin) && spin is Dictionary<string, object> spinning)
                    {
                        part.SpinAxis = Vec(spinning["axis"], Vector3.up);
                        part.SpinFirst = lists.Keys.Count;
                        part.SpinCount = AddKeys(spinning, "keys", lists.Keys, true, shift);
                    }
                    if (orbitValue is Dictionary<string, object> orbiting)
                    {
                        part.OrbitCentre = Vec(orbiting["centre"], Vector3.zero);
                        // A piece of another body orbits that body (the recipe gives every leaf the main body's centre).
                        if (onBody != 0) part.OrbitCentre = Vector3.zero;
                        part.OrbitFirst = lists.Keys.Count;
                        part.OrbitCount = AddKeys(orbiting, "keys", lists.Keys, true, shift);
                    }
                    if (part.Repeats) made.LoopFrom = Mathf.Min(made.LoopFrom, show);
                    // Soaked's puddle stays small and low round the feet, whatever size the render gave it.
                    if (slug == "water_soaked" && kind == ElementLookSet.Flat)
                    {
                        float grown = 1f;
                        for (int k = part.ScaleFirst; k < part.ScaleFirst + part.ScaleCount; k++)
                            grown = Mathf.Max(grown, Mathf.Max(lists.Keys[k].Value.x, lists.Keys[k].Value.z));
                        float across = Mathf.Max(mesh.bounds.size.x * part.Scale.x, mesh.bounds.size.z * part.Scale.z) * grown;
                        if (across > SoakedPuddle) part.Scale *= SoakedPuddle / across;
                    }
                    lists.Parts.Add(part);
                }
            }

            if (look.TryGetValue("ribbons", out var ribbonValue) && ribbonValue is List<object> ribbons)
            {
                foreach (Dictionary<string, object> ribbon in ribbons)
                {
                    if (lists.Parts.Count - made.FirstPart >= ElementLooks.MostParts) break;
                    string centre = Text(ribbon, "centre");
                    // A ribbon round the player belongs to the player's look; round a body, to that body's.
                    if (lists.Reactions)
                    {
                        if ((centre == "player") != playerVariant) continue;
                        if (!playerVariant && CentreBody(centre) != onBody) continue;
                    }
                    else
                    {
                        if (centre != "player" && CentreBody(centre) != onBody) continue;
                        if (centre == "player") made.OnPlayer = true;
                    }
                    var motion = (Dictionary<string, object>)ribbon["motion"];
                    var window = Window(motion);
                    float show = window.x - shift, hide = window.y >= 9f ? 9f : window.y - shift;
                    string material = Text(ribbon, "material");
                    string note = Text(motion, "note");
                    var points = new List<Vector3>();
                    foreach (var point in (List<object>)ribbon["points"]) points.Add(Vec(point, Vector3.zero));
                    if (RibbonLifts.TryGetValue(slug, out var stretch) && points.Count > 1)
                    {
                        float low = float.MaxValue, high = float.MinValue;
                        foreach (var point in points)
                        {
                            low = Mathf.Min(low, point.y);
                            high = Mathf.Max(high, point.y);
                        }
                        if (high >= stretch.x && high - low > 0.01f)
                            for (int p = 0; p < points.Count; p++)
                                points[p] += Vector3.up * (stretch.y * (points[p].y - low) / (high - low));
                    }
                    float width = Number(ribbon, "width_m", 0.1f) * (RibbonWidths.TryGetValue(slug + "|" + material, out float wider) ? wider : 1f);
                    var taper = new Vector2(1f, 1f);
                    if (ribbon.TryGetValue("taper_tail_head", out var taperValue) && taperValue is List<object> taperList && taperList.Count >= 2)
                        taper = new Vector2((float)(double)taperList[0], (float)(double)taperList[1]);
                    bool jagged = material.Contains("bolt");
                    // Wind is a band of air (wide across, thin through); a bolt is round.
                    float through = jagged ? 1f : 0.45f;
                    // A look's own paint for the ribbon's material where it has one (The Bolt Runs True's bolt).
                    var paint = PaintOf(LookPaints.ContainsKey(slug + "|" + material) ? slug + "|" + material : material);
                    var mesh = RibbonMesh($"look_{key}_{lists.Parts.Count - made.FirstPart:00}_{Text(ribbon, "object")}", points, width, Text(ribbon, "radius_profile"), taper, jagged, through, paint.Colour);
                    AssetDatabase.AddObjectToAsset(mesh, lists.Target);
                    // The solid a ribbon is drawn in, as for pieces: SolidOf first (the curse's chain in the dark solid).
                    string owner = SolidOf.TryGetValue(material, out string ribbonSolid) ? ribbonSolid : ReactionArtBuilder.ElementOf(material, element + "_" + element);
                    int ring = ReactionDesigns.RingIndex(owner);
                    bool inked = !(ribbon.TryGetValue("ink", out var inkValue) && inkValue is bool ink && !ink);
                    var part = new ElementLookSet.Part
                    {
                        Mesh = mesh, Material = ring >= 0 && ring < elementArt.Solids.Length ? elementArt.Solids[ring] : elementArt.Solids[0],
                        Kind = ElementLookSet.Ribbon, Follows = true,
                        // What comes again and again while the look is kept: the curls, the arcs; not the wind arriving,
                        // nor a strike's own shapes (played once).
                        Repeats = hide < 9f && !note.Contains("comes in from the blow") && slug != "wind_strike",
                        Position = Vector3.zero, Rotation = Quaternion.identity, Scale = Vector3.one, Show = show, Hide = hide,
                        InkColour = RibbonInks.TryGetValue(material, out var inkColour) ? inkColour : elementArt.Solids[Mathf.Max(0, ring)].GetColor("_OutlineColor"),
                        InkWidth = inked ? Mathf.Clamp(width * 0.16f, 0.008f, 0.025f) : 0f,
                        Thin = RibbonThin.TryGetValue(material, out float thin) ? thin : 0f,
                    };
                    part.PathFirst = lists.Keys.Count;
                    part.PathCount = AddKeys(motion, "path", lists.Keys, false, shift);
                    part.DrawFirst = lists.Keys.Count;
                    part.DrawCount = AddKeys(motion, "draw_on", lists.Keys, false, shift);
                    part.EraseFirst = lists.Keys.Count;
                    part.EraseCount = AddKeys(motion, "erase", lists.Keys, false, shift);
                    if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit)
                    {
                        // Its points lie about its centre, so it turns about its own origin: degrees from its start on,
                        // a steady rate (Unity's sense, as the recipe's angles are); a rise climbs it as it turns.
                        float start = Number(orbit, "start", show + shift) - shift;
                        float end = hide >= 9f ? seconds : hide;
                        float degrees = 360f * Number(orbit, "turns_per_second", 0f) * Mathf.Max(0.01f, end - start);
                        part.OrbitCentre = Vector3.zero;
                        part.OrbitFirst = lists.Keys.Count;
                        lists.Keys.Add(new ElementLookSet.Key { Time = start, Value = Vector3.zero, Ease = ElementLookSet.Linear });
                        lists.Keys.Add(new ElementLookSet.Key { Time = end, Value = new Vector3(degrees, 0f, 0f), Ease = ElementLookSet.Linear });
                        part.OrbitCount = 2;
                        part.RunsOn = hide >= 9f;
                        float rise = Number(orbit, "rise", 0f);
                        if (rise != 0f && part.PathCount == 0)
                        {
                            part.PathFirst = lists.Keys.Count;
                            lists.Keys.Add(new ElementLookSet.Key { Time = start, Value = Vector3.zero, Ease = ElementLookSet.Linear });
                            lists.Keys.Add(new ElementLookSet.Key { Time = end, Value = Vector3.up * (rise * (end - start)), Ease = ElementLookSet.Linear });
                            part.PathCount = 2;
                        }
                    }
                    if (part.Repeats) made.LoopFrom = Mathf.Min(made.LoopFrom, show);
                    lists.Parts.Add(part);
                    lists.Ribbons++;
                    ribbonsMade++;
                }
            }

            made.PartCount = lists.Parts.Count - made.FirstPart;
            if (shellTimes.Count > 0)
            {
                shellTimes.Sort((a, b) => a.start.CompareTo(b.start));
                made.ShellBands = shellTimes.Count;
                made.ShellStart = shellTimes[0].start;
                made.ShellStep = shellTimes.Count > 1 ? (shellTimes[shellTimes.Count - 1].start - shellTimes[0].start) / (shellTimes.Count - 1) : 0.1f;
                made.ShellGrow = shellTimes[0].grow;
            }
            lists.Looks.Add(made);
            Report.AppendLine($"| {key} | {made.Seconds:0.0} s | {(made.OnPlayer ? "player" : "body")} | {made.PartCount} | {ribbonsMade} | {made.LoopFrom:0.00} s | " +
                $"{(made.Shell == ElementLookSet.NoShell ? "-" : $"{made.ShellBands} bands from {made.ShellStart:0.00} s, every {made.ShellStep:0.00} s, {made.ShellGrow:0.00} s each")} |");
        }

        /// <summary>
        /// A ribbon's tube along its points (metres about its centre): smoothed through its points (a bolt keeps its hard
        /// corners), <paramref name="width"/> across at its widest, <paramref name="through"/> of that through it (a band
        /// of wind stands up across its way; one lying on the floor lies flat), narrowing by its profile (taper: from the
        /// tail's share to the head's; needle: thin at both ends) and closed at both ends. Its u runs from the tail, 0, to
        /// the head, 1, by length, for Clover/Spell's draw clip; uv1 carries the normals the ink is pushed along.
        /// </summary>
        public static Mesh RibbonMesh(string name, List<Vector3> points, float width, string profile, Vector2 taper, bool jagged, float through, Color colour)
        {
            var line = new List<Vector3>();
            if (points.Count < 2) points = new List<Vector3> { Vector3.zero, Vector3.up * 0.1f };
            if (jagged) line.AddRange(points);
            else
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector3 p0 = points[Mathf.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Mathf.Min(points.Count - 1, i + 2)];
                    for (int s = 0; s < RibbonSteps; s++) line.Add(CatmullRom(p0, p1, p2, p3, s / (float)RibbonSteps));
                }
                line.Add(points[points.Count - 1]);
            }
            int count = line.Count;
            var along = new float[count];
            for (int i = 1; i < count; i++) along[i] = along[i - 1] + Vector3.Distance(line[i - 1], line[i]);
            float total = Mathf.Max(0.0001f, along[count - 1]);
            bool lying = true;
            foreach (var p in line) lying &= p.y < 0.12f;
            float cap = Mathf.Min(width * 0.6f, total * 0.2f);

            var vertices = new List<Vector3>(count * RibbonSides);
            var normals = new List<Vector3>(count * RibbonSides);
            var uvs = new List<Vector2>(count * RibbonSides);
            var colours = new List<Color>(count * RibbonSides);
            for (int i = 0; i < count; i++)
            {
                Vector3 tangent = (line[Mathf.Min(count - 1, i + 1)] - line[Mathf.Max(0, i - 1)]).normalized;
                if (tangent.sqrMagnitude < 0.5f) tangent = Vector3.forward;
                // Across: up (a standing band), or flat along the floor for one lying on it.
                Vector3 side = Vector3.Cross(Vector3.up, tangent);
                if (side.sqrMagnitude < 0.0001f) side = Vector3.Cross(Vector3.forward, tangent);
                side.Normalize();
                Vector3 up = Vector3.Cross(tangent, side).normalized;
                Vector3 wide = lying ? side : up, thin = lying ? up : side;
                float u = along[i] / total;
                float shape = profile == "needle" ? Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * u)), 0.6f) : profile == "flat" ? 1f : Mathf.Lerp(taper.x, taper.y, u);
                float closed = cap > 0f ? Mathf.Sqrt(Mathf.Clamp01(Mathf.Min(along[i], total - along[i]) / cap)) : 1f;
                float radius = 0.5f * width * Mathf.Max(0f, shape) * closed;
                for (int s = 0; s < RibbonSides; s++)
                {
                    float angle = s * Mathf.PI * 2f / RibbonSides;
                    float c = Mathf.Cos(angle), n = Mathf.Sin(angle);
                    vertices.Add(line[i] + wide * (c * radius) + thin * (n * radius * through));
                    normals.Add((wide * (c * through) + thin * n).normalized);
                    uvs.Add(new Vector2(u, s / (float)RibbonSides));
                    colours.Add(new Color(colour.r, colour.g, colour.b, 1f));
                }
            }
            var triangles = new List<int>((count - 1) * RibbonSides * 6);
            for (int i = 0; i < count - 1; i++)
                for (int s = 0; s < RibbonSides; s++)
                {
                    int a = i * RibbonSides + s, b = i * RibbonSides + (s + 1) % RibbonSides, c = a + RibbonSides, d = b + RibbonSides;
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(b); triangles.Add(c); triangles.Add(d);
                }
            // Faces turned out (Unity's front is the side the winding's normal points to), so the ink's back faces are inside.
            int probe = 0;
            for (int t = 0; t < triangles.Count; t += 3)
            {
                Vector3 face = Vector3.Cross(vertices[triangles[t + 1]] - vertices[triangles[t]], vertices[triangles[t + 2]] - vertices[triangles[t]]);
                if (face.sqrMagnitude < 1e-12f) continue;
                probe = Vector3.Dot(face, normals[triangles[t]]) >= 0f ? 1 : -1;
                break;
            }
            if (probe < 0)
                for (int t = 0; t < triangles.Count; t += 3)
                {
                    int keep = triangles[t + 1];
                    triangles[t + 1] = triangles[t + 2];
                    triangles[t + 2] = keep;
                }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colours);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
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
            string path = kit == E2Kit || kit == BrazierKit ? ReactionArtBuilder.CombosFolder + "/" + kit : ReactionArtBuilder.KitPath(kit);
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
        // render re-colours shared meshes per piece), or a colour a slot for the batch's own many-slot pieces (the sword),
        // or a flame's bands by height.
        private static Mesh Bake(string slug, int index, MeshFilter filter, bool comboTurn, string material, string element, Vector3 scale, bool onGround,
            out byte kind, out Material wears, ElementArtSet elementArt, Material glow, Object into)
        {
            string bare = material.Substring(material.IndexOf('|') + 1);
            // A brazier's bed is drawn in its own colour, flat (the solid's bright top facet turned the beds grey), and
            // keeps its own paint where the look repaints other pieces of the same material (Light's gilded bed).
            bool bed = filter.name == BedPiece;
            if (bed) material = bare;
            if (filter.name == LeafPiece && LeafColours.TryGetValue(bare, out var leafColour))
                return BakeLeaf(slug, index, filter, comboTurn, element, leafColour, out kind, out wears, elementArt, into);
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
            bool perSlot = source.subMeshCount > 1 && filter.sharedMesh.name.StartsWith("vfxe2_") && material.Substring(material.IndexOf('|') + 1).StartsWith("m3_");
            var mainPaint = PaintOf(material);
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var paint = perSlot && s < slots.Length && slots[s] != null ? PaintOf(slots[s].name) : mainPaint;
                var subTriangles = source.GetTriangles(s);
                // A solid's alpha is Clover/Spell's flag (below a half is foam); a see-through piece's is how much shows.
                float alpha = mainPaint.Glow || bed || (onGround && FlatAfter(source, toKit, scale)) ? paint.Colour.a : paint.Flag;
                foreach (int v in subTriangles) colours[v] = new Color(paint.Colour.r, paint.Colour.g, paint.Colour.b, alpha);
                triangles.AddRange(subTriangles);
            }
            // A flame tongue or lick is banded from its foot up, hottest at the foot (notes.md, "Fire: Burn").
            // (Only in fire's own paint: Smoulder's black flame keeps its black.)
            if ((filter.name == "vfxe2_flame" || filter.name == "vfxe2_flame_lick") && bare.StartsWith("m3_flame") && !material.Contains("|") && vertices.Length > 0)
            {
                float low = float.MaxValue, high = float.MinValue;
                foreach (var v in vertices)
                {
                    low = Mathf.Min(low, v.y);
                    high = Mathf.Max(high, v.y);
                }
                for (int i = 0; i < vertices.Length; i++)
                {
                    float share = (vertices[i].y - low) / Mathf.Max(0.0001f, high - low);
                    int band = 0;
                    for (int b = 0; b < FlameBandFrom.Length; b++)
                        if (share >= FlameBandFrom[b]) band = b;
                    colours[i] = new Color(FlameBands[band].r, FlameBands[band].g, FlameBands[band].b, colours[i].a);
                }
            }
            var bounds = new Bounds(vertices.Length > 0 ? Vector3.Scale(vertices[0], scale) : Vector3.zero, Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(Vector3.Scale(v, scale));
            bool flat = (onGround && bounds.size.y < FlatHeight) || mainPaint.Glow || bed;
            kind = flat ? ElementLookSet.Flat : ElementLookSet.Solid;
            if (flat) wears = glow;
            else
            {
                string owner = SolidOf.TryGetValue(bare, out string drawnIn) ? drawnIn : ReactionArtBuilder.ElementOf(bare, element + "_" + element);
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
            AssetDatabase.AddObjectToAsset(mesh, into);
            return mesh;
        }

        // The kit piece's own axes, as Bake turns them: its vertices in the kit's frame about its pivot.
        private static Vector3[] KitVertices(MeshFilter filter, bool comboTurn)
        {
            var kitRoot = filter.transform.root;
            var toKit = kitRoot.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            toKit.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            if (comboTurn) toKit = ReactionArtBuilder.ComboKitTurn * toKit;
            var vertices = filter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++) vertices[i] = toKit.MultiplyPoint3x4(vertices[i]);
            return vertices;
        }

        /// <summary>
        /// A leaf as long and as wide as the kit's leaf, lying the way it lay (so the recipe's turns still fit): a pointed
        /// oval, folded a little along a darker midrib that runs from the stalk to the tip, drawn from both sides.
        /// </summary>
        public static Mesh LeafMesh(string name, Vector3 low, Vector3 high, int along, int across, int through, Color colour)
        {
            const int Stations = 9;
            Vector3 size = high - low;
            float length = size[along], width = size[across];
            float centreAcross = (low[across] + high[across]) * 0.5f, centreThrough = (low[through] + high[through]) * 0.5f;
            var rib = colour * 0.5f;
            rib.a = 1f;
            var vertices = new List<Vector3>();
            var colours = new List<Color>();
            var triangles = new List<int>();
            // Each station across the leaf: edge, rib's near side (leaf colour), rib (dark) twice, rib's far side, far edge.
            float[] spans = { -1f, -0.07f, -0.07f, 0.07f, 0.07f, 1f };
            for (int s = 0; s < Stations; s++)
            {
                float u = s / (float)(Stations - 1);
                // Rounder toward the stalk, drawn out to a point at the tip.
                float half = 0.5f * width * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * Mathf.Pow(u, 0.8f))), 0.75f);
                for (int k = 0; k < spans.Length; k++)
                {
                    float sideways = Mathf.Abs(spans[k]) < 0.5f ? spans[k] * width : spans[k] * half;
                    // The fold: the rib a little raised, the edges level.
                    float fold = Mathf.Abs(spans[k]) < 0.5f ? 0.08f * width : 0f;
                    var v = new Vector3();
                    v[along] = low[along] + u * length;
                    v[across] = centreAcross + sideways;
                    v[through] = centreThrough + fold;
                    vertices.Add(v);
                    colours.Add(k == 2 || k == 3 ? rib : new Color(colour.r, colour.g, colour.b, 1f));
                }
            }
            for (int s = 0; s < Stations - 1; s++)
                for (int k = 0; k < spans.Length - 1; k += 2)
                {
                    int a = s * spans.Length + k, b = a + 1, c = a + spans.Length, d = b + spans.Length;
                    triangles.AddRange(new[] { a, c, b, b, c, d });
                }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            var normals = new List<Vector3>();
            mesh.GetNormals(normals);
            mesh.SetUVs(1, normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BakeLeaf(string slug, int index, MeshFilter filter, bool comboTurn, string element, Color colour,
            out byte kind, out Material wears, ElementArtSet elementArt, Object into)
        {
            var vertices = KitVertices(filter, comboTurn);
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            var size = bounds.size;
            int along = size.x >= size.y && size.x >= size.z ? 0 : size.y >= size.z ? 1 : 2;
            int through = size.x <= size.y && size.x <= size.z ? 0 : size.y <= size.z ? 1 : 2;
            if (through == along) through = (along + 1) % 3;
            int across = 3 - along - through;
            var mesh = LeafMesh($"look_{slug}_{index:00}_{filter.name}_leaf", bounds.min, bounds.max, along, across, through, colour);
            AssetDatabase.AddObjectToAsset(mesh, into);
            kind = ElementLookSet.Solid;
            int ring = ReactionDesigns.RingIndex(element);
            wears = ring >= 0 && ring < elementArt.Solids.Length ? elementArt.Solids[ring] : elementArt.Solids[0];
            return mesh;
        }

        // Glaze's crystal: the kit's hexagonal crystal stood up on its base (its long axis up, the base at its origin),
        // in a clear blue, deeper in its facets, and the ice solid it is drawn in.
        private static void BakeCrystal(ElementLookSet set, Lists lists, ElementArtSet elementArt)
        {
            set.Crystal = null;
            if (!TryKitPiece(E2Kit, CrystalPiece, lists.Kits, out var filter, out bool comboTurn)) return;
            var vertices = KitVertices(filter, comboTurn);
            var bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);
            var size = bounds.size;
            int along = size.x >= size.y && size.x >= size.z ? 0 : size.y >= size.z ? 1 : 2;
            var up = Vector3.zero;
            up[along] = 1f;
            var stand = Quaternion.FromToRotation(up, Vector3.up);
            float foot = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = stand * (vertices[i] - bounds.center);
                foot = Mathf.Min(foot, vertices[i].y);
            }
            var colours = new Color[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y -= foot;
                colours[i] = new Color(GlazeCrystalPaint.Colour.r, GlazeCrystalPaint.Colour.g, GlazeCrystalPaint.Colour.b, GlazeCrystalPaint.Flag);
            }
            var mesh = new Mesh { name = "glaze_crystal" };
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(filter.sharedMesh.triangles, 0);
            mesh.RecalculateNormals();
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, mesh.normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, set);
            set.Crystal = mesh;
            int ring = ReactionDesigns.RingIndex("ice");
            set.CrystalMaterial = ring >= 0 && ring < elementArt.Solids.Length ? elementArt.Solids[ring] : null;
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
            if (material != null) Unknown.Add(material);
            return new ReactionArtBuilder.Paint(0.5f, 0.5f, 0.5f);
        }

        // A channel's keys: [[t, value, easing], ...]; a value is a vector, a share, or degrees for a spin or an orbit;
        // its times moved back by shift (a body's own look begun at its own start).
        private static int AddKeys(Dictionary<string, object> from, string name, List<ElementLookSet.Key> keys, bool degrees, float shift = 0f)
        {
            if (!from.TryGetValue(name, out var value) || !(value is List<object> list)) return 0;
            int added = 0;
            foreach (var entry in list)
            {
                if (!(entry is List<object> key) || key.Count < 2) continue;
                float time = (float)(double)key[0] - shift;
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
