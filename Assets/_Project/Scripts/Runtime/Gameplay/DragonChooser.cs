using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Decides what the storm dragon does next. It is not pure chance. The fight opens with a set pattern that shows
    /// every attack of the mouth, the tail and the magic but the bite and the storm; a player who stays within reach
    /// of the head is bitten; nothing is done twice running or again with only one thing between, and nor is the same
    /// kind of thing (mouth, tail, magic) done twice running, so the three share the fight; a kind that has not come for three turns comes next; the tail's moves are dealt
    /// round, none a second time until at least four of the others have come since; and after two heavy attacks a lull is owed.
    /// It knows nothing of Unity, so tests can run it.
    /// </summary>
    [Serializable]
    public class DragonChooser
    {
        /// <summary>What it does first, in order, once it has risen. The bite is never part of it: that one is an answer. Mouth, tail and magic take turns.</summary>
        public DragonAttack[] Opening =
        {
            DragonAttack.Beam, DragonAttack.TailSlam, DragonAttack.Bolts, DragonAttack.TailSweep, DragonAttack.Breath, DragonAttack.TailSting,
            DragonAttack.Orbs, DragonAttack.TailScythe, DragonAttack.Beam, DragonAttack.TailCoil, DragonAttack.Sigils, DragonAttack.TailFlops,
        };
        /// <summary>The shares of each attack when where the player stands does not decide it. The tail's is shared out among its moves.</summary>
        public float BeamShare = 0.15f;
        public float BreathShare = 0.12f;
        public float BiteShare = 0.09f;
        public float TailShare = 0.34f;
        public float BoltsShare = 0.1f;
        public float SigilsShare = 0.08f;
        public float OrbsShare = 0.07f;
        public float StormShare = 0.05f;
        /// <summary>How many heavy attacks earn the player a lull.</summary>
        public int HeavyBeforeLull = 2;
        /// <summary>A kind of attack that has stayed away this many turns is the next one.</summary>
        public int KindOwedAfter = 3;

        /// <summary>True when the rest before the next attack should be the long one. Read it with <see cref="TakeLull"/>.</summary>
        public bool LullOwed => heavySinceLull >= HeavyBeforeLull;
        public DragonAttack Last => last;
        public int Chosen { get; private set; }

        private DragonAttack last = DragonAttack.None;
        private DragonAttack before = DragonAttack.None;
        private int heavySinceLull;
        private int tailTurns;

        // Everything it can choose, the storm last so that the highest roll is the storm when the storm is allowed.
        private static readonly DragonAttack[] All =
        {
            DragonAttack.Beam, DragonAttack.Breath, DragonAttack.Bite,
            DragonAttack.TailSlam, DragonAttack.TailSweep, DragonAttack.TailCoil, DragonAttack.TailScythe, DragonAttack.TailFlops, DragonAttack.TailSting,
            DragonAttack.Bolts, DragonAttack.Sigils, DragonAttack.Orbs, DragonAttack.Storm,
        };
        private const int Mouth = 0, Tail = 1, Magic = 2;

        [NonSerialized] private float[] weights;
        [NonSerialized] private int[] uses;
        [NonSerialized] private int[] since;

        public void Reset()
        {
            last = DragonAttack.None;
            before = DragonAttack.None;
            heavySinceLull = 0;
            tailTurns = 0;
            Chosen = 0;
            Ready();
            Array.Clear(uses, 0, uses.Length);
            Array.Clear(since, 0, since.Length);
        }

        private void Ready()
        {
            if (weights != null) return;
            weights = new float[All.Length];
            uses = new int[All.Length];
            since = new int[3];
        }

        public static bool IsMouth(DragonAttack attack) =>
            attack == DragonAttack.Beam || attack == DragonAttack.Breath || attack == DragonAttack.Bite;

        public static bool IsTail(DragonAttack attack) =>
            attack == DragonAttack.TailSweep || attack == DragonAttack.TailSlam || attack == DragonAttack.TailCoil
            || attack == DragonAttack.TailScythe || attack == DragonAttack.TailFlops || attack == DragonAttack.TailSting;

        public static bool IsMagic(DragonAttack attack) =>
            attack == DragonAttack.Bolts || attack == DragonAttack.Sigils || attack == DragonAttack.Orbs || attack == DragonAttack.Storm;

        /// <summary>An attack that asks the player to move or to guard hard, with no easy answer.</summary>
        public static bool IsHeavy(DragonAttack attack) =>
            attack == DragonAttack.Beam || attack == DragonAttack.Bite || attack == DragonAttack.TailSweep || attack == DragonAttack.Sigils
            || attack == DragonAttack.TailScythe || attack == DragonAttack.TailCoil;

        private static int Kind(DragonAttack attack) => IsMouth(attack) ? Mouth : IsTail(attack) ? Tail : Magic;

        /// <summary>
        /// The next attack. <paramref name="roll"/> is chance, from 0 to 1. <paramref name="lingeredInReach"/> is true
        /// when the player has stood within reach of the head long enough to be bitten for it;
        /// <paramref name="inReach"/> when they stand there at all (out of it, the bite is never chosen);
        /// <paramref name="stormOn"/> while quarters of the floor still crackle from the last storm.
        /// </summary>
        public DragonAttack Next(float roll, bool lingeredInReach, bool inReach, bool stormOn)
        {
            Ready();
            DragonAttack pick = DragonAttack.None;
            if (Chosen < Opening.Length)
            {
                if (Opening[Chosen] != last && Possible(Opening[Chosen], inReach, stormOn)) pick = Opening[Chosen];
            }
            else if (lingeredInReach && inReach && last != DragonAttack.Bite) pick = DragonAttack.Bite;
            if (pick == DragonAttack.None) pick = ByShare(roll, inReach, stormOn);
            Record(pick);
            return pick;
        }

        /// <summary>Writes down an attack decided elsewhere (the storm that opens the fury), so the rules count it.</summary>
        public void Record(DragonAttack attack)
        {
            Ready();
            before = last;
            last = attack;
            Chosen++;
            if (IsHeavy(attack)) heavySinceLull++;
            for (int i = 0; i < since.Length; i++) since[i]++;
            since[Kind(attack)] = 0;
            // For a move of the tail, which of the tail's turns it last came on.
            int index = Array.IndexOf(All, attack);
            if (index >= 0 && IsTail(attack)) uses[index] = ++tailTurns;
        }

        /// <summary>True once when a lull is owed: the caller rests longer, and the count begins again.</summary>
        public bool TakeLull()
        {
            if (!LullOwed) return false;
            heavySinceLull = 0;
            return true;
        }

        private float Share(DragonAttack attack)
        {
            switch (attack)
            {
                case DragonAttack.Beam: return BeamShare;
                case DragonAttack.Breath: return BreathShare;
                case DragonAttack.Bite: return BiteShare;
                case DragonAttack.Bolts: return BoltsShare;
                case DragonAttack.Sigils: return SigilsShare;
                case DragonAttack.Orbs: return OrbsShare;
                case DragonAttack.Storm: return StormShare;
                default: return TailShare;
            }
        }

        private static bool Possible(DragonAttack attack, bool inReach, bool stormOn)
        {
            // The neck is only so long.
            if (attack == DragonAttack.Bite && !inReach) return false;
            if (attack == DragonAttack.Storm && stormOn) return false;
            return true;
        }

        private DragonAttack ByShare(float roll, bool inReach, bool stormOn)
        {
            // The kind that has stayed away longest, when it has stayed away long enough, is owed.
            int owed = -1;
            for (int kind = 0; kind < since.Length; kind++)
                if (since[kind] >= KindOwedAfter && (owed < 0 || since[kind] > since[owed])) owed = kind;
            // The tail's moves are dealt round: only the two that came longest ago may come now, and the one that
            // came longest ago must once five others have come since, so any six of the tail's turns hold all six.
            int oldest = int.MaxValue, second = int.MaxValue;
            for (int i = 0; i < All.Length; i++)
            {
                if (!IsTail(All[i]) || All[i] == last) continue;
                if (uses[i] < oldest)
                {
                    second = oldest;
                    oldest = uses[i];
                }
                else if (uses[i] < second) second = uses[i];
            }
            int fewest = tailTurns - oldest >= 5 ? oldest : second;

            // First by every rule; then without what is owed; then without the turn-taking of kinds, so something always comes.
            for (int attempt = 0; attempt < 3; attempt++)
            {
                float total = 0f;
                int tails = 0;
                for (int i = 0; i < All.Length; i++)
                {
                    var attack = All[i];
                    int kind = Kind(attack);
                    bool fits = attack != last && Possible(attack, inReach, stormOn)
                        && (attempt >= 2 || last == DragonAttack.None || kind != Kind(last))
                        && (attempt >= 1 || owed < 0 || kind == owed)
                        // Nor one thing, another, and the first again: the beam would otherwise be every other attack.
                        && (attempt >= 1 || attack != before)
                        && (kind != Tail || uses[i] <= fewest);
                    weights[i] = fits ? Share(attack) : 0f;
                    if (fits && kind == Tail) tails++;
                }
                for (int i = 0; i < All.Length; i++)
                {
                    if (tails > 1 && weights[i] > 0f && IsTail(All[i])) weights[i] /= tails;
                    total += weights[i];
                }
                if (total <= 0f) continue;
                float at = Math.Max(0f, Math.Min(1f, roll)) * total;
                int pick = -1;
                for (int i = 0; i < All.Length; i++)
                {
                    if (weights[i] <= 0f) continue;
                    pick = i;
                    if ((at -= weights[i]) < 0f) break;
                }
                return All[pick];
            }
            return last == DragonAttack.Beam ? DragonAttack.Breath : DragonAttack.Beam;
        }
    }
}
