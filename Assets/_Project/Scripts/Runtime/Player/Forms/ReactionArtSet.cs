using System;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The 24 reactions the look-dev designed (combos pass 2, staging/blender/2026-11-18_vfx, approved 2026-11-19), as the
    /// game draws them. Each look is a few parts built from the recipe's pieces: the solid pieces merged into one mesh per
    /// place and element (painted in their bands, drawn in that element's inked solid material), the see-through ones
    /// into one glow mesh per place, and the flat ones rasterised into one or two floor masks. Body parts follow the
    /// struck enemy; ground parts stay where it stood. Every mesh is in the recipe's space: metres, Y up, the origin at
    /// the main body's feet, as the look-dev camera saw it (that camera looked along -Z). Made by Clover > Build >
    /// Reaction Art (ReactionArtBuilder) and drawn by <see cref="ReactionArt"/>.
    /// </summary>
    public class ReactionArtSet : ScriptableObject
    {
        /// <summary>How a part is drawn: a solid inked mesh, a see-through glow mesh, or a mark on the floor.</summary>
        public const byte Solid = 0, Glow = 1, Decal = 2;

        /// <summary>Where a part is held: on the struck body (it follows it) or on the ground where it stood.</summary>
        public const byte OnBody = 0, OnGround = 1;

        [Serializable]
        public struct Part
        {
            /// <summary>"fire_ice body fire", "fire_ice mark 0".</summary>
            public string Name;
            public byte Kind;
            public byte Place;
            /// <summary>The merged mesh of a solid or glow part; null for a mark, which is drawn on a quad.</summary>
            public Mesh Mesh;
            /// <summary>Its element's solid material, the glow material, or the mark's own masked material.</summary>
            public Material Material;
            /// <summary>A mark's middle about the origin (x, z) and its height in the recipe (y, for its order); zero for meshes.</summary>
            public Vector3 Centre;
            /// <summary>A mark's quad size along X and Z; a mesh's bounds.</summary>
            public Vector3 Size;
            public int Triangles;
            /// <summary>How many of the recipe's pieces went into it.</summary>
            public int Pieces;
        }

        [Serializable]
        public struct Lamp
        {
            /// <summary>About the origin, in the recipe's space; lamps move with the body.</summary>
            public Vector3 Position;
            /// <summary>As Unity takes a light's colour (the recipe's linear colour, gamma-encoded).</summary>
            public Color Colour;
            public float Intensity;
            public float Range;
        }

        [Serializable]
        public struct Look
        {
            /// <summary>The pair it is drawn for (<see cref="Clover.Gameplay.AfflictionPairs"/>).</summary>
            public int Pair;
            /// <summary>The recipe's pair, "fire_ice", and its name, which may differ from the game's (The Sealed Jar is Shut in the Dark).</summary>
            public string Key;
            public string Name;
            /// <summary>Turned to the way the blow carried it (from the player through the enemy), not to the camera.</summary>
            public bool Directed;
            public int FirstPart;
            public int PartCount;
            public int FirstLamp;
            public int LampCount;
            public int Triangles;
            public int Pieces;
        }

        public Look[] Looks = new Look[0];
        public Part[] Parts = new Part[0];
        public Lamp[] Lamps = new Lamp[0];

        /// <summary>The most parts any one look has.</summary>
        public int MostParts
        {
            get
            {
                int most = 0;
                foreach (var look in Looks) most = Mathf.Max(most, look.PartCount);
                return most;
            }
        }
    }
}
