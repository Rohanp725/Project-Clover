using System;

namespace Clover.Player
{
    /// <summary>
    /// Heat: Fire's reward for staying in the fight. Blows landed build it, a level at a time; each level adds damage.
    /// Being struck puts it out, and so does standing off for too long.
    /// </summary>
    [Serializable]
    public class Heat
    {
        public int MaximumLevel = 5;
        /// <summary>Blows landed for each level.</summary>
        public int BlowsPerLevel = 3;
        /// <summary>Damage added by each level, as a share.</summary>
        public float DamagePerLevel = 0.06f;
        /// <summary>Seconds without landing a blow before a level is lost.</summary>
        public float CoolAfter = 6f;

        public int Level { get; private set; }
        public float DamageMultiplier => 1f + DamagePerLevel * Level;
        /// <summary>How far toward the next level, from 0 to 1.</summary>
        public float Progress => Level >= MaximumLevel || BlowsPerLevel <= 0 ? 0f : blows / (float)BlowsPerLevel;

        private int blows;
        private float sinceBlow;

        public void Reset()
        {
            Level = 0;
            blows = 0;
            sinceBlow = 0f;
        }

        /// <summary>A blow landed. Returns true when it raised the level.</summary>
        public bool Landed()
        {
            sinceBlow = 0f;
            if (Level >= MaximumLevel) return false;
            blows++;
            if (blows < BlowsPerLevel) return false;
            blows = 0;
            Level++;
            return true;
        }

        /// <summary>Raises the heat by whole levels at once, up to its most: what a drive through enemies gives for each one passed.</summary>
        public void Raise(int levels)
        {
            if (levels <= 0) return;
            sinceBlow = 0f;
            Level = System.Math.Min(MaximumLevel, Level + levels);
            if (Level >= MaximumLevel) blows = 0;
        }

        /// <summary>The player was struck: the heat is gone.</summary>
        public void Struck()
        {
            Level = 0;
            blows = 0;
        }

        /// <summary>The surge fills it at once.</summary>
        public void Fill()
        {
            Level = MaximumLevel;
            blows = 0;
            sinceBlow = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Level <= 0 && blows <= 0) return;
            sinceBlow += deltaTime;
            if (sinceBlow < CoolAfter) return;
            sinceBlow = 0f;
            blows = 0;
            if (Level > 0) Level--;
        }
    }
}
