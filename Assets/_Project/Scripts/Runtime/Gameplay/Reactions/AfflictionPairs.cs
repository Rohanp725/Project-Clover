namespace Clover.Gameplay
{
    /// <summary>
    /// The 55 pairs of the eleven afflictions, each numbered 0 to 54 in the order of the table "All 55 pairs" in
    /// docs/design/steel/README.md: the eleven pairs one apart on the ring first, then two apart, and so on to five. Each
    /// pair has its distance on the ring, the name of its plain reaction from the pair files, the id of that
    /// reaction's mark, and how long it rests by default on one enemy. Plain tables, built once.
    /// </summary>
    public static class AfflictionPairs
    {
        public const int Count = 55;
        public const int Farthest = 5;
        /// <summary>A reaction rests this long on one enemy unless its entry says otherwise (README.md, "How a reaction fires").</summary>
        public const float UsualRest = 4f;
        /// <summary>The pairs three apart all rest this long, because most of them hold an enemy or set up a heavy blow.</summary>
        public const float DistanceThreeRest = 6f;
        public const string IdPrefix = "react.";

        /// <summary>The ring of the elements, as their afflictions: Fire, Lightning, Wind, Ice, Water, Life, Earth, Metal, Light, Dark, Poison.</summary>
        public static readonly AfflictionKind[] Ring =
        {
            AfflictionKind.Burn, AfflictionKind.Static, AfflictionKind.Unsteady, AfflictionKind.Chill, AfflictionKind.Soaked,
            AfflictionKind.Seeded, AfflictionKind.Tremor, AfflictionKind.Sundered, AfflictionKind.Marked, AfflictionKind.Cursed,
            AfflictionKind.Poison,
        };

        // In pair order; the four pair files (pairs_d1_d2.md to pairs_d5.md) give each its name.
        private static readonly string[] names =
        {
            // Distance 1.
            "Flashover", "Thunder Gust", "Rime", "Supercooled", "Quickening", "Split Rock", "Anvil Stone", "Glint",
            "Diamond Ring", "Shut in the Dark", "Fire Medicine",
            // Distance 2.
            "Carried Embers", "Sleet Charge", "Capsize", "Winter Seed", "The Ground Gives", "Iron Thorns", "Earthlight",
            "Black Chain", "Burning Sap", "Smoulder", "Nerve Fire",
            // Distance 3.
            "Thermal Shock", "Seized", "Windthrow", "Frost Heave", "Rust", "Shining Herb", "Turned to Stone", "Quicksilver",
            "Kindling Point", "The Grudge", "Ill Wind",
            // Distance 4.
            "Steam", "Lightning Flower", "Logan Stone", "Cold Short", "Caught Light", "Girdled", "Chokedamp", "Red Hot",
            "The Bolt Runs True", "Breath Taken", "Venom Rime",
            // Distance 5.
            "Firebloom", "Earthed", "Sickle Wind", "Mock Suns", "Drowned Weight", "Rot Roots", "Burnt Mound", "Live Iron",
            "Golden Feathers", "Stolen Warmth", "Fish Poison",
        };

        private static readonly string[] ids = new string[Count];
        private static readonly AfflictionKind[] firsts = new AfflictionKind[Count];
        private static readonly AfflictionKind[] seconds = new AfflictionKind[Count];
        // Indexed by the two kinds' numbers, so finding a pair on every blow is one array read; -1 for no pair.
        private static readonly sbyte[] byKinds = new sbyte[Afflictions.Count * Afflictions.Count];

        static AfflictionPairs()
        {
            for (int i = 0; i < byKinds.Length; i++) byKinds[i] = -1;
            int ring = Ring.Length;
            for (int distance = 1; distance <= Farthest; distance++)
            {
                for (int start = 0; start < ring; start++)
                {
                    int pair = (distance - 1) * ring + start;
                    var first = Ring[start];
                    var second = Ring[(start + distance) % ring];
                    firsts[pair] = first;
                    seconds[pair] = second;
                    byKinds[(int)first * Afflictions.Count + (int)second] = (sbyte)pair;
                    byKinds[(int)second * Afflictions.Count + (int)first] = (sbyte)pair;
                    ids[pair] = IdPrefix + IdWord(names[pair]);
                }
            }
        }

        /// <summary>The pair two afflictions make, in either order; -1 when they make none (the same kind twice, or None).</summary>
        public static int Index(AfflictionKind one, AfflictionKind other)
        {
            int a = (int)one;
            int b = (int)other;
            if ((uint)a >= Afflictions.Count || (uint)b >= Afflictions.Count) return -1;
            return byKinds[a * Afflictions.Count + b];
        }

        /// <summary>The pair's first affliction, in the order the README's table names it.</summary>
        public static AfflictionKind First(int pair) => firsts[pair];
        public static AfflictionKind Second(int pair) => seconds[pair];

        /// <summary>How far apart the two are on the ring: 1 to 5.</summary>
        public static int Distance(int pair) => pair / Ring.Length + 1;

        /// <summary>The plain reaction's name, as the pair files give it.</summary>
        public static string Name(int pair) => names[pair];

        /// <summary>The id of the plain reaction's mark: "react." and its name in lower-case letters with nothing between.</summary>
        public static string Id(int pair) => ids[pair];

        /// <summary>How long the pair rests on one enemy once it has reacted, until its entry sets its own.</summary>
        public static float Rest(int pair) => Distance(pair) == 3 ? DistanceThreeRest : UsualRest;

        /// <summary>The other affliction of the pair, or None when the kind given is not in it.</summary>
        public static AfflictionKind Partner(int pair, AfflictionKind kind) =>
            firsts[pair] == kind ? seconds[pair] : seconds[pair] == kind ? firsts[pair] : AfflictionKind.None;

        private static string IdWord(string name)
        {
            var letters = new System.Text.StringBuilder(name.Length);
            foreach (char letter in name)
                if (char.IsLetterOrDigit(letter)) letters.Append(char.ToLowerInvariant(letter));
            return letters.ToString();
        }
    }
}
