using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// One patch of the player's ground, or one thing thrown, as <see cref="Hazards"/> is asked for it: a circle that
    /// bites what is inside it, once each or at intervals, for a while, and may move.
    /// </summary>
    public struct HazardSpec
    {
        /// <summary>How it is seen. <see cref="HazardKind.Element"/> is drawn in <see cref="Tint"/>.</summary>
        public HazardKind Kind;
        public Color Tint;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Radius;
        /// <summary>Seconds it lasts once it has begun.</summary>
        public float Seconds;
        /// <summary>Seconds between bites.</summary>
        public float BiteEvery;
        /// <summary>Seconds it waits before it begins.</summary>
        public float Delay;
        /// <summary>True when it bites each enemy once only.</summary>
        public bool Once;
        /// <summary>True when it arrives with a flash the size of what it strikes.</summary>
        public bool Burst;
        /// <summary>What each bite deals.</summary>
        public SpellBlow Blow;
        /// <summary>How it travels, when it has a velocity. Homing turns toward <see cref="Target"/>; Returning comes back to the player half-way through its time and may strike again.</summary>
        public Flight Flight;
        public Transform Target;
        /// <summary>True when it stays on <see cref="Target"/> wherever that goes: a helper that keeps striking one enemy.</summary>
        public bool Follow;
        /// <summary>It ends after this many bites; zero for as many as its time allows.</summary>
        public int Bites;
        /// <summary>True when a mark is drawn on the ground while it waits out its delay: something is about to fall here.</summary>
        public bool Warn;
        /// <summary>Something the player has grown or planted: it is counted, can be stood near, and can be burst with the rest.</summary>
        public bool Grown;
        /// <summary>For a grown thing: which kind it is (things of one kind share a limit) and how many of that kind may stand; one more takes the place of the one nearest its end. Zero: no limit.</summary>
        public int Family;
        public int Limit;
        /// <summary>True when a reaction laid it: what its bites lay starts no reaction of its own (IMPLEMENTATION.md 3.7).</summary>
        public bool FromReaction;
        /// <summary>
        /// Nonzero for the parts of one shape laid together (from <see cref="Hazards.NewGroup"/>): with <see cref="Once"/>,
        /// each enemy is struck once by the whole shape, however many of its parts it stands in.
        /// </summary>
        public int Group;
        /// <summary>Left alone by its bites: the enemy it came out of (Ill Wind's plume does not dose the one that shed it).</summary>
        public IDamageable Spares;
        /// <summary>
        /// Drawn by its reaction's own look and nothing else: no bolt from the sky at each bite, no ring, no puffs. Seized's
        /// live film bit every half second with a bolt, a white ring and a flash of the whole sky, which hurt the eyes
        /// (the owner, 2026-11-20: "make it less flickery so it doesnt hurt peoples eyes").
        /// </summary>
        public bool Quiet;
        /// <summary>Laid besides its own blow, a dose a bite on all it bites (The Host: a grown thing fed on the poisoned doses too).</summary>
        public AfflictionKind AlsoLays;
        /// <summary>
        /// Above zero, a helper that walks at this many metres a second from where it rose to <see cref="Target"/>, biting
        /// nothing on the way, and then keeps to it and bites (The Drops Quicken's rime creature, risen where an enemy fell).
        /// </summary>
        public float WalksTo;
        /// <summary>A thrown piece: it ends at the first it strikes (a feather, a flake, a shard), and lays AlsoLays only on what it struck.</summary>
        public bool StopsAtFirst;
        /// <summary>
        /// Ground that acts on whoever stands in it at each bite, not only on what stood there when it formed (the ledger's
        /// ground-states): the mark's effect (a Player.UpgradeEffect) told to the player's marks with each bite (Hazards.GroundActs).
        /// </summary>
        public object Acts;
        /// <summary>Above zero: draws everything standing in it toward its middle at this many metres a second (the ledger's pull-to-point).</summary>
        public float Pull;
    }
}
