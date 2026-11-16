using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>How a pose is arrived at from the one before it.</summary>
    public enum Ease
    {
        Linear,
        /// <summary>Starts slowly and arrives at speed: a blow coming in.</summary>
        In,
        /// <summary>Leaves at speed and settles: a follow-through.</summary>
        Out,
        InOut,
        /// <summary>Arrives almost at once, then eases the last of the way: a snap.</summary>
        Snap,
        /// <summary>Goes a little past the pose and comes back to it.</summary>
        Overshoot,
        /// <summary>Stays on the pose before until the very end.</summary>
        Hold,
    }

    /// <summary>
    /// One pose of a body, written in the character's own space: x to its right, y up, z ahead, metres, with the origin
    /// on the ground under it. Feet and hands are given as places to be; the rig works out the knees and elbows. A new
    /// pose is a relaxed stand, so a clip only says what differs.
    /// </summary>
    public class ForgePose
    {
        /// <summary>The hips, moved from where they rest.</summary>
        public Vector3 Hips;
        /// <summary>Degrees: lean is forward, yaw is to the right, roll tips toward the left side.</summary>
        public float HipLean, HipYaw, HipRoll;
        /// <summary>The spine and chest together, on top of the hips.</summary>
        public float WaistLean, WaistYaw, WaistRoll;
        /// <summary>The neck and head together, on top of the chest.</summary>
        public float HeadLean, HeadYaw, HeadRoll;

        /// <summary>Where each foot stands: x and z on the ground, y how far it is lifted.</summary>
        public Vector3 FootL = new Vector3(-0.13f, 0f, 0f), FootR = new Vector3(0.13f, 0f, 0f);
        /// <summary>Degrees each foot is turned out (positive is toward its own side) and tipped toe-down.</summary>
        public float FootLTurn = 8f, FootRTurn = 8f, FootLTip, FootRTip;
        /// <summary>Which way each knee points.</summary>
        public Vector3 KneeL = new Vector3(-0.2f, 0f, 1f), KneeR = new Vector3(0.2f, 0f, 1f);

        /// <summary>Where each hand is.</summary>
        public Vector3 HandL = new Vector3(-0.3f, 0.86f, 0.06f), HandR = new Vector3(0.3f, 0.86f, 0.06f);
        /// <summary>Which way each elbow points.</summary>
        public Vector3 ElbowL = new Vector3(-0.5f, -0.4f, -0.8f), ElbowR = new Vector3(0.5f, -0.4f, -0.8f);
        /// <summary>How far each shoulder is raised (y) and brought forward (z), in degrees of its own swing.</summary>
        public float ShoulderLUp, ShoulderRUp, ShoulderLForward, ShoulderRForward;

        /// <summary>When true the right hand is turned so the weapon in it points along <see cref="Blade"/>.</summary>
        public bool AimBlade;
        public Vector3 Blade = Vector3.down;
        /// <summary>Degrees the blade is rolled about its own length.</summary>
        public float BladeRoll;
        /// <summary>When true the left hand holds the weapon's grip below the right hand.</summary>
        public bool TwoHanded;
        /// <summary>Metres down the grip from the right hand that the left hand holds.</summary>
        public float GripGap = 0.16f;
        /// <summary>Degrees the left hand is turned about the forearm and bent back at the wrist, when it holds nothing.</summary>
        public float HandLTwist, HandLBend, HandRTwist, HandRBend;

        public ForgePose Copy() => (ForgePose)MemberwiseClone();

        /// <summary>The pose part-way from one to another. Hands travel on an arc about the chest, not a straight line through it.</summary>
        public static ForgePose Blend(ForgePose a, ForgePose b, float t)
        {
            var p = new ForgePose
            {
                Hips = Vector3.LerpUnclamped(a.Hips, b.Hips, t),
                HipLean = Mathf.LerpUnclamped(a.HipLean, b.HipLean, t),
                HipYaw = Mathf.LerpUnclamped(a.HipYaw, b.HipYaw, t),
                HipRoll = Mathf.LerpUnclamped(a.HipRoll, b.HipRoll, t),
                WaistLean = Mathf.LerpUnclamped(a.WaistLean, b.WaistLean, t),
                WaistYaw = Mathf.LerpUnclamped(a.WaistYaw, b.WaistYaw, t),
                WaistRoll = Mathf.LerpUnclamped(a.WaistRoll, b.WaistRoll, t),
                HeadLean = Mathf.LerpUnclamped(a.HeadLean, b.HeadLean, t),
                HeadYaw = Mathf.LerpUnclamped(a.HeadYaw, b.HeadYaw, t),
                HeadRoll = Mathf.LerpUnclamped(a.HeadRoll, b.HeadRoll, t),
                FootL = Step(a.FootL, b.FootL, t),
                FootR = Step(a.FootR, b.FootR, t),
                FootLTurn = Mathf.LerpUnclamped(a.FootLTurn, b.FootLTurn, t),
                FootRTurn = Mathf.LerpUnclamped(a.FootRTurn, b.FootRTurn, t),
                FootLTip = Mathf.LerpUnclamped(a.FootLTip, b.FootLTip, t),
                FootRTip = Mathf.LerpUnclamped(a.FootRTip, b.FootRTip, t),
                KneeL = Vector3.LerpUnclamped(a.KneeL, b.KneeL, t),
                KneeR = Vector3.LerpUnclamped(a.KneeR, b.KneeR, t),
                HandL = Arc(a.HandL, b.HandL, a.Hips, b.Hips, t),
                HandR = Arc(a.HandR, b.HandR, a.Hips, b.Hips, t),
                ElbowL = Vector3.LerpUnclamped(a.ElbowL, b.ElbowL, t),
                ElbowR = Vector3.LerpUnclamped(a.ElbowR, b.ElbowR, t),
                ShoulderLUp = Mathf.LerpUnclamped(a.ShoulderLUp, b.ShoulderLUp, t),
                ShoulderRUp = Mathf.LerpUnclamped(a.ShoulderRUp, b.ShoulderRUp, t),
                ShoulderLForward = Mathf.LerpUnclamped(a.ShoulderLForward, b.ShoulderLForward, t),
                ShoulderRForward = Mathf.LerpUnclamped(a.ShoulderRForward, b.ShoulderRForward, t),
                AimBlade = t < 0.5f ? a.AimBlade : b.AimBlade,
                Blade = Vector3.SlerpUnclamped(a.Blade.normalized, b.Blade.normalized, t),
                BladeRoll = Mathf.LerpUnclamped(a.BladeRoll, b.BladeRoll, t),
                TwoHanded = t < 0.5f ? a.TwoHanded : b.TwoHanded,
                GripGap = Mathf.LerpUnclamped(a.GripGap, b.GripGap, t),
                HandLTwist = Mathf.LerpUnclamped(a.HandLTwist, b.HandLTwist, t),
                HandLBend = Mathf.LerpUnclamped(a.HandLBend, b.HandLBend, t),
                HandRTwist = Mathf.LerpUnclamped(a.HandRTwist, b.HandRTwist, t),
                HandRBend = Mathf.LerpUnclamped(a.HandRBend, b.HandRBend, t),
            };
            return p;
        }

        // A foot that changes place is picked up on the way, not dragged.
        private static Vector3 Step(Vector3 a, Vector3 b, float t)
        {
            Vector3 p = Vector3.LerpUnclamped(a, b, t);
            float travel = new Vector2(b.x - a.x, b.z - a.z).magnitude;
            float clamped = Mathf.Clamp01(t);
            if (travel > 0.12f) p.y += Mathf.Sin(clamped * Mathf.PI) * Mathf.Min(0.16f, travel * 0.3f);
            return p;
        }

        // The hand swings about a point at the chest: direction turns, distance stretches.
        private static Vector3 Arc(Vector3 a, Vector3 b, Vector3 hipsA, Vector3 hipsB, float t)
        {
            Vector3 pivotA = new Vector3(0f, 1.3f, 0f) + hipsA, pivotB = new Vector3(0f, 1.3f, 0f) + hipsB;
            Vector3 fromA = a - pivotA, fromB = b - pivotB;
            if (fromA.sqrMagnitude < 1e-6f || fromB.sqrMagnitude < 1e-6f) return Vector3.LerpUnclamped(a, b, t);
            Vector3 direction = Vector3.SlerpUnclamped(fromA.normalized, fromB.normalized, t);
            float distance = Mathf.LerpUnclamped(fromA.magnitude, fromB.magnitude, t);
            return Vector3.LerpUnclamped(pivotA, pivotB, t) + direction * distance;
        }

        public static float Apply(Ease ease, float t)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case Ease.In: return t * t * t;
                case Ease.Out: return 1f - (1f - t) * (1f - t) * (1f - t);
                case Ease.InOut: return t * t * (3f - 2f * t);
                case Ease.Snap: return 1f - Mathf.Pow(1f - t, 5f);
                case Ease.Overshoot:
                    const float c = 1.9f;
                    float u = t - 1f;
                    return 1f + (c + 1f) * u * u * u + c * u * u;
                case Ease.Hold: return t >= 1f ? 1f : 0f;
                default: return t;
            }
        }

        /// <summary>Puts the rig in this pose.</summary>
        public void ApplyTo(ForgeRig rig)
        {
            rig.Rest();
            var hips = rig["hips"];
            hips.Transform.position = hips.RestPosition + Hips;
            rig.TurnAbsolute("hips", Quaternion.Euler(HipLean, HipYaw, HipRoll));
            rig.TurnOnParent("spine", Quaternion.Euler(WaistLean * 0.4f, WaistYaw * 0.4f, WaistRoll * 0.4f));
            rig.TurnOnParent("chest", Quaternion.Euler(WaistLean * 0.6f, WaistYaw * 0.6f, WaistRoll * 0.6f));
            rig.TurnOnParent("neck", Quaternion.Euler(HeadLean * 0.4f, HeadYaw * 0.4f, HeadRoll * 0.4f));
            rig.TurnOnParent("head", Quaternion.Euler(HeadLean * 0.6f, HeadYaw * 0.6f, HeadRoll * 0.6f));

            float ankle = rig["foot.L"].RestPosition.y;
            rig.Reach("thigh.L", "shin.L", "foot.L", FootL + Vector3.up * ankle, KneeL);
            rig.TurnAbsolute("foot.L", Quaternion.Euler(FootLTip, -FootLTurn, 0f));
            rig.Reach("thigh.R", "shin.R", "foot.R", FootR + Vector3.up * ankle, KneeR);
            rig.TurnAbsolute("foot.R", Quaternion.Euler(FootRTip, FootRTurn, 0f));

            rig.TurnOnParent("shoulder.R", Quaternion.Euler(0f, -ShoulderRForward, ShoulderRUp));
            rig.TurnOnParent("shoulder.L", Quaternion.Euler(0f, ShoulderLForward, -ShoulderLUp));
            rig.Reach("upperarm.R", "forearm.R", "hand.R", HandR, ElbowR);
            Vector3 blade = Blade.sqrMagnitude > 1e-6f ? Blade.normalized : Vector3.down;
            if (AimBlade)
            {
                rig.TurnAbsolute("hand.R", Quaternion.AngleAxis(BladeRoll, blade) * Quaternion.FromToRotation(rig.RestBlade, blade));
            }
            else
            {
                Vector3 forearm = (rig["hand.R"].Transform.position - rig["forearm.R"].Transform.position).normalized;
                rig.TurnOnParent("hand.R", Quaternion.AngleAxis(HandRTwist, forearm) * Quaternion.AngleAxis(HandRBend, Vector3.Cross(forearm, Vector3.up)));
            }

            Vector3 leftTarget = TwoHanded && AimBlade ? rig["hand.R"].Transform.position - blade * GripGap : HandL;
            rig.Reach("upperarm.L", "forearm.L", "hand.L", leftTarget, ElbowL);
            Vector3 forearmL = (rig["hand.L"].Transform.position - rig["forearm.L"].Transform.position).normalized;
            rig.TurnOnParent("hand.L", Quaternion.AngleAxis(HandLTwist, forearmL) * Quaternion.AngleAxis(HandLBend, Vector3.Cross(forearmL, Vector3.up)));
        }
    }
}
