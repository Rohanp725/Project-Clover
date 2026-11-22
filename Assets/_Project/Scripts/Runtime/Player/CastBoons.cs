namespace Clover.Player
{
    /// <summary>
    /// Boons held by one cast, not by the player: a number made stronger only for the blows and bursts of the cast that
    /// earned it (One by One's Master: the snap on the poison's beat makes that cast's own bursts half again as hard, and
    /// no other burst in the same moment). A cast is known by its number (<see cref="SpellCast.Cast"/>), which every blow
    /// it deals carries. A few are held at once; a new one takes the place of the oldest.
    /// </summary>
    public sealed class CastBoons
    {
        /// <summary>How many casts' boons are held at once.</summary>
        public const int Most = 4;

        private readonly int[] casts = new int[Most];
        private readonly PlayerStat[] stats = new PlayerStat[Most];
        private readonly float[] amounts = new float[Most];
        private readonly float[] until = new float[Most];
        private int next;

        /// <summary>Gives the cast numbered <paramref name="cast"/> <paramref name="amount"/> more of a stat until <paramref name="endsAt"/>. Nothing for cast 0 (no cast).</summary>
        public void Add(int cast, PlayerStat stat, float amount, float endsAt)
        {
            if (cast == 0 || amount == 0f) return;
            casts[next] = cast;
            stats[next] = stat;
            amounts[next] = amount;
            until[next] = endsAt;
            next = (next + 1) % Most;
        }

        /// <summary>What the stat is multiplied by for a blow of the cast numbered <paramref name="cast"/> at <paramref name="now"/>: 1 when it holds none.</summary>
        public float Multiplier(int cast, PlayerStat stat, float now)
        {
            if (cast == 0) return 1f;
            float sum = 0f;
            for (int i = 0; i < Most; i++)
                if (casts[i] == cast && stats[i] == stat && now < until[i]) sum += amounts[i];
            return System.Math.Max(0f, 1f + sum);
        }

        /// <summary>Forgets every boon (a new scene).</summary>
        public void Clear()
        {
            for (int i = 0; i < Most; i++) casts[i] = 0;
            next = 0;
        }
    }
}
