using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The sun of the sun-king's land setting for the first time in ages, once its god has fallen and his fire is taken
    /// (WORLD.md, "Fire's land: changes"). It is plain numbers over its seconds, so the rule can be tested without a
    /// scene; the court's director turns the light, the shadow and the gate by them. The light leans down from the noon
    /// height to the western horizon and swings round to shine from the west, so the obelisk's shadow turns to point
    /// east and stretches long across the court; the west gate opens to the low sun only at the end, and the way out is
    /// west, into the light, after the sun.
    /// </summary>
    [System.Serializable]
    public class Sunset
    {
        public float Seconds = 8f;
        /// <summary>Degrees the sun stands above the ground at noon and when it has set as low as it goes here.</summary>
        public float NoonPitch = 78f, LowPitch = 13f;
        /// <summary>Degrees round from north (toward the east for more) the light shines toward at the end: from the west, eastward.</summary>
        public float Toward = 90f;
        /// <summary>How many times its noon length the drawn shadow of the obelisk is at the end.</summary>
        public float LongestShadow = 2.4f;
        /// <summary>The share of the sunset after which the west gate starts to open.</summary>
        public float GateFrom = 0.6f;

        public bool Begun { get; private set; }
        public float Elapsed { get; private set; }
        private float fromAngle, fromPitch = 78f;

        /// <summary>The sun starts down, with the shadow pointing this many degrees round from north and the sun this high.</summary>
        public void Begin(float shadowAngle, float sunPitch)
        {
            Begun = true;
            Elapsed = 0f;
            fromAngle = shadowAngle;
            fromPitch = sunPitch;
        }

        /// <summary>Back to noon, as if it had never moved.</summary>
        public void Reset()
        {
            Begun = false;
            Elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Begun) Elapsed += Mathf.Max(0f, deltaTime);
        }

        /// <summary>How far through it is, 0 to 1; 0 before it has begun.</summary>
        public float Progress => !Begun ? 0f : Seconds > 0f ? Mathf.Clamp01(Elapsed / Seconds) : 1f;
        public bool Done => Begun && Progress >= 1f;
        // Slow to leave noon and slow to settle at the horizon: a weight being moved, not a dial turned.
        private float Eased => Mathf.SmoothStep(0f, 1f, Progress);

        /// <summary>Where the obelisk's shadow points now, in degrees round from north; the light shines the same way.</summary>
        public float ShadowAngle => Mathf.Repeat(Mathf.LerpAngle(fromAngle, Toward, Eased), 360f);
        /// <summary>The sun's height above the ground now, in degrees.</summary>
        public float Pitch => Mathf.Lerp(fromPitch, LowPitch, Eased);
        /// <summary>How many times its noon length the drawn shadow is now.</summary>
        public float ShadowStretch => Mathf.Lerp(1f, LongestShadow, Eased);
        /// <summary>How warm the light, the air and the grade are now, 0 at noon to 1 at sunset.</summary>
        public float Warmth => Eased;
        /// <summary>How far the west gate has opened, 0 to 1.</summary>
        public float GateOpen => GateFrom >= 1f ? (Done ? 1f : 0f) : Mathf.Clamp01((Progress - GateFrom) / (1f - GateFrom));
    }
}
