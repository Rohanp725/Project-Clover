using System;
using UnityEngine;

namespace Clover.Player.Growth
{
    /// <summary>
    /// The one pile of Shards a death leaves (IMPLEMENTATION.md 3.13): how many, in which scene, on which enemy or on
    /// what ground. Only the data for now: the rules come with <c>ShardKeeper</c> (task L.3). <c>JsonUtility</c> cannot
    /// write a null, so a pile of no Shards is no pile.
    /// </summary>
    [Serializable]
    public class LostShards
    {
        public int Amount;
        public string Scene = "";
        /// <summary>The scene's name and the killer's index in <c>AreaDirector.Enemies</c>; "" when they lie on the ground.</summary>
        public string KillerKey = "";
        /// <summary>Where they lie when no enemy carries them: the last firm ground.</summary>
        public Vector3 Position;

        /// <summary>True when there is a pile to reclaim.</summary>
        public bool Any => Amount > 0;
    }
}
