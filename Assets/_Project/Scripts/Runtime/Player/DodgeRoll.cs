using System;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The dodge roll: a fixed-distance burst in one direction with a window in which hits pass through, then a short
    /// recovery before the player can roll again. Plain C#; the controller feeds it time and applies the movement it returns.
    /// </summary>
    [Serializable]
    public class DodgeRoll
    {
        public float Distance = 4.5f;
        public float Duration = 0.35f;
        public float StaminaCost = 25f;
        /// <summary>When set, the roll is paid for by asking this (true: paid) and costs no stamina. Leaf on the Wind sets it.</summary>
        [NonSerialized] public Func<bool> PayInstead;
        /// <summary>Hits pass through the player between these two moments of the roll, in seconds from its start.</summary>
        public float InvulnerableFrom = 0.03f;
        public float InvulnerableUntil = 0.25f;
        /// <summary>Seconds after the roll ends before another roll can start.</summary>
        public float Recovery = 0.1f;

        public bool IsRolling { get; private set; }
        public bool IsInvulnerable => IsRolling && elapsed >= InvulnerableFrom && (elapsed < InvulnerableUntil || WholeSafe);
        /// <summary>For this roll alone (Into the Fire, toward the enemy): safe for its whole length. Cleared as each roll starts.</summary>
        [NonSerialized] public bool WholeSafe;
        /// <summary>For this roll alone (Into the Fire, away from the enemy): never a perfect dodge. Cleared as each roll starts.</summary>
        [NonSerialized] public bool NoPerfect;
        /// <summary>For this roll alone (Into the Fire, toward the enemy): through bodies, stopping only at walls. Cleared as each roll starts.</summary>
        [NonSerialized] public bool PassesBodies;
        public bool CanStart => !IsRolling && sinceEnd >= Recovery;
        public Vector3 Direction { get; private set; }
        /// <summary>How far through the roll's time the player is, from 0 to 1; 0 when not rolling.</summary>
        public float Progress => IsRolling && Duration > 0f ? Mathf.Clamp01(elapsed / Duration) : 0f;

        private float elapsed;
        private float sinceEnd = float.MaxValue;

        public void Reset()
        {
            IsRolling = false;
            elapsed = 0f;
            sinceEnd = float.MaxValue;
        }

        /// <summary>Seconds since the roll began; meaningful only while rolling.</summary>
        public float Elapsed => elapsed;

        /// <summary>Ends the roll where it is, as a blow that lands on its unprotected tail does.</summary>
        public void Stop()
        {
            if (!IsRolling) return;
            IsRolling = false;
            sinceEnd = 0f;
        }

        public bool TryStart(Vector3 direction, StaminaPool stamina) => TryStart(direction, stamina, DodgeTerms.Plain);

        /// <summary>Starts a roll on the given terms (its cost, how it guards): false when it cannot be paid for or begun.</summary>
        public bool TryStart(Vector3 direction, StaminaPool stamina, DodgeTerms terms)
        {
            direction.y = 0f;
            if (!CanStart || direction.sqrMagnitude < 0.0001f) return false;
            float scale = terms.CostScale > 0f ? terms.CostScale : 1f;
            if (PayInstead != null)
            {
                if (!PayInstead()) return false;
                // Paid in breath, a dearer roll still asks the stamina beyond the usual.
                if (scale > 1f && !stamina.TrySpend(StaminaCost * (scale - 1f))) return false;
            }
            else if (!stamina.TrySpend(StaminaCost * scale)) return false;
            Direction = direction.normalized;
            IsRolling = true;
            elapsed = 0f;
            WholeSafe = terms.WholeSafe;
            NoPerfect = terms.NoPerfect;
            PassesBodies = terms.PassesBodies;
            return true;
        }

        /// <summary>Advances the roll and returns how far to move this step. The roll starts fast and eases out.</summary>
        public Vector3 Tick(float deltaTime)
        {
            if (!IsRolling)
            {
                if (sinceEnd < float.MaxValue) sinceEnd += deltaTime;
                return Vector3.zero;
            }

            float before = Eased(elapsed);
            elapsed = Mathf.Min(Duration, elapsed + deltaTime);
            float after = Eased(elapsed);
            if (elapsed >= Duration)
            {
                IsRolling = false;
                sinceEnd = 0f;
            }
            return Direction * (Distance * (after - before));
        }

        // Ease-out: covers most of the distance early, so the roll feels like a burst that settles.
        private float Eased(float time)
        {
            float t = Duration > 0f ? Mathf.Clamp01(time / Duration) : 1f;
            return 1f - (1f - t) * (1f - t);
        }
    }
}
