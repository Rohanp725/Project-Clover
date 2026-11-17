using System;
using Clover.Core;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// One staged moment of a finisher before its real blow: time stops for an instant, the camera shakes, something
    /// sparks on the target, and the target may be moved. It deals nothing.
    /// </summary>
    [Serializable]
    public class FinisherBeat
    {
        /// <summary>Seconds into the step.</summary>
        public float Time;
        public float HitStopSeconds = 0.03f;
        public float Shake;
        public VfxKind Effect = VfxKind.HitSpark;
        /// <summary>Metres the target is shoved, in the player's space (z is away from the player). Targets that cannot be shoved stay put.</summary>
        public Vector3 Shove;
        /// <summary>Seconds the target is thrown into the air at this beat; zero leaves it on the ground.</summary>
        public float Lift;
        /// <summary>
        /// Above zero: at this beat the player is carried through the target to a point this many metres beyond it, on
        /// the bearing given (degrees from the way the player faced when the step began), over <see cref="DashSeconds"/>,
        /// leaving a line of light through the target that hangs until the blow.
        /// </summary>
        public float DashBeyond;
        public float DashBearing;
        public float DashSeconds = 0.1f;
    }
}
