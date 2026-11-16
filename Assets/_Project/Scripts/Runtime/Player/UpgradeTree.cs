using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The shape of the element trees: which upgrade grows from which, where each one sits on the great chart that
    /// holds every element's tree around one middle, and which one a push of the stick moves the choice to. Plain
    /// rules with no scene in them, so they can be tested.
    /// </summary>
    public static class UpgradeTree
    {
        /// <summary>How far from the middle of the chart an element's own emblem sits, in steps.</summary>
        public const float EmblemDistance = 1.6f;
        /// <summary>A move is only made toward something within this many degrees of where the stick points.</summary>
        public const float StepHalfAngle = 62f;

        /// <summary>
        /// The way an element's tree grows from the middle of the chart, in degrees clockwise from straight up. Each
        /// element has its own, so the trees fan out from one point and neighbours can one day be joined.
        /// </summary>
        public static float Bearing(string element)
        {
            switch (element)
            {
                case ElementKit.FireName: return 0f;
                case ElementKit.LightningName: return 65f;
                default: return 0f;
            }
        }

        /// <summary>Every element that has a tree, in the order they stand round the chart.</summary>
        public static readonly IReadOnlyList<string> Elements = new[] { ElementKit.FireName, ElementKit.LightningName };

        /// <summary>True when what the upgrade grows from has been learned, or it grows from the root.</summary>
        public static bool Reachable(Upgrade upgrade, ICollection<string> learned) =>
            upgrade != null && (string.IsNullOrEmpty(upgrade.Requires) || learned.Contains(upgrade.Requires));

        /// <summary>Where a place in an element's own tree lies on the chart, in steps from its middle.</summary>
        public static Vector2 OnChart(string element, float x, float y)
        {
            float turn = -Bearing(element) * Mathf.Deg2Rad;
            float along = y + EmblemDistance;
            float cos = Mathf.Cos(turn), sin = Mathf.Sin(turn);
            return new Vector2(x * cos - along * sin, x * sin + along * cos);
        }

        public static Vector2 OnChart(Upgrade upgrade) => OnChart(upgrade.Element, upgrade.X, upgrade.Y);

        /// <summary>Where the element's emblem, the foot of its tree, lies on the chart.</summary>
        public static Vector2 Emblem(string element) => OnChart(element, 0f, 0f);

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
