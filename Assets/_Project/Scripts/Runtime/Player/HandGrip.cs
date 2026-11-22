using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Closes the player's fingers round what each hand holds, laid over whatever clip is playing (the clips were made
    /// before the hands had fingers and never move them). Each weapon held in a hand sits in the fist at its grip
    /// (staging/blender/2026-11-22_hands/notes.md), across the palm; the clips aim the weapon as they always did, so the
    /// hand is turned about the weapon by a fixed turn worked out when the weapon was hung (<see cref="Hold.Turn"/>):
    /// the weapon points exactly where the clip points it, and the hand closes round its handle.
    /// Bare hands make fists while the Fists or the Armament are carried, and hang open otherwise.
    /// Runs early in LateUpdate, after the Animator has posed the body and before anything reads the hands.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public class HandGrip : MonoBehaviour
    {
        /// <summary>The hand's finger poses, in the order the grip file names them.</summary>
        public enum Pose { Open = 0, Closed = 1, KnifeReverse = 2, Bow = 3, Fist = 4 }
        public const int PoseCount = 5;
        /// <summary>Finger bones a hand: thumb, index, middle, ring and little finger, three each.</summary>
        public const int FingersPerHand = 15;

        /// <summary>One thing that may be held: while its part is shown, that hand takes the pose and the turn.</summary>
        [System.Serializable]
        public class Hold
        {
            public GameObject Part;
            /// <summary>0 the right hand, 1 the left.</summary>
            public int Hand;
            public Pose Pose = Pose.Closed;
            /// <summary>The hand's turn, in its own space, that keeps the weapon where the clip aims it now it sits across the palm.</summary>
            public Quaternion Turn = Quaternion.identity;
        }

        [SerializeField] private Transform handRight, handLeft;
        [SerializeField] private Transform[] fingers = new Transform[0];
        /// <summary>Local rotations of every finger bone (right hand's 15, then the left's) for each pose, pose after pose.</summary>
        [SerializeField] private Quaternion[] poses = new Quaternion[0];
        [SerializeField] private Hold[] holds = new Hold[0];
        /// <summary>Carries whose bare hands are fists (the Fists, the Armament).</summary>
        [SerializeField] private string[] fistCarries = { "", "arm" };
        /// <summary>Carries held in both hands: the left hand closes on the haft below the right.</summary>
        [SerializeField] private string[] twoHandedCarries = { "gsword", "scythe", "spear", "sunspear", "hammer" };

        /// <summary>Degrees a second a hand turns onto a weapon put in it (or off one let go), and a finger closes or opens.</summary>
        public float HandDegreesPerSecond = 900f, FingerDegreesPerSecond = 1400f;

        /// <summary>The weapon carried when there is no player to ask (the Forge's sheets, edit-mode stills): null for none.</summary>
        [System.NonSerialized] public string CarryWithoutPlayer;

        private PlayerCombat combat;
        private readonly Quaternion[] shownTurn = { Quaternion.identity, Quaternion.identity };
        private readonly int[] poseNow = { 0, 0 };
        private readonly Quaternion[] fingerNow = new Quaternion[FingersPerHand * 2];
        private bool started;

        public void Configure(Transform right, Transform left, Transform[] fingerBones, Quaternion[] posesByPose, Hold[] held)
        {
            handRight = right;
            handLeft = left;
            fingers = fingerBones;
            poses = posesByPose;
            holds = held;
        }

        public Hold[] Holds => holds;
        public Transform[] Fingers => fingers;

        /// <summary>The local rotation of finger <paramref name="index"/> (0-29) in a pose.</summary>
        public Quaternion FingerIn(Pose pose, int index) => poses[(int)pose * FingersPerHand * 2 + index];

        /// <summary>The pose a hand is in now (0 right, 1 left), for probes.</summary>
        public Pose PoseOf(int hand) => (Pose)poseNow[hand];

        /// <summary>The turn laid on a hand now (0 right, 1 left), for probes.</summary>
        public Quaternion TurnOf(int hand) => shownTurn[hand];

        private void Awake() => combat = GetComponentInParent<PlayerCombat>();

        private void LateUpdate() => Apply(Time.deltaTime);

        /// <summary>Lays the grips over the pose the body is in now; <paramref name="deltaTime"/> moves the hands toward a new grip (the Forge's sheets pass a large one to snap).</summary>
        public void Apply(float deltaTime)
        {
            if (fingers == null || fingers.Length != FingersPerHand * 2 || poses == null || poses.Length != PoseCount * FingersPerHand * 2) return;
            string carry = combat != null && combat.Weapon != null ? combat.Weapon.Carry : CarryWithoutPlayer;
            for (int hand = 0; hand < 2; hand++)
            {
                var bone = hand == 0 ? handRight : handLeft;
                if (bone == null) continue;
                Hold held = Held(hand);
                Pose pose = held != null ? held.Pose
                    : carry != null && Has(fistCarries, carry) ? Pose.Fist
                    : hand == 1 && carry != null && Has(twoHandedCarries, carry) && Held(0) != null ? Pose.Closed
                    : Pose.Open;
                Quaternion want = held != null ? held.Turn : Quaternion.identity;
                // Turned onto the weapon and closed over a few frames, not snapped: the clip changes the moment it is drawn.
                shownTurn[hand] = started ? Quaternion.RotateTowards(shownTurn[hand], want, HandDegreesPerSecond * deltaTime) : want;
                poseNow[hand] = (int)pose;
                bone.localRotation = bone.localRotation * shownTurn[hand];
                int from = hand * FingersPerHand, at = (int)pose * FingersPerHand * 2 + from;
                float step = FingerDegreesPerSecond * deltaTime;
                for (int f = 0; f < FingersPerHand; f++)
                {
                    var finger = fingers[from + f];
                    if (finger == null) continue;
                    Quaternion target = poses[at + f];
                    fingerNow[from + f] = started ? Quaternion.RotateTowards(fingerNow[from + f], target, step) : target;
                    finger.localRotation = fingerNow[from + f];
                }
            }
            started = true;
        }

        // The first shown part held in a hand, or null.
        private Hold Held(int hand)
        {
            for (int i = 0; i < holds.Length; i++)
                if (holds[i].Hand == hand && holds[i].Part != null && holds[i].Part.activeInHierarchy) return holds[i];
            return null;
        }

        private static bool Has(string[] list, string carry)
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i] == carry) return true;
            return false;
        }

        /// <summary>
        /// The hand's turn that keeps a weapon where a clip aims it once it is moved to the grip: <paramref name="aimed"/> is
        /// the weapon's frame in the hand's space as the clips were made with, <paramref name="seated"/> its frame in the fist.
        /// </summary>
        public static Quaternion TurnFor(Quaternion aimed, Quaternion seated) => aimed * Quaternion.Inverse(seated);
    }
}
