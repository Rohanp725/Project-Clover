using System.Collections.Generic;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The round-4 placements the owner asked for (docs/design/feedback/2026-11-22_round4_plan.md, "Not yet"), made on a
    /// copy of the recipe look before it is built, so the Blender worker's recipes stay as delivered:
    /// Anvil Stone's, Live Iron's and Sundered's blades run through the body (guard outside one side, point out of the
    /// other) at the stand-in's chest, waist and shoulder, which <see cref="ElementLooks.FitOnEnemy"/> carries to each
    /// body's own anchors and size, Sundered with several; Iron Thorns' thorns in the first enemy shrink each time one flies
    /// off; Golden Feathers' feathers come down on the others and settle on their shoulders; Frost Heave's ice and the body
    /// riding it rise less, so both stay in frame; Stone Rise lifts the body on top of its rock and lets it down with it;
    /// Briar Patch's thorns stand on its plants, a little bigger, not out of the ground; Seeded's seeds orbit evenly at the
    /// chest, golden-brown with spring-green sprout tips.
    /// </summary>
    public static partial class ElementLookBuilder
    {
        /// <summary>Uniform scale of a blade run through a body: on the castle soldier its point comes out a hand's width behind.</summary>
        public const float ImpaleScale = 2.1f;

        /// <summary>How far out from the stand-in's upright a blade's guard stands: just past its reach (BodyAnchors.StandInChest).</summary>
        public const float ImpaleGuard = 0.47f;

        /// <summary>Where blades go through a body, in order: height on the stand-in, turn round it from the side they enter, tilt down.</summary>
        private static readonly (float height, float turn, float tilt)[] ImpaleSpots =
        {
            (1.36f, 0f, 15f), (1.02f, 70f, 6f), (1.58f, -55f, 32f),
        };

        /// <summary>The looks whose blades go through the body, and whether they enter from behind (Sundered: the point out of the chest toward the camera).</summary>
        private static readonly Dictionary<string, bool> ImpaledLooks = new Dictionary<string, bool>
        {
            ["anvil_stone"] = false, ["live_iron"] = false, ["metal_sundered"] = true,
        };

        /// <summary>Moments that lift the body struck on what they raise: Stone Rise's rock.</summary>
        public static readonly HashSet<string> RaisedMoments = new HashSet<string> { "stone_rise" };

        /// <summary>How high Frost Heave's ice lifts now, and the body on it (it rose 1.4 m and left the frame).</summary>
        public const float FrostHeaveRise = 0.8f, FrostHeaveBody = 0.7f;

        /// <summary>The seeds' ring at the chest: its radius on the stand-in.</summary>
        public const float SeedRing = 0.9f;

        /// <summary>A look as the round-4 placements change it, or the same look when it has none.</summary>
        private static Dictionary<string, object> Round4Placement(Dictionary<string, object> look, string slug)
        {
            bool fromBehind = false;
            bool impaled = ImpaledLooks.TryGetValue(slug, out fromBehind);
            bool any = impaled || slug == "iron_thorns" || slug == "golden_feathers" || slug == "frost_heave" || slug == "stone_rise"
                || slug == "briar_patch" || slug == "life_seeded";
            if (!any || look == null) return look;
            var copy = (Dictionary<string, object>)DeepCopy(look);
            if (impaled) Impale(copy, fromBehind, slug == "metal_sundered" ? 3 : 1);
            if (slug == "iron_thorns") ShrinkIronThorns(copy);
            if (slug == "golden_feathers") SettleFeathers(copy);
            if (slug == "frost_heave") LowerFrostHeave(copy);
            if (slug == "stone_rise") RideStoneRise(copy);
            if (slug == "briar_patch") ThornsOnBriars(copy);
            if (slug == "life_seeded") EvenSeeds(copy);
            return copy;
        }

        private static object DeepCopy(object value)
        {
            if (value is Dictionary<string, object> map)
            {
                var made = new Dictionary<string, object>(map.Count);
                foreach (var pair in map) made[pair.Key] = DeepCopy(pair.Value);
                return made;
            }
            if (value is List<object> list)
            {
                var made = new List<object>(list.Count);
                foreach (var item in list) made.Add(DeepCopy(item));
                return made;
            }
            return value;
        }

        private static List<Vector3> BodiesOf(Dictionary<string, object> look)
        {
            var bodies = new List<Vector3>();
            if (look.TryGetValue("bodies", out var value) && value is List<object> list)
                foreach (Dictionary<string, object> body in list) bodies.Add(Vec(body["position"], Vector3.zero));
            return bodies;
        }

        private static Vector3 PlayerOf(Dictionary<string, object> look) =>
            look.TryGetValue("player", out var value) && value is Dictionary<string, object> player ? Vec(player["position"], Vector3.zero) : new Vector3(1.93f, 0f, 2.3f);

        // The body a piece is on: the one it names, else the nearest.
        private static int BodyIndex(Dictionary<string, object> motion, Vector3 at, List<Vector3> bodies)
        {
            if (motion.TryGetValue("body", out var body) && body is double given) return (int)given;
            if (motion.TryGetValue("follow_body", out var follow) && follow is double followed) return (int)followed;
            return bodies.Count > 0 ? Nearest(bodies, at) : -1;
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.forward;
        }

        private static List<object> Euler(Quaternion rotation)
        {
            var e = rotation.eulerAngles;
            return new List<object> { (double)Mathf.Round(e.x), (double)Mathf.Round(e.y), (double)Mathf.Round(e.z) };
        }

        // Every key of a motion's channels later by seconds (a second blade struck in after the first).
        private static void Later(Dictionary<string, object> motion, float seconds)
        {
            if (motion.TryGetValue("visible", out var visibleValue) && visibleValue is List<object> visible)
            {
                visible[0] = (double)visible[0] + seconds;
                if ((double)visible[1] < 9.0) visible[1] = (double)visible[1] + seconds;
            }
            foreach (string channel in new[] { "path", "scale_keys" })
                if (motion.TryGetValue(channel, out var keysValue) && keysValue is List<object> keys)
                    foreach (List<object> key in keys) key[0] = (double)key[0] + seconds;
            if (motion.TryGetValue("spin", out var spinValue) && spinValue is Dictionary<string, object> spin && spin.TryGetValue("keys", out var spinKeys) && spinKeys is List<object> turns)
                foreach (List<object> key in turns) key[0] = (double)key[0] + seconds;
        }

        // ---- Blades through the body.

        private static void Impale(Dictionary<string, object> look, bool fromBehind, int bladesEach)
        {
            var bodies = BodiesOf(look);
            if (bodies.Count == 0) return;
            Vector3 player = PlayerOf(look);
            // Where each dagger stood and where its guard goes now, by body, for what sat at it (sparks, a wound, a glint).
            var moved = new List<(int body, Vector3 from, Vector3 to)>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfxe2_dagger") continue;
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                var counts = new Dictionary<int, int>();
                int original = instances.Count;
                for (int n = 0; n < original; n++)
                {
                    var instance = (List<object>)instances[n];
                    var motion = (Dictionary<string, object>)motions[n];
                    Vector3 was = Vec(instance[1], Vector3.zero);
                    int body = BodyIndex(motion, was, bodies);
                    if (body < 0) continue;
                    counts.TryGetValue(body, out int k);
                    counts[body] = k + 1;
                    Vector3 guard = PlaceBlade(instance, bodies[body], player, fromBehind, k);
                    moved.Add((body, was, guard));
                    // Sundered: more blades through the same body at its other spots, each struck a little later.
                    for (int more = 1; more < bladesEach && k == 0; more++)
                    {
                        var extra = (List<object>)DeepCopy(instance);
                        var extraMotion = (Dictionary<string, object>)DeepCopy(motion);
                        extra[0] = (instance[0] as string ?? "dagger") + "_more" + more;
                        PlaceBlade(extra, bodies[body], player, fromBehind, more);
                        Later(extraMotion, 0.12f * more);
                        instances.Add(extra);
                        motions.Add(extraMotion);
                    }
                }
            }
            // What sat on a dagger goes with it to its guard.
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") == "vfxe2_dagger") continue;
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                for (int n = 0; n < instances.Count; n++)
                {
                    var instance = (List<object>)instances[n];
                    Vector3 at = Vec(instance[1], Vector3.zero);
                    int body = BodyIndex((Dictionary<string, object>)motions[n], at, bodies);
                    foreach (var (onBody, from, to) in moved)
                        if (onBody == body && (at - from).magnitude < 0.45f)
                        {
                            instance[1] = ToList(at + (to - from));
                            break;
                        }
                }
            }
        }

        // One blade through the body at spot k: its guard just outside the side it enters, its blade (the kit dagger's +y)
        // through the middle and out of the far side. Returns the guard's place.
        private static Vector3 PlaceBlade(List<object> instance, Vector3 body, Vector3 player, bool fromBehind, int k)
        {
            var spot = ImpaleSpots[k % ImpaleSpots.Length];
            Vector3 toPlayer = Flat(player - body);
            Vector3 enters = Quaternion.Euler(0f, spot.turn, 0f) * (fromBehind ? -toPlayer : toPlayer);
            Vector3 guard = body + enters * ImpaleGuard + Vector3.up * spot.height;
            // The blade runs in toward the upright and on through, tilted down.
            Vector3 inward = -enters;
            Vector3 side = Vector3.Cross(Vector3.up, inward).normalized;
            Vector3 blade = Quaternion.AngleAxis(spot.tilt, side) * inward;
            instance[1] = ToList(guard);
            // The kit dagger lies along its own +y from the guard (as built by KitVertices): +y down the blade, its flat facing the side.
            instance[2] = Euler(Quaternion.LookRotation(side, blade));
            instance[3] = (double)ImpaleScale;
            return guard;
        }

        // ---- Iron Thorns: the thorns in the first enemy go down as each flies off to another.

        private static void ShrinkIronThorns(Dictionary<string, object> look)
        {
            var flights = new List<float>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
                if (Text(group, "kit_object") == "vfx3c_vfx_life_thorn")
                    foreach (Dictionary<string, object> motion in (List<object>)group["motion"])
                        if (motion.ContainsKey("path")) flights.Add(Window(motion).x);
            flights.Sort();
            if (flights.Count == 0) return;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (!Text(group, "kit_object").EndsWith("ironthorns")) continue;
                foreach (Dictionary<string, object> motion in (List<object>)group["motion"])
                {
                    var keys = motion.TryGetValue("scale_keys", out var value) && value is List<object> had ? had : new List<object>();
                    float left = 1f;
                    for (int i = 0; i < flights.Count; i++)
                    {
                        float at = flights[i];
                        float next = left * (i == flights.Count - 1 ? 0.3f : 0.68f);
                        keys.Add(new List<object> { (double)at, ToList(Vector3.one * left), "linear" });
                        keys.Add(new List<object> { (double)(at + 0.08f), ToList(Vector3.one * next), "out" });
                        left = next;
                    }
                    motion["scale_keys"] = keys;
                }
            }
        }

        // ---- Golden Feathers: what rode the gust fades as it reaches the others, and a feather comes down on each and settles.

        private static void SettleFeathers(Dictionary<string, object> look)
        {
            var bodies = BodiesOf(look);
            if (bodies.Count < 2) return;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfxe3_feather2") continue;
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                int original = instances.Count;
                for (int n = 0; n < original; n++)
                {
                    var motion = (Dictionary<string, object>)motions[n];
                    if (!(motion.TryGetValue("path", out var pathValue) && pathValue is List<object> path) || path.Count < 2) continue;
                    float end = (float)(double)((List<object>)path[path.Count - 1])[0];
                    // The one riding the gust goes as it reaches them (the one landing on each takes over).
                    var keys = motion.TryGetValue("scale_keys", out var had) && had is List<object> list ? list : new List<object>();
                    keys.Add(new List<object> { (double)(end - 0.2f), ToList(Vector3.one), "linear" });
                    keys.Add(new List<object> { (double)end, ToList(Vector3.one * 0.05f), "in" });
                    motion["scale_keys"] = keys;
                    ((List<object>)motion["visible"])[1] = (double)(end + 0.02f);
                    int target = 1 + n % (bodies.Count - 1);
                    Vector3 from = Flat(bodies[0] - bodies[target]);
                    Vector3 side = Vector3.Cross(Vector3.up, from).normalized;
                    // On the shoulder toward where it came from, lying nearly flat, rocked down as it settles.
                    Vector3 rest = bodies[target] + from * 0.22f + side * (n % 2 == 0 ? 0.12f : -0.12f) + Vector3.up * 1.62f;
                    var instance = (List<object>)DeepCopy(instances[n]);
                    instance[0] = (instance[0] as string ?? "feather") + "_settles";
                    instance[1] = ToList(rest);
                    instance[2] = Euler(Quaternion.LookRotation(side, Vector3.up) * Quaternion.Euler(0f, 0f, 8f));
                    float lands = end - 0.25f;
                    var settle = new Dictionary<string, object>
                    {
                        ["verb"] = "comes down on its shoulder and settles",
                        ["visible"] = new List<object> { (double)(lands - 0.3f), 9.0 },
                        ["body"] = (double)target,
                        ["follow_body"] = (double)target,
                        ["path"] = new List<object>
                        {
                            new List<object> { (double)(lands - 0.3f), ToList(from * 0.9f + Vector3.up * 0.7f), "linear" },
                            new List<object> { (double)lands, ToList(Vector3.up * 0.1f), "out" },
                            new List<object> { (double)(lands + 0.25f), ToList(Vector3.up * 0.03f), "inout" },
                            new List<object> { (double)(lands + 0.5f), ToList(Vector3.zero), "inout" },
                        },
                        ["spin"] = new Dictionary<string, object>
                        {
                            ["axis"] = ToList(from),
                            ["keys"] = new List<object>
                            {
                                new List<object> { (double)(lands - 0.3f), 25.0, "linear" },
                                new List<object> { (double)lands, -12.0, "inout" },
                                new List<object> { (double)(lands + 0.25f), 6.0, "inout" },
                                new List<object> { (double)(lands + 0.5f), 0.0, "inout" },
                            },
                        },
                        ["scale_keys"] = new List<object>
                        {
                            new List<object> { (double)(lands - 0.3f), ToList(Vector3.one * 0.6f), "linear" },
                            new List<object> { (double)(lands - 0.1f), ToList(Vector3.one), "out" },
                        },
                    };
                    instances.Add(instance);
                    motions.Add(settle);
                }
            }
        }

        // ---- Frost Heave: the ice and the body on it rise less, so both stay in the game's frame.

        private static void LowerFrostHeave(Dictionary<string, object> look)
        {
            float rockRose = 0f;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfxe4_rock_c") continue;
                foreach (Dictionary<string, object> motion in (List<object>)group["motion"])
                    rockRose = Mathf.Max(rockRose, ScalePathHeight(motion, FrostHeaveRise, true));
            }
            if (rockRose <= 0f) return;
            var bodies = (List<object>)look["bodies"];
            if (bodies.Count == 0) return;
            var body = (Dictionary<string, object>)bodies[0];
            if (body.TryGetValue("motion", out var value) && value is Dictionary<string, object> bodyMotion)
                ScalePathHeight(bodyMotion, FrostHeaveBody, true);
        }

        // Scales a path's heights so its highest key is <to>, its overshoot taken out (out, not outback); returns the old highest.
        private static float ScalePathHeight(Dictionary<string, object> motion, float to, bool noOvershoot)
        {
            if (!(motion.TryGetValue("path", out var value) && value is List<object> path)) return 0f;
            float highest = 0f;
            foreach (List<object> key in path) highest = Mathf.Max(highest, Vec(key[1], Vector3.zero).y);
            if (highest <= 0.01f) return 0f;
            float lowest = 0f;
            foreach (List<object> key in path) lowest = Mathf.Min(lowest, Vec(key[1], Vector3.zero).y);
            foreach (List<object> key in path)
            {
                Vector3 at = Vec(key[1], Vector3.zero);
                // Only the rise is scaled: a start below the floor stays where it was.
                if (at.y > 0f) at.y *= to / highest;
                key[1] = ToList(at);
                if (noOvershoot && key.Count > 2 && key[2] as string == "outback") key[2] = "out";
            }
            return highest;
        }

        // ---- Stone Rise: the body rides up on the rock's top and comes down as it sinks.

        private static void RideStoneRise(Dictionary<string, object> look)
        {
            Dictionary<string, object> rockMotion = null;
            float tall = 0f;
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfxe4_rock_c") continue;
                var instance = (List<object>)((List<object>)group["instances"])[0];
                rockMotion = (Dictionary<string, object>)((List<object>)group["motion"])[0];
                // The kit's rock_c stands 1.15 m on its base, grown by its own height scale.
                Vector3 scale = instance[3] is List<object> ? Vec(instance[3], Vector3.one) : Vector3.one * (float)(double)instance[3];
                tall = 1.15f * scale.y + Vec(instance[1], Vector3.zero).y;
            }
            if (rockMotion == null || !(rockMotion.TryGetValue("path", out var value) && value is List<object> rockPath)) return;
            var bodies = (List<object>)look["bodies"];
            if (bodies.Count == 0) return;
            var body = (Dictionary<string, object>)bodies[0];
            if (!(body.TryGetValue("motion", out var bodyValue) && bodyValue is Dictionary<string, object> bodyMotion)) return;
            // The body stands on the rock's top: the top's height at each of the rock's keys, never under the floor.
            var path = new List<object>();
            foreach (List<object> key in rockPath)
            {
                float top = Mathf.Max(0f, tall + Vec(key[1], Vector3.zero).y);
                path.Add(new List<object> { key[0], ToList(Vector3.up * top), key.Count > 2 ? key[2] : "linear" });
            }
            // Before the rock reaches its feet it stands on the floor.
            var first = (List<object>)path[0];
            path.Insert(0, new List<object> { first[0], ToList(Vector3.zero), "linear" });
            bodyMotion["path"] = path;
        }

        // ---- Briar Patch: the thorns stand on the plants, a little bigger than the plants' own, and grow up with them.

        private static void ThornsOnBriars(Dictionary<string, object> look)
        {
            var briars = new List<(Vector3 at, float tall)>();
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfxe3_briar") continue;
                foreach (List<object> instance in (List<object>)group["instances"])
                {
                    Vector3 scale = instance[3] is List<object> ? Vec(instance[3], Vector3.one) : Vector3.one * (float)(double)instance[3];
                    briars.Add((Vec(instance[1], Vector3.zero), 1.23f * scale.y));
                }
            }
            if (briars.Count == 0) return;
            var bodies = BodiesOf(look);
            foreach (Dictionary<string, object> group in (List<object>)look["groups"])
            {
                if (Text(group, "kit_object") != "vfx3c_vfx_life_thorn") continue;
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                for (int n = 0; n < instances.Count; n++)
                {
                    var instance = (List<object>)instances[n];
                    var motion = (Dictionary<string, object>)motions[n];
                    Vector3 was = Vec(instance[1], Vector3.zero);
                    // The ones at the enemies' feet stay theirs (and are left out); the patch's go up onto its plants.
                    if (bodies.Count > 0 && BodyIndex(motion, was, bodies) > 0) continue;
                    int nearest = 0;
                    for (int b = 1; b < briars.Count; b++)
                        if ((briars[b].at - was).sqrMagnitude < (briars[nearest].at - was).sqrMagnitude) nearest = b;
                    var briar = briars[nearest];
                    Vector3 outward = Flat(was - briar.at);
                    float height = briar.tall * (0.3f + 0.45f * ((n * 7) % 10) / 10f);
                    Vector3 at = briar.at + outward * 0.12f + Vector3.up * height;
                    instance[1] = ToList(at);
                    instance[2] = Euler(Quaternion.FromToRotation(Vector3.up, (outward + Vector3.up * 0.6f).normalized));
                    instance[3] = ToList(new Vector3(0.75f, 1.1f, 0.75f) * 0.32f);
                    // It rises with its plant out of the ground.
                    motion["path"] = new List<object>
                    {
                        new List<object> { 0.12, ToList(Vector3.down * height), "linear" },
                        new List<object> { 0.32, ToList(Vector3.zero), "out" },
                    };
                    motion["scale_keys"] = new List<object>
                    {
                        new List<object> { 0.12, ToList(Vector3.one * 0.2f), "linear" },
                        new List<object> { 0.34, ToList(Vector3.one), "outback" },
                    };
                    motion["visible"] = new List<object> { 0.12, 9.0 };
                    motion["verb"] = "a hooked thorn on the briar, a little bigger than its own";
                }
            }
        }

        // ---- Seeded: the seeds round the chest, evenly spaced, golden-brown with a spring-green sprout at each tip.

        private static void EvenSeeds(Dictionary<string, object> look)
        {
            var bodies = BodiesOf(look);
            foreach (Dictionary<string, object> group in new List<object>((List<object>)look["groups"]))
            {
                if (Text(group, "kit_object") != "vfxe2_spore") continue;
                var instances = (List<object>)group["instances"];
                var motions = (List<object>)group["motion"];
                // Each body's seeds, in order, spread evenly round it.
                var byBody = new Dictionary<int, List<int>>();
                for (int n = 0; n < instances.Count; n++)
                {
                    int body = BodyIndex((Dictionary<string, object>)motions[n], Vec(((List<object>)instances[n])[1], Vector3.zero), bodies);
                    if (!byBody.TryGetValue(body, out var list)) byBody[body] = list = new List<int>();
                    list.Add(n);
                }
                var tips = new List<object>();
                var tipMotions = new List<object>();
                foreach (var pair in byBody)
                {
                    Vector3 centre = pair.Key >= 0 && pair.Key < bodies.Count ? bodies[pair.Key] : Vector3.zero;
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        int n = pair.Value[i];
                        var instance = (List<object>)instances[n];
                        var motion = (Dictionary<string, object>)motions[n];
                        float angle = i * 360f / pair.Value.Count;
                        Vector3 at = centre + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * SeedRing + Vector3.up * BodyAnchors.StandIn.Chest;
                        instance[1] = ToList(at);
                        // One bob for all, so the ring stays even as it turns.
                        if (motion.TryGetValue("path", out var pathValue) && pathValue is List<object> path && pair.Value.Count > 0)
                            motion["path"] = DeepCopy(((Dictionary<string, object>)motions[pair.Value[0]])["path"]);
                        if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is Dictionary<string, object> orbit)
                        {
                            if (pair.Value.Count > 0 && ((Dictionary<string, object>)motions[pair.Value[0]]).TryGetValue("orbit", out var firstOrbit))
                                orbit["keys"] = DeepCopy(((Dictionary<string, object>)firstOrbit)["keys"]);
                        }
                        // The sprout at its tip, riding the same way.
                        float seedScale = instance[3] is double s ? (float)s : 3.22f;
                        var tip = new List<object> { (instance[0] as string ?? "seed") + "_sprout", ToList(at + Vector3.up * 0.085f * seedScale), ToList(Vector3.zero), (double)(seedScale * 0.45f), "b" };
                        tips.Add(tip);
                        tipMotions.Add(DeepCopy(motion));
                    }
                }
                if (tips.Count == 0) continue;
                ((List<object>)look["groups"]).Add(new Dictionary<string, object>
                {
                    ["mesh"] = "e2k_sprout", ["kit"] = "vfx_elements_kit_e2.fbx", ["kit_object"] = "vfxe2_sprout", ["material"] = "e4_seed_sprout",
                    ["pass"] = "round4", ["instances"] = tips, ["motion"] = tipMotions,
                });
            }
        }
    }
}
