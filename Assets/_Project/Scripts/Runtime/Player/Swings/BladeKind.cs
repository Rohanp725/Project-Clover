namespace Clover.Player.Swings
{
    /// <summary>What part of a weapon sweeps through a swing, which decides how its line is measured and drawn.</summary>
    public enum BladeKind
    {
        /// <summary>A cutting edge from where it leaves the guard to the point: the Katana, the Massive Sword, the scythe's crescent, a knife, the sickle.</summary>
        Edge,
        /// <summary>A point that thrusts: the spear's leaf, an arrow's head. Drawn as a line along the thrust when it goes straight.</summary>
        Point,
        /// <summary>A blunt mass at the end of a haft or a chain: the hammer's head, the chain's weight. Its line runs along the haft through the head.</summary>
        Head,
        /// <summary>A striking length with no edge: the bow's limbs, from the grip to the nock.</summary>
        Stave,
        /// <summary>A part of the body: the forearm to the fist, the shin to the foot.</summary>
        Limb,
        /// <summary>The outer rim of a fan, opened.</summary>
        Rim,
    }
}
