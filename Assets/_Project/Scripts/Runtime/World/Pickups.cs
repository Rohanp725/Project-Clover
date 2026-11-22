using UnityEngine;

namespace Clover.World
{
    /// <summary>What lies on the floor for the player to take (the ledger's pickups; the owner's answer of 2026-11-21: real pickups, walked over).</summary>
    public enum PickupKind
    {
        /// <summary>Shining Herb's herb at the enemy's feet, on the player's side.</summary>
        Herb,
        /// <summary>A flame lily's fire (Firebloom), or the gap between a pair of them (Between Two Fires).</summary>
        LilyFire,
        /// <summary>Seed Under Snow's frozen seed, cracked by stepping on it.</summary>
        FrozenSeed,
    }

    /// <summary>One thing on the floor. Plain data: <see cref="Pickups"/> keeps them.</summary>
    public struct Pickup
    {
        public bool Active;
        public PickupKind Kind;
        public Vector3 Position;
        /// <summary>When it is gone if no one takes it.</summary>
        public float Until;
        /// <summary>How near the player's feet must come to take it.</summary>
        public float Reach;
        /// <summary>The mark that dropped it, by index: only its own effects answer its taking, and its being left.</summary>
        public int Owner;
        /// <summary>Drawn across the floor to the player (The Whole Mountain): taken when it arrives.</summary>
        public bool Drawn;
        /// <summary>The effect that dropped it (a Player.UpgradeEffect): the effects written beside it answer its taking and its being left.</summary>
        public object Source;
    }

    /// <summary>The plain rules of a pickup.</summary>
    public static class PickupRules
    {
        /// <summary>The owner's 8 seconds on the floor, and the metre the feet must come within.</summary>
        public const float Life = 8f, Reach = 1f;
        /// <summary>How fast a drawn pickup crosses the floor to the player, in metres a second.</summary>
        public const float DrawSpeed = 9f;

        /// <summary>True when feet at <paramref name="feet"/> take it.</summary>
        public static bool Takes(in Pickup pickup, Vector3 feet)
        {
            if (!pickup.Active) return false;
            Vector3 gap = pickup.Position - feet;
            gap.y = 0f;
            return gap.magnitude <= pickup.Reach;
        }

        /// <summary>One step of a drawn pickup toward the player's feet.</summary>
        public static Vector3 DrawStep(Vector3 at, Vector3 feet, float deltaTime)
        {
            Vector3 gap = feet - at;
            gap.y = 0f;
            return at + Vector3.ClampMagnitude(gap, DrawSpeed * deltaTime);
        }
    }

    /// <summary>
    /// Everything on the floor for the player to take. Marks drop them; the player's marks take the ones the feet reach each
    /// frame (<see cref="TakeAt"/>) and are told of those left to lie until their time ran out (<see cref="TakeLeft"/>).
    /// </summary>
    public static class Pickups
    {
        public const int Most = 16;
        private static readonly Pickup[] items = new Pickup[Most];

        // Domain reload is off: nothing dropped in the last play lies in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        public static void Clear()
        {
            for (int i = 0; i < Most; i++) items[i] = default;
        }

        /// <summary>
        /// Drops a pickup at <paramref name="at"/> for <paramref name="seconds"/> (0: the plain 8). With <paramref name="most"/>
        /// above zero, at most that many of its mark lie at once: the one nearest its end goes. Returns its place.
        /// </summary>
        public static int Drop(PickupKind kind, Vector3 at, float seconds, int owner, float now, int most = 0, float reach = 0f, object source = null)
        {
            if (most > 0)
                while (Count(owner) >= most) items[NearestEnd(owner)] = default;
            int slot = -1;
            for (int i = 0; i < Most && slot < 0; i++)
                if (!items[i].Active) slot = i;
            if (slot < 0) slot = NearestEnd(-1);
            items[slot] = new Pickup
            {
                Active = true, Kind = kind, Position = at, Until = now + (seconds > 0f ? seconds : PickupRules.Life),
                Reach = reach > 0f ? reach : PickupRules.Reach, Owner = owner, Source = source,
            };
            return slot;
        }

        public static Pickup Get(int slot) => slot >= 0 && slot < Most ? items[slot] : default;

        /// <summary>How many lie now, of one mark when given.</summary>
        public static int Count(int owner = -1)
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
                if (items[i].Active && (owner < 0 || items[i].Owner == owner)) count++;
            return count;
        }

        /// <summary>Takes one pickup the feet at <paramref name="feet"/> reach; false when none.</summary>
        public static bool TakeAt(Vector3 feet, out Pickup taken)
        {
            taken = default;
            for (int i = 0; i < Most; i++)
            {
                if (!PickupRules.Takes(items[i], feet)) continue;
                taken = items[i];
                items[i] = default;
                return true;
            }
            return false;
        }

        /// <summary>Takes one pickup whose time ran out untaken; false when none.</summary>
        public static bool TakeLeft(float now, out Pickup left)
        {
            left = default;
            for (int i = 0; i < Most; i++)
            {
                if (!items[i].Active || now < items[i].Until) continue;
                left = items[i];
                items[i] = default;
                return true;
            }
            return false;
        }

        /// <summary>Every pickup of one kind starts across the floor to the player (The Whole Mountain). Returns how many.</summary>
        public static int DrawAll(PickupKind kind)
        {
            int drawn = 0;
            for (int i = 0; i < Most; i++)
            {
                if (!items[i].Active || items[i].Kind != kind || items[i].Drawn) continue;
                items[i].Drawn = true;
                drawn++;
            }
            return drawn;
        }

        /// <summary>Moves the drawn pickups a step toward the player's feet.</summary>
        public static void StepDrawn(Vector3 feet, float deltaTime)
        {
            for (int i = 0; i < Most; i++)
                if (items[i].Active && items[i].Drawn) items[i].Position = PickupRules.DrawStep(items[i].Position, feet, deltaTime);
        }

        private static int NearestEnd(int owner)
        {
            int best = 0;
            float soonest = float.MaxValue;
            for (int i = 0; i < Most; i++)
            {
                if (!items[i].Active || (owner >= 0 && items[i].Owner != owner) || items[i].Until >= soonest) continue;
                soonest = items[i].Until;
                best = i;
            }
            return best;
        }
    }
}
