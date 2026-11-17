using System;

namespace Clover.Player
{
    /// <summary>
    /// Being held, as a wick holds: the one held can do nothing but struggle. The hold runs out by itself, burning at
    /// intervals as it goes, and every struggle takes time off it. Plain C#: whoever holds ticks it and deals the bites.
    /// </summary>
    [Serializable]
    public class Grip
    {
        public float Seconds = 2.5f;
        public float BiteEvery = 0.5f;
        public float BiteDamage = 6f;
        /// <summary>Seconds each press of light attack or dodge takes off the hold.</summary>
        public float PressTakes = 0.4f;

        public bool IsHeld { get; private set; }
        /// <summary>True when the hold ended because the one held struggled out of it and not because it ran its time.</summary>
        public bool BrokenByStruggle { get; private set; }
        public int Presses { get; private set; }
        public int Bites { get; private set; }
        public float Left => left;

        private float left;
        private float elapsed;

        public void Begin()
        {
            IsHeld = true;
            BrokenByStruggle = false;
            Presses = 0;
            Bites = 0;
            left = Seconds;
            elapsed = 0f;
        }

        /// <summary>One struggle. It counts toward breaking the hold only while held.</summary>
        public void Press()
        {
            if (!IsHeld) return;
            Presses++;
            left -= PressTakes;
            if (left <= 0f)
            {
                IsHeld = false;
                BrokenByStruggle = true;
            }
        }

        /// <summary>Advances the hold. Returns how many bites fall on this tick.</summary>
        public int Tick(float deltaTime)
        {
            if (!IsHeld) return 0;
            // Counted from the time held so far, so the last bite falls with the end of the hold and is not lost to rounding.
            elapsed = Math.Min(Seconds, elapsed + Math.Min(deltaTime, left));
            int due = BiteEvery > 0f ? (int)Math.Floor(elapsed / BiteEvery + 0.0001f) : 0;
            int bites = Math.Max(0, due - Bites);
            Bites += bites;
            left -= deltaTime;
            if (left <= 0f)
            {
                IsHeld = false;
                // Ended early because it was struggled against: that is the struggle breaking it, whichever tick it fell on.
                BrokenByStruggle = Presses > 0;
            }
            return bites;
        }

        /// <summary>Lets go at once, as when the holder is struck down.</summary>
        public void Release() => IsHeld = false;
    }
}
