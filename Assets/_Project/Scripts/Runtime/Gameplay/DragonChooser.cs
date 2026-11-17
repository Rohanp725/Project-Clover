using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Decides what the storm dragon does next. Like the chooser of the colossus it is not pure chance: the fight
    /// opens with a set pattern that shows the mouth, the tail and the magic; a player who stays within reach of the
    /// head is bitten; nothing is done twice running; a spell or a tail attack is always followed by something from
    /// the mouth, so the mouth is most of the fight; and after two heavy attacks a lull is owed. It knows nothing of
    /// Unity, so tests can run it.
    /// </summary>
    [Serializable]
    public class DragonChooser
    {
        /// <summary>What it does first, in order, once it has risen. The bite is never part of it: that one is an answer.</summary>
        public DragonAttack[] Opening = { DragonAttack.Beam, DragonAttack.TailSlam, DragonAttack.Breath, DragonAttack.Bolts, DragonAttack.Beam, DragonAttack.TailSweep };
        /// <summary>The shares of each attack when where the player stands does not decide it.</summary>
        public float BeamShare = 0.2f;
        public float BreathShare = 0.14f;
        public float BiteShare = 0.12f;
        public float TailSlamShare = 0.14f;
        public float TailSweepShare = 0.1f;
        public float BoltsShare = 0.1f;
        public float SigilsShare = 0.08f;
        public float OrbsShare = 0.07f;
        public float StormShare = 0.05f;
        /// <summary>How many heavy attacks earn the player a lull.</summary>
        public int HeavyBeforeLull = 2;

        /// <summary>True when the rest before the next attack should be the long one. Read it with <see cref="TakeLull"/>.</summary>
        public bool LullOwed => heavySinceLull >= HeavyBeforeLull;
        public DragonAttack Last => last;
        public int Chosen { get; private set; }

        private DragonAttack last = DragonAttack.None;
        private int heavySinceLull;

        // The order the others are tried in when the one that came up is not allowed: the mouth first.
        private static readonly DragonAttack[] Fallbacks =
        {
            DragonAttack.Beam, DragonAttack.Breath, DragonAttack.Bite, DragonAttack.TailSlam, DragonAttack.Bolts,
            DragonAttack.TailSweep, DragonAttack.Sigils, DragonAttack.Orbs, DragonAttack.Storm,
        };

        public void Reset()
        {
            last = DragonAttack.None;
            heavySinceLull = 0;
            Chosen = 0;
        }

        public static bool IsMouth(DragonAttack attack) =>
            attack == DragonAttack.Beam || attack == DragonAttack.Breath || attack == DragonAttack.Bite;

        public static bool IsTail(DragonAttack attack) => attack == DragonAttack.TailSweep || attack == DragonAttack.TailSlam;

        public static bool IsMagic(DragonAttack attack) =>
            attack == DragonAttack.Bolts || attack == DragonAttack.Sigils || attack == DragonAttack.Orbs || attack == DragonAttack.Storm;

        /// <summary>An attack that asks the player to move or to guard hard, with no easy answer.</summary>
        public static bool IsHeavy(DragonAttack attack) =>
            attack == DragonAttack.Beam || attack == DragonAttack.Bite || attack == DragonAttack.TailSweep || attack == DragonAttack.Sigils;

        /// <summary>
        /// The next attack. <paramref name="roll"/> is chance, from 0 to 1. <paramref name="lingeredInReach"/> is true
        /// when the player has stood within reach of the head long enough to be bitten for it;
        /// <paramref name="inReach"/> when they stand there at all (out of it, the bite is never chosen);
        /// <paramref name="stormOn"/> while quarters of the floor still crackle from the last storm.
        /// </summary>
        public DragonAttack Next(float roll, bool lingeredInReach, bool inReach, bool stormOn)
        {
            DragonAttack pick;
            if (Chosen < Opening.Length) pick = Opening[Chosen];
            else if (lingeredInReach && last != DragonAttack.Bite) pick = DragonAttack.Bite;
            else pick = ByShare(roll);

            if (!Allowed(pick, inReach, stormOn))
            {
                foreach (var other in Fallbacks)
                {
                    if (!Allowed(other, inReach, stormOn)) continue;
                    pick = other;
                    break;
                }
            }
            Record(pick);
            return pick;
        }

        /// <summary>Writes down an attack decided elsewhere (the storm that opens the fury), so the rules count it.</summary>
        public void Record(DragonAttack attack)
        {
            last = attack;
            Chosen++;
            if (IsHeavy(attack)) heavySinceLull++;
        }

        /// <summary>True once when a lull is owed: the caller rests longer, and the count begins again.</summary>
        public bool TakeLull()
        {
            if (!LullOwed) return false;
            heavySinceLull = 0;
            return true;
        }

        private DragonAttack ByShare(float roll)
        {
            float total = BeamShare + BreathShare + BiteShare + TailSlamShare + TailSweepShare + BoltsShare + SigilsShare + OrbsShare + StormShare;
            float at = Math.Max(0f, Math.Min(1f, roll)) * total;
            if ((at -= BeamShare) < 0f) return DragonAttack.Beam;
            if ((at -= BreathShare) < 0f) return DragonAttack.Breath;
            if ((at -= BiteShare) < 0f) return DragonAttack.Bite;
            if ((at -= TailSlamShare) < 0f) return DragonAttack.TailSlam;
            if ((at -= TailSweepShare) < 0f) return DragonAttack.TailSweep;
            if ((at -= BoltsShare) < 0f) return DragonAttack.Bolts;
            if ((at -= SigilsShare) < 0f) return DragonAttack.Sigils;
            if ((at -= OrbsShare) < 0f) return DragonAttack.Orbs;
            return DragonAttack.Storm;
        }

        private bool Allowed(DragonAttack attack, bool inReach, bool stormOn)
        {
            // Nothing twice running.
            if (attack == last) return false;
            // After the tail or a spell, the mouth: it is a dragon, and its head is where it is hurt.
            if (!IsMouth(attack) && last != DragonAttack.None && !IsMouth(last)) return false;
            // The neck is only so long.
            if (attack == DragonAttack.Bite && !inReach) return false;
            if (attack == DragonAttack.Storm && stormOn) return false;
            return true;
        }
    }
}
