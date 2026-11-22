using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>The plain rules of Uraeus' cobra (fire.uraeus; the ledger's enemy-begins-attack for its spit).</summary>
    public static class CobraRules
    {
        /// <summary>One spit at an enemy for each of its wind-ups: the next only this long after.</summary>
        public const float SpitAgain = 1f;
        /// <summary>The venom burns at the enemy's feet this long.</summary>
        public const float VenomSeconds = 2f;
        /// <summary>Where it sits on the shoulder: up and to the left of the player's middle.</summary>
        public const float ShoulderUp = 1.45f, ShoulderLeft = 0.28f;

        /// <summary>
        /// True when the cobra spits at an enemy now: it has spits left, the enemy is within reach and begins an attack (or, set
        /// down, simply stands in its circle on its beat), and it was not spat at in the last second.
        /// </summary>
        public static bool Spits(int spitsLeft, float distance, float reach, bool windingUp, bool planted, float sinceSpatAt) =>
            spitsLeft > 0 && distance <= reach && (windingUp || planted) && sinceSpatAt >= SpitAgain;
    }

    /// <summary>The plain rules of Riptide's River Serpent (water.base.shape).</summary>
    public static class SerpentRules
    {
        /// <summary>It drops what it took this far in front of the player.</summary>
        public const float DropAhead = 1.5f;

        /// <summary>Where the serpent sets down what it brought: in front of the player along the way they face.</summary>
        public static Vector3 DropAt(Vector3 player, Vector3 forward)
        {
            forward.y = 0f;
            return player + (forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward) * DropAhead;
        }
    }

    /// <summary>The plain rules of Stone Oak's totem (trees_b.md, overgrowth.stoneoak).</summary>
    public static class OakRules
    {
        /// <summary>Six of your weapon blows landed within eight metres of it grow it an age; it has three.</summary>
        public const int BlowsAnAge = 6, Ages = 3;
        public const float GrowReach = 8f;
        /// <summary>Young, its roots strike the nearest within 5 m; old, all within 6 m, and its shade heals 3 a second within 4 m.</summary>
        public const float YoungReach = 5f, OldReach = 6f, ShadeReach = 4f, ShadeHeal = 3f;

        /// <summary>Its age after <paramref name="blows"/> blows near it: 1 the sapling, 2 the young tree, 3 the old oak.</summary>
        public static int AgeFor(int blows) => Mathf.Clamp(1 + Mathf.Max(0, blows) / BlowsAnAge, 1, Ages);

        /// <summary>How far its roots reach at an age (0: the sapling strikes nothing).</summary>
        public static float StrikeReach(int age) => age >= 3 ? OldReach : age == 2 ? YoungReach : 0f;

        /// <summary>True when its roots strike everything in reach and Seed it (the old oak), not only the nearest.</summary>
        public static bool StrikesAll(int age) => age >= 3;

        /// <summary>The health a second its shade gives the player at an age, standing within <see cref="ShadeReach"/>.</summary>
        public static float HealsInShade(int age) => age >= 3 ? ShadeHeal : 0f;
    }
}
