using System;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The elements' approved effect art (the look-dev's second pass, staging/blender/2026-11-18_vfx), as the game draws
    /// it: every piece of the eleven kits baked into a mesh of its own (solid pieces painted in their bands, inked by
    /// their element) or a masked mark for the floor, the drawn 4x4 sheets with their materials, and the flat shadow
    /// card. Made by Clover > Build > Element Art (ElementArtBuilder) and read through <see cref="ElementArt"/>.
    /// </summary>
    public class ElementArtSet : ScriptableObject
    {
        /// <summary>How a piece is drawn: a solid inked shape, a see-through unlit shape (a beam, a gas layer), or a mark on the floor.</summary>
        public const byte Solid = 0, Glow = 1, Decal = 2;

        [Serializable]
        public struct Piece
        {
            /// <summary>The kit's name without its "vfx2_" and with ".001" as "_1": "fire_hoods_spell", "dark_inkring_affliction_1".</summary>
            public string Name;
            public byte Kind;
            /// <summary>The mesh of a solid or glow piece; null for a mark.</summary>
            public Mesh Mesh;
            /// <summary>Its element's solid material, the glow material, or the mark's own masked material.</summary>
            public Material Material;
            /// <summary>Where the piece's footprint lies about its origin (x, z) and how big it is, in metres.</summary>
            public Vector3 Centre;
            public Vector3 Size;
        }

        [Serializable]
        public struct Sheet
        {
            /// <summary>"fire_flame_loop", "dark_hole_close_v2", ...</summary>
            public string Name;
            public Material Material;
            public bool Loops;
        }

        public Piece[] Pieces = new Piece[0];
        public Sheet[] Sheets = new Sheet[0];
        /// <summary>Each element's solid material by its place on the ring: the band shading and its own ink colour.</summary>
        public Material[] Solids = new Material[0];
        /// <summary>A flat body's shadow lying along the ground from the feet (the mask's v runs from the feet outward).</summary>
        public Material ShadowFigure;
        /// <summary>The font of the line that says which effects are shown after the switch is pressed.</summary>
        public Font Label;
        /// <summary>The reactions' designed looks (Clover > Build > Reaction Art); with them here the art draws the reactions too.</summary>
        public ReactionArtSet Reactions;
    }
}
