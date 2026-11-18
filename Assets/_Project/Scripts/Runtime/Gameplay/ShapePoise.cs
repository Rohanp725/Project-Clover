using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Bearer's poise after a fall (fire_land.md 6.4, reworked on 2026-11-18). Once a shape has got up from being
    /// exposed or from a broken balance and has taken its sun up again, its balance cannot be broken for
    /// <see cref="Seconds"/>; and while it is going back for the sun it holds full poise. Blows still hurt it and still
    /// fill the meter, to one short of breaking: what was owed is there when the poise ends. Plain C#.
    /// </summary>
    [Serializable]
    public class ShapePoise
    {
        public float Seconds = 12f;

        public float Left { get; private set; }
        /// <summary>True while the shape is on its way back to the sun after a fall.</summary>
        public bool Returning;
        /// <summary>True while its balance cannot be broken.</summary>
        public bool Holds => Left > 0f || Returning;

        public void Reset()
        {
            Left = 0f;
            Returning = false;
        }

        /// <summary>It has got up and holds its sun again: the poise begins.</summary>
        public void Recovered()
        {
            Returning = false;
            Left = Seconds;
        }

        public void Tick(float deltaTime)
        {
            if (Left > 0f) Left = Math.Max(0f, Left - deltaTime);
        }

        /// <summary>Adds to the meter as a blow would. True when this breaks the balance, which it never does while the poise holds.</summary>
        public bool Add(StaggerMeter meter, float amount)
        {
            if (meter == null || amount <= 0f || meter.IsStaggered) return false;
            if (!Holds) return meter.Add(amount);
            float room = meter.Maximum - 1f - meter.Value;
            if (room > 0f) meter.Add(Math.Min(amount, room));
            return false;
        }
    }
}
