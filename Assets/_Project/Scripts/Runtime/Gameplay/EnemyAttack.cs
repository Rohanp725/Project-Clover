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
        public string Name = "attack";
        /// <summary>The animation state that shows this attack; the attack's own name when left empty.</summary>
        public string Animation = "";
        public string AnimationName => string.IsNullOrEmpty(Animation) ? Name : Animation;
        /// <summary>Seconds the attack is shown before it lands: the time the player has to answer it.</summary>
        public float Telegraph = 0.8f;
        /// <summary>Seconds of follow-through after the blow, then seconds before the enemy can act again.</summary>
        public float Strike = 0.15f;
        public float Recovery = 0.6f;
        public float Damage = 20f;
        /// <summary>For a wedge: metres it reaches and its full width in degrees.</summary>
        public float Reach = 3f;
        public float ArcDegrees = 60f;
        /// <summary>Above zero, the attack is a straight lane this wide and <see cref="Reach"/> long, not a wedge.</summary>
        public float LineWidth;
        /// <summary>The attacker passes through the target as it drives forward, ending up behind it.</summary>
        public bool PassThrough;
        /// <summary>Above zero, the attacker vanishes as the attack begins and reappears this far from the target, to one side or behind.</summary>
        public float BlinkTo;
        /// <summary>The attack is only used from this phase of the fight on.</summary>
        public int MinPhase;
        /// <summary>Above zero, the attack is a circle of this radius instead of a wedge.</summary>
        public float Radius;
        /// <summary>A circle thrown onto the spot where the target stood when the attack began, not around the enemy.</summary>
        public bool Ranged;
        /// <summary>
        /// A circle marked where the target stood when the attack began, which the attacker jumps to and lands beside
        /// as the blow falls. Unlike a thrown circle it is a blow struck by hand: a perfect guard turns it back on him.
        /// </summary>
        public bool Leap;
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

        /// <summary>Chosen only while the target is found standing in the sun (the sun-blind priest's ray).</summary>
        public bool OnlyLit;
        /// <summary>Above zero: chosen only while the target carries at least this many flasks.</summary>
        public int NeedsFlasks;
        /// <summary>A blow that lands takes a flask from the target's belt, and the attacker stops to drink it.</summary>
        public bool Snatches;
        /// <summary>A blow that lands takes hold of the target.</summary>
        public bool Grabs;
        /// <summary>Above zero: the circle goes on burning for this long after the blow, biting whoever stands in it.</summary>
        public float BurnSeconds;
        public float BurnDamage = 4f;
        public float BurnEvery = 0.5f;
        /// <summary>Above zero: the circle counts as sunlit for this long after the blow.</summary>
        public float LightsSeconds;
        /// <summary>The blow is shown as fire and not as lightning.</summary>
        public bool Fiery;
        /// <summary>The attacker bursts with the blow and is gone (the ashen): its <see cref="AshenBurst"/> does the rest.</summary>
        public bool Bursts;
        /// <summary>The blow pours molten gold along the edge of its shape, a low wall that stands for a while (the warden).</summary>
        public bool Pours;
        /// <summary>The blow is a beam of reflected sun: the lane it falls on counts as sunlit for a while after.</summary>
        public bool Beams;
        /// <summary>The weapon is thrown with the blow and stands where it lands until the attacker fetches it (the spearman): its <see cref="SpearCast"/> does the rest.</summary>
        public bool Casts;

        public float Total => Telegraph + Strike + Recovery;
        public bool IsCircle => Radius > 0f;
        public bool IsLine => LineWidth > 0f && Radius <= 0f;
    }
}
