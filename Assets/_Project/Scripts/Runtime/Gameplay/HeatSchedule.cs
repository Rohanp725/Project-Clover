using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// When the sun swells over a district (fire_rework.md, section 2): the first swell begins <see cref="FirstAt"/>
    /// seconds in and another every <see cref="Every"/> seconds after, each announced for <see cref="Warning"/>
    /// seconds and lasting <see cref="Swell"/>. Plain numbers, so the rhythm can be tested without a scene.
    /// </summary>
    [Serializable]
    public struct HeatSchedule
    {
        public enum Phase { Calm, Warning, Swell }

        /// <summary>Seconds from the start to the first swell's burning; below zero, the sun never swells.</summary>
        public float FirstAt;
        /// <summary>Seconds from one swell's start to the next; zero or less for one swell only.</summary>
        public float Every;
        public float Warning;
        public float Swell;

        public HeatSchedule(float firstAt, float every, float warning = 5f, float swell = 15f)
        {
            FirstAt = firstAt;
            Every = every;
            Warning = warning;
            Swell = swell;
        }

        /// <summary>A sky that never swells.</summary>
        public static HeatSchedule Never => new HeatSchedule(-1f, 0f);

        public bool Swells => FirstAt >= 0f && Swell > 0f;

        /// <summary>Where the rhythm stands at a moment: calm, the warning (with how far through it), or the swell (likewise).</summary>
        public Phase At(float seconds, out float progress)
        {
            progress = 0f;
            if (!Swells) return Phase.Calm;
            // Seconds since the most recent swell began (negative before the first).
            float since = seconds - FirstAt;
            if (since >= 0f && Every > 0f)
            {
                float cycle = Math.Max(Every, Warning + Swell);
                since %= cycle;
                // Within the warning of the next one.
                if (since >= cycle - Warning)
                {
                    progress = (since - (cycle - Warning)) / Math.Max(0.0001f, Warning);
                    return Phase.Warning;
                }
            }
            if (since >= 0f && since < Swell)
            {
                progress = since / Swell;
                return Phase.Swell;
            }
            if (since < 0f && since >= -Warning)
            {
                progress = 1f + since / Math.Max(0.0001f, Warning);
                return Phase.Warning;
            }
            return Phase.Calm;
        }

        /// <summary>The rule of the swell: someone standing in the sun burns, in shade they do not, and in a calm zone (an elite's arena) no one does.</summary>
        public static bool Burns(Phase phase, bool lit, bool calm) => phase == Phase.Swell && lit && !calm;
    }
}
