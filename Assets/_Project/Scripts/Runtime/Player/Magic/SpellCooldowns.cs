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

        /// <summary>While true no clock is started at all (the RuleBook's <c>CooldownMode.None</c>: Given Fire).</summary>
        public bool Frozen;

        /// <summary>True during a breath (In One Breath): every spell slot is ready, whatever its clock says.</summary>
        public bool InBreath => breathLeft > 0f;
        /// <summary>Seconds of the breath still to run.</summary>
        public float BreathLeft => breathLeft;

        private float breathLeft;
        // The wait each spell slot cast in the breath owes, started from full when the breath ends.
        private readonly float[] owed = new float[Count];
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
            // Cast in the breath: no clock now; the whole wait starts when the breath ends.
            if (breathLeft > 0f && IsSpell(slot))
            {
                owed[slot] = Math.Max(owed[slot], seconds);
                return;
            }
            left[slot] = seconds;
            length[slot] = seconds;
        }

        /// <summary>
        /// A breath of <paramref name="seconds"/> (In One Breath, after a perfect guard or dodge): the four spell slots have
        /// no cooldown until it ends, the strikes keep theirs. A spell cast in it waits its whole time once the breath is
        /// over; the clocks of spells not cast run on beneath it. A breath taken during one lasts the longer of the two.
        /// Returns a bit for each spell slot that was waiting and is ready now, so the HUD can flash it.
        /// </summary>
        public int Breathe(float seconds)
        {
            if (seconds <= 0f || Frozen) return 0;
            int freed = 0;
            if (breathLeft <= 0f)
                for (int i = 0; i < StrikeFirst; i++)
                    if (left[i] > 0f) freed |= 1 << i;
            breathLeft = Math.Max(breathLeft, seconds);
            return freed;
        }

        /// <summary>The breath ends at once; what was cast in it starts its wait. Returns a bit for each slot whose wait started.</summary>
        public int EndBreath()
        {
            if (breathLeft <= 0f) return 0;
            breathLeft = 0f;
            int started = 0;
            for (int i = 0; i < StrikeFirst; i++)
            {
                if (owed[i] <= 0f) continue;
                left[i] = owed[i];
                length[i] = owed[i];
                owed[i] = 0f;
                started |= 1 << i;
            }
            return started;
        }

        /// <summary>
        /// Runs every clock on by <paramref name="deltaTime"/> times <paramref name="rate"/>. Returns a bit for each slot
        /// whose clock came to its end on this tick (bit 0 is West), so the HUD can flash it.
        /// </summary>
        public int Tick(float deltaTime, float rate = 1f)
        {
            float step = Math.Max(0f, deltaTime * rate);
            int done = 0;
            // The breath runs on the game's own time, not the clocks' rate. A wait it owed starts when it ends, so on the
            // step that ends it that wait loses only the time after the end.
            bool breathing = breathLeft > 0f;
            int started = 0;
            float afterBreath = 0f;
            if (breathing)
            {
                float passed = Math.Max(0f, deltaTime);
                if (passed >= breathLeft)
                {
                    afterBreath = Math.Max(0f, (passed - breathLeft) * rate);
                    started = EndBreath();
                }
                else breathLeft -= passed;
            }
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
                left[i] -= (started & (1 << i)) != 0 ? afterBreath : step;
                if (left[i] > 0f) continue;
                left[i] = 0f;
                // A spell slot already shown ready by the breath does not flash again.
                if (!(breathing && IsSpell(i))) done |= 1 << i;
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
                // A spell slot the breath already shows ready does not flash again.
                if (!Breathing(i)) done |= 1 << i;
            }
            return done;
        }

        /// <summary>
        /// Keeps only <paramref name="share"/> of a slot's wait, as if it had been that long from the start (a counter nothing
        /// struck waits half as long). The time already run counts; a wait owed to the breath is cut the same way.
        /// </summary>
        public void KeepShare(int slot, float share)
        {
            if (!Valid(slot) || share >= 1f) return;
            share = Math.Max(0f, share);
            owed[slot] *= share;
            if (left[slot] <= 0f) return;
            left[slot] = Math.Max(0f, left[slot] - length[slot] * (1f - share));
        }

        /// <summary>Ends one clock at once (and any wait it owes the breath).</summary>
        public void Reset(int slot)
        {
            if (!Valid(slot)) return;
            left[slot] = 0f;
            owed[slot] = 0f;
        }

        /// <summary>Ends every clock, and any breath, as a rest does.</summary>
        public void ResetAll()
        {
            breathLeft = 0f;
            for (int i = 0; i < Count; i++)
            {
                left[i] = 0f;
                owed[i] = 0f;
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
        public bool Ready(int slot) => !Valid(slot) || left[slot] <= 0f || Breathing(slot);
        public float Remaining(int slot) => Valid(slot) && !Breathing(slot) ? left[slot] : 0f;
        public float Length(int slot) => Valid(slot) ? length[slot] : 0f;
        /// <summary>The wait a spell slot cast in the breath owes, to start when the breath ends; zero for none.</summary>
        public float Owed(int slot) => Valid(slot) ? owed[slot] : 0f;

        /// <summary>How much of the clock is still to run: 1 just cast, 0 ready.</summary>
        public float Fraction(int slot) => Valid(slot) && !Breathing(slot) && left[slot] > 0f && length[slot] > 0f ? Math.Min(1f, left[slot] / length[slot]) : 0f;

        /// <summary>True for the four spell slots (West, North, East, South); false for the strikes.</summary>
        public static bool IsSpell(int slot) => slot >= 0 && slot < StrikeFirst;

        private bool Breathing(int slot) => breathLeft > 0f && IsSpell(slot);

        private static bool Valid(int slot) => slot >= 0 && slot < Count;
    }
}
