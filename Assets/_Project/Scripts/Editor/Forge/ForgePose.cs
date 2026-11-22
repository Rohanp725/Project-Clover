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
        /// <summary>
        /// When set, the way the back of the weapon faces (its edge, or a scythe's crescent, faces the other way). It
        /// fixes the roll outright, so a sweep can say "edge leading" without counting degrees. Zero: the shortest turn from rest.
        /// </summary>
        public Vector3 BladeBack;
        /// <summary>
        /// When true the left hand is turned so the weapon in it (a second knife) points along <see cref="BladeL"/>,
        /// with its back toward <see cref="BladeLBack"/> when that is set. Ignored while the left hand is on the right hand's grip.
        /// </summary>
        public bool AimBladeL;
        public Vector3 BladeL = Vector3.down;
        public Vector3 BladeLBack;
        /// <summary>
        /// Zero: the weapon lies along the hand, out past the knuckles (a sword's hilt in the fist), and the hand is
        /// turned however its blade must point. Above zero: degrees between the hand's length and a short blade that
        /// crosses the palm (the knives, <see cref="WeaponBuilder.KnifeGripAngle"/>). The hand then stays near the line
        /// of the forearm, bent at the wrist by no more than <see cref="WristBend"/>, the blade comes as near the asked
        /// way as that allows, and the forearm takes part of the hand's turn about it so the wrist does not wring.
        /// </summary>
        public float GripAngle;
        /// <summary>Degrees the knife hands are rolled about the forearm after aiming: a flourish of the wrist.</summary>
        public float WristRollR, WristRollL;
        /// <summary>The furthest a hand holding a cross-gripped blade bends from the line of its forearm.</summary>
        public const float WristBend = 45f;
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
                BladeBack = a.BladeBack.sqrMagnitude < 1e-6f || b.BladeBack.sqrMagnitude < 1e-6f ? (t < 0.5f ? a.BladeBack : b.BladeBack)
                    : Vector3.SlerpUnclamped(a.BladeBack.normalized, b.BladeBack.normalized, t),
                AimBladeL = t < 0.5f ? a.AimBladeL : b.AimBladeL,
                BladeL = Vector3.SlerpUnclamped(a.BladeL.normalized, b.BladeL.normalized, t),
                BladeLBack = a.BladeLBack.sqrMagnitude < 1e-6f || b.BladeLBack.sqrMagnitude < 1e-6f ? (t < 0.5f ? a.BladeLBack : b.BladeLBack)
                    : Vector3.SlerpUnclamped(a.BladeLBack.normalized, b.BladeLBack.normalized, t),
                GripAngle = Mathf.LerpUnclamped(a.GripAngle, b.GripAngle, t),
                WristRollR = Mathf.LerpUnclamped(a.WristRollR, b.WristRollR, t),
                WristRollL = Mathf.LerpUnclamped(a.WristRollL, b.WristRollL, t),
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
            if (AimBlade && GripAngle > 0f)
            {
                AimAcross(rig, "upperarm.R", "forearm.R", "hand.R", rig.RestBlade, rig.RestBladeUp, blade, ElbowR, WristRollR);
            }
            else if (AimBlade)
            {
                Vector3 back = Vector3.ProjectOnPlane(BladeBack, blade);
                Quaternion aim = back.sqrMagnitude > 1e-5f
                    ? Quaternion.LookRotation(blade, back) * Quaternion.Inverse(Quaternion.LookRotation(rig.RestBlade, rig.RestBladeUp))
                    : Quaternion.FromToRotation(rig.RestBlade, blade);
                rig.TurnAbsolute("hand.R", Quaternion.AngleAxis(BladeRoll, blade) * aim);
            }
            else
            {
                Vector3 forearm = (rig["hand.R"].Transform.position - rig["forearm.R"].Transform.position).normalized;
                rig.TurnOnParent("hand.R", Quaternion.AngleAxis(HandRTwist, forearm) * Quaternion.AngleAxis(HandRBend, Vector3.Cross(forearm, Vector3.up)));
            }

            Vector3 leftTarget = TwoHanded && AimBlade ? rig["hand.R"].Transform.position - blade * GripGap : HandL;
            rig.Reach("upperarm.L", "forearm.L", "hand.L", leftTarget, ElbowL);
            if (AimBladeL && !(TwoHanded && AimBlade))
            {
                // The left hand's weapon lies along the right's rest line mirrored across the body (as its socket is made).
                Vector3 restL = new Vector3(-rig.RestBlade.x, rig.RestBlade.y, rig.RestBlade.z);
                Vector3 restUpL = rig.RestBladeUp;
                restUpL.x = -restUpL.x;
                Vector3 bladeL = BladeL.sqrMagnitude > 1e-6f ? BladeL.normalized : Vector3.down;
                if (GripAngle > 0f)
                {
                    AimAcross(rig, "upperarm.L", "forearm.L", "hand.L", restL, restUpL, bladeL, ElbowL, WristRollL);
                    return;
                }
                Vector3 backL = Vector3.ProjectOnPlane(BladeLBack, bladeL);
                Quaternion aimL = backL.sqrMagnitude > 1e-5f
                    ? Quaternion.LookRotation(bladeL, backL) * Quaternion.Inverse(Quaternion.LookRotation(restL, restUpL))
                    : Quaternion.FromToRotation(restL, bladeL);
                rig.TurnAbsolute("hand.L", aimL);
                return;
            }
            Vector3 forearmL = (rig["hand.L"].Transform.position - rig["forearm.L"].Transform.position).normalized;
            rig.TurnOnParent("hand.L", Quaternion.AngleAxis(HandLTwist, forearmL) * Quaternion.AngleAxis(HandLBend, Vector3.Cross(forearmL, Vector3.up)));
        }

        /// <summary>
        /// Turns a hand holding a short blade across its palm (<see cref="GripAngle"/>). <paramref name="restHand"/> is the
        /// hand's length at rest (its socket's +Z) and <paramref name="restSide"/> the socket's +Y, the side of the fist
        /// the blade leaves from (the thumb's).
        ///
        /// A wrist can only do so much, so the asked way is met as an arm would meet it. From the hand straight on the
        /// forearm with the thumb where the arm carries it, the shortest turn that brings the blade the asked way is split
        /// in two: the part about the forearm, a roll of the forearm, kept within a quarter turn either way; and the rest, a
        /// bend at the wrist, kept within <see cref="WristBend"/>. Where the asked way is out of reach the blade points
        /// as near it as those allow. Every part of this moves smoothly with the pose, so the hand never turns over
        /// between two frames (short of a blade asked for straight back through the little finger).
        /// </summary>
        private void AimAcross(ForgeRig rig, string upperName, string forearmName, string handName, Vector3 restHand, Vector3 restSide, Vector3 blade, Vector3 elbow, float roll)
        {
            var forearm = rig[forearmName];
            Vector3 along = (rig[handName].Transform.position - forearm.Transform.position).normalized;
            float g = GripAngle * Mathf.Deg2Rad;
            Vector3 restBlade = (Mathf.Cos(g) * restHand + Mathf.Sin(g) * restSide).normalized;
            // The thumb's side with the forearm neither turned in nor out: away from where the elbow points, square to the
            // forearm (a handshake: elbow down, thumb up). The elbow's way is the reach's own (the pose's elbow made square
            // to the line from shoulder to hand), which moves smoothly while the arm is bent; the forearm bone's own turn
            // about its length is whatever the reach left it with, and can jump. A straight arm can turn its hand any way
            // at all and its elbow's way means little, so the straighter the arm the more the asked blade's own side
            // counts (and, last, which way is up), so a straight arm's hand never turns over when its elbow hint does.
            Vector3 shoulder = rig[upperName].Transform.position;
            Vector3 reach = (rig[handName].Transform.position - shoulder).normalized;
            Vector3 elbowWay = Vector3.ProjectOnPlane(elbow, reach);
            if (elbowWay.sqrMagnitude < 1e-6f) elbowWay = Vector3.Cross(reach, Vector3.right);
            float bent = Mathf.Clamp01(Vector3.ProjectOnPlane(forearm.Transform.position - shoulder, reach).magnitude / 0.1f);
            Vector3 thumb = Vector3.ProjectOnPlane(-elbowWay.normalized, along) * bent
                + Vector3.ProjectOnPlane(blade, along) * 0.3f + Vector3.ProjectOnPlane(Vector3.up, along) * 0.05f;
            if (thumb.sqrMagnitude < 1e-6f) thumb = Vector3.ProjectOnPlane(rig.Turned(forearm.Transform) * restSide, along);
            thumb.Normalize();

            // The hand straight on the forearm, thumb that way: the blade leaves the fist at the grip angle.
            Vector3 natural = Mathf.Cos(g) * along + Mathf.Sin(g) * thumb;
            Quaternion straight = Quaternion.LookRotation(natural, along) * Quaternion.Inverse(Quaternion.LookRotation(restBlade, restHand));
            // The shortest turn to the asked way, as a roll about the forearm and a bend of the wrist, each held in reach.
            Quaternion toAsked = Quaternion.FromToRotation(natural, blade);
            Vector3 onAxis = Vector3.Project(new Vector3(toAsked.x, toAsked.y, toAsked.z), along);
            var twist = new Quaternion(onAxis.x, onAxis.y, onAxis.z, toAsked.w);
            float twistLength = Mathf.Sqrt(twist.x * twist.x + twist.y * twist.y + twist.z * twist.z + twist.w * twist.w);
            twist = twistLength > 1e-6f ? new Quaternion(twist.x / twistLength, twist.y / twistLength, twist.z / twistLength, twist.w / twistLength) : Quaternion.identity;
            Quaternion swing = toAsked * Quaternion.Inverse(twist);
            float rolled = Mathf.Clamp(TwistAbout(twist, along), -90f, 90f) + roll;
            swing.ToAngleAxis(out float swingAngle, out Vector3 swingAxis);
            if (swingAngle > 180f) swingAngle -= 360f;
            if (float.IsNaN(swingAxis.x) || swingAxis.sqrMagnitude < 1e-8f) swingAngle = 0f;
            Quaternion bend = Quaternion.AngleAxis(Mathf.Clamp(swingAngle, -WristBend, WristBend), swingAngle == 0f ? along : swingAxis);
            Quaternion turn = bend * Quaternion.AngleAxis(rolled, along) * straight;
            // The forearm takes half of the hand's turn about it, so the wrist is not wrung like a cloth. Near half a turn
            // either way is the same turn, so there the share eases to nothing rather than jump from one side to the other.
            float wrung = TwistAbout(turn * Quaternion.Inverse(rig.Turned(forearm.Transform)), along);
            float share = 0.5f * Mathf.Clamp01((180f - Mathf.Abs(wrung)) / 45f);
            forearm.Transform.rotation = Quaternion.AngleAxis(wrung * share, along) * forearm.Transform.rotation;
            rig.TurnAbsolute(handName, turn);
        }

        /// <summary>Degrees a rotation turns about an axis (its twist, with the swing off the axis left out), from -180 to 180.</summary>
        public static float TwistAbout(Quaternion rotation, Vector3 axis)
        {
            axis.Normalize();
            float along = rotation.x * axis.x + rotation.y * axis.y + rotation.z * axis.z;
            float degrees = 2f * Mathf.Atan2(along, rotation.w) * Mathf.Rad2Deg;
            if (degrees > 180f) degrees -= 360f;
            if (degrees < -180f) degrees += 360f;
            return degrees;
        }
    }
}
