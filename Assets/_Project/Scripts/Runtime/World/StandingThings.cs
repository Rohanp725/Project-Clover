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
        }

        /// <summary>The prefab a kind is raised from, set by the scene's <see cref="StandingLibrary"/>; null until the editor step wires it.</summary>
        public static void SetPrefab(StandingKind kind, StandingThingBody prefab) => prefabs[(int)kind] = prefab;
        public static StandingThingBody PrefabOf(StandingKind kind) => prefabs[(int)kind];

        /// <summary>
        /// Raises a standing thing at <paramref name="at"/>, its front along <paramref name="forward"/>, for <paramref name="seconds"/>
        /// (0: until broken) with <paramref name="health"/> (0: it cannot be broken by blows). Its body is made from the kind's
        /// prefab when one is wired. Returns its place in the list.
        /// </summary>
        public static int Raise(StandingKind kind, Vector3 at, Vector3 forward, float seconds, float health, float now, int owner = -1, float burstAfterStruck = 0f)
        {
            int slot = FreeSlot();
            End(slot, false);
            forward.y = 0f;
            things[slot] = new Standing
            {
                Active = true, Kind = kind, Position = at, Forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward,
                Health = health, Until = seconds > 0f ? now + seconds : 0f, BurstAfterStruck = burstAfterStruck, Owner = owner, FirstStruckAt = -100f,
            };
            var prefab = prefabs[(int)kind];
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

        /// <summary>A blow on the thing in <paramref name="slot"/>; a broken thing's body breaks apart.</summary>
        public static StandingStruck Strike(int slot, float damage, float now)
        {
            if (slot < 0 || slot >= Most) return StandingStruck.None;
            var struck = StandingRules.Strike(ref things[slot], damage, now);
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
