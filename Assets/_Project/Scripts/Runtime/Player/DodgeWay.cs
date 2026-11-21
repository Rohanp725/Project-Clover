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
        /// How far, in metres on the ground, the player may get from the enemy a run of dodges began against before it is let
        /// go (the owner's answer of 2026-11-20): the fight's 12 m and most of one 4.5 m roll more, so a roll back from an
        /// enemy at the fight's edge still keeps it.
        /// </summary>
        public const float HoldRange = 16f;

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

        /// <summary>
        /// Whether an enemy held since an earlier dodge still counts for the next one from <paramref name="from"/>: it lives
        /// and stands within <paramref name="range"/> on the ground.
        /// </summary>
        public static bool StillHeld(bool alive, Vector3 from, Vector3 held, float range)
        {
            if (!alive) return false;
            Vector3 apart = held - from;
            apart.y = 0f;
            return apart.sqrMagnitude <= range * range;
        }
    }

    /// <summary>
    /// The enemy a run of dodges is measured against (the owner's answer of 2026-11-20). A roll turns the player the way it
    /// goes, so after a roll away the enemy ahead is often another one, and in a crowd a second roll away could count as
    /// toward someone else. So the first dodge holds the enemy it was measured against, and the dodges after it keep to that
    /// one until it dies or the player is more than <see cref="DodgeRules.HoldRange"/> from it. Plain, so tests can hold any
    /// kind of thing.
    /// </summary>
    public sealed class DodgeHold<T> where T : class
    {
        /// <summary>The enemy held, or null when no dodge has held one (or the last one was let go).</summary>
        public T Held { get; private set; }

        /// <summary>
        /// Whether the enemy held still counts for a dodge from <paramref name="from"/>, given whether it lives and where it
        /// stands. Lets it go when not, so the next dodge takes the enemy faced then.
        /// </summary>
        public bool Keeps(Vector3 from, bool heldAlive, Vector3 heldAt, float range)
        {
            if (Held == null) return false;
            if (DodgeRules.StillHeld(heldAlive, from, heldAt, range)) return true;
            Held = null;
            return false;
        }

        /// <summary>
        /// The enemy a dodge from <paramref name="from"/> is measured against: the one held while it still counts, otherwise
        /// <paramref name="faced"/>, the one faced now.
        /// </summary>
        public T Against(Vector3 from, T faced, bool heldAlive, Vector3 heldAt, float range) =>
            Keeps(from, heldAlive, heldAt, range) ? Held : faced;

        /// <summary>A dodge went against this enemy: the first of a run holds it. A dodge against no one holds nothing.</summary>
        public void Took(T against)
        {
            if (Held == null) Held = against;
        }

        /// <summary>Lets the held enemy go (a new place, a new fight).</summary>
        public void Clear() => Held = null;
    }
}
