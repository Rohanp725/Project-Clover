using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// Whether the player is in a fight (depth_elements.md): a fight begins when an enemy within <see cref="FightRange"/>
    /// is aware of the player, and ends once none has been for <see cref="CalmSeconds"/>. The caller samples the enemies
    /// and says whether one is aware and near; this keeps the time.
    /// </summary>
    [Serializable]
    public class FightClock
    {
        /// <summary>Metres within which an aware enemy counts. The caller reads it when it looks.</summary>
        public float FightRange = 12f;
        public float CalmSeconds = 8f;

        public bool InFight { get; private set; }
        /// <summary>How many fights have begun on this clock: the fight a once-a-fight effect last went off in is told by it.</summary>
        public int Fights { get; private set; }
        /// <summary>Seconds since an aware enemy was last near, while in a fight.</summary>
        public float Calm { get; private set; }

        /// <summary>
        /// Runs the clock on. True on the tick a fight begins, so the caller can do what a fight's start does (mana is
        /// brought up to half).
        /// </summary>
        public bool Tick(float deltaTime, bool awareEnemyNear)
        {
            if (awareEnemyNear)
            {
                Calm = 0f;
                if (InFight) return false;
                InFight = true;
                Fights++;
                return true;
            }
            if (!InFight) return false;
            Calm += Math.Max(0f, deltaTime);
            if (Calm >= CalmSeconds) InFight = false;
            return false;
        }

        /// <summary>Out of any fight at once, as a rest, a fall or a new scene leaves the player.</summary>
        public void Reset()
        {
            InFight = false;
            Calm = 0f;
        }
    }

    /// <summary>
    /// The hour a mark keeps in a fight (Keeping the Hour): it fills over its seconds while a fight is on, stays full until a
    /// heavy blow spends it, and empties when the fight ends. Plain C#: the caller says how long an hour is and whether a fight is on.
    /// </summary>
    public sealed class HourClock
    {
        /// <summary>Seconds filled toward the hour.</summary>
        public float Filled { get; private set; }
        /// <summary>True while the hour is up and waits for its heavy blow.</summary>
        public bool Ready { get; private set; }

        /// <summary>Runs it on; true on the tick the hour comes up.</summary>
        public bool Tick(float deltaTime, float every, bool inFight)
        {
            if (every <= 0f || !inFight)
            {
                Filled = 0f;
                Ready = false;
                return false;
            }
            if (Ready) return false;
            Filled += Math.Max(0f, deltaTime);
            if (Filled < every) return false;
            Filled = every;
            Ready = true;
            return true;
        }

        /// <summary>How full it is, 0 to 1, for the dial.</summary>
        public float Share(float every) => every > 0f ? Math.Min(1f, Filled / every) : 0f;

        /// <summary>The heavy blow on the hour landed: it starts filling again.</summary>
        public void Spend()
        {
            Filled = 0f;
            Ready = false;
        }
    }
}
