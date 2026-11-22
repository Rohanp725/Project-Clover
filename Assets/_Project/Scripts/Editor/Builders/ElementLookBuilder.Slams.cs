using System.Collections.Generic;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Great Hammer's infused strike as the Blender worker drew it (staging/blender/2026-11-21_strikes/slam, round 5b):
    /// for each element a hammered-down, star-cracked patch of its ground 2.5 m ahead with one hero piece bursting from
    /// its centre. Cut like the arc strikes (ElementLookBuilder.Strikes.cs): what is made once per enemy the
    /// slam reaches (motion.per_struck_enemy) goes on the struck body's look, "e3_slam_dark@b", so a miss draws none of
    /// it; the rest goes on "e3_slam_dark", kept about the impact point (the recipe's origin), which
    /// <see cref="ElementMoves.PlaySlam"/> plays where the head lands.
    ///
    /// Times stay the recipe's: t 0 is the head landing. Lightning's bolt strikes the raised head before that, so its
    /// look begins earlier by its lead (<see cref="ElementLookSet.Look.Lead"/>) and is started that much before the blow.
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public const string SlamRecipePath = ReactionArtBuilder.CombosFolder + "/slam_recipe.json";
        public const string SlamKit = "vfx_slam_kit.fbx";

        /// <summary>
        /// The slam recipe's own materials, in linear colour from its specs (tools/slam_specs.py). The flat and emissive
        /// ones are drawn flat; a ramp's colour is taken a quarter of the way out from its core. The toon ones (the thorn
        /// and the sword's blade, guard and grip) are lit solids with the ink. Dark's smoke and Poison's venom are solids
        /// too, so each carries its rim as the solid's ink (violet, sickly green: <see cref="SolidInks"/>). None is white.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> SlamPaints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            ["st_sl_earth_patch"] = Flat(0.13f, 0.09f, 0.055f), ["st_sl_earth_in"] = Flat(0.08f, 0.055f, 0.035f),
            ["st_sl_ice_patch"] = Flat(0.1f, 0.27f, 0.52f), ["st_sl_ice_in"] = Flat(0.06f, 0.18f, 0.42f),
            ["st_sl_fire_in"] = Flat(0.28f, 0.05f, 0.01f),
            ["st_sl_water_in"] = Flat(0.02f, 0.1f, 0.24f),
            ["st_sl_scorch"] = Flat(0.03f, 0.03f, 0.045f), ["st_sl_scorch_in"] = Flat(0.018f, 0.02f, 0.04f),
            ["st_sl_steel_patch"] = Flat(0.1f, 0.105f, 0.12f), ["st_sl_steel_in"] = Flat(0.06f, 0.065f, 0.08f),
            ["st_sl_soil"] = Flat(0.09f, 0.065f, 0.035f), ["st_sl_soil_in"] = Flat(0.055f, 0.04f, 0.022f),
            ["st_sl_void"] = Flat(0.004f, 0.002f, 0.008f), ["st_sl_dark_patch"] = Flat(0.014f, 0.008f, 0.022f),
            ["st_sl_steel_crack"] = Flat(0.2f, 0.23f, 0.29f),
            ["st_sl_venom_in"] = Flat(0.004f, 0.012f, 0.006f), ["st_sl_venom_patch"] = Flat(0.03f, 0.045f, 0.035f),
            ["st_sl_gilt"] = Flat(0.24f, 0.11f, 0.015f), ["st_sl_gilt_in"] = Flat(0.42f, 0.2f, 0.03f),
            ["st_sl_dust"] = Flat(0.3f, 0.28f, 0.22f), ["st_sl_dust_in"] = Flat(0.22f, 0.21f, 0.17f),
            // The landing's flash a step darker than its spec: drawn flat in the glow it bloomed to a white blob at the game
            // camera (the slam stills of 2026-11-21); the strip's is cyan.
            ["st_sl_core_cyan"] = Flat(0.1f, 0.34f, 0.62f),
            ["st_sl_pit"] = Flat(0f, 0f, 0f),
            // The sword's one glint a step darker too, as the arc strike's st_glint: at the spec it bloomed to a white star.
            ["st_sl_glint"] = Flat(0.2f, 0.3f, 0.48f),
            ["st_sl_bleed"] = Flat(0.5f, 0.24f, 0.02f),
            ["st_sl_beam"] = Flat(0.99f, 0.545f, 0.095f),
            ["st_sl_column"] = Flat(0.95f, 0.485f, 0.065f),
            // The blade darker than its spec: lit by earth's matte solid it read pale grey beside the render's blue-grey steel.
            ["st_sl_blade"] = new ReactionArtBuilder.Paint(0.22f, 0.26f, 0.34f),
            ["st_sl_guard"] = new ReactionArtBuilder.Paint(0.34f, 0.25f, 0.12f),
            ["st_sl_grip"] = new ReactionArtBuilder.Paint(0.13f, 0.075f, 0.05f),
            // The thorn darker than its spec: lit by the life solid it read a pale lime beside the render's deep green.
            ["st_sl_thorn"] = new ReactionArtBuilder.Paint(0.08f, 0.17f, 0.03f),
            ["st_sl_shadow_smoke"] = new ReactionArtBuilder.Paint(0.018f, 0.004f, 0.033f),
            ["st_sl_venom"] = new ReactionArtBuilder.Paint(0.014f, 0.053f, 0.021f),
            // The spin's own (spin/tools/spin_specs.py) and the dash's vein web: dark at a vessel's middle with a sickly-green
            // rim, drawn as one mid green so the web reads at the game camera (dash/notes.md, Poison).
            ["st_sp_dust"] = Flat(0.26f, 0.2f, 0.13f), ["st_sp_fire"] = Flat(0.95f, 0.32f, 0.03f), ["st_sp_char"] = Flat(0.035f, 0.028f, 0.025f),
            ["st_sp_vein_rim"] = new ReactionArtBuilder.Paint(0.06f, 0.2f, 0.06f),
        };

        private static ReactionArtBuilder.Paint Flat(float r, float g, float b) => new ReactionArtBuilder.Paint(r, g, b, 1f, 1f, true);

        /// <summary>The slam recipe's looks (all eleven approved), their revision_of dropped.</summary>
        private static List<Dictionary<string, object>> SlamLooks()
        {
            var looks = new List<Dictionary<string, object>>();
            foreach (string path in ShapeRecipes)
            foreach (var look in RecipeLooks(path))
            {
                var copy = new Dictionary<string, object>(look);
                copy.Remove("revision_of");
                looks.Add(copy);
            }
            return looks;
        }

        // The earliest a sliced look shows anything, unclamped: below zero for a piece seen before the head lands.
        private static float EarliestShown(Dictionary<string, object> look)
        {
            float first = float.MaxValue;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                foreach (Dictionary<string, object> motion in (List<object>)group["motion"])
                    first = Mathf.Min(first, Window(motion).x);
            foreach (Dictionary<string, object> ribbon in (List<object>)look["ribbons"])
                first = Mathf.Min(first, Window((Dictionary<string, object>)ribbon["motion"]).x);
            return first == float.MaxValue ? 0f : first;
        }

        /// <summary>
        /// The game lays a flat piece a little off the floor (ElementLooks' flat lift, 3.5 cm), so the patch would lie over
        /// the cracks the recipe drew on it 1.6 cm up and hide every one (the slam stills of 2026-11-21: no star of cracks
        /// in Earth, Ice, Metal or Life). Ribbons lying on the ground are raised above the patch.
        /// </summary>
        public const float CrackLift = 0.045f;

        private static void LiftCracks(Dictionary<string, object> look)
        {
            foreach (Dictionary<string, object> ribbon in (List<object>)look["ribbons"])
            {
                var points = (List<object>)ribbon["points"];
                float high = float.MinValue;
                foreach (var point in points) high = Mathf.Max(high, Vec(point, Vector3.zero).y);
                if (high > 0.06f) continue;
                var raised = new List<object>();
                foreach (var point in points)
                {
                    Vector3 at = Vec(point, Vector3.zero);
                    raised.Add(new List<object> { (double)at.x, (double)(at.y + CrackLift), (double)at.z });
                }
                ribbon["points"] = raised;
            }
        }

        /// <summary>
        /// The strike shapes' recipes (staging/blender/2026-11-21_strikes/&lt;shape&gt;/): the Great Hammer's slam about where
        /// its head lands, and the Scythe's spin, the Great Spear's thrust, the Twin Knives' dash-through and the Chain
        /// Sickle's lash about the player's ground point. All eleven of each approved by the owner.
        /// </summary>
        public static readonly string[] ShapeRecipes =
        {
            SlamRecipePath, ReactionArtBuilder.CombosFolder + "/spin_recipe.json", ReactionArtBuilder.CombosFolder + "/thrust_recipe.json",
            ReactionArtBuilder.CombosFolder + "/dash_recipe.json", ReactionArtBuilder.CombosFolder + "/lash_recipe.json",
        };

        /// <summary>The way the spin recipe's player faced (its conventions: the station's forward axis).</summary>
        private static readonly Vector3 SpinFacing = new Vector3(-0.643f, 0f, -0.766f);

        // The way a shape's recipe player faced: down its lane or dash, the spin's station forward, or the slam's impact.
        private static float FacingOf(Dictionary<string, object> look, string slug, Vector3 player)
        {
            Vector3 way = look.TryGetValue("lane_direction", out var lane) ? Vec(lane, Vector3.zero)
                : look.TryGetValue("dash_direction", out var dash) ? Vec(dash, Vector3.zero)
                : slug.StartsWith("spin_") ? SpinFacing
                : -player;
            return Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg;
        }

        // Each shape: its look about its origin (the slam's impact, the player's ground point), begun its lead before its
        // moment, and its look on each body struck.
        private static void BuildSlams(Lists lists, ElementArtSet elementArt, Material glow)
        {
            foreach (var look in SlamLooks())
            {
                string slug = Text(look, "slug");
                var bodies = new List<Vector3>();
                foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodies.Add(Vec(body["position"], Vector3.zero));
                Vector3 player = Vec(((Dictionary<string, object>)look["player"])["position"], Vector3.zero);
                float facing = FacingOf(look, slug, player);
                float seconds = Number(look, "duration_s", 1.4f);
                string key = ElementMoves.Prefix + slug;
                var about = Slice(look, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds, Vector3.zero);
                LiftCracks(about);
                float lead = Mathf.Max(0f, -EarliestShown(about));
                AddElements3(about, key, -lead, facing, lists, elementArt, glow);
                int made = lists.Looks.FindIndex(l => l.Key == key);
                if (made >= 0)
                {
                    var kept = lists.Looks[made];
                    kept.Lead = lead;
                    lists.Looks[made] = kept;
                }
                // On each body struck: played as the head lands, its pieces at the recipe's own times from then.
                var onBody = Slice(look, Cut.OnBody, bodies, player, -99f, 99f, seconds);
                onBody["duration_s"] = (double)seconds;
                AddElements3(onBody, key + "@b", 0f, facing, lists, elementArt, glow);
            }
        }
    }
}
