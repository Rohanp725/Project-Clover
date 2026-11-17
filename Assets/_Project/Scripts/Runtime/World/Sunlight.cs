using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The one question the Fire land asks of its light: is this point in sun or in shade. A scene says so in
    /// rectangles of floor. Under an open sky everything is lit except the rectangles of shade (a yard, a street);
    /// under a roof nothing is lit except the rectangles of light (the hall of columns). A flare lights a circle for
    /// a few seconds wherever it goes off. The answer is a walk over a few dozen numbers: no rays are cast.
    /// </summary>
    public static class Sunlight
    {
        public const int MostRectangles = 160;
        public const int MostFlares = 8;

        private static readonly Vector4[] rectangles = new Vector4[MostRectangles];
        private static readonly bool[] shade = new bool[MostRectangles];
        private static int count;
        private static readonly Vector3[] flareAt = new Vector3[MostFlares];
        private static readonly float[] flareRadius = new float[MostFlares];
        private static readonly float[] flareLeft = new float[MostFlares];
        public const int MostBeams = 8;
        private static readonly Vector3[] beamFrom = new Vector3[MostBeams];
        private static readonly Vector3[] beamTo = new Vector3[MostBeams];
        private static readonly float[] beamHalfWidth = new float[MostBeams];
        private static readonly float[] beamLeft = new float[MostBeams];

        /// <summary>True where the sky is open: every point is lit unless a rectangle of shade covers it.</summary>
        public static bool OpenSky { get; private set; }
        /// <summary>True once a scene has said anything about its light; the sun-blind use their rule only then.</summary>
        public static bool InUse => OpenSky || count > 0 || Hand != null;
        public static int Rectangles => count;

        // Domain reload is off: the last play's light must not be this one's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            count = 0;
            OpenSky = false;
            Hand = null;
            for (int i = 0; i < MostFlares; i++) flareLeft[i] = 0f;
            for (int i = 0; i < MostBeams; i++) beamLeft[i] = 0f;
        }

        public static void SetOpenSky(bool open) => OpenSky = open;

        /// <summary>A shadow that turns (the sun-court's obelisk): shade wherever it lies, as a rectangle of shade is. Null for none.</summary>
        public static Clover.Gameplay.ShadowHand Hand { get; set; }

        /// <summary>True for a point the turning shadow covers.</summary>
        public static bool InHand(Vector3 position) => Hand != null && Hand.Covers(position);

        /// <summary>Where a shaft of sun aimed at this point is laid: the point itself, or the shadow's nearest edge if the shadow hides it.</summary>
        public static Vector3 OutOfHand(Vector3 position, float clear = 0.2f) => Hand != null ? Hand.NearestOutside(position, clear) : position;

        /// <summary>A rectangle of floor in the sun, by its least and greatest x and z.</summary>
        public static void AddLight(float leastX, float mostX, float leastZ, float mostZ) => Add(leastX, mostX, leastZ, mostZ, false);

        /// <summary>A rectangle of floor in shade, by its least and greatest x and z. Shade wins over light.</summary>
        public static void AddShade(float leastX, float mostX, float leastZ, float mostZ) => Add(leastX, mostX, leastZ, mostZ, true);

        private static void Add(float leastX, float mostX, float leastZ, float mostZ, bool isShade)
        {
            if (count >= MostRectangles) return;
            rectangles[count] = new Vector4(Mathf.Min(leastX, mostX), Mathf.Max(leastX, mostX), Mathf.Min(leastZ, mostZ), Mathf.Max(leastZ, mostZ));
            shade[count++] = isShade;
        }

        /// <summary>Lights a circle of ground for a while, as a priest's flare does.</summary>
        public static void Flare(Vector3 centre, float radius, float seconds)
        {
            int free = 0;
            for (int i = 0; i < MostFlares; i++)
                if (flareLeft[i] < flareLeft[free]) free = i;
            flareAt[free] = centre;
            flareRadius[free] = radius;
            flareLeft[free] = seconds;
        }

        /// <summary>
        /// Lights a lane of ground for a while, as a beam of sun thrown from a mirror does: from one point to another,
        /// this wide. Zero seconds or less puts nothing down.
        /// </summary>
        public static void Beam(Vector3 from, Vector3 to, float width, float seconds)
        {
            if (seconds <= 0f) return;
            int free = 0;
            for (int i = 0; i < MostBeams; i++)
                if (beamLeft[i] < beamLeft[free]) free = i;
            beamFrom[free] = from;
            beamTo[free] = to;
            beamHalfWidth[free] = width * 0.5f;
            beamLeft[free] = seconds;
        }

        /// <summary>True for a point a beam of reflected sun is falling on now.</summary>
        public static bool InBeam(Vector3 position)
        {
            for (int i = 0; i < MostBeams; i++)
            {
                if (beamLeft[i] <= 0f) continue;
                float x = beamTo[i].x - beamFrom[i].x, z = beamTo[i].z - beamFrom[i].z;
                float length = x * x + z * z;
                float px = position.x - beamFrom[i].x, pz = position.z - beamFrom[i].z;
                float along = length > 0.0001f ? Mathf.Clamp01((px * x + pz * z) / length) : 0f;
                float dx = px - x * along, dz = pz - z * along;
                if (dx * dx + dz * dz <= beamHalfWidth[i] * beamHalfWidth[i]) return true;
            }
            return false;
        }

        /// <summary>Lets the flares burn down. Called once a frame by the scene's <see cref="SunlightZones"/>.</summary>
        public static void Tick(float deltaTime)
        {
            for (int i = 0; i < MostFlares; i++)
                if (flareLeft[i] > 0f) flareLeft[i] -= deltaTime;
            for (int i = 0; i < MostBeams; i++)
                if (beamLeft[i] > 0f) beamLeft[i] -= deltaTime;
        }

        /// <summary>Whether a point on the ground is in the sun.</summary>
        public static bool IsLit(Vector3 position)
        {
            for (int i = 0; i < MostFlares; i++)
            {
                if (flareLeft[i] <= 0f) continue;
                float x = position.x - flareAt[i].x, z = position.z - flareAt[i].z;
                if (x * x + z * z <= flareRadius[i] * flareRadius[i]) return true;
            }
            if (InBeam(position)) return true;
            if (Hand != null && Hand.Covers(position)) return false;
            bool lit = OpenSky;
            for (int i = 0; i < count; i++)
            {
                var r = rectangles[i];
                if (position.x < r.x || position.x > r.y || position.z < r.z || position.z > r.w) continue;
                if (shade[i]) return false;
                lit = true;
            }
            return lit;
        }
    }
}
