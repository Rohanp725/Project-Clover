using System;

namespace Clover.Player
{
    /// <summary>
    /// Health owed to the player and given back over time, not at once (Venom: a tenth of what the poison really dealt,
    /// its strength, ripening and sharing counted). Each amount banked is paid out over about <see cref="Seconds"/>, the
    /// owed health falling away smoothly. Plain, so tests build one.
    /// </summary>
    public sealed class Mending
    {
        /// <summary>About how long a sum banked takes to be given.</summary>
        public const float Seconds = 2f;
        /// <summary>Below this little is owed nothing more is given, so the tail does not run on for ever.</summary>
        public const float Least = 0.01f;

        /// <summary>Health still owed.</summary>
        public float Owed { get; private set; }

        /// <summary>Banks health to be given over time. Nothing for none.</summary>
        public void Add(float amount)
        {
            if (amount > 0f) Owed += amount;
        }

        /// <summary>The health given in <paramref name="deltaTime"/>; what is given is no longer owed.</summary>
        public float Take(float deltaTime)
        {
            if (Owed <= 0f || deltaTime <= 0f) return 0f;
            // A share of what is owed each moment, but at least a steady trickle, so a small sum ends rather than halves forever.
            float given = Math.Min(Owed, Math.Max(Owed * Math.Min(1f, deltaTime / Seconds), deltaTime * 0.5f));
            Owed -= given;
            if (Owed < Least)
            {
                given += Owed;
                Owed = 0f;
            }
            return given;
        }

        /// <summary>Forgets what is owed (a death, a new run).</summary>
        public void Clear() => Owed = 0f;
    }
}
