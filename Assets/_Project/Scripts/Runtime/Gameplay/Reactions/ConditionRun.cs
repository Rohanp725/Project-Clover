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
        /// <summary>The affliction it makes bite harder, as a bit, by how much, and whether only while frozen.</summary>
        public int BiteMask;
        public float BiteScale;
        public bool BiteFrozen;
        /// <summary>The affliction whose bites it holds back, as a bit, and how hard they land when let go.</summary>
        public int HoldMask;
        public float HeldScale;
        /// <summary>The affliction whose beat it quickens, as a bit, and by how much.</summary>
        public int QuickMask;
        public float QuickBy;
        public int DeepBy;
        public int WhileMask;
        public bool OnBite;
        public ConditionBlow EndsWith;
        public int BlowLimit;
        /// <summary>What it is beyond its own name (<see cref="ConditionSpec.Is"/>), and the condition it lasts only while (<see cref="ConditionSpec.WhileIn"/>).</summary>
        public string Is;
        public string WhileIn;
        /// <summary>Only critical weapon blows are counted, told and spend it (<see cref="ConditionSpec.CriticalOnly"/>).</summary>
        public bool CriticalOnly;
        /// <summary>The way the enemy faced as it was set, when the spec keeps it (<see cref="ConditionSpec.KeepsFacing"/>); zero otherwise.</summary>
        public Vector3 Facing;
        /// <summary>Seconds a freeze lasts longer, and how much harder a shatter strikes, while it runs.</summary>
        public float FreezeLonger;
        public float ShatterHarder;
        /// <summary>It ends when the enemy freezes (<see cref="ConditionSpec.EndsOnFreeze"/>).</summary>
        public bool EndsOnFreeze;
        /// <summary>Metres it drifts to the nearest other enemy when its carrier dies (<see cref="ConditionSpec.DriftsTo"/>); zero: it dies with it.</summary>
        public float DriftsTo;
        /// <summary>Metres round its carrier within which another's death is told to it (<see cref="ConditionSpec.DeathsWithin"/>).</summary>
        public float DeathsWithin;
        /// <summary>Metres the body is drawn down while it runs (<see cref="ConditionSpec.Sink"/>).</summary>
        public float Sink;
        /// <summary>The point it is turned to face (<see cref="ConditionFlags.FacesPoint"/>).</summary>
        public Vector3 FacePoint;
        /// <summary>The leash's length, whether the player holds it, and the spot it was set on (<see cref="ConditionSpec.Leash"/>).</summary>
        public float Leash;
        public bool LeashToPlayer;
        public Vector3 Anchor;
        /// <summary>Spell blows pass along its chain too (<see cref="ConditionSpec.SharesSpells"/>).</summary>
        public bool SharesSpells;
    }
}
