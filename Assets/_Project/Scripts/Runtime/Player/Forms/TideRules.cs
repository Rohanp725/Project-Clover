using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The plain rules of Riptide, Water's wave: it rises ahead of the caster, rolls back toward them, carries what it
    /// reaches along with it and breaks a sword's length in front of their feet. Distances are measured along the
    /// line the caster faces, from where they stand.
    /// </summary>
    public static class TideRules
    {
        /// <summary>Metres in front of the caster where the wave breaks and sets down what it carried: sword reach.</summary>
        public const float BreaksAt = 1.3f;
        /// <summary>Seconds the wave takes to stand up before it moves.</summary>
        public const float RiseSeconds = 0.25f;
        /// <summary>Metres a second it rolls toward the caster.</summary>
        public const float Speed = 8f;
        /// <summary>How far to either side of its line the wave reaches.</summary>
        public const float HalfWidth = 2.4f;
        /// <summary>The wave takes hold of what stands this far in front of its face, and keeps what has fallen this far behind it.</summary>
        public const float Ahead = 0.4f, Behind = 1.3f;

        /// <summary>Where the wave's face is, in metres ahead of the caster, this long after the cast: it stands where it rose, then comes in, and stops where it breaks.</summary>
        public static float Front(float reach, float age)
        {
            reach = Mathf.Max(reach, BreaksAt);
            if (age <= RiseSeconds) return reach;
            return Mathf.Max(BreaksAt, reach - (age - RiseSeconds) * Speed);
        }

        /// <summary>True once the wave has come all the way in.</summary>
        public static bool Broken(float reach, float age) => age > RiseSeconds && Front(reach, age) <= BreaksAt;

        /// <summary>True when the wave's face has reached something standing this far ahead of the caster and this far off the wave's line.</summary>
        public static bool Reached(float along, float aside, float front) =>
            Mathf.Abs(aside) <= HalfWidth && along > BreaksAt && front <= along + Ahead && front >= along - Behind;

        /// <summary>
        /// Metres the wave carries something toward the caster in one step in which its face moved <paramref name="step"/>
        /// metres: as far as the wave itself moved, never past where it breaks, and nothing at all for what stands
        /// outside it, beyond where it rose, or already at the caster's feet.
        /// </summary>
        public static float Carry(float along, float aside, float front, float step)
        {
            if (step <= 0f || !Reached(along, aside, front)) return 0f;
            return Mathf.Min(step, along - BreaksAt);
        }
    }
}
