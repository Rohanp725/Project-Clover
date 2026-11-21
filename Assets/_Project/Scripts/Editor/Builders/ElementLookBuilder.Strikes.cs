using System.Collections.Generic;
using System.Linq;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Blender worker's infused strike end effects (staging/blender/2026-11-21_strikes), the answer to the owner's
    /// comments on the swing films (docs/design/feedback/2026-11-21_swing_comments.md): each effect grows out of the cut,
    /// along its arc and on what it hits. Cut up by the elements 3 builder like its moments (about the player, and once on
    /// each enemy struck), built as "e3_strike_earth" and "e3_strike_earth@b", which <see cref="ElementMoves"/> plays in
    /// place of the element's earlier strike.
    ///
    /// The recipe marks every piece made once per enemy the cut hits (motion.per_struck_enemy): those, and only those, go
    /// on the struck body's look, so a miss draws nothing of them.
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public const string StrikesRecipePath = ReactionArtBuilder.CombosFolder + "/strikes_recipe.json";
        public const string StrikesKit = "vfx_strikes_kit.fbx";

        /// <summary>
        /// The recipe's strikes not built yet: Dark and Poison are being redrawn in Blender (2026-11-21) and keep their
        /// current looks until the owner approves the new ones. Take a slug out of here (and bring the new recipe in) and
        /// re-run Clover > Build > Element Looks to build it.
        /// </summary>
        public static readonly HashSet<string> StrikesHeld = new HashSet<string> { "strike_dark", "strike_poison" };

        /// <summary>
        /// The strike recipe's own materials, in linear colour from its specs (tools/st_specs.py): a ramp's colour is taken
        /// a quarter of the way out from its core, as the elements 4 paints were. None is white: the glints are pale steel
        /// blue, the glare warm gold to amber.
        /// </summary>
        public static readonly Dictionary<string, ReactionArtBuilder.Paint> StrikePaints = new Dictionary<string, ReactionArtBuilder.Paint>
        {
            ["st_stone"] = new ReactionArtBuilder.Paint(0.40f, 0.28f, 0.18f),
            ["st_glint"] = new ReactionArtBuilder.Paint(0.64f, 0.71f, 0.82f),
            ["st_glint_flat"] = new ReactionArtBuilder.Paint(0.80f, 0.86f, 0.94f),
            ["st_shard"] = new ReactionArtBuilder.Paint(0.565f, 0.625f, 0.71f),
            ["st_flare"] = new ReactionArtBuilder.Paint(1.00f, 0.683f, 0.213f),
            ["st_glare"] = new ReactionArtBuilder.Paint(1.00f, 0.655f, 0.175f),
            ["st_glare_amber"] = new ReactionArtBuilder.Paint(0.95f, 0.45f, 0.05f),
            // Dark's and Poison's, for when they are built.
            ["st_shadow"] = new ReactionArtBuilder.Paint(0.010f, 0.003f, 0.017f),
            ["st_hand_small"] = new ReactionArtBuilder.Paint(0.020f, 0.006f, 0.040f),
            ["st_vein"] = new ReactionArtBuilder.Paint(0.012f, 0.075f, 0.022f),
            ["st_vein_glow"] = new ReactionArtBuilder.Paint(0.05f, 0.36f, 0.09f),
            ["st_venom"] = new ReactionArtBuilder.Paint(0.05f, 0.31f, 0.075f),
            ["st_venom_wet"] = new ReactionArtBuilder.Paint(0.025f, 0.17f, 0.04f),
            ["st_venom_dry"] = new ReactionArtBuilder.Paint(0.05f, 0.075f, 0.025f),
            ["st_wisp"] = new ReactionArtBuilder.Paint(0.30f, 0.38f, 0.30f),
        };

        /// <summary>True for a recipe look that marks its pieces per struck enemy (every look of the strikes recipe does).</summary>
        private static bool MarksStruck(Dictionary<string, object> look) => look.ContainsKey("struck_bodies");

        private static bool PerStruckEnemy(Dictionary<string, object> motion) =>
            motion != null && motion.TryGetValue("per_struck_enemy", out var flag) && flag is bool yes && yes;

        /// <summary>The strikes recipe's looks to build, the held ones left out, cut as moments (their revision_of dropped).</summary>
        private static List<Dictionary<string, object>> StrikeLooks()
        {
            var looks = new List<Dictionary<string, object>>();
            foreach (var look in RecipeLooks(StrikesRecipePath))
            {
                if (StrikesHeld.Contains(Text(look, "slug"))) continue;
                var copy = new Dictionary<string, object>(look);
                copy.Remove("revision_of");
                looks.Add(copy);
            }
            return looks;
        }

        /// <summary>
        /// The struck body's own lift from a strike (Earth's stones throw each one they reach 0.55 m up and let it down):
        /// the recipe's main body's height, played on the model only from the strike's start. A strike with nothing else on
        /// the body gets a look of its own for it.
        /// </summary>
        private static void AddStrikeRaise(Dictionary<string, object> look, string key, float shift, float facing, Lists lists)
        {
            if (!(look.TryGetValue("bodies", out var value) && value is List<object> bodies) || bodies.Count == 0) return;
            if (!(((Dictionary<string, object>)bodies[0]).TryGetValue("motion", out var motionValue) && motionValue is Dictionary<string, object> motion)) return;
            if (!(motion.TryGetValue("path", out var pathValue) && pathValue is List<object> path) || path.Count < 2) return;
            float most = 0f;
            foreach (var entry in path)
                if (entry is List<object> pose && pose.Count >= 2) most = Mathf.Max(most, Vec(pose[1], Vector3.zero).y);
            // A body that only lurches or squashes in place (Dark's tug) is not lifted.
            if (most < 0.1f) return;
            int at = lists.Looks.FindIndex(l => l.Key == key);
            ElementLookSet.Look made;
            if (at >= 0) made = lists.Looks[at];
            else
            {
                float seconds = Number(look, "duration_s", 1.4f) - shift;
                made = new ElementLookSet.Look { Key = key, Seconds = seconds, FirstPart = lists.Parts.Count, PartCount = 0, LoopFrom = seconds, FacingYaw = facing };
            }
            made.RaiseFirst = lists.Keys.Count;
            made.RaiseCount = 0;
            foreach (var entry in path)
            {
                if (!(entry is List<object> pose) || pose.Count < 2) continue;
                lists.Keys.Add(new ElementLookSet.Key
                {
                    Time = (float)(double)pose[0] - shift, Value = new Vector3(0f, Vec(pose[1], Vector3.zero).y, 0f), Ease = EaseOf(pose.Count > 2 ? pose[2] as string : "linear"),
                });
                made.RaiseCount++;
            }
            if (at >= 0) lists.Looks[at] = made;
            else
            {
                lists.Looks.Add(made);
                Report.AppendLine($"| {key} | {made.Seconds:0.0} s | body | 0 | 0 | - | lifts the body {most:0.00} m |");
            }
        }
    }
}
