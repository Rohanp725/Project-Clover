using System;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// One outfit as the player's body wears it: a whole character mesh skinned to the player's rig, its toon materials
    /// in submesh order, the names of the bones its bind poses are written against, and the bounds of its pose.
    /// </summary>
    [Serializable]
    public class Outfit
    {
        public string Id;
        public Mesh Mesh;
        public Material[] Materials = new Material[0];
        public string[] BoneNames = new string[0];
        public Bounds Bounds;
    }
}
