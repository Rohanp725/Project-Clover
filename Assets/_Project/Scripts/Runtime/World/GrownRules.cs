using UnityEngine;

namespace Clover.World
{
    /// <summary>The plain rules of ground that keeps acting on what stands in it (the ledger's ground-states, pull-to-point, mending-ground).</summary>
    public static class GroundRules
    {
        /// <summary>
        /// The shove each bite gives toward the middle for a pull of <paramref name="metresPerSecond"/>: a shove is spent over a
        /// moment and moves about two thirds of its length, so half as much again of the seconds between bites.
        /// </summary>
        public static float PullStep(float metresPerSecond, float biteEvery) => metresPerSecond * Mathf.Max(0.05f, biteEvery) * 1.5f;

        /// <summary>The health a bite of mending ground gives a player standing in it, for <paramref name="perSecond"/>.</summary>
        public static float MendStep(float perSecond, float biteEvery) => perSecond * Mathf.Max(0.05f, biteEvery);

        /// <summary>
        /// The seconds a state set by ground lasts on whoever stands in it: a little past the next bite, so it holds while they
        /// stay and goes soon after they step out.
        /// </summary>
        public static float StateSeconds(float biteEvery) => Mathf.Max(0.05f, biteEvery) + 0.25f;

        /// <summary>How far one must have gone between two bites to count as moving on ground that bites only the moving.</summary>
        public const float MovedAtLeast = 0.15f;

        /// <summary>True when one seen at <paramref name="before"/> and now at <paramref name="now"/> moved, flat, between two bites.</summary>
        public static bool Moved(Vector3 before, Vector3 now)
        {
            Vector3 gap = now - before;
            gap.y = 0f;
            return gap.magnitude >= MovedAtLeast;
        }

        /// <summary>How far either side of its line a seam of a star of cracks strikes, in metres.</summary>
        public const float SeamHalf = 0.5f;

        /// <summary>The way the seam numbered <paramref name="index"/> of <paramref name="seams"/> runs, the first along <paramref name="first"/>.</summary>
        public static Vector3 SeamWay(Vector3 first, int index, int seams)
        {
            first.y = 0f;
            if (first.sqrMagnitude < 0.0001f) first = Vector3.forward;
            // Turned about the up axis clockwise seen from above, as a yaw turns (worked out by hand: plain arithmetic, no engine call).
            first.Normalize();
            float turn = index * 2f * Mathf.PI / Mathf.Max(1, seams), cos = Mathf.Cos(turn), sin = Mathf.Sin(turn);
            return new Vector3(first.x * cos + first.z * sin, 0f, first.z * cos - first.x * sin);
        }

        /// <summary>True when <paramref name="point"/> stands on one of a star's seams, each <paramref name="length"/> long from <paramref name="middle"/>.</summary>
        public static bool OnStar(Vector3 point, Vector3 middle, int seams, float length, float half, Vector3 first)
        {
            for (int i = 0; i < seams; i++)
                if (OnLane(point, middle, SeamWay(first, i, seams), length, half)) return true;
            return false;
        }

        /// <summary>
        /// True when <paramref name="point"/> stands on a lane <paramref name="length"/> long and <paramref name="half"/> either side
        /// of its line, running from <paramref name="from"/> along <paramref name="way"/> (flat; the ends rounded by its width).
        /// </summary>
        public static bool OnLane(Vector3 point, Vector3 from, Vector3 way, float length, float half)
        {
            way.y = 0f;
            if (way.sqrMagnitude < 0.0001f) return Within(point, from, half);
            way.Normalize();
            Vector3 gap = point - from;
            gap.y = 0f;
            float along = Mathf.Clamp(Vector3.Dot(gap, way), 0f, Mathf.Max(0f, length));
            return (gap - way * along).magnitude <= half;
        }

        /// <summary>True when a point within the ground's radius of its middle stands in it.</summary>
        public static bool Within(Vector3 at, Vector3 middle, float radius)
        {
            Vector3 gap = at - middle;
            gap.y = 0f;
            return gap.magnitude <= radius;
        }

        /// <summary>
        /// True when the flat step from <paramref name="a"/> to <paramref name="b"/> crosses the line <paramref name="length"/> long
        /// from <paramref name="from"/> along <paramref name="way"/> (a blow struck across The Drawn Line).
        /// </summary>
        public static bool Crosses(Vector3 a, Vector3 b, Vector3 from, Vector3 way, float length)
        {
            way.y = 0f;
            if (way.sqrMagnitude < 0.0001f || length <= 0f) return false;
            way = way.normalized * length;
            float ax = a.x - from.x, az = a.z - from.z, bx = b.x - from.x, bz = b.z - from.z;
            // Which side of the line each end lies on, and which side of the step each end of the line lies on.
            float sideA = way.x * az - way.z * ax, sideB = way.x * bz - way.z * bx;
            if (sideA * sideB > 0f || (sideA == 0f && sideB == 0f)) return false;
            float sx = bx - ax, sz = bz - az;
            float startSide = sx * (0f - az) - sz * (0f - ax), endSide = sx * (way.z - az) - sz * (way.x - ax);
            return startSide * endSide <= 0f;
        }

        /// <summary>
        /// True when a body that was at <paramref name="before"/> and is now at <paramref name="now"/> stepped across the circle
        /// <paramref name="radius"/> round <paramref name="middle"/>, in or out (The Closed Ring's wall).
        /// </summary>
        public static bool CrossesRing(Vector3 before, Vector3 now, Vector3 middle, float radius) =>
            Within(before, middle, radius) != Within(now, middle, radius);
    }

    /// <summary>
    /// The player's own grounds by name, those a spell's form draws (Black Ice's sheet, The Drawn Line's spears) and those
    /// a mark lays (wet sand, the lamp's glow), so the stats and the marks can ask who stands in one (the ledger's
    /// in-own-ground), and an enemy shoved on a slick one goes farther. Pure: the caller says what time it is.
    /// </summary>
    public static class OwnGrounds
    {
        /// <summary>Hoarfrost's Black Ice: a sheet of clear ice.</summary>
        public const string Sheet = "ice.sheet";
        /// <summary>The Ground Gives' wet sand, The Land Was Sea's trail, The Island Breathes' out-breath.</summary>
        public const string WetSand = "earth.wetsand";
        /// <summary>Miasma's Low Mist: a sheet of mist along the floor.</summary>
        public const string LowMist = "poison.lowmist";
        /// <summary>Standing Light's glow round its lamp, and Row of Lamps' row.</summary>
        public const string Glow = "light.glow";
        /// <summary>Drawn From the Wood's Blood in the Furrow: the ring of green blood.</summary>
        public const string Blood = "life.blood";
        /// <summary>Ring of Spears' The Drawn Line: the row of spears across your front.</summary>
        public const string SpearLine = "light.spearline";
        /// <summary>Miasma's own cloud as it hangs and creeps (laid afresh at each of its bites).</summary>
        public const string Cloud = "poison.cloud";
        /// <summary>The Closed Ring: Bedrock's ring of standing stones.</summary>
        public const string StoneRing = "earth.stonering";

        private struct Patch
        {
            public bool Active;
            public string Name;
            public Vector3 At, Way;
            public float Length, Radius, Until, Thrown;
        }

        /// <summary>How many grounds can lie at once; one more takes the place of the one nearest its end.</summary>
        public const int Most = 24;
        private static readonly Patch[] patches = new Patch[Most];

        // Domain reload is off: no ground from the last play may lie in this one, and no listener from it be told.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
            Stopped = null;
        }

        /// <summary>Told when a body driven against the rim of The Closed Ring is stopped there (the body; the player's marks strike it).</summary>
        public static event System.Action<Component> Stopped;

        /// <summary>Tells the listeners that <paramref name="body"/> was driven against a ring's stones and stopped.</summary>
        public static void TellStopped(Component body) => Stopped?.Invoke(body);

        /// <summary>A step at least this fast, metres a second, is a body driven into the stones, not one walking up to them.</summary>
        public const float DrivenAtLeast = 3f;

        /// <summary>Forgets every ground (a new play, a test).</summary>
        public static void Clear()
        {
            for (int i = 0; i < Most; i++) patches[i] = default;
        }

        /// <summary>Lays a round ground <paramref name="radius"/> round <paramref name="at"/> until <paramref name="until"/>; an enemy shoved on it goes <paramref name="thrown"/> times as far.</summary>
        public static void Lay(string name, Vector3 at, float radius, float until, float thrown = 1f) =>
            LayLane(name, at, Vector3.zero, 0f, radius, until, thrown);

        /// <summary>Lays a lane <paramref name="length"/> long from <paramref name="from"/> along <paramref name="way"/>, <paramref name="half"/> either side of its line.</summary>
        public static void LayLane(string name, Vector3 from, Vector3 way, float length, float half, float until, float thrown = 1f)
        {
            if (string.IsNullOrEmpty(name)) return;
            int slot = -1;
            for (int i = 0; i < Most && slot < 0; i++)
                if (!patches[i].Active) slot = i;
            if (slot < 0)
            {
                slot = 0;
                for (int i = 1; i < Most; i++)
                    if (patches[i].Until < patches[slot].Until) slot = i;
            }
            way.y = 0f;
            patches[slot] = new Patch
            {
                Active = true, Name = name, At = from, Way = way.sqrMagnitude > 0.0001f ? way.normalized : Vector3.zero,
                Length = Mathf.Max(0f, length), Radius = Mathf.Max(0f, half), Until = until, Thrown = Mathf.Max(1f, thrown),
            };
        }

        private static bool Live(int i, float now) => patches[i].Active && now < patches[i].Until;

        private static bool On(int i, Vector3 at) =>
            GroundRules.OnLane(at, patches[i].At, patches[i].Way, patches[i].Length, patches[i].Radius);

        /// <summary>True when <paramref name="at"/> stands in a ground of this name at <paramref name="now"/>.</summary>
        public static bool Under(string name, Vector3 at, float now)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < Most; i++)
                if (Live(i, now) && patches[i].Name == name && On(i, at)) return true;
            return false;
        }

        /// <summary>How many times as far an enemy at <paramref name="at"/> is thrown by the slick ground under it (1 off it).</summary>
        public static float ThrownAt(Vector3 at, float now)
        {
            float most = 1f;
            for (int i = 0; i < Most; i++)
                if (Live(i, now) && patches[i].Thrown > most && On(i, at)) most = patches[i].Thrown;
            return most;
        }

        /// <summary>True when the step from <paramref name="a"/> to <paramref name="b"/> crosses a lane of this name (a blow across The Drawn Line).</summary>
        public static bool Across(string name, Vector3 a, Vector3 b, float now)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < Most; i++)
                if (Live(i, now) && patches[i].Name == name && patches[i].Length > 0f && GroundRules.Crosses(a, b, patches[i].At, patches[i].Way, patches[i].Length)) return true;
            return false;
        }

        /// <summary>
        /// True when a step from <paramref name="before"/> to <paramref name="now"/> would cross the rim of a round ground of this
        /// name, in or out (nothing walks or is thrown across The Closed Ring).
        /// </summary>
        public static bool AcrossRim(string name, Vector3 before, Vector3 now, float time)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < Most; i++)
                if (Live(i, time) && patches[i].Name == name && patches[i].Length <= 0f && GroundRules.CrossesRing(before, now, patches[i].At, patches[i].Radius)) return true;
            return false;
        }

        /// <summary>
        /// Grows the live round ground of this name under <paramref name="at"/> by <paramref name="wider"/> metres and
        /// <paramref name="longer"/> seconds (The Burning Lake: each enemy killed on it). False when none lies there.
        /// </summary>
        public static bool Grow(string name, Vector3 at, float wider, float longer, float now)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < Most; i++)
            {
                if (!Live(i, now) || patches[i].Name != name || patches[i].Length > 0f || !On(i, at)) continue;
                patches[i].Radius += Mathf.Max(0f, wider);
                patches[i].Until += Mathf.Max(0f, longer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// The reach and end of the live round ground of this name laid at <paramref name="centre"/> (a spell's field reads its own
        /// size so what grows it is seen); false when none lies there.
        /// </summary>
        public static bool Extent(string name, Vector3 centre, float now, out float radius, out float until)
        {
            radius = 0f;
            until = 0f;
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < Most; i++)
            {
                if (!Live(i, now) || patches[i].Name != name || patches[i].Length > 0f) continue;
                Vector3 apart = patches[i].At - centre;
                apart.y = 0f;
                if (apart.sqrMagnitude > 0.01f) continue;
                radius = patches[i].Radius;
                until = patches[i].Until;
                return true;
            }
            return false;
        }

        /// <summary>How many grounds of this name lie at <paramref name="now"/>.</summary>
        public static int Count(string name, float now)
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
                if (Live(i, now) && patches[i].Name == name) count++;
            return count;
        }

        /// <summary>Ends every ground of this name now (a ring that falls when its surge ends).</summary>
        public static void End(string name)
        {
            for (int i = 0; i < Most; i++)
                if (patches[i].Active && patches[i].Name == name) patches[i].Active = false;
        }
    }

    /// <summary>The plain rules of the player's grown things (the ledger's grown-bursts and grows-back), kept apart from the scene.</summary>
    public static class GrownRules
    {
        /// <summary>True when a grown thing at <paramref name="at"/> stands within <paramref name="reach"/> of <paramref name="from"/>; a reach of 0 reaches everything.</summary>
        public static bool Within(Vector3 at, Vector3 from, float reach)
        {
            if (reach <= 0f) return true;
            Vector3 offset = at - from;
            offset.y = 0f;
            return offset.sqrMagnitude <= reach * reach;
        }
    }

    /// <summary>
    /// Grown things waiting to grow back where they stood (Wood Grows Back), each with the time it comes up. Fixed in size:
    /// when it is full, the one due soonest is given up for the new one.
    /// </summary>
    public sealed class RegrowQueue
    {
        public const int Most = 16;
        private readonly HazardSpec[] specs = new HazardSpec[Most];
        private readonly float[] dueAt = new float[Most];
        private readonly bool[] waiting = new bool[Most];

        /// <summary>How many wait to grow back.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Most; i++)
                    if (waiting[i]) count++;
                return count;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Most; i++) waiting[i] = false;
        }

        /// <summary>Puts a grown thing in line to come up again at <paramref name="at"/>.</summary>
        public void Add(in HazardSpec spec, float at)
        {
            int slot = -1;
            for (int i = 0; i < Most && slot < 0; i++)
                if (!waiting[i]) slot = i;
            if (slot < 0)
            {
                slot = 0;
                for (int i = 1; i < Most; i++)
                    if (dueAt[i] < dueAt[slot]) slot = i;
            }
            specs[slot] = spec;
            dueAt[slot] = at;
            waiting[slot] = true;
        }

        /// <summary>Takes one whose time has come by <paramref name="now"/>; false when none has.</summary>
        public bool TakeDue(float now, out HazardSpec spec)
        {
            for (int i = 0; i < Most; i++)
            {
                if (!waiting[i] || dueAt[i] > now) continue;
                waiting[i] = false;
                spec = specs[i];
                return true;
            }
            spec = default;
            return false;
        }
    }
    /// <summary>
    /// Ground the player's light has made clean (the ledger's purify): no enemy cloud, flame or frost lies there or bites in it.
    /// Your own glow (Standing Light's, Row of Lamps') is clean while it lasts, and a mark may make a place clean for a while,
    /// or a ring round you that goes where you go (Circled Lamp, High Noon). Enemy ground asks <see cref="Covers"/>: a patch
    /// whose middle lies in clean ground goes out, and none bites whoever stands in it. Pure but for where a follower stands.
    /// </summary>
    public static class Purified
    {
        private struct Zone
        {
            public bool Active;
            public Vector3 At;
            public Transform Follows;
            public float Radius, Until;
        }

        public const int Most = 8;
        private static readonly Zone[] zones = new Zone[Most];

        // Domain reload is off: no clean ground from the last play may lie in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        /// <summary>Forgets every clean place (a new play, a test).</summary>
        public static void Clear()
        {
            for (int i = 0; i < Most; i++) zones[i] = default;
        }

        /// <summary>Makes ground <paramref name="radius"/> round <paramref name="at"/> clean until <paramref name="until"/>; round <paramref name="follows"/> wherever it goes, when given.</summary>
        public static void Add(Vector3 at, float radius, float until, Transform follows = null)
        {
            int slot = 0;
            for (int i = 0; i < Most; i++)
            {
                if (!zones[i].Active) { slot = i; break; }
                if (zones[i].Until < zones[slot].Until) slot = i;
            }
            zones[slot] = new Zone { Active = true, At = at, Follows = follows, Radius = Mathf.Max(0f, radius), Until = until };
        }

        /// <summary>True when <paramref name="at"/> is clean ground at <paramref name="now"/>: inside a clean place, or your own glow.</summary>
        public static bool Covers(Vector3 at, float now)
        {
            for (int i = 0; i < Most; i++)
            {
                if (!zones[i].Active || now >= zones[i].Until) continue;
                Vector3 centre = zones[i].Follows != null ? zones[i].Follows.position : zones[i].At;
                if (GroundRules.Within(at, centre, zones[i].Radius)) return true;
            }
            return OwnGrounds.Under(OwnGrounds.Glow, at, now) || Hazards.NamedUnder(at, OwnGrounds.Glow);
        }
    }
}
