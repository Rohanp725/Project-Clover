using System;
using System.Collections.Generic;

namespace Clover.Gameplay
{
    /// <summary>
    /// An affliction's depth moved between enemies as one number (the ledger's depth-moved): every enemy passed raised to
    /// the sickest one's depth (Share Alike), doses drawn out of one body into another (Rot Roots' Gathered In), every dose
    /// near gathered into one burst with no limit (All Into One), and a line of bursts each carrying the last one's doses
    /// on (One by One). Only the sums live here; the caller lays what they say through the one laying entry, so reactions
    /// and the marks that answer a laying still hear of it.
    /// </summary>
    public static class DepthMoves
    {
        /// <summary>The deepest the kind lies on any of them, and never less than <paramref name="least"/>.</summary>
        public static int Sickest(IReadOnlyList<AfflictionSet> sets, AfflictionKind kind, int least)
        {
            int deepest = Math.Max(0, least);
            if (sets == null || kind == AfflictionKind.None) return deepest;
            for (int i = 0; i < sets.Count; i++)
                if (sets[i] != null) deepest = Math.Max(deepest, sets[i].Stacks(kind));
            return deepest;
        }

        /// <summary>How many stacks to lay on <paramref name="set"/> to bring the kind up to <paramref name="depth"/>, no deeper than it can lie; zero when it is there already.</summary>
        public static int ToRaise(AfflictionSet set, AfflictionKind kind, int depth)
        {
            if (set == null || kind == AfflictionKind.None) return 0;
            int goal = Math.Min(depth, set.DepthOf(kind));
            return Math.Max(0, goal - set.Stacks(kind));
        }

        /// <summary>How many of <paramref name="count"/> stacks can move from one body into another: what the first carries, and only as deep as the second can take.</summary>
        public static int Movable(AfflictionSet from, AfflictionSet into, AfflictionKind kind, int count)
        {
            if (from == null || into == null || kind == AfflictionKind.None || count <= 0) return 0;
            int room = Math.Max(0, into.DepthOf(kind) - into.Stacks(kind));
            return Math.Min(Math.Min(count, from.Stacks(kind)), room);
        }

        /// <summary>
        /// One burst in a line that carries its doses on: it bursts with its own and all those carried to it, and what it
        /// burst with is carried to the next (each passing its doses to the next before it goes, the last the largest).
        /// </summary>
        public static int LineBurst(int own, ref int carried)
        {
            int total = Math.Max(0, own) + Math.Max(0, carried);
            carried = total;
            return total;
        }

        /// <summary>Half of a body's doses, rounded down: what a drink takes and leaves (The Yellow Gale takes half of each poisoned enemy's).</summary>
        public static int Half(int stacks) => Math.Max(0, stacks) / 2;

        /// <summary>
        /// A store split evenly among those a cast struck, rounded up (the owner's answer of 2026-11-22): one lone enemy takes
        /// it all, and the more are struck the thinner it spreads. Zero when there is no store or no one was struck.
        /// </summary>
        public static int ShareEach(int store, int struck) => store <= 0 || struck <= 0 ? 0 : (store + struck - 1) / struck;

        /// <summary>
        /// Doses laid on one that cannot take them all: those that fit, and what is pushed past its depth (One pushed past
        /// eight bursts at once and passes on). <paramref name="over"/> is zero when everything fits.
        /// </summary>
        public static int Fits(AfflictionSet into, AfflictionKind kind, int count, out int over)
        {
            over = 0;
            if (into == null || kind == AfflictionKind.None || count <= 0) return 0;
            int room = Math.Max(0, into.DepthOf(kind) - into.Stacks(kind));
            int fits = Math.Min(count, room);
            over = count - fits;
            return fits;
        }
    }
}
