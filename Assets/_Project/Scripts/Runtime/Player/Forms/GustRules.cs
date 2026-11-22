namespace Clover.Player.Forms
{
    /// <summary>The plain rules of Gust, Wind's rush of air: how hard it throws what it reaches.</summary>
    public static class GustRules
    {
        /// <summary>Metres the wind throws a body that nothing else is moving.</summary>
        public const float Throw = 3f;
        /// <summary>A shove is spent over a moment and moves a body two thirds of its length.</summary>
        public const float ShoveTravels = 2f / 3f;
        /// <summary>The whole shove the wind gives, spread over the time it blows, so the body travels <see cref="Throw"/>.</summary>
        public const float Impulse = Throw / ShoveTravels;

        /// <summary>
        /// The kind of the wind's push once <paramref name="blownSoFar"/> of it (0 to 1) has been given: the first step is
        /// the shove each body feels once, every later step only carries it on.
        /// </summary>
        public static Clover.Gameplay.PushKind KindAt(float blownSoFar) => blownSoFar <= 0f ? Clover.Gameplay.PushKind.Shove : Clover.Gameplay.PushKind.Drag;
    }
}
