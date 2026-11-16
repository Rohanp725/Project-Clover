using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>What is fixed about one element: where it stands on the ring, its colour, what it lays on enemies, and whom it mixes with.</summary>
    public class ElementInfo
    {
        /// <summary>Its place on the ring, counting clockwise from straight up.</summary>
        public int Index;
        public string Name = "";
        /// <summary>Its one saturated colour, as paint: for the chart, the HUD and tinted effects.</summary>
        public Color Colour = Color.white;
        /// <summary>The same colour as light, past white so it blooms: for slashes and the weapon wrapped in a surge.</summary>
        public Color Glow = Color.white;
        /// <summary>Degrees clockwise from straight up at which its tree grows from the middle of the chart.</summary>
        public float Bearing;
        public AfflictionKind Affliction;
        /// <summary>The next element round the ring, with which it shares a mixed branch, and what that mix is called.</summary>
        public string MixWith = "";
        public string MixName = "";
        /// <summary>The name of its picture among <see cref="UpgradeIcons"/>.</summary>
        public string Icon = "";
    }
}
