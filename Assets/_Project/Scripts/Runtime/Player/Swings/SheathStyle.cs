using System;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The element along the edge while the blade holds a charge (and while an infused strike winds up): a thin strip
    /// fitted to the blade line and turned to the camera, never pieces stuck on. Ice's is a row of frost teeth that run
    /// along the edge like a chain; Metal's is only a glint that runs from the guard to the point now and then.
    /// </summary>
    [Serializable]
    public struct SheathStyle
    {
        public bool Shown;
        public GlowStyle Style;
        /// <summary>Metres across, before the weapon's shape scales it.</summary>
        public float Width;
        /// <summary>The share of its width on the back side of the edge: 0.5 centred on it, 0 all on the edge's side.</summary>
        public float Inside;
        /// <summary>How far its brightness jumps from moment to moment, 0 to 1.</summary>
        public float Flicker;
        /// <summary>Times a second a bolt drawn along it takes a new shape.</summary>
        public float Rebolt;
        /// <summary>Frost teeth: their spacing in metres and how fast they run along the edge, metres a second.</summary>
        public bool Teeth;
        public float ToothEvery, ToothRun;
        /// <summary>A glint only: seconds between glints, seconds one takes to run the edge, and the share of the edge it lights.</summary>
        public bool Glint;
        public float GlintEvery, GlintSeconds, GlintSpan;
    }
}
