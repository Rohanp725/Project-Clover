using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// The player's switches over how afflictions behave on every enemy (IMPLEMENTATION.md 3.10): the part of the
    /// player's RuleBook that the enemies' <see cref="AfflictionSet"/>s read, kept in <c>Clover.Gameplay</c> so the sets
    /// need nothing from <c>Clover.Player</c>. One kind can be kept from fading by itself (Fire Without End's burns, Deep Winter's
    /// Chill), deepen by itself when left alone, be kept from being spent by blows (Full Daylight's mark), or rest after it
    /// fades before it can be laid again. Fixed arrays and masks, so reading it on every enemy every step allocates
    /// nothing. <see cref="Afflictions.Rules"/> is the copy in force; the player's RuleBook writes it when it is rebuilt.
    /// </summary>
    public sealed class AfflictionRules
    {
        private int keeps;
        private int unspent;
        private readonly float[] deepensEvery = new float[Afflictions.Count];
        private readonly int[] deepensTo = new int[Afflictions.Count];
        private readonly float[] restsAfter = new float[Afflictions.Count];
        private readonly float[] ripensAfter = new float[Afflictions.Count];
        private readonly float[] ripeShare = new float[Afflictions.Count];
        private readonly float[] unripeShare = new float[Afflictions.Count];
        private readonly int[] keepsWhile = new int[Afflictions.Count];
        private readonly int[] slowerWhile = new int[Afflictions.Count];
        private readonly float[] slowerPace = new float[Afflictions.Count];

        /// <summary>False while every affliction behaves as written: the sets then skip every rule here with one test.</summary>
        public bool Any { get; private set; }

        /// <summary>True when the kind never fades by itself: only being set off, spent, or its bearer's death ends it.</summary>
        public bool Keeps(AfflictionKind kind) => (keeps & Bit(kind)) != 0;

        /// <summary>
        /// True when the kind does not fade while the enemy also carries one of the kinds a rule pairs it with
        /// (Permafrost's Tremor on a chilled enemy); <paramref name="carried"/> is the enemy's mask of kinds carried.
        /// </summary>
        public bool KeepsWith(AfflictionKind kind, int carried) => Valid(kind) && (keepsWhile[(int)kind] & carried) != 0;

        /// <summary>How fast the kind fades on an enemy carrying <paramref name="carried"/>: 1 as written, less while a rule slows it (Brine).</summary>
        public float FadePace(AfflictionKind kind, int carried) => Valid(kind) && (slowerWhile[(int)kind] & carried) != 0 ? slowerPace[(int)kind] : 1f;

        /// <summary>True when the kind is not spent by weapon blows (a mark of light): it lasts its time instead.</summary>
        public bool Unspent(AfflictionKind kind) => (unspent & Bit(kind)) != 0;

        /// <summary>Seconds after which the kind, left alone (nothing new laid), deepens by one stack; zero for never.</summary>
        public float DeepensEvery(AfflictionKind kind) => Valid(kind) ? deepensEvery[(int)kind] : 0f;

        /// <summary>The depth the kind deepens to by itself, at most.</summary>
        public int DeepensTo(AfflictionKind kind) => Valid(kind) ? deepensTo[(int)kind] : 0;

        /// <summary>Seconds after the kind fades from an enemy before it can be laid on that enemy again; zero for none.</summary>
        public float RestsAfter(AfflictionKind kind) => Valid(kind) ? restsAfter[(int)kind] : 0f;

        /// <summary>
        /// Seconds the kind must lie on an enemy with nothing new laid on it before it is ripe (In Its Season's seeds); zero
        /// for a kind that never ripens. A ripe kind stops fading, and its burst is <see cref="RipeShare"/> as strong; an
        /// unripe one's is <see cref="UnripeShare"/> as strong.
        /// </summary>
        public float RipensAfter(AfflictionKind kind) => Valid(kind) ? ripensAfter[(int)kind] : 0f;

        /// <summary>What a ripe kind's burst is multiplied by (1 for a kind that never ripens).</summary>
        public float RipeShare(AfflictionKind kind) => RipensAfter(kind) > 0f ? ripeShare[(int)kind] : 1f;

        /// <summary>What an unripe kind's burst is multiplied by (1 for a kind that never ripens).</summary>
        public float UnripeShare(AfflictionKind kind) => RipensAfter(kind) > 0f ? unripeShare[(int)kind] : 1f;

        /// <summary>
        /// The kind ripens: left alone <paramref name="seconds"/> on an enemy it is ripe and stops fading, and when it bursts
        /// (set off, its bearer's death, a reaction) the burst is <paramref name="ripe"/> as strong, or <paramref name="unripe"/>
        /// as strong before it is ripe.
        /// </summary>
        public void Ripen(AfflictionKind kind, float seconds, float ripe, float unripe)
        {
            if (!Valid(kind) || seconds <= 0f) return;
            ripensAfter[(int)kind] = seconds;
            ripeShare[(int)kind] = ripe;
            unripeShare[(int)kind] = unripe;
            Any = true;
        }

        /// <summary>The kind never fades by itself.</summary>
        public void Keep(AfflictionKind kind)
        {
            if (!Valid(kind)) return;
            keeps |= Bit(kind);
            Any = true;
        }

        /// <summary>The kind does not fade while the enemy also carries <paramref name="with"/>; once that is gone it fades as written.</summary>
        public void KeepWhile(AfflictionKind kind, AfflictionKind with)
        {
            if (!Valid(kind) || !Valid(with) || kind == with) return;
            keepsWhile[(int)kind] |= Bit(with);
            Any = true;
        }

        /// <summary>The kind fades at <paramref name="pace"/> (0.5: lasts twice as long) while the enemy also carries <paramref name="with"/>.</summary>
        public void FadeSlowerWhile(AfflictionKind kind, AfflictionKind with, float pace)
        {
            if (!Valid(kind) || !Valid(with) || kind == with || pace <= 0f || pace >= 1f) return;
            slowerWhile[(int)kind] |= Bit(with);
            slowerPace[(int)kind] = slowerPace[(int)kind] > 0f ? Math.Min(slowerPace[(int)kind], pace) : pace;
            Any = true;
        }

        /// <summary>The kind is not spent by weapon blows.</summary>
        public void NotSpentByBlows(AfflictionKind kind)
        {
            if (!Valid(kind)) return;
            unspent |= Bit(kind);
            Any = true;
        }

        /// <summary>Left alone, the kind deepens by one stack every <paramref name="seconds"/>, up to <paramref name="depth"/>.</summary>
        public void Deepen(AfflictionKind kind, float seconds, int depth)
        {
            if (!Valid(kind) || seconds <= 0f || depth <= 0) return;
            deepensEvery[(int)kind] = seconds;
            deepensTo[(int)kind] = depth;
            Any = true;
        }

        /// <summary>Once the kind has faded from an enemy, it cannot be laid on that enemy again for <paramref name="seconds"/>.</summary>
        public void RestAfterFading(AfflictionKind kind, float seconds)
        {
            if (!Valid(kind) || seconds <= 0f) return;
            restsAfter[(int)kind] = seconds;
            Any = true;
        }

        /// <summary>Every affliction as written again.</summary>
        public void Clear()
        {
            keeps = 0;
            unspent = 0;
            Array.Clear(deepensEvery, 0, deepensEvery.Length);
            Array.Clear(deepensTo, 0, deepensTo.Length);
            Array.Clear(restsAfter, 0, restsAfter.Length);
            Array.Clear(ripensAfter, 0, ripensAfter.Length);
            Array.Clear(ripeShare, 0, ripeShare.Length);
            Array.Clear(unripeShare, 0, unripeShare.Length);
            Array.Clear(keepsWhile, 0, keepsWhile.Length);
            Array.Clear(slowerWhile, 0, slowerWhile.Length);
            Array.Clear(slowerPace, 0, slowerPace.Length);
            Any = false;
        }

        /// <summary>Takes every rule from another set of rules (the RuleBook publishing its own).</summary>
        public void CopyFrom(AfflictionRules other)
        {
            if (other == null || other == this)
            {
                if (other == null) Clear();
                return;
            }
            keeps = other.keeps;
            unspent = other.unspent;
            Array.Copy(other.deepensEvery, deepensEvery, deepensEvery.Length);
            Array.Copy(other.deepensTo, deepensTo, deepensTo.Length);
            Array.Copy(other.restsAfter, restsAfter, restsAfter.Length);
            Array.Copy(other.ripensAfter, ripensAfter, ripensAfter.Length);
            Array.Copy(other.ripeShare, ripeShare, ripeShare.Length);
            Array.Copy(other.unripeShare, unripeShare, unripeShare.Length);
            Array.Copy(other.keepsWhile, keepsWhile, keepsWhile.Length);
            Array.Copy(other.slowerWhile, slowerWhile, slowerWhile.Length);
            Array.Copy(other.slowerPace, slowerPace, slowerPace.Length);
            Any = other.Any;
        }

        private static bool Valid(AfflictionKind kind) => kind != AfflictionKind.None && (int)kind > 0 && (int)kind < Afflictions.Count;

        private static int Bit(AfflictionKind kind) => Valid(kind) ? 1 << (int)kind : 0;
    }
}
