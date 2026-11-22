namespace Clover.Gameplay
{
    /// <summary>When a push on an enemy is a shove its conditions answer (the ledger's shove-moment), apart from the body so it can be checked without a scene.</summary>
    public static class ShoveRules
    {
        /// <summary>The least push, in metres of impulse, that counts as a shove: a pull's steady drag each step is far below it.</summary>
        public const float MomentAt = 0.75f;
        /// <summary>Seconds within which a second shove on the same enemy is the same moment.</summary>
        public const float Apart = 0.25f;

        /// <summary>True when a push this strong, now, is a new shove moment after the last one at <paramref name="last"/>.</summary>
        public static bool IsMoment(float impulse, float now, float last) => IsMoment(PushKind.Blow, impulse, now, last);

        /// <summary>
        /// True when a push of this kind is a new shove moment: a drag never is, a spread push's first touch always is (its
        /// step is small however hard the whole push), and a blow's push is when it is strong enough.
        /// </summary>
        public static bool IsMoment(PushKind kind, float impulse, float now, float last)
        {
            if (kind == PushKind.Drag || now - last < Apart) return false;
            return kind == PushKind.Shove || impulse >= MomentAt;
        }
    }
}
