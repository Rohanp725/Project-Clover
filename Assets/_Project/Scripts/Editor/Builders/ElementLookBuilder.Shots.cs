using System.Collections.Generic;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Great Bow's SHOT strike (staging/blender/2026-11-21_strikes/shot, all eleven approved after round 2): a light
    /// trace along the arrow's flight and the element at its impact. The recipe's arrow is loosed 0.197 s before it stops
    /// in a soldier 6.5 m out; in play the arrow stops wherever it meets someone, or nowhere. So each look is cut in three:
    /// the trace about the player ("e3_shot_fire"), played as the arrow is loosed; the impact about the point it stopped
    /// ("e3_shot_fire@i"), played there when it hits; and the pieces made once per enemy struck ("e3_shot_fire@b"), on the
    /// one it hit. A miss draws only the trace (shot/notes.md, Unity notes).
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public const string ShotRecipePath = ReactionArtBuilder.CombosFolder + "/shot_recipe.json";

        /// <summary>
        /// A piece shown this long before the impact or later is the impact's; anything shown earlier is the trace drawn
        /// behind the arrow. The crossing strokes and the first spike of an impact begin up to a hundredth before it.
        /// </summary>
        public const float ShotImpactFrom = -0.012f;

        /// <summary>The ending of a shot's impact look's key.</summary>
        public const string ShotImpactEnding = "@i";

        private static void BuildShots(Lists lists, ElementArtSet elementArt, Material glow)
        {
            foreach (var recipeLook in RecipeLooks(ShotRecipePath))
            {
                var look = new Dictionary<string, object>(recipeLook);
                look.Remove("revision_of");
                string slug = Text(look, "slug");
                var bodies = new List<Vector3>();
                foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodies.Add(Vec(body["position"], Vector3.zero));
                Vector3 player = Vec(((Dictionary<string, object>)look["player"])["position"], Vector3.zero);
                float facing = FacingOf(look, slug, player);
                float seconds = Number(look, "duration_s", 1.3f);
                var timing = look.TryGetValue("shot_timing", out var timingValue) ? timingValue as Dictionary<string, object> : null;
                float loosed = timing != null ? Number(timing, "loosed_s", -0.197f) : -0.197f;
                float impactMetres = timing != null ? Number(timing, "impact_m", 6.5f) : 6.5f;
                Vector3 lane = Vec(look["lane_direction"], Vector3.forward).normalized;
                Vector3 impact = player + lane * impactMetres;
                string key = ElementMoves.Prefix + slug;

                // The trace: about the player, from the loosing (its time 0) to the impact; nothing to begin early.
                var trace = Shown(Slice(look, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds, Vector3.zero), -99f, ShotImpactFrom);
                AddElements3(trace, key, loosed, facing, lists, elementArt, glow);
                // The impact: about the point the arrow stopped, from the impact (a hundredth before it).
                var atImpact = Shown(Slice(look, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds, impact), ShotImpactFrom, 99f);
                AddElements3(atImpact, key + ShotImpactEnding, ShotImpactFrom, facing, lists, elementArt, glow);
                // On the enemy struck, from the impact.
                var onBody = Slice(look, Cut.OnBody, bodies, player, -99f, 99f, seconds);
                AddElements3(onBody, key + "@b", 0f, facing, lists, elementArt, glow);
                // Earth's stone throws the one it heaves up under: the body rides the recipe's own lift.
                AddStrikeRaise(look, key + "@b", 0f, facing, lists);
            }
        }

        // A sliced look's pieces and ribbons first shown from <from> up to <to> seconds only.
        private static Dictionary<string, object> Shown(Dictionary<string, object> sliced, float from, float to)
        {
            var groups = new List<object>();
            foreach (Dictionary<string, object> group in (List<object>)sliced["groups"])
            {
                var instances = new List<object>();
                var motions = new List<object>();
                var groupInstances = (List<object>)group["instances"];
                var groupMotions = (List<object>)group["motion"];
                for (int n = 0; n < groupInstances.Count; n++)
                {
                    float show = Window((Dictionary<string, object>)groupMotions[n]).x;
                    if (show < from || show >= to) continue;
                    instances.Add(groupInstances[n]);
                    motions.Add(groupMotions[n]);
                }
                if (instances.Count > 0) groups.Add(new Dictionary<string, object>(group) { ["instances"] = instances, ["motion"] = motions });
            }
            var ribbons = new List<object>();
            foreach (Dictionary<string, object> ribbon in (List<object>)sliced["ribbons"])
            {
                float show = Window((Dictionary<string, object>)ribbon["motion"]).x;
                if (show >= from && show < to) ribbons.Add(ribbon);
            }
            return new Dictionary<string, object>(sliced) { ["groups"] = groups, ["ribbons"] = ribbons };
        }
    }
}
