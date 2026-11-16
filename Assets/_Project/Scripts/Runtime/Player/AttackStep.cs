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

        /// <summary>Which way the cut travels, for the arc it leaves in the air: +1 right to left, -1 left to right, 0 straight.</summary>
        public float Sweep;

        /// <summary>While this step is winding up or swinging, a blow whose damage is at or under this still hurts but does not interrupt. Red attacks ignore it.</summary>
        public float Poise;
        /// <summary>Above zero: the blow is a lane <see cref="Reach"/> long and this wide, straight ahead; the arc is ignored.</summary>
        public float LineWidth;
        /// <summary>Above zero: the blow is a disc of this radius centred <see cref="Reach"/> metres ahead.</summary>
        public float Radius;
        /// <summary>A target nearer than this takes damage and stagger times <see cref="InnerDamage"/>: the haft of a pole weapon.</summary>
        public float InnerReach;
        public float InnerDamage = 1f;
        /// <summary>A target at or beyond this takes these multiples: the tip.</summary>
        public float SweetFrom;
        public float SweetDamage = 1f;
        public float SweetStagger = 1f;
        /// <summary>More blows in the one step: seconds after the first hit, each at <see cref="RepeatScale"/> of it.</summary>
        public float[] Repeats;
        public float RepeatScale = 1f;
        /// <summary>The step forward (or back) is over by this many seconds; zero means the whole wind-up.</summary>
        public float LungeSeconds;
        /// <summary>Metres covered during the swing itself, striking all the way; each target is hit once.</summary>
        public float Travel;
        /// <summary>The player ignores enemies' bodies while travelling. A seam for the knives: not yet read.</summary>
        public bool PassThrough;
        /// <summary>Metres the player is pushed back over the first of the recovery.</summary>
        public float Recoil;
        /// <summary>Metres the target is dragged: positive toward the player, negative away.</summary>
        public float Pull;
        /// <summary>Against something too heavy to drag, the player goes to it instead. A seam for the chain: not yet read.</summary>
        public bool PullSelf;
        /// <summary>Something that leaves the weapon at the blow and flies on. Null: nothing does.</summary>
        public WeaponShot Shot;
        /// <summary>Stacks of the held element's affliction this blow lays by itself, with or without the tree's root.</summary>
        public int ElementStacks;
        /// <summary>What else the blow sets going.</summary>
        public StepEffect Effect;
        /// <summary>Degrees the effect's line is turned from the player's facing: a finisher whose body has walked round its target cuts from there.</summary>
        public float EffectTurn;
        /// <summary>A finisher's staging: moments before the blow that stop time, shake and spark but deal nothing.</summary>
        public FinisherBeat[] Beats;
        /// <summary>Seconds into the step at which the camera leans in for the held pose, and for how long; zero leaves it to the blow.</summary>
        public float HoldAt;
        public float HoldSeconds;
        /// <summary>Metres a finished enemy is thrown sideways, the way the cut travels: the hammer's last blow.</summary>
        public float Fling;
        /// <summary>
        /// A step whose shot stays where it lands may be followed: this long after the thing has stuck, the player
        /// goes to it over <see cref="FollowSeconds"/>, untouchable on the way, and arrives with this step. Null: no one follows.
        /// </summary>
        [NonSerialized] public AttackStep FollowStep;
        public float FollowDelay = 0.15f;
        public float FollowSeconds = 0.22f;

        /// <summary>What a blow at this distance is multiplied by: less on the haft, more at the tip.</summary>
        public float BandDamage(float distance) => SweetFrom > 0f && distance >= SweetFrom ? SweetDamage : distance < InnerReach ? InnerDamage : 1f;
        public float BandStagger(float distance) => SweetFrom > 0f && distance >= SweetFrom ? SweetStagger : distance < InnerReach ? InnerDamage : 1f;
        /// <summary>True when the blow at this distance fell in the tip's band.</summary>
        public bool IsSweet(float distance) => SweetFrom > 0f && distance >= SweetFrom;

        public float Total => Windup + Active + Recovery;
    }
}
