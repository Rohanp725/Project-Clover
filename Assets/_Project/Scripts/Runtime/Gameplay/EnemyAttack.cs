using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// One attack an enemy can make: how long it is shown before it lands, what ground it covers, and what it does.
    /// The shape is a wedge in front of the enemy, or a circle (around the enemy, or thrown onto where the target stood).
    /// </summary>
    [Serializable]
    public class EnemyAttack
    {
        /// <summary>Also the name of the animation state that shows this attack.</summary>
        public string Name = "attack";
        /// <summary>Seconds the attack is shown before it lands: the time the player has to answer it.</summary>
        public float Telegraph = 0.8f;
        /// <summary>Seconds of follow-through after the blow, then seconds before the enemy can act again.</summary>
        public float Strike = 0.15f;
        public float Recovery = 0.6f;
        public float Damage = 20f;
        /// <summary>For a wedge: metres it reaches and its full width in degrees.</summary>
        public float Reach = 3f;
        public float ArcDegrees = 60f;
        /// <summary>Above zero, the attack is a circle of this radius instead of a wedge.</summary>
        public float Radius;
        /// <summary>A circle thrown onto the spot where the target stood when the attack began, not around the enemy.</summary>
        public bool Ranged;
        /// <summary>Metres the enemy drives forward as it strikes.</summary>
        public float Lunge;
        /// <summary>The attack is chosen only with the target between these distances.</summary>
        public float MinRange;
        public float MaxRange = 3f;
        /// <summary>A red attack: holding guard does not stop it.</summary>
        public bool Unblockable;
        /// <summary>Which way the blow travels, for the arc it leaves: +1 right to left, -1 left to right, 0 straight.</summary>
        public float Sweep;
        /// <summary>Seconds before this attack can be chosen again.</summary>
        public float Cooldown;
        /// <summary>The attack that follows at once, with no rest between; empty for none.</summary>
        public string ChainInto = "";
        /// <summary>The enemy stops turning to follow the target this far through the telegraph, so a late sidestep gets clear.</summary>
        public float AimLockAt = 0.65f;

        public float Total => Telegraph + Strike + Recovery;
        public bool IsCircle => Radius > 0f;
    }
}
