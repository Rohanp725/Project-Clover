using System;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The 24 reactions the look-dev designed (combos pass 2, staging/blender/2026-11-18_vfx, approved 2026-11-19), as the
    /// game draws them. Each look is a few parts built from the recipe's pieces, grouped so each group can move as one:
    /// the solid pieces merged into one mesh per place, element and motion role (a bolt, tongues, steam, chips...;
    /// <see cref="ReactionTimelines.RoleOf"/>), painted in their bands and drawn in that element's inked solid material;
    /// the see-through ones into glow meshes the same way; and the flat ones rasterised into one or two floor masks.
    /// Each part carries its cue (<see cref="ReactionMotion.Cue"/>: verb, beat, delay, times) and the pivot it moves
    /// about. Body parts follow the struck enemy; ground parts stay where it stood. Every mesh is in the recipe's space
    /// (metres, Y up, the origin at the main body's feet, as the look-dev camera saw it, looking along -Z) less its
    /// pivot. Made by Clover > Build > Reaction Art (ReactionArtBuilder) and drawn by <see cref="ReactionArt"/>.
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
            /// <summary>"fire_ice body ice debris", "fire_ice mark 0".</summary>
            public string Name;
            public byte Kind;
            public byte Place;
            /// <summary>Its motion role (<see cref="ReactionTimelines"/>: strike, flame, vapour, debris, mark...).</summary>
            public byte Role;
            /// <summary>When and how it moves; unset (no seconds) in a set built before the motion, which pops in and holds.</summary>
            public ReactionMotion.Cue Motion;
            /// <summary>The point it moves about, in the recipe's space: its base for what rises, its tip for a bolt drawn down, the body for a burst.</summary>
            public Vector3 Pivot;
            /// <summary>The way a ray is drawn out from its pivot, in the recipe's space; zero for the rest.</summary>
            public Vector3 Axis;
            /// <summary>The merged mesh of a solid or glow part, about its pivot; null for a mark, which is drawn on a quad.</summary>
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
            /// <summary>Seconds after it starts that its impact lands (its lamps light then), and that its last part is gone.</summary>
            public float ImpactAt;
            public float Seconds;
            /// <summary>Its timeline in words (<see cref="ReactionTimelines.Timeline.Story"/>).</summary>
            public string Story;
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
