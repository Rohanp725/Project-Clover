using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// The technique chord, R1 and R2 together (IMPLEMENTATION.md 3.5): the light attack starts at once as always; a
    /// heavy press within <see cref="ChordSeconds"/> of the light press, while that light step is still in the first
    /// <see cref="ChordSeconds"/> of its wind-up, turns it into the technique. No delay is ever added to a light attack.
    /// Wired when the techniques exist (task 5.3); the rule is settled now so the layer and the chord are written together.
    /// </summary>
    [Serializable]
    public class ChordRules
    {
        public float ChordSeconds = 0.08f;

        private float sinceLight = float.MaxValue;

        /// <summary>The light button went down.</summary>
        public void Light() => sinceLight = 0f;

        public void Tick(float deltaTime)
        {
            if (sinceLight < float.MaxValue) sinceLight += Math.Max(0f, deltaTime);
        }

        /// <summary>
        /// The heavy button went down while a light step has been winding up for <paramref name="lightWindupElapsed"/>
        /// seconds (negative: no light step is winding up). True: cancel the light into the technique.
        /// </summary>
        public bool Heavy(float lightWindupElapsed)
        {
            bool chord = sinceLight <= ChordSeconds && lightWindupElapsed >= 0f && lightWindupElapsed <= ChordSeconds;
            if (chord) sinceLight = float.MaxValue;
            return chord;
        }

        public void Reset() => sinceLight = float.MaxValue;
    }
}
