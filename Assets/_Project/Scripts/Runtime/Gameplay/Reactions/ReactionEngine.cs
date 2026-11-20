using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Finds reactions (IMPLEMENTATION.md 3.7). When an affliction lands on an enemy that already carries another, the
    /// pair they make reacts, unless it is resting on that enemy; the engine then rests the pair there and reports who,
    /// which pair, and which kind was laid. It only reports: what a reaction does is the listeners' business. A frame
    /// reports at most <see cref="MostPerFrame"/>; the rest wait, in order, for the next frame. Plain rules, keyed by
    /// <see cref="AfflictionKind"/>, so nothing here knows the player.
    /// </summary>
    public sealed class ReactionEngine
    {
        public const int DefaultMostPerFrame = 8;
        /// <summary>How many reactions may wait for a later frame. Past this a reaction is dropped and its pair left unrested, so the next laying may find it again.</summary>
        public const int MostWaiting = 64;

        /// <summary>The engine every <see cref="AfflictionSet"/> uses unless it is given its own; it reports through <see cref="CombatEvents.Reacted"/>.</summary>
        public static ReactionEngine Shared { get; private set; } = NewShared();

        // Domain reload is off: what waited at the end of the last play, and its frame count, must not carry over.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Shared = NewShared();

        private static ReactionEngine NewShared() => new ReactionEngine(CombatEvents.RaiseReacted, () => Time.frameCount);

        /// <summary>The most reactions reported in one frame. A tuning value (task 3.6 sets it from the profiler).</summary>
        public int MostPerFrame = DefaultMostPerFrame;

        private readonly Action<Component, int, AfflictionKind> report;
        private readonly Func<int> clock;
        private readonly float[] rests = new float[AfflictionPairs.Count];
        // A pair that waits for depth (Seized and The Grudge: "two Static or more") reacts only once this kind lies this deep.
        private readonly AfflictionKind[] deepKind = new AfflictionKind[AfflictionPairs.Count];
        private readonly int[] deepAt = new int[AfflictionPairs.Count];

        private int frame;
        private bool started;
        private int reportedThisFrame;

        // The reactions found this frame, so a forced one on the same enemy (a mark that lays the pair's second affliction
        // and then forces it) is not shown a second time. Fixed arrays: finding allocates nothing.
        private const int MostTracked = 128;
        private readonly AfflictionSet[] foundSet = new AfflictionSet[MostTracked];
        private readonly int[] foundPair = new int[MostTracked];
        private int foundCount;

        // A ring of the reactions waiting for a later frame, kept in arrays so waiting allocates nothing.
        private readonly Component[] waitingWho = new Component[MostWaiting];
        private readonly int[] waitingPair = new int[MostWaiting];
        private readonly AfflictionKind[] waitingLaid = new AfflictionKind[MostWaiting];
        private int waitingFirst;
        private int waitingCount;

        /// <param name="report">Told of each reaction: on whom, the pair (<see cref="AfflictionPairs"/>), and the kind laid.</param>
        /// <param name="clock">The number of the frame now; a new number begins a new frame's budget.</param>
        public ReactionEngine(Action<Component, int, AfflictionKind> report, Func<int> clock)
        {
            this.report = report ?? throw new ArgumentNullException(nameof(report));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            for (int pair = 0; pair < rests.Length; pair++) rests[pair] = AfflictionPairs.Rest(pair);
        }

        /// <summary>How many reactions are waiting for a later frame.</summary>
        public int Waiting => waitingCount;

        /// <summary>How long a pair rests on one enemy after it reacts.</summary>
        public float RestFor(int pair) => rests[pair];

        /// <summary>Sets a pair's rest, for an entry that gives its own (3 to 8 s in the pair files).</summary>
        public void SetRest(int pair, float seconds) => rests[pair] = Math.Max(0f, seconds);

        /// <summary>
        /// Makes a pair wait until <paramref name="kind"/> lies at least <paramref name="depth"/> deep on the enemy before it
        /// reacts, in whichever order the two came (Seized: "when a Soaked enemy carries two Static or more"). Until then
        /// nothing reacts and nothing rests, so the next stack can set it off. A depth of 1 or less asks for nothing.
        /// </summary>
        public void SetDepth(int pair, AfflictionKind kind, int depth)
        {
            deepKind[pair] = depth > 1 ? kind : AfflictionKind.None;
            deepAt[pair] = depth > 1 ? depth : 0;
        }

        /// <summary>The kind and depth a pair waits for; None when it waits for nothing.</summary>
        public AfflictionKind DepthKind(int pair) => deepKind[pair];
        public int DepthFor(int pair) => deepAt[pair];

        private bool DeepEnough(AfflictionSet set, int pair) => deepAt[pair] <= 1 || set.Stacks(deepKind[pair]) >= deepAt[pair];

        /// <summary>
        /// An affliction of <paramref name="laid"/> has landed on <paramref name="set"/>, which carried the kinds in
        /// <paramref name="carried"/> (a <see cref="AfflictionSet.PresentMask"/> read before the laying). Each other
        /// kind carried makes a pair; each pair not resting there reacts. Returns how many reacted, counting those
        /// that must wait for the next frame to be reported.
        /// </summary>
        public int Laid(AfflictionSet set, Component who, AfflictionKind laid, int carried)
        {
            int partners = carried & ~(1 << (int)laid);
            if (partners == 0) return 0;
            CatchUp();
            int reacted = 0;
            for (int kind = 1; kind < Afflictions.Count; kind++)
            {
                if ((partners & (1 << kind)) == 0) continue;
                int pair = AfflictionPairs.Index(laid, (AfflictionKind)kind);
                if (pair < 0 || set.ReactionRest(pair) > 0f || !DeepEnough(set, pair)) continue;
                if (Emit(set, who, pair, laid)) reacted++;
            }
            return reacted;
        }

        /// <summary>
        /// Sets a pair off on <paramref name="set"/> whether or not it rests there, though never short of the depth it waits for (a forced reaction: the action
        /// ForceReaction), within the frame's budget like any other, and rests it again from now. True when it was
        /// reported or waits to be.
        /// </summary>
        public bool Force(AfflictionSet set, Component who, int pair, AfflictionKind laid)
        {
            if (set == null || pair < 0 || pair >= rests.Length || !DeepEnough(set, pair)) return false;
            CatchUp();
            // Already found on this enemy this frame: it reacts once, not twice.
            if (FoundThisFrame(set, pair)) return true;
            return Emit(set, who, pair, laid);
        }

        private bool FoundThisFrame(AfflictionSet set, int pair)
        {
            for (int i = 0; i < foundCount; i++)
                if (foundPair[i] == pair && ReferenceEquals(foundSet[i], set)) return true;
            return false;
        }

        // Rested now in either case, so the same pair on the same enemy is not found twice while it waits.
        private bool Emit(AfflictionSet set, Component who, int pair, AfflictionKind laid)
        {
            if (foundCount < MostTracked)
            {
                foundSet[foundCount] = set;
                foundPair[foundCount] = pair;
                foundCount++;
            }
            if (reportedThisFrame < MostPerFrame)
            {
                set.RestReaction(pair, rests[pair]);
                reportedThisFrame++;
                report(who, pair, laid);
                return true;
            }
            if (waitingCount >= MostWaiting) return false;
            set.RestReaction(pair, rests[pair]);
            int slot = (waitingFirst + waitingCount) % MostWaiting;
            waitingWho[slot] = who;
            waitingPair[slot] = pair;
            waitingLaid[slot] = laid;
            waitingCount++;
            return true;
        }

        /// <summary>
        /// Begins a new frame's budget if the frame has moved on, and reports what waited. Laying calls it; whoever
        /// listens should also call it once a frame, so what waits is not kept until the next laying.
        /// </summary>
        public void Pump() => CatchUp();

        private void CatchUp()
        {
            int now = clock();
            if (started && now == frame) return;
            started = true;
            frame = now;
            reportedThisFrame = 0;
            // Let go of last frame's enemies, so the list keeps no destroyed one alive.
            Array.Clear(foundSet, 0, foundCount);
            foundCount = 0;
            while (waitingCount > 0 && reportedThisFrame < MostPerFrame)
            {
                var who = waitingWho[waitingFirst];
                int pair = waitingPair[waitingFirst];
                var laid = waitingLaid[waitingFirst];
                // Let go of the enemy, so a waiting slot does not keep a destroyed one alive.
                waitingWho[waitingFirst] = null;
                waitingFirst = (waitingFirst + 1) % MostWaiting;
                waitingCount--;
                reportedThisFrame++;
                report(who, pair, laid);
            }
        }
    }
}
