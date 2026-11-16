using System;

namespace Clover.Player
{
    /// <summary>
    /// The guard. Held, it blocks a blow for stamina and lets a little damage through. Pressed just before a blow lands,
    /// it is a perfect guard and costs nothing. Pressing again too soon gives no perfect window, so the button cannot be
    /// hammered in the hope of one. Plain C#: the player says when the button is pressed and when the guard is up.
    /// </summary>
    [Serializable]
    public class Guard
    {
        /// <summary>Seconds after a press in which a blow is guarded perfectly.</summary>
        public float PerfectWindow = 0.18f;
        /// <summary>Seconds after a press before the perfect window opens: a guard that is slow to rise (the hammer's).</summary>
        public float PerfectFrom;
        /// <summary>A press this soon after the last one has no perfect window.</summary>
        public float SpamLockout = 0.4f;
        /// <summary>Stamina a block costs for each point of the blow's damage.</summary>
        public float StaminaPerDamage = 0.9f;
        /// <summary>Share of a blocked blow's damage that still comes through, as grey health.</summary>
        public float ChipFraction = 0.35f;
        /// <summary>Full width in degrees, centred on the player's facing, that the guard covers. Blows from outside it land.</summary>
        public float ArcDegrees = 200f;

        public bool IsRaised { get; private set; }
        public bool IsPerfect => IsRaised && perfectArmed && sincePress >= PerfectFrom && sincePress <= PerfectWindow;

        private float sincePress = float.MaxValue;
        private bool perfectArmed;

        public void Reset()
        {
            IsRaised = false;
            sincePress = float.MaxValue;
            perfectArmed = false;
        }

        /// <summary>
        /// The button went down. The guard itself comes up when the player is free to raise it. A press made while the
        /// player could not have guarded at all (flinching, drinking) passes false: it earns no perfect window.
        /// </summary>
        public void Press(bool canBePerfect = true)
        {
            perfectArmed = canBePerfect && sincePress >= SpamLockout;
            sincePress = 0f;
        }

        public void SetRaised(bool raised) => IsRaised = raised;

        /// <summary>Whether the guard faces a blow whose attacker stands this many degrees off the player's facing.</summary>
        public bool Covers(float degreesFromFacing) => degreesFromFacing <= ArcDegrees * 0.5f;

        public void Tick(float deltaTime)
        {
            if (sincePress < float.MaxValue) sincePress += deltaTime;
        }

        /// <summary>Settles one blow. <paramref name="chip"/> is the damage that comes through a block.</summary>
        public GuardResult Resolve(float damage, bool unblockable, StaminaPool stamina, out float chip)
        {
            chip = 0f;
            if (!IsRaised) return GuardResult.NotGuarded;
            // A perfect guard turns even a blow that cannot be blocked.
            if (IsPerfect) return GuardResult.Perfect;
            if (unblockable) return GuardResult.NotGuarded;
            if (!stamina.TrySpend(damage * StaminaPerDamage))
            {
                stamina.Empty();
                return GuardResult.Broken;
            }
            chip = damage * ChipFraction;
            return GuardResult.Blocked;
        }
    }
}
