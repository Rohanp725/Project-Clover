namespace Clover.Player
{
    /// <summary>When the player counts as standing still (StatWhen.StoodStill; the ledger's stood-still), apart from the runner so it can be checked without a scene.</summary>
    public static class StillRules
    {
        /// <summary>Seconds of standing still that count.</summary>
        public const float Seconds = 1f;
        /// <summary>The most a body may move in one step, in metres, and still be standing still (a turn on the spot moves it a hair).</summary>
        public const float Hair = 0.03f;

        /// <summary>The seconds stood still after a step that moved the body this far (squared metres) in <paramref name="deltaTime"/>.</summary>
        public static float Next(float stoodFor, float movedSquared, float deltaTime) => movedSquared > Hair * Hair ? 0f : stoodFor + deltaTime;
    }
}
