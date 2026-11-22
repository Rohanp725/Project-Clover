using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The standing things a spell or a reaction raises (the ledger's standing-things, standin_clearance_plan.md pass 11).
    /// Each kind names the model the editor step brings in from staging/blender/2026-11-22_standin_art/ and the collider its
    /// notes.md gives. The prefab made from each is registered by a <see cref="StandingLibrary"/> in the scene.
    /// </summary>
    public enum StandingKind
    {
        /// <summary>anvil_stone.fbx: the iron-faced stone (Anvil Stone, "stone at its back"); faces the player; box 1.15 x 1.5 x 0.8, 0.75 up.</summary>
        AnvilStone,
        /// <summary>rimed_mound.fbx: Frost Heave's mound; round; cylinder 0.72 m radius, 0.8 m high; its ice shards for Pingo's burst.</summary>
        RimedMound,
        /// <summary>hollow_trunk.fbx: the trunk a girdled enemy leaves; round; capsule 0.45 m radius, 2 m high; carve 0.55 m.</summary>
        HollowTrunk,
        /// <summary>ice_wall.fbx, ice_wall_5m_lod0 (and ice_wall_5m_cracked_lod0 when first struck): Winter Wall; box 5.0 x 2.2 x 0.7, 1.1 up.</summary>
        IceWall,
        /// <summary>ice_wall.fbx, ice_wall_3m_lod0 (and ice_wall_3m_cracked_lod0): Shield Wall; box 3.0 x 2.2 x 0.7, 1.1 up.</summary>
        ShieldWall,
        /// <summary>stone_pillar.fbx: Stone Rise's Raised Stone; round; capsule 0.45 m radius, 2.4 m high; carve 0.6 m.</summary>
        StonePillar,
    }

    /// <summary>How a standing thing's collider and NavMesh carve are made (from each model's notes.md), for the editor step and for tests.</summary>
    public readonly struct StandingShape
    {
        /// <summary>True for a box (Size is its x, y, z); false for an upright capsule or cylinder (Size.x is its radius, Size.y its height).</summary>
        public readonly bool Box;
        public readonly Vector3 Size;
        /// <summary>The collider's centre above the root at the base centre.</summary>
        public readonly float CentreUp;
        /// <summary>The NavMesh carve's radius for a round thing (0: carve the box itself).</summary>
        public readonly float Carve;
        /// <summary>True when the thing has a front, turned toward the player (the anvil's iron face).</summary>
        public readonly bool Faces;
        /// <summary>The model file and, for a wall, the object shown whole and the one swapped in when it is first struck.</summary>
        public readonly string Model, Whole, Cracked;

        public StandingShape(bool box, Vector3 size, float centreUp, float carve, bool faces, string model, string whole, string cracked)
        {
            Box = box;
            Size = size;
            CentreUp = centreUp;
            Carve = carve;
            Faces = faces;
            Model = model;
            Whole = whole;
            Cracked = cracked;
        }

        public static StandingShape For(StandingKind kind)
        {
            switch (kind)
            {
                case StandingKind.AnvilStone: return new StandingShape(true, new Vector3(1.15f, 1.5f, 0.8f), 0.75f, 0f, true, "anvil_stone", "anvil_stone_lod0", "");
                case StandingKind.RimedMound: return new StandingShape(false, new Vector3(0.72f, 0.8f, 0f), 0.4f, 0.72f, false, "rimed_mound", "rimed_mound_lod0", "");
                case StandingKind.HollowTrunk: return new StandingShape(false, new Vector3(0.45f, 2f, 0f), 1f, 0.55f, false, "hollow_trunk", "hollow_trunk_lod0", "");
                case StandingKind.IceWall: return new StandingShape(true, new Vector3(5f, 2.2f, 0.7f), 1.1f, 0f, false, "ice_wall", "ice_wall_5m_lod0", "ice_wall_5m_cracked_lod0");
                case StandingKind.ShieldWall: return new StandingShape(true, new Vector3(3f, 2.2f, 0.7f), 1.1f, 0f, false, "ice_wall", "ice_wall_3m_lod0", "ice_wall_3m_cracked_lod0");
                default: return new StandingShape(false, new Vector3(0.45f, 2.4f, 0f), 1.2f, 0.6f, false, "stone_pillar", "stone_pillar_lod0", "");
            }
        }
    }

    /// <summary>Where a mark raises its standing thing, against the enemy its moment names.</summary>
    public enum StandingPlace
    {
        /// <summary>Where the enemy stands, or fell (the hollow trunk).</summary>
        At,
        /// <summary>Behind the enemy as you see it, its front to you (the anvil "at its back", the heaved mound).</summary>
        Behind,
        /// <summary>Beside the enemy, to one side of your line to it (Raised Stone's pillar).</summary>
        Beside,
        /// <summary>Ahead of you, along the way you face.</summary>
        Ahead,
        /// <summary>In front of the enemy, on your side of it, at its feet (Shining Herb's herb).</summary>
        Before,
    }

    /// <summary>A blow that landed on a standing thing, kept until the player's marks read it.</summary>
    public struct StandingNews
    {
        public int Slot;
        public Standing Thing;
        public StandingStruck Struck;
        /// <summary>The blow was the player's heavy blow (Raised Stone shatters under one).</summary>
        public bool Heavy;
        /// <summary>The blow was the player's (not an enemy's).</summary>
        public bool ByPlayer;
    }

    /// <summary>What a blow on a standing thing did to it.</summary>
    public enum StandingStruck
    {
        /// <summary>Nothing stands there (it was already broken or gone).</summary>
        None,
        Standing,
        /// <summary>Its first blow: a thing that bursts after being struck starts its clock now (Cracked Wall), and shows cracked.</summary>
        FirstStruck,
        Broken,
    }

    /// <summary>One standing thing: what it is, where, its health and its clocks. Plain data: the rules below move it.</summary>
    public struct Standing
    {
        public bool Active;
        public StandingKind Kind;
        public Vector3 Position;
        public Vector3 Forward;
        public float Health;
        public float Until;
        /// <summary>Above zero: it bursts this long after it is first struck (Cracked Wall: 1.5 s).</summary>
        public float BurstAfterStruck;
        public float FirstStruckAt;
        public bool Struck;
        /// <summary>Who raised it, by the raising mark's index (-1: none), so a mark can count, find and break its own.</summary>
        public int Owner;
        /// <summary>Above zero: enemies within this many metres turn on it and strike it instead of you (a thing that draws them).</summary>
        public float Draws;
        public float RaisedAt;
    }

    /// <summary>The plain rules of a standing thing.</summary>
    public static class StandingRules
    {
        /// <summary>A blow of <paramref name="damage"/> lands on it at <paramref name="now"/>.</summary>
        public static StandingStruck Strike(ref Standing thing, float damage, float now)
        {
            if (!thing.Active) return StandingStruck.None;
            bool first = !thing.Struck;
            if (first)
            {
                thing.Struck = true;
                thing.FirstStruckAt = now;
            }
            if (thing.Health > 0f)
            {
                thing.Health -= Mathf.Max(0f, damage);
                if (thing.Health <= 0f)
                {
                    thing.Active = false;
                    return StandingStruck.Broken;
                }
            }
            return first ? StandingStruck.FirstStruck : StandingStruck.Standing;
        }

        /// <summary>True when its time is up.</summary>
        public static bool Expired(in Standing thing, float now) => thing.Active && thing.Until > 0f && now >= thing.Until;

        /// <summary>True when a thing that bursts after being struck is due to burst now.</summary>
        public static bool BurstDue(in Standing thing, float now) =>
            thing.Active && thing.BurstAfterStruck > 0f && thing.Struck && now - thing.FirstStruckAt >= thing.BurstAfterStruck;

        /// <summary>
        /// Where a mark raises its thing: at, behind or beside the enemy at <paramref name="enemy"/> as seen from the player at
        /// <paramref name="player"/>, <paramref name="gap"/> metres off, or that far ahead of the player along <paramref name="forward"/>.
        /// </summary>
        public static Vector3 PlaceFor(StandingPlace place, Vector3 player, Vector3 forward, Vector3 enemy, float gap)
        {
            Vector3 line = enemy - player;
            line.y = 0f;
            Vector3 way = line.sqrMagnitude > 0.0001f ? line.normalized : (forward.sqrMagnitude > 0.0001f ? new Vector3(forward.x, 0f, forward.z).normalized : Vector3.forward);
            switch (place)
            {
                case StandingPlace.Behind: return enemy + way * gap;
                case StandingPlace.Beside: return enemy + Vector3.Cross(Vector3.up, way) * gap;
                case StandingPlace.Before: return enemy - way * gap;
                case StandingPlace.Ahead:
                    Vector3 ahead = forward;
                    ahead.y = 0f;
                    return player + (ahead.sqrMagnitude > 0.0001f ? ahead.normalized : way) * gap;
                default: return enemy;
            }
        }

        /// <summary>How far round the thing's middle a body must keep: its round radius, or half its box's longer side.</summary>
        public static float Reach(StandingKind kind)
        {
            var shape = StandingShape.For(kind);
            return shape.Box ? Mathf.Max(shape.Size.x, shape.Size.z) * 0.5f : shape.Size.x;
        }

        /// <summary>True when a point within <paramref name="radius"/> of <paramref name="at"/> touches the thing's footprint on the ground.</summary>
        public static bool Touches(in Standing thing, Vector3 at, float radius)
        {
            if (!thing.Active) return false;
            var shape = StandingShape.For(thing.Kind);
            Vector3 offset = at - thing.Position;
            offset.y = 0f;
            if (!shape.Box) return offset.magnitude <= shape.Size.x + radius;
            Vector3 across = Vector3.Cross(Vector3.up, thing.Forward);
            float x = Mathf.Abs(Vector3.Dot(offset, across)), z = Mathf.Abs(Vector3.Dot(offset, thing.Forward));
            return x <= shape.Size.x * 0.5f + radius && z <= shape.Size.z * 0.5f + radius;
        }

        /// <summary>True when the straight line from <paramref name="from"/> to <paramref name="to"/> passes through the thing (a blow or a throw it stops).</summary>
        public static bool Blocks(in Standing thing, Vector3 from, Vector3 to)
        {
            if (!thing.Active) return false;
            Vector3 line = to - from;
            line.y = 0f;
            float length = line.magnitude;
            if (length < 0.01f) return false;
            Vector3 way = line / length;
            // Sampled along the line, a quarter metre apart: walls are thick enough that nothing slips between the samples.
            for (float along = 0.25f; along < length - 0.25f; along += 0.25f)
                if (Touches(thing, from + way * along, 0f)) return true;
            return false;
        }

        /// <summary>
        /// A body at <paramref name="at"/> about to move by <paramref name="step"/>, kept off the thing: a step that would
        /// take it inside is turned along the thing's edge, to whichever side it was already leaning. Unchanged when clear.
        /// </summary>
        public static Vector3 Around(in Standing thing, Vector3 at, Vector3 step, float bodyRadius)
        {
            Vector3 flat = new Vector3(step.x, 0f, step.z);
            if (!thing.Active || flat.sqrMagnitude < 0.000001f) return step;
            // Looked at a body's width ahead, so it turns before it is pressed against the thing.
            Vector3 ahead = at + flat.normalized * Mathf.Max(flat.magnitude, bodyRadius);
            if (!Touches(thing, ahead, bodyRadius)) return step;
            Vector3 toThing = thing.Position - at;
            toThing.y = 0f;
            if (toThing.sqrMagnitude < 0.0001f) return step;
            Vector3 side;
            if (StandingShape.For(thing.Kind).Box)
            {
                // Along a wall's face, toward its nearer end.
                side = Vector3.Cross(Vector3.up, thing.Forward);
                if (Vector3.Dot(side, -toThing) < 0f) side = -side;
            }
            else
            {
                // Round a stone, to whichever side it was already leaning.
                side = Vector3.Cross(Vector3.up, toThing.normalized);
                if (Vector3.Dot(side, flat) < 0f) side = -side;
            }
            Vector3 turned = side * flat.magnitude;
            return new Vector3(turned.x, step.y, turned.z);
        }

        /// <summary>True when <paramref name="at"/> lies behind the thing as seen from <paramref name="from"/>, within <paramref name="reach"/> of it: "stone at its back".</summary>
        public static bool AtItsBack(in Standing thing, Vector3 from, Vector3 at, float reach)
        {
            if (!thing.Active) return false;
            Vector3 toThing = thing.Position - at;
            toThing.y = 0f;
            if (toThing.sqrMagnitude > reach * reach) return false;
            Vector3 push = at - from;
            push.y = 0f;
            return push.sqrMagnitude > 0.0001f && Vector3.Dot(push.normalized, toThing) > 0f;
        }
    }

    /// <summary>
    /// Every standing thing in the scene, a few at a time (the oldest gives way to a new one when all are taken). Marks raise,
    /// count, find and break them here; the bodies (<see cref="StandingThingBody"/>) show them and take the blows.
    /// </summary>
    public static class StandingThings
    {
        public const int Most = 16;
        private static readonly Standing[] things = new Standing[Most];
        private static readonly StandingThingBody[] bodies = new StandingThingBody[Most];
        private static readonly StandingThingBody[] prefabs = new StandingThingBody[System.Enum.GetValues(typeof(StandingKind)).Length];
        private static int next;
        private const int MostNews = 16;
        private static readonly StandingNews[] news = new StandingNews[MostNews];
        private static int newsCount;
        private static int tickedFrame = -1;

        // Domain reload is off: nothing raised in the last play may stand in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        /// <summary>Forgets every standing thing and every prefab (a new play, a test).</summary>
        public static void Clear()
        {
            for (int i = 0; i < Most; i++)
            {
                things[i] = default;
                bodies[i] = null;
            }
            for (int i = 0; i < prefabs.Length; i++) prefabs[i] = null;
            next = 0;
            newsCount = 0;
            tickedFrame = -1;
        }

        /// <summary>The prefab a kind is raised from, set by the scene's <see cref="StandingLibrary"/>; null until the editor step wires it.</summary>
        public static void SetPrefab(StandingKind kind, StandingThingBody prefab) => prefabs[(int)kind] = prefab;
        public static StandingThingBody PrefabOf(StandingKind kind) => prefabs[(int)kind];

        /// <summary>
        /// Raises a standing thing at <paramref name="at"/>, its front along <paramref name="forward"/>, for <paramref name="seconds"/>
        /// (0: until broken) with <paramref name="health"/> (0: it cannot be broken by blows). Its body is made from the kind's
        /// prefab when one is wired. Returns its place in the list.
        /// </summary>
        public static int Raise(StandingKind kind, Vector3 at, Vector3 forward, float seconds, float health, float now, int owner = -1, float burstAfterStruck = 0f, float draws = 0f,
            bool showBody = true)
        {
            int slot = FreeSlot();
            End(slot, false);
            forward.y = 0f;
            things[slot] = new Standing
            {
                Active = true, Kind = kind, Position = at, Forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward,
                Health = health, Until = seconds > 0f ? now + seconds : 0f, BurstAfterStruck = burstAfterStruck, Owner = owner, FirstStruckAt = -100f, Draws = draws, RaisedAt = now,
            };
            // A thing drawn by its own form (Stone Oak's tree stands in the way as a pillar would) takes no body of its kind.
            var prefab = showBody ? prefabs[(int)kind] : null;
            if (prefab != null)
            {
                var body = Object.Instantiate(prefab, at, Quaternion.LookRotation(things[slot].Forward, Vector3.up));
                body.Bind(slot);
                bodies[slot] = body;
            }
            return slot;
        }

        /// <summary>The thing in a slot, as it is now.</summary>
        public static Standing Get(int slot) => slot >= 0 && slot < Most ? things[slot] : default;

        /// <summary>
        /// A blow on the thing in <paramref name="slot"/>; a broken thing's body breaks apart. The blow is kept as news for the
        /// player's marks (<see cref="TakeNews"/>): whose it was and whether it was heavy.
        /// </summary>
        public static StandingStruck Strike(int slot, float damage, float now, bool heavy = false, bool byPlayer = false)
        {
            if (slot < 0 || slot >= Most) return StandingStruck.None;
            var struck = StandingRules.Strike(ref things[slot], damage, now);
            if (struck != StandingStruck.None && newsCount < MostNews)
                news[newsCount++] = new StandingNews { Slot = slot, Thing = things[slot], Struck = struck, Heavy = heavy, ByPlayer = byPlayer };
            if (bodies[slot] != null)
            {
                if (struck == StandingStruck.FirstStruck) bodies[slot].ShowCracked();
                if (struck == StandingStruck.Broken) End(slot, true);
            }
            return struck;
        }

        /// <summary>Ends the things whose time is up, and returns through <paramref name="burst"/> one due to burst now (-1 for none).</summary>
        public static void Tick(float now, out int burst)
        {
            burst = -1;
            for (int i = 0; i < Most; i++)
            {
                if (!things[i].Active) continue;
                if (burst < 0 && StandingRules.BurstDue(things[i], now))
                {
                    burst = i;
                    End(i, true);
                    continue;
                }
                if (StandingRules.Expired(things[i], now)) End(i, false);
            }
        }

        /// <summary>Takes the oldest blow on a standing thing not yet read; false when none waits.</summary>
        public static bool TakeNews(out StandingNews item)
        {
            item = default;
            if (newsCount == 0) return false;
            item = news[0];
            for (int i = 1; i < newsCount; i++) news[i - 1] = news[i];
            newsCount--;
            return true;
        }

        /// <summary>
        /// The clock, run once a frame whoever asks first (the scene's library, the player): ends what is due, and returns
        /// through <paramref name="burst"/> one that burst by itself this frame (-1 for none).
        /// </summary>
        public static void TickOnce(float now, int frame, out int burst)
        {
            burst = -1;
            if (frame == tickedFrame) return;
            tickedFrame = frame;
            Tick(now, out burst);
        }

        /// <summary>The first standing thing on the straight line between two places (a blow or a throw it stops); -1 for none.</summary>
        public static int FirstOnLine(Vector3 from, Vector3 to)
        {
            int best = -1;
            float bestSq = float.MaxValue;
            for (int i = 0; i < Most; i++)
            {
                if (!StandingRules.Blocks(things[i], from, to)) continue;
                Vector3 offset = things[i].Position - from;
                offset.y = 0f;
                if (offset.sqrMagnitude >= bestSq) continue;
                bestSq = offset.sqrMagnitude;
                best = i;
            }
            return best;
        }

        /// <summary>A body's step kept off every standing thing near it (enemies walk round them).</summary>
        public static Vector3 SteerAround(Vector3 at, Vector3 step, float bodyRadius)
        {
            for (int i = 0; i < Most; i++)
            {
                if (!things[i].Active) continue;
                Vector3 offset = things[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude > 36f) continue;
                step = StandingRules.Around(things[i], at, step, bodyRadius);
            }
            return step;
        }

        /// <summary>The standing thing that draws an enemy at <paramref name="at"/> onto itself (the nearest whose pull reaches it); -1 for none.</summary>
        public static int DrawnTo(Vector3 at)
        {
            int best = -1;
            float bestSq = float.MaxValue;
            for (int i = 0; i < Most; i++)
            {
                if (!things[i].Active || things[i].Draws <= 0f) continue;
                Vector3 offset = things[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude > things[i].Draws * things[i].Draws || offset.sqrMagnitude >= bestSq) continue;
                bestSq = offset.sqrMagnitude;
                best = i;
            }
            return best;
        }

        /// <summary>The body showing the thing in a slot (null until the editor step wires its prefab).</summary>
        public static StandingThingBody BodyOf(int slot) => slot >= 0 && slot < Most ? bodies[slot] : null;

        /// <summary>Breaks the thing of <paramref name="owner"/> standing nearest <paramref name="at"/> within <paramref name="within"/>; true when one broke.</summary>
        public static bool BreakNear(Vector3 at, float within, int owner)
        {
            int slot = Nearest(at, within, owner);
            if (slot < 0) return false;
            End(slot, true);
            return true;
        }

        /// <summary>Keeps at most <paramref name="most"/> of an owner's things standing: the oldest crumble first (Stone Remembers' eight).</summary>
        public static void KeepAtMost(int owner, int most)
        {
            while (Count(owner) > Mathf.Max(0, most))
            {
                int oldest = -1;
                for (int i = 0; i < Most; i++)
                {
                    if (!things[i].Active || things[i].Owner != owner) continue;
                    if (oldest < 0 || things[i].RaisedAt < things[oldest].RaisedAt) oldest = i;
                }
                if (oldest < 0) return;
                End(oldest, true);
            }
        }

        /// <summary>The nearest standing thing within <paramref name="within"/> of a place (of a kind, or of an owner, when given); -1 for none.</summary>
        public static int Nearest(Vector3 at, float within, int owner = -1)
        {
            int best = -1;
            float bestSq = within * within;
            for (int i = 0; i < Most; i++)
            {
                if (!things[i].Active || (owner >= 0 && things[i].Owner != owner)) continue;
                Vector3 offset = things[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude > bestSq) continue;
                bestSq = offset.sqrMagnitude;
                best = i;
            }
            return best;
        }

        /// <summary>True when a standing thing of <paramref name="kind"/> stands within <paramref name="within"/> of a place (Frozen Footing: by a mound).</summary>
        public static bool NearKind(Vector3 at, float within, StandingKind kind)
        {
            for (int i = 0; i < Most; i++)
            {
                if (!things[i].Active || things[i].Kind != kind) continue;
                if (StandingRules.Touches(things[i], at, within)) return true;
            }
            return false;
        }

        /// <summary>How many stand now, of one owner when given (Standing Stones: two at once).</summary>
        public static int Count(int owner = -1)
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
                if (things[i].Active && (owner < 0 || things[i].Owner == owner)) count++;
            return count;
        }

        /// <summary>Ends the thing in a slot: its body broken apart (<paramref name="broken"/>) or simply gone.</summary>
        public static void End(int slot, bool broken)
        {
            if (slot < 0 || slot >= Most) return;
            things[slot].Active = false;
            var body = bodies[slot];
            bodies[slot] = null;
            if (body != null) body.Fall(broken);
        }

        // A free place, or, when all are taken, the next in turn (round the list, so the longest-standing go first).
        private static int FreeSlot()
        {
            for (int i = 0; i < Most; i++)
                if (!things[i].Active) return i;
            int slot = next;
            next = (next + 1) % Most;
            return slot;
        }
    }
}
