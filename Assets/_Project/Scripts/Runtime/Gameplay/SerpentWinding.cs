using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// How the arches of a serpent come and go through a line of cloud so that one long body seems to wind in and out
    /// of it: each arch rises, slides forward along its own length while it stands, sinks, and waits under the cloud
    /// before rising again where it began, each at its own point in the round. Plain numbers, no Unity.
    /// </summary>
    [Serializable]
    public class SerpentWinding
    {
        /// <summary>Seconds one arch takes to rise, stand, sink and wait.</summary>
        public float RoundSeconds = 11f;
        /// <summary>The shares of the round spent rising, standing, and sinking. What is left is spent under the cloud.</summary>
        public float RisingShare = 0.2f;
        public float StandingShare = 0.42f;
        public float SinkingShare = 0.2f;
        /// <summary>How far round its own round each arch is from the one before it.</summary>
        public float Stagger = 0.375f;

        /// <summary>Where arch <paramref name="index"/> is in its round at <paramref name="time"/>, from 0 to 1.</summary>
        public float Phase(int index, float time)
        {
            float phase = time / Math.Max(0.01f, RoundSeconds) + index * Stagger;
            return phase - (float)Math.Floor(phase);
        }

        /// <summary>How far out of the cloud the arch stands: 0 wholly under, 1 wholly risen.</summary>
        public float Risen(int index, float time)
        {
            float phase = Phase(index, time);
            if (phase < RisingShare) return Ease(phase / RisingShare);
            phase -= RisingShare;
            if (phase < StandingShare) return 1f;
            phase -= StandingShare;
            if (phase < SinkingShare) return 1f - Ease(phase / SinkingShare);
            return 0f;
        }

        /// <summary>How far along its own length the arch has slid, from -0.5 as it breaks the cloud to 0.5 as it goes under.</summary>
        public float Slide(int index, float time)
        {
            float above = RisingShare + StandingShare + SinkingShare;
            return Math.Min(1f, Phase(index, time) / Math.Max(0.01f, above)) - 0.5f;
        }

        private static float Ease(float t) => t * t * (3f - 2f * t);
    }
}
