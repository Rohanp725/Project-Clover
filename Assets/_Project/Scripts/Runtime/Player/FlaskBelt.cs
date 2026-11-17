using System;

namespace Clover.Player
{
    /// <summary>
    /// The healing flasks. Drinking takes a moment and heals part-way through, so it has to be done at a safe time; a
    /// blow before that moment wastes nothing but the time. With every flask empty, dealing damage slowly earns one back.
    /// </summary>
    [Serializable]
    public class FlaskBelt
    {
        public int Maximum = 3;
        public float HealAmount = 45f;
        public float DrinkSeconds = 0.8f;
        /// <summary>Seconds into the drink at which the flask is used and the health comes.</summary>
        public float HealAt = 0.4f;
        /// <summary>Damage the player must deal, with no flasks left, to earn one back.</summary>
        public float RechargeDamage = 120f;
        /// <summary>How quickly damage dealt earns a flask back; the upgrades set it.</summary>
        [NonSerialized] public float EarnScale = 1f;

        public int Count { get; private set; }
        public bool IsDrinking { get; private set; }
        public float Progress => IsDrinking && DrinkSeconds > 0f ? Math.Min(1f, elapsed / DrinkSeconds) : 0f;
        /// <summary>How far the player has come toward earning a flask back, from 0 to 1; 0 while any flask is left.</summary>
        public float RechargeFraction => Count == 0 && RechargeDamage > 0f ? Math.Min(1f, earned / RechargeDamage) : 0f;

        private float elapsed;
        private float earned;
        private bool healed;

        /// <summary>Changes how many flasks are carried. One more comes full; one fewer takes a full one if there is no room for it.</summary>
        public void SetMaximum(int maximum)
        {
            if (maximum == Maximum) return;
            int gained = maximum - Maximum;
            Maximum = maximum;
            Count = Math.Max(0, Math.Min(Maximum, gained > 0 ? Count + gained : Count));
        }

        public void Refill()
        {
            Count = Maximum;
            IsDrinking = false;
            elapsed = 0f;
            earned = 0f;
            healed = false;
        }

        public bool TryStartDrink()
        {
            if (IsDrinking || Count <= 0) return false;
            IsDrinking = true;
            elapsed = 0f;
            healed = false;
            return true;
        }

        /// <summary>Advances a drink. Returns true on the tick the flask is used: heal then.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsDrinking) return false;
            elapsed += deltaTime;
            bool healNow = false;
            if (!healed && elapsed >= HealAt)
            {
                healed = true;
                Count--;
                healNow = true;
            }
            if (elapsed >= DrinkSeconds) IsDrinking = false;
            return healNow;
        }

        /// <summary>A full flask leaves the belt without being drunk, as when one of the thirsting snatches it. False when there is none.</summary>
        public bool TryTake()
        {
            if (Count <= 0) return false;
            Count--;
            return true;
        }

        /// <summary>A flask that was taken comes back, if there is room for it.</summary>
        public void PutBack()
        {
            if (Count < Maximum) Count++;
        }

        public void Interrupt() => IsDrinking = false;

        /// <summary>Uses a flask up without drinking it, as throwing it does. False when there is none, or one is at the lips.</summary>
        public bool TryThrow()
        {
            if (IsDrinking || Count <= 0) return false;
            Count--;
            return true;
        }

        /// <summary>Counts damage dealt toward a new flask. Returns true when one is earned.</summary>
        /// <summary>Fills part of a flask (a share of one). True when that made a whole one and there was room for it on the belt.</summary>
        public bool Fill(float share)
        {
            if (share <= 0f || Count >= Maximum || RechargeDamage <= 0f) return false;
            earned += share * RechargeDamage;
            if (earned < RechargeDamage) return false;
            earned -= RechargeDamage;
            Count++;
            return true;
        }

        public bool EarnFromDamage(float damage)
        {
            if (Count > 0 || Maximum <= 0 || damage <= 0f) return false;
            earned += damage * EarnScale;
            if (earned < RechargeDamage) return false;
            earned = 0f;
            Count = 1;
            return true;
        }
    }
}
