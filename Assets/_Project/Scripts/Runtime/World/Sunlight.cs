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
        public const int MostRectangles = 48;
        public const int MostFlares = 8;

        private static readonly Vector4[] rectangles = new Vector4[MostRectangles];
        private static readonly bool[] shade = new bool[MostRectangles];
        private static int count;
        private static readonly Vector3[] flareAt = new Vector3[MostFlares];
        private static readonly float[] flareRadius = new float[MostFlares];
        private static readonly float[] flareLeft = new float[MostFlares];

        /// <summary>True where the sky is open: every point is lit unless a rectangle of shade covers it.</summary>
        public static bool OpenSky { get; private set; }
        /// <summary>True once a scene has said anything about its light; the sun-blind use their rule only then.</summary>
        public static bool InUse => OpenSky || count > 0;
        public static int Rectangles => count;

        // Domain reload is off: the last play's light must not be this one's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            count = 0;
            OpenSky = false;
            for (int i = 0; i < MostFlares; i++) flareLeft[i] = 0f;
        }

        public static void SetOpenSky(bool open) => OpenSky = open;

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

        /// <summary>Lets the flares burn down. Called once a frame by the scene's <see cref="SunlightZones"/>.</summary>
        public static void Tick(float deltaTime)
        {
            for (int i = 0; i < MostFlares; i++)
                if (flareLeft[i] > 0f) flareLeft[i] -= deltaTime;
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
