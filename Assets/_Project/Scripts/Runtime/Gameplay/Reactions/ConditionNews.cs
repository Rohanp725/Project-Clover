using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>One thing that happened to one enemy condition: which, from which mark, and for a blow, what blow.</summary>
    public struct ConditionNews
    {
        public ConditionMoment Moment;
        /// <summary>The condition's name, as the mark that set it gave it.</summary>
        public string Id;
        /// <summary>The place in the catalogue of the mark that set it, or -1.</summary>
        public int Mark;
        /// <summary>How many beats have come, this one included.</summary>
        public int Count;
        /// <summary>How many weapon blows it has taken, this one included.</summary>
        public int Blows;
        public ConditionFlags Flags;
        /// <summary>The run's one number (see <see cref="ConditionSpec.Scale"/>).</summary>
        public float Scale;
        /// <summary>The enemy it is joined to, for a linked condition.</summary>
        public Component Link;
        /// <summary>For a blow: what it dealt before the enemy's own multipliers, whether it was heavy, and whether a spell.</summary>
        public float Damage;
        public bool Heavy;
        public bool Spell;
        /// <summary>For a blow: it broke the ice. The shatter ends the Chill before the news is answered, so this keeps it.</summary>
        public bool Shattered;
        /// <summary>For a blow: it was a critical hit.</summary>
        public bool Critical;
        /// <summary>What it is beyond its own name (<see cref="ConditionSpec.Is"/>): the plain reaction's name for a shape's same state, or empty.</summary>
        public string Is;
        /// <summary>For a blow: it was the finisher's own blow (a finisher is heavy too).</summary>
        public bool Finisher;
        /// <summary>For a blow: the side it came from, in degrees from the face (0) to the back (180); -1 when not known (<see cref="EnemyConditions.SideDegrees"/>).</summary>
        public float Side;
        /// <summary>For its carrier's blow on the player: the blow was turned by a perfect guard.</summary>
        public bool Turned;
        /// <summary>For an end: the enemy froze, and that ended it (<see cref="ConditionSpec.EndsOnFreeze"/>).</summary>
        public bool Frozen;
        /// <summary>For a blow: it was a shove or a throw, not a blow (a Gust, a ram, Riptide, a lift; the ledger's shove-moment).</summary>
        public bool Shoved;
        /// <summary>For a blow: it was no blow but another enemy's death near its carrier (<see cref="ConditionSpec.DeathsWithin"/>).</summary>
        public bool DiedNear;
    }
}
