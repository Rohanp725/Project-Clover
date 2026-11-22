using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// What the player's edge carries for its next few weapon blows, on any enemy (the ledger's depth-moved): the sickest
    /// one's depth of poison, which each blow sets what it strikes to (Share Alike's Carried on the Blade), or a dose of
    /// grit each light blow lays (Ill Wind's Dust on the Blade). A few coats at once, each counting its own blows down.
    /// </summary>
    public sealed class BladeCoat
    {
        public const int Most = 4;

        private struct Coat
        {
            public AfflictionKind Kind;
            public int Stacks;
            public bool SetsDepth;
            public bool LightOnly;
            public int Blows;
            public float Until;
            public bool Used;
        }

        private readonly Coat[] coats = new Coat[Most];

        /// <summary>True while any coat has blows left.</summary>
        public bool Any
        {
            get
            {
                for (int i = 0; i < Most; i++)
                    if (coats[i].Blows > 0) return true;
                return false;
            }
        }

        /// <summary>
        /// Loads the edge: its next <paramref name="blows"/> weapon blows (light ones only, with <paramref name="lightOnly"/>)
        /// lay <paramref name="stacks"/> of the kind, or with <paramref name="setsDepth"/> raise what they strike to that depth.
        /// A coat of the same kind and way is made the deeper and its blows counted again. <paramref name="until"/> of zero
        /// lasts until spent.
        /// </summary>
        public void Load(AfflictionKind kind, int stacks, bool setsDepth, bool lightOnly, int blows, float until)
        {
            if (kind == AfflictionKind.None || stacks <= 0 || blows <= 0) return;
            int free = -1;
            for (int i = 0; i < Most; i++)
            {
                ref var coat = ref coats[i];
                if (coat.Blows > 0 && coat.Kind == kind && coat.SetsDepth == setsDepth && coat.LightOnly == lightOnly)
                {
                    coat.Stacks = System.Math.Max(coat.Stacks, stacks);
                    coat.Blows = blows;
                    coat.Until = until;
                    return;
                }
                if (free < 0 && coat.Blows <= 0) free = i;
            }
            if (free < 0) free = 0;
            coats[free] = new Coat { Kind = kind, Stacks = stacks, SetsDepth = setsDepth, LightOnly = lightOnly, Blows = blows, Until = until };
        }

        /// <summary>
        /// One weapon blow landing on <paramref name="set"/>: for each coat that takes it, how many of which kind to lay
        /// (written into <paramref name="kinds"/> and <paramref name="stacks"/>); returns how many. The coat is counted
        /// down when the swing ends (<see cref="Swung"/>), so a swing through three is one blow.
        /// </summary>
        public int Blow(AfflictionSet set, bool heavy, float now, AfflictionKind[] kinds, int[] stacks)
        {
            int found = 0;
            for (int i = 0; i < Most && found < kinds.Length; i++)
            {
                ref var coat = ref coats[i];
                if (coat.Blows <= 0) continue;
                if (coat.Until > 0f && now > coat.Until)
                {
                    coat.Blows = 0;
                    continue;
                }
                if (coat.LightOnly && heavy) continue;
                coat.Used = true;
                int lay = coat.SetsDepth ? DepthMoves.ToRaise(set, coat.Kind, coat.Stacks) : coat.Stacks;
                if (lay <= 0) continue;
                kinds[found] = coat.Kind;
                stacks[found] = lay;
                found++;
            }
            return found;
        }

        /// <summary>A swing ended: each coat it used has one blow fewer.</summary>
        public void Swung()
        {
            for (int i = 0; i < Most; i++)
            {
                ref var coat = ref coats[i];
                if (!coat.Used) continue;
                coat.Used = false;
                if (coat.Blows > 0) coat.Blows--;
            }
        }

        /// <summary>Blows left on the coat of this kind (the most of any), for the HUD and tests.</summary>
        public int BlowsLeft(AfflictionKind kind)
        {
            int most = 0;
            for (int i = 0; i < Most; i++)
                if (coats[i].Kind == kind) most = System.Math.Max(most, coats[i].Blows);
            return most;
        }

        public void Clear()
        {
            for (int i = 0; i < Most; i++) coats[i] = default;
        }
    }
}
