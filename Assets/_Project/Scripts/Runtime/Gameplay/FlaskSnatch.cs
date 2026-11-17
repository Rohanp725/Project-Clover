using System;
using Clover.Player;

namespace Clover.Gameplay
{
    /// <summary>
    /// A flask in the hands of one of the thirsting. Taken from the belt, it is drunk over a moment; struck down or
    /// thrown off balance before the moment is up, the thirsting gives it back, and otherwise it is gone until the
    /// next rest.
    /// </summary>
    [Serializable]
    public class FlaskSnatch
    {
        public float DrinkSeconds = 1.6f;

        public bool Holding { get; private set; }
        public float Progress => Holding && DrinkSeconds > 0f ? Math.Min(1f, elapsed / DrinkSeconds) : 0f;

        private float elapsed;

        /// <summary>Takes one full flask off the belt. False when there was none to take.</summary>
        public bool Take(FlaskBelt belt)
        {
            if (Holding || belt == null || !belt.TryTake()) return false;
            Holding = true;
            elapsed = 0f;
            return true;
        }

        /// <summary>Advances the drink. True on the tick it is finished: the flask is gone.</summary>
        public bool Tick(float deltaTime)
        {
            if (!Holding) return false;
            elapsed += deltaTime;
            if (elapsed < DrinkSeconds) return false;
            Holding = false;
            return true;
        }

        /// <summary>The flask goes back on the belt, as when the thirsting is killed or broken mid-drink. False when it held none.</summary>
        public bool GiveBack(FlaskBelt belt)
        {
            if (!Holding) return false;
            Holding = false;
            if (belt != null) belt.PutBack();
            return true;
        }

        public void Drop() => Holding = false;
    }
}
