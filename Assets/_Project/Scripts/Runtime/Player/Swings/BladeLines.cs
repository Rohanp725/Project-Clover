using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// Every weapon's blade lines as the model notes give them (staging/blender notes, the stand-ins in the weapon
    /// builder): what a swing effect follows when a prefab was built before <see cref="BladeLine"/> existed, and what the
    /// builder's measurement is checked against. The builder reads the real lines off each model's mesh at build time
    /// and writes them onto the prefab; these are the fallback, not the source. Bare hands and the Armament sweep with
    /// the body (<see cref="Limbs"/>). The War Fan has no model or moves yet: its line is kept here for when it comes.
    /// </summary>
    public static class BladeLines
    {
        /// <summary>The carry name of the body's own lines (bare hands, the Armament).</summary>
        public const string BodyCarry = "body";
        /// <summary>The War Fan's carry name, planned (GAME.md, the Hermit of the Pass): no model, no moves yet.</summary>
        public const string FanCarry = "fan";

        private static readonly Vector3 EdgeDown = new Vector3(0f, -1f, 0f);

        public static readonly BladeLineSpec[] Defaults =
        {
            // The Katana: the edge from the habaki to the kissaki, 0.86 m, toward -Y.
            new BladeLineSpec(Weapon.KatanaCarry, "edge", BladeKind.Edge, EdgeDown, 0.02f, new Vector3(0f, -0.022f, 0.13f), new Vector3(0f, -0.012f, 0.99f)),
            // The Massive Sword: the long single edge from above the hooks to the clipped point.
            new BladeLineSpec("gsword", "edge", BladeKind.Edge, EdgeDown, 0.19f, new Vector3(0f, -0.17f, 0.14f), new Vector3(0f, -0.07f, 1.5f)),
            // The Scythe: the crescent's inside edge, leaving the top of the haft toward -Y and curving back to its point.
            new BladeLineSpec("scythe", "edge", BladeKind.Edge, new Vector3(0f, 0.53f, -0.85f), 0.12f, new Vector3(0f, -0.02f, 0.78f), new Vector3(0f, -0.55f, 0.66f), new Vector3(0f, -0.92f, -0.02f)),
            // The Great Spear: the leaf, from the crossbar to the point; its edges lie to either side (X).
            new BladeLineSpec("spear", "leaf", BladeKind.Point, Vector3.right, 0.1f, new Vector3(0f, 0f, 1.45f), new Vector3(0f, 0f, 2f)),
            new BladeLineSpec("sunspear", "leaf", BladeKind.Point, Vector3.right, 0.09f, new Vector3(0f, 0f, 1.5f), new Vector3(0f, 0f, 2f)),
            // The Great Hammer: through the head along the haft, the striking face toward -Y.
            new BladeLineSpec("hammer", "head", BladeKind.Head, EdgeDown, 0.21f, new Vector3(0f, -0.08f, 0.88f), new Vector3(0f, -0.08f, 1.27f)),
            // One of the Twin Knives (each hand carries one): the edge from the guard to the point.
            new BladeLineSpec("knife", "edge", BladeKind.Edge, EdgeDown, 0.04f, new Vector3(0f, -0.05f, 0.08f), new Vector3(0f, -0.008f, 0.37f)),
            // The Great Bow: each limb from the grip to its nock, the back (toward the target, -Y) the striking face.
            new BladeLineSpec("bow", "upper limb", BladeKind.Stave, EdgeDown, 0.03f, new Vector3(0f, 0f, 0.07f), new Vector3(0f, 0.13f, 0.76f), new Vector3(0f, 0.312f, 1.178f)),
            new BladeLineSpec("bow", "lower limb", BladeKind.Stave, EdgeDown, 0.03f, new Vector3(0f, 0f, -0.07f), new Vector3(0f, 0.19f, -0.5f), new Vector3(0f, 0.312f, -0.628f)),
            // The arrow, in the fist for the stab and in flight for the shots: its front half to the point.
            new BladeLineSpec("arrow", "head", BladeKind.Point, Vector3.right, 0.04f, new Vector3(0f, 0f, 0.55f), new Vector3(0f, 0f, 1.1f)),
            // The Chain Sickle's blade: the concave edge from the collar round to the point.
            new BladeLineSpec("sickle", "edge", BladeKind.Edge, new Vector3(0f, 0.54f, -0.84f), 0.06f, new Vector3(0f, -0.02f, 0.2f), new Vector3(0f, -0.25f, 0.15f), new Vector3(0f, -0.34f, -0.16f)),
            // The weight on the end of its chain: its body, from the eye to the end.
            new BladeLineSpec("weight", "weight", BladeKind.Head, Vector3.forward, 0.075f, Vector3.zero, new Vector3(0f, 0f, 0.22f)),
            // The War Fan, opened: from the pivot out to the rim. Planned; no model yet.
            new BladeLineSpec(FanCarry, "rim", BladeKind.Rim, EdgeDown, 0.02f, new Vector3(0f, 0f, 0.08f), new Vector3(0f, -0.2f, 0.36f), new Vector3(0f, -0.1f, 0.47f)),
        };

        /// <summary>The body's lines: each forearm to its knuckles, each shin to its toes.</summary>
        public static readonly LimbLine[] Limbs =
        {
            new LimbLine("fist R", "forearm.R", "hand.R", 0.09f, 0.06f),
            new LimbLine("fist L", "forearm.L", "hand.L", 0.09f, 0.06f),
            new LimbLine("foot R", "shin.R", "foot.R", 0.12f, 0.06f),
            new LimbLine("foot L", "shin.L", "foot.L", 0.12f, 0.06f),
        };

        /// <summary>True when a weapon is swung with the body itself: bare hands and the Armament.</summary>
        public static bool UsesLimbs(Weapon weapon) => weapon != null && (string.IsNullOrEmpty(weapon.Carry) || weapon.Carry == "arm");

        /// <summary>True when a line of this carry sweeps with this weapon: its own, the bow's arrow, the sickle's weight.</summary>
        public static bool Belongs(string carry, Weapon weapon)
        {
            if (weapon == null || string.IsNullOrEmpty(carry)) return false;
            if (carry == BodyCarry) return UsesLimbs(weapon);
            if (weapon.UsesSword) return carry == Weapon.KatanaCarry;
            if (carry == weapon.Carry) return true;
            return (weapon.Carry == "bow" && carry == "arrow") || (weapon.Carry == "sickle" && carry == "weight");
        }

        /// <summary>How many default lines a carry has (the bow two).</summary>
        public static int CountOf(string carry)
        {
            int count = 0;
            for (int i = 0; i < Defaults.Length; i++)
                if (Defaults[i].Carry == carry) count++;
            return count;
        }

        /// <summary>The <paramref name="nth"/> default line of a carry; false when it has no such line.</summary>
        public static bool TryGet(string carry, int nth, out BladeLineSpec line)
        {
            for (int i = 0; i < Defaults.Length; i++)
            {
                if (Defaults[i].Carry != carry) continue;
                if (nth-- == 0)
                {
                    line = Defaults[i];
                    return true;
                }
            }
            line = default;
            return false;
        }

        /// <summary>How many lines sweep with a weapon: its own and what goes with it, or the body's four.</summary>
        public static int CountFor(Weapon weapon)
        {
            if (weapon == null) return 0;
            if (UsesLimbs(weapon)) return Limbs.Length;
            int count = 0;
            for (int i = 0; i < Defaults.Length; i++)
                if (Belongs(Defaults[i].Carry, weapon)) count++;
            return count;
        }
    }
}
