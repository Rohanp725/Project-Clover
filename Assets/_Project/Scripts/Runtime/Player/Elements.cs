using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The eleven elements in the order they stand round the ring, clockwise from straight up. This is the one place
    /// that knows an element's colour, its bearing on the chart, its affliction and the mix it shares with the next
    /// element round. Everything else asks here.
    /// </summary>
    public static class Elements
    {
        public const string Fire = "Fire";
        public const string Lightning = "Lightning";
        public const string Wind = "Wind";
        public const string Ice = "Ice";
        public const string Water = "Water";
        public const string Life = "Life";
        public const string Earth = "Earth";
        public const string Metal = "Metal";
        public const string Light = "Light";
        public const string Dark = "Dark";
        public const string Poison = "Poison";

        public const int Count = 11;
        /// <summary>Degrees between one element and the next round the ring.</summary>
        public const float Step = 360f / Count;

        private static readonly ElementInfo[] all =
        {
            Make(0, Fire, new Color(1f, 0.56f, 0.2f), AfflictionKind.Burn, Lightning, "Plasma", UpgradeIcons.Flame, new Color(2.6f, 0.9f, 0.15f, 0.95f)),
            Make(1, Lightning, new Color(0.45f, 0.78f, 1f), AfflictionKind.Static, Wind, "Tempest", UpgradeIcons.Bolt, new Color(0.9f, 1.8f, 3.2f, 0.95f)),
            Make(2, Wind, new Color(0.62f, 0.95f, 0.8f), AfflictionKind.Unsteady, Ice, "Blizzard", UpgradeIcons.Gust),
            Make(3, Ice, new Color(0.7f, 0.92f, 1f), AfflictionKind.Chill, Water, "Glacier", UpgradeIcons.Snowflake),
            Make(4, Water, new Color(0.25f, 0.5f, 1f), AfflictionKind.Soaked, Life, "Bloom", UpgradeIcons.Drop),
            Make(5, Life, new Color(0.45f, 0.9f, 0.35f), AfflictionKind.Seeded, Earth, "Overgrowth", UpgradeIcons.Leaf),
            Make(6, Earth, new Color(0.72f, 0.52f, 0.3f), AfflictionKind.Tremor, Metal, "Forge", UpgradeIcons.Rock),
            Make(7, Metal, new Color(0.78f, 0.8f, 0.88f), AfflictionKind.Sundered, Light, "Halo", UpgradeIcons.Anvil),
            Make(8, Light, new Color(1f, 0.93f, 0.6f), AfflictionKind.Marked, Dark, "Eclipse", UpgradeIcons.Sun),
            Make(9, Dark, new Color(0.62f, 0.3f, 0.85f), AfflictionKind.Cursed, Poison, "Blight", UpgradeIcons.Moon),
            Make(10, Poison, new Color(0.6f, 0.85f, 0.15f), AfflictionKind.Poison, Fire, "Pyre", UpgradeIcons.Fang),
        };

        /// <summary>
        /// What the player holds before any element is taken: no colour of its own, nothing laid on enemies, no place
        /// on the ring. It is not one of <see cref="All"/>.
        /// </summary>
        public static readonly ElementInfo None = new ElementInfo
        {
            Index = 0,
            Name = "None",
            Colour = new Color(0.62f, 0.6f, 0.66f),
            Glow = new Color(1.3f, 1.25f, 1.4f, 0.9f),
            Affliction = AfflictionKind.None,
        };

        /// <summary>
        /// Whether a scene begins with the player holding no element: only where the scene asks for it (the Fire
        /// land) and only until the run carries one. Once the god's element is taken it is kept.
        /// </summary>
        public static bool StartsEmpty(bool sceneStartsWithNone, string runElement) => sceneStartsWithNone && string.IsNullOrEmpty(runElement);

        private static readonly Dictionary<string, ElementInfo> byName = Index();

        public static IReadOnlyList<ElementInfo> All => all;

        private static ElementInfo Make(int index, string name, Color colour, AfflictionKind affliction, string mixWith, string mixName, string icon, Color? glow = null) =>
            new ElementInfo
            {
                Index = index,
                Name = name,
                Colour = colour,
                Glow = glow ?? new Color(colour.r * 2.8f, colour.g * 2.8f, colour.b * 2.8f, 0.95f),
                Bearing = index * Step,
                Affliction = affliction,
                MixWith = mixWith,
                MixName = mixName,
                Icon = icon,
            };

        private static Dictionary<string, ElementInfo> Index()
        {
            var made = new Dictionary<string, ElementInfo>();
            foreach (var element in all) made[element.Name] = element;
            return made;
        }

        /// <summary>The element of that name, or null when there is none.</summary>
        public static ElementInfo ByName(string name) => name != null && byName.TryGetValue(name, out var found) ? found : null;

        /// <summary>The element of that name; Fire when the name is not an element's.</summary>
        public static ElementInfo ByNameOrFire(string name) => ByName(name) ?? all[0];

        public static Color Colour(string name) => ByNameOrFire(name).Colour;

        public static float Bearing(string name) => ByNameOrFire(name).Bearing;

        /// <summary>True when the two stand next to each other on the ring, and so share a mixed branch.</summary>
        public static bool AreNeighbours(string one, string other)
        {
            var a = ByName(one);
            var b = ByName(other);
            return a != null && b != null && (a.MixWith == b.Name || b.MixWith == a.Name);
        }

        /// <summary>The name of the mix two neighbours share ("Plasma" for Fire and Lightning, in either order); empty when they are not neighbours.</summary>
        public static string MixName(string one, string other)
        {
            var a = ByName(one);
            var b = ByName(other);
            if (a == null || b == null) return "";
            return a.MixWith == b.Name ? a.MixName : b.MixWith == a.Name ? b.MixName : "";
        }

        /// <summary>The bearing of the branch two neighbours share: half-way between their own.</summary>
        public static float MixBearing(string one, string other)
        {
            var a = ByNameOrFire(one);
            var b = ByNameOrFire(other);
            // The one whose mix it is stands first; the branch lies half a step clockwise of it.
            var first = a.MixWith == b.Name ? a : b;
            return first.Bearing + Step * 0.5f;
        }
    }
}
