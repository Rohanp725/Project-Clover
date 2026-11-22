namespace Clover.UI.Toolkit
{
    /// <summary>
    /// One push, one move; a held push repeats after 0.32 s, then every 0.2 s (ui_rest_fire_plan.md 3.5.4 and 4.5). Fed
    /// the stick's value each frame with the real time passed (menus run with the world stopped). Plain C#, so tested.
    /// </summary>
    public sealed class NavRepeat
    {
        public const float FirstDelay = 0.32f;
        public const float Every = 0.2f;
        /// <summary>How far the stick must lean before it counts as a push.</summary>
        public const float Threshold = 0.5f;

        private int held = -1;
        private float untilNext;

        /// <summary>Forgets the held push (a screen opened, or focus taken by the pointer).</summary>
        public void Reset()
        {
            held = -1;
            untilNext = 0f;
        }

        /// <summary>True when this frame's stick makes a move, and which way. Up is positive y.</summary>
        public bool Step(float x, float y, float deltaSeconds, out NavDirection direction)
        {
            direction = NavDirection.Up;
            int now = -1;
            float ax = x < 0f ? -x : x, ay = y < 0f ? -y : y;
            if (ax >= Threshold || ay >= Threshold)
                now = ay >= ax ? (int)(y > 0f ? NavDirection.Up : NavDirection.Down) : (int)(x > 0f ? NavDirection.Right : NavDirection.Left);
            if (now != held)
            {
                held = now;
                untilNext = FirstDelay;
                if (now < 0) return false;
                direction = (NavDirection)now;
                return true;
            }
            if (now < 0) return false;
            untilNext -= deltaSeconds;
            if (untilNext > 0f) return false;
            untilNext += Every;
            if (untilNext <= 0f) untilNext = Every;
            direction = (NavDirection)now;
            return true;
        }
    }
}
