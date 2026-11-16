using System;

namespace Clover.Player
{
    /// <summary>One attack of a weapon's chain: its timing, what it costs, and what it does when it lands.</summary>
    [Serializable]
    public class AttackStep
    {
        /// <summary>Also the name of the animation state that shows this attack.</summary>
        public string Name = "attack";
        /// <summary>Seconds before the hit; the player is committed and moves forward by <see cref="Lunge"/>.</summary>
        public float Windup = 0.1f;
        /// <summary>Seconds the swing is out after the hit is dealt.</summary>
        public float Active = 0.08f;
        /// <summary>Seconds after the swing in which the next attack of the chain, or a dodge, may start.</summary>
        public float Recovery = 0.24f;
        public float Damage = 10f;
        public float Stagger = 12f;
        public float StaminaCost = 12f;
        public float ManaGain = 6f;
        /// <summary>Metres the player steps forward during the wind-up.</summary>
        public float Lunge = 0.6f;
        /// <summary>Metres from the player's centre the swing reaches.</summary>
        public float Reach = 2.4f;
        /// <summary>Full width of the swing in degrees, centred on the player's facing.</summary>
        public float ArcDegrees = 130f;
        /// <summary>Seconds time nearly stops when this attack lands, which is most of what makes a hit feel heavy.</summary>
        public float HitStopSeconds = 0.05f;

        /// <summary>A heavy blow: it is the one that finishes a staggered enemy.</summary>
        public bool Heavy;
        /// <summary>How hard the camera shakes when this attack lands, in metres; zero for none.</summary>
        public float Shake;

        public float Total => Windup + Active + Recovery;
    }
}
