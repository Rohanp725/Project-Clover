using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Decides what the Sun-Bearer does next (fire_land.md, section 6.4). It opens with a set order that shows
    /// everything once. After that it stays in a shape for a few attacks and then changes; nothing is done twice
    /// running; the ram answers a player who stands near the scarab; no new lane is laid while three are hot; the
    /// Zenith never comes within 25 seconds of the last; and two heavy attacks earn a lull. It knows nothing of
    /// Unity, so tests can run it.
    /// </summary>
    [Serializable]
    public class SunBearerChooser
    {
        public SunBearerAttack[] Opening =
        {
            SunBearerAttack.Roll, SunBearerAttack.Ram, SunBearerAttack.Sparks, SunBearerAttack.Furrows, SunBearerAttack.Rising,
            SunBearerAttack.Stoop, SunBearerAttack.Feathers, SunBearerAttack.Pass, SunBearerAttack.Setting, SunBearerAttack.Roll,
            SunBearerAttack.Rising, SunBearerAttack.Zenith,
        };
        public float RollShare = 30f, FurrowsShare = 20f, SparksShare = 25f;
        public float StoopShare = 35f, PassShare = 25f, FeathersShare = 25f, ZenithShare = 15f;
        public int HeavyBeforeLull = 2;
        /// <summary>The most lanes of hot floor there may be before another is laid.</summary>
        public int MostHotLanes = 3;
        public float ZenithEvery = 25f;

        public SunBearerAttack Last { get; private set; }
        public int Chosen { get; private set; }
        /// <summary>How many attacks it has made in the shape it is in.</summary>
        public int InShape { get; private set; }
        public bool LullOwed => heavySinceLull >= HeavyBeforeLull;

        private int heavySinceLull;
        private int stayFor = 3;

        public void Reset()
        {
            Last = SunBearerAttack.None;
            Chosen = 0;
            InShape = 0;
            heavySinceLull = 0;
            stayFor = 3;
        }

        public static bool IsScarabs(SunBearerAttack attack) => attack >= SunBearerAttack.Roll && attack <= SunBearerAttack.Rising;
        public static bool IsFalcons(SunBearerAttack attack) => attack >= SunBearerAttack.Stoop && attack <= SunBearerAttack.Setting;
        public static bool IsChange(SunBearerAttack attack) => attack == SunBearerAttack.Rising || attack == SunBearerAttack.Setting;
        public static bool LaysLane(SunBearerAttack attack) => attack == SunBearerAttack.Roll || attack == SunBearerAttack.Furrows || attack == SunBearerAttack.Pass;
        public static bool IsHeavy(SunBearerAttack attack) =>
            attack == SunBearerAttack.Roll || attack == SunBearerAttack.Furrows || attack == SunBearerAttack.Stoop || attack == SunBearerAttack.Pass || attack == SunBearerAttack.Zenith;

        /// <summary>
        /// The next thing it does. <paramref name="roll"/> is chance, from 0 to 1; <paramref name="falcon"/> the shape
        /// it is in; <paramref name="lingeredNear"/> true when the player has stood near the scarab long enough to
        /// be rammed for it; <paramref name="hotLanes"/> how many lanes of floor still burn; and
        /// <paramref name="sinceZenith"/> the seconds since the last blaze ended.
        /// </summary>
        public SunBearerAttack Next(float roll, bool falcon, bool lingeredNear, int hotLanes, float sinceZenith)
        {
            SunBearerAttack pick = SunBearerAttack.None;
            if (Chosen < Opening.Length && Fits(Opening[Chosen], falcon, hotLanes, sinceZenith, true)) pick = Opening[Chosen];
            else if (!falcon && lingeredNear && Last != SunBearerAttack.Ram) pick = SunBearerAttack.Ram;
            else if (InShape >= stayFor) pick = falcon ? SunBearerAttack.Setting : SunBearerAttack.Rising;
            if (pick == SunBearerAttack.None) pick = ByShare(roll, falcon, hotLanes, sinceZenith);
            Record(pick, roll);
            return pick;
        }

        /// <summary>Writes down something decided elsewhere (the Zenith that opens the fury), so the rules count it.</summary>
        public void Record(SunBearerAttack attack, float roll = 0.5f)
        {
            Last = attack;
            Chosen++;
            if (IsHeavy(attack)) heavySinceLull++;
            if (IsChange(attack))
            {
                InShape = 0;
                // Three or four as the scarab; two or three as the falcon, which it becomes on a Rising.
                stayFor = attack == SunBearerAttack.Rising ? (roll < 0.5f ? 2 : 3) : (roll < 0.5f ? 3 : 4);
            }
            else InShape++;
        }

        /// <summary>True once when a lull is owed: the caller rests longer, and the count begins again.</summary>
        public bool TakeLull()
        {
            if (!LullOwed) return false;
            heavySinceLull = 0;
            return true;
        }

        private bool Fits(SunBearerAttack attack, bool falcon, int hotLanes, float sinceZenith, bool opening)
        {
            if (attack == Last) return false;
            if (falcon ? !IsFalcons(attack) : !IsScarabs(attack)) return false;
            if (LaysLane(attack) && hotLanes >= MostHotLanes) return false;
            // The opening's own Zenith is the first there has been.
            if (attack == SunBearerAttack.Zenith && !opening && sinceZenith < ZenithEvery) return false;
            return true;
        }

        private float Share(SunBearerAttack attack)
        {
            switch (attack)
            {
                case SunBearerAttack.Roll: return RollShare;
                case SunBearerAttack.Furrows: return FurrowsShare;
                case SunBearerAttack.Sparks: return SparksShare;
                case SunBearerAttack.Stoop: return StoopShare;
                case SunBearerAttack.Pass: return PassShare;
                case SunBearerAttack.Feathers: return FeathersShare;
                case SunBearerAttack.Zenith: return ZenithShare;
                default: return 0f;
            }
        }

        private SunBearerAttack ByShare(float roll, bool falcon, int hotLanes, float sinceZenith)
        {
            float total = 0f;
            for (var attack = SunBearerAttack.Roll; attack <= SunBearerAttack.Setting; attack++)
                if (Fits(attack, falcon, hotLanes, sinceZenith, false)) total += Share(attack);
            // Nothing fits (every lane is hot and the rest was just done): change shape, which is always allowed.
            if (total <= 0f) return falcon ? SunBearerAttack.Setting : SunBearerAttack.Rising;
            float at = Math.Min(Math.Max(roll, 0f), 0.9999f) * total;
            var fallback = SunBearerAttack.None;
            for (var attack = SunBearerAttack.Roll; attack <= SunBearerAttack.Setting; attack++)
            {
                if (!Fits(attack, falcon, hotLanes, sinceZenith, false)) continue;
                float share = Share(attack);
                if (share <= 0f) continue;
                fallback = attack;
                if (at < share) return attack;
                at -= share;
            }
            return fallback;
        }
    }
}
