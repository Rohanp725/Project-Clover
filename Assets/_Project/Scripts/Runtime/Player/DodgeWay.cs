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
        /// <summary>Above zero: it goes this many metres, not the roll's own (carried to an enemy by Eagle's Sight).</summary>
        public float Reach;

        /// <summary>A dodge as written.</summary>
        public static DodgeTerms Plain => new DodgeTerms { CostScale = 1f };
    }

    /// <summary>Plain rules for which way a dodge goes.</summary>
    public static class DodgeRules
    {
        /// <summary>The farthest a dodge toward an enemy that draws it is carried (Eagle's Sight: 6 m).</summary>
        public const float CarryReach = 6f;
        /// <summary>How far short of the enemy a carried dodge stops, so the cut lands at arm's length.</summary>
        public const float CarryShort = 1f;

        /// <summary>
        /// How far a dodge from <paramref name="from"/> along <paramref name="direction"/> is carried to an enemy at
        /// <paramref name="enemy"/>: to arm's length of it when it lies toward the dodge and within <see cref="CarryReach"/>; 0 otherwise.
        /// </summary>
        public static float CarryTo(Vector3 from, Vector3 direction, Vector3 enemy)
        {
            Vector3 to = enemy - from;
            to.y = 0f;
            direction.y = 0f;
            float distance = to.magnitude;
            if (distance < 0.0001f || distance > CarryReach || direction.sqrMagnitude < 0.0001f) return 0f;
            if (Vector3.Dot(to / distance, direction.normalized) < WithinDot) return 0f;
            return Mathf.Max(0.5f, distance - CarryShort);
        }

        /// <summary>The cosine of the widest angle that still counts as toward (or away): 60 degrees.</summary>
        public const float WithinDot = 0.5f;

        /// <summary>
        /// How far, in metres on the ground, the player may get from the enemy a run of dodges began against before it is let
        /// go (the owner's answer of 2026-11-20): the fight's 12 m and most of one 4.5 m roll more, so a roll back from an
        /// enemy at the fight's edge still keeps it.
        /// </summary>
        public const float HoldRange = 16f;

        /// <summary>
        /// Seconds without a dodge after which the enemy a run of dodges began against is let go (the owner's answer of
        /// 2026-11-20, evening), measured from the last dodge: the next dodge after a longer pause takes the enemy faced then.
        /// </summary>
        public const float HoldSeconds = 3f;

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

        /// <summary>
        /// Whether a run of dodges is still going at <paramref name="now"/>: no more than <paramref name="seconds"/> have
        /// passed since its last dodge, at <paramref name="lastDodge"/>.
        /// </summary>
        public static bool StillRunning(float now, float lastDodge, float seconds) => now - lastDodge <= seconds;
    }

    /// <summary>
    /// The enemy a run of dodges is measured against (the owner's answer of 2026-11-20). A roll turns the player the way it
    /// goes, so after a roll away the enemy ahead is often another one, and in a crowd a second roll away could count as
    /// toward someone else. So the first dodge holds the enemy it was measured against, and the dodges after it keep to that
    /// one until it dies, the player is more than <see cref="DodgeRules.HoldRange"/> from it, or
    /// <see cref="DodgeRules.HoldSeconds"/> pass without a dodge. Plain, with the time passed in, so tests can hold any kind
    /// of thing at any moment.
    /// </summary>
    public sealed class DodgeHold<T> where T : class
    {
        /// <summary>The enemy held, or null when no dodge has held one (or the last one was let go).</summary>
        public T Held { get; private set; }

        /// <summary>When the last dodge was, on the clock the caller passes in.</summary>
        public float LastDodgeAt { get; private set; } = float.NegativeInfinity;

        /// <summary>
        /// Whether the enemy held still counts for a dodge from <paramref name="from"/> at <paramref name="now"/>, given
        /// whether it lives and where it stands: alive, within <paramref name="range"/>, and no more than
        /// <paramref name="seconds"/> since the last dodge. Lets it go when not, so the next dodge takes the enemy faced then.
        /// </summary>
        public bool Keeps(Vector3 from, bool heldAlive, Vector3 heldAt, float range, float now, float seconds)
        {
            if (Held == null) return false;
            if (DodgeRules.StillHeld(heldAlive, from, heldAt, range) && DodgeRules.StillRunning(now, LastDodgeAt, seconds)) return true;
            Held = null;
            return false;
        }

        /// <summary>
        /// The enemy a dodge from <paramref name="from"/> at <paramref name="now"/> is measured against: the one held while it
        /// still counts, otherwise <paramref name="faced"/>, the one faced now.
        /// </summary>
        public T Against(Vector3 from, T faced, bool heldAlive, Vector3 heldAt, float range, float now, float seconds) =>
            Keeps(from, heldAlive, heldAt, range, now, seconds) ? Held : faced;

        /// <summary>
        /// A dodge went against this enemy at <paramref name="now"/>: the first of a run holds it, and every dodge starts the
        /// pause again. A dodge against no one holds nothing.
        /// </summary>
        public void Took(T against, float now)
        {
            if (Held == null) Held = against;
            LastDodgeAt = now;
        }

        /// <summary>Lets the held enemy go (a new place, a new fight).</summary>
        public void Clear() => Held = null;
    }
}
