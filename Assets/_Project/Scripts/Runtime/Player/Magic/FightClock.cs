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
    /// A grudge held for a while (The Exile's Grudge): each enemy whose blow reaches you is tied to it, with what it dealt
    /// counted; when it closes, each is paid back. Plain C#: the caller says who struck and for how much, and what time it is.
    /// </summary>
    public sealed class HarmTally
    {
        public const int Most = 12;
        private readonly object[] who = new object[Most];
        private readonly float[] dealt = new float[Most];
        private float until = -1f;

        /// <summary>True while it counts.</summary>
        public bool Open { get; private set; }
        /// <summary>How many are tied to it.</summary>
        public int Count { get; private set; }

        /// <summary>Opens it afresh until <paramref name="closesAt"/>: no one tied yet.</summary>
        public void Begin(float closesAt)
        {
            Close();
            Open = true;
            until = closesAt;
        }

        /// <summary>Ties <paramref name="enemy"/> to it (once) and counts <paramref name="amount"/> more against it.</summary>
        public void Add(object enemy, float amount)
        {
            if (!Open || enemy == null) return;
            for (int i = 0; i < Count; i++)
            {
                if (!ReferenceEquals(who[i], enemy)) continue;
                dealt[i] += Math.Max(0f, amount);
                return;
            }
            if (Count >= Most) return;
            who[Count] = enemy;
            dealt[Count] = Math.Max(0f, amount);
            Count++;
        }

        public object Who(int index) => index >= 0 && index < Count ? who[index] : null;
        public float Dealt(int index) => index >= 0 && index < Count ? dealt[index] : 0f;

        /// <summary>True when it is open and its time is up.</summary>
        public bool Due(float now) => Open && now >= until;

        /// <summary>Forgets everyone tied to it.</summary>
        public void Close()
        {
            for (int i = 0; i < Count; i++) who[i] = null;
            Count = 0;
            Open = false;
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
