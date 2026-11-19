using System;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// How a trail or a sheath is drawn with the game's own `Clover/Glow` shader (the slash arcs' material, copied once
    /// per look when the player starts): its shape number, whether it burns as fire, added light or laid-over colour,
    /// its edge and its breakup, and which window of the shape's UVs the strip is mapped onto. A per-element texture
    /// from a later art pass replaces the copied material through <see cref="SwingTrails.UseMaterial"/>.
    /// </summary>
    [Serializable]
    public struct GlowStyle
    {
        /// <summary>The shader's shape: 1 streak, 2 slash, 3 flat, 6 flame tongue, 7 bolt (see clover_glow.shader).</summary>
        public float Shape;
        /// <summary>Drawn as fire: read through the flame bands, cooling to red and smoke as its alpha falls.</summary>
        public bool Fire;
        /// <summary>Added to what is behind (light, lightning) or laid over it (water, shade, dust, venom).</summary>
        public bool Additive;
        public float Sharpness, Hard, Core, Noise, NoiseScale;
        public Vector2 NoiseSpeed;
        /// <summary>The material's colour; the strip's vertex colours multiply it.</summary>
        public Color Colour;
        /// <summary>The window of the shape the strip is laid on: U along the trail from its tail to its head, V across from the inner edge to the outer.</summary>
        public float U0, U1, V0, V1;
        /// <summary>U across and V along, for shapes drawn along V (the bolt).</summary>
        public bool Swap;
    }
}
