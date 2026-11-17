using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The plain rules of Lodestone, Lightning's pull: every enemy near is dragged toward the caster, further for each
    /// stack of Static on it, and then everything close is discharged at once.
    /// </summary>
    public static class LodestoneRules
    {
        /// <summary>Metres an enemy with no Static is dragged, and metres more for each stack on it.</summary>
        public const float Pull = 1f, PullAStack = 0.8f;
        /// <summary>Nothing is dragged nearer the caster than this: sword reach.</summary>
        public const float StopsAt = 1.3f;
        /// <summary>Metres round the caster within which the discharge reaches.</summary>
        public const float BurstRadius = 3.5f;

        /// <summary>Metres an enemy this far away, carrying this many stacks of Static, is dragged toward the caster.</summary>
        public static float Dragged(float gap, int staticStacks) =>
            Mathf.Clamp(Pull + PullAStack * Mathf.Max(0, staticStacks), 0f, Mathf.Max(0f, gap - StopsAt));
    }
}
