using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The pale piece a bar leaves behind when it drops: it holds where the bar was for a moment, then drains down to
    /// where the bar is now, so the size of the loss can be read. A bar that rises takes the piece up with it at once.
    /// </summary>
    public class LagMeter
    {
        /// <summary>Seconds the piece holds before it starts to drain.</summary>
        public float Hold = 0.45f;
        /// <summary>How much of the whole bar drains each second once it starts.</summary>
        public float DrainPerSecond = 0.9f;

        /// <summary>Where the pale piece ends, as a share of the whole bar.</summary>
        public float Fraction { get; private set; } = 1f;

        private float held;

        /// <summary>Puts the piece exactly where the bar is, as when the bar is first shown.</summary>
        public void Snap(float now)
        {
            Fraction = now;
            held = Hold;
        }

        /// <summary>Moves the piece on by a step of time and returns where it now ends.</summary>
        public float Tick(float now, float deltaTime)
        {
            if (now >= Fraction)
            {
                Fraction = now;
                held = Hold;
            }
            else if (held > 0f) held -= deltaTime;
            else Fraction = Mathf.MoveTowards(Fraction, now, DrainPerSecond * deltaTime);
            return Fraction;
        }
    }
}
