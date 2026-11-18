using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Decides what the Sun-Bearer does next (fire_land.md, section 6.4, as reworked on 2026-11-18). It opens with a set
    /// order that shows everything once. After that it stays in a shape for a few attacks and then changes; nothing is
    /// done twice running, and what it did lately is less likely to come round again; a few attacks lead into a set
    /// partner (the Ricochet into the Ram, the Ember Gust into the Stoop, the Burrow into the Flares), and in its fury
    /// they always do, with two more pairs of their own; the ram answers a player who stands near the scarab; no new lane
    /// is laid while three are hot; the Zenith never comes within 25 seconds of the last; and two heavy attacks earn a
    /// lull. It knows nothing of Unity, so tests can run it.
    /// </summary>
    [Serializable]
    public class SunBearerChooser
    {
        public SunBearerAttack[] Opening =
        {
            SunBearerAttack.Roll, SunBearerAttack.Ram, SunBearerAttack.Sparks, SunBearerAttack.Ricochet, SunBearerAttack.Rising,
            SunBearerAttack.Stoop, SunBearerAttack.Gust, SunBearerAttack.Feathers, SunBearerAttack.Setting, SunBearerAttack.Burrow,
            SunBearerAttack.Flares, SunBearerAttack.Furrows, SunBearerAttack.Rising, SunBearerAttack.Grab, SunBearerAttack.Pass,
            SunBearerAttack.Sunfall, SunBearerAttack.Setting, SunBearerAttack.Roll, SunBearerAttack.Rising, SunBearerAttack.Zenith,
        };
        public float RollShare = 20f, FurrowsShare = 14f, SparksShare = 14f, RicochetShare = 18f, BurrowShare = 16f, FlaresShare = 16f;
        public float StoopShare = 22f, PassShare = 16f, FeathersShare = 12f, GustShare = 16f, GrabShare = 14f, SunfallShare = 18f, ZenithShare = 14f;
        /// <summary>How likely a pair is to follow on once its first half is done, outside the fury (in it, always).</summary>
        public float ChainChance = 0.6f;
        /// <summary>What weighs against a repeat: the share kept by an attack done two or three picks ago, and four ago.</summary>
        public float RecentShare = 0.25f, OlderShare = 0.6f;
        public int HeavyBeforeLull = 2;
        /// <summary>The most lanes of hot floor there may be before another is laid.</summary>
        public int MostHotLanes = 3;
        public float ZenithEvery = 25f;
        /// <summary>True below half health: the pairs always follow, and two more pairs are made.</summary>
        [NonSerialized] public bool Fury;

        public SunBearerAttack Last { get; private set; }
        public int Chosen { get; private set; }
        /// <summary>How many attacks it has made in the shape it is in.</summary>
        public int InShape { get; private set; }
        public bool LullOwed => heavySinceLull >= HeavyBeforeLull;
        /// <summary>True when the last pick was the second half of a pair.</summary>
        public bool LastWasChained { get; private set; }

        private int heavySinceLull;
        private int stayFor = 3;
        // The last four attacks made (changes of shape are not counted), the newest first.
        private readonly SunBearerAttack[] recent = new SunBearerAttack[4];

        public void Reset()
        {
            Last = SunBearerAttack.None;
            Chosen = 0;
            InShape = 0;
            heavySinceLull = 0;
            stayFor = 3;
            LastWasChained = false;
            for (int i = 0; i < recent.Length; i++) recent[i] = SunBearerAttack.None;
        }

        public static bool IsScarabs(SunBearerAttack attack)
        {
            switch (attack)
            {
                case SunBearerAttack.Roll:
                case SunBearerAttack.Furrows:
                case SunBearerAttack.Ram:
                case SunBearerAttack.Sparks:
                case SunBearerAttack.Rising:
                case SunBearerAttack.Ricochet:
                case SunBearerAttack.Burrow:
                case SunBearerAttack.Flares:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFalcons(SunBearerAttack attack)
        {
            switch (attack)
            {
                case SunBearerAttack.Stoop:
                case SunBearerAttack.Pass:
                case SunBearerAttack.Feathers:
                case SunBearerAttack.Zenith:
                case SunBearerAttack.Setting:
                case SunBearerAttack.Gust:
                case SunBearerAttack.Grab:
                case SunBearerAttack.Sunfall:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsChange(SunBearerAttack attack) => attack == SunBearerAttack.Rising || attack == SunBearerAttack.Setting;
        public static bool LaysLane(SunBearerAttack attack) => attack == SunBearerAttack.Roll || attack == SunBearerAttack.Furrows || attack == SunBearerAttack.Pass || attack == SunBearerAttack.Ricochet;
        public static bool IsHeavy(SunBearerAttack attack) =>
            attack == SunBearerAttack.Roll || attack == SunBearerAttack.Furrows || attack == SunBearerAttack.Stoop || attack == SunBearerAttack.Pass || attack == SunBearerAttack.Zenith
            || attack == SunBearerAttack.Ricochet || attack == SunBearerAttack.Burrow || attack == SunBearerAttack.Grab;

        /// <summary>What follows an attack as its pair, if anything: three always, two more in the fury.</summary>
        public static SunBearerAttack PairOf(SunBearerAttack attack, bool fury)
        {
            switch (attack)
            {
                case SunBearerAttack.Ricochet: return SunBearerAttack.Ram;
                case SunBearerAttack.Gust: return SunBearerAttack.Stoop;
                case SunBearerAttack.Burrow: return SunBearerAttack.Flares;
                case SunBearerAttack.Sparks: return fury ? SunBearerAttack.Ricochet : SunBearerAttack.None;
                case SunBearerAttack.Sunfall: return fury ? SunBearerAttack.Grab : SunBearerAttack.None;
                default: return SunBearerAttack.None;
            }
        }

        /// <summary>
        /// The next thing it does. <paramref name="roll"/> is chance, from 0 to 1; <paramref name="falcon"/> the shape
        /// it is in; <paramref name="lingeredNear"/> true when the player has stood near the scarab long enough to
        /// be rammed for it; <paramref name="hotLanes"/> how many lanes of floor still burn; and
        /// <paramref name="sinceZenith"/> the seconds since the last blaze ended.
        /// </summary>
        public SunBearerAttack Next(float roll, bool falcon, bool lingeredNear, int hotLanes, float sinceZenith)
        {
            SunBearerAttack pick = SunBearerAttack.None;
            bool chained = false;
            if (Chosen < Opening.Length && Fits(Opening[Chosen], falcon, hotLanes, sinceZenith, true)) pick = Opening[Chosen];
            if (pick == SunBearerAttack.None && Chosen >= Opening.Length && !LastWasChained && InShape < stayFor)
            {
                // A pair follows on: always in the fury, often otherwise. The chance is read from the far end of the
                // roll so it does not decide alike with the share below.
                var pair = PairOf(Last, Fury);
                if (pair != SunBearerAttack.None && (Fury || 1f - roll < ChainChance) && Fits(pair, falcon, hotLanes, sinceZenith, false))
                {
                    pick = pair;
                    chained = true;
                }
            }
            if (pick == SunBearerAttack.None && !falcon && lingeredNear && Last != SunBearerAttack.Ram && Chosen >= Opening.Length) pick = SunBearerAttack.Ram;
            if (pick == SunBearerAttack.None && InShape >= stayFor) pick = falcon ? SunBearerAttack.Setting : SunBearerAttack.Rising;
            if (pick == SunBearerAttack.None) pick = ByShare(roll, falcon, hotLanes, sinceZenith);
            Record(pick, roll);
            LastWasChained = chained;
            return pick;
        }

        /// <summary>Writes down something decided elsewhere (the Zenith that opens the fury), so the rules count it.</summary>
        public void Record(SunBearerAttack attack, float roll = 0.5f)
        {
            Last = attack;
            Chosen++;
            LastWasChained = false;
            if (IsHeavy(attack)) heavySinceLull++;
            if (IsChange(attack))
            {
                InShape = 0;
                // Three or four as the scarab; two or three as the falcon, which it becomes on a Rising.
                stayFor = attack == SunBearerAttack.Rising ? (roll < 0.5f ? 2 : 3) : (roll < 0.5f ? 3 : 4);
                return;
            }
            InShape++;
            for (int i = recent.Length - 1; i > 0; i--) recent[i] = recent[i - 1];
            recent[0] = attack;
        }

        /// <summary>True once when a lull is owed: the caller rests longer, and the count begins again.</summary>
        public bool TakeLull()
        {
            if (!LullOwed) return false;
            heavySinceLull = 0;
            return true;
        }

        /// <summary>How many of the last four attacks were this one.</summary>
        public int TimesLately(SunBearerAttack attack)
        {
            int times = 0;
            foreach (var done in recent)
                if (done == attack) times++;
            return times;
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
            float share;
            switch (attack)
            {
                case SunBearerAttack.Roll: share = RollShare; break;
                case SunBearerAttack.Furrows: share = FurrowsShare; break;
                case SunBearerAttack.Sparks: share = SparksShare; break;
                case SunBearerAttack.Ricochet: share = RicochetShare; break;
                case SunBearerAttack.Burrow: share = BurrowShare; break;
                case SunBearerAttack.Flares: share = FlaresShare; break;
                case SunBearerAttack.Stoop: share = StoopShare; break;
                case SunBearerAttack.Pass: share = PassShare; break;
                case SunBearerAttack.Feathers: share = FeathersShare; break;
                case SunBearerAttack.Gust: share = GustShare; break;
                case SunBearerAttack.Grab: share = GrabShare; break;
                case SunBearerAttack.Sunfall: share = SunfallShare; break;
                case SunBearerAttack.Zenith: share = ZenithShare; break;
                default: return 0f;
            }
            // Weighed against coming round again soon: the one before last and the one before that count most.
            if (recent[1] == attack || recent[2] == attack) share *= RecentShare;
            else if (recent[3] == attack) share *= OlderShare;
            return share;
        }

        private static readonly SunBearerAttack[] Everything =
        {
            SunBearerAttack.Roll, SunBearerAttack.Furrows, SunBearerAttack.Sparks, SunBearerAttack.Ricochet, SunBearerAttack.Burrow, SunBearerAttack.Flares,
            SunBearerAttack.Stoop, SunBearerAttack.Pass, SunBearerAttack.Feathers, SunBearerAttack.Gust, SunBearerAttack.Grab, SunBearerAttack.Sunfall, SunBearerAttack.Zenith,
        };

        private SunBearerAttack ByShare(float roll, bool falcon, int hotLanes, float sinceZenith)
        {
            float total = 0f;
            foreach (var attack in Everything)
                if (Fits(attack, falcon, hotLanes, sinceZenith, false)) total += Share(attack);
            // Nothing fits (every lane is hot and the rest was just done): change shape, which is always allowed.
            if (total <= 0f) return falcon ? SunBearerAttack.Setting : SunBearerAttack.Rising;
            float at = Math.Min(Math.Max(roll, 0f), 0.9999f) * total;
            var fallback = SunBearerAttack.None;
            foreach (var attack in Everything)
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
