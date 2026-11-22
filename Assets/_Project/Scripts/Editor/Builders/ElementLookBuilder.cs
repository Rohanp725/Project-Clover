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
    public static partial class ElementLookBuilder
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

        /// <summary>
        /// Seconds a reaction's look stays on the body struck, its held pieces and shell standing, where what the reaction
        /// does outlasts the recipe's 1.6 s (the films of 2026-11-20: Turned to Stone's stone fell away at 1.6 s while the
        /// enemy stood as stone 1.9 s more; Venom Rime's green ice gave way to plain Chill while the poison still froze it).
        /// Turned to Stone: 1 + 1 s of climbing and 1.5 s as stone (Reactions.Three). Venom Rime: its bites go on until it
        /// freezes, about three (Reactions.Four).
        /// </summary>
        public static readonly Dictionary<string, float> ReactionHolds = new Dictionary<string, float>
        {
            ["turned_to_stone"] = 3.5f,
            ["venom_rime"] = 4.5f,
            // Black Chain binds the two for 5 s (Reactions.Near): the chain stays drawn between them as long.
            ["black_chain"] = 5f,
        };

        /// <summary>
        /// Seconds every other reaction's look stays on each body it is on, its held pieces standing (the crack, the
        /// grown plants, the pit, the cloud that waits on) and the afflictions it spent hidden under it: past the
        /// recipe's 1.6 s the films of 2026-11-20 showed the plain looks of what it spent come back over its aftermath
        /// (Seeded's hexagon seeds over Quickening's plants, "they should just stay grown"; Static's crawl where The
        /// Grudge's cloud had been; Marked's sun-wheel filling the floor).
        /// </summary>
        public const float AftermathHold = 3f;

        /// <summary>
        /// Shell timings one look draws its own way, by slug: (first band, each next band later by, each band's growing
        /// time, the time the shell is gone by; 0: at the look's end). Rime (the owner, 2026-11-20: "the rime instantly
        /// all appears instead of slowly building"): its five bands came on in 0.8 s and stayed white over the whole
        /// body to 1.7 s; they climb a band each 0.2 s and are gone with the heavy that breaks the crust off (1.05 s).
        /// </summary>
        public static readonly Dictionary<string, Vector4> ShellTimes = new Dictionary<string, Vector4>
        {
            ["rime"] = new Vector4(0.08f, 0.19f, 0.22f, 1.05f),
            // Winter Seed: the frost the seed slept under thaws and is gone by 0.6 s (it stayed over the body to the end).
            ["winter_seed"] = new Vector4(-0.3f, 0f, 0.01f, 0.6f),
        };

        /// <summary>
        /// Ribbons one look leaves out, by "slug|material": The Grudge's cyan crawl over the body before the black bolt
        /// (the owner, 2026-11-19: "it looks like some lightning is frozen in place even before the black bolt").
        /// </summary>
        public static readonly HashSet<string> LeftOutRibbons = new HashSet<string>
        {
            "the_grudge|m3_bolt",
            // Round 4: the wind trails left combed on the floor by Windthrow (the game's Seed Storm) and Golden Feathers.
            "seed_storm|m3_wind_dust", "golden_feathers|e3_wind_comb",
        };

        /// <summary>
        /// Ribbons one look breaks up and fades out, by "slug|part of the ribbon's name": (when it begins to go, when it is
        /// gone). Split Rock's (the game's Root Snare) root web stayed on the floor after the roots were torn (round 4):
        /// it is wiped from its tail as the roots break at 0.64 s and gone by 1 s.
        /// </summary>
        public static readonly Dictionary<string, Vector2> RibbonFades = new Dictionary<string, Vector2>
        {
            ["root_snare|grip"] = new Vector2(0.64f, 1.0f),
            ["root_snare|rootlet"] = new Vector2(0.64f, 1.0f),
        };

        /// <summary>
        /// Ribbons one look carries further out from their body, by "slug|material" (the share their reach is grown by):
        /// Breath Taken's black wind (the owner, 2026-11-20: "it reads not as black wind coming out, it needs to have more
        /// wind identity") was curls hugging the body; it bursts out and away.
        /// </summary>
        public static readonly Dictionary<string, float> RibbonReach = new Dictionary<string, float>
        {
            ["breath_taken|r4_blackwind"] = 2.1f,
        };

        /// <summary>
        /// Low ribbons one look heaves up off the floor, by "slug|material", metres at their middle: Rot Roots' runners
        /// lay flat on the floor (the owner, 2026-11-19: "roots that actually grow up from the ground").
        /// </summary>
        public static readonly Dictionary<string, float> RibbonHumps = new Dictionary<string, float>
        {
            ["rot_roots|r4_rotroot"] = 0.3f,
        };

        /// <summary>
        /// Ribbons painted from their tail to their head in two colours, by "slug|material": Rot Roots' roots black-brown
        /// at the foot going sick olive at the tips (the owner, 2026-11-20: "the roots don't give enough of the rot idea").
        /// </summary>
        public static readonly Dictionary<string, (Color tail, Color head)> RibbonGradients = new Dictionary<string, (Color, Color)>
        {
            ["rot_roots|r4_rotroot"] = (new Color(0.035f, 0.022f, 0.015f), new Color(0.36f, 0.4f, 0.05f)),
        };

        /// <summary>
        /// A ribbon that winds round its body close in (a root up the shins, an arc over the chest) is laid this much
        /// further out: the clay stand-in was slim, and round a castle soldier's armour the roots of Root Snare, Seed
        /// Storm and Rot Roots were inside the body, so no root was seen gripping the legs (the audit of 2026-11-20).
        /// </summary>
        public const float WrapOut = 1.5f, WrapRadius = 0.36f;

        /// <summary>Reactions whose hold keeps their look on every body it reaches, not only the one struck: both ends of Black Chain's chain.</summary>
        public static readonly HashSet<string> HeldOnEveryBody = new HashSet<string> { "black_chain" };

        /// <summary>
        /// Ribbons drawn as a chain of links instead of a smooth tube, by "slug|material": Black Chain's chain (the owner,
        /// 2026-11-19: "I dont see the black chain at all in the gif, it should be a chain in pure black binding two enemies
        /// together"). Links big enough to read from the game camera: length, width and the thickness of the iron, metres.
        /// </summary>
        public static readonly Dictionary<string, Vector3> ChainLinks = new Dictionary<string, Vector3>
        {
            ["black_chain|r4_chain"] = new Vector3(0.34f, 0.2f, 0.045f),
        };

        /// <summary>
        /// Kit pieces one look leaves out, by "slug|kit object", where the builder draws them its own way: Black Chain's
        /// straight kit chain, 1 m of thin links aimed from the wound, which ran behind the struck enemy's health bar
        /// (its film of 2026-11-20); the chain is drawn instead as a span of links between the two bodies (<see cref="ChainSpans"/>).
        /// </summary>
        public static readonly HashSet<string> LeftOut = new HashSet<string>
        {
            "black_chain|vfxr4_chain",
            // Round 4 (the owner's review of 2026-11-22, docs/design/feedback/2026-11-22_round4_plan.md): Ill Wind's green
            // oval on the floor; Briar Patch's thorns out of the ground (the plants' own thorns carry it); the brown thorns,
            // seed lumps and sprouts on the enemies' backs from Life's charged blows and Grown Into It; Miasma's pool (the
            // cloud only); Chill's crystals (the base look is the frost on the body); Sundered's drops.
            "ill_wind|vfxe3_disc",
            // Briar Patch's own thorns now stand on its plants (ElementLookBuilder.Round4); those at the enemies' feet stay out.
            "e3_briar_patch@b|vfx3c_vfx_life_thorn",
            "e3_life_blade_hit1|vfx3c_vfx_life_thorn", "e3_life_blade_hit2|vfx3c_vfx_life_thorn", "e3_life_blade_hit3|vfx3c_vfx_life_thorn",
            "e3_life_blade_hit1|vfxe2_seed", "e3_life_blade_hit2|vfxe2_seed", "e3_life_blade_hit3|vfxe2_seed",
            "e3_life_blade_hit1|vfx3c_vfx_life_briar", "e3_life_blade_hit2|vfx3c_vfx_life_briar", "e3_life_blade_hit3|vfx3c_vfx_life_briar",
            "e3_life_blade_hit1|vfxe2_spore", "e3_life_blade_hit2|vfxe2_spore", "e3_life_blade_hit3|vfxe2_spore",
            "e3_grown_into_it@b|vfxe2_seed", "e3_grown_into_it@b|vfxe3_vfx_life_sapling", "e3_grown_into_it@b|vfxe2_spore",
            "e3_miasma|vfx3c_vfx_water_ink", "e3_miasma@b|vfx3c_vfx_water_ink",
            "ice_chill|vfxe2_crystal",
            "metal_sundered|vfx3c_vfx_water_drop",
            // Metal's charged blows threw a spray of white-hot sparks round the bite, a white starburst on every weapon (the
            // swing films of 2026-11-20): Metal lands as a glint alone, painted steel (LookPaints).
            "e3_metal_blade_hit1|vfx3c_vfx_metal_spark", "e3_metal_blade_hit2|vfx3c_vfx_metal_spark", "e3_metal_blade_hit3|vfx3c_vfx_metal_spark",
            // Chill's frost rosettes lay as a white snowflake under the frozen body in the element films of 2026-11-20 (the
            // owner of the ice hits: "The white spiderweb thing is really throwing off everything"; the elements 3 ice has no
            // snowflake decal): the crystals and the shell are Chill's look.
            "ice_chill|vfx3c_vfx_ice_rosette",
            // Seed Storm (the owner, 2026-11-20: "the wind throwing the enemy has a bunch of random stuff pop up at the uproot
            // location"): the solid brown pit disc and the ring of green turf chunks round it are gone; the crack and the
            // clods thrown up out of it stay.
            "seed_storm|vfxr4_disc", "seed_storm|vfx3c_vfx_earth_slab_a", "seed_storm|vfx3c_vfx_earth_slab_b",
            // Golden Feathers' vane, inner band and shaft lay in one plane and read as flat yellow strips ("they look like
            // wheat", 2026-11-20): each feather is drawn whole from its vane (FeatherMesh), quill, light inner vane and barbs.
            "golden_feathers|vfxr4_feather2_inner", "golden_feathers|vfxr4_feather2_shaft",
            // Frost Heave's frost running into the ground was Ice's rune ring, drawn as a pale crescent round its feet in the
            // reactions 5 film (2026-11-20; the owner wants no rings): the frost flower on the floor carries the frost.
            "frost_heave|vfx2_ice_runes_spell",
        };

        /// <summary>
        /// Pieces one look moves, by "slug|kit object", metres in the recipe's space: Shining Herb's herb, its flower, its
        /// shaft and its lotus stood 1 m from the clay stand-in and grew out of a castle soldier's side ("The herb shouldnt
        /// be on the enemy it should be next to it", 2026-11-19): they stand 1.6 m off.
        /// </summary>
        public static readonly Dictionary<string, Vector3> LookOffsets = new Dictionary<string, Vector3>
        {
            ["shining_herb|vfxr4_vfx_life_sapling"] = new Vector3(0.58f, 0f, -0.25f),
            ["shining_herb|vfx2_light_shaft_surge"] = new Vector3(0.58f, 0f, -0.25f),
            ["shining_herb|vfx2_light_glint_charged"] = new Vector3(0.58f, 0f, -0.25f),
            ["shining_herb|vfxr4_vfx_life_flower"] = new Vector3(0.58f, 0f, -0.25f),
            ["shining_herb|vfxr4_vfx_light_mote"] = new Vector3(0.58f, 0f, -0.25f),
            ["shining_herb|vfx2_light_lotus_hit"] = new Vector3(0.58f, 0f, -0.25f),
        };

        /// <summary>
        /// Pieces one look draws stretched, by "slug|kit object" (x, y, z on its own scale): Earthlight's slatted shafts were
        /// 4 m beams that from the game camera read as rays coming down from the sky ("It doesnt really give the feel of
        /// light shining upward", 2026-11-19): short broad shafts pouring up out of the glowing cracks.
        /// </summary>
        public static readonly Dictionary<string, Vector3> LookStretch = new Dictionary<string, Vector3>
        {
            ["earthlight|vfx2_light_shaft_surge"] = new Vector3(1.6f, 0.42f, 1.6f),
        };

        /// <summary>
        /// Ribbons one look keeps on longer, by "slug|part of the ribbon's name", seconds: Flashover's bolt from the sky was
        /// drawn and gone in a quarter of a second, before any frame of its film ("The Lightning doesnt look like its
        /// coming from the sky", 2026-11-19).
        /// </summary>
        public static readonly Dictionary<string, float> RibbonLinger = new Dictionary<string, float>
        {
            ["flashover|boltsky"] = 0.3f,
            // The Grudge's black bolt, so it is seen joined to the cloud ("the black bolt doesnt connect to the cloud").
            ["the_grudge|boltblack"] = 0.5f,
        };

        /// <summary>
        /// Solid pieces one look inks in its own colour, by "slug|material": The Grudge's cloud, a heap of lumps each ringed
        /// in the dark solid's lilac ink, read as a pale lilac puff on film, not a black cloud.
        /// </summary>
        public static readonly Dictionary<string, Color> SolidInks = new Dictionary<string, Color>
        {
            ["the_grudge|r4_grudgecloud"] = new Color(0.2f, 0.09f, 0.3f),
            // Dark's strike: the near-black hands read only by a pale violet rim round their outer silhouette (the Blender
            // worker's InkHands line set, notes.md); in the usual dark ink they melt into the shadow under the enemy.
            // (The render's colour is linear; a material colour is set as seen.)
            ["e3_strike_dark@b|st_hand_small"] = new Color(0.5f, 0.3f, 0.82f).gamma,
            // The Dark slam's smoke, about the pit and on each one struck: near black, read by the same violet silhouette
            // (slam notes, dark_outline: 0.42, 0.18, 0.7 linear).
            ["e3_slam_dark|st_sl_shadow_smoke"] = new Color(0.42f, 0.18f, 0.7f).gamma,
            ["e3_slam_dark@b|st_sl_shadow_smoke"] = new Color(0.42f, 0.18f, 0.7f).gamma,
            // The Poison slam's venom: near-black green with the sickly-green rim the owner asked for, so it reads on dark
            // floors (round 5: 0.3, 0.6, 0.12 linear at its edge), drawn as the solid's ink round its silhouette.
            ["e3_slam_poison|st_sl_venom"] = new Color(0.3f, 0.6f, 0.12f).gamma,
        };

        /// <summary>
        /// Ribbons one look draws gnarled (their points knocked a few centimetres about), by "slug|material": Girdled's
        /// coils were smooth even spirals that read as wind round the roots ("it just kinda looks like there is a wind
        /// effect around the black roots", 2026-11-19).
        /// </summary>
        public static readonly HashSet<string> Gnarled = new HashSet<string> { "girdled|r4_figroot" };

        /// <summary>
        /// True for a reaction's lodged seed (the elements 2 kit's seed, a faceted chunk): on the castle soldiers it read as
        /// green hexagon chunks in Quickening, Root Snare, Seed Storm, Winter Seed and Shining Herb (the audit and the
        /// Blender worker's scale notes, 2026-11-20). <paramref name="material"/> is null outside a reaction. Reactions 5's
        /// seeds (e4_seedpod: the seeds diving into Split Rock's ground, lying in Windthrow's torn pit, flung by Quickening)
        /// are the seeds the owner asked to see, and are built.
        /// </summary>
        public static bool IsLodgedSeed(string kitObject, string material) => material != null && kitObject == "vfxe2_seed" && material != "e4_seedpod";

        /// <summary>The flame tongues of a reaction whose body keeps its own Burn: within this far of the body they are the body's real flame (BodyFlames).</summary>
        public const float BodyFlameReach = 0.6f;

        /// <summary>
        /// True for a reaction's flame tongue that stands on a body whose Burn is drawn by the real flame field
        /// (<see cref="ElementLooks.KeepsBodyFlame"/>): the recipe's ring of tongues round the body, stacked up its sides,
        /// read as the staircase the owner turned down (2026-11-20), so the body burns in BodyFlames' fire instead.
        /// <paramref name="fromItsBody"/> is the piece's place from its own body.
        /// </summary>
        public static bool IsBodyFlame(string slug, string kitObject, string material, Vector3 fromItsBody) =>
            ElementLooks.KeepsBodyFlame(slug) && kitObject.StartsWith("vfxe2_flame") && material != null
            && (material.StartsWith("m3_flame") || material.StartsWith("r4_blackflame")) && new Vector2(fromItsBody.x, fromItsBody.z).magnitude < BodyFlameReach;

        /// <summary>
        /// Chains drawn from the struck body to another, by slug: (the recipe's other body, the height its ends wrap the
        /// bodies at, how far its middle arcs up above them, when it whips out and when it has reached). Not baked into
        /// the look: in a fight the game binds the nearest enemy within 6 m, wherever it stands, so the chain is built as a
        /// <see cref="ElementLookSet.Span"/> (one link, the ribbon's paint, solid and ink) that <see cref="BoundChains"/>
        /// lays between the two real bound bodies for as long as the bind lasts. The ends sit at chest height, inside the
        /// recipe's wraps; the span arcs up between the two, where no health bar stands.
        /// </summary>
        public static readonly Dictionary<string, (int body, float height, float arc, float from, float reached)> ChainSpans =
            new Dictionary<string, (int, float, float, float, float)>
            {
                ["black_chain"] = (1, 1.12f, 0.75f, 0.3f, 0.5f),
            };

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
            ["r4_feather_pale"] = new ReactionArtBuilder.Paint(0.92f, 0.62f, 0.26f),
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
            // Reactions 5's Carried Embers: the burst of sparks where the whirl reaches an enemy, hot orange, never white.
            ["r4_ember_hot"] = new ReactionArtBuilder.Paint(1f, 0.52f, 0.1f),
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
            // Elements 3's redesigned reactions, from the Blender worker's reaction_scale_notes.md (linear): Rime's ice a
            // saturated cyan to deep blue (never white), Smoulder's flame near black with a violet heart over a violet coal.
            ["e3_ice_tooth"] = new ReactionArtBuilder.Paint(0.16f, 0.48f, 0.9f),
            ["e3_ice_trail"] = new ReactionArtBuilder.Paint(0.22f, 0.55f, 0.95f),
            ["e3_frost_bed"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.8f, 0.45f, 1f, true),
            ["e3_blackflame"] = new ReactionArtBuilder.Paint(0.012f, 0f, 0.022f),
            ["e3_blackflame_core"] = new ReactionArtBuilder.Paint(0.3f, 0.08f, 0.62f),
            ["e3_coal"] = new ReactionArtBuilder.Paint(0.3f, 0.06f, 0.55f, 0.7f, 1f, true),
            ["e3_black"] = new ReactionArtBuilder.Paint(0.02f, 0.01f, 0.03f),
            ["e3_dark_trail"] = new ReactionArtBuilder.Paint(0.02f, 0f, 0.04f),
            ["e3_breath"] = new ReactionArtBuilder.Paint(0.34f, 0.37f, 0.44f, 0.6f, 1f, true),
            ["e3_dark_leaf"] = new ReactionArtBuilder.Paint(0.12f, 0.04f, 0.2f),
            ["e3_black_comb"] = new ReactionArtBuilder.Paint(0.03f, 0.02f, 0.04f, 0.7f, 1f, true),
            ["e3_feather_vane"] = new ReactionArtBuilder.Paint(1f, 0.62f, 0.16f),
            ["e3_gold_mote"] = new ReactionArtBuilder.Paint(1f, 0.7f, 0.2f),
            // Elements 3's wind a mint, its rim teal: the pale dust it was drawn in read as thin white lines in Golden Feathers' gust.
            ["e3_wind"] = new ReactionArtBuilder.Paint(0.4f, 0.78f, 0.6f),
            // Floor combs short dark dashes, never pale lines (the scale notes).
            ["e3_wind_comb"] = new ReactionArtBuilder.Paint(0.16f, 0.16f, 0.2f, 0.55f, 1f, true),
            // Elements 3's own moments (tools/e3mats.py, e3react.py, e3pass2.py): flat colours, and each emission ramp
            // read a third of the way from its centre to its rim, the face the camera sees most of.
            ["e3_bolt"] = new ReactionArtBuilder.Paint(0.340f, 0.725f, 1.000f),
            ["e3_briar"] = new ReactionArtBuilder.Paint(0.130f, 0.329f, 0.051f),
            ["e3_char"] = new ReactionArtBuilder.Paint(0.045f, 0.028f, 0.022f, 0.75f, 1f, true),
            ["e3_crack"] = new ReactionArtBuilder.Paint(0.050f, 0.035f, 0.025f, 0.85f, 1f, true),
            ["e3_dark_rim"] = new ReactionArtBuilder.Paint(0.450f, 0.150f, 0.850f),
            ["e3_dk_b"] = new ReactionArtBuilder.Paint(0.020f, 0.004f, 0.035f),
            ["e3_dk_v"] = new ReactionArtBuilder.Paint(0.360f, 0.090f, 0.720f),
            ["e3_dust_d"] = new ReactionArtBuilder.Paint(0.200f, 0.130f, 0.070f),
            ["e3_dust_l"] = new ReactionArtBuilder.Paint(0.600f, 0.470f, 0.290f),
            ["e3_dust_m"] = new ReactionArtBuilder.Paint(0.400f, 0.290f, 0.160f),
            ["e3_feather2_vane"] = new ReactionArtBuilder.Paint(1.000f, 0.600f, 0.120f),
            ["e3_fire_o"] = new ReactionArtBuilder.Paint(1.000f, 0.400f, 0.040f),
            ["e3_fire_r"] = new ReactionArtBuilder.Paint(0.820f, 0.140f, 0.020f),
            ["e3_fire_y"] = new ReactionArtBuilder.Paint(1.000f, 0.680f, 0.140f),
            ["e3_flameflag"] = new ReactionArtBuilder.Paint(0.989f, 0.421f, 0.049f),
            ["e3_foam"] = new ReactionArtBuilder.Paint(0.500f, 0.840f, 0.940f),
            ["e3_frost_mist"] = new ReactionArtBuilder.Paint(0.360f, 0.600f, 0.860f),
            ["e3_glint"] = new ReactionArtBuilder.Paint(0.800f, 0.860f, 0.930f),
            ["e3_gold_burn"] = new ReactionArtBuilder.Paint(0.420f, 0.200f, 0.020f, 0.7f, 1f, true),
            ["e3_gold_d"] = new ReactionArtBuilder.Paint(0.620f, 0.250f, 0.020f),
            ["e3_gold_edge"] = new ReactionArtBuilder.Paint(1.000f, 0.626f, 0.160f),
            ["e3_gold_l"] = new ReactionArtBuilder.Paint(1.000f, 0.740f, 0.240f),
            ["e3_gold_m"] = new ReactionArtBuilder.Paint(1.000f, 0.500f, 0.060f),
            ["e3_gold_trail"] = new ReactionArtBuilder.Paint(1.000f, 0.573f, 0.124f),
            ["e3_ice_d"] = new ReactionArtBuilder.Paint(0.040f, 0.200f, 0.580f),
            ["e3_ice_l"] = new ReactionArtBuilder.Paint(0.360f, 0.740f, 0.970f),
            ["e3_ice_m"] = new ReactionArtBuilder.Paint(0.090f, 0.460f, 0.900f),
            ["e3_leaf"] = new ReactionArtBuilder.Paint(0.250f, 0.620f, 0.120f),
            ["e3_leaf_d"] = new ReactionArtBuilder.Paint(0.050f, 0.250f, 0.040f),
            ["e3_leaf_l"] = new ReactionArtBuilder.Paint(0.380f, 0.750f, 0.160f),
            ["e3_leaf_m"] = new ReactionArtBuilder.Paint(0.150f, 0.500f, 0.080f),
            ["e3_miasma"] = new ReactionArtBuilder.Paint(0.490f, 0.645f, 0.075f),
            ["e3_miasma_dark"] = new ReactionArtBuilder.Paint(0.264f, 0.391f, 0.046f),
            // The floor's marks lie see-through on it, never as a solid slab with a pale rim (the films of 2026-11-20: Riptide's
            // and water's pools read as white rings, Miasma's stain as a lime slab).
            ["e3_poison_stain"] = new ReactionArtBuilder.Paint(0.170f, 0.280f, 0.025f, 0.6f, 1f, true),
            ["e3_spark"] = new ReactionArtBuilder.Paint(1.000f, 0.580f, 0.140f),
            ["e3_spark_hot"] = new ReactionArtBuilder.Paint(1.000f, 0.800f, 0.400f),
            ["e3_spike"] = new ReactionArtBuilder.Paint(0.188f, 0.555f, 0.910f),
            ["e3_splash"] = new ReactionArtBuilder.Paint(0.053f, 0.371f, 0.738f),
            ["e3_steel"] = new ReactionArtBuilder.Paint(0.660f, 0.708f, 0.782f),
            ["e3_steel_trail"] = new ReactionArtBuilder.Paint(0.339f, 0.385f, 0.464f),
            ["e3_stone"] = new ReactionArtBuilder.Paint(0.500f, 0.370f, 0.240f),
            ["e3_storm"] = new ReactionArtBuilder.Paint(0.072f, 0.085f, 0.164f),
            ["e3_storm_lit"] = new ReactionArtBuilder.Paint(0.295f, 0.505f, 0.965f),
            ["e3_thorn"] = new ReactionArtBuilder.Paint(0.240f, 0.130f, 0.040f),
            ["e3_ven_d"] = new ReactionArtBuilder.Paint(0.100f, 0.250f, 0.020f),
            ["e3_ven_l"] = new ReactionArtBuilder.Paint(0.600f, 0.820f, 0.120f),
            ["e3_ven_m"] = new ReactionArtBuilder.Paint(0.320f, 0.600f, 0.050f),
            ["e3_venom"] = new ReactionArtBuilder.Paint(0.358f, 0.673f, 0.061f),
            ["e3_vine"] = new ReactionArtBuilder.Paint(0.203f, 0.497f, 0.079f),
            ["e3_wat_d"] = new ReactionArtBuilder.Paint(0.020f, 0.130f, 0.420f),
            ["e3_wat_l"] = new ReactionArtBuilder.Paint(0.220f, 0.680f, 0.940f),
            ["e3_wat_m"] = new ReactionArtBuilder.Paint(0.040f, 0.340f, 0.780f),
            ["e3_water"] = new ReactionArtBuilder.Paint(0.044f, 0.322f, 0.749f),
            ["e3_water_trail"] = new ReactionArtBuilder.Paint(0.058f, 0.360f, 0.774f),
            ["e3_wave"] = new ReactionArtBuilder.Paint(0.023f, 0.171f, 0.458f),
            ["e3_wet"] = new ReactionArtBuilder.Paint(0.03f, 0.1f, 0.2f, 0.55f, 1f, true),
            ["e3_wn_l"] = new ReactionArtBuilder.Paint(0.500f, 0.860f, 0.660f),
            ["e3_wn_m"] = new ReactionArtBuilder.Paint(0.220f, 0.700f, 0.500f),
            ["e3_wn_t"] = new ReactionArtBuilder.Paint(0.060f, 0.340f, 0.300f),
            ["e3_feather2_inner"] = new ReactionArtBuilder.Paint(1.000f, 0.820f, 0.420f),
            ["e3_feather2_shaft"] = new ReactionArtBuilder.Paint(0.200f, 0.080f, 0.020f),
            ["e3_feather2_edge"] = new ReactionArtBuilder.Paint(0.680f, 0.280f, 0.030f),
            // Elements 4 and reactions 5 (the third review's pass), from the materials the Blender worker's specs carry
            // (tools/specs_*.gltf, MATS): a flat colour as it is, a toon's lit band, an emission ramp read a third of the way
            // from its centre to its rim; the floor's frost, ripples, live film, glow, steam, sap and pit see-through.
            ["e4_armour_seam"] = new ReactionArtBuilder.Paint(0.950f, 0.520f, 0.100f),
            ["e4_beam"] = new ReactionArtBuilder.Paint(0.995f, 0.604f, 0.119f),
            ["e4_beamcore"] = new ReactionArtBuilder.Paint(1.000f, 0.827f, 0.408f),
            ["e4_blackbolt"] = new ReactionArtBuilder.Paint(0.017f, 0.002f, 0.034f),
            ["e4_bolt_violet"] = new ReactionArtBuilder.Paint(0.560f, 0.217f, 1.000f),
            ["e4_bolt_vivid"] = new ReactionArtBuilder.Paint(0.083f, 0.529f, 1.000f),
            ["e4_crackglow"] = new ReactionArtBuilder.Paint(1.000f, 0.420f, 0.060f),
            ["e4_damp"] = new ReactionArtBuilder.Paint(0.173f, 0.263f, 0.097f),
            ["e4_drain"] = new ReactionArtBuilder.Paint(0.060f, 0.010f, 0.106f),
            ["e4_earth_bed"] = new ReactionArtBuilder.Paint(0.200f, 0.130f, 0.070f),
            ["e4_fang"] = new ReactionArtBuilder.Paint(0.084f, 0.017f, 0.143f),
            ["e4_fish"] = new ReactionArtBuilder.Paint(0.700f, 0.760f, 0.620f),
            ["e4_frost_bed"] = new ReactionArtBuilder.Paint(0.100f, 0.360f, 0.780f, 0.45f, 1f, true),
            ["e4_frost_dark"] = new ReactionArtBuilder.Paint(0.160f, 0.120f, 0.420f, 0.55f, 1f, true),
            ["e4_gilt"] = new ReactionArtBuilder.Paint(0.700f, 0.400f, 0.050f),
            ["e4_glaze"] = new ReactionArtBuilder.Paint(0.067f, 0.480f, 0.965f),
            ["e4_glaze_crystal"] = new ReactionArtBuilder.Paint(0.178f, 0.649f, 1.000f),
            ["e4_gold_glow"] = new ReactionArtBuilder.Paint(1.000f, 0.660f, 0.160f, 0.60f, 1f, true),
            ["e4_hand"] = new ReactionArtBuilder.Paint(0.044f, 0.009f, 0.074f),
            ["e4_ice_dark"] = new ReactionArtBuilder.Paint(0.138f, 0.104f, 0.455f),
            ["e4_ice_glint"] = new ReactionArtBuilder.Paint(0.450f, 0.860f, 1.000f),
            ["e4_iceflame"] = new ReactionArtBuilder.Paint(0.096f, 0.457f, 0.862f),
            ["e4_livefilm"] = new ReactionArtBuilder.Paint(0.020f, 0.120f, 0.360f, 0.85f, 1f, true),
            ["e4_lotus_deep"] = new ReactionArtBuilder.Paint(0.054f, 0.277f, 0.760f),
            ["e4_lotus_icepale"] = new ReactionArtBuilder.Paint(0.415f, 0.761f, 0.987f),
            ["e4_lotus_pale"] = new ReactionArtBuilder.Paint(1.000f, 0.820f, 0.460f),
            ["e4_maw"] = new ReactionArtBuilder.Paint(0.012f, 0.003f, 0.022f),
            ["e4_miasma_green"] = new ReactionArtBuilder.Paint(0.116f, 0.458f, 0.091f),
            ["e4_molten"] = new ReactionArtBuilder.Paint(0.700f, 0.200f, 0.020f),
            ["e4_molten_hot"] = new ReactionArtBuilder.Paint(1.000f, 0.500f, 0.060f),
            ["e4_mote_violet"] = new ReactionArtBuilder.Paint(0.500f, 0.160f, 0.900f),
            ["e4_net"] = new ReactionArtBuilder.Paint(1.000f, 0.720f, 0.200f),
            ["e4_pit"] = new ReactionArtBuilder.Paint(0.090f, 0.060f, 0.035f, 0.92f, 1f, true),
            ["e4_poison_green"] = new ReactionArtBuilder.Paint(0.160f, 0.720f, 0.080f),
            ["e4_pool_deep"] = new ReactionArtBuilder.Paint(0.010f, 0.100f, 0.360f),
            ["e4_pool_light"] = new ReactionArtBuilder.Paint(0.300f, 0.760f, 0.980f),
            ["e4_pool_mid"] = new ReactionArtBuilder.Paint(0.030f, 0.270f, 0.700f),
            ["e4_pool_rim"] = new ReactionArtBuilder.Paint(0.160f, 0.620f, 0.950f),
            ["e4_quick_green"] = new ReactionArtBuilder.Paint(0.151f, 0.669f, 0.101f),
            ["e4_ray"] = new ReactionArtBuilder.Paint(1.000f, 0.660f, 0.140f),
            ["e4_ray_deep"] = new ReactionArtBuilder.Paint(0.980f, 0.460f, 0.050f),
            ["e4_rg_glow"] = new ReactionArtBuilder.Paint(0.420f, 0.920f, 0.300f),
            ["e4_rg_leaf"] = new ReactionArtBuilder.Paint(0.320f, 0.740f, 0.180f),
            ["e4_rg_petal"] = new ReactionArtBuilder.Paint(1.000f, 0.520f, 0.720f),
            ["e4_rg_root"] = new ReactionArtBuilder.Paint(0.360f, 0.620f, 0.160f),
            ["e4_ripple"] = new ReactionArtBuilder.Paint(0.060f, 0.450f, 0.880f, 0.70f, 1f, true),
            ["e4_root_dark"] = new ReactionArtBuilder.Paint(0.220f, 0.130f, 0.060f),
            ["e4_sap"] = new ReactionArtBuilder.Paint(0.549f, 0.534f, 0.043f),
            ["e4_sap_dark"] = new ReactionArtBuilder.Paint(0.160f, 0.200f, 0.020f, 0.80f, 1f, true),
            ["e4_seedpod"] = new ReactionArtBuilder.Paint(0.420f, 0.800f, 0.200f),
            ["e4_spark_cyan"] = new ReactionArtBuilder.Paint(0.200f, 0.750f, 1.000f),
            ["e4_steam"] = new ReactionArtBuilder.Paint(0.446f, 0.526f, 0.626f, 0.55f, 1f, true),
            ["e4_steel_bar"] = new ReactionArtBuilder.Paint(0.475f, 0.517f, 0.597f),
            ["e4_steel_shine"] = new ReactionArtBuilder.Paint(0.820f, 0.860f, 0.920f),
            ["e4_stone_armour"] = new ReactionArtBuilder.Paint(0.460f, 0.360f, 0.260f),
            ["e4_storm"] = new ReactionArtBuilder.Paint(0.040f, 0.047f, 0.104f),
            ["e4_sun"] = new ReactionArtBuilder.Paint(1.000f, 0.743f, 0.267f),
            ["e4_weal"] = new ReactionArtBuilder.Paint(0.620f, 0.780f, 0.100f),
            ["e4_well"] = new ReactionArtBuilder.Paint(0.018f, 0.004f, 0.028f),
            ["e4_wind"] = new ReactionArtBuilder.Paint(0.295f, 0.743f, 0.563f),
            // The briar's buds (the elements 3 kit's copy of the life flower): its petals the render's bud pink.
            ["m_vfx_life_petal"] = new ReactionArtBuilder.Paint(0.95f, 0.4f, 0.62f),
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
            // The Lightning slam's charge forking through the cracks and its sparks, a step darker than the shared bolt: in
            // the slam's tight star they bloomed into a white knot at the game camera (the slam stills of 2026-11-21).
            ["e3_slam_lightning|e4_bolt_vivid"] = new ReactionArtBuilder.Paint(0.03f, 0.26f, 0.6f),
            ["e3_slam_lightning|e4_spark_cyan"] = new ReactionArtBuilder.Paint(0.07f, 0.36f, 0.66f),
            ["e3_slam_lightning@b|e4_spark_cyan"] = new ReactionArtBuilder.Paint(0.07f, 0.36f, 0.66f),
            ["e3_slam_lightning@b|e4_bolt_vivid"] = new ReactionArtBuilder.Paint(0.03f, 0.26f, 0.6f),
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
            // Round 4: Seeded's seeds warm golden-brown with spring-green sprout tips, nothing in Poison's green.
            ["life_seeded|e4_seedpod"] = new ReactionArtBuilder.Paint(0.46f, 0.27f, 0.08f),
            ["e4_seed_sprout"] = new ReactionArtBuilder.Paint(0.1f, 0.72f, 0.36f),
            // Iron Thorns's seeds are Seeded's, in the same colour.
            ["iron_thorns|e4_seedpod"] = new ReactionArtBuilder.Paint(0.46f, 0.27f, 0.08f),
            // Poisoned's stain lay as a flat teal square under the body after Miasma's cloud (its film of 2026-11-20; the
            // audit: "a flat teal square pool"): a dark sick green, see-through.
            ["poison_poisoned|m2_poison_stain"] = new ReactionArtBuilder.Paint(0.12f, 0.2f, 0.03f, 0.5f, 1f, true),
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
            // Frost Heave's frost flower and Mock Suns' frosted sun-wheel lay as a pale near-white star 3 m across in
            // their films (2026-11-20): the same see-through mid blue.
            ["frost_heave|r4_frost_bed"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.8f, 0.45f, 1f, true),
            ["mock_suns|r4_frost_bed"] = new ReactionArtBuilder.Paint(0.12f, 0.36f, 0.8f, 0.45f, 1f, true),
            ["brazier_ice|m_vfx_ice_frost"] = new ReactionArtBuilder.Paint(0.22f, 0.5f, 0.9f, 0.6f, 1f, true),
            ["brazier_ice|m_vfx_ice"] = new ReactionArtBuilder.Paint(0.15f, 0.45f, 1f),

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
            // Its Sundered daggers read as a large pale-white blade over the enemy behind: a darker steel.
            ["anvil_stone|m3_steel_dagger"] = new ReactionArtBuilder.Paint(0.3f, 0.32f, 0.37f),
            // Red Hot's white-hot blade read as a cream-white cross: hot orange, as metal glows.
            ["red_hot|r4_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.45f, 0.08f),
            // Round 4: the sword heats from its steel through a dull red to the orange, not straight to orange.
            ["red_hot|r4_redhot"] = new ReactionArtBuilder.Paint(0.3f, 0.04f, 0.02f),
            // Round 4: Cold Short's frost over the whole sword, the hilt too, in the ice's blue rather than pale steel.
            ["cold_short|r4_frost_steel"] = new ReactionArtBuilder.Paint(0.25f, 0.55f, 1f),
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
            // Earthlight's and Sun-Sap's shafts of light stacked to a cream near white over the bloom: a deeper gold.
            ["earthlight|r4_light_ray"] = new ReactionArtBuilder.Paint(1f, 0.66f, 0.24f, 0.14f, 1f, true),
            ["sun_sap|r4_light_ray"] = new ReactionArtBuilder.Paint(1f, 0.66f, 0.24f, 0.14f, 1f, true),
            ["sickle_wind|r4_wind_inner"] = new ReactionArtBuilder.Paint(0.27f, 0.3f, 0.28f),
            ["sickle_wind|m3_wind_dust"] = new ReactionArtBuilder.Paint(0.42f, 0.38f, 0.3f),
            ["earthlight|m_vfx_earth_rock"] = new ReactionArtBuilder.Paint(0.3f, 0.23f, 0.16f),
            ["the_grudge|r4_grudgecloud"] = new ReactionArtBuilder.Paint(0.07f, 0.05f, 0.12f),
            ["the_grudge|r4_grudgelit"] = new ReactionArtBuilder.Paint(0.55f, 0.3f, 1f),
            ["fish_poison|r4_splash"] = new ReactionArtBuilder.Paint(0.05f, 0.34f, 0.5f),
            ["caught_light|r4_splash"] = new ReactionArtBuilder.Paint(0.05f, 0.34f, 0.5f),
            // The polished films (the 06:30 export): Quicksilver's mercury beads lit as white balls in the metal solid's
            // shine (a darker quicksilver grey), Kindling Point's white-hot flare as a cream-white burst (a hot gold).
            ["quicksilver|r4_mercury"] = new ReactionArtBuilder.Paint(0.4f, 0.43f, 0.48f),
            ["kindling_point|r4_whitehot"] = new ReactionArtBuilder.Paint(1f, 0.6f, 0.16f),
            // The Bolt Runs True's straight bolt drew as a pure white slab across the frame: a pale violet-blue bolt.
            ["the_bolt_runs_true|m3_bolt"] = new ReactionArtBuilder.Paint(0.42f, 0.5f, 1f),
            // The audit of 2026-11-20 against the Blender worker's r4 strips. Rust ("The rust is not a rusty color at all"):
            // the linear paints lit to a pale tan (RGB 167, 117, 67 on film); a deep red-brown rust, its flakes a little
            // brighter, its stain darker.
            ["rust|m3_rust"] = new ReactionArtBuilder.Paint(0.26f, 0.045f, 0.012f),
            ["rust|m3_rust_flake"] = new ReactionArtBuilder.Paint(0.42f, 0.09f, 0.02f),
            ["rust|m3_rust_dark"] = new ReactionArtBuilder.Paint(0.09f, 0.022f, 0.008f, 0.85f, 1f, true),
            ["rust|r4_rustwater"] = new ReactionArtBuilder.Paint(0.2f, 0.045f, 0.014f),
            // The Ground Gives ("the ground frames really need like a splash outward ... to give more of a feel of water"):
            // the splash off the rim was brown mud and tan pebbles; it is water.
            ["the_ground_gives|r4_mudwater"] = new ReactionArtBuilder.Paint(0.05f, 0.36f, 0.72f),
            // Root Snare: the dust burst was a pale tan slab over the body (a thin see-through puff now), and the spores out
            // of the crack a cream lost on the floor (a sick spore green-gold).
            ["root_snare|m3_earth_dust"] = new ReactionArtBuilder.Paint(0.36f, 0.29f, 0.2f, 0.4f, 1f, true),
            ["root_snare|m3_spore"] = new ReactionArtBuilder.Paint(0.5f, 0.62f, 0.12f),
            // Flashover's violet flame lit a pale pink-lilac on film: a deep violet with a paler violet heart.
            ["flashover|r4_plasmaflame"] = new ReactionArtBuilder.Paint(0.3f, 0.05f, 0.85f),
            // Metal's bite glint, a cold steel blue at a moderate brightness, not white (the swing films of 2026-11-20).
            ["e3_metal_blade_hit1|e3_glint"] = new ReactionArtBuilder.Paint(0.36f, 0.44f, 0.58f),
            ["e3_metal_blade_hit2|e3_glint"] = new ReactionArtBuilder.Paint(0.36f, 0.44f, 0.58f),
            ["e3_metal_blade_hit3|e3_glint"] = new ReactionArtBuilder.Paint(0.36f, 0.44f, 0.58f),
            ["flashover|r4_plasmacore"] = new ReactionArtBuilder.Paint(0.55f, 0.28f, 1f),
            // Quickening ("they should be much bluer to really get the water feel"): a deep blue, not a cyan.
            ["quickening|r4_waterplant"] = new ReactionArtBuilder.Paint(0.025f, 0.16f, 0.8f),
            ["quickening|r4_waterpetal"] = new ReactionArtBuilder.Paint(0.04f, 0.22f, 1f),
            // Thunder Gust ("it needs to be more like a lightning tornado"): the funnel's bands a dark storm slate, so the
            // bright arcs crawling up it are what is seen, not pale streaks.
            ["thunder_gust|r4_stormband"] = new ReactionArtBuilder.Paint(0.15f, 0.16f, 0.3f),
            ["thunder_gust|r4_stormband_dark"] = new ReactionArtBuilder.Paint(0.05f, 0.05f, 0.11f),
            ["thunder_gust|r4_dustband"] = new ReactionArtBuilder.Paint(0.26f, 0.25f, 0.32f),
            // Lightning Flower ("like a blue purple flower not pink"): its petals lit near white; a deep blue-violet.
            ["lightning_flower|r4_boltpetal"] = new ReactionArtBuilder.Paint(0.17f, 0.08f, 0.85f),
            // Drowned Weight ("the water should be super dark, pretty much black"): blacker still.
            ["drowned_weight|r4_blackwater"] = new ReactionArtBuilder.Paint(0.012f, 0.015f, 0.035f),
            // The Grudge's bolt and Breath Taken's wind: near black, so the rim alone carries them.
            ["the_grudge|r4_blackbolt"] = new ReactionArtBuilder.Paint(0.025f, 0.01f, 0.045f),
            ["breath_taken|r4_blackwind"] = new ReactionArtBuilder.Paint(0.03f, 0.02f, 0.045f),
            // Its breath torn out: a dim grey-blue puff, not a white one.
            ["breath_taken|r4_breath"] = new ReactionArtBuilder.Paint(0.3f, 0.33f, 0.4f, 0.55f, 1f, true),
            // Root Snare's torn turf read as bright green hexagon chunks round the burst: a dark mossy earth.
            ["root_snare|r4_moss"] = new ReactionArtBuilder.Paint(0.1f, 0.13f, 0.04f),
            // Rot Roots' pustules were teal beads: swollen sick yellow-green.
            ["rot_roots|m_vfx_poison_venom"] = new ReactionArtBuilder.Paint(0.42f, 0.5f, 0.04f),
            ["black_chain|r4_chain"] = new ReactionArtBuilder.Paint(0.014f, 0.012f, 0.018f),
            ["girdled|r4_figroot"] = new ReactionArtBuilder.Paint(0.03f, 0.018f, 0.022f),
            // Earthlight: the light out of the cracks a deep gold, strong enough to be seen pouring up.
            ["earthlight|r4_light_ray"] = new ReactionArtBuilder.Paint(1f, 0.58f, 0.12f, 0.32f, 1f, true),
            // Quicksilver ("no silver mercury is visible"): the beads a bright quicksilver, never white.
            ["quicksilver|r4_mercury"] = new ReactionArtBuilder.Paint(0.52f, 0.56f, 0.62f),
            // Reactions 5's films (2026-11-20): Fish Poison's slick and Burning Sap's pool lay as a light olive slab and Ill
            // Wind's sickened lane as a lime one on the dark floor (as Miasma's stain once did); the slick dark green-black and
            // the pool and the lane a deep see-through sick green, as notes.md draws them.
            ["fish_poison|e4_sap_dark"] = new ReactionArtBuilder.Paint(0.025f, 0.05f, 0.015f, 0.85f, 1f, true),
            ["sun_sap|e4_sap_dark"] = new ReactionArtBuilder.Paint(0.07f, 0.08f, 0.01f, 0.8f, 1f, true),
            ["ill_wind|e3_poison_stain"] = new ReactionArtBuilder.Paint(0.05f, 0.09f, 0.012f, 0.55f, 1f, true),        };

        /// <summary>
        /// Pieces one look lifts, by "slug|kit object", metres: The Grudge's cloud hung at 2.3 m, right behind the enemy's
        /// health bar, and was not seen at all from the game camera; it gathers higher, clear of the bar, and its bolts
        /// are stretched up to reach it (<see cref="RibbonLifts"/>).
        /// </summary>
        public static readonly Dictionary<string, float> LookLifts = new Dictionary<string, float>
        {
            ["the_grudge|vfxe2_cloud_a"] = 1.05f,
            ["the_grudge|vfxe2_cloud_b"] = 1.05f,
        };

        /// <summary>
        /// Ribbons one look stretches upward, by slug: a ribbon reaching above the first height has its points lifted by up
        /// to the second, nothing at its foot and all of it at its top, so a bolt still leaves the lifted cloud and still
        /// lands on the body.
        /// </summary>
        public static readonly Dictionary<string, Vector2> RibbonLifts = new Dictionary<string, Vector2>
        {
            ["the_grudge"] = new Vector2(1.9f, 1.05f),
        };

        /// <summary>Pieces one look draws at another size, by "slug|kit object": Seeded's seeds were large for a castle soldier (notes.md).</summary>
        public static readonly Dictionary<string, float> LookScales = new Dictionary<string, float>
        {
            ["life_seeded|vfxe2_seed"] = 0.65f,
            // The Metal slam's one glint is the light kit's charged glint (KitAliases), larger than the elements 3 star
            // the render drew: at the recipe's size it stood over the sword's point as a star wider than a soldier.
            ["e3_slam_metal|vfxe3_glint"] = 0.45f,
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
            // Iron Thorns' iron roots and the thorns through the armour read as thin grey wires from the game camera (its
            // film of 2026-11-20): larger, so the iron growth is what is seen.
            ["iron_thorns|vfxr4_vfx2c_life_metal_ironroots"] = 1.4f,
            ["iron_thorns|vfxr4_vfx2c_life_metal_ironthorns"] = 1.4f,
            // Stolen Warmth's wisp was a single orange dot at the game camera (the Blender worker's notes: "small at 12 m"):
            // half as large again (twice was a ball over the player), still one wisp a blow so the screen stays quiet.
            ["stolen_warmth|vfxr4_orb"] = 1.5f,
            // The audit of 2026-11-20 against the r4 strips, on the castle's soldiers. Rust: the rust segments lay inside the
            // blade and it stayed plain steel; they case it now. Quicksilver's poison-green segments likewise.
            ["rust|vfxe2_sword_rust0"] = 1.1f, ["rust|vfxe2_sword_rust1"] = 1.1f, ["rust|vfxe2_sword_rust2"] = 1.1f,
            ["rust|vfxe2_sword_rust3"] = 1.1f, ["rust|vfxe2_sword_rust4"] = 1.1f,
            ["quicksilver|vfxe2_sword_rust0"] = 1.1f, ["quicksilver|vfxe2_sword_rust1"] = 1.1f, ["quicksilver|vfxe2_sword_rust2"] = 1.1f,
            ["quicksilver|vfxe2_sword_rust3"] = 1.1f, ["quicksilver|vfxe2_sword_rust4"] = 1.1f,
            ["quicksilver|vfxr4_c3k_bead"] = 1.7f,
            // Root Snare ("brown roots should grow out of the ground and snare the legs ... a large crack on the ground that
            // spores come out of"): thicker roots up the legs, the crack twice as large, the spores larger, the dust smaller.
            ["root_snare|vfx3c_vfx_life_vine_b"] = 1.5f, ["root_snare|vfx3c_vfx_life_vine_a"] = 1.5f,
            // (Its spores are gone in reactions 5: the spore mesh is Seeded's circling seeds, kept at their own size.)
            ["root_snare|vfx3c_vfx_earth_crack2"] = 1.5f, ["root_snare|vfx3c_vfx_earth_dust"] = 0.7f,
            ["seed_storm|vfx3c_vfx_life_vine_b"] = 1.5f, ["seed_storm|vfx3c_vfx_life_vine_a"] = 1.5f,
            ["rot_roots|vfx3c_vfx_life_vine_b"] = 1.4f, ["rot_roots|vfx3c_vfx_life_vine_a"] = 1.4f, ["rot_roots|vfxr4_vfx_poison_bubble"] = 1.8f,
            // The Ground Gives: its splash crowns and drops big enough to read as water thrown out off the rim.
            ["the_ground_gives|vfx3c_vfx_water_drop"] = 1.5f, ["the_ground_gives|vfx3c_vfx_water_crown"] = 2.4f,
            ["shining_herb|vfxr4_vfx_life_sapling"] = 1.3f, ["shining_herb|vfxr4_vfx_life_flower"] = 1.3f,
            // Winter Seed ("the blooming flower should be a ice flower with ice thorns and not a pink flower overshadowed by
            // massive ice thorns"): the flower twice the size, the thorns smaller round it.
            // Reactions 5 drew the lotus larger itself (1.7 against 1.5) and ringed it with big hooked thorns: half as much again
            // keeps it the largest thing there without burying the thorns.
            ["winter_seed|vfxr4_iceflower"] = 1.5f, ["winter_seed|vfxe2_crystal"] = 1.3f, ["winter_seed|vfx3c_vfx_life_thorn"] = 0.7f,
            ["lightning_flower|vfxr4_vfx_life_flower"] = 1.4f,
            ["golden_feathers|vfxr4_feather2_vane"] = 0.8f,
            ["golden_feathers|vfxe3_feather"] = 1.4f,
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
            // The audit of 2026-11-20 on the castle's soldiers: roots thick enough to be seen gripping the legs; Breath
            // Taken's black wind broad bands; the arcs of Live Iron, Thunder Gust and Flashover, The Grudge's black bolt
            // and Earthlight's glowing cracks thick enough to read from the game camera.
            // Reactions 5 drew Split Rock's and Windthrow's roots thick (7 to 14 cm), Girdled's fig roots and The Grudge's
            // crawl at their own weight: a little more for the game camera, not the earlier doubling.
            ["root_snare|r4_root"] = 1.2f,
            ["seed_storm|r4_root"] = 1.4f,
            ["rot_roots|r4_rotroot"] = 2f,
            ["breath_taken|r4_blackwind"] = 1.9f,
            ["live_iron|m3_bolt"] = 1.5f,
            ["thunder_gust|m3_bolt"] = 1.8f,
            ["thunder_gust|r4_bolt_bright"] = 1.8f,
            ["flashover|m3_bolt"] = 1.6f,
            ["the_grudge|r4_blackbolt"] = 1.5f,
            ["earthlight|r4_light_ray"] = 3f,
            ["girdled|r4_figroot"] = 1.2f,
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
            // Then the audit of 2026-11-20: Black Chain's chain "dark with a bright violet glow ... reads purple, not pure
            // black" ("it should be a chain in pure black"): a dark grey-violet edge only, enough to part it from the floor.
            ["r4_chain"] = new Color(0.3f, 0.22f, 0.36f),
            ["r4_blackbolt"] = new Color(0.62f, 0.32f, 1f),
            // Girdled's roots: a bark-dark edge, not a violet one (the violet rims read as wind round them).
            ["r4_figroot"] = new Color(0.26f, 0.13f, 0.2f),
            ["e3_ice_trail"] = new Color(0.05f, 0.26f, 0.66f),
            ["e3_dark_trail"] = new Color(0.46f, 0.16f, 0.88f),
            ["e3_wind"] = new Color(0.06f, 0.32f, 0.28f),
            // Elements 4 and reactions 5: wind's teal rim, the bolts' violet fringe, the dark's bright violet edge (the
            // Blender worker's rims), the net's deep gold edge, the roots a dark green.
            ["e4_wind"] = new Color(0.06f, 0.32f, 0.28f),
            ["e4_bolt_vivid"] = new Color(0.42f, 0.2f, 0.95f),
            ["e4_bolt_violet"] = new Color(0.3f, 0.05f, 0.75f),
            ["e4_drain"] = new Color(0.55f, 0.2f, 0.95f),
            ["e4_blackbolt"] = new Color(0.62f, 0.32f, 1f),
            ["e4_fang"] = new Color(0.6f, 0.22f, 1f),
            ["e4_net"] = new Color(0.55f, 0.3f, 0.05f),
            ["e4_rg_root"] = new Color(0.1f, 0.26f, 0.05f),
            ["e4_crackglow"] = new Color(0.45f, 0.12f, 0.02f),
            ["e4_pool_rim"] = new Color(0.03f, 0.2f, 0.5f),
            ["e4_frost_bed"] = new Color(0.05f, 0.2f, 0.55f),
        };

        /// <summary>
        /// The widest a ribbon's ink may be, by material, where it is wider than the usual 2.5 cm: The Grudge's black bolt,
        /// near black over the dark floor and the cloud, was seen only as a sliver under the cloud; a broad bright violet
        /// rim carries it from the cloud down into the body ("the black bolt doesnt connect to the cloud", 2026-11-19).
        /// </summary>
        public static readonly Dictionary<string, float> RibbonInkMost = new Dictionary<string, float>
        {
            ["r4_blackbolt"] = 0.09f,
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
            // Elements 4: Marked's and Clarity's motes drifting down the beam, Glaze's glints over the ice and its mist.
            "drift down inside", "runs over the ice", "sinks off you",
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
            // Golden Feathers' feathers: in the light solid the pale inner vane lit as a white blob half a metre long
            // (its film of 2026-11-20); fire's solid keeps the gold.
            ["r4_feather_gold"] = "fire",
            ["r4_feather_pale"] = "fire",
            ["r4_feather_shaft"] = "fire",
            // Quicksilver's mercury beads: round, they caught the metal solid's chrome glint whole and lit white; in
            // earth's matte solid they are grey beads.
            ["r4_mercury"] = "earth",
            // Elements 3's ice in earth's matte solid: in the ice solid the tops of Frost Bite's chunks lit white, and in the
            // water solid's shine its 2.4 m spikes read as white streaks through the bodies (the films of 2026-11-20).
            ["e3_ice_tooth"] = "earth",
            ["e3_spike"] = "earth",
            ["e3_ice_d"] = "earth",
            ["e3_ice_m"] = "earth",
            ["e3_ice_l"] = "earth",
            ["e3_ice_trail"] = "earth",
            // Elements 4 and reactions 5: the new ice in earth's matte solid too (no white tops), the gold light (beams, suns,
            // rays, the net) in fire's solid that keeps its gold, the dark (fangs, hands, the black bolt, the drain) in the
            // dark solid for its violet ink, the steel bar matte so it reads as steel and never as a white chrome flash.
            ["e4_glaze_crystal"] = "earth", ["e4_ice_dark"] = "earth", ["e4_iceflame"] = "earth", ["e4_lotus_deep"] = "earth",
            ["e4_lotus_icepale"] = "earth", ["e4_ice_glint"] = "earth", ["e4_steel_bar"] = "earth", ["e4_stone_armour"] = "earth",
            ["e4_beam"] = "fire", ["e4_beamcore"] = "fire", ["e4_sun"] = "fire", ["e4_ray"] = "fire", ["e4_ray_deep"] = "fire",
            ["e4_net"] = "fire", ["e4_lotus_pale"] = "fire", ["e4_gold_glow"] = "fire",
            ["e4_fang"] = "dark", ["e4_hand"] = "dark", ["e4_maw"] = "dark", ["e4_blackbolt"] = "dark", ["e4_drain"] = "dark", ["e4_well"] = "dark",
            ["e4_seedpod"] = "life", ["e4_seed_sprout"] = "life", ["e4_rg_leaf"] = "life", ["e4_rg_petal"] = "life", ["e4_rg_root"] = "life",
            // The slam's sword in earth's matte solid, so its steel blade, bronze guard and leather grip keep their own
            // colours and the blade never catches the metal solid's chrome glint as a white flash.
            ["st_sl_blade"] = "earth", ["st_sl_guard"] = "earth", ["st_sl_grip"] = "earth",
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
            // The foot a hot yellow-orange, not cream: Red Hot's blast read as "cream and yellow cone spikes" (the audit of
            // 2026-11-20; the owner: fire must look like flame, not white or gold light).
            new Color(1f, 0.62f, 0.08f), new Color(1f, 0.38f, 0.025f), new Color(0.95f, 0.15f, 0.012f), new Color(0.6f, 0.04f, 0.01f),
        };
        public static readonly float[] FlameBandFrom = { 0f, 0.24f, 0.5f, 0.78f };

        /// <summary>
        /// The Blender worker's elements 3 pass (staging/blender/2026-11-20_elements3), which redesigns four reactions
        /// (each look marked revision_of: r4_slug). The ones built from it so far; the rest keep combos 4's look.
        /// </summary>
        public const string RevisionRecipePath = ReactionArtBuilder.CombosFolder + "/elements3_recipe.json";
        public static readonly HashSet<string> RevisedReactions = new HashSet<string> { "rime", "smoulder", "breath_taken", "golden_feathers" };

        // The redesigned reactions' looks by slug, from the elements 3 recipe when it is in the project.
        private static Dictionary<string, Dictionary<string, object>> Revisions()
        {
            var found = new Dictionary<string, Dictionary<string, object>>();
            var recipe = AssetDatabase.LoadAssetAtPath<TextAsset>(RevisionRecipePath);
            if (recipe == null) return found;
            foreach (Dictionary<string, object> look in (List<object>)((Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text))["looks"])
            {
                string slug = Text(look, "slug");
                if (look.ContainsKey("revision_of") && RevisedReactions.Contains(slug)) found[slug] = look;
            }
            return found;
        }

        /// <summary>
        /// Kit pieces of the elements 3 kit (not brought into the project) drawn with the same piece of an earlier kit:
        /// Smoulder's coal-glow disc is the brazier and combos 4 disc.
        /// </summary>
        public static readonly Dictionary<string, (string kit, string kitObject)> KitAliases = new Dictionary<string, (string, string)>
        {
            ["vfx_elements_kit_e3.fbx|vfxe3_disc"] = ("vfx_combo_kit_r4.fbx", "vfxr4_disc"),
            // Golden Feathers' feather is drawn whole by FeatherMesh (from the combos 4 vane, scaled to the 1.4 m feather),
            // its small gold flare on each enemy by Light's glint.
            ["vfx_elements_kit_e3.fbx|vfxe3_feather"] = ("vfx_combo_kit_r4.fbx", "vfxr4_feather2_vane"),
            ["vfx_elements_kit_e3.fbx|vfxe3_glint"] = ("vfx_light_kit_v2.fbx", "vfx2_light_glint_charged"),
        };

        private static readonly StringBuilder Report = new StringBuilder();
        private static readonly HashSet<string> Unknown = new HashSet<string>();
        private static readonly Dictionary<Material, Material> ReactionSolids = new Dictionary<Material, Material>();

        /// <summary>
        /// The solid a reaction's piece is drawn in: its element's solid with no white sheen on the lit faces and no white
        /// glint on the tops. The Blender worker's reactions are flat bands of their paint; in the element solids every
        /// face toward the light took a quarter more of its paint plus up to 0.42 of white, so the reactions came out
        /// paler than drawn (the audit of 2026-11-20: Lightning Flower's blue-violet lavender-white, Rime's ice white).
        /// The element looks keep their own solids.
        /// </summary>
        // A reaction solid inked in its own colour, made once a build under the solid's name and the ink's.
        private static Material ReactionSolid(Material solid, Color ink)
        {
            var plain = ReactionSolid(solid);
            if (plain == null) return null;
            string path = $"{ElementArtBuilder.Folder}/m_reaction_{solid.name.Replace("m_element_", "")}_ink_{ColorUtility.ToHtmlStringRGB(ink).ToLowerInvariant()}.mat";
            var inked = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (inked == null)
            {
                inked = new Material(plain);
                AssetDatabase.CreateAsset(inked, path);
            }
            else inked.CopyPropertiesFromMaterial(plain);
            inked.SetColor("_OutlineColor", ink);
            EditorUtility.SetDirty(inked);
            return inked;
        }

        private static Material ReactionSolid(Material solid)
        {
            if (solid == null) return null;
            if (ReactionSolids.TryGetValue(solid, out var made)) return made;
            string path = $"{ElementArtBuilder.Folder}/m_reaction_{solid.name.Replace("m_element_", "")}.mat";
            made = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (made == null)
            {
                made = new Material(solid);
                AssetDatabase.CreateAsset(made, path);
            }
            else
            {
                made.shader = solid.shader;
                made.CopyPropertiesFromMaterial(solid);
            }
            made.SetFloat("_Sheen", 0f);
            made.SetFloat("_Bands4", 0f);
            EditorUtility.SetDirty(made);
            ReactionSolids[solid] = made;
            return made;
        }

        [MenuItem("Clover/Build/Element Looks")]
        public static void BuildMenu() => Build();

        [MenuItem("Clover/Build/Brazier Looks")]
        public static void BuildBraziersMenu() => BuildBraziers();

        /// <summary>Whether the elements 3 blade sheaths (Fire's flame flags, Ice's teeth, Earth's spine...) are built: not since 2026-11-20.</summary>
        public const bool BladeSheathsWorn = false;

        public static string Build() => BuildSet(RecipePath, SetPath, "review/checks/element_looks.md", "Element looks (batches 1 and 2)", true);

        /// <summary>The key of a reaction's look on another body (n of the recipe's bodies), or on the player (n below nought).</summary>
        public static string ReactionKey(string slug, int body) => body == 0 ? slug : body < 0 ? slug + "@p" : slug + "@" + body;

        /// <summary>
        /// Why a recipe piece is built as no mesh of its look, or null when it is one: a piece the look draws its own way
        /// (<see cref="LeftOut"/>), a shell or second skin the body's own material draws (the mock-ups, Soaked's sheet,
        /// Turned to Stone's stone), Soaked's foam rings, Temper's rings and clang (its shader's front). Every other
        /// piece the recipe gives a body is built on that body's look (ElementLooksTests).
        /// </summary>
        public static string NotAMesh(string slug, string kit, string kitObject, string material = null, Vector3 fromItsBody = default, string piece = null, bool redrawn = false)
        {
            // A piece of the recipe the look is now built from (reactions 5, elements 4) is judged by what it is, not by its
            // reaction having been redrawn.
            if (redrawn && material != null)
            {
                string why = NotAMesh(slug, kit, kitObject, null, fromItsBody, piece);
                if (why != null) return why;
                if (kit == Mockup && ThawedReactions.Contains(slug)) return "a frost shell left off (it lit the body near white)";
                if (IsBodyFlame(slug, kitObject, material, fromItsBody)) return "the body's own real flame (BodyFlames) burns there";
                if (IsLodgedSeed(kitObject, material)) return "a lodged seed, read as a green hexagon chunk";
                return null;
            }
            if (LeftOut.Contains(slug + "|" + kitObject) || LeftOutByName(slug, piece)) return "drawn by the look its own way";
            if (E4BodyDrawn.Contains(slug + "|" + kitObject)) return "drawn on the player's own body (StoneArmour) or left with the stand-in enemies";
            if (kit == Mockup && material != null && ThawedReactions.Contains(slug)) return "a frost shell left off (it lit the body near white)";
            if (IsBodyFlame(slug, kitObject, material, fromItsBody)) return "the body's own real flame (BodyFlames) burns there";
            if (IsLodgedSeed(kitObject, material)) return "a lodged seed, read as a green hexagon chunk";
            if (material != null && Reactions5Built.Contains(slug) && Reactions5In()) return "the reaction is built from its reactions 5 redesign";
            if (material != null && RevisedReactions.Contains(slug) && AssetDatabase.LoadAssetAtPath<TextAsset>(RevisionRecipePath) != null)
                return "the reaction is built from its elements 3 redesign";
            if (kit == Mockup || kit == "enemy_model") return "a shell the body's own material draws";
            if (slug == "water_soaked" && kitObject.Contains("wet_affliction")) return "Soaked's sheen, drawn by the body's own material";
            if (slug == "water_soaked" && kitObject.Contains("water_ripple")) return "Soaked's foam rings, left out";
            if (slug == "metal_temper" && (kitObject.Contains("metal_ring") || kitObject.Contains("clang"))) return "Temper's front, drawn by its shader";
            return null;
        }

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
            public readonly List<ElementLookSet.Span> Spans = new List<ElementLookSet.Span>();
            public ElementLookSet Target;
            /// <summary>Building reactions: pieces split by the body they are on, the player's apart.</summary>
            public bool Reactions;
            /// <summary>Building the elements 3 moments: solids and ribbons in the reaction solids, without the white sheen.</summary>
            public bool Flat;
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
            ReactionSolids.Clear();
            var lists = new Lists { Target = set };
            // The reactions' looks join the element looks, played by the same pool.
            if (elementLooks)
            {
                var reactions = AssetDatabase.LoadAssetAtPath<TextAsset>(ReactionRecipePath);
                if (reactions != null)
                {
                    lists.Reactions = true;
                    var revised = Revisions();
                    var redrawn = Reactions5();
                    foreach (Dictionary<string, object> combos4Look in (List<object>)((Dictionary<string, object>)ReactionRecipe.ReadJson(reactions.text))["looks"])
                    {
                        string slug = Text(combos4Look, "slug");
                        if (!ReactionsBuilt.Contains(slug)) continue;
                        // A reaction the Blender worker redrew for the third review (reactions 5) is built from that recipe,
                        // else one it redesigned in elements 3 from that one.
                        var look = redrawn.TryGetValue(slug, out var third) ? third : revised.TryGetValue(slug, out var revision) ? revision : combos4Look;
                        // Round 4's placements (ElementLookBuilder.Round4): blades through the body, thorns shrinking, feathers settling.
                        look = Round4Placement(look, slug);
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
            foreach (Dictionary<string, object> recipeLook in looks)
            {
                var look = recipeLook;
                string slug = Text(look, "slug");
                if (slug == "brazier_lineup") continue;
                // Six braziers are elements 4's (Lightning's caged storm with more detail, the drop falling into the water...),
                // and so are thirteen of the element looks (the third review's asks).
                if (!elementLooks && Elements3Brazier(slug) is Dictionary<string, object> caged) look = caged;
                if (elementLooks && Elements4For(slug) is Dictionary<string, object> redrawnLook) look = redrawnLook;
                if (elementLooks) look = Round4Placement(look, slug);
                // Drawn as the Blender worker's flat bands, without the solid's sheen (its dark storm lit grey on film).
                lists.Flat = look != recipeLook;
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
            // The element looks elements 4 adds: Marked's one beam, Glaze's frost and ward, Bedrock's grit about the armour.
            if (elementLooks)
                foreach (string key in E4NewLooks)
                {
                    var look = Elements4For(key);
                    if (look == null) continue;
                    lists.Flat = true;
                    var bodyAt = new List<Vector3>();
                    foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodyAt.Add(Vec(body["position"], Vector3.zero));
                    BuildLook(look, key, key, 0, bodyAt, 0f, lists, elementArt, glow);
                }
            lists.Flat = false;
            // Each element's own moments drawn by the elements 3 pass: the blades' blows, the spells, the strikes, Tailwind.
            if (elementLooks) BuildElements3(lists, elementArt, glow);
            set.Looks = lists.Looks.ToArray();
            set.Parts = lists.Parts.ToArray();
            set.Keys = lists.Keys.ToArray();
            // No element stands on the blade any more (the owner, 2026-11-20: "take out all the elements on top of the blade
            // effects ... the burning flames on the sword, the iceicles on the sides"): the sheaths are not built, and the
            // weapon keeps only its element-dyed accent and inlay (WeaponRack) and the slash trails.
            if (elementLooks) set.Sheaths = BladeSheathsWorn ? BuildSheaths(lists, elementArt) : new ElementLookSet.BladeSheath[0];
            if (elementLooks) set.Spans = lists.Spans.ToArray();
            if (elementLooks) BakeCrystal(set, lists, elementArt);
            if (elementLooks) BakeArmourPlate(set, lists, elementArt);
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
            // A reaction whose doing outlasts its look keeps it standing on the body struck as long (ReactionHolds).
            if (lists.Reactions && (onBody == 0 || onBody > 0 && HeldOnEveryBody.Contains(slug)) && ReactionHolds.TryGetValue(slug, out float hold)) made.Hold = hold;
            else if (lists.Reactions && onBody >= 0) made.Hold = AftermathHold;
            // The way the recipe's player faced: toward the main body.
            if (look.TryGetValue("player", out var playerValue) && playerValue is Dictionary<string, object> player && bodyAt.Count > 0)
            {
                Vector3 toward = bodyAt[0] - Vec(player["position"], Vector3.zero);
                made.FacingYaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
            }
            AddRaise(look, slug, onBody, shift, lists, ref made);
            made.FacesCamera = CameraFacing.Contains(key);
            var shellTimes = new List<(float start, float grow)>();
            float bodyFlameFrom = float.MaxValue;
            bool bodyFlameBlack = false;
            int ribbonsMade = 0;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                string kit = Text(group, "kit"), kitObject = Text(group, "kit_object"), material = Text(group, "material");
                // Marked in a reaction is a small gold flare on the body, not its sun-wheel (the Blender worker's scale notes:
                // "The Marked mandala must not be a reaction's biggest shape"; Earthlight's film of 2026-11-20 rang a gold
                // wheel round every head): the wheel's place and time, drawn as light's four-pointed glint.
                bool markedFlare = lists.Reactions && kitObject == MarkedWheel && material == "m2_light_gold";
                if (markedFlare) kitObject = MarkedFlare;
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
                    Vector3 turned = Vec(instance[2], Vector3.zero);
                    string pieceName = instance[0] as string ?? "";
                    if (LeftOut.Contains(slug + "|" + kitObject) || LeftOutByName(slug, pieceName)) continue;
                    if (E4BodyDrawn.Contains(slug + "|" + kitObject)) continue;
                    // Glaze's ice and Bedrock's stone skin are the player's own material (GlazeIce, StoneArmour).
                    if (kit == Mockup && (slug == "ice_glaze" || slug == "earth_bedrock")) continue;
                    // The body the recipe gives the piece, read before it is restaged (its key pose says where it stood).
                    int pieceBody = bodyAt.Count > 0 ? PieceBody(motion, position, bodyAt) : 0;
                    if (lists.Reactions && IsLodgedSeed(kitObject, material)) continue;
                    if (lists.Reactions && IsBodyFlame(slug, kitObject, material, position - (pieceBody >= 0 && pieceBody < bodyAt.Count ? bodyAt[pieceBody] : Vector3.zero)))
                    {
                        // This body burns in the real flame field from when its first tongue would have shown.
                        if (!playerVariant && pieceBody == onBody)
                        {
                            bodyFlameFrom = Mathf.Min(bodyFlameFrom, Window(motion).x - shift);
                            bodyFlameBlack |= material.StartsWith("r4_blackflame");
                        }
                        continue;
                    }
                    if (lists.Reactions && pieceBody == 0 && LookOffsets.TryGetValue(slug + "|" + kitObject, out var moved)) position += moved;
                    if (pieceBody == 0) Restage(slug, pieceName, bodyAt.Count > 0 ? bodyAt[0] - playerAt : Vector3.forward, ref position, ref turned, ref motion);
                    if (lists.Reactions)
                    {
                        // A reaction's piece belongs to the player's look or a body's, never to both.
                        bool onThePlayer = onPlayer || placementHere == "p";
                        if (onThePlayer != playerVariant) continue;
                        if (playerVariant) onPlayer = true;
                    }
                    made.OnPlayer |= onPlayer;
                    // The look's own body's pieces only (or the player's): an element look's other two bodies show the same
                    // timeline again. A look with no bodies (a brazier's) keeps every piece.
                    motion.TryGetValue("orbit", out var orbitValue);
                    if (!onPlayer && bodyAt.Count > 0 && pieceBody != onBody) continue;
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
                    // Winter Seed's frost crust lit the body a pale near white (its film of 2026-11-20): no shell.
                    if (kit == Mockup && lists.Reactions && ThawedReactions.Contains(slug)) continue;
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
                            // Stolen Warmth's ice darker with a violet hue (the owner, 2026-11-20), reactions 5's e4_ice_dark.
                            : material == "e4_ice_dark" ? ElementLookSet.DarkIce
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
                    var rotation = Quaternion.Euler(turned);
                    var scale = Vec(instance[3], Vector3.one);
                    if (LookScales.TryGetValue(slug + "|" + kitObject, out float resize)) scale *= resize;
                    if (markedFlare) scale *= MarkedFlareSize;
                    if (LookStretch.TryGetValue(slug + "|" + kitObject, out var stretched)) scale = Vector3.Scale(scale, stretched);
                    if (LookLifts.TryGetValue(slug + "|" + kitObject, out float lift)) position.y += lift;
                    if (LookPaints.ContainsKey(slug + "|" + material)) material = slug + "|" + material;
                    string placement = instance.Count > 4 ? instance[4] as string : "b";
                    string verb = Text(motion, "verb");
                    var mesh = Bake(key, lists.Parts.Count - made.FirstPart, pieceName, filter, comboTurn, material, element, scale, placement == "g", out byte kind, out Material wears, elementArt, glow, lists.Target);
                    if ((lists.Reactions || lists.Flat) && kind == ElementLookSet.Solid)
                        wears = SolidInks.TryGetValue(slug + "|" + material.Substring(material.IndexOf('|') + 1), out var ownInk) ? ReactionSolid(wears, ownInk) : ReactionSolid(wears);
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
                        Facing = RibbonFacing.TryGetValue(slug + "|" + Text(group, "material"), out float pieceFacing) ? pieceFacing : 0f,
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

            var ribbons = new List<object>();
            if (look.TryGetValue("ribbons", out var ribbonValue) && ribbonValue is List<object> recipeRibbons) ribbons.AddRange(recipeRibbons);
            if (lists.Reactions && onBody == 0 && ChainSpans.TryGetValue(slug, out var span)) lists.Spans.Add(MakeSpan(slug, element, span, elementArt, lists.Target));
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
                    string ribbonKey = slug + "|" + material;
                    if (LeftOutRibbons.Contains(ribbonKey) || LeftOutByName(slug, Text(ribbon, "object"))) continue;
                    string note = Text(motion, "note");
                    // Unsteady's curl coming in from the blow lit as a white crescent beside the body it was laid on (Thunder
                    // Gust's film of 2026-11-20, 0.05 to 0.10 s): drawn in the elements 3 wind's mint with its teal rim.
                    if (material == "m3_wind_ribbon" && note.Contains("comes in from the blow")) material = "e3_wind";
                    var points = new List<Vector3>();
                    foreach (var point in (List<object>)ribbon["points"]) points.Add(Vec(point, Vector3.zero));
                    if (lists.Reactions) Reshape(ribbonKey, points);
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
                    bool inked = !(ribbon.TryGetValue("ink", out var inkValue) && inkValue is bool ink && !ink);
                    var paint = RibbonDress(slug, material, element, width, inked, elementArt, out var solid, out var inkColour, out float inkWidth, out float thin);
                    if (lists.Reactions || lists.Flat) solid = ReactionSolid(solid);
                    string meshName = $"look_{key}_{lists.Parts.Count - made.FirstPart:00}_{Text(ribbon, "object")}";
                    var mesh = ChainLinks.TryGetValue(slug + "|" + material, out var link)
                        ? ChainMesh(meshName, points, link.x, link.y, link.z, paint.Colour)
                        : RibbonMesh(meshName, points, width, Text(ribbon, "radius_profile"), taper, jagged, through, paint.Colour);
                    if (RibbonGradients.TryGetValue(ribbonKey, out var gradient)) Grade(mesh, gradient.tail, gradient.head);
                    AssetDatabase.AddObjectToAsset(mesh, lists.Target);
                    var part = new ElementLookSet.Part
                    {
                        Mesh = mesh, Material = solid,
                        Kind = ElementLookSet.Ribbon, Follows = true,
                        // What comes again and again while the look is kept: the curls, the arcs; not the wind arriving,
                        // nor a strike's own shapes (played once).
                        Repeats = hide < 9f && !note.Contains("comes in from the blow") && slug != "wind_strike",
                        // A ribbon cut out of its look's frame (an elements 3 moment) keeps its points about its own centre,
                        // set where that centre stood.
                        Position = ribbon.TryGetValue("offset", out var offset) ? Vec(offset, Vector3.zero) : Vector3.zero,
                        Rotation = Quaternion.identity, Scale = Vector3.one, Show = show, Hide = hide,
                        InkColour = inkColour, InkWidth = inkWidth, Thin = thin,
                        Facing = RibbonFacing.TryGetValue(ribbonKey, out float facing) ? facing : 0f,
                    };
                    part.PathFirst = lists.Keys.Count;
                    part.PathCount = AddKeys(motion, "path", lists.Keys, false, shift);
                    part.DrawFirst = lists.Keys.Count;
                    part.DrawCount = AddKeys(motion, "draw_on", lists.Keys, false, shift);
                    part.EraseFirst = lists.Keys.Count;
                    float linger = Linger(slug, Text(ribbon, "object"));
                    var fade = Fade(slug, Text(ribbon, "object"));
                    if (fade.y > 0f)
                    {
                        // Wiped from its tail between the fade's two times, and gone at its end.
                        lists.Keys.Add(new ElementLookSet.Key { Time = fade.x - shift, Value = Vector3.zero, Ease = ElementLookSet.Linear });
                        lists.Keys.Add(new ElementLookSet.Key { Time = fade.y - shift, Value = Vector3.one, Ease = ElementLookSet.In });
                        part.EraseCount = 2;
                        part.Hide = Mathf.Min(part.Hide, fade.y - shift + 0.02f);
                    }
                    else part.EraseCount = AddKeys(motion, "erase", lists.Keys, false, shift - linger);
                    if (linger > 0f && part.Hide < 9f) part.Hide += linger;
                    if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit)
                    {
                        // Its points lie about its centre, so it turns about its own origin: degrees from its start on,
                        // a steady rate (Unity's sense, as the recipe's angles are); a rise climbs it as it turns.
                        float start = Number(orbit, "start", show + shift) - shift;
                        float end = hide >= 9f ? seconds : hide;
                        float degrees = 360f * Number(orbit, "turns_per_second", 0f) * Mathf.Max(0.01f, end - start);
                        part.OrbitCentre = part.Position;
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

            if (lists.Reactions && onBody == 0) AddHealPulse(slug, key, lists, made, glow);
            made.PartCount = lists.Parts.Count - made.FirstPart;
            if (lists.Reactions && ThawedReactions.Contains(slug)) made.Thawed = true;
            if (bodyFlameFrom < float.MaxValue)
            {
                made.BodyFlame = bodyFlameBlack ? ElementLookSet.BlackFlame : ElementLookSet.Fire;
                made.BodyFlameFrom = bodyFlameFrom;
            }
            if (shellTimes.Count > 0)
            {
                shellTimes.Sort((a, b) => a.start.CompareTo(b.start));
                made.ShellBands = shellTimes.Count;
                made.ShellStart = shellTimes[0].start;
                made.ShellStep = shellTimes.Count > 1 ? (shellTimes[shellTimes.Count - 1].start - shellTimes[0].start) / (shellTimes.Count - 1) : 0.1f;
                made.ShellGrow = shellTimes[0].grow;
                if (lists.Reactions && ShellTimes.TryGetValue(slug, out var own))
                {
                    made.ShellStart = own.x;
                    made.ShellStep = own.y;
                    made.ShellGrow = own.z;
                    made.ShellUntil = own.w;
                }
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

        // A reaction's ribbon reshaped before it is made (its points about its body): one winding close round the body
        // laid out round the real enemy's armour, one carried further out, one heaved up off the floor at its middle.
        private static void Reshape(string ribbonKey, List<Vector3> points)
        {
            if (points.Count < 2) return;
            float widest = 0f, low = float.MaxValue, high = float.MinValue;
            foreach (var p in points)
            {
                widest = Mathf.Max(widest, new Vector2(p.x, p.z).magnitude);
                low = Mathf.Min(low, p.y);
                high = Mathf.Max(high, p.y);
            }
            if (Gnarled.Contains(ribbonKey))
            {
                // Knocked about by a few centimetres, the same way each build, keeping both ends where they were.
                var knock = new System.Random(points.Count * 7919 + ribbonKey.Length);
                for (int i = 1; i < points.Count - 1; i++)
                    points[i] += new Vector3((float)knock.NextDouble() - 0.5f, ((float)knock.NextDouble() - 0.5f) * 0.6f, (float)knock.NextDouble() - 0.5f) * 0.09f;
            }
            float outward = widest < WrapRadius && high - low > 0.3f ? WrapOut : 1f;
            if (RibbonReach.TryGetValue(ribbonKey, out float reach)) outward *= reach;
            RibbonHumps.TryGetValue(ribbonKey, out float hump);
            bool lying = high < 0.15f;
            float total = 0f;
            for (int i = 1; i < points.Count; i++) total += Vector3.Distance(points[i - 1], points[i]);
            float along = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0) along += Vector3.Distance(points[i - 1], points[i]);
                var p = points[i];
                p.x *= outward;
                p.z *= outward;
                // Only a long runner along the floor is heaved: its middle up, its two ends still in the ground.
                if (hump > 0f && lying && total > 0.8f) p.y += hump * Mathf.Sin(Mathf.PI * along / total);
                points[i] = p;
            }
        }

        // A ribbon's paint from its tail (u 0) to its head (u 1).
        private static void Grade(Mesh mesh, Color tail, Color head)
        {
            var uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            var colours = new Color[uvs.Count];
            for (int i = 0; i < uvs.Count; i++)
            {
                var c = Color.Lerp(tail, head, Mathf.SmoothStep(0f, 1f, uvs[i].x));
                colours[i] = new Color(c.r, c.g, c.b, 1f);
            }
            mesh.colors = colours;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }

        /// <summary>
        /// A ribbon's dress, the same for the recipe's ribbons and a live span: its paint (a look's own where it has one,
        /// The Bolt Runs True's bolt), the solid it is drawn in (SolidOf first: the curse's chain in the dark solid), and
        /// its ink's colour, width and stipple.
        /// </summary>
        private static ReactionArtBuilder.Paint RibbonDress(string slug, string material, string element, float width, bool inked, ElementArtSet elementArt,
            out Material solid, out Color inkColour, out float inkWidth, out float thin)
        {
            var paint = PaintOf(LookPaints.ContainsKey(slug + "|" + material) ? slug + "|" + material : material);
            string owner = SolidOf.TryGetValue(material, out string ribbonSolid) ? ribbonSolid : ReactionArtBuilder.ElementOf(material, element + "_" + element);
            int ring = ReactionDesigns.RingIndex(owner);
            solid = ring >= 0 && ring < elementArt.Solids.Length ? elementArt.Solids[ring] : elementArt.Solids[0];
            inkColour = RibbonInks.TryGetValue(material, out var colour) ? colour : elementArt.Solids[Mathf.Max(0, ring)].GetColor("_OutlineColor");
            inkWidth = inked ? Mathf.Clamp(width * 0.16f, 0.008f, RibbonInkMost.TryGetValue(material, out float most) ? most : 0.025f) : 0f;
            thin = RibbonThin.TryGetValue(material, out float stipple) ? stipple : 0f;
            return paint;
        }

        /// <summary>The chain's material in the recipe (Black Chain's wraps wear it too), the width its ribbon was drawn at, and how far inside each body's wrap its ends sit.</summary>
        public const string SpanMaterial = "r4_chain";
        public const float SpanWidth = 0.2f, SpanInset = 0.3f;

        /// <summary>
        /// A reaction's live chain (<see cref="ChainSpans"/>): one link of the size its built chain had
        /// (<see cref="ChainLinks"/>), in the chain ribbon's paint, solid and ink, and the span's shape and times.
        /// </summary>
        private static ElementLookSet.Span MakeSpan(string slug, string element, (int body, float height, float arc, float from, float reached) span, ElementArtSet elementArt, Object into)
        {
            var link = ChainLinks.TryGetValue(slug + "|" + SpanMaterial, out var size) ? size : new Vector3(0.34f, 0.2f, 0.045f);
            var paint = RibbonDress(slug, SpanMaterial, element, SpanWidth, true, elementArt, out var solid, out var inkColour, out float inkWidth, out float thin);
            solid = ReactionSolid(solid);
            var mesh = LinkMesh($"look_{slug}_span_link", link.x, link.y, link.z, paint.Colour);
            AssetDatabase.AddObjectToAsset(mesh, into);
            return new ElementLookSet.Span
            {
                Key = slug, Body = span.body, Condition = SpanConditions.TryGetValue(slug, out string condition) ? condition : "",
                Link = mesh, Material = solid, InkColour = inkColour, InkWidth = inkWidth, Thin = thin,
                // Links overlap by their bar at each end, as a chain's do (as ChainMesh lays them).
                Pitch = Mathf.Max(0.05f, link.x - 2f * link.z), Height = span.height, Arc = span.arc, Inset = SpanInset, From = span.from, Reached = span.reached,
            };
        }

        /// <summary>The condition the game sets on the body a span binds (Reactions.Near: Black Chain's on the nearest enemy within 6 m), by slug.</summary>
        public static readonly Dictionary<string, string> SpanConditions = new Dictionary<string, string>
        {
            ["black_chain"] = Clover.Player.Trees.Reactions.Chain,
        };

        /// <summary>
        /// One oval link about its middle, its length along z and its width across x, lying flat (its face up y), as
        /// <see cref="ChainMesh"/> draws each: u half way along for Clover/Spell's draw clip (a live link is drawn whole
        /// or not at all), uv1 the normals the ink is pushed along.
        /// </summary>
        public static Mesh LinkMesh(string name, float length, float width, float iron, Color colour)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colours = new List<Color>();
            var triangles = new List<int>();
            AddLink(vertices, normals, uvs, colours, triangles, Vector3.zero, Vector3.forward, Vector3.right, length, width, iron, colour, _ => 0.5f);
            return FinishChain(name, vertices, normals, uvs, colours, triangles);
        }

        // One link's bar swept round its oval, appended: about its centre, its length along tangent, its width along
        // wide; u for each vertex from where it lies.
        private static void AddLink(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<Color> colours, List<int> triangles,
            Vector3 centre, Vector3 tangent, Vector3 wide, float length, float width, float iron, Color colour, System.Func<Vector3, float> uOf)
        {
            const int Around = 14;
            float halfLong = length * 0.5f - iron, halfWide = width * 0.5f - iron;
            Vector3 facing = Vector3.Cross(tangent, wide).normalized;
            int first = vertices.Count;
            for (int j = 0; j < Around; j++)
            {
                float angle = j * Mathf.PI * 2f / Around;
                float c = Mathf.Cos(angle), n = Mathf.Sin(angle);
                Vector3 onBar = centre + tangent * (halfLong * c) + wide * (halfWide * n);
                Vector3 barWay = (tangent * (-halfLong * n) + wide * (halfWide * c)).normalized;
                Vector3 outward = Vector3.Cross(barWay, facing).normalized;
                for (int s = 0; s < RibbonSides; s++)
                {
                    float turn = s * Mathf.PI * 2f / RibbonSides;
                    Vector3 normal = (outward * Mathf.Cos(turn) + facing * Mathf.Sin(turn)).normalized;
                    Vector3 vertex = onBar + normal * iron;
                    vertices.Add(vertex);
                    normals.Add(normal);
                    uvs.Add(new Vector2(uOf(vertex), s / (float)RibbonSides));
                    colours.Add(new Color(colour.r, colour.g, colour.b, 1f));
                }
            }
            for (int j = 0; j < Around; j++)
                for (int s = 0; s < RibbonSides; s++)
                {
                    int a = first + j * RibbonSides + s, b = first + j * RibbonSides + (s + 1) % RibbonSides;
                    int c = first + (j + 1) % Around * RibbonSides + s, d = first + (j + 1) % Around * RibbonSides + (s + 1) % RibbonSides;
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(b); triangles.Add(c); triangles.Add(d);
                }
        }

        // Faces turned out, as a ribbon's are, so the ink's back faces are inside; then the mesh.
        private static Mesh FinishChain(string name, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<Color> colours, List<int> triangles)
        {
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
                    (triangles[t + 1], triangles[t + 2]) = (triangles[t + 2], triangles[t + 1]);
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

        /// <summary>
        /// A chain of oval links along a ribbon's points (metres about its centre), each link turned a quarter about the
        /// line from the one before, as iron links hang: <paramref name="length"/> and <paramref name="width"/> the link's
        /// outside, <paramref name="iron"/> the thickness of its bar. Laid out as a ribbon is (u from the tail, 0, to the
        /// head, 1, by length, for Clover/Spell's draw clip; uv1 the normals the ink is pushed along), so it whips out and
        /// carries its rim as the recipe's ribbons do.
        /// </summary>
        public static Mesh ChainMesh(string name, List<Vector3> points, float length, float width, float iron, Color colour)
        {
            if (points.Count < 2) points = new List<Vector3> { Vector3.zero, Vector3.up * 0.1f };
            var line = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p0 = points[Mathf.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Mathf.Min(points.Count - 1, i + 2)];
                for (int s = 0; s < RibbonSteps * 2; s++) line.Add(CatmullRom(p0, p1, p2, p3, s / (float)(RibbonSteps * 2)));
            }
            line.Add(points[points.Count - 1]);
            var along = new float[line.Count];
            for (int i = 1; i < line.Count; i++) along[i] = along[i - 1] + Vector3.Distance(line[i - 1], line[i]);
            float total = Mathf.Max(0.0001f, along[line.Count - 1]);
            // Links overlap by their bar at each end, as a chain's do.
            float pitch = Mathf.Max(0.05f, length - 2f * iron);
            int links = Mathf.Max(2, Mathf.RoundToInt(total / pitch));
            float step = total / links;
            var vertices = new List<Vector3>(links * 14 * RibbonSides);
            var normals = new List<Vector3>(vertices.Capacity);
            var uvs = new List<Vector2>(vertices.Capacity);
            var colours = new List<Color>(vertices.Capacity);
            var triangles = new List<int>(links * 14 * RibbonSides * 6);
            int segment = 0;
            for (int k = 0; k < links; k++)
            {
                float at = (k + 0.5f) * step;
                while (segment < line.Count - 2 && along[segment + 1] < at) segment++;
                float share = Mathf.InverseLerp(along[segment], along[segment + 1], at);
                Vector3 centre = Vector3.Lerp(line[segment], line[segment + 1], share);
                Vector3 tangent = (line[segment + 1] - line[segment]).normalized;
                if (tangent.sqrMagnitude < 0.5f) tangent = Vector3.forward;
                Vector3 side = Vector3.Cross(Vector3.up, tangent);
                if (side.sqrMagnitude < 0.0001f) side = Vector3.Cross(Vector3.forward, tangent);
                side.Normalize();
                Vector3 up = Vector3.Cross(tangent, side).normalized;
                // Every other link stands across the one before it.
                Vector3 wide = k % 2 == 0 ? side : up;
                AddLink(vertices, normals, uvs, colours, triangles, centre, tangent, wide, length, width, iron, colour,
                    vertex => Mathf.Clamp01((at + Vector3.Dot(vertex - centre, tangent)) / total));
            }
            return FinishChain(name, vertices, normals, uvs, colours, triangles);
        }

        /// <summary>
        /// Anvil Stone (the owner, 2026-11-19: "it just appears out of nowhere, it is better to have it spring up from the
        /// ground, and have the anvil in the center of the effect not one side"): the recipe heaved the anvil up 0.78 m
        /// behind the enemy in 0.2 s, its length along the player's line, while the cracks and dust stayed under the
        /// enemy's feet. Here the anvil stands across the line, right at the enemy's back (<see cref="AnvilBack"/>), so the
        /// enemy stands against its middle; it heaves up from under the floor in two jolts; and the cracks, the dust, the
        /// thrown stones and the ring of sparks all spread from it. Its pieces are the struck body's (the names' numbers,
        /// "spark15", "heavechip4", are not bodies: read as such, all but one spark and one stone went astray).
        /// </summary>
        public const float AnvilBack = 0.22f, AnvilWasBack = 0.78f, AnvilSink = 0.95f;

        // When a ribbon begins to fade and is gone (RibbonFades), by a part of its name; zero for none.
        private static Vector2 Fade(string slug, string ribbonObject)
        {
            foreach (var pair in RibbonFades)
            {
                int bar = pair.Key.IndexOf('|');
                if (pair.Key.Substring(0, bar) == slug && ribbonObject.Contains(pair.Key.Substring(bar + 1))) return pair.Value;
            }
            return Vector2.zero;
        }

        // Seconds a reaction's ribbon is kept on longer (RibbonLinger), by a part of its name.
        private static float Linger(string slug, string ribbonObject)
        {
            foreach (var pair in RibbonLinger)
            {
                int bar = pair.Key.IndexOf('|');
                if (pair.Key.Substring(0, bar) == slug && ribbonObject.Contains(pair.Key.Substring(bar + 1))) return pair.Value;
            }
            return 0f;
        }

        // A piece of the struck body moved, turned or timed anew for its look, in the recipe's space (away: from the
        // recipe's player through the struck body).
        private static void Restage(string slug, string piece, Vector3 away, ref Vector3 position, ref Vector3 turned, ref Dictionary<string, object> motion)
        {
            // Reactions 5's anvil heaves up right under the enemy and lifts it (the owner: "the anvil goes through the person
            // instead of pushing them up"): it is drawn where it was drawn, never moved behind.
            if (slug != "anvil_stone" || Reactions5In()) return;
            away.y = 0f;
            away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.forward;
            Vector3 anvil = away * AnvilBack;
            if (piece.EndsWith("_anvil"))
            {
                position = new Vector3(anvil.x, -AnvilSink, anvil.z);
                turned.y += 90f;
                List<object> Rise(double time, double up, string ease) => new List<object> { time, new List<object> { 0.0, up, 0.0 }, ease };
                // Its top breaks the floor and catches, then the whole stone heaves out, overshoots and settles.
                motion = new Dictionary<string, object>(motion)
                {
                    ["path"] = new List<object>
                    {
                        Rise(0.18, 0.0, "linear"), Rise(0.3, 0.42, "out"), Rise(0.38, 0.4, "inout"), Rise(0.52, AnvilSink + 0.12, "out"), Rise(0.6, AnvilSink, "inout"),
                    },
                };
                return;
            }
            if (piece.Contains("heavechip"))
            {
                position += anvil - away * AnvilWasBack;
                return;
            }
            if (piece.Contains("crack") || piece.Contains("dustheave") || piece.Contains("spark"))
            {
                position = new Vector3(anvil.x, position.y, anvil.z);
                return;
            }
        }

        /// <summary>
        /// A baked piece's mesh name: its look, its place in the look, its kit object and, after a tilde, the recipe's own
        /// name for the piece, so a check can tell which piece went into which look (<see cref="PieceOf"/>).
        /// </summary>
        public static string PartName(string look, int index, string kitObject, string piece) =>
            string.IsNullOrEmpty(piece) ? $"look_{look}_{index:00}_{kitObject}" : $"look_{look}_{index:00}_{kitObject}~{piece}";

        /// <summary>The recipe's name for the piece a baked mesh was made from; null for a ribbon's or one named before.</summary>
        public static string PieceOf(Mesh mesh)
        {
            if (mesh == null) return null;
            int at = mesh.name.IndexOf('~');
            return at >= 0 ? mesh.name.Substring(at + 1) : null;
        }

        /// <summary>
        /// The body a recipe gives a piece (an index into its bodies, 0 the main one). The recipe says so outright: the
        /// combos 4 recipe in motion.body (written from the Blender pass's own specs by AgentScripts/recipe_bodies.py),
        /// either recipe in follow_body (the body whose motion it rides) and in its orbit's centre_is ("body 2"). A piece
        /// with none of these (the elements recipe's still pieces) is the body's it stands nearest, by its orbit's centre
        /// when it has one. The numbers in a piece's name are never read: "spark15", "heavechip4", "crystalring10_0" and
        /// "blast60_3" are counts and angles, not bodies (read as bodies, they dropped pieces or put them on another enemy).
        /// </summary>
        public static int PieceBody(Dictionary<string, object> motion, Vector3 position, List<Vector3> bodies)
        {
            if (bodies == null || bodies.Count == 0) return 0;
            if (motion != null)
            {
                if (motion.TryGetValue("body", out var body) && body is double given) return (int)given;
                if (motion.TryGetValue("follow_body", out var follow) && follow is double followed) return (int)followed;
                if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit)
                {
                    if (orbit.TryGetValue("centre_is", out var centreIs) && centreIs is string named && CentreBody(named) >= 0) return CentreBody(named);
                    if (orbit.TryGetValue("centre", out var centre)) position = Vec(centre, position);
                }
            }
            return Nearest(bodies, position);
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
            if (KitAliases.TryGetValue(kit + "|" + kitObject, out var alias)) (kit, kitObject) = alias;
            string path = kit == E2Kit || kit == BrazierKit || kit == E3Kit || kit == E4Kit || kit == R5Kit || kit == StrikesKit || kit == SlamKit ? ReactionArtBuilder.CombosFolder + "/" + kit : ReactionArtBuilder.KitPath(kit);
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
        private static Mesh Bake(string slug, int index, string piece, MeshFilter filter, bool comboTurn, string material, string element, Vector3 scale, bool onGround,
            out byte kind, out Material wears, ElementArtSet elementArt, Material glow, Object into)
        {
            string bare = material.Substring(material.IndexOf('|') + 1);
            // A brazier's bed is drawn in its own colour, flat (the solid's bright top facet turned the beds grey), and
            // keeps its own paint where the look repaints other pieces of the same material (Light's gilded bed).
            bool bed = filter.name == BedPiece;
            // (A reaction's disc on the floor, Ill Wind's sickened lane, keeps the paint its look gives it.)
            if (bed && slug.StartsWith("brazier")) material = bare;
            if (filter.name == LeafPiece && LeafColours.TryGetValue(bare, out var leafColour))
                return BakeLeaf(slug, index, piece, filter, comboTurn, element, leafColour, out kind, out wears, elementArt, into);
            if (filter.name == FeatherPiece)
            {
                var vane = KitVertices(filter, comboTurn);
                var size = new Bounds(vane[0], Vector3.zero);
                foreach (var v in vane) size.Encapsulate(v);
                var feather = FeatherMesh(PartName(slug, index, filter.name + "_whole", piece), size.size.y, size.size.x);
                AssetDatabase.AddObjectToAsset(feather, into);
                kind = ElementLookSet.Solid;
                int fireRing = ReactionDesigns.RingIndex("fire");
                wears = fireRing >= 0 && fireRing < elementArt.Solids.Length ? elementArt.Solids[fireRing] : elementArt.Solids[0];
                return feather;
            }
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
            bool perSlot = source.subMeshCount > 1 && (filter.sharedMesh.name.StartsWith("vfxe2_") && material.Substring(material.IndexOf('|') + 1).StartsWith("m3_")
                // The elements 3 pieces of several paints: the feather's vane, edge, inner band and shaft; the briar's thorns.
                || filter.sharedMesh.name.StartsWith("vfxe3_")
                // The slam's sword: its blade, bronze guard and leather grip.
                || filter.sharedMesh.name.StartsWith("vfxsl_"));
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
            var mesh = new Mesh { name = PartName(slug, index, filter.name, piece) };
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

        /// <summary>Golden Feathers' vane, drawn whole as a feather (<see cref="FeatherMesh"/>).</summary>
        public const string FeatherPiece = "vfxr4_feather2_vane";

        /// <summary>
        /// A flight feather as long and as wide as the kit's vane, along y from its base, across x, drawn from both sides
        /// in flat bands: a dark quill raised along its middle from the base to the tip, a light gold inner vane either side
        /// of it, a deeper gold outer vane cut into barbs that sweep toward the tip, a notch in the broad side, and a bare
        /// stub of quill below the vane. The owner (2026-11-20): "The feathers dont really look like feathers they look like
        /// wheat"; the kit's three flat parts in one plane read as yellow strips on film.
        /// </summary>
        public static Mesh FeatherMesh(string name, float length, float width)
        {
            const int Stations = 22;
            var quill = new Color(0.4f, 0.19f, 0.04f);
            var inner = new Color(1f, 0.7f, 0.2f);
            var outer = new Color(0.85f, 0.42f, 0.05f);
            var barb = new Color(0.55f, 0.24f, 0.03f);
            float q = 0.05f * width;
            var vertices = new List<Vector3>();
            var colours = new List<Color>();
            var triangles = new List<int>();
            // The points across the feather at a share u of its length: left edge, left inner, quill's left, ridge,
            // quill's right, right inner, right edge; the vane sweeps toward the tip as it goes out from the quill.
            Vector3[] Across(float u)
            {
                float open = u < 0.13f ? 0f : Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * Mathf.Pow((u - 0.13f) / 0.87f, 0.7f))), 0.55f);
                float left = q + 0.32f * width * open, right = q + 0.62f * width * open;
                // The notch on the broad side.
                right = q + (right - q) * (1f - 0.6f * Mathf.Max(0f, 1f - Mathf.Abs(u - 0.6f) / 0.05f));
                float[] xs = { -left, -(q + 0.4f * (left - q)), -q, 0f, q, q + 0.4f * (right - q), right };
                var row = new Vector3[xs.Length];
                for (int k = 0; k < xs.Length; k++)
                {
                    float sweep = 0.16f * length * Mathf.Abs(xs[k]) / width;
                    row[k] = new Vector3(xs[k], Mathf.Min(length, u * length + sweep), k == 3 ? 0.06f * width : 0f);
                }
                return row;
            }
            for (int s = 0; s < Stations - 1; s++)
            {
                float u0 = s / (float)(Stations - 1), u1 = (s + 1) / (float)(Stations - 1);
                var a = Across(u0);
                var b = Across(u1);
                for (int k = 0; k < a.Length - 1; k++)
                {
                    bool shaft = k == 2 || k == 3;
                    bool outside = k == 0 || k == 5;
                    var colour = shaft ? quill : outside ? (s % 3 == 1 ? barb : outer) : inner;
                    if (!shaft && u0 < 0.13f) continue;
                    foreach (int side in new[] { 1, -1 })
                    {
                        int first = vertices.Count;
                        vertices.Add(a[k]);
                        vertices.Add(b[k]);
                        vertices.Add(a[k + 1]);
                        vertices.Add(b[k + 1]);
                        for (int c = 0; c < 4; c++) colours.Add(colour);
                        if (side > 0) triangles.AddRange(new[] { first, first + 1, first + 2, first + 2, first + 1, first + 3 });
                        else triangles.AddRange(new[] { first, first + 2, first + 1, first + 2, first + 3, first + 1 });
                    }
                }
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

        private static Mesh BakeLeaf(string slug, int index, string piece, MeshFilter filter, bool comboTurn, string element, Color colour,
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
            var mesh = LeafMesh(PartName(slug, index, filter.name + "_leaf", piece), bounds.min, bounds.max, along, across, through, colour);
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
            if (material != null && StrikePaints.TryGetValue(material, out paint)) return paint;
            if (material != null && SlamPaints.TryGetValue(material, out paint)) return paint;
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
