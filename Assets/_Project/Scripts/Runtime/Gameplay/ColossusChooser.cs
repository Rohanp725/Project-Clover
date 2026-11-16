using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Decides what a colossus does next. It is not pure chance: the fight opens with a set pattern that shows the
    /// player each kind of attack; the head answers where the player stands (the bite for one who hugs the rim under
    /// it, the breath for one who stays far off); nothing is done three times running; and after two heavy attacks a
    /// lull is owed, so there is always a moment to breathe and drink. It knows nothing of Unity, so tests can run it.
    /// </summary>
    [Serializable]
    public class ColossusChooser
    {
        /// <summary>What it does first, in order, once it has risen. The bite is never part of it: that one is an answer.</summary>
        public ColossusAttack[] Opening = { ColossusAttack.Slam, ColossusAttack.Sweep, ColossusAttack.Breath, ColossusAttack.Clap };
        /// <summary>The chance, out of one, that it breathes at a player standing far to the south.</summary>
        public float BreathWhenFar = 0.6f;
        /// <summary>The shares of each attack when nothing about where the player stands decides it. They are made to add to one.</summary>
        public float SlamShare = 0.32f;
        public float SweepShare = 0.2f;
        public float ClapShare = 0.18f;
        public float BoltsShare = 0.18f;
        public float BreathShare = 0.12f;
        /// <summary>How many heavy attacks earn the player a lull.</summary>
        public int HeavyBeforeLull = 2;

        /// <summary>True when the rest before the next attack should be the long one. Read it with <see cref="TakeLull"/>.</summary>
        public bool LullOwed => heavySinceLull >= HeavyBeforeLull;
        public ColossusAttack Last => last;
        public int Chosen { get; private set; }

        private ColossusAttack last = ColossusAttack.None;
        private ColossusAttack beforeLast = ColossusAttack.None;
        private int heavySinceLull;

        // The order the others are tried in when the one that came up is not allowed.
        private static readonly ColossusAttack[] Fallbacks =
            { ColossusAttack.Slam, ColossusAttack.Sweep, ColossusAttack.Clap, ColossusAttack.Bolts, ColossusAttack.Breath };

        public void Reset()
        {
            last = ColossusAttack.None;
            beforeLast = ColossusAttack.None;
            heavySinceLull = 0;
            Chosen = 0;
        }

        /// <summary>An attack that asks the player to move or to guard hard, with no easy answer.</summary>
        public static bool IsHeavy(ColossusAttack attack) =>
            attack == ColossusAttack.Sweep || attack == ColossusAttack.Breath || attack == ColossusAttack.Bite || attack == ColossusAttack.Clap;

        /// <summary>
        /// The next attack. <paramref name="roll"/> and <paramref name="secondRoll"/> are chance, from 0 to 1.
        /// <paramref name="underTheHead"/> is true when the player has stood by the north rim long enough to be bitten;
        /// <paramref name="nearTheHead"/> when they stand there at all (the breath has no line on them);
        /// <paramref name="farSouth"/> when they keep the whole floor between them and it;
        /// <paramref name="slamsComeInPairs"/> when each slam chosen is already two (his fury), so even the slam is not chosen twice running.
        /// </summary>
        public ColossusAttack Next(float roll, float secondRoll, bool underTheHead, bool nearTheHead, bool farSouth, bool slamsComeInPairs = false)
        {
            ColossusAttack pick;
            if (Chosen < Opening.Length) pick = Opening[Chosen];
            else if (underTheHead && last != ColossusAttack.Bite) pick = ColossusAttack.Bite;
            else if (farSouth && last != ColossusAttack.Breath && secondRoll < BreathWhenFar) pick = ColossusAttack.Breath;
            else pick = ByShare(roll);

            if (!Allowed(pick, nearTheHead, slamsComeInPairs))
            {
                foreach (var other in Fallbacks)
                {
                    if (!Allowed(other, nearTheHead, slamsComeInPairs)) continue;
                    pick = other;
                    break;
                }
            }
            beforeLast = last;
            last = pick;
            Chosen++;
            if (IsHeavy(pick)) heavySinceLull++;
            return pick;
        }

        /// <summary>True once when a lull is owed: the caller rests longer, and the count begins again.</summary>
        public bool TakeLull()
        {
            if (!LullOwed) return false;
            heavySinceLull = 0;
            return true;
        }

        private ColossusAttack ByShare(float roll)
        {
            float total = SlamShare + SweepShare + ClapShare + BoltsShare + BreathShare;
            float at = Math.Max(0f, Math.Min(1f, roll)) * total;
            if ((at -= SlamShare) < 0f) return ColossusAttack.Slam;
            if ((at -= SweepShare) < 0f) return ColossusAttack.Sweep;
            if ((at -= ClapShare) < 0f) return ColossusAttack.Clap;
            if ((at -= BoltsShare) < 0f) return ColossusAttack.Bolts;
            return ColossusAttack.Breath;
        }

        private bool Allowed(ColossusAttack attack, bool nearTheHead, bool slamsComeInPairs)
        {
            // Never three of anything running; and the ones that take the whole floor, never twice.
            if (attack == last && attack == beforeLast) return false;
            if (attack == last && (attack != ColossusAttack.Slam || slamsComeInPairs)) return false;
            // The breath starts at the rim: on a player standing there it would have no lane to show.
            if (attack == ColossusAttack.Breath && nearTheHead) return false;
            return true;
        }
    }
}
