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
        /// <summary>Weapon blows taken so far.</summary>
        public int Blows;
        /// <summary>The place in the catalogue of the mark that set it, or -1.</summary>
        public int Mark;
        public ConditionFlags Flags;
        public float Pace;
        public float Scale;
        public float Sight;
        public Component Link;
        /// <summary>True when the mark that set it has something to do when it is struck, when it ends, or when the enemy dies with it.</summary>
        public bool AnswersStruck;
        public bool AnswersEnd;
        public bool AnswersDeath;
        /// <summary>True when the mark that set it has something to do when its carrier strikes at the player.</summary>
        public bool AnswersAttack;
        /// <summary>The afflictions it keeps from fading, and those it lasts only while, as bits at <c>1 &lt;&lt; (int)kind</c>.</summary>
        public int KeepsMask;
        /// <summary>The affliction it lets lie deeper, as a bit, and by how much.</summary>
        public int DeepMask;
        public int DeepBy;
        public int WhileMask;
        public bool OnBite;
        public ConditionBlow EndsWith;
        public int BlowLimit;
    }
}
