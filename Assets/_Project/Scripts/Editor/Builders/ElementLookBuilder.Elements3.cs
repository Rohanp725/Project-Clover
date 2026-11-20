using System.Collections.Generic;
using System.Linq;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Blender worker's elements 3 pass (staging/blender/2026-11-20_elements3, its second pass in): each element's own
    /// moments, cut out of their recipe into the looks the game plays them by (<see cref="ElementMoves"/>).
    ///
    /// The recipe draws every moment as the look-dev saw it: a stand-in katana swung about a standing player, three
    /// soldiers stood about the spot 3 m ahead. The game has a real weapon and real enemies wherever they stand, so each
    /// timeline is cut by what it is about:
    /// - the charged blades: the sheath the element wears on the blade (in the blade's own frame, laid on the real weapon's
    ///   blade line by the swing trails), and each blow's landing on the one it struck, the third with its extra. The
    ///   katana, the recipe's trails along its mock-up orbit and what flies off that orbit are the swing trails' work;
    /// - the spells and the infused strikes: what happens about the caster (the rolling front, the wave, the cloud bank,
    ///   the crescent, the slabs heaving up the line) and what happens on each body it reaches (it catches, is lifted,
    ///   snagged, speared, hooked), the second played on every real body the moment reaches;
    /// - Tailwind, all of it on the player.
    /// </summary>
    public static partial class ElementLookBuilder
    {
        public const string E3Kit = "vfx_elements_kit_e3.fbx";

        /// <summary>The kit pieces the charged blades wear on the blade, and the stand-in katana they were drawn on.</summary>
        public static readonly HashSet<string> SheathPieces = new HashSet<string>
        {
            "vfxe3_katana", "vfxe3_flameflag_a", "vfxe3_flameflag_b", "vfxe3_iceteeth", "vfxe3_watersheath", "vfxe3_thornvine",
            "vfxe3_stonespine", "vfxe3_edgelight", "vfxe3_venomcoat",
        };

        /// <summary>The recipe's blade frame: the blade from the guard to the tip along the sheath meshes' own x (notes.md).</summary>
        public const float BladeFrom = 0.3f, BladeTo = 1.25f;

        /// <summary>
        /// The recipe's three cuts, 0.45 s apart; each blow's landing is played from just before its edge meets the body
        /// (0.05 s into its cut) for 0.6 s, and what it lays the affliction's own look carries on.
        /// </summary>
        public const float BlowEvery = 0.45f, BlowLead = 0.05f, BlowShown = 0.6f;

        /// <summary>A piece lying on the floor this near a body is that body's (a pool under it, its char, its hole).</summary>
        public const float BodyGround = 1.0f;

        /// <summary>Ribbons drawn toward the camera, by "slug|material", metres: The Grudge's black bolt drops in front of the health bar.</summary>
        public static readonly Dictionary<string, float> RibbonFacing = new Dictionary<string, float>
        {
            // The coordinator (2026-11-20): "the black bolt is faint behind the health bar". Bolt and cloud both stand
            // in front of the bar, so the bolt still leaves the cloud.
            ["the_grudge|r4_blackbolt"] = 0.6f,
            ["the_grudge|r4_grudgecloud"] = 0.6f,
            ["the_grudge|r4_grudgelit"] = 0.6f,
        };

        /// <summary>Marked's sun-wheel piece, the glint a reaction draws in its place, and how much larger than the wheel was drawn.</summary>
        public const string MarkedWheel = "vfx2_light_wheel_affliction", MarkedFlare = "vfx2_light_glint_charged";
        public const float MarkedFlareSize = 1.8f;

        /// <summary>Reactions whose body is not drawn frozen pale: Winter Seed (no frost crust, no frozen glow while its look plays).</summary>
        public static readonly HashSet<string> ThawedReactions = new HashSet<string> { "winter_seed" };

        /// <summary>
        /// Pieces one reaction leaves out by a part of the recipe's own name for them, where the look draws them anew:
        /// Red Hot's blast of stacked tongues, its white-hot sparks and its char fan (the owner, 2026-11-20: "The cone
        /// should be like a blast of fire through the enemy; look to the flame wave spell as inspiration") give way to
        /// the elements 3 Flame Wave's rolling front, out of the enemy's far side (<see cref="BlastLooks"/>).
        /// </summary>
        public static readonly Dictionary<string, string[]> LeftOutNamed = new Dictionary<string, string[]>
        {
            ["red_hot"] = new[] { "_blast", "_wave", "_sparksh", "_fan" },
        };

        public static bool LeftOutByName(string slug, string piece)
        {
            if (piece == null || !LeftOutNamed.TryGetValue(slug, out var names)) return false;
            foreach (var name in names)
                if (piece.Contains(name)) return true;
            return false;
        }

        /// <summary>
        /// Reactions blasting fire on out of the enemy's far side as the elements 3 Flame Wave's front, by slug: (seconds
        /// into the reaction it rolls, how far it reaches, metres, its tongues' size against the wave's). Built as the
        /// look "slug@+", played on the body struck with the reaction's own look (<see cref="ElementLooks.PlayReaction"/>).
        /// Red Hot's blast: a 3 m cone, tongues 1.1 to 1.5 m (the scale notes).
        /// </summary>
        public static readonly Dictionary<string, Vector3> BlastLooks = new Dictionary<string, Vector3>
        {
            ["red_hot"] = new Vector3(0.6f, 3f, 0.6f),
        };

        /// <summary>
        /// Reactions that pulse once on the ground as they finish, by slug: (seconds in, metres across). Quickening (the
        /// owner, 2026-11-19: "this growth can also give off a green/brown pulse, when grown kind of visualizing the heal
        /// and the seeding"): green edged brown, 2.5 m, as the plants finish growing.
        /// </summary>
        public static readonly Dictionary<string, Vector2> HealPulses = new Dictionary<string, Vector2>
        {
            ["quickening"] = new Vector2(0.95f, 2.5f),
        };
        public static readonly Color PulseGreen = new Color(0.2f, 0.55f, 0.12f, 0.65f), PulseBrown = new Color(0.3f, 0.18f, 0.06f, 0.8f);

        // The heal pulse: a see-through disc on the floor, green at its middle going brown at its edge, opening out once and gone.
        private static void AddHealPulse(string slug, string key, Lists lists, ElementLookSet.Look made, Material glow)
        {
            if (!HealPulses.TryGetValue(slug, out var pulse)) return;
            const int Around = 32;
            var vertices = new List<Vector3> { Vector3.zero };
            var colours = new List<Color> { PulseGreen };
            var triangles = new List<int>();
            float radius = pulse.y * 0.5f;
            for (int ring = 1; ring <= 2; ring++)
            {
                float r = ring == 1 ? radius * 0.72f : radius;
                for (int k = 0; k < Around; k++)
                {
                    float angle = k * Mathf.PI * 2f / Around;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
                    colours.Add(ring == 1 ? Color.Lerp(PulseGreen, PulseBrown, 0.25f) : PulseBrown);
                }
            }
            for (int k = 0; k < Around; k++)
            {
                int a = 1 + k, b = 1 + (k + 1) % Around, c = 1 + Around + k, d = 1 + Around + (k + 1) % Around;
                triangles.AddRange(new[] { 0, b, a, a, b, d, a, d, c });
            }
            var mesh = new Mesh { name = PartName(key, lists.Parts.Count - made.FirstPart, "heal_pulse", null) };
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, lists.Target);
            var part = new ElementLookSet.Part
            {
                Mesh = mesh, Material = glow, Kind = ElementLookSet.Flat, Follows = false,
                Position = Vector3.zero, Rotation = Quaternion.identity, Scale = Vector3.one, Show = pulse.x, Hide = pulse.x + 0.85f,
            };
            part.ScaleFirst = lists.Keys.Count;
            lists.Keys.Add(new ElementLookSet.Key { Time = pulse.x, Value = new Vector3(0.25f, 1f, 0.25f), Ease = ElementLookSet.Linear });
            lists.Keys.Add(new ElementLookSet.Key { Time = pulse.x + 0.45f, Value = Vector3.one, Ease = ElementLookSet.Out });
            lists.Keys.Add(new ElementLookSet.Key { Time = pulse.x + 0.85f, Value = new Vector3(1.12f, 1f, 1.12f), Ease = ElementLookSet.Linear });
            part.ScaleCount = 3;
            lists.Parts.Add(part);
        }

        // Each blast: the Flame Wave's front (its licking band, its tongues and their cores, the low flames left behind and
        // its char) drawn smaller, out from just beyond the enemy, from the moment the reaction blasts.
        private static void BuildBlasts(List<Dictionary<string, object>> looks, Lists lists, ElementArtSet elementArt, Material glow)
        {
            var wave = looks.FirstOrDefault(l => Text(l, "slug") == "flame_wave");
            if (wave == null) return;
            var bodies = new List<Vector3>();
            foreach (Dictionary<string, object> body in (List<object>)wave["bodies"]) bodies.Add(Vec(body["position"], Vector3.zero));
            Vector3 player = Vec(((Dictionary<string, object>)wave["player"])["position"], Vector3.zero);
            float facing = Mathf.Atan2(-player.x, -player.z) * Mathf.Rad2Deg;
            Vector3 forward = new Vector3(-player.x, 0f, -player.z).normalized;
            float seconds = Number(wave, "duration_s", 1.4f);
            // How far the front rolls in the recipe (to 6 m) and from where (its foot just ahead of the caster).
            const float WaveReach = 6f, FarSide = 0.45f;
            foreach (var blast in BlastLooks)
            {
                var sliced = Slice(wave, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds);
                float reach = blast.Value.y / WaveReach;
                // The front rolls its shorter way in the same share of a second: faster, as a blast.
                float time = Mathf.Lerp(reach, 1f, 0.35f);
                Rescale(sliced, reach, blast.Value.z, forward * FarSide, time, blast.Value.x);
                sliced["duration_s"] = (double)(seconds * time + blast.Value.x);
                AddElements3(sliced, blast.Key + "@+", 0f, facing, lists, elementArt, glow);
            }
        }

        private static double Retimed(object value, float time, float from) => value is double t && t < 9.0 ? t * time + from : value is double held ? held : 0.0;

        private static void RetimeKeys(Dictionary<string, object> holder, string name, float time, float from, float reach)
        {
            if (!holder.TryGetValue(name, out var value) || !(value is List<object> keys)) return;
            var made = new List<object>();
            foreach (var entry in keys)
            {
                if (!(entry is List<object> key) || key.Count < 2) { made.Add(entry); continue; }
                var copy = new List<object>(key) { [0] = Retimed(key[0], time, 0f) + from };
                if (reach != 1f && key[1] is List<object> offset) copy[1] = ToList(Vec(offset, Vector3.zero) * reach);
                made.Add(copy);
            }
            holder[name] = made;
        }

        // A sliced look drawn smaller and sooner or later: its places and paths times reach about its anchor and moved by
        // shift, its pieces and ribbons times size across, its times times time and moved on by from.
        private static void Rescale(Dictionary<string, object> look, float reach, float size, Vector3 shift, float time, float from)
        {
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                for (int n = 0; n < instances.Count; n++)
                {
                    var instance = (List<object>)instances[n];
                    instance[1] = ToList(Vec(instance[1], Vector3.zero) * reach + shift);
                    instance[3] = instance[3] is List<object> scale ? ToList(Vec(scale, Vector3.one) * size) : (object)((instance[3] is double uniform ? uniform : 1.0) * size);
                    var motion = (Dictionary<string, object>)motions[n];
                    motion["visible"] = new List<object> { Retimed(((List<object>)motion["visible"])[0], time, 0f) + from, ((List<object>)motion["visible"])[1] is double end && end >= 9.0 ? end : Retimed(((List<object>)motion["visible"])[1], time, 0f) + from };
                    RetimeKeys(motion, "path", time, from, reach);
                    RetimeKeys(motion, "scale_keys", time, from, 1f);
                    if (motion.TryGetValue("spin", out var spin) && spin is Dictionary<string, object> spinning)
                    {
                        var copy = new Dictionary<string, object>(spinning);
                        RetimeKeys(copy, "keys", time, from, 1f);
                        motion["spin"] = copy;
                    }
                    if (motion.TryGetValue("orbit", out var orbit) && orbit is Dictionary<string, object> round)
                    {
                        var copy = new Dictionary<string, object>(round);
                        RetimeKeys(copy, "keys", time, from, 1f);
                        copy["centre"] = ToList(Vec(round.TryGetValue("centre", out var c) ? c : null, Vector3.zero) * reach + shift);
                        motion["orbit"] = copy;
                    }
                }
            }
            foreach (Dictionary<string, object> ribbon in (List<object>)look["ribbons"])
            {
                var points = new List<object>();
                foreach (var point in (List<object>)ribbon["points"]) points.Add(ToList(Vec(point, Vector3.zero) * reach));
                ribbon["points"] = points;
                ribbon["offset"] = ToList(Vec(ribbon.TryGetValue("offset", out var o) ? o : null, Vector3.zero) * reach + shift);
                ribbon["width_m"] = Number(ribbon, "width_m", 0.1f) * (double)size;
                var motion = new Dictionary<string, object>((Dictionary<string, object>)ribbon["motion"]);
                motion["visible"] = new List<object> { Retimed(((List<object>)motion["visible"])[0], time, 0f) + from, ((List<object>)motion["visible"])[1] is double end && end >= 9.0 ? end : Retimed(((List<object>)motion["visible"])[1], time, 0f) + from };
                RetimeKeys(motion, "draw_on", time, from, 1f);
                RetimeKeys(motion, "erase", time, from, 1f);
                RetimeKeys(motion, "path", time, from, reach);
                ribbon["motion"] = motion;
            }
        }

        private enum Cut { AboutPlayer, OnBody, BladeBlow }

        private static List<Dictionary<string, object>> Elements3Looks()
        {
            var recipe = AssetDatabase.LoadAssetAtPath<TextAsset>(RevisionRecipePath);
            if (recipe == null) return new List<Dictionary<string, object>>();
            return ((List<object>)((Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text))["looks"]).Cast<Dictionary<string, object>>().ToList();
        }

        /// <summary>The elements 3 brazier that replaces one of the eleven loops (Lightning's caged storm), or null; its brazier itself is the hub's own.</summary>
        private static Dictionary<string, object> Elements3Brazier(string slug)
        {
            if (slug != "brazier_lightning") return null;
            var look = Elements3Looks().FirstOrDefault(l => Text(l, "slug") == slug);
            if (look == null) return null;
            var copy = new Dictionary<string, object>(look);
            var groups = new List<object>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                if (!Text(group, "kit_object").StartsWith("vfxe3_brazier_")) groups.Add(group);
            copy["groups"] = groups;
            return copy;
        }

        // The spells, the infused strikes and Tailwind, and each blade's three blows.
        private static void BuildElements3(Lists lists, ElementArtSet elementArt, Material glow)
        {
            lists.Flat = true;
            foreach (var look in Elements3Looks())
            {
                string slug = Text(look, "slug"), moment = Text(look, "moment");
                if (look.ContainsKey("revision_of") || moment == "brazier") continue;
                var bodies = new List<Vector3>();
                foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodies.Add(Vec(body["position"], Vector3.zero));
                Vector3 player = Vec(((Dictionary<string, object>)look["player"])["position"], Vector3.zero);
                // The way the recipe's player faced: to the spot 3 m ahead (the recipe's origin), the main body on that line.
                float facing = Mathf.Atan2(-player.x, -player.z) * Mathf.Rad2Deg;
                float seconds = Number(look, "duration_s", 1.5f);
                string key = ElementMoves.Prefix + slug;
                if (moment == "charged")
                {
                    for (int blow = 1; blow <= 3; blow++)
                    {
                        float cut = (blow - 1) * BlowEvery;
                        float shift = cut + BlowLead;
                        var sliced = Slice(look, Cut.BladeBlow, bodies, player, cut - 0.02f, blow == 3 ? 99f : cut + BlowEvery - 0.02f, shift + BlowShown);
                        AddElements3(sliced, key + "_hit" + blow, shift, facing, lists, elementArt, glow);
                    }
                    continue;
                }
                if (moment == "surge")
                {
                    AddElements3(Slice(look, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds), key, 0f, facing, lists, elementArt, glow);
                    continue;
                }
                if (moment != "spell" && moment != "infused") continue;
                AddElements3(Slice(look, Cut.AboutPlayer, bodies, player, -99f, 99f, seconds), key, 0f, facing, lists, elementArt, glow);
                // On each body: from when the moment first reaches it (its first piece), as long as the recipe's own.
                var onBody = Slice(look, Cut.OnBody, bodies, player, -99f, 99f, seconds);
                float first = FirstShown(onBody);
                onBody["duration_s"] = (double)seconds;
                AddElements3(onBody, key + "@b", first, facing, lists, elementArt, glow);
            }
            // Red Hot's blast of fire through the enemy, as the Flame Wave's front.
            BuildBlasts(Elements3Looks(), lists, elementArt, glow);
            lists.Flat = false;
        }

        private static void AddElements3(Dictionary<string, object> sliced, string key, float shift, float facing, Lists lists, ElementArtSet elementArt, Material glow)
        {
            int before = lists.Looks.Count;
            BuildLook(sliced, key, key, 0, new List<Vector3>(), shift, lists, elementArt, glow);
            if (lists.Looks.Count == before) return;
            var made = lists.Looks[lists.Looks.Count - 1];
            // A moment with nothing on this side of it (Frost Bite is all on the bodies it strikes) has no look there.
            if (made.PartCount == 0)
            {
                lists.Looks.RemoveAt(lists.Looks.Count - 1);
                return;
            }
            made.FacingYaw = facing;
            made.OnPlayer = false;
            // Ice that bursts through or out of the body is what is seen there: the body is not lit frozen pale under it.
            made.Thawed = key.StartsWith("e3_frost_bite@") || key.StartsWith("e3_ice_blade_hit");
            lists.Looks[lists.Looks.Count - 1] = made;
            // Riptide's wave is darker at its foot and foams pale at its crest (notes.md), not one flat blue.
            for (int k = made.FirstPart; k < made.FirstPart + made.PartCount; k++)
            {
                var mesh = lists.Parts[k].Mesh;
                if (mesh != null && (mesh.name.Contains("_vfxe3_wave~") || mesh.name.Contains("_vfxe3_wave_"))) RampByHeight(mesh, WaveRamp);
            }
        }

        private static readonly (float at, Color colour)[] WaveRamp =
        {
            (0f, new Color(0.015f, 0.07f, 0.24f)), (0.45f, new Color(0.025f, 0.2f, 0.52f)), (0.78f, new Color(0.06f, 0.42f, 0.8f)), (0.93f, new Color(0.3f, 0.76f, 0.95f)),
        };

        private static Color Ramp((float at, Color colour)[] ramp, float x)
        {
            if (x <= ramp[0].at) return ramp[0].colour;
            for (int i = 0; i + 1 < ramp.Length; i++)
                if (x <= ramp[i + 1].at) return Color.Lerp(ramp[i].colour, ramp[i + 1].colour, (x - ramp[i].at) / Mathf.Max(0.0001f, ramp[i + 1].at - ramp[i].at));
            return ramp[ramp.Length - 1].colour;
        }

        private static void RampByHeight(Mesh mesh, (float at, Color colour)[] ramp)
        {
            var vertices = mesh.vertices;
            var colours = mesh.colors;
            if (vertices.Length == 0 || colours.Length != vertices.Length) return;
            float low = vertices.Min(v => v.y), high = vertices.Max(v => v.y);
            for (int i = 0; i < vertices.Length; i++)
            {
                var c = Ramp(ramp, (vertices[i].y - low) / Mathf.Max(0.0001f, high - low));
                colours[i] = new Color(c.r, c.g, c.b, colours[i].a);
            }
            mesh.colors = colours;
        }

        private static float FirstShown(Dictionary<string, object> look)
        {
            float first = float.MaxValue;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                foreach (Dictionary<string, object> motion in (List<object>)group["motion"])
                    first = Mathf.Min(first, Window(motion).x);
            foreach (Dictionary<string, object> ribbon in (List<object>)look["ribbons"])
                first = Mathf.Min(first, Window((Dictionary<string, object>)ribbon["motion"]).x);
            return first == float.MaxValue ? 0f : Mathf.Max(0f, first);
        }

        // The body a piece of the recipe is about: the one it rides or orbits when it says so, else the one it stands nearest.
        private static int BodyOf(Dictionary<string, object> motion, Vector3 at, List<Vector3> bodies)
        {
            if (bodies.Count == 0) return -1;
            if (motion.TryGetValue("body", out var body) && body is double given) return (int)given;
            if (motion.TryGetValue("follow_body", out var follow) && follow is double followed) return (int)followed;
            if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit
                && orbit.TryGetValue("centre_is", out var centreIs) && centreIs is string named && CentreBody(named) >= 0) return CentreBody(named);
            return Nearest(bodies, at);
        }

        private static List<object> ToList(Vector3 v) => new List<object> { (double)v.x, (double)v.y, (double)v.z };

        /// <summary>
        /// One moment's pieces cut out of its recipe look, moved into the frame they are played in (about the player, or
        /// about the one body) and keyed from <paramref name="from"/> to <paramref name="to"/> seconds (a blade's blow),
        /// as a look of no bodies, so every piece kept is built and none is looked for on another body.
        /// </summary>
        private static Dictionary<string, object> Slice(Dictionary<string, object> look, Cut cut, List<Vector3> bodies, Vector3 player, float from, float to, float seconds)
        {
            Vector3 frame = cut == Cut.AboutPlayer || bodies.Count == 0 ? player : bodies[0];
            var groups = new List<object>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                string kitObject = Text(group, "kit_object");
                if (SheathPieces.Contains(kitObject) || kitObject.StartsWith("vfxe3_brazier_")) continue;
                var instances = new List<object>();
                var motions = new List<object>();
                var groupInstances = (List<object>)group["instances"];
                var groupMotions = (List<object>)group["motion"];
                for (int n = 0; n < groupInstances.Count; n++)
                {
                    var instance = (List<object>)groupInstances[n];
                    var motion = (Dictionary<string, object>)groupMotions[n];
                    bool onPlayer = motion.TryGetValue("on_player", out var flag) && flag is bool yes && yes;
                    string placement = instance.Count > 4 ? instance[4] as string : "b";
                    Vector3 at = Vec(instance[1], Vector3.zero) + (onPlayer ? player : Vector3.zero);
                    int body = BodyOf(motion, at, bodies);
                    bool bodys = !onPlayer && body >= 0 && (placement != "g" || cut == Cut.BladeBlow
                        || new Vector2(at.x - bodies[body].x, at.z - bodies[body].z).magnitude <= BodyGround);
                    bool keep;
                    switch (cut)
                    {
                        case Cut.AboutPlayer: keep = !bodys; break;
                        case Cut.OnBody: keep = bodys && body == 0; break;
                        default:
                        {
                            float show = Window(motion).x;
                            keep = bodys && body == 0 && show >= from && show < to;
                            break;
                        }
                    }
                    if (!keep) continue;
                    var moved = new List<object>(instance);
                    moved[1] = ToList(at - frame);
                    var plain = new Dictionary<string, object>(motion);
                    plain.Remove("on_player");
                    plain.Remove("follow_body");
                    plain.Remove("body");
                    if (plain.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit)
                    {
                        var round = new Dictionary<string, object>(orbit);
                        Vector3 centre = Vec(orbit.TryGetValue("centre", out var c) ? c : null, Vector3.zero);
                        // A piece's orbit centre is about what it names: a body ("body 1"), the player, or the spot ahead.
                        string centreIs = orbit.TryGetValue("centre_is", out var named) ? named as string ?? "" : "";
                        int about = centreIs.StartsWith("body ") ? CentreBody(centreIs) : -1;
                        Vector3 origin = about >= 0 && about < bodies.Count ? bodies[about] : centreIs == "player" || (centreIs.Length == 0 && onPlayer) ? player : Vector3.zero;
                        round["centre"] = ToList(centre + origin - frame);
                        round.Remove("centre_is");
                        plain["orbit"] = round;
                    }
                    instances.Add(moved);
                    motions.Add(plain);
                }
                if (instances.Count == 0) continue;
                var kept = new Dictionary<string, object>(group) { ["instances"] = instances, ["motion"] = motions };
                groups.Add(kept);
            }
            var ribbons = new List<object>();
            if (look.TryGetValue("ribbons", out var ribbonValue) && ribbonValue is List<object> recipeRibbons)
                foreach (Dictionary<string, object> ribbon in recipeRibbons)
                {
                    string centre = Text(ribbon, "centre");
                    string name = Text(ribbon, "object");
                    // The trails along the stand-in's swing: the swing trails draw them along the real weapon's.
                    if (System.Text.RegularExpressions.Regex.IsMatch(name, @"_\d\d\d_trail")) continue;
                    int body = centre.StartsWith("body ") ? CentreBody(centre) : -1;
                    Vector3 middle = centre == "player" ? player : body >= 0 && body < bodies.Count ? bodies[body] : Vector3.zero;
                    bool keep;
                    switch (cut)
                    {
                        case Cut.AboutPlayer: keep = body < 0; break;
                        case Cut.OnBody: keep = body == 0; break;
                        default:
                        {
                            float show = Window((Dictionary<string, object>)ribbon["motion"]).x;
                            keep = body == 0 && show >= from && show < to;
                            break;
                        }
                    }
                    if (!keep) continue;
                    var moved = new Dictionary<string, object>(ribbon) { ["centre"] = "station", ["offset"] = ToList(middle - frame) };
                    ribbons.Add(moved);
                }
            return new Dictionary<string, object>(look)
            {
                ["groups"] = groups, ["ribbons"] = ribbons, ["bodies"] = new List<object>(), ["duration_s"] = (double)seconds,
            };
        }

        /// <summary>
        /// Each element's charged-blade sheath, out of its recipe look: the kit mesh in the blade's own frame (its own axes:
        /// the length along x, the edge toward +y, the flat's face toward +z), painted as the render painted it, drawn in
        /// its element's reaction solid. Fire's two flicker phases and Ice's teeth on the far edge are its second.
        /// Metal wears only a glint, the kit's four-pointed star. Wind and Dark wear no mesh: their air and smoke stay the
        /// swing trails' own sheath.
        /// </summary>
        private static ElementLookSet.BladeSheath[] BuildSheaths(Lists lists, ElementArtSet elementArt)
        {
            var made = new List<ElementLookSet.BladeSheath>();
            foreach (var look in Elements3Looks())
            {
                if (Text(look, "moment") != "charged") continue;
                string slug = Text(look, "slug"), element = Text(look, "element");
                int ring = ReactionDesigns.RingIndex(element);
                if (ring < 0) continue;
                var sheath = new ElementLookSet.BladeSheath { Element = ring, From = BladeFrom, To = BladeTo };
                foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                {
                    string kitObject = Text(group, "kit_object"), material = Text(group, "material");
                    bool glint = kitObject == "vfxe3_glint" && slug == "metal_blade";
                    if ((!SheathPieces.Contains(kitObject) || kitObject == "vfxe3_katana") && !glint) continue;
                    if (!TryKitPiece(E3Kit, kitObject, lists.Kits, out var filter, out _))
                    {
                        lists.Missing.Add($"{slug}: {kitObject} (the blade's sheath)");
                        continue;
                    }
                    var mesh = SheathMesh($"sheath_{slug}_{kitObject}", filter, material, false, lists.Target);
                    if (sheath.Mesh == null)
                    {
                        sheath.Mesh = mesh;
                        sheath.Motion = kitObject.StartsWith("vfxe3_flameflag") ? ElementLookSet.SheathFlicker
                            : kitObject == "vfxe3_iceteeth" ? ElementLookSet.SheathTeeth : glint ? ElementLookSet.SheathGlint : ElementLookSet.SheathStill;
                        int solidRing = ring < elementArt.Solids.Length ? ring : 0;
                        sheath.Material = ReactionSolid(elementArt.Solids[solidRing]);
                        // Ice's teeth on the far edge: the same row turned half round the blade, running the other way.
                        if (kitObject == "vfxe3_iceteeth")
                        {
                            sheath.Second = SheathMesh($"sheath_{slug}_{kitObject}_far", filter, material, true, lists.Target);
                            sheath.Step = 0.032f;
                        }
                    }
                    else if (sheath.Second == null && kitObject.StartsWith("vfxe3_flameflag")) sheath.Second = mesh;
                    if (glint) break;
                }
                if (sheath.Mesh != null) made.Add(sheath);
            }
            Report.AppendLine($"| blade sheaths | - | blade | {made.Count} | - | - | - |");
            return made.ToArray();
        }

        // A sheath piece in the blade's frame, painted per slot and, for the flame, the teeth and the edge of light, by a ramp
        // out from the blade (hottest or palest at the blade, as the render's emission ramps ran).
        private static Mesh SheathMesh(string name, MeshFilter filter, string material, bool turnedRound, Object into)
        {
            var source = filter.sharedMesh;
            var vertices = source.vertices;
            var normals = source.normals;
            if (turnedRound)
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x, -vertices[i].y, -vertices[i].z);
                    if (i < normals.Length) normals[i] = new Vector3(normals[i].x, -normals[i].y, -normals[i].z);
                }
            var slots = filter.GetComponent<MeshRenderer>() != null ? filter.GetComponent<MeshRenderer>().sharedMaterials : new Material[0];
            var colours = new Color[vertices.Length];
            var triangles = new List<int>();
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var paint = s < slots.Length && slots[s] != null && source.subMeshCount > 1 ? PaintOf(slots[s].name) : PaintOf(material);
                var sub = source.GetTriangles(s);
                foreach (int v in sub) colours[v] = new Color(paint.Colour.r, paint.Colour.g, paint.Colour.b, paint.Flag);
                triangles.AddRange(sub);
            }
            (float at, Color colour)[] ramp = null;
            if (filter.name.StartsWith("vfxe3_flameflag"))
                ramp = new[] { (0f, new Color(1f, 0.8f, 0.28f)), (0.2f, new Color(1f, 0.56f, 0.07f)), (0.48f, new Color(0.98f, 0.3f, 0.03f)), (0.78f, new Color(0.72f, 0.09f, 0.02f)) };
            else if (filter.name == "vfxe3_iceteeth")
                ramp = new[] { (0f, new Color(0.5f, 0.84f, 1f)), (0.35f, new Color(0.16f, 0.56f, 0.95f)), (0.75f, new Color(0.05f, 0.26f, 0.66f)) };
            if (ramp != null && vertices.Length > 0)
            {
                // Out from the blade along the edge axis, the way the render's ramp ran (its foot at the blade).
                float most = vertices.Max(v => Mathf.Abs(v.y));
                for (int i = 0; i < vertices.Length; i++)
                {
                    var c = Ramp(ramp, Mathf.Abs(vertices[i].y) / Mathf.Max(0.0001f, most));
                    colours[i] = new Color(c.r, c.g, c.b, colours[i].a);
                }
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(1, ElementArtBuilder.SmoothNormals(vertices, normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, into);
            return mesh;
        }
    }
}
