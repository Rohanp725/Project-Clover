using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// When burning floor bites whoever is on it (fire_land.md 6.4, the hot-floor rule as reworked on 2026-11-18): the
    /// first bite half a second after stepping on, then one every half second, so walking straight across a lane costs
    /// one bite at most; a dodge roll across it costs nothing, and the count starts again where the roll ends. Plain C#.
    /// </summary>
    [Serializable]
    public class HotFloorClock
    {
        public float FirstBite = 0.5f;
        public float Every = 0.5f;

        public int Bites { get; private set; }
        /// <summary>Seconds on hot floor without a break, rolls not counted.</summary>
        public float OnFor => onFor;

        private float onFor, untilBite;

        public void Reset()
        {
            onFor = 0f;
            untilBite = FirstBite;
        }

        /// <summary>Advances the clock. True when this tick bites.</summary>
        public bool Tick(float deltaTime, bool onHot, bool rolling)
        {
            if (!onHot || rolling)
            {
                onFor = 0f;
                return false;
            }
            if (onFor <= 0f) untilBite = FirstBite;
            onFor += deltaTime;
            untilBite -= deltaTime;
            if (untilBite > 0f) return false;
            untilBite += Every;
            Bites++;
            return true;
        }
    }
}
