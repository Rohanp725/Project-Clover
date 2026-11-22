namespace Clover.Player
{
    /// <summary>What adds a ring to a gathered spell (SpellSpec.RingsBy).</summary>
    public enum RingSource
    {
        /// <summary>Each weapon blow landed (Daybreak).</summary>
        Blows,
        /// <summary>Each blow perfectly guarded or perfectly dodged (Bag of Winds: the attacker's wind caught).</summary>
        Turned,
        /// <summary>Each blow a spell's own wall stopped (Red Before the Sun: the shield of dawn), gathered into the Learn's spell.</summary>
        Stopped,
    }

    /// <summary>
    /// A spell begun and then gathered by the blows landed after it, a ring a blow up to its most, and loosed with all it
    /// gathered by a second press, or by itself when its time is up (the ledger's gathered-rings: Daybreak; and the
    /// bag-of-winds: Bag of Winds, a wind for each blow turned, kept for Kept Wind's time and lost at its end). The spell's own
    /// numbers are written for <see cref="SpellSpec.RingsFor"/> rings, and are scaled by the rings gathered. Plain C#: the
    /// player's upgrades begin it, add the rings and loose it.
    /// </summary>
    public sealed class RingGather
    {
        /// <summary>The mark whose spell is gathering; null when none is.</summary>
        public Upgrade Mark { get; private set; }
        /// <summary>The rings gathered so far, and the most it takes.</summary>
        public int Rings { get; private set; }
        public int Most { get; private set; }
        /// <summary>When it is loosed by itself, and the slot cast's number its blows carry.</summary>
        public float Until { get; private set; }
        public int Tag { get; private set; }
        /// <summary>The spell's numbers as it was begun (the shape and Empower worn then), before the rings scale them.</summary>
        public SpellSpec Spec { get; } = new SpellSpec();

        public bool Active => Mark != null;
        /// <summary>The form it is loosed through when not its branch's worn one (Red Before the Sun: the Learn's beam); null for that.</summary>
        public string Form { get; private set; }
        /// <summary>What adds its rings, and whether its time's end loses them rather than loosing them.</summary>
        public RingSource By => Spec.RingsBy;
        public bool Lapses => Spec.RingsLapse;
        // When each ring was added, for "three within three seconds".
        private readonly float[] addedAt = new float[16];

        /// <summary>Begins gathering <paramref name="mark"/>'s spell as <paramref name="spec"/> has it, loosed by itself at <paramref name="until"/>.</summary>
        public void Begin(Upgrade mark, SpellSpec spec, int tag, float until, string form = null)
        {
            Mark = mark;
            Form = string.IsNullOrEmpty(form) ? null : form;
            SpellChange.Copy(spec, Spec);
            Most = System.Math.Max(1, spec.Rings);
            Rings = 0;
            Tag = tag;
            Until = until;
        }

        /// <summary>A blow landed: one more ring, up to the most. False when nothing is gathering or it is full.</summary>
        public bool Add() => Add(0f);

        /// <summary>As above, the ring added at <paramref name="now"/>.</summary>
        public bool Add(float now)
        {
            if (!Active || Rings >= Most) return false;
            if (Rings < addedAt.Length) addedAt[Rings] = now;
            Rings++;
            return true;
        }

        /// <summary>True when it holds at least <paramref name="count"/> rings and the last that many came within <paramref name="seconds"/> before <paramref name="now"/>.</summary>
        public bool LastWithin(int count, float seconds, float now)
        {
            if (!Active || count <= 0 || Rings < count) return false;
            int first = Rings - count;
            return first < addedAt.Length && now - addedAt[first] <= seconds;
        }

        /// <summary>True when its time is up and it is loosed by itself.</summary>
        public bool Due(float now) => Active && now >= Until;

        /// <summary>
        /// Ends the gathering and writes the numbers it is loosed with into <paramref name="into"/>: damage and stagger scaled by
        /// the rings gathered over the rings its numbers are written for. False when nothing was gathering.
        /// </summary>
        public bool Loose(SpellSpec into)
        {
            if (!Active || into == null) return false;
            SpellChange.Copy(Spec, into);
            float scale = Scale(Rings, Spec.RingsFor);
            into.Damage = Spec.Damage * scale;
            into.Stagger = Spec.Stagger * scale;
            into.Rings = 0;
            End();
            return true;
        }

        /// <summary>Forgets it, loosing nothing (a new scene, death).</summary>
        public void End()
        {
            Mark = null;
            Form = null;
            Rings = 0;
        }

        /// <summary>What the numbers written for <paramref name="writtenFor"/> rings are multiplied by with <paramref name="rings"/> gathered.</summary>
        public static float Scale(int rings, int writtenFor) => writtenFor <= 0 ? 1f : System.Math.Max(0, rings) / (float)writtenFor;
    }
}
