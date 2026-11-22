namespace Clover.Player
{
    /// <summary>The moments a spell may be cast just after, for a Master that asks for it (the ledger's spell-windows).</summary>
    public enum CastMoment
    {
        None,
        /// <summary>A finisher landed (Chain Lance's Master: within half a second after it).</summary>
        Finisher,
        PerfectDodge,
        /// <summary>A perfect guard turned a blow (Poured Bronze's Master: within a second of it).</summary>
        PerfectGuard,
        /// <summary>A blow landed on the player.</summary>
        Struck,
        /// <summary>A frozen enemy shattered (Hoarfrost's Master: within half a second after it).</summary>
        Shatter,
        /// <summary>An enemy died carrying the affliction the effect names in CastAfterCarrying (Reap What Is Sown's Master: a Seeded one).</summary>
        Death,
        /// <summary>The last of Waiting Thunder's bolts was sent down by a light blow (its Master: a heavy blow within a second of it).</summary>
        LastBolt,
        /// <summary>Riptide's wave came all the way in and broke at the caster's feet (its Master: pressed again within a second of it).</summary>
        WaveIn,
    }

    /// <summary>
    /// When each moment last came, so a cast can be asked whether it came within a moment's window. Deaths are kept for each
    /// affliction the dead carried. Plain C#: the player's upgrades note the moments and ask as a cast begins.
    /// </summary>
    public sealed class CastMoments
    {
        private const float Never = -1000f;
        private static readonly int MomentCount = System.Enum.GetValues(typeof(CastMoment)).Length;
        private static readonly int KindCount = System.Enum.GetValues(typeof(Clover.Gameplay.AfflictionKind)).Length;
        private readonly float[] at = new float[MomentCount];
        private readonly float[] deathCarrying = new float[KindCount];

        public CastMoments() => Clear();

        /// <summary>Forgets every moment.</summary>
        public void Clear()
        {
            for (int i = 0; i < at.Length; i++) at[i] = Never;
            for (int i = 0; i < deathCarrying.Length; i++) deathCarrying[i] = Never;
        }

        /// <summary>The moment came at <paramref name="now"/>.</summary>
        public void Note(CastMoment moment, float now)
        {
            if (moment != CastMoment.None) at[(int)moment] = now;
        }

        /// <summary>An enemy died at <paramref name="now"/> carrying <paramref name="kind"/>.</summary>
        public void NoteDeathCarrying(Clover.Gameplay.AfflictionKind kind, float now)
        {
            at[(int)CastMoment.Death] = now;
            if (kind != Clover.Gameplay.AfflictionKind.None) deathCarrying[(int)kind] = now;
        }

        /// <summary>When the moment last came (for a death, one carrying <paramref name="carrying"/> when it names one).</summary>
        public float LastAt(CastMoment moment, Clover.Gameplay.AfflictionKind carrying = Clover.Gameplay.AfflictionKind.None) =>
            moment == CastMoment.Death && carrying != Clover.Gameplay.AfflictionKind.None ? deathCarrying[(int)carrying] : at[(int)moment];

        /// <summary>True when a cast begun at <paramref name="castAt"/> came within <paramref name="window"/> seconds after the moment.</summary>
        public bool Within(CastMoment moment, float window, float castAt, Clover.Gameplay.AfflictionKind carrying = Clover.Gameplay.AfflictionKind.None)
        {
            if (moment == CastMoment.None) return true;
            float last = LastAt(moment, carrying);
            return castAt >= last && castAt - last <= window;
        }
    }
}
