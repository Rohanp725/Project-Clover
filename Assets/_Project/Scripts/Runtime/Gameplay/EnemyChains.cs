using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Enemies chained to each other (the ledger's links-pass-blows): Black Chain's chain, Shared Wound's. A chain is a
    /// condition on one enemy (<see cref="ConditionFlags.Chained"/>) joined to another (the run's Link); this keeps the
    /// enemies that carry one, so the other end can find its mate too, and holds the plain rules of a leash. Kept small and
    /// fixed: nothing is allocated after it starts.
    /// </summary>
    public static class EnemyChains
    {
        /// <summary>The most chained carriers kept at once; a new one takes the place of one whose chain is gone.</summary>
        public const int Most = 24;
        /// <summary>Metres a second a body beyond its leash is drawn back toward what holds it.</summary>
        public const float DrawRate = 2f;

        private static readonly Component[] carriers = new Component[Most];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        /// <summary>Forgets every chain: each play begins so, and tests that note one put it back.</summary>
        public static void Clear() => Array.Clear(carriers, 0, carriers.Length);

        /// <summary>A chain has begun on <paramref name="carrier"/>: it is kept, so its other end can find it.</summary>
        public static void Note(Component carrier)
        {
            if (carrier == null) return;
            int free = -1;
            for (int i = 0; i < Most; i++)
            {
                if (carriers[i] == carrier) return;
                if (free < 0 && !Holds(carriers[i])) free = i;
            }
            if (free >= 0) carriers[free] = carrier;
        }

        /// <summary>
        /// The one chained to <paramref name="who"/>: the other end of a chain it carries, or else one that carries a chain to
        /// it; null when it is chained to no one. Its share of blows passed, and whether spells pass too, come with it.
        /// </summary>
        public static Component MateOf(Component who, out float share, out bool spells)
        {
            share = 0f;
            spells = false;
            if (who == null) return null;
            if (who is IAfflictable own)
            {
                var mate = own.Afflictions.Conditions.ChainMate(out share, out spells);
                if (mate != null) return mate;
            }
            for (int i = 0; i < Most; i++)
            {
                var carrier = carriers[i];
                if (carrier == null || carrier == who || !(carrier is IAfflictable held)) continue;
                if (held.Afflictions.Conditions.ChainedTo(who, out share, out spells)) return carrier;
            }
            return null;
        }

        public static Component MateOf(Component who) => MateOf(who, out _, out _);

        /// <summary>
        /// Every mate of <paramref name="who"/> a blow on it passes to (both ways along each chain), written into the arrays
        /// given with each one's share; spell blows pass only along chains that share them. Returns how many.
        /// </summary>
        public static int Mates(Component who, bool spell, Component[] into, float[] shares)
        {
            int count = 0;
            if (who == null || into == null || shares == null) return 0;
            if (who is IAfflictable own && own.Afflictions.Conditions.Has(ConditionFlags.Chained))
            {
                var mate = own.Afflictions.Conditions.ChainMate(out float share, out bool spells);
                if (mate != null && share > 0f && (!spell || spells) && count < into.Length)
                {
                    into[count] = mate;
                    shares[count++] = share;
                }
            }
            for (int i = 0; i < Most && count < into.Length; i++)
            {
                var carrier = carriers[i];
                if (carrier == null || carrier == who || !(carrier is IAfflictable held)) continue;
                if (!held.Afflictions.Conditions.ChainedTo(who, out float share, out bool spells) || share <= 0f || (spell && !spells)) continue;
                if (Array.IndexOf(into, carrier, 0, count) >= 0) continue;
                into[count] = carrier;
                shares[count++] = share;
            }
            return count;
        }

        /// <summary>
        /// What holds <paramref name="who"/> on a leash, if anything: the far end of its own chain, the player, or the spot it
        /// was chained to the floor at; or one that carries a leashed chain to it. False when it is free.
        /// </summary>
        public static bool LeashOf(Component who, Vector3 player, out Vector3 anchor, out float leash)
        {
            anchor = Vector3.zero;
            leash = 0f;
            if (who == null) return false;
            if (who is IAfflictable own && own.Afflictions.Conditions.Leashed(out Component link, out Vector3 spot, out leash, out bool toPlayer))
            {
                anchor = toPlayer ? player : link != null ? link.transform.position : spot;
                return true;
            }
            for (int i = 0; i < Most; i++)
            {
                var carrier = carriers[i];
                if (carrier == null || carrier == who || !(carrier is IAfflictable held)) continue;
                if (!held.Afflictions.Conditions.ChainedTo(who, out _, out _)) continue;
                if (!held.Afflictions.Conditions.Leashed(out Component theirEnd, out _, out leash, out bool theirsToPlayer) || theirsToPlayer || theirEnd != who) continue;
                anchor = carrier.transform.position;
                return true;
            }
            return false;
        }

        /// <summary>
        /// The step a leashed body may take: never farther from <paramref name="anchor"/> than its leash, or than it already
        /// stands if it is beyond it, and drawn back toward it at <see cref="DrawRate"/> while it is beyond. Height is its own.
        /// </summary>
        public static Vector3 Leash(Vector3 at, Vector3 step, Vector3 anchor, float leash, float deltaTime)
        {
            if (leash <= 0f) return step;
            Vector3 from = at - anchor;
            from.y = 0f;
            float now = from.magnitude;
            Vector3 next = at + step - anchor;
            next.y = 0f;
            float limit = Math.Max(leash, now - DrawRate * Math.Max(0f, deltaTime));
            if (next.magnitude <= limit) return step;
            Vector3 way = next.sqrMagnitude > 0.0001f ? next.normalized : (now > 0.0001f ? from / now : Vector3.forward);
            Vector3 kept = anchor + way * limit;
            Vector3 result = kept - at;
            result.y = step.y;
            return result;
        }

        // A carrier whose chain is still on it, and it still there.
        private static bool Holds(Component carrier) =>
            carrier != null && carrier.gameObject.activeInHierarchy && carrier is IAfflictable held && held.Afflictions.Conditions.Has(ConditionFlags.Chained);
    }
}
