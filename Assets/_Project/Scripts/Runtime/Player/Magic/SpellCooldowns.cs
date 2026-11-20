using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// The clocks of the four spell slots and the two infused strike buttons (docs/design/steel/IMPLEMENTATION.md 3.1).
    /// A slot's clock starts when its spell is cast and runs down; weapon play takes seconds off every clock that is
    /// running. Plain C# with fixed arrays: nothing is allocated, so the HUD may read it every frame.
    /// </summary>
    [Serializable]
    public class SpellCooldowns
    {
        /// <summary>Four slots (West, North, East, South) and the two strikes (L2 with R1, L2 with R2).</summary>
        public const int Count = 6;
        public const int StrikeFirst = 4;
        public const int StrikeSecond = 5;

        /// <summary>While true no clock is started at all (In One Breath, Given Fire).</summary>
        public bool Frozen;

        private readonly float[] left = new float[Count];
        private readonly float[] length = new float[Count];
        private readonly bool[] instant = new bool[Count];
        // Seconds an instant promise still keeps; zero keeps it until the slot is cast.
        private readonly float[] instantLeft = new float[Count];

        /// <summary>Starts a clock of this many seconds, unless the next cast of that slot was promised free.</summary>
        public void Start(int slot, float seconds)
        {
            if (!Valid(slot)) return;
            if (instant[slot])
            {
                instant[slot] = false;
                instantLeft[slot] = 0f;
                return;
            }
            if (Frozen || seconds <= 0f)
            {
                left[slot] = 0f;
                return;
            }
            left[slot] = seconds;
            length[slot] = seconds;
        }

        /// <summary>
        /// Runs every clock on by <paramref name="deltaTime"/> times <paramref name="rate"/>. Returns a bit for each slot
        /// whose clock came to its end on this tick (bit 0 is West), so the HUD can flash it.
        /// </summary>
        public int Tick(float deltaTime, float rate = 1f)
        {
            float step = Math.Max(0f, deltaTime * rate);
            int done = 0;
            for (int i = 0; i < Count; i++)
            {
                // A promise with a window lapses with it (a Master's "cast in the same instant").
                if (instant[i] && instantLeft[i] > 0f)
                {
                    instantLeft[i] -= Math.Max(0f, deltaTime);
                    if (instantLeft[i] <= 0f)
                    {
                        instantLeft[i] = 0f;
                        instant[i] = false;
                    }
                }
                if (left[i] <= 0f) continue;
                left[i] -= step;
                if (left[i] > 0f) continue;
                left[i] = 0f;
                done |= 1 << i;
            }
            return done;
        }

        /// <summary>Takes seconds off every running clock (a blow landed, a perfect guard). Returns which came to an end.</summary>
        public int Cut(float seconds) => Cut(seconds, -1);

        /// <summary>The same, leaving one clock alone: an infused strike's own blow does not shorten its own clock.</summary>
        public int Cut(float seconds, int except)
        {
            if (seconds <= 0f) return 0;
            int done = 0;
            for (int i = 0; i < Count; i++)
            {
                if (left[i] <= 0f || i == except) continue;
                left[i] -= seconds;
                if (left[i] > 0f) continue;
                left[i] = 0f;
                done |= 1 << i;
            }
            return done;
        }

        /// <summary>Ends one clock at once.</summary>
        public void Reset(int slot)
        {
            if (Valid(slot)) left[slot] = 0f;
        }

        /// <summary>Ends every clock, as a rest does.</summary>
        public void ResetAll()
        {
            for (int i = 0; i < Count; i++)
            {
                left[i] = 0f;
                instant[i] = false;
                instantLeft[i] = 0f;
            }
        }

        /// <summary>The next cast of this slot starts no clock: a Master's promise.</summary>
        public void InstantNext(int slot) => InstantNext(slot, 0f);

        /// <summary>
        /// The same, kept only for <paramref name="seconds"/> (0: until the slot is cast, and its clock is ended now). With a
        /// window the clock is left running: "cast in the same instant as the blow lands, and its cooldown does not start"
        /// asks that the spell was ready.
        /// </summary>
        public void InstantNext(int slot, float seconds)
        {
            if (!Valid(slot)) return;
            instant[slot] = true;
            instantLeft[slot] = Math.Max(0f, seconds);
            if (seconds <= 0f) left[slot] = 0f;
        }

        public bool IsInstant(int slot) => Valid(slot) && instant[slot];
        public bool Ready(int slot) => !Valid(slot) || left[slot] <= 0f;
        public float Remaining(int slot) => Valid(slot) ? left[slot] : 0f;
        public float Length(int slot) => Valid(slot) ? length[slot] : 0f;

        /// <summary>How much of the clock is still to run: 1 just cast, 0 ready.</summary>
        public float Fraction(int slot) => Valid(slot) && left[slot] > 0f && length[slot] > 0f ? Math.Min(1f, left[slot] / length[slot]) : 0f;

        private static bool Valid(int slot) => slot >= 0 && slot < Count;
    }
}
