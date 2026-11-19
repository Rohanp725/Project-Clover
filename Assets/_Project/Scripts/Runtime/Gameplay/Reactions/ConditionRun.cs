using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>One condition running on one enemy: one of the six slots of <see cref="EnemyConditions"/>.</summary>
    public struct ConditionRun
    {
        public bool Active;
        public string Id;
        public float Left;
        public float Beat;
        public float BeatClock;
        /// <summary>Beats come so far.</summary>
        public int Count;
        /// <summary>The place in the catalogue of the mark that set it, or -1.</summary>
        public int Mark;
        public ConditionFlags Flags;
        public float Pace;
        public float Scale;
        public float Sight;
        public Component Link;
        /// <summary>True when the mark that set it has something to do when it is struck, or when it ends.</summary>
        public bool AnswersStruck;
        public bool AnswersEnd;
    }
}
