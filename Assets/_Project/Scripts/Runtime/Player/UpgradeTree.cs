using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The shape of the great chart: where each mark of each tree lies round the one middle, which marks are open to
    /// be learned, and which one a push of the stick moves the choice to. Plain rules with no scene in them, so they
    /// can be tested. Distances are in steps; the menu decides how many pixels a step is.
    /// </summary>
    public static class UpgradeTree
    {
        /// <summary>How far from the middle of the chart an element's own emblem sits. A tree's marks are placed from there: X across, Y outward.</summary>
        public const float EmblemDistance = 9f;
        /// <summary>How far from the middle the foot of a mixed branch sits. A mix's marks are placed from there.</summary>
        public const float MixDistance = 10.5f;
        /// <summary>The furthest out a tree's mark may sit (the god's gift at its crown), and a mix's.</summary>
        public const float TreeHeight = 12f;
        public const float MixHeight = 9f;
        /// <summary>A mark on a mixed branch sits no further than this to either side of the branch's middle.</summary>
        public const float MixHalfWidth = 0.6f;
        /// <summary>No two marks anywhere on the chart sit nearer than this.</summary>
        public const float LeastSpacing = 0.85f;
        /// <summary>A move is only made toward something within this many degrees of where the stick points.</summary>
        public const float StepHalfAngle = 62f;

        /// <summary>
        /// How far to either side of its tree's middle a mark at this height may sit. A tree is a wedge: it may spread
        /// as it grows outward, and must leave the gap beside it to the branch it shares with its neighbour.
        /// </summary>
        public static float HalfWidth(float y) => 0.28f * (EmblemDistance + y) - 1.45f;

        /// <summary>The way an element's tree grows from the middle of the chart, in degrees clockwise from straight up.</summary>
        public static float Bearing(string element) => Elements.Bearing(element);

        /// <summary>The way the tree or branch a mark is on grows.</summary>
        public static float Bearing(Upgrade upgrade) =>
            upgrade.IsMix ? Elements.MixBearing(upgrade.Element, upgrade.Element2) : Elements.Bearing(upgrade.Element);

        /// <summary>
        /// True when what the upgrade grows from has been learned, or it grows from the foot of its tree. Whether
        /// the tree itself is open to the player (the element held, a mix unlocked) is asked elsewhere. An element's base
        /// spell (<c>fire.base</c>) counts as learned: it comes with its element, so its branch's Empower is open at once.
        /// </summary>
        public static bool Reachable(Upgrade upgrade, ICollection<string> learned)
        {
            if (upgrade == null) return false;
            if (upgrade.Requires == null || upgrade.Requires.Length == 0) return true;
            foreach (string id in upgrade.Requires)
                if (learned.Contains(id) || Magic.SpellSlots.IsBase(id)) return true;
            return false;
        }

        /// <summary>
        /// True when a mixed branch is open: the player holds one of its two elements and has held or consumed the
        /// other in this run.
        /// </summary>
        public static bool MixOpen(Upgrade upgrade, string held, ICollection<string> everHeld, ICollection<string> consumed)
        {
            if (upgrade == null || !upgrade.IsMix || !upgrade.BelongsTo(held)) return false;
            string other = upgrade.Element == held ? upgrade.Element2 : upgrade.Element;
            return everHeld.Contains(other) || consumed.Contains(other);
        }

        /// <summary>Where a place lies on the chart, given the bearing of its tree and how far out that tree's foot is.</summary>
        public static Vector2 OnChart(float bearing, float footDistance, float x, float y)
        {
            float turn = -bearing * Mathf.Deg2Rad;
            float along = y + footDistance;
            float cos = Mathf.Cos(turn), sin = Mathf.Sin(turn);
            return new Vector2(x * cos - along * sin, x * sin + along * cos);
        }

        public static Vector2 OnChart(string element, float x, float y) => OnChart(Elements.Bearing(element), EmblemDistance, x, y);

        public static Vector2 OnChart(Upgrade upgrade) =>
            OnChart(Bearing(upgrade), upgrade.IsMix ? MixDistance : EmblemDistance, upgrade.X, upgrade.Y);

        /// <summary>Where the element's emblem, the foot of its tree, lies on the chart.</summary>
        public static Vector2 Emblem(string element) => OnChart(element, 0f, 0f);

        /// <summary>Where the foot of the branch two neighbours share lies on the chart.</summary>
        public static Vector2 MixFoot(string one, string other) => OnChart(Elements.MixBearing(one, other), MixDistance, 0f, 0f);

        /// <summary>Turns a place on the chart so that the tree with this bearing stands upright.</summary>
        public static Vector2 Upright(Vector2 place, float bearing)
        {
            float turn = bearing * Mathf.Deg2Rad;
            float cos = Mathf.Cos(turn), sin = Mathf.Sin(turn);
            return new Vector2(place.x * cos - place.y * sin, place.x * sin + place.y * cos);
        }

        /// <summary>
        /// The place the choice moves to from <paramref name="from"/> when the stick is pushed toward
        /// <paramref name="direction"/>: the nearest place roughly that way, preferring one straight along the push
        /// to one off to its side. Returns <paramref name="from"/> when nothing lies that way.
        /// </summary>
        public static int Step(IReadOnlyList<Vector2> places, int from, Vector2 direction)
        {
            if (from < 0 || from >= places.Count || direction.sqrMagnitude < 0.0001f) return from;
            direction.Normalize();
            float narrowest = Mathf.Cos(StepHalfAngle * Mathf.Deg2Rad);
            int best = from;
            float bestScore = float.MaxValue;
            for (int i = 0; i < places.Count; i++)
            {
                if (i == from) continue;
                Vector2 offset = places[i] - places[from];
                float distance = offset.magnitude;
                if (distance < 0.0001f) continue;
                float along = Vector2.Dot(offset / distance, direction);
                if (along < narrowest) continue;
                // Twice as far straight ahead is as good as near but well off to the side.
                float score = distance * (1f + 2.5f * (1f - along));
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            return best;
        }
    }
}
