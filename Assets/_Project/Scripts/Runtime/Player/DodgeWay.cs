using UnityEngine;

namespace Clover.Player
{
    /// <summary>Which way a dodge went against the enemy faced (the ledger's dodge-direction): toward it, away from it, or across.</summary>
    public enum DodgeWay
    {
        /// <summary>For an effect: any dodge. For a dodge: no enemy to measure against, or across its line.</summary>
        Any,
        /// <summary>Toward or through the enemy faced: within 60 degrees of the line to it.</summary>
        Toward,
        /// <summary>Away from the enemy faced: within 60 degrees of the line away from it.</summary>
        Away,
    }

    /// <summary>
    /// What a dodge is, for the marks that change it (Into the Fire): what it costs and how it guards the player. Set at
    /// the dodge's start and kept for that dodge alone.
    /// </summary>
    public struct DodgeTerms
    {
        /// <summary>The stamina it costs, as a share of the usual.</summary>
        public float CostScale;
        /// <summary>Safe from blows for its whole length, not only its usual window.</summary>
        public bool WholeSafe;
        /// <summary>It can never be a perfect dodge.</summary>
        public bool NoPerfect;
        /// <summary>It goes through enemies' bodies, stopping only at walls.</summary>
        public bool PassesBodies;

        /// <summary>A dodge as written.</summary>
        public static DodgeTerms Plain => new DodgeTerms { CostScale = 1f };
    }

    /// <summary>Plain rules for which way a dodge goes.</summary>
    public static class DodgeRules
    {
        /// <summary>The cosine of the widest angle that still counts as toward (or away): 60 degrees.</summary>
        public const float WithinDot = 0.5f;

        /// <summary>
        /// Which way a dodge from <paramref name="from"/> in <paramref name="direction"/> goes against an enemy standing at
        /// <paramref name="enemy"/>: toward it within 60 degrees of the line to it, away within 60 degrees of the line away
        /// from it, otherwise across (<see cref="DodgeWay.Any"/>). With no enemy (<paramref name="hasEnemy"/> false), across.
        /// </summary>
        public static DodgeWay WayOf(Vector3 direction, Vector3 from, Vector3 enemy, bool hasEnemy)
        {
            if (!hasEnemy) return DodgeWay.Any;
            direction.y = 0f;
            Vector3 toward = enemy - from;
            toward.y = 0f;
            if (direction.sqrMagnitude < 0.0001f || toward.sqrMagnitude < 0.0001f) return DodgeWay.Any;
            float dot = Vector3.Dot(direction.normalized, toward.normalized);
            if (dot >= WithinDot) return DodgeWay.Toward;
            if (dot <= -WithinDot) return DodgeWay.Away;
            return DodgeWay.Any;
        }
    }
}
